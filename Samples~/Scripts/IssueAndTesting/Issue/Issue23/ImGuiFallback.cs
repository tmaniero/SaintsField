using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue23
{
    public class ImGuiFallback : MonoBehaviour
    {
        public bool toggle;

        [Serializable]
        public class Container<T1, T2>
        {
            [Serializable]
            protected sealed class Entry
            {
                public T1 Name;
                public T2 Object;
            }

            [SerializeField]
            protected List<Entry> Entries;
        }

        [Serializable]
        public sealed class ContainerChild<T> : Container<string, T>
        {
            public T GetByName(string name)
            {
                var entry = Entries.Find(e => e.Name == name);
                return entry == default ? default : entry.Object;
            }
        }

        // public ContainerChild<GameObject> normal;
        [HideIf(nameof(toggle)), InfoBox("Type CustomDrawer fallback", above: true)] public ContainerChild<GameObject> withIf;

        // public SaintsArray<string> plain;

        // [ShowIf(nameof(toggle)), InfoBox("Type CustomDrawer fallback", above: true)]
        // public SaintsArray<string> dec;
    }
}