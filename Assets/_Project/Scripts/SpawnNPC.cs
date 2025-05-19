using System.Collections.Generic;
using UnityEngine;

public class SpawnNPC : MonoBehaviour
{
    [SerializeField] private GameObject npcPref;
    [SerializeField] private Vector2 minMaxX;
    [SerializeField] private Vector2 minMaxZ;
    [SerializeField] private float Y;
    [SerializeField] private int amount;

    [SerializeField] private List<NPC> npcs = new List<NPC>();

    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(minMaxX.x, minMaxX.y), Y, Random.Range(minMaxZ.x, minMaxZ.y));
            NPC npc = Instantiate(npcPref, pos, Quaternion.identity).GetComponent<NPC>();
            npcs.Add(npc);
        }
    }

    public NPC GetRandomNPC() {
        return npcs[Random.Range(0, npcs.Count)];   
    }
}
