using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    public abstract class AbstractNode : ScriptableObject
    {
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 nodePos;
        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt, Action onGraphViewUpdate)
        {
            
        }

        public virtual AbstractNode Clone()
        {
            return Instantiate(this);
        }
    }
}