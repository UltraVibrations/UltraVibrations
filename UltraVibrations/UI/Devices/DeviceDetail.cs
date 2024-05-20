using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Services;
using OtterGui.Widgets;
using UltraVibrations.Devices;
using UltraVibrations.Services;
using UltraVibrations.UI.Components;

namespace UltraVibrations.UI.Devices;

public class DeviceDetail(DeviceManagerService deviceManagerService) : IService
{
    private readonly List<TagButtons> tagButtons = [];

    private TagButtons GetTagButtonsForIndex(int idx)
    {
        if (tagButtons.Count <= idx)
        {
            tagButtons.Add(new TagButtons());
        }

        return tagButtons[idx];
    }

    public void Draw(uint? deviceId)
    {
        using var group = ImRaii.Group();
        using var child = ImRaii.Child("##DeviceDetails", new Vector2(-1, -1), true);

        var device = deviceId != null ? deviceManagerService.GetDevice(deviceId.Value) : null;

        if (device != null)
        {
            DrawConfig(device);
        }
    }

    public void DrawConfig(Device device)
    {
        ConnectedIndicator.Draw(device.AssumeConnected);
        ImGui.SameLine();
        ImGui.Text(device.Name);

        for (var idx = 0; idx < device.Outputs.Count; idx++)
        {
            var tags = GetTagButtonsForIndex(idx);
            DrawOutput(tags, device, device.Outputs[idx]);
        }
    }


    public static void DrawOutput(TagButtons tags, Device device, DeviceOutput output)
    {
        using var id = ImRaii.PushId($"OutputGroup:{output.DeviceId}:{output.Type}:{output.Id}");
        var description = output.Description == "N/A" ? $"Motor {output.Id + 1}" : output.Description;
        var label = $"{output.Type.ToString()} {output.Id + 1} - {description}";
        using var framedGroup = ImRaii.FramedGroup(label, new Vector2(x: -1, y: 0));

        {
            var maxSpeed = output.MaxSpeed;
            var changed = ImGui.SliderFloat("Max Speed", ref maxSpeed, 0.0f, 1.0f, null, ImGuiSliderFlags.AlwaysClamp);
            if (changed)
            {
                output.MaxSpeed = maxSpeed;
                device.Save();
            }
        }

        {
            var editedIdx = tags.Draw(
                "Channels",
                "Will follow the output of these channels.",
                output.Channels,
                out var editedTag
            );

            if (editedIdx >= 0 && editedIdx < output.Channels.Count)
            {
                if (editedTag == string.Empty)
                {
                    output.Channels.RemoveAt(editedIdx);
                    device.Save();
                }
                else
                {
                    output.Channels[editedIdx] = editedTag;
                    // Deduplicate Groups
                    var groups = output.Channels.Distinct().ToList();
                    output.Channels.Clear();
                    output.Channels.AddRange(groups);
                    device.Save();
                }
            }
            else if (editedIdx == output.Channels.Count)
            {
                output.Channels.Add(editedTag);
                // Deduplicate Groups
                var groups = output.Channels.Distinct().ToList();
                output.Channels.Clear();
                output.Channels.AddRange(groups);
                device.Save();
            }
        }
    }
}
