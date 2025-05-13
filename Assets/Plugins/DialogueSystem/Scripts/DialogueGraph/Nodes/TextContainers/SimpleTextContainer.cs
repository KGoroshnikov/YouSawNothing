using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.TextContainers
{
    [EditorPath("TextContainers")]
    public class SimpleTextContainer : TextContainer
    {
        [SerializeField] private string text;
        public override string Text => text;

        public override AbstractNode Clone()
        {
            var clone = base.Clone() as SimpleTextContainer;
            clone.text = text;
            return clone;
        }
    }
}