using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SearchMinigame : MonoBehaviour
{
    [SerializeField] private Transform hand;
    [SerializeField] private float handSpeed;
    [SerializeField] private int passes;
    [SerializeField] private Animator handAnim;
    [SerializeField] private Animator searchAnim;

    [SerializeField] private Transform[] slots;

    [SerializeField] private Image timerImage;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerInput playerInput;
    private InputAction spaceAction;

    [SerializeField] private GameObject searchUI;

    [System.Serializable]
    public class slot
    {
        public Image bg;
        public Image sprite;
    }
    [SerializeField] private List<slot> slotsImages;
    [SerializeField] private Color colorBGNormal, colorBGSussy;
    [SerializeField] private Color colorSpriteNormal, colorSpriteSussy;

    private int targetIndex;
    private int currentIndex;
    private int direction = 1;
    private int passesDone;
    private int nextIndex;
    private bool isActive;
    private Vector3 targetPos;

    private float totalTime;
    private float timer;

    private List<Inventory.itemData> currentItems;

    private Police policeChecker;

    void Awake()
    {
        spaceAction = playerInput.actions["Jump"];
        spaceAction.performed += ctx => SpacePessed();   
    }

    void OnEnable()
    {
        spaceAction.Enable();
    }

    void OnDisable()
    {
        spaceAction.Disable();
    }

    void Start()
    {
        //StartMinigame();
    }

    void SpacePessed()
    {
        if (!isActive) return;

        Vector3 prevPos = slots[currentIndex].localPosition;
        prevPos.y = hand.localPosition.y;
        if (Vector3.Distance(hand.localPosition, targetPos) < Vector3.Distance(hand.localPosition, prevPos))
            CheckSlot(nextIndex);
        else
            CheckSlot(currentIndex);
    }

    void SetupSlots()
    {
        currentItems = inventory.GetCurrentItems();

        for (int i = 0; i < slotsImages.Count; i++)
        {
            if (currentItems[i].id != 0)
            {
                slotsImages[i].sprite.gameObject.SetActive(true);
                slotsImages[i].sprite.sprite = currentItems[i].uiSprite;
            }
            else slotsImages[i].sprite.gameObject.SetActive(false);

            slotsImages[i].bg.color = currentItems[i].isSussyItem ? colorBGSussy : colorBGNormal;
            slotsImages[i].sprite.color = currentItems[i].isSussyItem ? colorSpriteSussy : colorSpriteNormal;
        }
    }

    public void StartMinigame(Police _policeChecker)
    {
        SetupSlots();
        policeChecker = _policeChecker;
        searchUI.SetActive(true);
        searchAnim.SetTrigger("Show");
        playerController.SetWehicle();
        playerController.SetLockLook(true);
        Invoke("StartGame", 1f);
    }

    void StartGame()
    {
        StartSearch(0);
    }

    void StartSearch(int _targetIndex)
    {
        currentIndex = _targetIndex;
        direction = currentIndex >= slots.Length - 1 ? -1 : 1;
        passesDone = 0;
        targetIndex = _targetIndex;
        isActive = true;

        Vector3 startPos = slots[currentIndex].localPosition;
        startPos.y = hand.localPosition.y;
        hand.localPosition = startPos;
        targetPos = slots[currentIndex + direction].localPosition;
        targetPos.y = hand.localPosition.y;

        float slotSpacing = Vector3.Distance(slots[0].localPosition, slots[1].localPosition);
        int firstPassSegments = slots.Length - (currentIndex + 1);
        if (currentIndex >= slots.Length - 1) firstPassSegments = slots.Length - 1;
        int lastPassSegments = firstPassSegments;
        if (passes % 2 == 0) lastPassSegments = slots.Length - firstPassSegments - 1;
        int segmentsPerPass = slots.Length - 1;
        int totalSegments = (passes - 1) * segmentsPerPass + firstPassSegments + lastPassSegments;
        totalTime = totalSegments * (slotSpacing / handSpeed); // пиздец
        timer = totalTime;
        timerImage.fillAmount = 1f;
    }

    void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        float fill = Mathf.Clamp01(timer / totalTime);
        timerImage.fillAmount = fill;

        hand.localPosition = Vector3.MoveTowards(hand.localPosition, targetPos, handSpeed * Time.deltaTime);

        if (Vector3.Distance(hand.localPosition, targetPos) < 0.1f)
        {
            currentIndex += direction;
            nextIndex = currentIndex + direction;

            if (passesDone >= passes && currentIndex == targetIndex)
            {
                CheckSlot(currentIndex);
                return;
            }
            else if (currentIndex <= 0 || currentIndex >= slots.Length - 1)
            {
                direction *= -1;
                passesDone++;
                nextIndex = currentIndex + direction;
            }
            targetPos = slots[nextIndex].localPosition;
            targetPos.y = hand.localPosition.y;
        }
    }

    void CheckSlot(int slotIndex)
    {
        Vector3 pos = slots[slotIndex].localPosition;
        pos.y = hand.localPosition.y;
        hand.localPosition = pos;

        isActive = false;
        handAnim.SetTrigger("Check");

        if (currentItems[slotIndex].isSussyItem)
        {
            handAnim.SetTrigger("Check");
            Invoke("PlayerCaught", 1.5f);
            return;
        }

        Invoke("ContinueSearch", 2f);
    }

    void PlayerCaught()
    {
        searchAnim.SetTrigger("Hide");
        playerController.LeaveWehicle();
        playerController.SetLockLook(false);
        policeChecker.FinishCheck(true);
    }

    void ContinueSearch()
    {
        targetIndex++;
        if (targetIndex >= slots.Length)
        {
            searchAnim.SetTrigger("Hide");
            playerController.LeaveWehicle();
            playerController.SetLockLook(false);
            policeChecker.FinishCheck(false);
            return;
        }
        StartSearch(targetIndex);
    }
}
