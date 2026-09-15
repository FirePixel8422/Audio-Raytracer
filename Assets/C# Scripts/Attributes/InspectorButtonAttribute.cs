using System;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class InspectorButtonAttribute : Attribute
{
    public string Label;

    public bool AllowUsageOutsidePlayMode = false;

    public InspectorButtonAttribute(string label = null, bool allowUsageOutsidePlayMode = false)
    {
        Label = label;
        AllowUsageOutsidePlayMode = allowUsageOutsidePlayMode;
    }
}