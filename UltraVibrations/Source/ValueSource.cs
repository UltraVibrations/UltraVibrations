namespace UltraVibrations.Source;

public class ValueSource(double value, double duration) : ISource
{
    public double GetValue(double time) => value;
    public double GetDuration() => duration;
}
