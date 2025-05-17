using System;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OutputPort : Attribute
    {
        public readonly Type type;
        public readonly string name;
        public OutputPort(Type type, string name = null)
        {
            this.type = type;
            this.name = name;
        }
    }
}