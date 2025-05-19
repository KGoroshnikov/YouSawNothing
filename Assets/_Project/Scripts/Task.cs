using UnityEngine;

[CreateAssetMenu(fileName = "Task", menuName = "ScriptableObjects/Tasks", order = 1)]
public class Task : ScriptableObject
{
    public enum taskType
    {
        deliver, kill, getMoney, getBack, paint
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

    [Header("Kill")]
    public GameObject killTarget; // not implemented
    public bool justRandom; // implemented, but this variable doesnt do anything

    [Header("Paint")]
    public int targetPaint;
}