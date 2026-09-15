using System;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class InspectorNameAttribute : PropertyAttribute
    {
        public readonly string label;

        public InspectorNameAttribute(string label)
        {
            this.label = label;
        }
    }
}