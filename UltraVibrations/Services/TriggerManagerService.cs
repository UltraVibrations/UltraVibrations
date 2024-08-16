using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using UltraVibrations.Triggers;

namespace UltraVibrations.Services;

public class TriggerManagerService
{
    private readonly SaveService saveService;
    private readonly IChatGui chatGui;
    private readonly ChannelService channelService;

    public TriggerManagerService(SaveService saveService, IChatGui chatGui, ChannelService channelService)
    {
        this.saveService = saveService;
        this.chatGui = chatGui;
        this.channelService = channelService;
        Load();

        this.chatGui.ChatMessage += HandleChatReceived;
    }

    public void Load()
    {
        Plugin.Log.Debug("Loading triggers.");
        if (!Directory.Exists(saveService.FileNames.TriggerConfigsDirectory))
        {
            Directory.CreateDirectory(saveService.FileNames.TriggerConfigsDirectory);
        }

        var dirty = false;
        foreach (var fileName in Directory.EnumerateFiles(saveService.FileNames.TriggerConfigsDirectory))
        {
            Plugin.Log.Debug(fileName);
            if (!fileName.EndsWith(".json"))
            {
                continue;
            }

            var trigger = Trigger.FromFile(fileName, saveService);

            if (trigger != null)
            {
                Triggers.Add(trigger);
                dirty = true;
            }
        }

        if (dirty)
        {
            OnTriggersChanged?.Invoke(this, Triggers);
        }
    }

    public readonly List<Trigger> Triggers = [];

    // Device Change EventHandler
    public EventHandler<IEnumerable<Trigger>>? OnTriggersChanged;

    public void ClearTriggers()
    {
        Triggers.Clear();
        OnTriggersChanged?.Invoke(this, Triggers);
    }

    public Trigger? GetTrigger(string id) => Triggers.Find(d => d.Id == id);

    public void RemoveTrigger(string id)
    {
        Triggers.RemoveAll(d => d.Id == id);
        OnTriggersChanged?.Invoke(this, Triggers);
    }

    public string CreateTrigger(string name)
    {
        var trigger = new Trigger(saveService) { Name = name };
        Triggers.Add(trigger);
        trigger.Save();
        OnTriggersChanged?.Invoke(this, Triggers);
        return trigger.Id;
    }

    private void HandleChatReceived(
        XivChatType type,
        int senderId,
        ref SeString sender,
        ref SeString message,
        ref bool isHandled)
    {
        if (Triggers.Count <= 0)
        {
            return;
        }

        var playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload;

        var triggerIterator = Triggers.Where(trigger => trigger is { Enabled: true, Type: TriggerType.Chat } &&
                                                        trigger.ChatSettings.ChatChannels.Contains(type));

        foreach (var trigger in triggerIterator)
        {
            if (playerPayload != null && trigger.ChatSettings.BlacklistPlayers.Contains(playerPayload.PlayerName))
            {
                continue;
            }

            if (trigger.ChatSettings.WhitelistPlayers.Count > 0)
            {
                if (playerPayload == null)
                {
                    continue;
                }

                if (!trigger.ChatSettings.WhitelistPlayers.Contains(playerPayload.PlayerName))
                {
                    continue;
                }
            }

            if (trigger.ChatSettings.GetPreparedRegex().Match(message.TextValue).Success)
            {
                Plugin.Log.Debug("Running Trigger");
                channelService.RunTrigger(trigger);
            }
        }
    }
}
