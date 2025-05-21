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
    private float maxStamina => playerStats.Stamina;
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

    [Header("Attack")]
    [SerializeField] private float attackDist;
    [SerializeField] private float attackThic;
    [SerializeField] private int baseballDamage;
    [SerializeField] private float baseballPush;
    [SerializeField] private VisualEffect gunMuzzle;
    [SerializeField] private VisualEffect bulletTrail;
    [SerializeField] private int pistolDamage;
    [SerializeField] private LayerMask lm;

    [Header("Other")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Animator camAnim, itemAnim;
    [SerializeField] private Tips tips;
    [SerializeField] private Camera cam;
    [SerializeField] private VisualEffect windVFX;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Inventory inventory;
    [SerializeField] private HP hp;
    private GameManager gameManager;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction lmbAction;

    private float additiveFOV;

    private Vector3 moveInput;
    private Vector2 lookInput;
    private float cameraPitch;
    private bool isGrounded;
    private bool isCrouching;
    private float currentStamina;

    private bool lookIsLocked;

    private enum state
    {
        idle, walk, run, onWehicle
    }
    [SerializeField] private state mState;

    private bool died;

    private bool canAttack = true;

    private bool freezed;
    private SkateboardController skateboardController;

    [SerializeField] private PauseManager pauseManager;

    void Awake()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
        lmbAction = playerInput.actions["LMB"];

        jumpAction.performed += _ => HandleJump();
        crouchAction.performed += _ => SetCrouch(true);
        crouchAction.canceled += _ => SetCrouch(false);
        sprintAction.performed += _ => SetSprint(true);
        sprintAction.canceled += _ => SetSprint(false);
        lmbAction.performed += _ => OnLMB();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;

        SetWehicle();
        SetLockLook(true);
        Invoke("StartGame", 3);
    }

    void StartGame()
    {
        LeaveWehicle();
        SetLockLook(false);
        pauseManager.SetAblePause(true);
    }

    void Start()
    {
        GetSens();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void OnLMB()
    {
        if (!canAttack || freezed) return;
        if (inventory.currentHoldingId() == 4)
            camAnim.SetTrigger("Baseball");
        else if (inventory.currentHoldingId() == 5)
            camAnim.SetTrigger("Gun");
        else if (inventory.currentHoldingId() == 6)
        {
            inventory.GetGraple().Shoot();
            return;
        }
        else if (inventory.currentHoldingId() == 7)
        {
            inventory.GetSprayPaint().SpawnPaint();
            canAttack = false;
            itemAnim.SetTrigger("Paint");
            CancelInvoke("ResetAttackColldown");
            Invoke("ResetAttackColldown", 1.5f);
            return;
        }
        else return;

        canAttack = false;
        CancelInvoke("ResetAttackColldown");
        Invoke("ResetAttackColldown", 2); // not a real cooldown
    }

    public void ResetAttackColldown() // from anim
    {
        CancelInvoke("ResetAttackColldown");
        canAttack = true;
    }

    public void DealBaseballDamage() // from anim
    {
        RaycastHit[] hit = Physics.SphereCastAll(cam.transform.position, attackThic, cam.transform.forward, attackDist);
        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider.CompareTag("NPC"))
            {
                NPC npc = hit[i].collider.GetComponent<NPC>();
                Vector3 dir = hit[i].transform.position - transform.position;
                dir.y = 0;
                npc.RagdollDamaged(dir.normalized * baseballPush, baseballDamage);
            }
            else if (hit[i].collider.gameObject != gameObject) break;
        }
    }

    public void GunShoot()
    {
        gunMuzzle.Play();
        bulletTrail.Play();
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100, lm))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                NPC npc = hit.collider.GetComponent<NPC>();
                Vector3 dir = hit.transform.position - transform.position;
                dir.y = 0;
                npc.RagdollDamaged(dir.normalized * baseballPush, pistolDamage);
            }
        }
    }

    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

    public Inventory GetInventory()
    {
        return inventory;
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
        lmbAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
        lmbAction.Disable();
    }

    void Update()
    {
        if (freezed) return;

        moveInput = new Vector3(moveAction.ReadValue<Vector2>().x, 0, moveAction.ReadValue<Vector2>().y);
        lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        HandleLook();
        CheckGround();
        HandleStamina();
        HandleFOV();


        Debug.DrawRay(transform.position, Vector3.up, Color.blue);
        Debug.DrawRay(transform.position, lastRight, Color.red);
        Debug.DrawRay(transform.position, lastDir.normalized, Color.green);

        Debug.DrawRay(transform.position, transform.TransformDirection(moveInput), Color.magenta);
    }

    void FixedUpdate()
    {
        if (freezed) return;
        HandleMovement();
    }

    void HandleFOV()
    {
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

    public void FreezePlayer()
    {
        if (skateboardController != null) skateboardController.SetFreezed(true);
        rb.isKinematic = true;
        freezed = true;
    }
    public void UnfreezePlayer()
    {
        if (skateboardController != null) skateboardController.SetFreezed(false);
        rb.isKinematic = false;
        freezed = false;
    }

    public void GetSens()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("PlayerSens", 0.3f);
    }

    void HandleJump()
    {
        if (currentStamina < jumpStanima || mState == state.onWehicle || freezed) return;
        currentStamina -= jumpStanima;
        if (isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void SetCrouch(bool crouch)
    {
        if (mState == state.onWehicle || freezed) return;

        isCrouching = crouch;
        if (isCrouching && mState == state.run) SetState(state.walk);
        float targetY = isCrouching ? crouchHeight : standingHeight;
        Vector3 scale = transform.localScale;
        scale.y = targetY;
        transform.localScale = scale;
    }

    void SetSprint(bool sprint)
    {
        if (mState == state.onWehicle || freezed) return;

        if (sprint && !isCrouching)
        {
            SetState(state.run);
        }
    }

    void HandleStamina()
    {
        Debug.Log(maxStamina);
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

    void SetState(state newState)
    {
        if (died) return;
        mState = newState;
        ResetAllTriggers();
        switch (mState)
        {
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

    void UpdateStamina()
    {
        if (!uiIsOn && currentStamina != maxStamina)
        {
            staminaAnim.ResetTrigger("Hide");
            staminaAnim.SetTrigger("Show");
            uiIsOn = true;
        }
        else if (uiIsOn && currentStamina == maxStamina)
        {
            staminaAnim.ResetTrigger("Show");
            staminaAnim.SetTrigger("Hide");
            uiIsOn = false;
        }
        fillSprint.fillAmount = currentStamina / maxStamina;
    }

    void HandleLook()
    {
        if (lookIsLocked) return;

        transform.Rotate(Vector3.up * lookInput.x);
        cameraPitch -= lookInput.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = Vector3.right * cameraPitch;
    }

    public void SetAdditiveFOV(float fov)
    {
        additiveFOV = fov;
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    public bool isPlayerOnWehicle()
    {
        return mState == state.onWehicle;
    }

    public void SetWehicle(SkateboardController skate = null)
    {
        if (skate != null) skateboardController = skate;
        SetState(state.onWehicle);
    }
    public void LeaveWehicle()
    {
        rb.isKinematic = false;
        SetState(state.idle);
    }

    public void Die()
    {
        if (died) return;
        died = true;
        camAnim.SetTrigger("Death");
        SetWehicle();
        SetLockLook(true);
    }

    public void CloseEyes()
    {
        gameManager.PlayerDied();
    }

    public void TakeDamage(int dmg)
    {
        camAnim.SetTrigger("Damaged");
        hp.TakeDamage(dmg);
    }

    public void SetLockLook(bool a)
    {
        lookIsLocked = a;
    }

    public Tips GetTips()
    {
        return tips;
    }

    public void SetWind(bool a)
    {
        if (a) windVFX.Play();
        else windVFX.Stop();
    }


    private Vector3 lastDir = Vector3.zero, lastRight = Vector3.zero;
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("NPC") || died) return;

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

    public Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cam.transform.position, attackThic);
        Vector3 pos2 = cam.transform.position + cam.transform.forward * (attackDist - attackThic / 2);
        Gizmos.DrawWireSphere(pos2, attackThic);
        Gizmos.DrawLine(cam.transform.position, pos2);
    }
}
