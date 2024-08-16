using System;
using ImGuiNET;
using OtterGui.Widgets;

namespace UltraVibrations.UI.Tabs;

public class SettingsTab(Configuration configuration) : ITab
{
    public ReadOnlySpan<byte> Label => "Settings"u8;

    public void DrawContent()
    {
        {
            var buttplugServerUrl = configuration.ButtplugServerUrl;
            ImGui.Text("Intiface Server URL");
            if (ImGui.InputText("##ButtplugServerUrl", ref buttplugServerUrl, 100))
            {
                configuration.ButtplugServerUrl = buttplugServerUrl;
                configuration.Save();
            }
        }

        ImGui.Spacing();

        {
            var buttplugServerReconnectAttempts = configuration.ButtplugServerReconnectAttempts;
            ImGui.Text("Reconnect Attempts");
            if (ImGui.InputInt("##ButtplugServerReconnectAttempts", ref buttplugServerReconnectAttempts))
            {
                configuration.ButtplugServerReconnectAttempts = buttplugServerReconnectAttempts;
                configuration.Save();
            }
        }

        ImGui.Spacing();

        {
            var buttplugServerReconnectDelay = configuration.ButtplugServerReconnectDelay;
            ImGui.Text("Reconnect Delay (ms)");
            if (ImGui.InputInt("##ButtplugServerReconnectDelay", ref buttplugServerReconnectDelay))
            {
                configuration.ButtplugServerReconnectDelay = buttplugServerReconnectDelay;
                configuration.Save();
            }
        }

        ImGui.Spacing();

        {
            var buttplugServerScanDuration = configuration.ButtplugServerScanDuration;
            ImGui.Text("Scan Duration (ms)");
            if (ImGui.InputInt("##ButtplugServerScanDuration", ref buttplugServerScanDuration))
            {
                configuration.ButtplugServerScanDuration = buttplugServerScanDuration;
                configuration.Save();
            }
        }
    }
}

