using System;
using ImGuiNET;
using OtterGui.Widgets;
using UltraVibrations.UI.Triggers;

namespace UltraVibrations.UI.Tabs;

public class TriggersTab(TriggerSelector triggerSelector, TriggerDetails triggerDetails) : ITab
{
    public ReadOnlySpan<byte> Label => "Triggers"u8;

    public void DrawContent()
    {
        triggerSelector.Draw();
        ImGui.SameLine();
        triggerDetails.Draw(triggerSelector.SelectedId);
    }
}
