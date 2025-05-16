using UnityEngine;
using UnityEngine.AI;

public class Police : NPC
{
    [SerializeField] private float radiusPossibleSearch;
    [SerializeField] private float chanceSearch;
    [SerializeField] private float checkPlayerTime;

    [SerializeField] private FOV fov;

    private PlayerController playerController;

    private SearchMinigame searchMinigame;

    private bool chasingPlayer;
    private bool lostPlayer;

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("CheckPlayer", checkPlayerTime, checkPlayerTime);
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        searchMinigame = GameObject.Find("SearchMinigame").GetComponent<SearchMinigame>();
    }

    protected override void SetRandMat()
    {
        
    }

    void OnEnable()
    {
        Inventory.OnSussyPicked += PlayerDidSussyThing;
    }

    void OnDisable()
    {
        Inventory.OnSussyPicked -= PlayerDidSussyThing;
    }

    void PlayerDidSussyThing()
    {
        if (!fov.isMeVisible(playerController.gameObject)) return;
        chasingPlayer = true;
        CancelInvoke();
        mState = State.walk;
        animator.SetTrigger("Walk");
    }

    protected override void FixedUpdate()
    {
        if (mState == State.none) return;
        else if (mState == State.walk && !chasingPlayer) CheckWalkTarget();
        else if (mState == State.walk && chasingPlayer) CheckPlayerDuringChase();
    }

    void CheckPlayerDuringChase()
    {
        agent.SetDestination(GetPos(playerController.transform.position));
        if (!agent.pathPending && agent.remainingDistance <= radiusPossibleSearch / 1.5f)
        {
            chasingPlayer = false;
            StartSearch();
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

    void ForgetPlayer()
    {
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
        searchMinigame.StartMinigame(this);
        mState = State.none;
        CancelInvoke();
        agent.ResetPath();
        animator.SetTrigger("Idle");
    }

    void CheckPlayer()
    {
        if (Vector3.Distance(transform.position, playerController.transform.position) > radiusPossibleSearch) return;

        if (Random.value > chanceSearch) return;

        StartSearch();
    }

    public void FinishCheck(bool isPlayerSussy)
    {
        if (isPlayerSussy)
        {
            // going to kill
        }
        else
        {
            mState = State.idle;
            Invoke("MakeDecision", Random.Range(idleTime.x, idleTime.y));
            InvokeRepeating("CheckPlayer", checkPlayerTime * 3, checkPlayerTime);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusPossibleSearch);
    }
}
