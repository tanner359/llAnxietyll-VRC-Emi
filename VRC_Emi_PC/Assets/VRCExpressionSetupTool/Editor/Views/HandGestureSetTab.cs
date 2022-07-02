using UnityEditor;
using UnityEngine;
using VRCExpressionSetupTool.Editor.ViewModels;

namespace VRCExpressionSetupTool.Editor.Views
{
    public class HandGestureSetTab : TabBase, INavigationAware
    {
        private HandGestureSetView rightHandGestureSetView;
        private HandGestureSetView leftHandGestureSetView;
        
        private Vector2 scrollPos;

        public HandGestureSetTab(EditorWindow parentWindow) : base(parentWindow)
        {
            this.Title = "Set Hand Gesture";
        }

        public override void Initialize(ExpressionSetupWindowViewModel viewModel)
        {
            if (viewModel.GestureAnimatorController == null ||
                viewModel.FxAnimatorController == null) return;
            this.rightHandGestureSetView = new HandGestureSetView("Right Hand", viewModel.RightExpressionGestureDataList);
            this.leftHandGestureSetView = new HandGestureSetView("Left Hand", viewModel.LeftExpressionGestureDataList);
        }

        public override void OnInspectorGUI()
        {
            using (var scope = new GUILayout.ScrollViewScope(this.scrollPos))
            {
                this.scrollPos = scope.scrollPosition;
                
                this.leftHandGestureSetView?.OnGUI();
                this.rightHandGestureSetView?.OnGUI();
            }
        }

        public void OnNavigatedTo()
        {
            
        }

        public void OnNavigatedFrom()
        {
            
        }
    }
}