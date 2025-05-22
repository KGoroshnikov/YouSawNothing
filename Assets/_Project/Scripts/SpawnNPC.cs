using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts;
using UnityEngine;

public class SpawnNPC : MonoBehaviour
{
    [SerializeField] private GameObject npcPref;
    [SerializeField] private Vector2 minMaxX;
    [SerializeField] private Vector2 minMaxZ;
    [SerializeField] private float Y;
    [SerializeField] private int amount;

    [SerializeField] private List<NPC> npcs = new List<NPC>();

    [SerializeField] private ConcurrentTextSelector textSelector;

    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(minMaxX.x, minMaxX.y), Y, Random.Range(minMaxZ.x, minMaxZ.y));
            var go = Instantiate(npcPref, pos, Quaternion.identity);
            if (go.TryGetComponent<StorylinePlayer>(out var player))
                player.textSelector = textSelector;
            NPC npc = go.GetComponent<NPC>();
            
            npcs.Add(npc);
        }
    }

    public NPC GetRandomNPC() {
        return npcs[Random.Range(0, npcs.Count)];   
    }
}
