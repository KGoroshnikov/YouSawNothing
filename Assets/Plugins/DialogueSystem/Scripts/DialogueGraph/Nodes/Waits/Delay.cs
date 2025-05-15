using System;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Waits
{
    [EditorPath("Wait Conditions")]
    public class Delay : WaitCondition
    {
        public float delay = 1;
        public override AbstractNode Clone()
        {
            var clone = base.Clone() as Delay;
            clone.delay = delay;
            return clone;
        }
        
        private DateTime StartTime;
        public override void StartWait(StorylinePlayer player, Storyline storyline) => StartTime = DateTime.Now;

        public override bool IsCompleted(StorylinePlayer player, Storyline storyline) =>
            DateTime.Now.Subtract(StartTime).TotalSeconds > delay;
    }
}