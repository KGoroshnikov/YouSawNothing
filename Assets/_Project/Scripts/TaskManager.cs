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

    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private Car car;

    [Header("Time")]
    [SerializeField] private Vector2 maxMinTime;
    [SerializeField] private int tasksToMinTime;

    [Header("Deliver")]
    [SerializeField] private DeliverTrigger[] possibleDelivers;
    private List<GameObject> objectsToDeliver = new List<GameObject>();
    private List<bool> delivered = new List<bool>();
    private List<int> taskIdDeliver = new List<int>();

    [Header("Kill")]
    [SerializeField] private SpawnNPC spawnNPC;
    [SerializeField] private List<NPC> specificNPCToKill;
    private List<NPC> targetsToKill = new List<NPC>();
    private List<int> taskIdKill = new List<int>();
    public delegate void DeathCallback(NPC npc);
    public DeathCallback onDeathNPC;

    [Header("Paint")]
    [SerializeField] private PaintHolder[] paintHolders;
    [SerializeField] private GameObject sprayPref;
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

    [Header("Teleports")]
    [SerializeField] private List<TeleportPair> teleportPairs;
    [System.Serializable]
    public class TeleportPair
    {
        public GameObject tp1, tp2;
    }

    private bool[] taskCompleted;

    private int totalCompletedTasks;

    public int LoadNextTask()
    {
        int randAmount = Random.Range(1, 3);
        List<Task> randTasks = new List<Task>();
        for (int i = 0; i < randAmount; i++)
        {
            Task randTask = defaultTasks[Random.Range(0, defaultTasks.Count)];
            for (int att = 0; att < 50; att++)
            {
                if (!randTasks.Contains(randTask))
                    break;
                randTask = defaultTasks[Random.Range(0, defaultTasks.Count)];
            }
            randTasks.Add(randTask);
        }

        float tt = (float)totalCompletedTasks / (float)tasksToMinTime;
        tt = Mathf.Clamp01(tt);
        float time = Mathf.Lerp(maxMinTime.x, maxMinTime.y, tt);

        time += playerStats.Time;

        SetNewTask(randTasks, time);

        totalCompletedTasks++;

        return currentTasks.Count;
    }

    public void ForceOpenTablet()
    {
        tablet.ForceOpenTablet();
    }

    public void SetNewTask(List<Task> newTasks, float time)
    {
        currentTasks = newTasks;
        taskCompleted = new bool[currentTasks.Count];
        secondsLeft = time;
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

        bool taskToPaint = false;

        for (int i = 0; i < teleportPairs.Count; i++)
        {
            teleportPairs[i].tp1.SetActive(false);
            teleportPairs[i].tp2.SetActive(false);
        }

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
                    ChooseTargetToKill(i, currentTasks[i]);
                }
                else if (currentTasks[i].mTaskType == Task.taskType.paint && currentTasks[i].targetPaint >= targetPainted)
                {
                    targetPainted = currentTasks[i].targetPaint;
                    taskIdPaint.Add(i);
                    taskToPaint = true;
                }
                else if (currentTasks[i].mTaskType == Task.taskType.steal)
                {
                    ChooseTargetToSteal(i, currentTasks[i].stealIdItem);
                }

            }

        if (taskToPaint)
            car.SpawnObject(sprayPref);

        SetDeliversTrigger();

        tablet.SetTasks(currentTasks);

        CheckTasks();
    }

    public void SetTime(float newTime)
    {
        secondsLeft = newTime;
    }

    public void UpdateStateTask(int id, bool completed)
    {
        if (!taskIsActive) return;
        Debug.Log("id " + id + " taskCompleted.co " + taskCompleted.Length);
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

    void ChooseTargetToKill(int taskId, Task task)
    {
        if (task.killTarget != -1)
        {
            if (task.killTarget >= specificNPCToKill.Count || specificNPCToKill[task.killTarget].GetIsDead())
            {
                // complete task
                return;
            }

            if (task.activateTeleport != -1)
            {
                teleportPairs[task.activateTeleport].tp1.SetActive(true);
                teleportPairs[task.activateTeleport].tp2.SetActive(true);
            }

            onDeathNPC = NPCKilled;
            specificNPCToKill[task.killTarget].SetTragetArrow(true);
            specificNPCToKill[task.killTarget].SubscribeToDeath(onDeathNPC);
            targetsToKill.Add(specificNPCToKill[task.killTarget]);
            taskIdKill.Add(taskId);
            return;
        }

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

    void CheckTasks()
    {
        for (int i = 0; i < currentTasks.Count; i++)
        {
            if (currentTasks[i].mTaskType == Task.taskType.steal && !taskIdSteal.Contains(i))
            {
                UpdateStateTask(i, true);
            }
            else if (currentTasks[i].mTaskType == Task.taskType.kill && !taskIdKill.Contains(i))
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
