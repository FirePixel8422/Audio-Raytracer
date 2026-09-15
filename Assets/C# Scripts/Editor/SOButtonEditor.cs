using UnityEditor;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class SOButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath == "m_Script")
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(property);
                    EditorGUI.EndDisabledGroup();
                    continue;
                }

                InspectorButtonDrawer.DrawProperty(serializedObject, property);
            }

            InspectorButtonDrawer.DrawObjectMethods(target);

            serializedObject.ApplyModifiedProperties();
        }
    }
}