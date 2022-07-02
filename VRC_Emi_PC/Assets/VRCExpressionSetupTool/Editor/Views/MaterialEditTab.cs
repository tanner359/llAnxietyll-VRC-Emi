using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRCExpressionSetupTool.Editor.ViewModels;

namespace VRCExpressionSetupTool.Editor.Views
{
    internal class MaterialEditTab : TabBase, INavigationAware
    {
        private readonly List<Material> materials;
        private ExpressionSetupWindowViewModel viewModel;
        private bool isInitialized;
        private Vector2 scrollPosition;

        public MaterialEditTab(EditorWindow parentWindow) : base(parentWindow)
        {
            this.Title = "Material Parameter";
            this.materials = new List<Material>();
        }
        
        public override void Initialize(ExpressionSetupWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.materials.Clear();
            
            if (viewModel == null || viewModel.AvatarDescriptor == null)
            {
                this.isInitialized = false;
                return;
            }

            var renderers = viewModel.AvatarDescriptor.GetComponentsInChildren<Renderer>();
            if(renderers == null)
            {
                this.isInitialized = false;
                return;
            }
            
            
            foreach (var renderer in renderers)
            {
                var m = new List<Material>();
                renderer.GetMaterials(m);
                
                this.materials.AddRange(m);
            }

            this.isInitialized = this.materials.Count != 0;
        }

        public override void OnInspectorGUI()
        {
            if (!this.isInitialized)
            {
                EditorGUILayout.LabelField("Does not contain Mesh Renderer or Materials");
                return;
            }

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            foreach (var material in this.materials)
            {
                EditorGUILayout.LabelField(material.name, EditorStyles.boldLabel);
                ShaderParameterGUI(material);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private static void ShaderParameterGUI(Material material)
        {
            EditorGUI.indentLevel++;
            for (var i = 0; i < ShaderUtil.GetPropertyCount(material.shader); i++)
            {
                if(ShaderUtil.IsShaderPropertyHidden(material.shader, i)) continue;
                
                var propertyName = ShaderUtil.GetPropertyName(material.shader, i);

                var type = ShaderUtil.GetPropertyType(material.shader, i);
                switch (type)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                        material.SetFloat(propertyName, EditorGUILayout.FloatField(propertyName, material.GetFloat(propertyName)));
                        break;
                    case ShaderUtil.ShaderPropertyType.Range:
                        break;
                }
            }
            EditorGUI.indentLevel--;
        }

        private static float PreviewSlider(string title, float value, float min = -1.0f, float max = 1.0f)
        {
            var result = EditorGUILayout.Slider(title, value, min, max);
            if (result > -0.100000001490116 && result < 0.100000001490116) return 0.0f;
            return result;
        }

        public void OnNavigatedTo()
        {
            
        }

        public void OnNavigatedFrom()
        {
            
        }
    }
}