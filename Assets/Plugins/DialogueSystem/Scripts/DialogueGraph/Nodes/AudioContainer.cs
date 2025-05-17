using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [OutputPort(typeof(AudioContainer),"Audio")]
    public abstract class AudioContainer : AbstractNode
    {
        public abstract AudioClip Audio { get; }
    }
}