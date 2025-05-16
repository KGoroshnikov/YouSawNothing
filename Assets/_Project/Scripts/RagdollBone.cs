using UnityEngine;

public class RagdollBone : MonoBehaviour
{
    [SerializeField] private NPC npc;

    public NPC GetNPC(){
        return npc;
    }
}
