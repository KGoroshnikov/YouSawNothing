using System.Collections.Generic;
using UnityEngine;

public class DeliverTrigger : MonoBehaviour
{
    private List<GameObject> targetItems = new List<GameObject>();
    private TaskManager taskManager;
    private List<Collider> _inside = new List<Collider>();

    [SerializeField] private GameObject redArrow;

    public void InitMe(GameObject item, TaskManager tm)
    {
        targetItems.Add(item);
        taskManager = tm;
    }

    public void AddNewTarget(GameObject item)
    {
        targetItems.Add(item);
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

    void CheckArrow()
    {
        if (_inside.Count == targetItems.Count)
        {
            redArrow.SetActive(false);
        }
        else redArrow.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (targetItems.Contains(other.gameObject))
        {
            _inside.Add(other);
            taskManager.SomethingDelivered(other.gameObject, true);
            CheckArrow();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (targetItems.Contains(other.gameObject))
        {
            _inside.Remove(other);
            taskManager.SomethingDelivered(other.gameObject, false);
            CheckArrow();
        }
    }
}
