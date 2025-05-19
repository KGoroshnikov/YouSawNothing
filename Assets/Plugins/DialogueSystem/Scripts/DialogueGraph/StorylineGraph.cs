using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Storyline;
using Unity.VisualScripting;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph
{
    [CreateAssetMenu(fileName = "Storyline Graph")]
    public class StorylineGraph : ScriptableObject
    {
        [HideInInspector] public List<StorylineStart> roots = new();
        [HideInInspector] public List<AbstractNode> nodes = new();

        public static Storyline Clone(Storyline node)
        {
            var clones = new Dictionary<AbstractNode, AbstractNode>();
            var queue = new Queue<AbstractNode>();
            
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var n = queue.Dequeue();
                if (clones.ContainsKey(n)) continue;
                clones[n] = n.Clone();
                foreach (var field in n.GetType().GetFields())
                {
                    if (!field.HasAttribute(typeof(InputPort))) continue;
                    if (field.FieldType.IsGenericType && field.FieldType.GetInterface(nameof(IList)) != null)
                    {
                        if (field.GetValue(n) is not IList values) continue;
                        foreach (var value in values)
                            if (value is AbstractNode abstractNode) 
                                queue.Enqueue(abstractNode);
                    }
                    else
                    {
                        if (field.GetValue(n) is AbstractNode abstractNode)
                            queue.Enqueue(abstractNode);
                    }
                }
                
                if (n is not Storyline dialogueNode) continue;
                queue.Enqueue(dialogueNode.GetNext());
            }
            
            var completed = new List<AbstractNode>();
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var n = queue.Dequeue();
                if (completed.Contains(n)) continue;
                completed.Add(n);

                var clone = clones[n];
                if (!clone) continue;

                foreach (var field in n.GetType().GetFields())
                {
                    if (!field.HasAttribute(typeof(InputPort))) continue;
                    
                    if (field.FieldType.IsGenericType && field.FieldType.GetInterface(nameof(IList)) != null)
                    {
                        if (field.GetValue(n) is not IList values) continue;
                        var list =  (IList) Activator.CreateInstance(field.FieldType);
                        foreach (var value in values)
                            if (value is AbstractNode abstractNode) 
                                list.Add(abstractNode ? clones[abstractNode] : null);
                        field.SetValue(clone, list);
                    }
                    else
                    {
                        var value = field.GetValue(n) as AbstractNode;
                        field.SetValue(clone, value ? clones[value] : null);
                    }
                }

                if (n is not Storyline storyline) continue;
                var storylineClone = clone as Storyline;
                storylineClone?.SetNext(storyline.GetNext() ? clones[storyline.GetNext()] as Storyline : null);
                if (storyline.GetNext()) queue.Enqueue(storyline.GetNext());
            }

            return clones[node] as Storyline;
        }
    }
}