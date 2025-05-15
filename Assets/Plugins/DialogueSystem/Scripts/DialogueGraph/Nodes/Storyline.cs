using System;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [EditorPath("Storylines")]
    [OutputPort(typeof(Storyline),"Storyline")]
    public class Storyline : AbstractNode
    {
        [HideInInspector] public Storyline next;
        
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
        public Storyline GetNext() => next;
    }
}