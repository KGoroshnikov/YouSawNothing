using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueSystem
{
    public class DialogueGraphEditor : EditorWindow
    {
        private DialogueGraphView _graphView;
        private InspectorView _inspectorView;
        

        [MenuItem("Window/Dialogue Graph Editor")]
        public static void OpenWindow()
        {
            DialogueGraphEditor wnd = GetWindow<DialogueGraphEditor>();
            wnd.titleContent = new GUIContent("DialogueGraphEditor");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/DialogueSystem/DialogueGraphEditor.uxml");
            visualTree.CloneTree(root);
        
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DialogueSystem/DialogueGraphEditor.uss");
            root.styleSheets.Add(styleSheet);

            _graphView = root.Q<DialogueGraphView>();
            _inspectorView = root.Q<InspectorView>();

            var save = root.Q<ToolbarButton>("save");
            if (save != null)
                save.clickable.clicked += _graphView.Save;
            
            
            _graphView.onNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            var graph = Selection.activeObject as Plugins.DialogueSystem.Scripts.DialogueGraph.StorylineGraph;
            if (graph && AssetDatabase.CanOpenAssetInEditor(graph.GetInstanceID())) 
                _graphView.PopulateView(graph);
            
        }

        private void OnNodeSelectionChanged(NodeView view)
        {
            _inspectorView.UpdateSelection(view);
        }
    }
}
