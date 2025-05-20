using UnityEngine;

public class Item : IInteractable
{
    [Header("Item")]
    [SerializeField] private Inventory.itemData myData;
    private Inventory inventory;
    [SerializeField] private Collider collider;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform itemContainer;

    [Header("Money")]
    [SerializeField] private bool isMoney;
    [SerializeField] private int amountMoney;

    protected override void Start()
    {
        base.Start();
        myData.obj = gameObject;
        myData.item = this;
        myData.stackable = isMoney;
        if (vfxTip != null) vfxTip.Play();
    }

    public override void GetUsed(Interaction player)
    {
        inventory = player.GetInventory();
        if (inventory.AddItem(myData)){
            if (animator != null){
                animator.enabled = false;
            }
            itemContainer.localPosition = Vector3.zero;
            itemContainer.localEulerAngles = Vector3.zero;
            collider.enabled = false;
            rb.isKinematic = true;
            SetInteracted(1);
            if (vfxTip != null){
                vfxTip.Reinit();
                vfxTip.Stop();
            }
        }
    }

    void OnDestroy()
    {
        if (inventory == null) return;
        int mSlot = inventory.findSlotWithId(myData.id);
        if (mSlot == -1) return;
        inventory.DropSlot(mSlot, myData.obj);
    }

    public int GetMoney()
    {
        return amountMoney;
    }
    
    public void SetMoney(int amount){
        amountMoney = amount;
    }

    public void ThrowMe(Vector3 force, Vector3 player){
        transform.localEulerAngles = Vector3.zero;
        collider.enabled = true;
        rb.useGravity = true;
        rb.isKinematic = false;
        SetInteracted(0);
        if (vfxTip != null) vfxTip.Play();
        int wallLayer = LayerMask.GetMask("Default");
        Collider[] overlaps = Physics.OverlapBox(
            collider.bounds.center,
            collider.bounds.extents,
            transform.rotation,
            wallLayer
        );
        bool wasInWall = false;
        foreach (Collider col in overlaps)
        {
            if (col.gameObject == gameObject || col.isTrigger) continue;
            transform.position = player;
            wasInWall = true;
            break;
        }

        if (!wasInWall) rb.AddForce(force, ForceMode.Impulse);

        if (animator != null){
            animator.enabled = true;
            animator.SetTrigger("Idle");
        }
    }

}
