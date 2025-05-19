using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Storyline
{
    [EditorPath("Storylines")]
    [OutputPort(typeof(Storyline),"Storyline")]
    public class Storyline : AbstractNode
    {
        [HideInInspector] private Storyline next;
        
        [InputPort("TextPlayer")]
        [HideInInspector] 
        public TextPlayer textPlayer;
        
        [InputPort("AudioPlayer")]
        [HideInInspector] 
        public AudioPlayer audioPlayer;
        
        [InputPort("WaitCondition")]
        [HideInInspector] 
        public WaitCondition waitCondition;
        
        
        public string tag;
        public virtual Storyline GetNext() => next;
        public virtual void SetNext(Storyline storyline) => next = storyline;

        public override AbstractNode Clone()
        {
            var node = Instantiate(this);
            node.next = next;
            node.textPlayer = textPlayer;
            node.audioPlayer = audioPlayer;
            node.waitCondition = waitCondition;
            node.tag = tag;
            return node;
        }
    }
}