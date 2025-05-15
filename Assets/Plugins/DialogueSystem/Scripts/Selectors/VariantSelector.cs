using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Plugins.DialogueSystem.Scripts.Selectors
{
    public class VariantSelector : MonoBehaviour
    {
        [SerializeField] private string invokeTag = "";
        [SerializeField] private Variant[] variants;
        
        public void ShowIfTagMatch(string inTag) {
            if (inTag != invokeTag) return;
            Show();
        }

        public virtual void Show()
        {
            foreach (var variant in variants) 
                variant.Show(this);
        }

        public virtual void Hide()
        {
            foreach (var variant in variants) 
                variant.Hide();
        }
    }

    [Serializable]
    public class Variant
    {
        [SerializeField] private string text;
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text field;

        public string Text => text;

        public void Show(VariantSelector parent)
        {
            Debug.Log($"Showed variant: {text}");
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(parent.Hide);
            button.onClick.AddListener(onSelected.Invoke);
            field.text = text;
        }

        public void Hide()
        {
            Debug.Log($"Hided variant: {text}");
            button.interactable = false;
            button.gameObject.SetActive(false);
        }
    }
}