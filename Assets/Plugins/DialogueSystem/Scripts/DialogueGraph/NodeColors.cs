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
            Colors.Add(typeof(TextPlayer), new Color(0, 0.2f, 0.8f));
            
            Colors.Add(typeof(AudioContainer), new Color(0.8f, 0.4f, 0));
            Colors.Add(typeof(AudioPlayer), new Color(0.5f, 0.9f, 0.2f));
            Colors.Add(typeof(WaitCondition), new Color(0.5f, 0.0f, 0.7f));
        }
        public static readonly Dictionary<Type, Color> Colors = new();
        
    }
}