using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private TMP_Text moneyText;

    [SerializeField] private Task currentTask;
    private float secondsLeft;
    private bool taskIsActive;

    [SerializeField] private Inventory inventory;

    void Start()
    {
        SetNewTask(currentTask);
    } 

    public void SetNewTask(Task newTask){
        currentTask = newTask;
        secondsLeft = currentTask.secondsToComplete;
        taskIsActive = true;
        timerText.gameObject.SetActive(true);
        UpdateMoney();
    }

    public void CompleteCurrentTask(){
        taskIsActive = false;
        timerText.gameObject.SetActive(false);
        UpdateMoney();
    }

    public void UpdateMoney(){
        int currentMoney = inventory.GetMoney();
        if (currentTask.mTaskType == Task.taskType.getMoney){
            string moneyColor = currentMoney >= currentTask.targetMoney ? "#57FF2F" : "#FF7B60";
            moneyText.text = $"<color={moneyColor}>{currentMoney}</color> / {currentTask.targetMoney}";
        }
        else{
            moneyText.text = currentMoney + "";
        }
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
