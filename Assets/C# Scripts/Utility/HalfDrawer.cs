#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

[CustomPropertyDrawer(typeof(half))]
public class HalfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var oldCol = GUI.contentColor;
        var value = property.FindPropertyRelative("value");
        if (value.hasMultipleDifferentValues)
            GUI.contentColor = Color.red;
        var rect = EditorGUI.PrefixLabel(position, label);
        half tmp;
        tmp.value = (ushort)value.intValue;
        EditorGUI.BeginChangeCheck();
        tmp = new half(EditorGUI.FloatField(rect, tmp));
        if (EditorGUI.EndChangeCheck())
            value.intValue = tmp.value;
        GUI.contentColor = oldCol;
    }
}
#endif
