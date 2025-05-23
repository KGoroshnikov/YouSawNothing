using Plugins.DialogueSystem.Scripts.DialogueGraph;
using UnityEngine;
using UnityEngine.Events;

public class QTEVariantSelector : VariantSelector
{
    public PlayerStats stats;
    [SerializeField] private float decisionTime = 10;
    [SerializeField] private UnityEvent onQteFailure;

    public override void Show()
    {
        base.Show();
        var time = decisionTime;
        if (stats) time *= stats.Conviction;
        else Debug.LogWarning("No stats found!");
        Invoke(nameof(Failure), time);
    }

    public override void Hide()
    {
        CancelInvoke(nameof(Failure));
        base.Hide();
    }

    private void Failure()
    {
        Hide();
        onQteFailure.Invoke();
    }
}
