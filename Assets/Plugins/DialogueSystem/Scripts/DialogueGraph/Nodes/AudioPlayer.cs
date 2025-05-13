using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [OutputPort(typeof(AudioPlayer),"AudioPlayer")]
    public abstract class AudioPlayer : AbstractNode
    {
        [FormerlySerializedAs("container")]
        [InputPort("Audio")]
        [HideInInspector]
        public AudioContainer audioContainer;

        public abstract void OnDraw(StorylinePlayer storylinePlayer);
        public abstract bool IsCompleted();
        public abstract void PauseAudio(StorylinePlayer storylinePlayer);
        public abstract void PlayAudio(StorylinePlayer storylinePlayer);
    }
}