﻿using System;
using UnityEngine;

namespace ExtInspector.Standalone
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string DrawerClass => "ExtInspector.Editor.Standalone.MinMaxSliderAttributeDrawer";

        public readonly float Min;
        public readonly float Max;
        public readonly float Step;
        // public readonly bool DataFields = true;
        // public readonly bool FlexibleFields = true;
        public readonly bool Bound = true;
        public readonly bool Round = true;

        public MinMaxSliderAttribute(float min, float max, float step=-1f)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
