using System;
using System.Text.RegularExpressions;
using ImGuiNET;

namespace UltraVibrations.UI.Components;

public class PhraseInput
{
    private string phraseToAdd        = string.Empty;
    private bool   phraseIsValidRegex = true;

    public string? Draw()
    {
        if (!phraseIsValidRegex)
        {
            ImGui.BeginDisabled();
        }
     
        var add = ImGui.Button("Add");
            
        if (!phraseIsValidRegex)
        {
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("This phrase is invalid regex");
            }
            ImGui.EndDisabled();
        }
            
        ImGui.SameLine();
        if (ImGui.InputText("##NewPhrase", ref phraseToAdd, 2048))
        {
            try
            {
                _                  = new Regex(phraseToAdd);
                phraseIsValidRegex = true;
            }
            catch (ArgumentException)
            {
                phraseIsValidRegex = false;
            }
        }
        
        return add ? phraseToAdd : null;
    }
}
