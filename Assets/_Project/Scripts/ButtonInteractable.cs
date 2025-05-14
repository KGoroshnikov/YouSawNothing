using UnityEngine;
using UnityEngine.Events;

public class ButtonInteractable : IInteractable
{
    [SerializeField] private UnityEvent onClick;

    public override void GetUsed(Interaction player)
    {
        onClick?.Invoke();
    }
}
