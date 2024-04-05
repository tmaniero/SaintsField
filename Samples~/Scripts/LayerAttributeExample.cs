﻿using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class LayerAttributeExample: MonoBehaviour
    {
        [Layer][RichLabel("<icon=star.png /><label />")] public string layerString;
        [Layer] public int layerInt;

        public LayerMask myLayerMask;
    }
}
