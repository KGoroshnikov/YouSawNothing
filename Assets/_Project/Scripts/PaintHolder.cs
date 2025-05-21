using System;
using UnityEngine;

public class PaintHolder : MonoBehaviour
{
    public static event Action OnPainted;
    private bool painted;
    private TaskManager taskManager;

    void Start()
    {
        taskManager = GameObject.Find("TaskManager").GetComponent<TaskManager>();
    }

    public void GetPainted(bool a)
    {
        painted = a;
        taskManager.UpdatePainted();
        if (painted) OnPainted.Invoke();
    }

    public bool IsPainted(){
        return painted;
    }
}
