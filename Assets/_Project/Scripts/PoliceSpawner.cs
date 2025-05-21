using UnityEngine;

public class PoliceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject policePref;
    [SerializeField] private Vector2 spawnAmoutPerTask;
    [SerializeField] private Animator spawnAnim;
    [SerializeField] private Transform spawnCar;
    [SerializeField] private float radius;
    private int tasksCompleted;

    public void SetTasks(int amnt)
    {
        tasksCompleted = amnt;
        spawnAnim.SetTrigger("Spawn");
    }

    public void SpawnPolices()
    {
        int amnt = (int)Random.Range(spawnAmoutPerTask.x, spawnAmoutPerTask.y) * tasksCompleted;
        Debug.Log(amnt + " " + tasksCompleted);
        for (int i = 0; i < amnt; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            pos += spawnCar.position;
            Instantiate(policePref, pos, Quaternion.identity);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnCar.position, radius);
    }
}
