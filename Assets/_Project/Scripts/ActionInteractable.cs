using UnityEngine;
using UnityEngine.Events;

public class ActionInteractable : IInteractable
{
    [SerializeField] private UnityEvent<Interaction> action;
    
    public override void GetUsed(Interaction player) => action.Invoke(player);
}