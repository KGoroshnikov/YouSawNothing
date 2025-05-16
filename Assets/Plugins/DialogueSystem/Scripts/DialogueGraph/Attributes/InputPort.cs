using System;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InputPort : Attribute
    {
        public readonly string name;
        public InputPort(string name = null)
        {
            this.name = name;
        }
    }
}