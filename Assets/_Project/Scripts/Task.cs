using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Tasks", order = 1)]
public class Task : ScriptableObject
{   
    public enum taskType{
        deliver, kill, getMoney
    }
    [Header("General")]
    public taskType mTaskType;
    public int secondsToComplete;

    [Header("Earn Money")]
    public int targetMoney;
}