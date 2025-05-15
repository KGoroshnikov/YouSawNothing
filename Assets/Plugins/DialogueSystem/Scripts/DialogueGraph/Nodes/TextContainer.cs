using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [OutputPort(typeof(TextContainer),"Text")]
    public abstract class TextContainer : AbstractNode
    {
        public abstract string Text { get; }
    }
}