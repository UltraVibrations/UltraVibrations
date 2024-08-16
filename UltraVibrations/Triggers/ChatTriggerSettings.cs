using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dalamud.Game.Text;

namespace UltraVibrations.Triggers;

public class ChatTriggerSettings
{
    public readonly List<string> WhitelistPlayers = [];
    public readonly List<string> BlacklistPlayers = [];

    public readonly List<string> MatchedPhrases = [];
    public string? CustomRegex;

    public readonly List<XivChatType> ChatChannels = [];

    [JsonIgnore]
    private Regex? preparedRegex;

    public Regex GetPreparedRegex()
    {
        if (preparedRegex != null)
        {
            return preparedRegex;
        }

        if (!string.IsNullOrWhiteSpace(CustomRegex))
        {
            return preparedRegex = new Regex(CustomRegex);
        }

        if (MatchedPhrases.Count == 0)
        {
            return preparedRegex = new Regex("");
        }

        return preparedRegex = new Regex(string.Join("|", MatchedPhrases.Select(phrase => Regex.Escape(phrase))));
    }

    public void Invalidate()
    {
        preparedRegex = null;
    }
}
