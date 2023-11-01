﻿using System;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            EditorGUI.BeginDisabledGroup(IsDisabled(property, (ReadOnlyAttribute)saintsAttribute));
            return (true, position);
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            EditorGUI.EndDisabledGroup();
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error == "")
            {
                return position;
            }

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position, HelpBox.GetHeight(_error, position.width, MessageType.Error));
            HelpBox.Draw(errorRect, _error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            // Debug.Log("check extra height!");
            if (_error == "")
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        private bool IsDisabled(SerializedProperty property, ReadOnlyAttribute targetAttribute)
        {
            string by = targetAttribute.ReadOnlyBy;
            if(by is null)
            {
                return targetAttribute.ReadOnlyDirectValue;
            }

            UnityEngine.Object target = property.serializedObject.targetObject;

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);
            switch (found)
            {
                case (ReflectUtils.GetPropType.NotFound, _):
                {
                    _error = $"No field or method named `{by}` found on `{target}`";
                    Debug.LogError(_error);
                    return false;
                }
                case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
                {
                    return ReflectUtils.Truly(propertyInfo.GetValue(target));
                }
                case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
                {
                    return ReflectUtils.Truly(foundFieldInfo.GetValue(target));
                }
                case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
                {
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    return (bool)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(found), found, null);
            }
            //
            // List<Type> types = ReflectUtils.GetSelfAndBaseTypes(property.serializedObject.targetObject);
            // foreach (Type systemType in types)
            // {
            //     foreach (FieldInfo objFiledInfo in systemType
            //                  .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
            //                             BindingFlags.Public | BindingFlags.DeclaredOnly))
            //     {
            //         // Debug.LogError(objFiledInfo.Name);
            //         // ReSharper disable once InvertIf
            //         if (objFiledInfo.Name == by || objFiledInfo.Name == $"<{by}>k__BackingField")
            //         {
            //             _error = "";
            //
            //             // object fieldValue = null;
            //             // bool result;
            //             // try
            //             // {
            //             //     fieldValue = objFiledInfo.GetValue(target);
            //             //     // result = (objFiledInfo.GetValue(target) == null) || (bool) fieldValue;
            //             //     // if (fieldValue)
            //             //     // {
            //             //     //     result = true;
            //             //     // }
            //             //     result = Convert.ToBoolean(fieldValue);
            //             // }
            //             // catch (InvalidCastException)
            //             // {
            //             //     bool equalNull = fieldValue == null;
            //             //     if (equalNull)
            //             //     {
            //             //         result = false;
            //             //     }
            //             //     else
            //             //     {
            //             //         try
            //             //         {
            //             //             result = (UnityEngine.Object)fieldValue == null;
            //             //         }
            //             //         catch (InvalidCastException)
            //             //         {
            //             //             result = true;
            //             //         }
            //             //     }
            //             // }
            //             // catch (NullReferenceException)
            //             // {
            //             //     result = false;
            //             // }
            //
            //             // Debug.Log($"{by} = {result} / {objFiledInfo.GetValue(target)}");
            //             return ReflectUtils.Truly(objFiledInfo.GetValue(target));
            //         }
            //     }
            //
            //     foreach (MethodInfo objMethodInfo in systemType
            //                  .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
            //                              BindingFlags.Public | BindingFlags.DeclaredOnly))
            //     {
            //         // ReSharper disable once InvertIf
            //         if (objMethodInfo.Name == by)
            //         {
            //             _error = "";
            //
            //             ParameterInfo[] methodParams = objMethodInfo.GetParameters();
            //             Debug.Assert(methodParams.All(p => p.IsOptional));
            //             Debug.Assert(objMethodInfo.ReturnType == typeof(bool));
            //             return (bool)objMethodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
            //         }
            //     }
            // }
            //
            // _error = $"No field or method named `{by}` found on `{target}`";
            // Debug.LogError(_error);
            // return false;
        }
    }
}