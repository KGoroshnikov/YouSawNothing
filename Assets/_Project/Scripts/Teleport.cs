using UnityEngine;

public class Teleport : MonoBehaviour
{
    private PlayerController playerController;

    [SerializeField] private Teleport targetTeleport;

    private bool doNotRegisterPlayer;

    void OnTriggerEnter(Collider other)
    {
        if (doNotRegisterPlayer) return;
        if (other.CompareTag("Player"))
        {
            if (playerController == null) playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.SetFade(true);
            playerController.ForceLeaveWehicle();
            playerController.SetWehicle();

            Invoke("TeleportPlayer", 1.5f);
        }
    }

    public void PlayerTeleportedToMe()
    {
        doNotRegisterPlayer = true;
        Invoke("ResetCD", 1);
    }

    void ResetCD()
    {
        doNotRegisterPlayer = false;
    }

    void TeleportPlayer()
    {
        if (playerController == null) playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        targetTeleport.PlayerTeleportedToMe();

        playerController.transform.position = targetTeleport.transform.position;
        playerController.transform.rotation = targetTeleport.transform.rotation;

        playerController.SetFade(false);

        playerController.LeaveWehicle();
    }
}
