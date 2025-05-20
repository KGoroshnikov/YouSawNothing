using TMPro;
using UnityEngine;

public class Tips : MonoBehaviour
{
    [SerializeField] private GameObject wehicleTip;

    [SerializeField] private GameObject reticleDefault, reticleInteraction;

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRect;

    [SerializeField] private GameObject tipsObj;
    [SerializeField] private TMP_Text itemName, itemAction;

    [SerializeField] private GameObject dropTip;

    [SerializeField] private RectTransform pointerHand, mainHand;
    [SerializeField] private float distanceOverScale;
    private Transform targetPos, mainHandTarget;
    private Vector2 mainHandOffset;
    private bool active;

    public void SetWehicleTip(bool a){
        wehicleTip.SetActive(a);
    }

    public void SetDropTip(bool a){
        dropTip.SetActive(a);
    }

    public void SetInteractionReticle(){
        SetReticle(true);
    }

    public void SetDefaultReticle(){
        SetReticle(false);
    }

    void SetReticle(bool interact){
        reticleDefault.SetActive(!interact);
        reticleInteraction.SetActive(interact);
    }

    public void EnableMe(Transform obj, string name, string action){
        tipsObj.SetActive(true);
        itemName.text = name;
        itemAction.text = action;

        pointerHand.gameObject.SetActive(true);
        targetPos = obj;
        active = true;
    }

    public void EnableMainHand(Transform obj, Vector2 offset){
        mainHand.gameObject.SetActive(true);
        mainHandTarget = obj;
        mainHandOffset = offset;
    }

    public void DisableMainHand(){
        mainHand.gameObject.SetActive(false);
    }

    void Update()
    {
        if (mainHand.gameObject.activeSelf){
            UpdateUI(mainHandTarget.position, mainHandOffset, mainHand);
        }
        if (!active) return;
        if (targetPos == null)
        {
            DisableMe();
            return;
        }
        UpdateUI(targetPos.position, Vector2.zero, pointerHand);
        pointerHand.localScale = Vector3.one * (1 / Vector3.Distance(transform.position, targetPos.position)) * distanceOverScale;
    }

    public void DisableMe(){
        tipsObj.SetActive(false);
        pointerHand.gameObject.SetActive(false);
        active = false;
    }

    public void UpdateUI(Vector3 targetPos, Vector2 offset, RectTransform rectTransform){
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(targetPos);
        bool isVisible = screenPoint.z > 0;
        if (!isVisible) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPosition);
        rectTransform.localPosition = localPosition + offset;
    }
}