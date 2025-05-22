using UnityEngine;

public class Boat : MonoBehaviour
{
    private PlayerController playerController;

    [SerializeField] private Transform playerPos;
    [SerializeField] private Animator animator;
    [SerializeField] private string animName;
    [SerializeField] private Boat otherBoat;
    private Vector3 defPose;
    private bool used;

    void Start()
    {
        defPose = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (other.CompareTag("Player"))
        {
            if (playerController == null) playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.ForceLeaveWehicle();
            playerController.SetWehicle();
            playerController.transform.SetParent(playerPos);
            playerController.transform.localPosition = Vector3.zero;
            animator.SetTrigger(animName);
            used = true;
            otherBoat.ResetBoat();
        }
    }

    public void ResetBoat()
    {
        animator.SetTrigger("Rest");
        transform.position = defPose;
        used = false;
    }

    public void ReleasePlayer()
    {
        playerController.LeaveWehicle();
        playerController.transform.SetParent(null);
    }

}
