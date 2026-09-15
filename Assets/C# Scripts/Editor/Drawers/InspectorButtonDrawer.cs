using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Fire_Pixel.Utility
{
    public static class InspectorButtonDrawer
    {
        private class MethodCacheEntry
        {
            public MethodInfo method;
            public ParameterInfo[] parameters;
            public object[] args;
        }

        private static readonly Dictionary<string, MethodCacheEntry> cache = new();
        private static readonly HashSet<Type> compatibilityWarnings = new();

        public static void DrawProperty(
            SerializedObject serializedObject,
            SerializedProperty property)
        {
            Type type = GetPropertyType(property);

            if (type == null)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            WarnIfMissingCompatibility(
                type,
                serializedObject.targetObject);

            if (!IsMarked(type))
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            DrawMarkedProperty(
                serializedObject,
                property);
        }

        private static void DrawMarkedProperty(
            SerializedObject serializedObject,
            SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property, true);

            if (!property.isExpanded)
                return;

            object value = GetValue(
                serializedObject.targetObject,
                property.propertyPath);

            if (value != null)
            {
                DrawMethods(
                    value,
                    serializedObject.targetObject,
                    property.propertyPath);
            }
        }

        private static void DrawMethods(
            object obj,
            UnityEngine.Object rootObject,
            string path)
        {
            MethodInfo[] methods = obj.GetType().GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                InspectorButtonAttribute button =
                    method.GetCustomAttribute<InspectorButtonAttribute>();

                if (button == null)
                    continue;

                DrawMethod(
                    obj,
                    rootObject,
                    method,
                    button,
                    path);
            }
        }

        public static void DrawObjectMethods(object obj)
        {
            DrawMethods(
                obj,
                obj as UnityEngine.Object,
                obj.GetType().Name);
        }

        private static void WarnIfMissingCompatibility(
            Type type,
            UnityEngine.Object rootObject)
        {
            if (type.GetCustomAttribute<InspectorButtonCompatibilityAttribute>() != null)
                return;

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InspectorButtonAttribute>() == null)
                    continue;

                if (compatibilityWarnings.Add(type))
                {
                    Debug.LogError(
                        $"{ObjectNames.NicifyVariableName(type.Name)} is missing [InspectorButtonCompatibility]. [InspectorButton] only works on non-MonoBehaviour targets when they have the [InspectorButtonCompatibility] attribute.",
                        rootObject);
                }

                return;
            }
        }

        private static void DrawMethod(
            object obj,
            UnityEngine.Object rootObject,
            MethodInfo method,
            InspectorButtonAttribute button,
            string path)
        {
            bool allowed =
                button.AllowUsageOutsidePlayMode ||
                EditorApplication.isPlaying;

            string key =
                $"{rootObject.GetEntityId()}_{path}_{method.MetadataToken}";

            if (!cache.TryGetValue(
                    key,
                    out MethodCacheEntry entry))
            {
                ParameterInfo[] parameters =
                    method.GetParameters();

                entry = new MethodCacheEntry
                {
                    method = method,
                    parameters = parameters,
                    args = new object[parameters.Length]
                };

                cache[key] = entry;
            }

            string label =
                string.IsNullOrEmpty(button.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : button.Label;

            if (!allowed)
                label += " (Play Mode Only)";

            using (new EditorGUI.DisabledScope(!allowed))
            {
                if (GUILayout.Button(
                        label,
                        GUILayout.Height(22)))
                {
                    Undo.RecordObject(
                        rootObject,
                        label);

                    try
                    {
                        method.Invoke(
                            obj,
                            entry.args.Length == 0
                                ? null
                                : entry.args);

                        EditorUtility.SetDirty(rootObject);
                        SerializedObjectUpdate(rootObject);
                    }
                    catch (TargetInvocationException exception)
                    {
                        Debug.LogException(
                            exception.InnerException ?? exception);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }

            if (entry.parameters.Length > 0)
            {
                EditorGUI.indentLevel++;

                for (int i = 0;
                     i < entry.parameters.Length;
                     i++)
                {
                    entry.args[i] = DrawParameter(
                        entry.parameters[i],
                        entry.args[i]);
                }

                EditorGUI.indentLevel--;
            }
        }

        private static object DrawParameter(
            ParameterInfo parameter,
            object current)
        {
            Type type = parameter.ParameterType;

            if (type == typeof(int))
            {
                return EditorGUILayout.IntField(
                    parameter.Name,
                    current != null
                        ? (int)current
                        : 0);
            }

            if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(
                    parameter.Name,
                    current != null
                        ? (float)current
                        : 0f);
            }

            if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(
                    parameter.Name,
                    current != null &&
                    (bool)current);
            }

            if (type == typeof(string))
            {
                return EditorGUILayout.TextField(
                    parameter.Name,
                    current as string ?? "");
            }

            if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(
                    parameter.Name,
                    current != null
                        ? (Vector3)current
                        : Vector3.zero);
            }

            if (type.IsEnum)
            {
                Enum value =
                    current as Enum ??
                    (Enum)Activator.CreateInstance(type);

                return Attribute.IsDefined(
                    type,
                    typeof(FlagsAttribute))
                    ? EditorGUILayout.EnumFlagsField(
                        parameter.Name,
                        value)
                    : EditorGUILayout.EnumPopup(
                        parameter.Name,
                        value);
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(
                    parameter.Name,
                    current as UnityEngine.Object,
                    type,
                    true);
            }

            EditorGUILayout.LabelField(
                $"{parameter.Name} (unsupported: {type.Name})");

            return current;
        }

        private static Type GetPropertyType(
            SerializedProperty property)
        {
            Type type =
                property.serializedObject.targetObject.GetType();

            string[] parts =
                property.propertyPath.Split('.');

            foreach (string part in parts)
            {
                if (part == "Array" ||
                    part == "data")
                {
                    return null;
                }

                FieldInfo field = type.GetField(
                    part,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (field == null)
                    return null;

                type = field.FieldType;
            }

            return type;
        }

        private static bool IsMarked(Type type) =>
            type.GetCustomAttribute<InspectorButtonCompatibilityAttribute>() != null;

        private static object GetValue(
            object obj,
            string path)
        {
            string[] parts =
                path.Split('.');

            foreach (string part in parts)
            {
                FieldInfo field = obj.GetType().GetField(
                    part,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (field == null)
                    return null;

                obj = field.GetValue(obj);

                if (obj == null)
                    return null;
            }

            return obj;
        }

        private static void SerializedObjectUpdate(
            UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
        }
    }
}