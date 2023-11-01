﻿using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Utils
{
    public static class HelpBox
    {
        public static float GetHeight(string content, float width, MessageType messageType)
        {
            float basicHeight = GUI.skin.box.CalcHeight(new GUIContent(content), width);
            return messageType == MessageType.None
                ? basicHeight
                : Mathf.Max(EditorGUIUtility.singleLineHeight * 2.0f, basicHeight);
        }

        public static Rect Draw(Rect position, string content, MessageType messageType)
        {
            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, GetHeight(content, position.width, messageType));
            EditorGUI.HelpBox(curRect, content, messageType);
            return leftRect;
        }
    }
}
