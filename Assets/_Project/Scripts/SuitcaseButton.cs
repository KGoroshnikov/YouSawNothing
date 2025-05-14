using UnityEngine;

public class SuitcaseButton : IInteractable
{
    [SerializeField] private int statId;
    [SerializeField] private Suitcase suitcase;
    [SerializeField] private bool imItem;
    [SerializeField] private int itemPrice;

    public override bool CanInteractWithMe(Interaction player)
    {
        if (!imItem) return suitcase.CanUpgrade(statId);
        return suitcase.CanBuy(statId, itemPrice);
    }

    public override void GetUsed(Interaction player)
    {
        if (!imItem) suitcase.Upgrade(statId);
        else{
            suitcase.BuyItem(itemPrice, statId);
        }
    }
}
