using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace Fire_Pixel.Utility
{
    [CustomEditor(typeof(Button))]
    public class ButtonEditor : UnityEditor.UI.ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(Application.isPlaying ? "Click" : "Click (Play Mode Only)") && Application.isPlaying)
                    ((Button)target).onClick.Invoke();
            }
        }
    }
}