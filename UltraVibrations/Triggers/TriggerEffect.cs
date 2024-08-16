using System.Collections.Generic;
using UltraVibrations.Source;

namespace UltraVibrations.Triggers;

public class TriggerEffect
{
    public EffectSource EffectSource = EffectSource.None;
    public MixMode MixMode = MixMode.None;
    public readonly List<string> Channels = [];
    
    // Value
    public double Value = 0.0;
    public double Duration = 0.0;
    
    // Pattern
    public List<(double, double)> Pattern = [];
    public PatternSource.InterpolationMode Interpolation = PatternSource.InterpolationMode.None;
}
