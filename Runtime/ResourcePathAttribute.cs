﻿using System;
using System.Linq;

namespace SaintsField
{
    public class ResourcePathAttribute: RequireTypeAttribute
    {
        public override SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public override string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly EStr EStr;
        // ReSharper enable InconsistentNaming

        public Type CompType => RequiredTypes[0];

        public ResourcePathAttribute(EStr eStr, bool freeSign, bool customPicker, Type compType, params Type[] requiredTypes)
            : base(EPick.Assets, freeSign, customPicker, requiredTypes.Prepend(compType).ToArray())
        {
            EStr = eStr;
        }

        public ResourcePathAttribute(bool freeSign, bool customPicker, Type compType, params Type[] requiredTypes)
            : this(EStr.Resource, freeSign, customPicker, compType, requiredTypes)
        {
        }

        public ResourcePathAttribute(bool freeSign, Type compType, params Type[] requiredTypes)
            : this(EStr.Resource, freeSign, true, compType, requiredTypes)
        {
        }

        public ResourcePathAttribute(Type compType, params Type[] requiredTypes)
            : this(EStr.Resource, false, true, compType, requiredTypes)
        {
        }
    }
}