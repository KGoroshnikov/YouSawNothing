using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Tasks", order = 1)]
public class Task : ScriptableObject
{
    public enum taskType
    {
        deliver, kill, getMoney, getBack, paint, steal, sell
    }
    [Header("General")]
    public taskType mTaskType;
    public string taskName;
    public float nameFontSize = 0.19f;
    public string taskDescription;
    public float descriptionFontSize = 0.1f;
    
    [Header("Earn Money")]
    public int targetMoney;

    [Header("Deliver")]
    public GameObject[] possibleObjectsToDeliver;

    [Header("Kill")]
    public int killTarget = -1;
    public int activateTeleport = -1;
    
    [Header("Paint")]
    public int targetPaint;

    [Header("Steal")]
    public int stealIdItem; // IN ARRAY IN TASK MANAGER
}