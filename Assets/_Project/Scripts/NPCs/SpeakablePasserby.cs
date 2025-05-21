using Plugins.DialogueSystem.Scripts.DialogueGraph;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class SpeakablePasserby : NPC
{
    [Header("Text Player")]
    [SerializeField] private StorylinePlayer player;
    [SerializeField] private string[] rootVariants;
    [SerializeField] private TaskManager taskManager;
    [Header("Tasks")]
    [SerializeField] private string[] sellRoots;
    [SerializeField] private int trustThreshold = 10;
    [SerializeField] private int trust;
    [SerializeField] private int trustGain = 2;
    [SerializeField] private int trustLoss = 1;
    
    [SerializeField] private int trustDecreaseAfterSell = 3;
    [SerializeField] private UnityEvent onWalkerThrust;
    [SerializeField] private UnityEvent onWalkerNotThrust;
    [SerializeField] private UnityEvent onWalkerIncreaseThrust;
    [SerializeField] private UnityEvent onWalkerDecreaseThrust;

    public void Speak()
    {
        if (player == null) return;
        if (player.IsPlaying) return;
        player.StartStorylineNow(taskManager.CanSell
            ? sellRoots[Random.Range(0, sellRoots.Length)]
            : rootVariants[Random.Range(0, rootVariants.Length)]
        );
    }
    public void IncreaseTrust()
    {
        trust += trustGain;
        onWalkerIncreaseThrust.Invoke();
        if (trust == trustThreshold) 
            onWalkerThrust.Invoke();
    }

    public void DecreaseTrust()
    {
        trust -= trustLoss;
        onWalkerDecreaseThrust.Invoke();
        if (!IsWalkerThrustYou) 
            onWalkerNotThrust.Invoke();
    }

    public bool TrySell()
    {
        if (IsWalkerThrustYou)
            taskManager.Sell();
        trust -= trustDecreaseAfterSell;
        return IsWalkerThrustYou;
    }

    public bool IsWalkerThrustYou => trust >= trustThreshold;
}