using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buttplug.Client;
using Buttplug.Core.Messages;
using Newtonsoft.Json;
using UltraVibrations.Services;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace UltraVibrations.Devices;

public class Device : ISavable
{
    public Device(SaveService saveService)
    {
        this.saveService = saveService;
    }

    [JsonIgnore]
    private readonly SaveService saveService;

    public static Device? FromFile(string filePath, SaveService saveService)
    {
        if (File.Exists(filePath))
        {
            try
            {
                var text = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                var loadedDevice = new Device(saveService);
                JsonConvert.PopulateObject(
                    text,
                    loadedDevice,
                    new JsonSerializerSettings { Error = HandleDeserializationError }
                );

                return loadedDevice;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error loading Configuration:\n{ex}");
                return null;
            }
        }

        return null;

        void HandleDeserializationError(object? sender, ErrorEventArgs errorArgs)
        {
            Plugin.Log.Error(
                $"Error parsing Device Configuration at {errorArgs.ErrorContext.Path}:\n{errorArgs.ErrorContext.Error}"
            );
            errorArgs.ErrorContext.Handled = true;
        }
    }

    public static Device FromButtplugDevice(ButtplugClientDevice device, SaveService saveService)
    {
        var newDevice = new Device(saveService)
        {
            Id = device.Index,
            Name = device.Name,
            AssumeConnected = true,
        };

        foreach (var attribute in device.VibrateAttributes)
        {
            newDevice.Outputs.Add(DeviceOutput.FromAttribute(device.Index, attribute));
        }

        foreach (var attribute in device.OscillateAttributes)
        {
            newDevice.Outputs.Add(DeviceOutput.FromAttribute(device.Index, attribute));
        }

        foreach (var attribute in device.RotateAttributes)
        {
            newDevice.Outputs.Add(DeviceOutput.FromAttribute(device.Index, attribute));
        }

        newDevice.Save();

        return newDevice;
    }

    public void UpdateFromButtplugDevice(ButtplugClientDevice device)
    {
        Name = device.Name;
        AssumeConnected = true;

        var keepIds = new List<Tuple<ActuatorType, uint>>();
        foreach (var attribute in device.VibrateAttributes)
        {
            keepIds.Add(new Tuple<ActuatorType, uint>(attribute.ActuatorType, attribute.Index));
            var output = Outputs.FirstOrDefault(o => o.Id == attribute.Index && o.Type == attribute.ActuatorType);
            if (output == null)
            {
                Outputs.Add(DeviceOutput.FromAttribute(device.Index, attribute));
                continue;
            }

            output.UpdateFromAttribute(attribute);
        }

        foreach (var attribute in device.OscillateAttributes)
        {
            keepIds.Add(new Tuple<ActuatorType, uint>(attribute.ActuatorType, attribute.Index));
            var output = Outputs.FirstOrDefault(o => o.Id == attribute.Index && o.Type == attribute.ActuatorType);
            if (output == null)
            {
                Outputs.Add(DeviceOutput.FromAttribute(device.Index, attribute));
                continue;
            }

            output.UpdateFromAttribute(attribute);
        }

        foreach (var attribute in device.RotateAttributes)
        {
            keepIds.Add(new Tuple<ActuatorType, uint>(attribute.ActuatorType, attribute.Index));
            var output = Outputs.FirstOrDefault(o => o.Id == attribute.Index && o.Type == attribute.ActuatorType);
            if (output == null)
            {
                Outputs.Add(DeviceOutput.FromAttribute(device.Index, attribute));
                continue;
            }

            output.UpdateFromAttribute(attribute);
        }

        Outputs.RemoveAll(o => !keepIds.Contains(new Tuple<ActuatorType, uint>(o.Type, o.Id)));

        Save();
    }

    public int Version = 0;
    public uint Id;
    public string Name = "";

    [JsonIgnore]
    public bool AssumeConnected = false;

    public readonly List<DeviceOutput> Outputs = [];

    public DeviceOutput? GetOutput(uint outputId) => Outputs.Find(o => o.Id == outputId);

    public string ToFilename(FilenameService fileNames) => Path.Join(fileNames.DeviceConfigsDirectory, $"{Id}.json");

    public void Save() => saveService.QueueSave(this);

    public void Save(StreamWriter writer)
    {
        using var jWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        var serializer = new JsonSerializer();
        serializer.Serialize(jWriter, this);
    }
}
