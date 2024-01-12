﻿using System;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabelWidth)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parent);
            _error = metaInfo.Error;
            if (_error != "")
            {
                DefaultDrawer(position, property, label);
                return;
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue = metaInfo.MinValue;
            float maxValue = metaInfo.MaxValue;

            float labelWidth = label.text == ""? 0: EditorGUIUtility.labelWidth;

            float leftFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.x, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.x, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth);
            leftFieldWidth += 5f;
            float rightFieldWidth = property.propertyType == SerializedPropertyType.Vector2
                ? GetNumberFieldWidth(property.vector2Value.y, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth)
                : GetNumberFieldWidth(property.vector2IntValue.y, minMaxSliderAttribute.MinWidth, minMaxSliderAttribute.MaxWidth);

            // float floatFieldWidth = EditorGUIUtility.fieldWidth;
            float sliderWidth = position.width - labelWidth - leftFieldWidth - rightFieldWidth;
            const float sliderPadding = 4f;

            (Rect labelWithMinFieldRect, Rect fieldRect) = RectUtils.SplitWidthRect(position, labelWidth + leftFieldWidth);

            (Rect sliderRect, Rect field3Rect) = RectUtils.SplitWidthRect(new Rect(fieldRect)
            {
                x = fieldRect.x + sliderPadding,
            }, sliderWidth - sliderPadding);

            (Rect maxFloatFieldRect, Rect _) = RectUtils.SplitWidthRect(new Rect(field3Rect)
            {
                x = field3Rect.x +sliderPadding,
            }, rightFieldWidth);

            // Draw the slider
            EditorGUI.BeginChangeCheck();

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 sliderValue = property.vector2Value;
                EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);

                // GUI.SetNextControlName(FieldControlName);
                sliderValue.x = EditorGUI.FloatField(labelWithMinFieldRect, label, sliderValue.x);
                sliderValue.x = Mathf.Clamp(sliderValue.x, minValue, Mathf.Min(maxValue, sliderValue.y));

                sliderValue.y = EditorGUI.FloatField(maxFloatFieldRect, sliderValue.y);
                sliderValue.y = Mathf.Clamp(sliderValue.y, Mathf.Max(minValue, sliderValue.x), maxValue);

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2Value = minMaxSliderAttribute.Step < 0
                        ? sliderValue
                        : BoundV2Step(sliderValue, minValue, maxValue, minMaxSliderAttribute.Step);
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                Vector2Int sliderValue = property.vector2IntValue;
                float xValue = sliderValue.x;
                float yValue = sliderValue.y;
                EditorGUI.MinMaxSlider(sliderRect, ref xValue, ref yValue, minValue, maxValue);

                // GUI.SetNextControlName(FieldControlName);
                sliderValue.x = EditorGUI.IntField(labelWithMinFieldRect, label, (int)xValue);
                sliderValue.x = (int)Mathf.Clamp(sliderValue.x, minValue, Mathf.Min(maxValue, sliderValue.y));

                sliderValue.y = EditorGUI.IntField(maxFloatFieldRect, (int)yValue);
                sliderValue.y = (int)Mathf.Clamp(sliderValue.y, Mathf.Max(minValue, sliderValue.x), maxValue);

                if (EditorGUI.EndChangeCheck())
                {
                    // Debug.Log(sliderValue);
                    int actualStep = Mathf.Max(1, Mathf.RoundToInt(minMaxSliderAttribute.Step));
                    property.vector2IntValue = actualStep == 1
                        ? sliderValue
                        : BoundV2IntStep(sliderValue, minValue, maxValue, actualStep);
                }
            }

            // ClickFocus(labelWithMinFieldRect, _fieldControlName);
        }

        private static float GetNumberFieldWidth(float value, float minWidth, float maxWidth) => GetFieldWidth($"{value}", minWidth, maxWidth);
        private static float GetNumberFieldWidth(int value, float minWidth, float maxWidth) => GetFieldWidth($"{value}", minWidth, maxWidth);

        private static float GetFieldWidth(string content, float minWidth, float maxWidth)
        {
            float actualWidth = EditorStyles.numberField.CalcSize(new GUIContent(content)).x;
            if (minWidth > 0 && actualWidth < minWidth)
            {
                return minWidth;
            }

            if (maxWidth > 0 && actualWidth > maxWidth)
            {
                return maxWidth;
            }

            return actualWidth;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private struct MetaInfo
        {
            public string Error;
            public float MinValue;
            public float MaxValue;
        }

        private static Vector2 BoundV2Step(Vector2 curValue, float min, float max, float step)
        {
            return new Vector2(
                Util.BoundFloatStep(curValue.x, min, max, step),
                Util.BoundFloatStep(curValue.y, min, max, step)
            );
        }

        private static Vector2Int BoundV2IntStep(Vector2Int curValue, float min, float max, int step)
        {
            return new Vector2Int(
                Util.BoundIntStep(curValue.x, min, max, step),
                Util.BoundIntStep(curValue.y, min, max, step)
            );
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, ISaintsAttribute saintsAttribute, object parentTarget)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return new MetaInfo
                {
                    Error = $"Expect Vector2 or Vector2Int, get {property.propertyType}",
                    MinValue = 0,
                    MaxValue = 1,
                };
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue;
            if (minMaxSliderAttribute.MinCallback == null)
            {
                minValue = minMaxSliderAttribute.Min;
            }
            else
            {
                (float getValue, string getError) = Util.GetCallbackFloat(parentTarget, minMaxSliderAttribute.MinCallback);
                if (!string.IsNullOrEmpty(getError))
                {
                    return new MetaInfo
                    {
                        Error = getError,
                        MinValue = 0,
                        MaxValue = 1,
                    };
                }
                minValue = getValue;
            }

            float maxValue;
            if (minMaxSliderAttribute.MaxCallback == null)
            {
                maxValue = minMaxSliderAttribute.Max;
            }
            else
            {
                (float getValue, string getError) = Util.GetCallbackFloat(parentTarget, minMaxSliderAttribute.MaxCallback);
                if (!string.IsNullOrEmpty(getError))
                {
                    return new MetaInfo
                    {
                        Error = getError,
                        MinValue = 0,
                        MaxValue = 1,
                    };
                }
                maxValue = getValue;
            }

            if (minValue > maxValue)
            {
                return new MetaInfo
                {
                    Error = $"invalid min ({minValue}) max ({maxValue}) value",
                    MinValue = 0,
                    MaxValue = 1,
                };
            }

            return new MetaInfo
            {
                Error = "",
                MinValue = minValue,
                MaxValue = maxValue,
            };
        }

        #region UIToolkit

        private static string NameSlider(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_Slider";
        private static string NameMinInteger(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MinIntegerField";
        private static string NameMaxInteger(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MaxIntegerField";
        private static string NameMinFloat(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MinFloatField";
        private static string NameMaxFloat(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_MaxFloatField";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__MinMaxSlider_HelpBox";

        private const int InputWidth = 50;

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, VisualElement container, Label fakeLabel, object parent)
        {
            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector2Int)
            {
                return null;
            }

            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Vector2 sliderValue = isInt ? property.vector2IntValue : property.vector2Value;

            MinMaxSlider minMaxSlider = new MinMaxSlider(sliderValue.x, sliderValue.y, sliderValue.x, sliderValue.y)
            {
                name = NameSlider(property),
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    paddingLeft = 5,
                    paddingRight = 5,
                },
                userData = new MetaInfo
                {
                    Error = "",
                    MinValue = sliderValue.x,
                    MaxValue = sliderValue.y,
                },
            };

            if (isInt)
            {
                Vector2Int curValue = property.vector2IntValue;

                root.Add(new IntegerField(new string(' ', property.displayName.Length))
                {
                    value = curValue.x,
                    name = NameMinInteger(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth + LabelBaseWidth,
                    },
                });
                root.Add(minMaxSlider);
                root.Add(new IntegerField
                {
                    value = curValue.y,
                    name = NameMaxInteger(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
            }
            else
            {
                Vector2 curValue = property.vector2Value;
                // slider.SetValueWithoutNotify(curValue);

                root.Add(new FloatField(new string(' ', property.displayName.Length))
                {
                    value = curValue.x,
                    name = NameMinFloat(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth + LabelBaseWidth,
                    },
                });
                root.Add(minMaxSlider);
                root.Add(new FloatField
                {
                    value = curValue.y,
                    name = NameMaxFloat(property),
                    style =
                    {
                        flexGrow = 0,
                        width = InputWidth,
                    },
                });
            }

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 1,
                },
                name = NameHelpBox(property),
            };
        }

        protected override void OnAwakeUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, object parent)
        {
            // if (property.propertyType != SerializedPropertyType.Vector2 &&
            //     property.propertyType != SerializedPropertyType.Vector2Int)
            // {
            //     return;
            // }

            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            MinMaxSlider minMaxSlider = container.Q<MinMaxSlider>(NameSlider(property));
            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;

            if (isInt)
            {
                IntegerField minIntField = container.Q<IntegerField>(NameMinInteger(property));
                IntegerField maxIntField = container.Q<IntegerField>(NameMaxInteger(property));
                minMaxSlider.RegisterValueChangedCallback(changed =>
                {
                    MetaInfo metaInfo = (MetaInfo)minMaxSlider.userData;
                    ApplyIntValue(property, minMaxSliderAttribute.Step, changed.newValue, (int)metaInfo.MinValue,
                        (int)metaInfo.MaxValue, minMaxSlider, minIntField, maxIntField, onValueChangedCallback);
                });
                minIntField.RegisterValueChangedCallback(changed =>
                {
                    MetaInfo metaInfo = (MetaInfo)minMaxSlider.userData;
                    ApplyIntValue(property, minMaxSliderAttribute.Step,
                        new Vector2(changed.newValue, maxIntField.value), (int)metaInfo.MinValue,
                        (int)metaInfo.MaxValue, minMaxSlider, minIntField, maxIntField, onValueChangedCallback);
                });
                maxIntField.RegisterValueChangedCallback(changed =>
                {
                    MetaInfo metaInfo = (MetaInfo)minMaxSlider.userData;
                    ApplyIntValue(property, minMaxSliderAttribute.Step,
                        new Vector2(minIntField.value, changed.newValue), (int)metaInfo.MinValue,
                        (int)metaInfo.MaxValue, minMaxSlider, minIntField, maxIntField, onValueChangedCallback);
                });
            }
            else
            {
                FloatField minFloatField = container.Q<FloatField>(NameMinFloat(property));
                FloatField maxFloatField = container.Q<FloatField>(NameMaxFloat(property));

                minMaxSlider.RegisterValueChangedCallback(changed =>
                {
                    MetaInfo metaInfo = (MetaInfo)minMaxSlider.userData;
                    ApplyFloatValue(property, minMaxSliderAttribute.Step, changed.newValue, metaInfo.MinValue,
                        metaInfo.MaxValue, minMaxSlider, minFloatField, maxFloatField, onValueChangedCallback);
                });
                minFloatField.RegisterValueChangedCallback(changed =>
                {
                    MetaInfo metaInfo = (MetaInfo)minMaxSlider.userData;
                    ApplyFloatValue(property, minMaxSliderAttribute.Step,
                        new Vector2(changed.newValue, maxFloatField.value), metaInfo.MinValue,
                        metaInfo.MaxValue, minMaxSlider, minFloatField, maxFloatField, onValueChangedCallback);
                });
                maxFloatField.RegisterValueChangedCallback(changed =>
                {
                    MetaInfo metaInfo = (MetaInfo)minMaxSlider.userData;
                    ApplyFloatValue(property, minMaxSliderAttribute.Step,
                        new Vector2(maxFloatField.value, changed.newValue), metaInfo.MinValue,
                        metaInfo.MaxValue, minMaxSlider, minFloatField, maxFloatField, onValueChangedCallback);
                });
            }
        }

        protected override void OnUpdateUiToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            // bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;

            MinMaxSlider minMaxSlider = container.Q<MinMaxSlider>(NameSlider(property));
            // MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            MetaInfo curMetaInfo = (MetaInfo)minMaxSlider.userData;
            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, parent);

            bool changed = false;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (metaInfo.Error == "" && metaInfo.MinValue != curMetaInfo.MinValue
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || metaInfo.MaxValue != curMetaInfo.MaxValue)
            {
                changed = true;
                // WTF Unity? Fix your shit!
                if (metaInfo.MinValue >= minMaxSlider.highLimit)
                {
                    minMaxSlider.highLimit = metaInfo.MaxValue;
                    minMaxSlider.lowLimit = metaInfo.MinValue;
                }
                else
                {
                    minMaxSlider.lowLimit = metaInfo.MinValue;
                    minMaxSlider.highLimit = metaInfo.MaxValue;
                }
            }

            if (metaInfo.Error != curMetaInfo.Error)
            {
                changed = true;
                HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property));
                helpBox.text = metaInfo.Error;
                helpBox.style.display = metaInfo.Error == "" ? DisplayStyle.None : DisplayStyle.Flex;

                minMaxSlider.SetEnabled(metaInfo.Error == "");
            }

            if (changed)
            {
                minMaxSlider.userData = metaInfo;
            }
        }

        private static void ApplyIntValue(SerializedProperty property, float step, Vector2 sliderValue, int minValue, int maxValue, MinMaxSlider slider, IntegerField minField, IntegerField maxField, Action<object> onValueChangedCallback)
        {
            int actualStep = Mathf.Max(1, Mathf.RoundToInt(step));
            Vector2Int vector2IntValue = new Vector2Int(
                Util.BoundIntStep(sliderValue.x, minValue, maxValue, actualStep),
                Util.BoundIntStep(sliderValue.y, minValue, maxValue, actualStep)
            );

            property.vector2IntValue = vector2IntValue;
            property.serializedObject.ApplyModifiedProperties();
            // Debug.Log($"update value to {vector2IntValue}");
            onValueChangedCallback.Invoke(vector2IntValue);

            slider.SetValueWithoutNotify(vector2IntValue);
            minField.SetValueWithoutNotify(vector2IntValue.x);
            maxField.SetValueWithoutNotify(vector2IntValue.y);
        }

        private static void ApplyFloatValue(SerializedProperty property, float step, Vector2 sliderValue, float minValue, float maxValue, MinMaxSlider slider, FloatField minField, FloatField maxField, Action<object> onValueChangedCallback)
        {
            Vector2 vector2Value = step <= 0f
                ? sliderValue
                : BoundV2Step(sliderValue, minValue, maxValue, step);

            property.vector2Value = vector2Value;
            property.serializedObject.ApplyModifiedProperties();
            onValueChangedCallback.Invoke(vector2Value);

            slider.SetValueWithoutNotify(vector2Value);
            minField.SetValueWithoutNotify(vector2Value.x);
            maxField.SetValueWithoutNotify(vector2Value.y);
        }

        #endregion
    }
}
