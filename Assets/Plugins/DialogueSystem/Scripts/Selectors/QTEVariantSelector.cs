using UnityEngine;
using UnityEngine.Events;

namespace Plugins.DialogueSystem.Scripts.Selectors
{
    public class QTEVariantSelector : VariantSelector
    {
        [SerializeField] private float decisionTime = 10;
        [SerializeField] private UnityEvent onQteFailure;
        public override void Show()
        {
            base.Show();
            Invoke(nameof(Failure), decisionTime);
        }

        public override void Hide()
        {
            CancelInvoke(nameof(Failure));
            base.Hide();
        }

        private void Failure()
        {
            Hide();
            onQteFailure.Invoke();
        }
    }
}