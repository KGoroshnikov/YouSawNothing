using UnityEngine;
using UnityEngine.Events;

public class TouchTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onExit;

    [SerializeField] private float sleepTime;
    private bool sleeping;

    void Start()
    {
        if (sleepTime != 0)
        {
            sleeping = true;
            Invoke("StopSleep", sleepTime);
        }
    }

    void StopSleep() => sleeping = false;

    void OnTriggerEnter(Collider other)
    {
        if (sleeping) return;
        if (other.CompareTag("Player")) onEnter.Invoke();
    }
    void OnTriggerExit(Collider other)
    {
        if (sleeping) return;
        if (other.CompareTag("Player")) onExit.Invoke();
    }
}
