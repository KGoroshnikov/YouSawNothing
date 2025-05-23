using UnityEngine;

public class Water : MonoBehaviour
{
    private PlayerController playerController;

    [SerializeField] private Transform audioSource;

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        audioSource.position = new Vector3(playerController.transform.position.x, audioSource.position.y, audioSource.position.z);
    }
}
