using UnityEngine.UIElements;

namespace Editor.DialogueSystem
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        private UnityEditor.Editor _editor;

        public void UpdateSelection(NodeView view)
        {
            Clear();
            if (view == null || view.node == null) return;
            _editor = UnityEditor.Editor.CreateEditor(view.node);
            Add(new IMGUIContainer(() => _editor.OnInspectorGUI()));
        }
    }
}