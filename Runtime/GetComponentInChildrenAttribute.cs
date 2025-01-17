﻿using System;
using System.Diagnostics;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class GetComponentInChildrenAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute, IPlayaArraySizeAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy { get; }

        public readonly Type CompType;
        public readonly bool IncludeInactive;

        public GetComponentInChildrenAttribute(bool includeInactive = false, Type compType = null, string groupBy = "")
        {
            CompType = compType;
            IncludeInactive = includeInactive;
            GroupBy = groupBy;
        }
    }
}
