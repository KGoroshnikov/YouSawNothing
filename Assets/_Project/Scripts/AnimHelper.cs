using UnityEngine;
using UnityEngine.Events;

public class AnimHelper : MonoBehaviour
{
    [SerializeField] private UnityEvent event1;

    [SerializeField] private UnityEvent event2;
    [SerializeField] private UnityEvent event3;
    [SerializeField] private UnityEvent event4;

    public void TriggerEvent1()
    {
        event1.Invoke();
    }

    public void TriggerEventN(int n)
    {
        if (n == 2) event2.Invoke();
        else if (n == 3) event3.Invoke();
        else if (n == 4) event4.Invoke();
    }
}
