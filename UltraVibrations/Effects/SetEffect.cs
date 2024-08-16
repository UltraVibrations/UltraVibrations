using System;
using UltraVibrations.Source;

namespace UltraVibrations.Effects;

public class SetEffect(ISource source, DateTime startTime, double duration, string[] channels): Effect(startTime, duration, channels)
{
    public override uint Priority => 2;
    public override double GetNext(double time, double value) => source.GetValue(time);
}
