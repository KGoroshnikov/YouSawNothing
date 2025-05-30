﻿
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VariantContainer : MonoBehaviour
{
    [SerializeField] private Variant[] variants;
    
    public void ShowVariant(int index, string text, UnityAction onClick)
    {
        variants[index].Button.gameObject.SetActive(true);
        variants[index].Button.interactable = true;
        variants[index].Button.onClick.RemoveAllListeners();
        variants[index].Button.onClick.AddListener(onClick);
        variants[index].Field.text = text;
    }

    public void HideVariant(int index)
    {
        variants[index].Button.interactable = false;
        variants[index].Button.gameObject.SetActive(false);
    }
    public int VariantCount => variants.Length;
    
    public class Variant
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text field;
        
        public Button Button => button;
        public TMP_Text Field => field;
    }
}