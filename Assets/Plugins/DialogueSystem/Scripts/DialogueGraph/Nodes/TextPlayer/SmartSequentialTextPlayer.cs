using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using Plugins.DialogueSystem.Scripts.Utils;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.DrawerNodes
{
    [EditorPath("Drawers")]
    public class SmartSequentialTextPlayer : TextPlayer
    {
        [SerializeField] private string narrator;
        [SerializeField] private UDictionary<char, float> symbolTime;
        [SerializeField] private float defaultSymbolTime;

        private string _currentText;
        private float _timeLimit;
        private float _time;
        private int _index;
        public override AbstractNode Clone()
        {
            var node = Instantiate(this);
            node.narrator = narrator;
            node.symbolTime = new UDictionary<char, float>();
            foreach (var pair in symbolTime) 
                node.symbolTime.Add(pair.Key, pair.Value);
            node.defaultSymbolTime = defaultSymbolTime;
            return node;
        }
        
        public override void OnDrawStart(StorylinePlayer storylinePlayer, Storyline storyline)
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
            _timeLimit = symbolTime.TryGetValue(key, out var value) ? value : defaultSymbolTime;
        }

        public override void Draw(StorylinePlayer storylinePlayer)
        {
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