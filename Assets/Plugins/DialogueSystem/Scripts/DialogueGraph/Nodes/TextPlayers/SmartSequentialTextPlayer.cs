using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.TextPlayers
{
    [EditorPath("Text Players")]
    public class SmartSequentialTextPlayer : TextPlayer
    {
        [SerializeField] private UDictionary<string, float> symbolTime;
        [SerializeField] private float defaultSymbolTime;

        private string _currentText;
        private float _timeLimit;
        private float _time;
        private int _index;
        public override AbstractNode Clone()
        {
            var node = Instantiate(this);
            node.symbolTime = new UDictionary<string, float>();
            foreach (var pair in symbolTime) 
                node.symbolTime.Add(pair.Key, pair.Value);
            node.defaultSymbolTime = defaultSymbolTime;
            return node;
        }
        
        public override void OnDrawStart(StorylinePlayer storylinePlayer, Storyline.Storyline storyline)
        {
            _currentText = textContainer.Text;
            _time = 0;
            _index = 0;
            ComputeLimit();
        }

        private void ComputeLimit()
        {
            if (IsCompleted())
            {
                _timeLimit = 0;
                return;
            }
            var key = _currentText[_index];
            _timeLimit = defaultSymbolTime;
            foreach (var pair in symbolTime)
                if (pair.Key.Contains(key))
                    _timeLimit = pair.Value;
        }

        public override void Draw(StorylinePlayer storylinePlayer)
        {
            if (IsCompleted()) return;
            PlayDraw(storylinePlayer);
            
            _time += Time.deltaTime;
            if (_time < _timeLimit) return;
            while (_time >= _timeLimit)
            {
                _index++;
                _time -= _timeLimit;
            }
            ComputeLimit();
        }

        public override void PauseDraw(StorylinePlayer storylinePlayer) => 
            storylinePlayer.ClearText();

        public override void PlayDraw(StorylinePlayer storylinePlayer)
        {
            if (!IsCompleted())
                storylinePlayer.ShowText(_currentText[..(_index + 1)]);
        }

        public override bool IsCompleted() => _index >= _currentText.Length;
    }
}