using UnityEngine;

public class SpawnNPC : MonoBehaviour
{
    [SerializeField] private GameObject npcPref;
    [SerializeField] private Vector2 minMaxX;
    [SerializeField] private Vector2 minMaxZ;
    [SerializeField] private float Y;
    [SerializeField] private int amount;

    void Start()
    {
        for(int i = 0; i < amount; i++){
            Vector3 pos = new Vector3(Random.Range(minMaxX.x, minMaxX.y), Y, Random.Range(minMaxZ.x, minMaxZ.y));
            Instantiate(npcPref, pos, Quaternion.identity);
        }
    }
}
