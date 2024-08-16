using System;
using UltraVibrations.Source;

namespace UltraVibrations.Effects;

public class AddEffect(ISource source, DateTime startTime, double duration, string[] channels) : Effect(startTime, duration, channels)
{
    public override uint Priority => 0;
    public override double GetNext(double time, double value) => value + source.GetValue(time);
}
