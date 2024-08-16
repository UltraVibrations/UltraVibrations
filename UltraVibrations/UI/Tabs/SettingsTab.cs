using System;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using OtterGui.Widgets;
using UltraVibrations.Services;

namespace UltraVibrations.UI.Tabs;

public class SettingsTab(ButtplugService buttplugService) : ITab
{
    public ReadOnlySpan<byte> Label => "Settings"u8;

    public void DrawContent()
    {
        
        {
            ImGui.Text("Enable Connection");
            ImGui.SameLine();
            var desiredConnected = buttplugService.DesiredConnected;
            if (ImGuiComponents.ToggleButton("Connect", ref desiredConnected))
            {
                buttplugService.DesiredConnected = desiredConnected;
            }
            ImGui.SameLine();
            if (buttplugService.IsConnected)
            {
                if (buttplugService.IsScanning)
                {
                    ImGuiComponents.DisabledButton("Scanning##ScanButton");
                }
                else
                {
                    if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.MagnifyingGlassArrowRight, "Scan"))
                    {
                        buttplugService.RequestScan();
                    }
                }
            }
        }
    }
}
