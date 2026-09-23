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

[CustomPropertyDrawer(typeof(half3))]
public class Half3Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var xValue = property.FindPropertyRelative("x").FindPropertyRelative("value");
        var yValue = property.FindPropertyRelative("y").FindPropertyRelative("value");
        var zValue = property.FindPropertyRelative("z").FindPropertyRelative("value");

        var rect = EditorGUI.PrefixLabel(position, label);

        half3 tmp;
        tmp.x.value = (ushort)xValue.intValue;
        tmp.y.value = (ushort)yValue.intValue;
        tmp.z.value = (ushort)zValue.intValue;

        Vector3 vector = (float3)tmp;

        EditorGUI.BeginChangeCheck();
        vector = EditorGUI.Vector3Field(rect, string.Empty, vector);

        if (EditorGUI.EndChangeCheck())
        {
            tmp = new half3((float3)vector);

            xValue.intValue = tmp.x.value;
            yValue.intValue = tmp.y.value;
            zValue.intValue = tmp.z.value;
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
}
#endif