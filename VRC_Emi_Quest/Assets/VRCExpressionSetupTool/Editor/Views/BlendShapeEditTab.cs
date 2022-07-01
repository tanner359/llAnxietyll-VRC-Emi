using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRCExpressionSetupTool.Editor.ViewModels;

namespace VRCExpressionSetupTool.Editor.Views
{
    internal class BlendShapeEditTab : TabBase
    {
        private BlendShapeTreeView blendShapeTreeView;
        private List<List<float>> initialBlendShapeValues;
        private bool isInitialized;
        
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        private bool excludeZero = true;

        public BlendShapeEditTab(EditorWindow parentWindow) : base(parentWindow)
        {
            this.Title = "BlendShape";
        }

        public override void Initialize(ExpressionSetupWindowViewModel viewModel)
        {
            if (viewModel == null || viewModel.AvatarDescriptor == null)
            {
                this.isInitialized = false;
                return;
            }
            
            this.skinnedMeshRenderers = viewModel.AvatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>();

            this.isInitialized = this.skinnedMeshRenderers != null && this.skinnedMeshRenderers.Length > 0 &&
                                 this.skinnedMeshRenderers.Max(x => x.sharedMesh.blendShapeCount) > 0;

            if (!this.isInitialized)
            {
                return;
            }

            this.initialBlendShapeValues = new List<List<float>>();
            var meshRenderers = this.skinnedMeshRenderers;
            if (meshRenderers == null) return;
            
            foreach (var skinnedMeshRenderer in meshRenderers)
            {
                var internalList = new List<float>();
                for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    internalList.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));
                this.initialBlendShapeValues.Add(internalList);
            }

            var state = new TreeViewState();
            var header = new BlendShapeTableHeader(null);
            this.blendShapeTreeView = new BlendShapeTreeView(state, header, meshRenderers);
        }

        public override void OnInspectorGUI()
        {
            if (!this.isInitialized)
            {
                EditorGUILayout.LabelField("Does not contain Skinned Mesh Renderer or Blend Shapes");
                return;
            }

            this.excludeZero = EditorGUILayout.Toggle("Exclude Zero Values", this.excludeZero);
            
            var rect = GUILayoutUtility.GetLastRect();
            rect.y += EditorGUIUtility.singleLineHeight * 2;
            rect.height = this.ParentWindow.position.height - rect.y - EditorGUIUtility.singleLineHeight * 2 - 5;
            this.blendShapeTreeView.OnGUI(rect);

            for (var i = 0; i < this.skinnedMeshRenderers.Length; i++)
            for (var i1 = 0; i1 < this.skinnedMeshRenderers[i].sharedMesh.blendShapeCount; i1++)
            {
                this.Modified = !Mathf.Approximately(this.initialBlendShapeValues[i][i1],
                    this.skinnedMeshRenderers[i].GetBlendShapeWeight(i1));
                if (this.Modified)
                {
                    break;
                }
            }
        }

        public override void OnDestroy()
        {
            if (!this.isInitialized || this.skinnedMeshRenderers == null)
            {
                return;
            }

            for (var i = 0; i < this.skinnedMeshRenderers.Length; i++)
            {
                var skinnedMeshRenderer = this.skinnedMeshRenderers[i];
                if(skinnedMeshRenderer == null) continue;
                for (var i1 = 0; i1 < skinnedMeshRenderer.sharedMesh.blendShapeCount; i1++)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(i1, this.initialBlendShapeValues[i][i1]);
                }
            }
        }

        public AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            if (!this.isInitialized)
            {
                return animationClip;
            }

            var blendShapeTreeItems = this.blendShapeTreeView
                .GetAllRows()
                .OfType<BlendShapeTreeItem>()
                .Where(x => x.BlendShapeTreeElement.IsExport);
            foreach (var blendShapeTreeItem in blendShapeTreeItems)
            {
                var blendShapeTreeElement = blendShapeTreeItem.BlendShapeTreeElement;
                var skinnedMeshRenderer =
                    this.skinnedMeshRenderers.First(x => x.name == blendShapeTreeElement.MeshName);

                var path = GetHierarchyPath(skinnedMeshRenderer.transform);
                if(this.excludeZero && Mathf.Approximately(blendShapeTreeElement.BlendShapeValue, 0)) continue;
                    
                var curveBinding = new EditorCurveBinding
                {
                    type = typeof(SkinnedMeshRenderer),
                    path = path,
                    propertyName = "blendShape." + blendShapeTreeElement.BlendShapeName
                };

                var curve = new AnimationCurve();
                curve.AddKey(0f, blendShapeTreeElement.BlendShapeValue);
                curve.AddKey(0.01f, blendShapeTreeElement.BlendShapeValue);

                AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
            }

            return animationClip;
        }

        private static string GetHierarchyPath(Transform self)
        {
            var path = self.gameObject.name;
            var parent = self.parent;
            while (parent.parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}