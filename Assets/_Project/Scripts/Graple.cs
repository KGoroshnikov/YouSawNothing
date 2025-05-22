using UnityEngine;
using UnityEngine.InputSystem;

public class Graple : MonoBehaviour
{
    [SerializeField] private LayerMask lm;
    [SerializeField] private float maxDistShoot;
    [SerializeField] private float flyingTime;

    [SerializeField] private Transform grapleHead;
    [SerializeField] private Transform ropeTransform;
    [SerializeField] private Transform startRopePos;
    [SerializeField] private float ropeMul;

    [SerializeField] private AudioSource audioSource;

    private PlayerController player;
    [SerializeField] private float suckTime;
    [SerializeField] private float endOffset;

    public enum state
    {
        idle, forwarding, sucking
    }
    [SerializeField] private state mState;

    private Vector3 initialScale;
    private Vector3 defaultPos;
    private Vector3 endPos;
    private Quaternion endRot;
    private float tlerp;

    private Vector3 playerStart, playerEnd;
    private float playerLerp;

    private Transform interestingObject;

    void Start()
    {
        defaultPos = grapleHead.localPosition;
        initialScale = ropeTransform.localScale;

        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void OnDisable()
    {
        if (mState == state.idle) return;
        
        mState = state.idle;
        grapleHead.localPosition = defaultPos;
        grapleHead.localEulerAngles = Vector3.zero;
        ropeTransform.localScale = Vector3.zero;
        player.SetGraple(false);
        if (interestingObject == null) player.LeaveWehicle();
    }

    public void Shoot()
    {
        if (mState != state.idle) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistShoot, lm))
        {
            endRot = transform.rotation;
            endPos = hit.point;
            mState = state.forwarding;
            tlerp = 0;

            audioSource.Play();

            if (hit.collider.GetComponent<Item>() || hit.collider.GetComponent<NPC>())
            {
                interestingObject = hit.collider.transform;
            }
            else
            {
                player.ForceLeaveWehicle();
                player.SetGraple(true);
                interestingObject = null;
            }
        }
    }

    void Update()
    {
        if (mState == state.sucking)
        {
            playerLerp += Time.deltaTime / suckTime;
            playerLerp = Mathf.Clamp01(playerLerp);
            if (playerLerp >= 1)
            {
                mState = state.idle;
                grapleHead.localPosition = defaultPos;
                grapleHead.localEulerAngles = Vector3.zero;
                ropeTransform.localScale = Vector3.zero;
                player.SetGraple(false);
                if (interestingObject == null) player.LeaveWehicle();
                else
                {

                }
            }
            if (interestingObject == null)
                player.transform.position = Vector3.Lerp(playerStart, playerEnd, playerLerp);
            else
            {
                interestingObject.position = Vector3.Lerp(playerStart, playerEnd, playerLerp);
                tlerp -= Time.deltaTime / flyingTime;
            }
        }

        if (mState == state.idle) return;

        if (mState != state.sucking)
            tlerp += Time.deltaTime / flyingTime;
        tlerp = Mathf.Clamp01(tlerp);
        grapleHead.position = Vector3.Lerp(transform.TransformPoint(defaultPos), endPos, tlerp);
        SetRope(startRopePos.position, grapleHead.position);
        grapleHead.rotation = endRot;

        if (tlerp >= 1 && mState != state.sucking)
        {
            mState = state.sucking;

            if (interestingObject != null)
            {
                playerStart = interestingObject.position;
                playerEnd = transform.TransformPoint(defaultPos);
            }
            else
            {
                player.SetWehicle();
                playerStart = player.transform.position;
                Vector3 direction = transform.TransformPoint(defaultPos) - endPos;
                playerEnd = endPos - direction.normalized * endOffset;
            }
            playerLerp = 0;
        }
    }

    void SetRope(Vector3 start, Vector3 end)
    {
        ropeTransform.position = start;

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        ropeTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        Vector3 newScale = initialScale;
        newScale.z = distance * ropeMul;
        ropeTransform.localScale = newScale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxDistShoot);
    }
}
