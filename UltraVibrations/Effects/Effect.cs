using System;

namespace UltraVibrations.Effects;

public abstract class Effect(DateTime startTime, double duration, string[] channels)
{
    public abstract uint Priority { get; }
    public DateTime StartTime { get; } = startTime;
    public double Duration { get; } = duration;
    public DateTime EndTime => StartTime.AddMilliseconds(Duration);
    public string[] Channels { get; } = channels;
    public abstract double GetNext(double time, double value);
}
