using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float distance;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Inventory inventory;
    [SerializeField] private Tips tips;
    private bool interactionActive = true;
    [SerializeField] private PlayerInput playerInput;
    private InputAction interactionButton;
    private IInteractable interaction;
    private GameObject interactionObj;

    void Awake()
    {
        interactionButton = playerInput.actions["Interact"];

        interactionButton.performed += InteractWithItem;
    }

    void InteractWithItem(InputAction.CallbackContext context){
        if (interaction != null && !interaction.CanInteractWithMe(this)) return; 
        if (interaction == null || !interactionActive) return;
        interaction.GetUsed(this);
    }

    void OnEnable()
    {
        interactionButton.Enable();
    }

    void OnDisable()
    {
        interactionButton.performed -= InteractWithItem;
        interactionButton.Disable();
    }

    void Update(){
        if (!interactionActive){
            return;
        }
        CheckReachable();
    }

    void ClearInteraction(){
        if (interaction == null) return;

        interaction = null;
        interactionObj = null;
        tips.SetDefaultReticle();
        tips.DisableMe();
    }

    public void RefreshUI(){
        if (interaction != null && interaction.NeedTip)
        {
            tips.EnableMe(interactionObj.transform, interaction.I_Name, interaction.I_Action);
        }
        else{
            tips.DisableMe();
        }
    }

    public void CheckReachable(){
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray.origin, ray.direction, out hit, distance, layerMask) && hit.collider.CompareTag("Interactable")) {
            if (interactionObj != hit.collider.gameObject){
                tips.SetInteractionReticle();
                interactionObj = hit.collider.gameObject;
                interaction = interactionObj.GetComponent<IInteractable>();
                RefreshUI();
            }
        }
        else if (interaction != null){
            ClearInteraction();
        }
    }

    public Inventory GetInventory(){
        return inventory;
    }

    public Tips GetTips(){
        return tips;
    }

    public PlayerController GetPlayerController(){
        return playerController;
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(camera.transform.position,camera.transform.forward * distance);
    }
}
