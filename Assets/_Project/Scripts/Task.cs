using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Tasks", order = 1)]
public class Task : ScriptableObject
{   
    public enum taskType{
        deliver, kill, getMoney
    }
    [Header("General")]
    public taskType mTaskType;
    public string taskName;
    public float nameFontSize;
    public string taskDescription;
    public float descriptionFontSize;
    public string dialogueRoot;
    
    [Header("Earn Money")]
    public int targetMoney;
}