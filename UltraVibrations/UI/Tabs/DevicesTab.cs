using System;
using ImGuiNET;
using OtterGui.Widgets;
using UltraVibrations.UI.Devices;

namespace UltraVibrations.UI.Tabs;

public class DevicesTab(DeviceSelector deviceSelector, DeviceDetail deviceDetail) : ITab
{
    public ReadOnlySpan<byte> Label => "Devices"u8;

    public void DrawContent()
    {
        deviceSelector.Draw();
        ImGui.SameLine();
        deviceDetail.Draw(deviceSelector.SelectedId);
    }
}
