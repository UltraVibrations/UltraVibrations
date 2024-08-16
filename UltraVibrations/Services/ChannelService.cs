using System;
using System.Collections.Generic;
using Buttplug.Core.Messages;
using Dalamud.Plugin.Services;
using UltraVibrations.Effects;
using UltraVibrations.Source;
using UltraVibrations.Triggers;

namespace UltraVibrations.Services;

public class ChannelService : IDisposable
{
    private readonly IFramework framework;
    private readonly DeviceManagerService deviceManagerService;
    private readonly ButtplugService buttplugService;

    public ChannelService(
        IFramework framework, DeviceManagerService deviceManagerService, ButtplugService buttplugService
    )
    {
        this.framework = framework;
        this.deviceManagerService = deviceManagerService;
        this.buttplugService = buttplugService;

        framework.Update += RunUpdate;
    }


    public void Dispose()
    {
        framework.Update -= RunUpdate;
    }

    private void RunUpdate(IFramework f)
    {
        if (!buttplugService.IsConnected)
        {
            if (effects.Count > 0)
            {
                effects.Clear();
            }

            return;
        }

        effects.RemoveAll(effect => effect.EndTime < f.LastUpdate);

        var deltaTime = f.UpdateDelta;

        Dictionary<string, double> channelValues = new();

        foreach (var effect in effects)
        {
            foreach (var channel in effect.Channels)
            {
                channelValues[channel] =
                    effect.GetNext(deltaTime.TotalMilliseconds, channelValues.GetValueOrDefault(channel, 0.0));
            }
        }

        // Send channel values to devices
        foreach (var device in deviceManagerService.Devices)
        {
            if (!device.AssumeConnected)
            {
                continue;
            }

            var buttplugDevice = buttplugService.GetDevice(device.Id);

            if (buttplugDevice == null)
            {
                continue;
            }

            // Prepare output values
            var vibrateValues = new double[buttplugDevice.VibrateAttributes.Count];
            var rotateValues = new (double, bool)[buttplugDevice.RotateAttributes.Count];
            var oscillatorValues = new double[buttplugDevice.OscillateAttributes.Count];

            foreach (var deviceOutput in device.Outputs)
            {
                double maxOutput = 0.0f;
                foreach (var channel in deviceOutput.Channels)
                {
                    maxOutput = Math.Max(maxOutput, channelValues.GetValueOrDefault(channel, 0.0));
                }

                maxOutput = Math.Clamp(maxOutput * deviceOutput.MaxSpeed, 0.0, deviceOutput.MaxSpeed);

                switch (deviceOutput.Type)
                {
                    case ActuatorType.Vibrate:
                        vibrateValues[deviceOutput.Id] = maxOutput;
                        break;
                    case ActuatorType.Rotate:
                        rotateValues[deviceOutput.Id] = (maxOutput, false);
                        break;
                    case ActuatorType.Oscillate:
                        oscillatorValues[deviceOutput.Id] = maxOutput;
                        break;
                }
            }

            if (vibrateValues.Length > 0)
            {
                buttplugDevice.VibrateAsync(vibrateValues);
            }

            if (rotateValues.Length > 0)
            {
                buttplugDevice.RotateAsync(rotateValues);
            }

            if (oscillatorValues.Length > 0)
            {
                buttplugDevice.OscillateAsync(oscillatorValues);
            }
        }
    }

    private readonly List<Effect> effects = [];

    public void RunTrigger(Trigger trigger)
    {
        if (!buttplugService.IsConnected)
        {
            return;
        }

        foreach (var triggerEffect in trigger.Effects)
        {
            RunTriggerEffect(triggerEffect);
        }
    }

    public void RunTriggerEffect(TriggerEffect triggerEffect)
    {
        if (!buttplugService.IsConnected)
        {
            return;
        }

        var effect = GetEffectForTriggerEffect(triggerEffect);

        if (effect == null)
        {
            return;
        }

        effects.Add(effect);
        effects.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    private static ISource? GetSourceForTriggerEffect(TriggerEffect triggerEffect)
    {
        return triggerEffect.EffectSource switch
        {
            EffectSource.Value => new ValueSource(triggerEffect.Value, triggerEffect.Duration),
            EffectSource.Pattern => new PatternSource(triggerEffect.Pattern, triggerEffect.Interpolation),
            _ => null
        };
    }

    private Effect? GetEffectForTriggerEffect(TriggerEffect triggerEffect)
    {
        var source = GetSourceForTriggerEffect(triggerEffect);

        if (source == null)
        {
            return null;
        }

        return triggerEffect.MixMode switch
        {
            MixMode.Add => new AddEffect(
                source,
                framework.LastUpdate,
                triggerEffect.Duration,
                triggerEffect.Channels.ToArray()
            ),
            MixMode.Multiply => new MultiplyEffect(
                source,
                framework.LastUpdate,
                triggerEffect.Duration,
                triggerEffect.Channels.ToArray()
            ),
            MixMode.Set => new SetEffect(
                source,
                framework.LastUpdate,
                triggerEffect.Duration,
                triggerEffect.Channels.ToArray()
            ),
            _ => null
        };
    }
}
