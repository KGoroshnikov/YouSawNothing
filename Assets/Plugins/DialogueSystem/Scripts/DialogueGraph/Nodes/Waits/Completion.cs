using System;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Waits
{
    [EditorPath("Wait Conditions")]
    public class Completion : WaitCondition
    {
        public override AbstractNode Clone()
        {
            var clone = Instantiate(this);
            return clone;
        }
        
        public override void StartWait(StorylinePlayer player, Storyline.Storyline storyline) {}

        public override bool IsCompleted(StorylinePlayer player, Storyline.Storyline storyline) =>
            !player.IsAudioPlaying && !player.IsTextPlaying;
    }
}