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

    [Header("Deliver")]
    [SerializeField] private DeliverTrigger[] possibleDelivers;
    private List<GameObject> objectsToDeliver = new List<GameObject>();
    private List<bool> delivered = new List<bool>();
    private List<int> taskIdDeliver = new List<int>();

    [Header("Kill")]
    [SerializeField] private SpawnNPC spawnNPC;
    private List<NPC> targetsToKill;
    public delegate void DeathCallback(NPC npc);

    private bool[] taskCompleted;

    public int LoadNextTask()
    {
        SetNewTask(defaultTasks);
        return currentTasks.Count;
    }

    public void ForceOpenTablet()
    {
        tablet.ForceOpenTablet();
    }

    public void SetNewTask(List<Task> newTasks)
    {
        currentTasks = newTasks;
        taskCompleted = new bool[currentTasks.Count];
        secondsLeft = timeToComplete;
        taskIsActive = true;
        timerText.gameObject.SetActive(true);
        UpdateMoney();

        for (int i = 0; i < objectsToDeliver.Count; i++) Destroy(objectsToDeliver[i]);
        objectsToDeliver.Clear();
        delivered.Clear();
        taskIdDeliver.Clear();

        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].mTaskType == Task.taskType.deliver)
            {
                int rnd = Random.Range(0, currentTasks[i].possibleObjectsToDeliver.Length);
                GameObject obj = car.SpawnObject(currentTasks[i].possibleObjectsToDeliver[rnd]).gameObject;
                objectsToDeliver.Add(obj);
                delivered.Add(false);
                taskIdDeliver.Add(i);
            }
            else if (currentTasks[i].mTaskType == Task.taskType.kill)
            {
                ChooseTargetToKill();
            }
        }

        SetDeliversTrigger();

        tablet.SetTasks(currentTasks);
    }

    public void SetTime(float newTime)
    {
        secondsLeft = newTime;
    }

    public void UpdateStateTask(int id, bool completed)
    {
        if (!taskIsActive) return;

        taskCompleted[id] = completed;
        tablet.UpdateStateTask(id, completed);
        bool isAllCompleted = true;
        for (int i = 0; i < taskCompleted.Length; i++)
        {
            if (!taskCompleted[i])
            {
                isAllCompleted = false;
                break;
            }
        }
        car.PlayerCompletedTasks(isAllCompleted);
    }

    public void RemoveTasks()
    {
        taskIsActive = false;
        timerText.text = "";
    }

    public void TakeMoneyForTask()
    {
        int targetMoney = -1;
        int idMoneyTask = -1;
        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].mTaskType == Task.taskType.getMoney)
            {
                targetMoney = currentTasks[i].targetMoney;
                idMoneyTask = i;
                break;
            }
        }
        if (targetMoney == -1) return;

        inventory.RemoveMoney(targetMoney);
    }

    public void UpdateMoney()
    {
        int currentMoney = inventory.GetMoney();
        int targetMoney = -1;
        int idMoneyTask = -1;
        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].mTaskType == Task.taskType.getMoney)
            {
                targetMoney = currentTasks[i].targetMoney;
                idMoneyTask = i;
                break;
            }
        }
        if (targetMoney != -1)
        {
            string moneyColor = currentMoney >= targetMoney ? "#57FF2F" : "#FF7B60";
            moneyText.text = $"<color={moneyColor}>{currentMoney}</color> / {targetMoney}";

            if (currentMoney >= targetMoney && !taskCompleted[idMoneyTask])
            {
                UpdateStateTask(idMoneyTask, true);
            }
            else if (currentMoney < targetMoney && taskCompleted[idMoneyTask])
            {
                UpdateStateTask(idMoneyTask, false);
            }
        }
        else
        {
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

    void SetDeliversTrigger()
    {
        for (int i = 0; i < possibleDelivers.Length; i++) possibleDelivers[i].gameObject.SetActive(false);

        List<int> usedTriggers = new List<int>();
        for (int i = 0; i < objectsToDeliver.Count; i++)
        {
            int rand = 0;
            for (int att = 0; att < 100; i++)
            {
                rand = Random.Range(0, possibleDelivers.Length);
                if (!usedTriggers.Contains(rand))
                    break;
            }
            usedTriggers.Add(rand);

            possibleDelivers[rand].gameObject.SetActive(true);
            possibleDelivers[rand].InitMe(objectsToDeliver[i], this);
        }
    }

    public void SomethingDelivered(GameObject obj, bool state)
    {
        for (int i = 0; i < objectsToDeliver.Count; i++)
        {
            if (objectsToDeliver[i] == obj)
            {
                delivered[i] = state;
                UpdateStateTask(taskIdDeliver[i], delivered[i]);
                break;
            }
        }
    }

    void ChooseTargetToKill()
    {
        NPC rand = null;
        for (int i = 0; i < 100; i++)
        {
            rand = spawnNPC.GetRandomNPC();
            if (!targetsToKill.Contains(rand))
            {
                //rand.SubscribeToDeath(NPCKilled);
                targetsToKill.Add(rand);
                break;
            }
        }
    }

    public void NPCKilled(NPC npc)
    {
        
    }
}
