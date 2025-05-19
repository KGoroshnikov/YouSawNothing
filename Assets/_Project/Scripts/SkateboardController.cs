using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkateboardController : MonoBehaviour
{
    public static event Action OnStolenWehicle;
    [SerializeField] private bool isIllegalToRide;
    
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationRate;
    [SerializeField] private float stoppingRate;
    [SerializeField] private float baseTurnSpeed;
    [SerializeField] private float turnSensitivityFactor;

    [SerializeField] private float maxFOV;

    [SerializeField] private bool isJumpActive;
    [SerializeField] private float jumpForce;

    [SerializeField] private float maxTiltAngle;
    [SerializeField] private float tiltSpeed;

    [SerializeField] private float forcePushNPC;

    private PlayerController playerController;
    private EscManager escManager;
    [SerializeField] private Rigidbody rb;
    private PlayerInput playerInput;
    [SerializeField] private Transform skateMain;
    [SerializeField] private Transform playerPos;
    [SerializeField] private Animator animator;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField] private bool isBicycle;

    private Vector2 moveInput;
    private float currentSpeed;
    private float currentTilt;

    private bool isGrounded;

    private bool imEnabled;

    private bool doNotRegisterPlayer;
    private bool windIsOn;
    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        escManager = GameObject.Find("EscManager").GetComponent<EscManager>();

        playerInput = GameObject.Find("PlayerInput").GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        jumpAction.performed += _ => HandleJump();

        moveAction.Enable();
        jumpAction.Enable();

        if (isBicycle) animator.SetTrigger("BRide");
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    void Update()
    {
        if (!imEnabled) return;
        moveInput = moveAction.ReadValue<Vector2>();
        HandleTilt();
        CheckGround();
    }

    void HandleJump()
    {
        if (!imEnabled || !isJumpActive) return;
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    void FixedUpdate()
    {
        if (moveInput.y > 0f)
        {
            currentSpeed += accelerationRate * Time.fixedDeltaTime;
        }
        else if (moveInput.y < 0f)
        {
            currentSpeed -= accelerationRate * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, stoppingRate * Time.fixedDeltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        Vector3 targetVelocity = transform.forward * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;

        float turnAmount = moveInput.x * baseTurnSpeed * Time.fixedDeltaTime;
        float sensitivity = 1f / (1f + currentSpeed * turnSensitivityFactor);
        float turnAngle = turnAmount * sensitivity;

        Quaternion turnOffset = Quaternion.Euler(0f, turnAngle, 0f);
        //rb.MoveRotation(rb.rotation * turnOffset);

        float yaw = transform.eulerAngles.y + turnAngle;
        Quaternion targetRotation = Quaternion.Euler(0f, yaw, 0f);
        rb.rotation = targetRotation;
        rb.angularVelocity = Vector3.zero;

        if (isBicycle)
        {
            float tspeed = Mathf.Abs(currentSpeed) / maxSpeed;
            animator.speed = tspeed;
        }
        
        if (imEnabled)
        {
            playerController.SetAdditiveFOV(maxFOV * (Mathf.Abs(currentSpeed) / maxSpeed));
            if ((Mathf.Abs(currentSpeed) / maxSpeed) >= 0.5f && !windIsOn)
            {
                playerController.SetWind(true);
                windIsOn = true;
            }
            else if ((Mathf.Abs(currentSpeed) / maxSpeed) < 0.5f && windIsOn)
            {
                playerController.SetWind(false);
                windIsOn = false;
            }
        }
    }

    void HandleTilt()
    {
        float targetTilt = -moveInput.x * maxTiltAngle;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        skateMain.localRotation = Quaternion.Euler(0f, 0, currentTilt);
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f);
    }

    public void PlayerEntered(){
        if (doNotRegisterPlayer || playerController.isPlayerOnWehicle()) return;
        if (isIllegalToRide)
            OnStolenWehicle.Invoke();
        imEnabled = true;
        playerController.SetWehicle();
        playerController.transform.SetParent(playerPos);
        playerController.transform.localEulerAngles = Vector3.zero;
        playerController.transform.localPosition = Vector3.zero;
        playerController.SetAdditiveFOV(0);
        playerController.GetTips().SetWehicleTip(true);
        playerController.SetWind(false);
        escManager.AddWeight(0, gameObject, PlayerExited);
    }

    void PlayerExited(){
        doNotRegisterPlayer = true;
        escManager.RemoveWeight(0, gameObject);
        playerController.GetTips().SetWehicleTip(false);
        playerController.SetAdditiveFOV(0);
        playerController.transform.SetParent(null);
        playerController.LeaveWehicle();
        playerController.SetWind(false);
        playerController.transform.localEulerAngles = new Vector3(0, playerController.transform.localEulerAngles.y, 0);
        imEnabled = false;
        moveInput = Vector2.zero;
        Invoke("ResetSkate", 1);
    }

    void ResetSkate(){
        doNotRegisterPlayer = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("NPC")){
            if (collision.gameObject.TryGetComponent<NPC>(out NPC bone)){
                Vector3 dir = collision.transform.position - new Vector3(transform.position.x, collision.transform.position.y, transform.position.z);
                bone.EnableRagdoll(rb.linearVelocity.magnitude * dir * forcePushNPC);
            }
            //collision.gameObject.GetComponent<NPC>().EnableRagdoll();
        }
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal.normalized;
            Vector3 velocity = rb.linearVelocity;

            float velocityIntoSurface = Vector3.Dot(velocity, -normal);
            if (velocityIntoSurface > 0f)
            {
                Vector3 reductionVector = normal * velocityIntoSurface;
                Vector3 newVelocity = velocity + reductionVector;
                rb.linearVelocity = newVelocity;

                float speedAlongForward = Vector3.Dot(newVelocity, transform.forward);
                currentSpeed = speedAlongForward;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        //OnCollisionEnter(collision);
    }
}
