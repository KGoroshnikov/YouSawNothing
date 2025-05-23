using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts;
using Plugins.DialogueSystem.Scripts.DialogueGraph;
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
    [SerializeField] private VariantContainer variantContainer;
    [SerializeField] private PlayerStats stats;

    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(minMaxX.x, minMaxX.y), Y, Random.Range(minMaxZ.x, minMaxZ.y));
            var go = Instantiate(npcPref, pos, Quaternion.identity);
            if (go.TryGetComponent<StorylinePlayer>(out var player))
                player.textSelector = textSelector;
            foreach (var selector in go.GetComponents<VariantSelector>())
            {
                selector.container = variantContainer;
                if (selector is QTEVariantSelector qte)
                    qte.stats = stats;
            }

            NPC npc = go.GetComponent<NPC>();
            
            npcs.Add(npc);
        }
    }

    public NPC GetRandomNPC() {
        return npcs[Random.Range(0, npcs.Count)];   
    }
}
