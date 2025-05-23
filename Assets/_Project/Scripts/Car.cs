using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts;
using Plugins.DialogueSystem.Scripts.DialogueGraph;
using UnityEngine;

public class Car : MonoBehaviour
{
    private Transform player;

    [SerializeField] private Transform eyesParent;
    [SerializeField] private Vector3 eyesOffset;

    [SerializeField] private Animator animator;

    [SerializeField] private TaskManager taskManager;

    [SerializeField] private Task getBackTask;

    [SerializeField] private Transform moneySpawn;
    [SerializeField] private GameObject moneyPref;
    [SerializeField] private float throwForce;
    [SerializeField] private Vector2 moneyPerTask;

    [SerializeField] private PoliceSpawner policeSpawner;

    private bool waitingForPlayer;
    private bool dialogueWithPlayer;

    [SerializeField] private AudioSource windowSound;
    [SerializeField] private AudioSource buttonSound;
    [SerializeField] private AudioSource caseSound;

    private int amountOfTasks;
    [Header("Storyline")]
    [SerializeField] private StorylinePlayer palyer;
    [SerializeField] private string startRoot = "start";
    [SerializeField] private string endRoot = "saw_nothing";

    void Start()
    {
        player = Camera.main.transform;
        //amountOfTasks = taskManager.LoadNextTask();

        waitingForPlayer = true;
        PlayerGetClose();
    }

    void Update()
    {
        eyesParent.forward = (player.position + eyesOffset - eyesParent.position).normalized;
    }

    public void PlayerGetClose(){
        animator.SetTrigger("WindowDown");
        windowSound.Play();

        if (waitingForPlayer && !dialogueWithPlayer)
        {
            taskManager.RemoveTasks();
            taskManager.TakeMoneyForTask();
            dialogueWithPlayer = true;
            waitingForPlayer = false;
            Invoke("SpawnMoney", 2);
            palyer?.QueueStoryline(startRoot);
        }
        else if (dialogueWithPlayer)
        {
            taskManager.UpdateStateTask(0, true);
            taskManager.RemoveTasks();
        }
    }

    void SpawnMoney()
    {
        for (int i = 0; i < amountOfTasks; i++)
        {
            Item item = Instantiate(moneyPref, moneySpawn.position, Quaternion.Euler(Vector3.zero)).GetComponent<Item>();
            item.SetMoney((int)Random.Range(moneyPerTask.x, moneyPerTask.y));
            item.ThrowMe(moneySpawn.forward * throwForce, moneySpawn.position);
        }
        animator.SetTrigger("ShowCase");

        if (amountOfTasks == 0) return;
        policeSpawner.SetTasks(amountOfTasks);
    }

    public Item SpawnObject(GameObject pref)
    {
        Item item = Instantiate(pref, moneySpawn.position, Quaternion.Euler(Vector3.zero)).GetComponent<Item>();
        item.ThrowMe(moneySpawn.forward * throwForce, moneySpawn.position);
        return item;
    }

    public void PlayerLeaved()
    {
        animator.SetTrigger("WindowUp");
        windowSound.Play();

        if (dialogueWithPlayer)
        {
            List<Task> taskBack = new List<Task>() { getBackTask };
            taskManager.SetNewTask(taskBack, 5);
            taskManager.SetTime(5);
            taskManager.ForceOpenTablet();
        }
    }

    public void SuitcaseSound()
    {
        caseSound.Play();
    }

    public void HideEverythingAndGetAJob()
    {
        palyer?.QueueStoryline(endRoot);
        dialogueWithPlayer = false;
        waitingForPlayer = false;
        buttonSound.Play();

        animator.SetTrigger("HideCase");
        taskManager.RemoveTasks();
        amountOfTasks = taskManager.LoadNextTask();
        taskManager.ForceOpenTablet();
    }

    public void PlayerCompletedTasks(bool real){
        waitingForPlayer = real;
    }
}
