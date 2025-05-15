using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.TextPlayers
{
    [EditorPath("Text Players")]
    public class SequentialTextPlayer : TextPlayer
    {
        [SerializeField] private float time = 1;

        private string _currentText;
        private float _time;

        public override AbstractNode Clone()
        {
            var node = Instantiate(this);
            node.time = time;
            return node;
        }

        public override void OnDrawStart(StorylinePlayer storylinePlayer, Storyline storyline)
        {
            _currentText = textContainer.Text;
            _time = 0;
        }

        public override void Draw(StorylinePlayer storylinePlayer)
        {
            PlayDraw(storylinePlayer);
            _time += Time.deltaTime;
        }

        public override void PauseDraw(StorylinePlayer storylinePlayer) => 
            storylinePlayer.ClearText();

        public override void PlayDraw(StorylinePlayer storylinePlayer) => 
            storylinePlayer.ShowText(_currentText[..(int) ((_currentText.Length + 1) * _time / time)]);

        public override bool IsCompleted() => _time > time;
    }
}