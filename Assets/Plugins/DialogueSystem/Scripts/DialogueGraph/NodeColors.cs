using System;
using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph
{
    public static class NodeColors
    {
        static NodeColors()
        {
            Colors.Add(typeof(Storyline), new Color(0, 0.5f, 0));
            Colors.Add(typeof(TextContainer), new Color(0.75f, 0, 0));
            Colors.Add(typeof(TextPlayer), new Color(0.8f, 0.4f, 0));
        }
        public static readonly Dictionary<Type, Color> Colors = new();
        
    }
}