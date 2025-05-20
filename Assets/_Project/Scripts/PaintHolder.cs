using UnityEngine;

public class PaintHolder : MonoBehaviour
{
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
    }

    public bool IsPainted(){
        return painted;
    }
}
