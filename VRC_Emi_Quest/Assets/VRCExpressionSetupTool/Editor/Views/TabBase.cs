using UnityEditor;
using UnityEngine;
using VRCExpressionSetupTool.Editor.ViewModels;

namespace VRCExpressionSetupTool.Editor.Views
{
    public class TabBase
    {
        protected readonly EditorWindow ParentWindow;

        public TabBase(EditorWindow parentWindow)
        {
            this.ParentWindow = parentWindow;
        }

        public string Title { get; set; }

        public bool Modified { get; set; }

        public virtual void Initialize(ExpressionSetupWindowViewModel viewModel) { }

        public virtual void OnInspectorGUI() { }
        
        // public virtual void OnHandleRenderer(){}
        // public virtual void Update() { }

        public virtual void OnDestroy() { }
    }
}