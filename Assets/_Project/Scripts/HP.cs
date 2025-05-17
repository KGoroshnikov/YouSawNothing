using UnityEngine;

public class HP : MonoBehaviour
{
    [SerializeField] private int mHP;

    [SerializeField] private PlayerController playerController;

    public void TakeDamage(int dmg)
    {
        mHP -= dmg;

        if (mHP <= 0)
        {
            if (playerController != null) playerController.Die();
        }
    }

    public int GetHP()
    {
        return mHP;
    }
}
