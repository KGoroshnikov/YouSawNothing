using UnityEngine;
using UnityEngine.Events;

public class TouchTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnter;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) onEnter.Invoke();
    }
}
