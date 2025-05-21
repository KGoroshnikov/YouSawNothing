using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts.DialogueGraph;
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
    private List<NPC> targetsToKill = new List<NPC>();
    private List<int> taskIdKill = new List<int>();
    public delegate void DeathCallback(NPC npc);
    public DeathCallback onDeathNPC;

    [Header("Paint")]
    [SerializeField] private PaintHolder[] paintHolders;
    private int targetPainted;
    private int currentPainted;
    private List<int> taskIdPaint = new List<int>();

    [Header("Steal")]
    [SerializeField] private DeliverTrigger mDeliverTrigger;
    [System.Serializable]
    public class targetToSteal
    {
        public GameObject target;
        public GameObject destroyThis;
    }
    [SerializeField] private List<targetToSteal> stealTargets;
    private List<targetToSteal> currentTargetsToSteal = new List<targetToSteal>();
    private List<int> taskIdSteal = new List<int>();

    private bool[] taskCompleted;
    
    [Header("Storyline")]
    [SerializeField] private StorylinePlayer palyer;
    [SerializeField] private string startRoot = "start";
    [SerializeField] private string endRoot = "saw_nothing";

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
        palyer?.QueueStoryline(startRoot);
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

        taskIdKill.Clear();
        for (int i = 0; i < targetsToKill.Count; i++)
            if (targetsToKill != null) targetsToKill[i].SetTragetArrow(false);
        targetsToKill.Clear();

        for (int i = 0; i < paintHolders.Length; i++)
            paintHolders[i].GetPainted(false);
        taskIdPaint.Clear();
        currentPainted = 0;
        targetPainted = 0;

        for (int i = 0; i < currentTargetsToSteal.Count; i++)
            if (currentTargetsToSteal[i].destroyThis != null) Destroy(currentTargetsToSteal[i].destroyThis);
        currentTargetsToSteal.Clear();
        taskIdSteal.Clear();
        mDeliverTrigger.gameObject.SetActive(false);

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
                ChooseTargetToKill(i);
            }
            else if (currentTasks[i].mTaskType == Task.taskType.paint && currentTasks[i].targetPaint > targetPainted)
            {
                targetPainted = currentTasks[i].targetPaint;
                taskIdPaint.Add(i);
            }
            else if (currentTasks[i].mTaskType == Task.taskType.steal)
            {
                ChooseTargetToSteal(i, currentTasks[i].stealIdItem);
            }

        }

        SetDeliversTrigger();

        tablet.SetTasks(currentTasks);

        CheckStealTasks();
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
        palyer?.QueueStoryline(endRoot);
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
        bool ok = false;
        for (int i = 0; i < objectsToDeliver.Count; i++)
        {
            if (objectsToDeliver[i] == obj)
            {
                ok = true;
                delivered[i] = state;
                UpdateStateTask(taskIdDeliver[i], delivered[i]);
                break;
            }
        }
        if (ok) return;

        for (int i = 0; i < currentTargetsToSteal.Count; i++)
        {
            if (currentTargetsToSteal[i].target == obj)
            {
                UpdateStateTask(taskIdSteal[i], state);
                break;
            }
        }
    }

    void ChooseTargetToKill(int taskId)
    {
        NPC rand = null;
        for (int i = 0; i < 100; i++)
        {
            rand = spawnNPC.GetRandomNPC();
            if (!targetsToKill.Contains(rand) && !rand.GetIsDead())
            {
                onDeathNPC = NPCKilled;
                rand.SetTragetArrow(true);
                rand.SubscribeToDeath(onDeathNPC);
                targetsToKill.Add(rand);
                taskIdKill.Add(taskId);
                break;
            }
        }
    }

    public void NPCKilled(NPC npc)
    {
        for (int i = 0; i < targetsToKill.Count; i++)
        {
            if (targetsToKill[i] == npc)
            {
                npc.SetTragetArrow(false);
                UpdateStateTask(taskIdKill[i], true);
                break;
            }
        }
    }

    public void UpdatePainted()
    {
        currentPainted = 0;
        for (int i = 0; i < paintHolders.Length; i++)
            currentPainted += paintHolders[i].IsPainted() ? 1 : 0;

        if (currentPainted >= targetPainted)
        {
            for (int i = 0; i < taskIdPaint.Count; i++)
                UpdateStateTask(taskIdPaint[i], true);
        }
    }

    void CheckStealTasks()
    {
        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].mTaskType == Task.taskType.steal && !taskIdSteal.Contains(i))
            {
                UpdateStateTask(i, true);
            }
        }
    }

    void ChooseTargetToSteal(int taskId, int idSteal)
    {
        if (idSteal >= stealTargets.Count || stealTargets[idSteal].destroyThis == null)
        {
            return;
        }
        currentTargetsToSteal.Add(stealTargets[idSteal]);
        taskIdSteal.Add(taskId);
        stealTargets[idSteal] = null;

        if (!mDeliverTrigger.gameObject.activeSelf)
        {
            mDeliverTrigger.gameObject.SetActive(true);
            mDeliverTrigger.InitMe(currentTargetsToSteal[currentTargetsToSteal.Count - 1].target, this);
        }
        else mDeliverTrigger.AddNewTarget(currentTargetsToSteal[currentTargetsToSteal.Count - 1].target);
    }
}
