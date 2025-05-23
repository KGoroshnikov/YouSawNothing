using System;
using Plugins.DialogueSystem.Scripts;
using UnityEngine;

public class TradingVariantSelector : MonoBehaviour
{
    [SerializeField] private string invokeTag = "";
    public VariantContainer container;
    public PlayerStats stats;
    public StorylinePlayer storylinePlayer;
    [SerializeField] private float decisionTime = 5;
    [SerializeField] private NPC npc;
    [SerializeField] private Variant[] variants;
    public ResultType failedResult;
    public string failedRoot;

    public void ShowIfTagMatch(string inTag)
    {
        if (inTag != invokeTag) return;
        Show();
    }

    public virtual void Show()
    {
        for (var i = 0; i < container.VariantCount; i++)
        {
            var i1 = i;
            container.ShowVariant(i, variants[i].text, () =>
            {
                InvokeResult(variants[i1].resultId, variants[i1].branchRoot);
                Hide();
            });
        }
        var time = decisionTime;
        if (stats) time *= stats.Conviction;
        else Debug.LogWarning("No stats found!");
        Invoke(nameof(Failure), time);
    }

    public virtual void Hide()
    {
        CancelInvoke(nameof(Failure));
        for (var i = 0; i < variants.Length; i++)
            container.HideVariant(i);
    }

    private void Failure()
    {
        Hide();
        InvokeResult(failedResult, failedRoot);
    }

    private void InvokeResult(ResultType id, string root)
    {
        storylinePlayer.StartStorylineNow(root);
        switch (id)
        {
            case ResultType.NoSpeech:
                npc.DisableSpeech();
                break;
            case ResultType.CallPolice:
                npc.CallPolice();
                break;
            case ResultType.DecreaseThrust1:
                npc.DecreaseTrust(1);
                break;
            case ResultType.DecreaseThrust2:
                npc.DecreaseTrust(2);
                break;
            case ResultType.IncreaseThrust1:
                npc.IncreaseTrust(1);
                break;
            case ResultType.IncreaseThrust2:
                npc.IncreaseTrust(2);
                break;
            case ResultType.Sold:
                npc.TrySell();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }
    }


    [System.Serializable]
    public class Variant
    {
        public string text;
        public ResultType resultId;
        public string branchRoot;

    }

    public enum ResultType
    {
        NoSpeech,
        CallPolice,
        DecreaseThrust1,
        DecreaseThrust2,
        IncreaseThrust1,
        IncreaseThrust2,
        Sold
    }
}
