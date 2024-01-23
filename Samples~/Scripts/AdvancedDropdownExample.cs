﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class AdvancedDropdownExample: MonoBehaviour
    {
        public int place1;
        public int place2;
        public int place3;
        public int place4;

        [Serializable]
        public struct MyStruct
        {
            [PostFieldButton(nameof(ShowNewValue), "Click")]
            [OnValueChanged(nameof(ShowNewValue))]
            [AboveRichLabel(nameof(dropIt), true)]
            [AdvancedDropdown(nameof(AdvDropdown))] public int dropIt;

            public AdvancedDropdownList<int> AdvDropdown()
            {
                return new AdvancedDropdownList<int>("Days", new List<AdvancedDropdownList<int>>
                {
                    new AdvancedDropdownList<int>("First Half", new List<AdvancedDropdownList<int>>
                    {
                        new AdvancedDropdownList<int>("Monday", 1, icon: "eye.png"),
                        new AdvancedDropdownList<int>("Tuesday", 2),
                    }),
                    new AdvancedDropdownList<int>("Second Half", new List<AdvancedDropdownList<int>>
                    {
                        new AdvancedDropdownList<int>("Wednesday", new List<AdvancedDropdownList<int>>
                        {
                            new AdvancedDropdownList<int>("Morning", 3, icon: "eye.png"),
                            new AdvancedDropdownList<int>("Afternoon", 8),
                        }),
                        new AdvancedDropdownList<int>("Thursday", 4, true, icon: "eye.png"),
                    }),
                    AdvancedDropdownList<int>.Separator(),
                    new AdvancedDropdownList<int>("Friday", 5, true),
                    AdvancedDropdownList<int>.Separator(),
                    new AdvancedDropdownList<int>("Saturday", 6, icon: "eye.png"),
                    new AdvancedDropdownList<int>("Sunday", 7, icon: "eye.png"),
                });
            }

            public void ShowNewValue()
            {
                Debug.Log($"dropIt new value: {dropIt}");
            }
        }

        public MyStruct strTyp;


    }
}
