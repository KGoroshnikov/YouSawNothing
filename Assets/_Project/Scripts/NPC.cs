using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [Header("NavMesh")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] private float wanderRadius;
    [SerializeField] protected Vector2 idleTime;

    [Header("Group")]
    [SerializeField] private float groupChance;
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
    [SerializeField] private Collider mainCollider;

    [SerializeField] private Renderer renderer;
    [SerializeField] private Material[] randMats;

    protected enum State{
        idle, walk, none
    }
    [SerializeField] protected State mState;
    private Vector3 walkTarget;
    
    private bool isGrouping;
    private Vector3 groupCenter;
    private static List<NPC> allNPCs = new List<NPC>();

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
        renderer.material = randMats[Random.Range(0, randMats.Length)];
    }

    protected virtual void Start()
    {
        Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
    }

    void OnDestroy()
    {
        allNPCs.Remove(this);
    }

    void MakeDecision(){
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

    void Update()
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

    void WanderRandom()
    {
        Vector3 randomPos = RandomPos(transform.position, wanderRadius);
        agent.SetDestination(randomPos);
    }

    List<NPC> findNearest(){
        List<NPC> nearest = new List<NPC>();
        for(int i = 0; i < allNPCs.Count; i++){
            if (Vector3.Distance(transform.position, allNPCs[i].transform.position) <= lookingForGroupRadius)
                nearest.Add(allNPCs[i]);
        }
        return nearest;
    }

    void StartGrouping(List<NPC> nearest)
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

    void WanderAround(Vector3 center)
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

    public void EnableRagdoll(Vector3 push){
        mState = State.none;
        CancelInvoke();
        agent.ResetPath();
        
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
}
