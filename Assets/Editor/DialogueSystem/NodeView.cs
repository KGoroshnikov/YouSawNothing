using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Plugins.DialogueSystem.Scripts.DialogueGraph;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueSystem
{
    public class NodeView : Node
    {
        public Action<NodeView> onNodeSelected;
        public Action onGraphViewUpdate;
        
        public readonly AbstractNode node;
        
        public FieldInfo[] InputFields { get; private set; }
        public int shift;
        public Port[] Inputs { get; private set; }
        public  Port[] Outputs { get; private set; }
        public NodeView(AbstractNode node)
        {
            this.node = node;
            title = this.node.name;
            viewDataKey = this.node.guid;

            style.left = this.node.nodePos.x;
            style.top = this.node.nodePos.y;

            CreateInputPorts();
            CreateOutputPorts();
        }

        private void CreateInputPorts()
        {
            switch (node)
            {
                case Storyline dialogue:
                    shift = 1;
                    InputFields = node.GetType().GetFields().Where(field => field.HasAttribute(typeof(InputPort))).ToArray();
                    Inputs = new Port[InputFields.Length + 1];
                    if (dialogue is not StorylineStart)
                    {
                        Inputs[0] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi,
                            typeof(Storyline));
                        Inputs[0].portColor = GetColor(typeof(Storyline));
                        inputContainer.Add(Inputs[0]);
                    }
                    for (var i = 1; i <= InputFields.Length; i++)
                    {
                        var inputPort = InputFields[i - 1].GetAttribute<InputPort>();

                        Port.Capacity capacity;
                        var type = InputFields[i - 1].FieldType;
                        if (type.IsGenericType && type.GetInterface(nameof(IList)) != null)
                        {
                            capacity = Port.Capacity.Multi;
                            type = type.GetGenericArguments()[0];
                        }
                        else capacity = Port.Capacity.Single;

                        Inputs[i] = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
                        Inputs[i].portColor = GetColor(type);
                        if (inputPort.name != null) Inputs[i].portName = inputPort.name;
                        inputContainer.Add(Inputs[i]);
                    }
                    
                    return;
                default:
                    shift = 0;
                    InputFields = node.GetType().GetFields().Where(field => field.HasAttribute(typeof(InputPort))).ToArray();
                    Inputs = new Port[InputFields.Length];
                    for (var i = 0; i < InputFields.Length; i++)
                    {
                        var inputPort = InputFields[i].GetAttribute<InputPort>();

                        Port.Capacity capacity;
                        var type = InputFields[i].FieldType;
                        if (type.IsGenericType && type.GetInterface(nameof(IList)) != null)
                        {
                            capacity = Port.Capacity.Multi;
                            type = type.GetGenericArguments()[0];
                        }
                        else capacity = Port.Capacity.Single;

                        Inputs[i] = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, type);
                        Inputs[i].portColor = GetColor(type);
                        if (inputPort.name != null) Inputs[i].portName = inputPort.name;
                        inputContainer.Add(Inputs[i]);
                    }
                    return;
            }
        }

        private void CreateOutputPorts()
        {
            Outputs = new[] {
                InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, node.GetType())
            };
            if (node.GetType().HasAttribute(typeof(OutputPort)))
            {
                var outputPort = node.GetType().GetAttribute<OutputPort>();
                Outputs[0].portType = outputPort.type;
                Outputs[0].portColor = GetColor(outputPort.type);
                Outputs[0].portName = outputPort.name;
            }

            outputContainer.Add(Outputs[0]);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            node.BuildContextualMenu(evt, onGraphViewUpdate);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            node.nodePos.x = newPos.xMin;
            node.nodePos.y = newPos.yMin;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            onNodeSelected?.Invoke(this);
        }

        private static Color GetColor(Type type)
        {
            var t = type;
            while (t != null)
            {
                if (NodeColors.Colors.TryGetValue(t, out var color))
                    return color;
                t = t.BaseType;
            }
            return new Color(1, 1, 1);
        }
    }
}