using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.AudioContainers
{
    [EditorPath("Audio Containers")]
    public class SimpleAudioContainer : AudioContainer
    {
        [SerializeField] private AudioClip audio;
        public override AudioClip Audio => audio;

        public override AbstractNode Clone()
        {
            var clone = base.Clone() as SimpleAudioContainer;
            clone.audio = audio;
            return clone;
        }
    }
}