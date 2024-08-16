using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using OtterGui.Services;
using UltraVibrations.Services;
using UltraVibrations.Triggers;
using UltraVibrations.UI.Components;

namespace UltraVibrations.UI.Triggers;

public class TriggerSelector : IService
{
    private readonly TriggerManagerService triggerManagerService;

    public string? SelectedId { get; private set; }

    public TriggerSelector(TriggerManagerService triggerManagerService)
    {
        this.triggerManagerService = triggerManagerService;
        TriggerClicked += HandleTriggerClicked;
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
        using var child = ImRaii.Child("##TriggerSelector",
                                       new Vector2(300 * ImGuiHelpers.GlobalScale, -ImGui.GetFrameHeight()), true);
        if (!child)
        {
            return;
        }

        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ySpacing };
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        Dictionary<string, int> seen = new();
        foreach (var trigger in triggerManagerService.Triggers)
        {
            if (seen.TryGetValue(trigger.Name, out var count))
            {
                seen[trigger.Name] = count + 1;
            }
            else
            {
                seen[trigger.Name] = 1;
            }

            ConnectedIndicator.Draw(trigger.Enabled);
            ImGui.SameLine();
            if (ImGui.Selectable(count >= 1 ? $"{trigger.Name} ({count})" : trigger.Name, trigger.Id == SelectedId))
            {
                TriggerClicked?.Invoke(this, trigger);
            }
        }

        style.Pop();
    }

    public void DrawButtons()
    {
        using var child = ImRaii.Child(
            "##TriggerSelectorButtons",
            new Vector2(300 * ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight()),
            false
        );

        using var style = ImRaii
                          .PushStyle(ImGuiStyleVar.FrameRounding, 0f)
                          .Push(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        if (!child)
        {
            return;
        }

        this.TriggerAddButton(new Vector2(300 * ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight()));

        style.Pop();
    }

    private string triggerNewName = string.Empty;

    protected void TriggerAddButton(Vector2 size)
    {
        const string newTriggerName = "triggerName";

        if (
            ImGuiUtil.DrawDisabledButton(
                FontAwesomeIcon.PlusCircle.ToIconString(),
                size,
                "Create a new Trigger.",
                false,
                true
            )
        )
        {
            ImGui.OpenPopup(newTriggerName);
        }

        // Does not need to be delayed since it is not in the iteration itself.
        if (ImGuiUtil.OpenNameField(newTriggerName, ref triggerNewName) && triggerNewName.Length > 0)
        {
            SelectedId = triggerManagerService.CreateTrigger(triggerNewName);
            triggerNewName = string.Empty;
        }
    }

    public EventHandler<Trigger>? TriggerClicked;

    private void HandleTriggerClicked(object? sender, Trigger trigger)
    {
        SelectedId = trigger.Id;
    }
}
