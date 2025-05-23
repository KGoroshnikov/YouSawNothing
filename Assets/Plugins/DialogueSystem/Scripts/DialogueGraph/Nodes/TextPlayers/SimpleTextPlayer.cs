using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.TextPlayers
{
    [EditorPath("Text Players")]
    public class SimpleTextPlayer : TextPlayer
    {

        public override AbstractNode Clone() => Instantiate(this);
        public override void OnDrawStart(StorylinePlayer storylinePlayer, Storyline.Storyline storyline) {}
        public override void Draw(StorylinePlayer storylinePlayer) => storylinePlayer.ShowText(textContainer.Text);
        public override void PauseDraw(StorylinePlayer storylinePlayer) => storylinePlayer.ClearText();
        public override void PlayDraw(StorylinePlayer storylinePlayer) => Draw(storylinePlayer);
        public override bool IsCompleted() => true;
    }
}