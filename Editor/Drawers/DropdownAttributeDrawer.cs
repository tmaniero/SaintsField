﻿using System;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public class DropdownAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            DropdownAttribute dropdownAttribute = (DropdownAttribute) saintsAttribute;
            IDropdownList dropdownListValue;

            string funcName = dropdownAttribute.FuncName;
            object parentObj = GetParentTarget(property);
            Debug.Assert(parentObj != null);
            Type parentType = parentObj.GetType();
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(parentType, funcName);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error = $"not found `{funcName}` on target `{parentObj}`";
                    DefaultDrawer(position, property, label);
                }
                    return;
                case ReflectUtils.GetPropType.Property:
                {
                    PropertyInfo foundPropertyInfo = (PropertyInfo)fieldOrMethodInfo;
                    dropdownListValue = foundPropertyInfo.GetValue(parentObj) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo foundFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    dropdownListValue = foundFieldInfo.GetValue(parentObj) as IDropdownList;
                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(string));
                    // if (methodInfo.ReturnType != typeof(string))
                    // {
                    //     _error =
                    //         $"Return type of callback method `{decButtonAttribute.ButtonLabel}` should be string";
                    //     return decButtonAttribute.ButtonLabel;
                    // }

                    _error = "";
                    try
                    {
                        dropdownListValue =
                            methodInfo.Invoke(parentObj, methodParams.Select(p => p.DefaultValue).ToArray()) as
                                IDropdownList;
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.Assert(e.InnerException != null);
                        _error = e.InnerException.Message;
                        Debug.LogException(e);
                        return;
                    }
                    catch (Exception e)
                    {
                        _error = e.Message;
                        Debug.LogException(e);
                        return;
                    }

                    if (dropdownListValue == null)
                    {
                        _error = $"dropdownListValue is null from `{funcName}()` on target `{parentObj}`";
                        DefaultDrawer(position, property, label);
                        return;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }

            // int selectedIndex = -1;
            // Debug.Log(property.propertyPath);

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                              BindingFlags.Public | BindingFlags.DeclaredOnly;
            // Object target = property.serializedObject.targetObject;
            FieldInfo field = parentType.GetField(property.name, bindAttr);
            Debug.Assert(field != null, $"{property.name}/{parentObj}");
            object curValue = field.GetValue(parentObj);
            // Debug.Log($"get cur value {curValue}, {parentObj}->{field}");
            string curDisplay = "";
            Debug.Assert(dropdownListValue != null);
            foreach (ValueTuple<string, object, bool, bool> itemInfos in dropdownListValue.Where(each => !each.Item4))
            {
                string name = itemInfos.Item1;
                object itemValue = itemInfos.Item2;

                if (curValue == null && itemValue == null)
                {
                    curDisplay = name;
                    break;
                }
                if (curValue is Object curValueObj
                          && curValueObj == itemValue as Object)
                {
                    curDisplay = name;
                    break;
                }
                if (itemValue == null)
                {
                    // nothing
                }
                else if (itemValue.Equals(curValue))
                {
                    curDisplay = name;
                    break;
                }
            }

            bool hasLabel = label.text != "";
            float labelWidth = hasLabel? EditorGUIUtility.labelWidth: 0;
            Rect labelRect = new Rect(position)
            {
                width = labelWidth,
            };
            // (Rect labelRect, Rect fieldRect) = RectUtils.SplitWidthRect(position, labelWidth);
            //
            // EditorGUI.LabelField(labelRect, label);

            Rect fieldRect = EditorGUI.PrefixLabel(position, label);

            // int newIndex = EditorGUI.Popup(position, label, selectedIndex, options.Select(each => new GUIContent(each)).ToArray());
            GUI.SetNextControlName(FieldControlName);
            // string display = selectedIndex == -1 ? "" : options[selectedIndex];
            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(curDisplay), FocusType.Keyboard))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                Debug.Assert(dropdownListValue != null);
                foreach (ValueTuple<string, object, bool, bool> itemInfo in dropdownListValue)
                {
                    string curName = itemInfo.Item1;
                    object curItem = itemInfo.Item2;
                    bool disabled = itemInfo.Item3;
                    bool curIsSeparator = itemInfo.Item4;
                    if (curIsSeparator)
                    {
                        menu.AddSeparator(curName);
                    }
                    else if (disabled)
                    {
                        // Debug.Log($"disabled: {curName}");
                        menu.AddDisabledItem(new GUIContent(curName), curName == curDisplay);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent(curName), curName == curDisplay, () =>
                        {
                            // selectedIndex = options.IndexOf(option);
                            Undo.RecordObject(property.serializedObject.targetObject, "Dropdown");
                            // object newValue = curItem;
                            // Debug.Log($"set value {parentObj}->{field.Name} = {curItem}");
                            if(!parentType.IsValueType)  // reference type
                            {
                                // Debug.Log($"not struct");
                                field.SetValue(parentObj, curItem);
                            }
                            else  // hack struct :(
                            {
                                // Debug.Log($"{property.propertyType}: {curItem}");
                                switch (property.propertyType)
                                {
                                    case SerializedPropertyType.Generic:
                                        property.objectReferenceValue = (Object) curItem;
                                        break;
                                    case SerializedPropertyType.LayerMask:
                                    case SerializedPropertyType.Integer:
                                    case SerializedPropertyType.Enum:
                                        property.intValue = (int) curItem;
                                        // Debug.Log($"{property.propertyType}: set, ={property.intValue}");
                                        break;
                                    case SerializedPropertyType.Boolean:
                                        property.boolValue = (bool) curItem;
                                        break;
                                    case SerializedPropertyType.Float:
                                        property.floatValue = (float) curItem;
                                        break;
                                    case SerializedPropertyType.String:
                                        property.stringValue = curItem.ToString();
                                        break;
                                    case SerializedPropertyType.Color:
                                        property.colorValue = (Color) curItem;
                                        break;
                                    case SerializedPropertyType.ObjectReference:
                                        property.objectReferenceValue = (Object) curItem;
                                        break;
                                    case SerializedPropertyType.Vector2:
                                        property.vector2Value = (Vector2) curItem;
                                        break;
                                    case SerializedPropertyType.Vector3:
                                        property.vector3Value = (Vector3) curItem;
                                        break;
                                    case SerializedPropertyType.Vector4:
                                        property.vector4Value = (Vector4) curItem;
                                        break;
                                    case SerializedPropertyType.Rect:
                                        property.rectValue = (Rect) curItem;
                                        break;
                                    case SerializedPropertyType.ArraySize:
                                        property.arraySize = (int) curItem;
                                        break;
                                    case SerializedPropertyType.Character:
                                        property.intValue = (char) curItem;
                                        break;
                                    case SerializedPropertyType.AnimationCurve:
                                        property.animationCurveValue = (AnimationCurve) curItem;
                                        break;
                                    case SerializedPropertyType.Bounds:
                                        property.boundsValue = (Bounds) curItem;
                                        break;
                                    // case SerializedPropertyType.Gradient:
                                    //     property.gradientValue = (Gradient) curItem;
                                    //     break;
                                    case SerializedPropertyType.Quaternion:
                                        property.quaternionValue = (Quaternion) curItem;
                                        break;
                                    case SerializedPropertyType.ExposedReference:
                                        property.exposedReferenceValue = (Object) curItem;
                                        break;
                                    // case SerializedPropertyType.FixedBufferSize:
                                    //     property.fixedBufferSize = (int) curItem;
                                    //     break;
                                    case SerializedPropertyType.Vector2Int:
                                        property.vector2IntValue = (Vector2Int) curItem;
                                        break;
                                    case SerializedPropertyType.Vector3Int:
                                        property.vector3IntValue = (Vector3Int) curItem;
                                        break;
                                    case SerializedPropertyType.RectInt:
                                        property.rectIntValue = (RectInt) curItem;
                                        break;
                                    case SerializedPropertyType.BoundsInt:
                                        property.boundsIntValue = (BoundsInt) curItem;
                                        break;
                                    case SerializedPropertyType.ManagedReference:
                                        property.managedReferenceValue = (Object) curItem;
                                        break;
                                    case SerializedPropertyType.Gradient:
                                    case SerializedPropertyType.FixedBufferSize:
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
                                }

                                property.serializedObject.ApplyModifiedProperties();
                                SetValueChanged(property);
                            }

                        });
                    }
                }

                // for (int index = 0; index < options.Count; index++)
                // {
                //     int curIndex = index;
                //     string option = options[curIndex];
                //     menu.AddItem(new GUIContent(option), curIndex == selectedIndex, () =>
                //     {
                //         // selectedIndex = options.IndexOf(option);
                //         Undo.RecordObject(target, "Dropdown");
                //         object newValue = values[curIndex];
                //         field.SetValue(target, newValue);
                //     });
                // }

                // display the menu
                // menu.ShowAsContext();
                menu.DropDown(fieldRect);
            }

            if(hasLabel)
            {
                ClickFocus(labelRect, FieldControlName);
            }

            // int newIndex = selectedIndex;
            // // ReSharper disable once InvertIf
            // if (changed.changed)
            // {
            //     Undo.RecordObject(target, "Dropdown");
            //     object newValue = values[newIndex];
            //     field.SetValue(target, newValue);
            // }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
