using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float regularFOV;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpStanima;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float standingHeight;
    [SerializeField] private float crouchHeight;
    
    [Header("Look")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxLookAngle;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float maxStamina;
    [SerializeField] private float sprintRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float sprintFOV;
    [SerializeField] private float speedChangeFOV;

    [Header("Push")]
    [SerializeField] private float pushMultiplierMe;
    [SerializeField] private float pushMultiplierNPC;
    [SerializeField] private Vector3 pushVector;
    [SerializeField] private float pushDamping;
    [SerializeField] private float pushSlowMultiplier;

    [Header("UI")]
    [SerializeField] private Image fillSprint;
    [SerializeField] private Animator staminaAnim;
    private bool uiIsOn;

    [Header("Other")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Animator camAnim;
    [SerializeField] private Tips tips;
    [SerializeField] private Camera cam;
    [SerializeField] private VisualEffect windVFX;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction lookAction;
    private InputAction sprintAction;

    private float additiveFOV;

    private Vector3 moveInput;
    private Vector2 lookInput;
    private float cameraPitch;
    private bool isGrounded;
    private bool isCrouching;
    private float currentStamina;

    private enum state{
        idle, walk, run, onWehicle
    }
    [SerializeField] private state mState;

    void Awake()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];

        jumpAction.performed += _ => HandleJump();
        crouchAction.performed += _ => SetCrouch(true);
        crouchAction.canceled += _ => SetCrouch(false);
        sprintAction.performed += _ => SetSprint(true);
        sprintAction.canceled += _ => SetSprint(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
    }

    void Update()
    {
        moveInput = new Vector3(moveAction.ReadValue<Vector2>().x, 0, moveAction.ReadValue<Vector2>().y);
        lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        HandleLook();
        CheckGround();
        HandleStamina();
        HandleFOV();

        
        Debug.DrawRay(transform.position, Vector3.up, Color.blue);
        Debug.DrawRay(transform.position, lastRight, Color.red);
        Debug.DrawRay(transform.position, lastDir.normalized, Color.green);

        Debug.DrawRay(transform.position,  transform.TransformDirection(moveInput), Color.magenta);
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleFOV(){
        if (mState == state.run)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFOV, speedChangeFOV * Time.deltaTime);
        else
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, regularFOV + additiveFOV, speedChangeFOV * Time.deltaTime);
    }

    void HandleMovement()
    {
        if (mState == state.onWehicle) return;

        pushVector *= pushDamping;

        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.z;
        float speed = moveSpeed;

        if (isCrouching)
            speed = crouchSpeed;
        else if (mState == state.run && moveInput.magnitude > 0 && currentStamina > 0)
            speed = sprintSpeed;
        
        if (mState == state.idle && speed == moveSpeed && moveInput.magnitude > 0)
            SetState(state.walk);
        else if (mState != state.idle && moveInput.magnitude == 0)
            SetState(state.idle);

        float pushMag = pushVector.magnitude;
        float slowFactor = Mathf.Clamp01(1f - pushMag * pushSlowMultiplier);
        Vector3 vel = direction * speed * slowFactor + pushVector;
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;
    }

    void HandleJump()
    {
        if (currentStamina < jumpStanima || mState == state.onWehicle) return;
        currentStamina -= jumpStanima;
        if (isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void SetCrouch(bool crouch)
    {
        if (mState == state.onWehicle) return;

        isCrouching = crouch;
        if (isCrouching && mState == state.run) SetState(state.walk);
        float targetY = isCrouching ? crouchHeight : standingHeight;
        Vector3 scale = transform.localScale;
        scale.y = targetY;
        transform.localScale = scale;
    }

    void SetSprint(bool sprint)
    {
        if (mState == state.onWehicle) return;

        if (sprint && !isCrouching)
        {
            SetState(state.run);
        }
    }

    void HandleStamina()
    {
        if (mState == state.run && moveInput.magnitude > 0 && currentStamina > 0)
        {
            currentStamina -= sprintRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                SetState(state.walk);
            }
        }
        else if (mState != state.run && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }
        UpdateStamina();
    }

    void ResetAllTriggers()
    {
        foreach (var param in camAnim.parameters)
            if (param.type == AnimatorControllerParameterType.Trigger)
                camAnim.ResetTrigger(param.name);
    }

    void SetState(state newState){
        mState = newState;
        ResetAllTriggers();
        switch (mState){
            case state.idle:
                camAnim.SetTrigger("Idle");
                SetWind(false);
                break;
            case state.walk:
                camAnim.SetTrigger("Walk");
                SetWind(false);
                break;
            case state.run:
                camAnim.SetTrigger("Run");
                SetWind(true);
                break;
            case state.onWehicle:
                camAnim.SetTrigger("Idle");
                rb.isKinematic = true;
                break;
        }
    }

    void UpdateStamina(){
        if (!uiIsOn && currentStamina != maxStamina){
            staminaAnim.ResetTrigger("Hide");
            staminaAnim.SetTrigger("Show");
            uiIsOn = true;
        }
        else if (uiIsOn && currentStamina == maxStamina){
            staminaAnim.ResetTrigger("Show");
            staminaAnim.SetTrigger("Hide");
            uiIsOn = false;
        }
        fillSprint.fillAmount = currentStamina / maxStamina;
    }

    void HandleLook()
    {
        transform.Rotate(Vector3.up * lookInput.x);
        cameraPitch -= lookInput.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = Vector3.right * cameraPitch;
    }

    public void SetAdditiveFOV(float fov){
        additiveFOV = fov;
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    public bool isPlayerOnWehicle(){
        return mState == state.onWehicle;
    }

    public void SetWehicle(){
        SetState(state.onWehicle);
    }
    public void LeaveWehicle(){
        rb.isKinematic = false;
        SetState(state.idle);
    }

    public Tips GetTips(){
        return tips;
    }

    public void SetWind(bool a){
        if (a) windVFX.Play();
        else windVFX.Stop();
    }


    private Vector3 lastDir = Vector3.zero, lastRight = Vector3.zero;
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("NPC")) return;

        Vector3 dirMove = transform.TransformDirection(moveInput);

        Vector3 right = Vector3.Cross(Vector3.up, dirMove.normalized);

        lastDir = dirMove.normalized;
        lastRight = right;

        Vector3 toNPC = collision.transform.position - transform.position;
        toNPC.y = 0;
        float side = Vector3.Dot(toNPC.normalized, right) >= 0 ? 1f : -1f;
        camAnim.SetTrigger(side == 1 ? "PushLeft" : "PushRight");
        Vector3 lateralDir = right * side;
        float pushStrength = dirMove.magnitude * pushMultiplierMe;
        pushVector = -lateralDir * pushStrength;
        NPC npc = collision.gameObject.GetComponent<NPC>();
        if (npc != null)
        {
            npc.PushMe(lateralDir * dirMove.magnitude * pushMultiplierNPC);
        }
    }
}
