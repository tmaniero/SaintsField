﻿using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    // ReSharper disable once InconsistentNaming
    public class DOTweenPlayAttribute: Attribute, IPlayaAttribute, IPlayaMethodAttribute, ISaintsGroup
    {
        // ReSharper disable InconsistentNaming
        public readonly string Label;
        public readonly ETweenStop DOTweenStop;
        // ReSharper enable InconsistentNaming

        public const string DOTweenPlayGroupBy = "__SAINTSFIELD_DOTWEEN_PLAY__";
        public string GroupBy { get; }
        public ELayout Layout => 0;
        public bool GroupAllFieldsUntilNextGroupAttribute { get; }
        public bool ClosedByDefault { get; }

        public DOTweenPlayAttribute(string label = null, ETweenStop stopAction = ETweenStop.Rewind, string groupBy="", bool groupAllFieldsUntilNextGroupAttribute = false, bool closedByDefault = false)
        {
            Label = label;
            DOTweenStop = stopAction;

            GroupBy = string.IsNullOrEmpty(groupBy)? DOTweenPlayGroupBy: $"{groupBy}/{DOTweenPlayGroupBy}";
            GroupAllFieldsUntilNextGroupAttribute = groupAllFieldsUntilNextGroupAttribute;
            ClosedByDefault = closedByDefault;
        }

        public DOTweenPlayAttribute(ETweenStop stopAction, string groupBy=""): this(null, stopAction, groupBy)
        {
        }

        public DOTweenPlayAttribute(string label, string groupBy): this(label, ETweenStop.Rewind, groupBy)
        {
        }
    }
}
