using UnityEngine;

public class Item : IInteractable
{
    [Header("Item")]
    [SerializeField] private Inventory.itemData myData;
    private Inventory inventory;
    [SerializeField] private Collider collider;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform itemContainer;

    protected override void Start()
    {
        base.Start();
        myData.obj = gameObject;
        myData.item = this;
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
            if (col.gameObject == gameObject) continue;
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
