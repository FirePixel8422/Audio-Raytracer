using System;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        public readonly string condition;

        public ShowIfAttribute(string condition)
        {
            this.condition = condition;
        }
    }
}