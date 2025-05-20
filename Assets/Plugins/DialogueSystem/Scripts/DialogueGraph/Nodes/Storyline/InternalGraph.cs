using System.Collections.Generic;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Storyline
{
    public class InternalGraph : Storyline
    {
        [SerializeField] private StorylineGraph graph;
        [SerializeField] private string rootName;

        private Storyline entrypoint;
        public override Storyline GetNext()
        {
            var start = graph.roots.Find(root => root.RootName == rootName);
            if (entrypoint) return entrypoint;
            entrypoint = start.Clone() as Storyline;
            var curr = entrypoint;
            while (curr.GetNext())
            {
                var prev = curr;
                curr = curr.GetNext().Clone() as Storyline;
                prev.SetNext(curr);
            }
            curr.SetNext(base.GetNext());
            return entrypoint;
        }
        public override AbstractNode Clone()
        {
            var node = base.Clone() as InternalGraph;
            node.graph = graph;
            node.rootName = rootName;
            node.entrypoint = entrypoint;
            return node;
        }
    }
}