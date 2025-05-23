using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts.DialogueGraph;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class Police : NPC
{
    [Header("Base")]
    [SerializeField] private float radiusPossibleSearch;
    [SerializeField] private float chanceSearch;
    [SerializeField] private float checkPlayerTime;
    [SerializeField] private bool imWithGun;

    [Header("Patrol")]
    [SerializeField] private Transform patrolObject;

    [Header("Gun")]
    [SerializeField] private float gunAttackRadius;
    [SerializeField] private Vector2 accuracy;
    [SerializeField] private Vector2 PlayerVel;
    [SerializeField] private int gunDamage;

    [Header("Baseball")]
    [SerializeField] private int baseballDamage;
    [SerializeField] private float attackRadius;
    [SerializeField] private float damageRadius;
    private bool wantToKill;

    [Header("Other")]
    [SerializeField] private FOV fov;
    [SerializeField] private VisualEffect muzzle;
    [SerializeField] private VisualEffect bulletTrail;
    [SerializeField] private GameObject baseballObj, gunObj;
    
    [FormerlySerializedAs("chaseRoot")]
    [Header("Text Player")]
    [SerializeField] private string[] chaseRoots = { "chase" };
    [SerializeField] private string[] searchRoots = { "search" };
    [SerializeField] private string[] didSussyRoots = { "sussy" };
    [SerializeField] private string[] didIllegalRoots = { "illegal" };

    [SerializeField] private AudioSource gunshot, baseball;

    private PlayerController playerController;

    private SearchMinigame searchMinigame;

    private bool chasingPlayer;
    private bool lostPlayer;

    protected override void Start()
    {
        imWithGun = Random.value <= 0.5f;
        base.Start();
        InvokeRepeating("CheckPlayer", checkPlayerTime, checkPlayerTime);
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        searchMinigame = GameObject.Find("SearchMinigame").GetComponent<SearchMinigame>();

        baseballObj.SetActive(!imWithGun);
        gunObj.SetActive(imWithGun);

        //WantToKillPlayer();
    }

    protected override void SetRandMat()
    {
        
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Inventory.OnSussyPicked += PlayerDidSussyThing;
        Inventory.OnSussyInHand += PlayerDidSussyThing;
        SkateboardController.OnStolenWehicle += PlayerDidIllegal;
        NPC.OnDamaged += PlayerDidIllegal;
        PaintHolder.OnPainted += PlayerDidIllegal;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Inventory.OnSussyPicked -= PlayerDidSussyThing;
        Inventory.OnSussyInHand -= PlayerDidSussyThing;
        SkateboardController.OnStolenWehicle -= PlayerDidIllegal;
        NPC.OnDamaged -= PlayerDidIllegal;
        PaintHolder.OnPainted -= PlayerDidIllegal;
    }

    public void PlayerDidSussyThing()
    {
        StorylinePlayer?.QueueStoryline(didSussyRoots[Random.Range(0, didSussyRoots.Length)]);
        if (!fov.isMeVisible(playerController.gameObject) || mState == State.none) return;
        chasingPlayer = true;
        CancelInvoke();
        mState = State.walk;
        animator.SetTrigger("Walk");
    }

    public void PlayerDidIllegal()
    {
        StorylinePlayer?.QueueStoryline(didIllegalRoots[Random.Range(0, didIllegalRoots.Length)]);
        if (!fov.isMeVisible(playerController.gameObject) || mState == State.none) return;
        chasingPlayer = true;
        wantToKill = true;
        CancelInvoke();
        mState = State.walk;
        animator.SetTrigger("Walk");
    }

    void LateUpdate()
    {
        base.Update();
        if (mState == State.none && !laying)
        {
            if (wantToKill)
            {
                bulletTrail.transform.forward = playerController.transform.position - bulletTrail.transform.position;
            }
            return;
        }
    }

    protected override void FixedUpdate()
    {
        if (mState == State.none && !laying)
        {
            if (wantToKill)
            {
                Vector3 dir = playerController.transform.position - transform.position;
                dir.y = 0;
                transform.forward = dir;
            }
            return;
        }
        else if (mState == State.walk && !chasingPlayer) CheckWalkTarget();
        else if (mState == State.walk && chasingPlayer) CheckPlayerDuringChase();
    }

    public override void RagdollDamaged(Vector3 push, int damage)
    {
        base.RagdollDamaged(push, damage);
        wantToKill = true;
        chasingPlayer = true;
    }

    public override void StandupFinish()
    {
        CancelInvoke();
        animator.enabled = true;
        mainCollider.enabled = true;
        laying = false;

        if (chasingPlayer || wantToKill)
        {
            WantToKillPlayer();
        }
        else
        {
            ForgetPlayer();
        }
    }


    void CheckPlayerDuringChase()
    {
        agent.SetDestination(GetPos(playerController.transform.position));
        if (!wantToKill && !agent.pathPending && agent.remainingDistance <= radiusPossibleSearch / 1.5f)
        {
            chasingPlayer = false;
            StartSearch();
        }
        else if (wantToKill && imWithGun && !agent.pathPending && agent.remainingDistance <= gunAttackRadius)
        {
            agent.ResetPath();
            mState = State.none;
            animator.ResetTrigger("Walk");
            animator.SetTrigger("Shoot");
        }
        else if (wantToKill && !imWithGun && !agent.pathPending && agent.remainingDistance <= attackRadius)
        {
            agent.ResetPath();
            mState = State.none;
            animator.ResetTrigger("Walk");
            animator.SetTrigger("HandAttack");
        }

        if (!lostPlayer && !fov.isMeVisible(playerController.gameObject))
        {
            lostPlayer = true;
            Invoke("ForgetPlayer", 5);
        }
        else if (lostPlayer && fov.isMeVisible(playerController.gameObject))
        {
            lostPlayer = false;
            CancelInvoke("ForgetPlayer");
        }
    }

    public void DoBaseballDamage()
    {
        baseball.Play();
        if (Vector3.Distance(transform.position, playerController.transform.position) > damageRadius) return;

        playerController.TakeDamage(baseballDamage);
    }

    public void Shoot()
    {
        muzzle.Play();
        bulletTrail.Play();
        gunshot.Play();

        float it = Mathf.InverseLerp(PlayerVel.x, PlayerVel.y, playerController.GetVelocity().magnitude);
        float acc = Mathf.Lerp(accuracy.x, accuracy.y, it);
        if (Random.value <= acc)
        {
            RaycastHit hit;
            Vector3 startPos = transform.position + transform.forward + new Vector3(0, 0.5f, 0);
            Vector3 toPlayer = playerController.transform.position - startPos;
            if (Physics.Raycast(startPos, toPlayer, out hit)){
                if (hit.collider.gameObject == playerController.gameObject)
                {
                    playerController.TakeDamage(gunDamage);
                }
            }
        }
        else
        {

        }
    }   

    public void ShootEnded()
    {
        if (Vector3.Distance(transform.position, playerController.transform.position) > gunAttackRadius)
        {
            animator.ResetTrigger("Shoot");
            animator.SetTrigger("Walk");
            mState = State.walk;
        }
    }

    public void AttackAnimEnd()
    {
        if (Vector3.Distance(transform.position, playerController.transform.position) > damageRadius)
        {
            animator.ResetTrigger("HandAttack");
            animator.SetTrigger("Walk");
            mState = State.walk;
        }
    }

    void ForgetPlayer()
    {
        wantToKill = false;
        chasingPlayer = false;
        lostPlayer = false;
        CancelInvoke();
        agent.ResetPath();
        mState = State.idle;
        animator.SetTrigger("Idle");
        Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
    }

    Vector3 GetPos(Vector3 origin)
    {
        NavMeshHit navHit;
        NavMesh.SamplePosition(origin, out navHit, 1, NavMesh.AllAreas);
        return navHit.position;
    }

    void StartSearch()
    {
        StorylinePlayer?.QueueStoryline(searchRoots[Random.Range(0, searchRoots.Length)]);
        searchMinigame.StartMinigame(this);
        mState = State.none;
        CancelInvoke();
        agent.ResetPath();
        animator.SetTrigger("Idle");
    }

    void CheckPlayer()
    {
        if (mState == State.none) return;

        if (Vector3.Distance(transform.position, playerController.transform.position) > radiusPossibleSearch) return;

        if (Random.value > chanceSearch) return;

        StartSearch();
    }

    void WantToKillPlayer()
    {
        StorylinePlayer?.QueueStoryline(chaseRoots[Random.Range(0, chaseRoots.Length)]);
        wantToKill = true;
        chasingPlayer = true;
        CancelInvoke();
        mState = State.walk;
        animator.SetTrigger("Walk");
    }

    protected override void OnDie()
    {
        base.OnDie();
        wantToKill = false;
        chasingPlayer = false;
        lostPlayer = false;
        CancelInvoke();
        agent.ResetPath();
        mState = State.none;
        laying = true;
    }

    public void FinishCheck(bool isPlayerSussy)
    {
        if (isPlayerSussy)
        {
            WantToKillPlayer();
        }
        else
        {
            mState = State.idle;
            Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
            InvokeRepeating("CheckPlayer", checkPlayerTime * 3, checkPlayerTime);
        }
    }
    protected override void MakeDecision(){
        if (isGrouping && patrolObject == null)
        {
            WanderAround(groupCenter);
        }
        else{
            if (Random.value < groupChance && patrolObject == null)
            {
                List<NPC> nearest = findNearest();
                if (nearest.Count > 0){
                    StartGrouping(nearest);
                    WanderAround(groupCenter);
                }
                else WanderRandom();
            }
            else if (patrolObject != null)
                WanderAround(patrolObject.position);
            else
                WanderRandom();
        }
        mState = State.walk;
        animator.SetTrigger("Walk");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusPossibleSearch);

        Gizmos.color = Color.magenta;
        if (imWithGun)
        {
            Gizmos.DrawWireSphere(transform.position, gunAttackRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, attackRadius);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
}
