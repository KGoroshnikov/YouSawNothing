using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 0 - nothing
// 1 - test item

public class Inventory : MonoBehaviour
{
    private MoveObjects moveObjects;
    [SerializeField] private Animator[] slotAnims;
    [SerializeField] private Image[] spriteSlots;

    [SerializeField] private float pickTime;
    [SerializeField] private float throwForce;
    [SerializeField] private Transform[] itemPoses;
    
    [System.Serializable]
    public class itemData
    {
        public int id;
        [HideInInspector] public GameObject obj;
        [HideInInspector] public Item item;
        public Sprite uiSprite;
    }
    private List<itemData> currentItems = new List<itemData>();

    private int currentSelected;

    [SerializeField] private Tips tips;
    [SerializeField] private PlayerInput playerInput;
    private InputAction scrollAction, dropAction;
    [SerializeField] private float scrollCooldown = 0.25f;
    private float lastScrollTime = 0f;

    void Awake()
    {
        scrollAction = playerInput.actions["Scroll"];
        scrollAction.performed += ctx => OnScroll(ctx);
        dropAction = playerInput.actions["Drop"];
        dropAction.performed += _ => OnDrop();
    }

    void Start()
    {
        moveObjects = GameObject.Find("MoveObjects").GetComponent<MoveObjects>();
        for(int i = 0; i < 4; i++){
            currentItems.Add(new itemData());
        }

        currentSelected = 0;
        slotAnims[currentSelected].SetTrigger("Select");
    }
    void OnEnable()
    {
        scrollAction.Enable();
        dropAction.Enable();
    }

    void OnDisable()
    {
        scrollAction.Disable();
        dropAction.Disable();
    }

    void OnDrop(){
        if (currentItems[currentSelected].id == 0) return;

        currentItems[currentSelected].obj.transform.SetParent(null);
        Vector3 vel = throwForce * Camera.main.transform.forward;
        currentItems[currentSelected].item.ThrowMe(vel, transform.position);

        currentItems[currentSelected].id = 0;
        currentItems[currentSelected].obj = null;
        currentItems[currentSelected].item = null;
        spriteSlots[currentSelected].gameObject.SetActive(false);
        tips.DisableMainHand();
        tips.SetDropTip(false);
    }

    public bool AddItem(itemData item){
        int freeSlot = -1;
        for(int i = 0; i < currentItems.Count; i++){
            if (currentItems[i].id == 0){
                freeSlot = i;
                break;
            }
        }
        if (freeSlot == -1) return false;

        currentItems[freeSlot].id = item.id;
        currentItems[freeSlot].obj = item.obj;
        currentItems[freeSlot].item = item.item;

        spriteSlots[freeSlot].gameObject.SetActive(true);
        spriteSlots[freeSlot].sprite = item.uiSprite;

        item.obj.transform.SetParent(itemPoses[item.id]);

        if (currentSelected == freeSlot)
        {
            tips.EnableMainHand(currentItems[currentSelected].obj.transform, Vector2.zero);
            tips.SetDropTip(true);
        }

        moveObjects.AddObjectToMove(item.obj, itemPoses[item.id].position, itemPoses[item.id].rotation, pickTime, ItemPicked);

        return true;
    }

    void OnScroll(InputAction.CallbackContext ctx)
    {
        float scrollValue = ctx.ReadValue<float>();
        if (Mathf.Approximately(scrollValue, 0f))
            return;

        if (Time.time - lastScrollTime < scrollCooldown)
            return;
        lastScrollTime = Time.time;

        slotAnims[currentSelected].SetTrigger("Leave");
        if (currentItems[currentSelected].id != 0)
            currentItems[currentSelected].obj.SetActive(false);
        if (scrollValue > 0f)
            currentSelected = (currentSelected + 1) % 4;
        else
            currentSelected = (currentSelected - 1 + 4) % 4;
        slotAnims[currentSelected].SetTrigger("Select");
        if (currentItems[currentSelected].id != 0)
        {
            currentItems[currentSelected].obj.SetActive(true);
            tips.EnableMainHand(currentItems[currentSelected].obj.transform, Vector2.zero);
            tips.SetDropTip(true);
        }
        else{
            tips.DisableMainHand();
            tips.SetDropTip(false);
        }
    }

    void ItemPicked(){
        for(int i = 0; i < currentItems.Count; i++){
            if (currentItems[i].id == 0) continue;
            if (currentSelected != i) currentItems[i].obj.SetActive(false);
            currentItems[i].obj.transform.localPosition = Vector3.zero;
            currentItems[i].obj.transform.localEulerAngles = Vector3.zero;
        }
    }
}
