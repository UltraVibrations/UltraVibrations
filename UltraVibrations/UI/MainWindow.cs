using System;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using OtterGui.Widgets;
using UltraVibrations.Services;
using UltraVibrations.UI.Tabs;

namespace UltraVibrations.UI;

public class MainWindow(
    DevicesTab devicesTab,
    PatternsTab patternsTab,
    TriggersTab triggersTab,
    SettingsTab settingsTab,
    ButtplugService buttplugService
) : Window("UltraVibrations###MainWindow")
{
    private readonly ITab[] tabs =
    [
        devicesTab,
        triggersTab,
        patternsTab,
        settingsTab,
    ];

    public override void Draw()
    {
        {
            var desiredConnected = buttplugService.DesiredConnected;
            if (ImGuiComponents.ToggleButton("Connect", ref desiredConnected))
            {
                buttplugService.DesiredConnected = desiredConnected;
            }

            ImGui.SameLine();
            ImGui.Text(buttplugService.StatusMessage);
        }

        TabBar.Draw(
            string.Empty,
            ImGuiTabBarFlags.NoTooltip,
            [],
            out _,
            () => { },
            tabs
        );
    }
}
