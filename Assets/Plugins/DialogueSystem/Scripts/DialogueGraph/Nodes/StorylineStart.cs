using System;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [Serializable]
    public class StorylineStart : Storyline
    {
        [SerializeField] private string rootName;
        public string RootName => rootName;
    }
}