using UnityEditor;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    [CustomPropertyDrawer(typeof(InspectorNameAttribute))]
    public sealed class RenameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InspectorNameAttribute attr = (InspectorNameAttribute)attribute;

            // only override text, keep tooltip/context intact
            if (!string.IsNullOrEmpty(attr.label))
            {
                label = new GUIContent(attr.label, label.tooltip);
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}