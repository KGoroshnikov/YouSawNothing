using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscManager : MonoBehaviour
{
    [SerializeField] private InputActionReference escButton;
    private Action<InputAction.CallbackContext> performedDelegate;

    public class escListener{
        public int weight;
        public GameObject obj;
        public Func.CallbackFunc callbackFunc;
    }
    private List<escListener> weightsList = new List<escListener>();

    void Awake()
    {
        performedDelegate = _ => EscPressed();
    }

    void OnEnable()
    {
        escButton.action.performed += performedDelegate;
    }
    void OnDisable()
    {
        escButton.action.performed -= performedDelegate;
    }

    public void AddWeight(int newWeight, GameObject obj, Func.CallbackFunc callback){
        escListener newListener = new escListener();
        newListener.weight = newWeight;
        newListener.obj = obj;
        newListener.callbackFunc = callback;
        weightsList.Add(newListener);
    }
    public void RemoveWeight(int newWeight, GameObject obj){
        for(int i = 0; i < weightsList.Count; i++){
            if (weightsList[i].weight == newWeight && weightsList[i].obj == obj){
                weightsList.RemoveAt(i);
                break;
            }
        }
    }

    public void ClearAll(){
        for(int i = 0; i < weightsList.Count; i++){
            if (weightsList[i].weight == 999) continue;
            weightsList.RemoveAt(i);
            i--;
        }
    }

    void EscPressed(){
        if (weightsList.Count == 0) return;
        escListener lowestWeight = weightsList[0];
        for(int i = 0; i < weightsList.Count; i++){
            if (weightsList[i].weight < lowestWeight.weight) lowestWeight = weightsList[i];
        }
        lowestWeight.callbackFunc.Invoke();
    }
}
