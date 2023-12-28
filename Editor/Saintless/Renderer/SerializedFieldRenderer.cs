﻿using UnityEditor;

namespace SaintsField.Editor.Saintless.Renderer
{
    public class SerializedFieldRenderer: AbsRenderer
    {
        public SerializedFieldRenderer(UnityEditor.Editor editor, SaintlessFieldWithInfo fieldWithInfo) : base(editor, fieldWithInfo)
        {
        }

        public override void Render()
        {
            SerializedProperty property = serializedObject.FindProperty(fieldWithInfo.fieldInfo.Name);
            EditorGUILayout.PropertyField(property);
        }
    }
}