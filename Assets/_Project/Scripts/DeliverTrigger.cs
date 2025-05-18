using System.Collections.Generic;
using UnityEngine;

public class DeliverTrigger : MonoBehaviour
{
    private GameObject targetItem;
    private TaskManager taskManager;
    private List<Collider> _inside = new List<Collider>();

    public void InitMe(GameObject item, TaskManager tm)
    {
        targetItem = item;
        taskManager = tm;
    }

    void Update()
    {
        if (_inside.Count == 0) return;
        for (int i = 0; i < _inside.Count; i++)
        {
            if (_inside[i] == null)
            {
                _inside.RemoveAt(i);
                i--;
                continue;
            }
            if (!_inside[i].enabled) OnTriggerExit(_inside[i]);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetItem)
        {
            _inside.Add(other);
            taskManager.SomethingDelivered(targetItem, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject);
        if (other.gameObject == targetItem)
        {
            _inside.Remove(other);
            taskManager.SomethingDelivered(targetItem, false);
        }
    }
}
