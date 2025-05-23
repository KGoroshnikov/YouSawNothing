using UnityEngine;

public class HP : MonoBehaviour
{
    [SerializeField] private int mHP;

    [SerializeField] private PlayerController playerController;

    [SerializeField] private AudioSource hittedSource;

    public void TakeDamage(int dmg)
    {
        mHP -= dmg;

        if (hittedSource != null)
            hittedSource.Play();

        if (mHP <= 0)
        {
            if (playerController != null) playerController.Die();
        }
    }

    public void WaterDeath()
    {
        playerController.FastDie();
    }

    public int GetHP()
    {
        return mHP;
    }
}
