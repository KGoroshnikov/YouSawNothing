using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private Task currentTask;
    private float secondsLeft;
    private bool taskIsActive;

    void Start()
    {
        SetNewTask(currentTask);
    } 

    public void SetNewTask(Task newTask){
        currentTask = newTask;
        secondsLeft = currentTask.secondsToComplete;
        taskIsActive = true;
        timerText.gameObject.SetActive(true);
    }

    public void CompleteCurrentTask(){
        taskIsActive = false;
        timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!taskIsActive) return;

        secondsLeft -= Time.deltaTime;
        if (secondsLeft < 0f) secondsLeft = 0f;

        int minutes = Mathf.FloorToInt(secondsLeft / 60f);
        int seconds = Mathf.FloorToInt(secondsLeft % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
