using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Tasks", order = 1)]
public class Task : ScriptableObject
{
    public enum taskType
    {
        deliver, kill, getMoney
    }
    [Header("General")]
    public taskType mTaskType;
    public string taskName;
    public float nameFontSize = 0.19f;
    public string taskDescription;
    public float descriptionFontSize = 0.1f;
    public string dialogueRoot;
    
    [Header("Earn Money")]
    public int targetMoney;

    [Header("Deliver")]
    public GameObject[] possibleObjectsToDeliver;
}