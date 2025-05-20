using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes;
using Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.Storyline;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph
{
    public class StorylinePlayer : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Settings")]
        [SerializeField] private StorylineGraph graph;
        public bool manual;
        public bool canSkip = true;
        [FormerlySerializedAs("skip")] [SerializeField] private InputActionReference skipAction;
        [SerializeField] private InputActionReference continueAction;
        [Space]
        public UnityEvent<string> onSentenceStart;
        public UnityEvent<string> onSentenceEnd;
        public UnityEvent<string> onStorylineEnd;
        
        [SerializeField] private bool lazy = true;

        private string currentName;
        private Storyline _current;
        private bool _wait = true;
        private readonly Queue<string> _fastSwap = new();
        private readonly Queue<string> _queue = new();
        private readonly Dictionary<AbstractNode, AbstractNode> _cloneBuffer = new();
        private readonly Queue<AbstractNode> _cloningQueue = new();

        public bool IsStarted => _current != null;
        public bool IsPlaying { get; private set; }

        private void Start()
        {
            if (continueAction?.action != null) continueAction.action.performed += TryGoToNext;
            if (skipAction?.action != null) skipAction.action.performed += TryGoToNext;
        }

        public bool IsCanContinue => IsPlaying && _wait && manual;
        public bool IsCanSkip => IsPlaying && canSkip;
        public void TryGoToNext(InputAction.CallbackContext callbackContext)
        {
            if (!IsCanSkip && !IsCanContinue) return;
            // _current.OnDrawEnd(this);
            // _current.OnDelayStart(this);
            GoToNext();
        }

        public void StartDialogueNow(string rootName)
        {
            var root = graph.roots.Find(r => r.RootName == rootName);
            if (!root) return;

            if (lazy)
            {
                _current = root.Clone() as Storyline;
                _cloneBuffer[root] = _current;
            }
            else _current = StorylineGraph.Clone(root);
            currentName = rootName;
            SwitchUpdate();
            PlayDialogue();
        }
        public void StartDialogue(string rootName) => _fastSwap.Enqueue(rootName);

        public void QueueDialogue(string rootName) => _queue.Enqueue(rootName);

        public void PlayDialogue()
        {
            if (IsPlaying) return;
            IsPlaying = true;
            _current.textPlayer?.PlayDraw(this);
            _current.audioPlayer?.PlayAudio(this);
            IsTextPlaying = true;
        }
        public void PauseDialogue()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            IsTextPlaying = false;
            _current.textPlayer?.PauseDraw(this);
            _current.audioPlayer?.StopAudio(this);
        }

        public void StopDialogue()
        {
            IsTextPlaying = false;
            _current = null;
        }
        public void ClearFastSwap()
        {
            _fastSwap.Clear();
        }
        public void ClearQueue()
        {
            _queue.Clear();
        }

        public void StopAll()
        {
            ClearFastSwap();
            ClearQueue();
            StopDialogue();
        }
        
        public void ToNext()
        {
            if (_current.waitCondition)
            {
                _current.waitCondition.StartWait(this, _current);
                InvokeRepeating(nameof(CheckCompletionCondition), 0.1f, 0.1f);
            }
            else GoToNext();
        }

        private void CheckCompletionCondition()
        {
            if (!_current.waitCondition.IsCompleted(this, _current)) return;
            GoToNext();
            CancelInvoke(nameof(CheckCompletionCondition));
        }

        private void Update()
        {
            if (!_current)
            {
                if (_fastSwap.TryDequeue(out var fastRoot))
                    StartDialogueNow(fastRoot);
                else if (_queue.TryDequeue(out var root)) 
                    StartDialogueNow(root);
            }
            if (!IsPlaying) return;
            
            if (_wait) return;
            if (!_current.textPlayer) return;
            _current.textPlayer.Draw(this);
            _current.audioPlayer?.OnDraw(this);

            if (_current.textPlayer.IsCompleted()) return;
            IsTextPlaying = false;
            // _current.OnDrawEnd(this);
            if (!manual) ToNext();
            _wait = true;
        }

        private void GoToNext()
        {
            // _current.OnDelayEnd(this);
            onSentenceEnd.Invoke(_current.tag);
            _current.textPlayer?.PauseDraw(this);
            _current.audioPlayer?.StopAudio(this);
            if (_fastSwap.TryDequeue(out var root))
            {
                StartDialogueNow(root);
                return;
            }
            _current = _current.GetNext();
            if (lazy)
            {
                if (_cloneBuffer.TryGetValue(_current, out var c)) _current = c as Storyline;
                else
                {
                    _cloneBuffer[_current] = _current.Clone();
                    _cloningQueue.Enqueue(_cloneBuffer[_current]);
                    while (_cloningQueue.Count > 0)
                    {
                        var clone = _cloningQueue.Dequeue();

                        foreach (var field in clone.GetType().GetFields())
                        {
                            if (!field.HasAttribute(typeof(InputPort))) continue;
                            if (field.FieldType.IsGenericType && field.FieldType.GetInterface(nameof(IList)) != null)
                            {
                                if (field.GetValue(clone) is not IList values) continue;
                                var list = (IList)Activator.CreateInstance(field.FieldType);
                                foreach (var value in values)
                                    if (value is AbstractNode abstractNode)
                                    {
                                        if (!abstractNode)
                                        {
                                            list.Add(null);
                                            return;
                                        }
                                        if (!_cloneBuffer.ContainsKey(abstractNode))
                                            _cloneBuffer[abstractNode] = abstractNode.Clone();
                                        _cloningQueue.Enqueue(_cloneBuffer[abstractNode]);
                                        list.Add(_cloneBuffer[abstractNode]);
                                    }

                                field.SetValue(clone, list);
                            }
                            else if (field.GetValue(clone) is AbstractNode abstractNode)
                            {
                                if (!abstractNode) return;
                                if (!_cloneBuffer.ContainsKey(abstractNode))
                                    _cloneBuffer[abstractNode] = abstractNode.Clone();
                                _cloningQueue.Enqueue(_cloneBuffer[abstractNode]);
                                field.SetValue(clone, _cloneBuffer[abstractNode]);
                            }
                        }
                    }

                    _current = _cloneBuffer[_current] as Storyline;
                }

            }
            SwitchUpdate();
        }
        private void SwitchUpdate()
        {
            if (!_current)
            {
                onStorylineEnd.Invoke(currentName);

                if (_fastSwap.TryDequeue(out var fastRoot))
                    StartDialogueNow(fastRoot);
                else if (_queue.TryDequeue(out var root))
                    StartDialogueNow(root);
                else if (lazy) _cloneBuffer.Clear();
                return;
            }
            // _current.OnDrawStart(this);
            _wait = false;
            IsTextPlaying = true;
            onSentenceStart.Invoke(_current.tag);
        }

        public void ShowText(string str) => text.text = str;
        public void ClearText() => text.text = "";
        public bool IsTextPlaying { get; private set; }

        public void PlayAudio(AudioClip clip, float pitch, float volume = 1)
        {
            audioSource.clip = clip;
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.Play();
        }
        public void StopAudio() => audioSource.Stop();
        public void PauseAudio() => audioSource.Pause();
        public void UnPauseAudio() => audioSource.UnPause();
        public bool IsAudioPlaying => audioSource.isPlaying;
    }
}