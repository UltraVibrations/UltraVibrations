using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace UltraVibrations.Source;

public class PatternSource : ISource
{
    public readonly List<PatternStep> Steps = [];
    public readonly InterpolationMode Interpolation;

    [JsonIgnore]
    private List<(double, double, double)>? preparedTimeline;

    public PatternSource(List<(double, double)> pattern, InterpolationMode interpolationMode)
    {
        Interpolation = interpolationMode;
        foreach (var (value, duration) in pattern)
        {
            Steps.Add(new PatternStep(value, duration));
        }
    }

    private void PrepareTimeline()
    {
        if (preparedTimeline != null)
        {
            return;
        }

        preparedTimeline = [];
        double currentDuration = 0;
        foreach (var step in Steps)
        {
            preparedTimeline.Add((currentDuration, step.Value, step.Duration));
            currentDuration += step.Duration;
        }
    }

    public double GetValue(double time)
    {
        PrepareTimeline();

        if (preparedTimeline == null)
        {
            return 0;
        }

        var wrappedTime = time % GetDuration();
        var currentIdx = preparedTimeline.FindIndex(t => t.Item1 > wrappedTime);
        var currentStep = preparedTimeline[currentIdx];
        var nextStep = preparedTimeline[(currentIdx + 1) % preparedTimeline.Count];

        var progress = (wrappedTime - currentStep.Item1) / currentStep.Item3;

        return Interpolation switch
        {
            InterpolationMode.None => currentStep.Item2,
            InterpolationMode.Lerp => Lerp(currentStep.Item2, nextStep.Item2, progress),
            InterpolationMode.EaseInSine => EaseInSine(currentStep.Item2, nextStep.Item2, progress),
            InterpolationMode.EaseOutSine => EaseOutSine(currentStep.Item2, nextStep.Item2, progress),
            InterpolationMode.EaseInOutSine => EaseInOutSine(currentStep.Item2, nextStep.Item2, progress),
            _ => 0,
        };
    }

    public double GetDuration()
    {
        return Steps.Sum(step => step.Duration);
    }


    public class PatternStep(double value, double duration)
    {
        public readonly double Value = value;
        public readonly double Duration = duration;
    }

    public enum InterpolationMode
    {
        None,
        Lerp,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
    }

    private static double Lerp(double t)
    {
        return t;
    }

    private static double Lerp(double a, double b, double t)
    {
        return a + ((b - a) * Lerp(t));
    }

    private static double EaseInSine(double t)
    {
        return 1 - Math.Cos(t * Math.PI / 2);
    }
    
    private static double EaseInSine(double a, double b, double t)
    {
        return a + ((b - a) * EaseInSine(t));
    }

    private static double EaseOutSine(double t)
    {
        return Math.Sin(t * Math.PI / 2);
    }
    
    private static double EaseOutSine(double a, double b, double t)
    {
        return a + ((b - a) * EaseOutSine(t));
    }

    private static double EaseInOutSine(double t)
    {
        return -(Math.Cos(Math.PI * t) - 1) / 2;
    }
    
    private static double EaseInOutSine(double a, double b, double t)
    {
        return a + ((b - a) * EaseInOutSine(t));
    }
}
