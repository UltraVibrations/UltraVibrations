using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using OtterGui.Services;
using UltraVibrations.Devices;
using UltraVibrations.Services;
using UltraVibrations.UI.Components;

namespace UltraVibrations.UI.Devices;

public class DeviceSelector : IService
{
    private readonly DeviceManagerService deviceManagerService;
    private readonly ButtplugService buttplugService;

    public uint? SelectedId { get; private set; }

    public DeviceSelector(DeviceManagerService deviceManagerService, ButtplugService buttplugService)
    {
        this.deviceManagerService = deviceManagerService;
        this.buttplugService = buttplugService;
        DeviceClicked += HandleDeviceClicked;
    }

    public void Draw()
    {
        using var group = ImRaii.Group();

        var ySpacing = ImGui.GetStyle().ItemSpacing.Y;
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = 0f };
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        DrawSelectors(ySpacing);
        DrawButtons();
        style.Pop();
    }

    public void DrawSelectors(float ySpacing)
    {
        using var child = ImRaii.Child("##DeviceSelector",
                                       new Vector2(300 * ImGuiHelpers.GlobalScale, -ImGui.GetFrameHeight()), true);
        if (!child)
        {
            return;
        }

        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ySpacing };
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        foreach (var device in deviceManagerService.Devices)
        {
            ConnectedIndicator.Draw(device.AssumeConnected);
            ImGui.SameLine();
            if (ImGui.Selectable($"{device.Name}", device.Id == SelectedId))
            {
                DeviceClicked?.Invoke(this, device);
            }
        }

        style.Pop();
    }

    public void DrawButtons()
    {
        using var child = ImRaii.Child("##TriggerSelectorButtons",
                                       new Vector2(300 * ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight()), false);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0f)
                                .Push(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        if (!child)
        {
            return;
        }

        this.TriggerScanButton(new Vector2(300 * ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight()));

        style.Pop();
    }

    protected void TriggerScanButton(Vector2 size)
    {
        if (ImGuiUtil.DrawDisabledButton(
                buttplugService.IsScanning
                    ? "Scanning..."
                    : FontAwesomeIcon.Sync.ToIconString(),
                size,
                "Initiate scanning for Devices.",
                !buttplugService.IsConnected,
                !buttplugService.IsScanning)
           )
        {
            buttplugService.RequestScan();
        }
    }

    public EventHandler<Device>? DeviceClicked;


    private void HandleDeviceClicked(object? sender, Device device)
    {
        SelectedId = device.Id;
    }
}
