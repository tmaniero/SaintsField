using UnityEngine;
// using NaughtyAttributes;

namespace SaintsField.Samples.Scripts.UIKit
{
    public class UIKitExample : MonoBehaviour
    {
        // [ShowNonSerializedField] public static readonly Color red = Color.red;
        // [ShowNativeProperty] public Color blue => Color.blue;

        public int sth;

        [AboveRichLabel("<color=green>SaintsField")]
        [UIKitPropDec] public string content;

        // [Button]
        // private void EditorButton()
        // {
        //     Debug.Log("EditorButton");
        // }
    }
}