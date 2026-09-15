using System;


namespace Fire_Pixel.Utility
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false)]
    public sealed class InspectorButtonCompatibilityAttribute : Attribute { }
}