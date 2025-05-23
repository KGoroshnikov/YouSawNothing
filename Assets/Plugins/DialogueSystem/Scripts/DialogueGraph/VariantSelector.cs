using System;
using UnityEngine;
using UnityEngine.Events;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph
{
    public class VariantSelector : MonoBehaviour
    {
        [SerializeField] private string invokeTag = "";
        public VariantContainer container;
        [SerializeField] private Variant[] variants;
        
        public void ShowIfTagMatch(string inTag) {
            if (inTag != invokeTag) return;
            Show();
        }

        public virtual void Show()
        {
            for (var i = 0; i < variants.Length; i++)
                if (variants[i].active)
                    container.ShowVariant(i, variants[i].text, 
                        variants[i].onSelected, Hide);
        }

        public virtual void Hide()
        {
            for (var i = 0; i < variants.Length; i++)
                container.HideVariant(i);
        }

        public void EnableVariant(int variant) => variants[variant].active = true;
        public void DisableVariant(int variant) => variants[variant].active = false;
    }

    [Serializable]
    public class Variant
    {
        public bool active = true;
        public string text;
        public UnityEvent onSelected;

    }
}