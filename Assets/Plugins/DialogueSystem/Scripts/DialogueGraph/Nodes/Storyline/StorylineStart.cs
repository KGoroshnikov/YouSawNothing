using System;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Storyline
{
    [Serializable]
    public class StorylineStart : Storyline
    {
        [SerializeField] private string rootName;
        public string RootName => rootName;
        public override AbstractNode Clone()
        {
            var node = base.Clone() as StorylineStart;
            node.rootName = rootName;
            return node;
        }
    }
}