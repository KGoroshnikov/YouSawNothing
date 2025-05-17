using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes
{
    [OutputPort(typeof(TextPlayer),"TextPlayer")]
    public abstract class TextPlayer : AbstractNode
    {
        [FormerlySerializedAs("container")]
        [InputPort("Text")]
        [HideInInspector]
        public TextContainer textContainer;

        public abstract void OnDrawStart(StorylinePlayer storylinePlayer, Storyline storyline);
        public abstract void Draw(StorylinePlayer storylinePlayer);
        public abstract bool IsCompleted();
        public abstract void PauseDraw(StorylinePlayer storylinePlayer);
        public abstract void PlayDraw(StorylinePlayer storylinePlayer);
    }
}