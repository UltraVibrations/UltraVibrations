using System;
using Dalamud.Game.Text;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using UltraVibrations.Triggers;

namespace UltraVibrations.UI.Components;

public class TriggerTypeSelector
{
    public static bool DrawTriggerTypeSelector(
        string label, string description, TriggerType currentValue, Action<TriggerType> setter)
    {
        using var id = ImRaii.PushId(label);
        using var combo = ImRaii.Combo(label, currentValue.ToString());
        ImGuiUtil.HoverTooltip(description);
        if (!combo)
            return false;

        var ret = false;
        // Draw the actual combo values.
        foreach (var type in Enum.GetValues<TriggerType>())
        {
            if (!ImGui.Selectable(type.ToString(), currentValue == type) || type == currentValue)
                continue;

            setter(type);
            ret = true;
        }

        return ret;
    }
}
