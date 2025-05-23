using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 0 - nothing
// 1 - test item
// 2 - money
// 3 - paketik
// 4 - baseball
// 5 - gun
// 6 - graple
// 7 - spray paint

public class Inventory : MonoBehaviour
{
    public static event Action OnSussyPicked;
    public static event Action OnSussyInHand;

    private MoveObjects moveObjects;
    [SerializeField] private Animator[] slotAnims;
    [SerializeField] private Image[] spriteSlots;

    [SerializeField] private float pickTime;
    [SerializeField] private float throwForce;
    [SerializeField] private Transform[] itemPoses;

    [SerializeField] private AudioSource pickupAudio, itemChangeAudio;

    [System.Serializable]
    public class itemData
    {
        public int id;
        [HideInInspector] public GameObject obj;
        [HideInInspector] public Item item;
        public Vector2 spriteSize = new Vector2(100, 100);
        public Sprite uiSprite;
        [HideInInspector] public bool stackable;
        public Vector2 offsetHand = Vector2.zero;
        public bool isSussyItem;
    }
    private List<itemData> currentItems = new List<itemData>();

    private int currentSelected;

    [SerializeField] private int moneyAmount;

    [SerializeField] private Tips tips;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TaskManager taskManager;
    private InputAction scrollAction, dropAction;
    [SerializeField] private float scrollCooldown = 0.25f;
    private float lastScrollTime = 0f;

    private Graple graple;
    private SprayPaint sprayPaint;

    void Awake()
    {
        scrollAction = playerInput.actions["Scroll"];
        scrollAction.performed += OnScroll;
        dropAction = playerInput.actions["Drop"];
        dropAction.performed += OnDrop;
    }

    void Start()
    {
        moveObjects = GameObject.Find("MoveObjects").GetComponent<MoveObjects>();
        for (int i = 0; i < 4; i++)
        {
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
        scrollAction.performed -= OnScroll;
        dropAction.performed -= OnDrop;
        scrollAction.Disable();
        dropAction.Disable();
    }

    public List<itemData> GetCurrentItems()
    {
        return currentItems;
    }

    void OnDrop(InputAction.CallbackContext context)
    {
        if (currentItems[currentSelected].id == 0) return;

        currentItems[currentSelected].obj.transform.SetParent(null);
        Vector3 vel = throwForce * Camera.main.transform.forward;
        currentItems[currentSelected].item.ThrowMe(vel, transform.position);

        if (currentItems[currentSelected].id == 2)
        {
            currentItems[currentSelected].item.SetMoney(moneyAmount);
            moneyAmount = 0;
            taskManager.UpdateMoney();
        }

        currentItems[currentSelected].id = 0;
        currentItems[currentSelected].obj = null;
        currentItems[currentSelected].item = null;
        spriteSlots[currentSelected].gameObject.SetActive(false);
        tips.DisableMainHand();
        tips.SetDropTip(false);
    }

    public void DropSlot(int slot, GameObject me)
    {
        if (currentItems[slot].obj != me) return;

        if (currentSelected == slot)
        {
            tips.DisableMainHand();
            tips.SetDropTip(false);
        }

        Destroy(currentItems[slot].obj);
        currentItems[slot].id = 0;
        currentItems[slot].obj = null;
        currentItems[slot].item = null;
        spriteSlots[slot].gameObject.SetActive(false);
    }

    public void RemoveMoney(int amout)
    {
        moneyAmount -= amout;
        taskManager.UpdateMoney();
        if (moneyAmount <= 0)
        {
            int moneySlot = findSlotWithId(2);

            if (currentSelected == moneySlot)
            {
                tips.DisableMainHand();
                tips.SetDropTip(false);
            }

            Destroy(currentItems[moneySlot].obj);
            currentItems[moneySlot].id = 0;
            currentItems[moneySlot].obj = null;
            currentItems[moneySlot].item = null;
            spriteSlots[moneySlot].gameObject.SetActive(false);
        }
    }

    public int findSlotWithId(int id)
    {
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (currentItems[i].id == id)
            {
                return i;
            }
        }
        return -1;
    }

    public int currentHoldingId()
    {
        return currentItems[currentSelected].id;
    }

    public int GetMoney()
    {
        return moneyAmount;
    }

    public Graple GetGraple()
    {
        return graple;
    }
    public SprayPaint GetSprayPaint()
    {
        return sprayPaint;
    }

    public bool AddItem(itemData item)
    {
        int freeSlot = -1;
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (currentItems[i].id == 0 || (item.stackable && currentItems[i].id == item.id))
            {
                freeSlot = i;
                break;
            }
        }
        if (freeSlot == -1) return false;

        if (item.stackable)
        {
            moneyAmount += item.item.GetMoney();
            taskManager.UpdateMoney();
            if (currentItems[freeSlot].id == item.id)
                Destroy(currentItems[freeSlot].obj);
        }

        pickupAudio.Play();

        currentItems[freeSlot].id = item.id;
        currentItems[freeSlot].obj = item.obj;
        currentItems[freeSlot].item = item.item;
        currentItems[freeSlot].isSussyItem = item.isSussyItem;
        currentItems[freeSlot].uiSprite = item.uiSprite;
        currentItems[freeSlot].spriteSize = item.spriteSize;
        currentItems[freeSlot].offsetHand = item.offsetHand;

        if (item.isSussyItem)
        {
            OnSussyPicked.Invoke();
        }

        if (currentItems[freeSlot].id == 6)
            graple = item.obj.GetComponent<Graple>();
        else if (currentItems[freeSlot].id == 7)
            sprayPaint = item.obj.GetComponent<SprayPaint>();

        spriteSlots[freeSlot].gameObject.SetActive(true);
        spriteSlots[freeSlot].sprite = item.uiSprite;
        spriteSlots[freeSlot].rectTransform.sizeDelta = item.spriteSize;

        item.obj.transform.SetParent(itemPoses[item.id]);

        if (currentSelected == freeSlot)
        {
            tips.EnableMainHand(currentItems[currentSelected].obj.transform, currentItems[currentSelected].offsetHand);
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

        itemChangeAudio.Play();

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
            CancelInvoke("CheckItemInHand");
            Invoke("CheckItemInHand", 0.5f);
            tips.EnableMainHand(currentItems[currentSelected].obj.transform, currentItems[currentSelected].offsetHand);
            tips.SetDropTip(true);
        }
        else
        {
            tips.DisableMainHand();
            tips.SetDropTip(false);
        }
    }

    void CheckItemInHand()
    {
        if (currentItems[currentSelected].isSussyItem) OnSussyInHand.Invoke();
    }

    void ItemPicked()
    {
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (currentItems[i].id == 0) continue;
            if (currentSelected != i) currentItems[i].obj.SetActive(false);
            currentItems[i].obj.transform.localPosition = Vector3.zero;
            currentItems[i].obj.transform.localEulerAngles = Vector3.zero;
        }
    }
}
