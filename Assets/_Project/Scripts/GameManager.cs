using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Animator fade;

    public void PlayerDied()
    {
        fade.SetTrigger("FadeIn");
    }
}
