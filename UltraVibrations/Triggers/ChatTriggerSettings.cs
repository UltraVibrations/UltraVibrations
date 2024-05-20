using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dalamud.Game.Text;

namespace UltraVibrations.Triggers;

public class ChatTriggerSettings
{
    public readonly List<string> WhitelistPlayers = [];
    public readonly List<string> BlacklistPlayers = [];

    public readonly List<string> MatchedPhrases = [];

    public readonly List<XivChatType> ChatChannels = [];

    [JsonIgnore]
    private Regex? preparedRegex;

    public Regex GetPreparedRegex()
    {
        if (preparedRegex != null)
        {
            return preparedRegex;
        }

        if (MatchedPhrases.Count == 0)
        {
            return preparedRegex ??= new Regex("");
        }

        return preparedRegex ??= new Regex($"({string.Join("|", MatchedPhrases)})");
    }

    public void Invalidate()
    {
        preparedRegex = null;
    }
}
