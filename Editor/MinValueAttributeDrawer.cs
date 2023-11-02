﻿using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    [CustomPropertyDrawer(typeof(MinValueAttribute))]
    public class MinValueAttributeDrawer : SaintsPropertyDrawer
    {
        private string _error = "";
        private int _prevInt = int.MinValue;
        private float _prevFloat = float.MinValue;

        // protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        // {
        //     EditorGUI.BeginChangeCheck();
        //     return (true, position);
        // }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                float curValue = property.floatValue;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (curValue == _prevFloat)
                {
                    return true;
                }
                MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
                float valueLimit;
                if (minValueAttribute.ValueCallback == null)
                {
                    valueLimit = minValueAttribute.Value;
                }
                else
                {
                    (float getValueLimit, string getError) = Util.GetCallbackFloat(property, minValueAttribute.ValueCallback);
                    valueLimit = getValueLimit;
                    _error = getError;
                }

                if (_error != "")
                {
                    return true;
                }

                if (valueLimit > curValue)
                {
                    property.floatValue = curValue = valueLimit;
                }

                _prevFloat = curValue;
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int curValue = property.intValue;
                if (curValue == _prevInt)
                {
                    return true;
                }
                MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
                float valueLimit;
                if (minValueAttribute.ValueCallback == null)
                {
                    valueLimit = minValueAttribute.Value;
                }
                else
                {
                    (float getValueLimit, string getError) = Util.GetCallbackFloat(property, minValueAttribute.ValueCallback);
                    valueLimit = getValueLimit;
                    _error = getError;
                }

                if (_error != "")
                {
                    return true;
                }

                if (valueLimit > curValue)
                {
                    property.intValue = curValue = (int)valueLimit;
                }

                _prevInt = curValue;
            }
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}