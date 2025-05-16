using System;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.AudioPlayers
{
    [EditorPath("Audio Players")]
    public class UndertaleAudioPlayer : AudioPlayer
    {
        [SerializeField] private float minPitch = 1;
        [SerializeField] private float maxPitch = 1;
        [SerializeField] private float volume = 1;
        
        public override AbstractNode Clone()
        {
            var clone = base.Clone() as UndertaleAudioPlayer;
            clone.maxPitch = maxPitch;
            clone.minPitch = minPitch;
            clone.volume = volume;
            return clone;
        }
        public override void OnDraw(StorylinePlayer storylinePlayer)
        {
            if (!storylinePlayer.IsAudioPlaying)
                storylinePlayer.PlayAudio(
                    audioContainer.Audio,
                    Random.Range(minPitch, maxPitch),
                    volume
                );
        }

        public override bool IsCompleted(StorylinePlayer storylinePlayer) => 
            !storylinePlayer.IsAudioPlaying && !storylinePlayer.IsTextPlaying;

        public override void StopAudio(StorylinePlayer storylinePlayer) => storylinePlayer.StopAudio();

        public override void PlayAudio(StorylinePlayer storylinePlayer) { }
    }
}