using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [OutputPort(typeof(WaitCondition),"WaitCondition")]
    public abstract class WaitCondition : AbstractNode
    {
        public abstract void StartWait(StorylinePlayer player, Storyline.Storyline storyline);
        public abstract bool IsCompleted(StorylinePlayer player, Storyline.Storyline storyline);
    }
}