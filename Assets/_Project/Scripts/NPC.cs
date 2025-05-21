using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System;
using Plugins.DialogueSystem.Scripts.DialogueGraph;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    public static event Action OnDamaged;
    private TaskManager.DeathCallback onDeath;

    [Header("NavMesh")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] private float wanderRadius;
    [SerializeField] protected Vector2 idleTime;

    [Header("Group")]
    [SerializeField] protected float groupChance;
    [SerializeField] private float groupRadius;
    [SerializeField] private float lookingForGroupRadius;
    [SerializeField] private Vector2 groupDuration;

    [Header("Push")]
    [SerializeField] private Vector3 pushVector;
    [SerializeField] private float pushDamping;

    [Header("Other")]
    [SerializeField] protected Animator animator;
    [SerializeField] private List<Rigidbody> ragdollRBs;
    private List<Collider> ragdollColliders;
    [SerializeField] protected Collider mainCollider;
    [SerializeField] private AnimationClip standUpClip;
    [SerializeField] private float blendDuration;
    [SerializeField] private GameObject targetArrow;

    [SerializeField] private Renderer renderer;
    
    
    [System.Serializable]
    public class skins
    {
        public Material matDefault, matDead, matSpeak;
    }
    private int currentSkin;
    [SerializeField] private skins[] randSkin;
    [SerializeField] private Transform rootBone;

    [SerializeField] private HP mHP;

    protected enum State
    {
        idle, walk, none
    }
    [SerializeField] protected State mState;
    
    [Header("Text Player")]
    [SerializeField] private StorylinePlayer storylinePlayer;
    [SerializeField] private string[] smallTalkRoots;
    [SerializeField] private TaskManager taskManager;
    
    [Header("Trust")] 
    [SerializeField] private int trustThreshold = 10;
    [SerializeField] private int trust;
    [SerializeField] private int trustGain = 2;
    [SerializeField] private int trustLoss = 1;
    
    [SerializeField] private UnityEvent onThrust;
    [SerializeField] private UnityEvent onNotThrust;
    [SerializeField] private UnityEvent onIncreaseThrust;
    [SerializeField] private UnityEvent onDecreaseThrust;
    
    [Header("Sell Task")] 
    [SerializeField] private string[] sellRoots;
    [SerializeField] private int trustDecreaseAfterSell = 3;

    protected StorylinePlayer StorylinePlayer => storylinePlayer;
    
    private Vector3 walkTarget;
    
    protected bool isGrouping;
    protected Vector3 groupCenter;
    private static List<NPC> allNPCs = new List<NPC>();

    private Vector3 relativeOffset;
    private Quaternion relativeRotation;

    private Transform[] bones;
    private Vector3[] targetLocalPos;
    private Quaternion[] targetLocalRot;

    private Action parentSetupCallback;

    protected bool laying;
    private bool isDead;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        allNPCs.Add(this);

        ragdollColliders = new List<Collider>();
        for (int i = 0; i < ragdollRBs.Count; i++)
        {
            ragdollRBs[i].isKinematic = true;
            ragdollColliders.Add(ragdollRBs[i].GetComponent<Collider>());
            ragdollColliders[ragdollColliders.Count - 1].enabled = false;
        }

        SetRandMat();
    }

    protected virtual void SetRandMat()
    {
        currentSkin = Random.Range(0, randSkin.Length);
        renderer.material = randSkin[currentSkin].matDefault;
    }

    protected virtual void Start()
    {
        relativeOffset = transform.position - rootBone.position;
        relativeRotation = Quaternion.Inverse(rootBone.rotation) * transform.rotation;

        Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
    }

    protected virtual void OnEnable()
    {
        parentSetupCallback += StartBlendAnim;
    }
    protected virtual void OnDisable()
    {
        parentSetupCallback -= StartBlendAnim;
    }

    void OnDestroy()
    {
        allNPCs.Remove(this);
    }

    protected virtual void MakeDecision(){
        if (isGrouping)
        {
            WanderAround(groupCenter);
        }
        else{
            if (Random.value < groupChance)
            {
                List<NPC> nearest = findNearest();
                if (nearest.Count > 0){
                    StartGrouping(nearest);
                    WanderAround(groupCenter);
                }
                else WanderRandom();
            }
            else
                WanderRandom();
        }
        mState = State.walk;
        animator.SetTrigger("Walk");
    }

    protected virtual void Update()
    {
        if (pushVector.sqrMagnitude > 0.0001f)
        {
            agent.Move(pushVector * Time.deltaTime);
            pushVector *= pushDamping;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (mState == State.none) return;

        CheckWalkTarget();
    }

    protected virtual void CheckWalkTarget()
    {
        if (mState == State.walk && !agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            agent.ResetPath();
            Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
            mState = State.idle;
            animator.SetTrigger("Idle");
        }
    }

    protected void WanderRandom()
    {
        Vector3 randomPos = RandomPos(transform.position, wanderRadius);
        agent.SetDestination(randomPos);
    }

    protected List<NPC> findNearest(){
        List<NPC> nearest = new List<NPC>();
        for(int i = 0; i < allNPCs.Count; i++){
            if (Vector3.Distance(transform.position, allNPCs[i].transform.position) <= lookingForGroupRadius)
                nearest.Add(allNPCs[i]);
        }
        return nearest;
    }

    protected void StartGrouping(List<NPC> nearest)
    {
        isGrouping = true;
        Invoke("StopGrouping", Random.Range(groupDuration.x, groupDuration.y));
        NPC other = nearest[Random.Range(0, nearest.Count)];
        groupCenter = other.transform.position;
    }

    void StopGrouping()
    {
        isGrouping = false;
        WanderRandom();
    }

    protected void WanderAround(Vector3 center)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * groupRadius;
        randomPoint.y = transform.position.y;
        Vector3 navPoint;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            navPoint = hit.position;
        else
            navPoint = transform.position;

        agent.SetDestination(navPoint);
    }

    Vector3 RandomPos(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, NavMesh.AllAreas);
        return navHit.position;
    }

    public void PushMe(Vector3 push)
    {
        pushVector += push;
    }

    IEnumerator AdjustParent(Action callback)
    {
        Vector3 bonePos = rootBone.position;
        Quaternion boneRot = rootBone.rotation;

        transform.position = relativeOffset + rootBone.position;
        transform.rotation = boneRot * relativeRotation;

        rootBone.position = bonePos;
        rootBone.rotation = boneRot;
        yield return null;
        rootBone.position = bonePos;
        rootBone.rotation = boneRot;

        callback.Invoke();
    }

    IEnumerator BlendToAnimation()
    {
        Vector3[] startLocalPos = new Vector3[bones.Length];
        Quaternion[] startLocalRot = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            startLocalPos[i] = bones[i].localPosition;
            startLocalRot[i] = bones[i].localRotation;
        }

        standUpClip.SampleAnimation(gameObject, 0);

        for (int i = 0; i < bones.Length; i++)
        {
            targetLocalPos[i] = bones[i].localPosition;
            targetLocalRot[i] = bones[i].localRotation;
        }
        
        
        float elapsed = 0f;
        while (elapsed < blendDuration)
        {
            float t = elapsed / blendDuration;
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].localPosition = Vector3.Lerp(startLocalPos[i], targetLocalPos[i], t);
                bones[i].localRotation = Quaternion.Slerp(startLocalRot[i], targetLocalRot[i], t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].localPosition = targetLocalPos[i];
            bones[i].localRotation = targetLocalRot[i];
        }

        animator.enabled = true;
        animator.Play(standUpClip.name, 0, 0);
    }

    void StandUp()
    {
        for (int i = 0; i < ragdollRBs.Count; i++)
        {
            ragdollColliders[i].enabled = false;
            ragdollRBs[i].isKinematic = true;
        }
        StartCoroutine(AdjustParent(parentSetupCallback));

        bones = GetComponentsInChildren<Transform>();
        targetLocalPos = new Vector3[bones.Length];
        targetLocalRot = new Quaternion[bones.Length];
    }

    public virtual void StandupFinish() // called from anim
    {
        mState = State.idle;
        animator.SetTrigger("Idle");
        CancelInvoke();
        animator.enabled = true;
        mainCollider.enabled = true;
        laying = false;
        Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
    }

    void StartBlendAnim() {
        StartCoroutine(BlendToAnimation());
    }

    public virtual void RagdollDamaged(Vector3 push, int damage)
    {
        OnDamaged.Invoke();
        mHP.TakeDamage(damage);
        EnableRagdoll(push);
    }

    void ResetAllTriggers()
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }
    }

    public void SetTragetArrow(bool a)
    {
        targetArrow.SetActive(a);
    }

    public void SubscribeToDeath(TaskManager.DeathCallback callback)
    {
        onDeath = callback;
    }

    protected virtual void OnDie()
    {
        renderer.material = randSkin[currentSkin].matDead;
        if (onDeath != null) onDeath(this);
        isDead = true;
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    public void EnableRagdoll(Vector3 push)
    {
        mState = State.none;
        ResetAllTriggers();
        CancelInvoke();
        agent.ResetPath();
        laying = true;

        if (mHP.GetHP() > 0) Invoke("StandUp", 2);
        else
        {
            OnDie();
        }

        animator.enabled = false;
        mainCollider.enabled = false;
        for (int i = 0; i < ragdollRBs.Count; i++)
        {
            ragdollColliders[i].enabled = true;
            ragdollRBs[i].isKinematic = false;
            ragdollRBs[i].gameObject.layer = LayerMask.NameToLayer("Water");
            ragdollRBs[i].AddForce(push, ForceMode.Impulse);
        }

    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, groupRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookingForGroupRadius);
    }
    
    public void Speak()
    {
        if (storylinePlayer == null) return;
        if (storylinePlayer.IsPlaying) return;
        if (taskManager.CanSell && sellRoots.Length > 0)
            storylinePlayer.StartStorylineNow(
                sellRoots[Random.Range(0, sellRoots.Length)]
            );
        else if (smallTalkRoots.Length > 0)
            storylinePlayer.StartStorylineNow(
                smallTalkRoots[Random.Range(0, smallTalkRoots.Length)]
                );
        
    }
    public void IncreaseTrust()
    {
        trust += trustGain;
        onIncreaseThrust.Invoke();
        if (trust == trustThreshold) 
            onThrust.Invoke();
    }

    public void DecreaseTrust()
    {
        trust -= trustLoss;
        onDecreaseThrust.Invoke();
        if (!IsWalkerThrustYou) 
            onNotThrust.Invoke();
    }

    public bool TrySell()
    {
        if (sellRoots.Length == 0) return false;
        if (IsWalkerThrustYou)
            taskManager.Sell();
        trust -= trustDecreaseAfterSell;
        return IsWalkerThrustYou;
    }

    public bool IsWalkerThrustYou => trust >= trustThreshold;
}
