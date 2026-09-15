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

        public static float GetPropertyHeight(
            SerializedObject serializedObject,
            SerializedProperty property)
        {
            Type type = GetPropertyType(property);

            if (type == null || !IsMarked(type))
                return EditorGUI.GetPropertyHeight(property, true);

            float height = EditorGUI.GetPropertyHeight(property, true);

            if (!property.isExpanded)
                return height;

            object value = GetValue(
                serializedObject.targetObject,
                property.propertyPath);

            if (value == null)
                return height;

            height += GetMethodsHeight(
                value,
                serializedObject.targetObject,
                property.propertyPath);

            return height;
        }

        public static void DrawProperty(
            Rect position,
            SerializedObject serializedObject,
            SerializedProperty property)
        {
            Type type = GetPropertyType(property);

            if (type == null)
            {
                EditorGUI.PropertyField(
                    position,
                    property,
                    true);

                return;
            }

            WarnIfMissingCompatibility(
                type,
                serializedObject.targetObject);

            if (!IsMarked(type))
            {
                EditorGUI.PropertyField(
                    position,
                    property,
                    true);

                return;
            }

            DrawMarkedProperty(
                position,
                serializedObject,
                property);
        }

        public static void DrawProperty(
            SerializedObject serializedObject,
            SerializedProperty property)
        {
            float height = GetPropertyHeight(
                serializedObject,
                property);

            Rect position = EditorGUILayout.GetControlRect(
                false,
                height);

            DrawProperty(
                position,
                serializedObject,
                property);
        }

        public static void DrawObjectMethods(object obj)
        {
            DrawMethods(
                obj,
                obj as UnityEngine.Object,
                obj.GetType().Name,
                GUILayoutUtility.GetLastRect());
        }

        private static void DrawMarkedProperty(
            Rect position,
            SerializedObject serializedObject,
            SerializedProperty property)
        {
            float propertyHeight =
                EditorGUI.GetPropertyHeight(
                    property,
                    true);

            Rect propertyRect = new Rect(
                position.x,
                position.y,
                position.width,
                propertyHeight);

            EditorGUI.PropertyField(
                propertyRect,
                property,
                true);

            if (!property.isExpanded)
                return;

            object value = GetValue(
                serializedObject.targetObject,
                property.propertyPath);

            if (value == null)
                return;

            Rect methodsRect = new Rect(
                position.x,
                propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                position.height - propertyHeight);

            DrawMethods(
                value,
                serializedObject.targetObject,
                property.propertyPath,
                methodsRect);
        }

        private static float GetMethodsHeight(
            object obj,
            UnityEngine.Object rootObject,
            string path)
        {
            MethodInfo[] methods = obj.GetType().GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            float height = 0f;

            foreach (MethodInfo method in methods)
            {
                InspectorButtonAttribute button =
                    method.GetCustomAttribute<InspectorButtonAttribute>();

                if (button == null)
                    continue;

                height += GetMethodHeight(
                    rootObject,
                    method,
                    button,
                    path);
            }

            return height;
        }

        private static float GetMethodHeight(
            UnityEngine.Object rootObject,
            MethodInfo method,
            InspectorButtonAttribute button,
            string path)
        {
            MethodCacheEntry entry = GetCacheEntry(
                rootObject,
                method,
                path);

            float height =
                22f +
                EditorGUIUtility.standardVerticalSpacing;

            if (entry.parameters.Length > 0)
            {
                height += entry.parameters.Length *
                    (EditorGUIUtility.singleLineHeight +
                     EditorGUIUtility.standardVerticalSpacing);
            }

            return height;
        }

        private static void DrawMethods(
            object obj,
            UnityEngine.Object rootObject,
            string path,
            Rect position)
        {
            MethodInfo[] methods = obj.GetType().GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            float y = position.y;

            foreach (MethodInfo method in methods)
            {
                InspectorButtonAttribute button =
                    method.GetCustomAttribute<InspectorButtonAttribute>();

                if (button == null)
                    continue;

                float height = GetMethodHeight(
                    rootObject,
                    method,
                    button,
                    path);

                Rect methodRect = new Rect(
                    position.x,
                    y,
                    position.width,
                    height);

                DrawMethod(
                    methodRect,
                    obj,
                    rootObject,
                    method,
                    button,
                    path);

                y += height;
            }
        }

        private static MethodCacheEntry GetCacheEntry(
            UnityEngine.Object rootObject,
            MethodInfo method,
            string path)
        {
            string key =
                $"{rootObject.GetEntityId()}_{path}_{method.MetadataToken}";

            if (cache.TryGetValue(
                    key,
                    out MethodCacheEntry entry))
            {
                return entry;
            }

            ParameterInfo[] parameters =
                method.GetParameters();

            entry = new MethodCacheEntry
            {
                method = method,
                parameters = parameters,
                args = new object[parameters.Length]
            };

            cache[key] = entry;

            return entry;
        }

        private static void DrawMethod(
            Rect position,
            object obj,
            UnityEngine.Object rootObject,
            MethodInfo method,
            InspectorButtonAttribute button,
            string path)
        {
            bool allowed =
                button.AllowUsageOutsidePlayMode ||
                EditorApplication.isPlaying;

            MethodCacheEntry entry = GetCacheEntry(
                rootObject,
                method,
                path);

            string label =
                string.IsNullOrEmpty(button.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : button.Label;

            if (!allowed)
                label += " (Play Mode Only)";

            Rect buttonRect = new Rect(
                position.x,
                position.y,
                position.width,
                22f);

            using (new EditorGUI.DisabledScope(!allowed))
            {
                if (GUI.Button(
                        buttonRect,
                        label))
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

            if (entry.parameters.Length == 0)
                return;

            float y =
                buttonRect.yMax +
                EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;

            for (int i = 0;
                 i < entry.parameters.Length;
                 i++)
            {
                Rect parameterRect = new Rect(
                    position.x,
                    y,
                    position.width,
                    EditorGUIUtility.singleLineHeight);

                entry.args[i] = DrawParameter(
                    parameterRect,
                    entry.parameters[i],
                    entry.args[i]);

                y +=
                    EditorGUIUtility.singleLineHeight +
                    EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        private static object DrawParameter(
            Rect position,
            ParameterInfo parameter,
            object current)
        {
            Type type = parameter.ParameterType;

            if (type == typeof(int))
            {
                return EditorGUI.IntField(
                    position,
                    parameter.Name,
                    current != null
                        ? (int)current
                        : 0);
            }

            if (type == typeof(float))
            {
                return EditorGUI.FloatField(
                    position,
                    parameter.Name,
                    current != null
                        ? (float)current
                        : 0f);
            }

            if (type == typeof(bool))
            {
                return EditorGUI.Toggle(
                    position,
                    parameter.Name,
                    current != null &&
                    (bool)current);
            }

            if (type == typeof(string))
            {
                return EditorGUI.TextField(
                    position,
                    parameter.Name,
                    current as string ?? "");
            }

            if (type == typeof(Vector3))
            {
                return EditorGUI.Vector3Field(
                    position,
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
                    ? EditorGUI.EnumFlagsField(
                        position,
                        parameter.Name,
                        value)
                    : EditorGUI.EnumPopup(
                        position,
                        parameter.Name,
                        value);
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return EditorGUI.ObjectField(
                    position,
                    parameter.Name,
                    current as UnityEngine.Object,
                    type,
                    true);
            }

            EditorGUI.LabelField(
                position,
                $"{parameter.Name} (unsupported: {type.Name})");

            return current;
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