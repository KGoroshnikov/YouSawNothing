using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private TMP_Text moneyText;

    [SerializeField] private List<Task> defaultTasks;

    [SerializeField] private List<Task> currentTasks;
    [SerializeField] private float timeToComplete;

    [SerializeField] private Tablet tablet;

    private float secondsLeft;
    private bool taskIsActive;

    [SerializeField] private Inventory inventory;

    [SerializeField] private Car car;

    private bool[] taskCompleted;

    public int LoadNextTask(){
        SetNewTask(defaultTasks);
        return currentTasks.Count;
    }

    public void ForceOpenTablet(){
        tablet.ForceOpenTablet();
    }

    public void SetNewTask(List<Task> newTasks){
        currentTasks = newTasks;
        taskCompleted = new bool[currentTasks.Count];
        secondsLeft = timeToComplete;
        taskIsActive = true;
        timerText.gameObject.SetActive(true);
        UpdateMoney();

        tablet.SetTasks(currentTasks);
    }

    public void SetTime(float newTime){
        secondsLeft = newTime;
    }

    public void UpdateStateTask(int id, bool completed){
        if (!taskIsActive) return;

        taskCompleted[id] = completed;
        tablet.UpdateStateTask(id, completed);
        bool isAllCompleted = true;
        for(int i = 0; i < taskCompleted.Length; i++){
            if (!taskCompleted[i]){
                isAllCompleted = false;
                break;
            }
        }
        car.PlayerCompletedTasks(isAllCompleted);
    }

    public void RemoveTasks(){
        taskIsActive = false;
        timerText.text = "";
    }

    public void TakeMoneyForTask(){
        int targetMoney = -1;
        int idMoneyTask = -1;
        for(int i = 0; i < currentTasks.Count; i++){
            if (currentTasks[i].mTaskType == Task.taskType.getMoney){
                targetMoney = currentTasks[i].targetMoney;
                idMoneyTask = i;
                break;
            }
        }
        if (targetMoney == -1) return;

        inventory.RemoveMoney(targetMoney);
    }

    public void UpdateMoney(){
        int currentMoney = inventory.GetMoney();
        int targetMoney = -1;
        int idMoneyTask = -1;
        for(int i = 0; i < currentTasks.Count; i++){
            if (currentTasks[i].mTaskType == Task.taskType.getMoney){
                targetMoney = currentTasks[i].targetMoney;
                idMoneyTask = i;
                break;
            }
        }
        if (targetMoney != -1){
            string moneyColor = currentMoney >= targetMoney ? "#57FF2F" : "#FF7B60";
            moneyText.text = $"<color={moneyColor}>{currentMoney}</color> / {targetMoney}";

            if (currentMoney >= targetMoney && !taskCompleted[idMoneyTask]){
                UpdateStateTask(idMoneyTask, true);
            }
            else if (currentMoney < targetMoney && taskCompleted[idMoneyTask]){
                UpdateStateTask(idMoneyTask, false);
            } 
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
