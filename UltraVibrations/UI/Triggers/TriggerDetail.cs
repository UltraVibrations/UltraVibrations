using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Services;
using OtterGui.Table;
using OtterGui.Widgets;
using UltraVibrations.Services;
using UltraVibrations.Triggers;
using UltraVibrations.UI.Components;

namespace UltraVibrations.UI.Triggers;

public class TriggerDetails(TriggerManagerService triggerManagerService) : IService
{
    public void Draw(string? triggerId)
    {
        using var group = ImRaii.Group();
        using var child = ImRaii.Child("##DeviceDetails", new Vector2(-1, -1), true);

        var trigger = triggerId != null ? triggerManagerService.GetTrigger(triggerId) : null;

        if (trigger != null)
        {
            DrawConfig(trigger);
        }
    }

    public void DrawConfig(Trigger trigger)
    {
        var changed = false;

        changed = ImGuiComponents.ToggleButton("##TriggerEnabled", ref trigger.Enabled) || changed;
        ImGui.SameLine();
        changed = ImGui.InputText("##TriggerName", ref trigger.Name, 100) || changed;

        TriggerTypeSelector.DrawTriggerTypeSelector("Type", "The type of trigger", trigger.Type, type =>
        {
            trigger.Type = type;
            changed = true;
        });

        DrawChatSettings(trigger);

        if (changed)
        {
            trigger.Save();
        }
    }

    public void DrawChatSettings(Trigger trigger)
    {
        if (trigger.Type != TriggerType.Chat)
        {
            return;
        }

        ImGui.Spacing();

        {
            if (ImGui.CollapsingHeader("Matching"))
            {
                using var indentSub = ImRaii.PushIndent();
                ImGui.Spacing();

                {
                    using var list = ImRaii.Table("##chattypes", 2,
                                                  ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders);

                    ImGui.TableSetupColumn("##ChatTypeRemove",
                                           ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize |
                                           ImGuiTableColumnFlags.NoReorder);
                    ImGui.TableSetupColumn("Chat Types",
                                           ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize |
                                           ImGuiTableColumnFlags.NoReorder);

                    ImGui.TableHeadersRow();

                    var idx = 0;
                    foreach (var channel in trigger.ChatSettings.ChatChannels)
                    {
                        using var id = ImRaii.PushId(++idx);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Spacing();
                        if (ImGuiComponents.IconButton(FontAwesomeIcon.Times.ToIconString()))
                        {
                            trigger.ChatSettings.ChatChannels.Remove(channel);
                            trigger.Save();
                        }

                        ImGui.Spacing();

                        ImGui.TableNextColumn();
                        ImGui.Spacing();
                        ImGui.Text(channel.ToString());
                    }
                }
                ImGui.Spacing();
                Widget.DrawChatTypeSelector(
                    "Add Chat Type",
                    "The types of chat messages to match",
                    XivChatType.None,
                    type =>
                    {
                        trigger.ChatSettings.ChatChannels.Add(type);
                        trigger.Save();
                    }
                );

                ImGui.Spacing();

                {
                    using var list = ImRaii.Table("##MatchedPhrases", 1,
                                                  ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders);

                    ImGui.TableSetupColumn("Matched Phrase",
                                           ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize |
                                           ImGuiTableColumnFlags.NoReorder);

                    ImGui.TableHeadersRow();

                    var idx = 0;
                    foreach (var matchPhrase in trigger.ChatSettings.MatchedPhrases)
                    {
                        using var id = ImRaii.PushId(++idx);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Spacing();
                        var phrase = matchPhrase;
                        ImGui.InputText($"##MatchedPhrase-{idx}", ref phrase, 100);
                        // var phrase = matchPhrase;
                        // if (Widget.InputOrText(true, $"##MatchedPhrase-{idx}", ref phrase, 100))
                        // {
                        //     if (phrase != trigger.ChatSettings.MatchedPhrases[idx])
                        //     {
                        //         trigger.ChatSettings.MatchedPhrases[idx] = phrase;
                        //         trigger.ChatSettings.Invalidate();
                        //         trigger.Save();
                        //     }
                        // }

                        ImGui.Spacing();
                    }
                }

                ImGui.Text("Matched Text:");
                for (var index = 0; index < trigger.ChatSettings.MatchedPhrases.Count; index++)
                {
                    var matchedPhrase = trigger.ChatSettings.MatchedPhrases[index];
                    Widget.InputOrText(true, "##MatchedPhrase", ref matchedPhrase, 100);
                    if (matchedPhrase != trigger.ChatSettings.MatchedPhrases[index])
                    {
                        trigger.ChatSettings.MatchedPhrases[index] = matchedPhrase;
                        trigger.ChatSettings.Invalidate();
                        trigger.Save();
                    }
                }
            }

            if (ImGui.CollapsingHeader("Whitelist"))
            {
                using var indentSub = ImRaii.PushIndent();

                if (trigger.ChatSettings.WhitelistPlayers.Count <= 0)
                {
                    ImGui.Text("All");
                }

                foreach (var player in trigger.ChatSettings.WhitelistPlayers)
                {
                    ImGui.Text(player);
                }
            }

            if (ImGui.CollapsingHeader("Blacklist"))
            {
                using var indentSub = ImRaii.PushIndent();
                ImGui.Text("Blacklisted:");
                foreach (var player in trigger.ChatSettings.BlacklistPlayers)
                {
                    ImGui.Text(player);
                }
            }
        }
    }
}
