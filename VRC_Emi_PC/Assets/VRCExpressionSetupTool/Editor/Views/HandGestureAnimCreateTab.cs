using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRCExpressionSetupTool.Editor.ViewModels;

namespace VRCExpressionSetupTool.Editor.Views
{
    public class HandGestureAnimCreateTab : TabBase, INavigationAware
    {
        private readonly BlendShapeEditTab blendShapeEditTab;
        private readonly SimpleBodyShapeEditTab simpleBodyShapeEditTab;
        // private readonly MaterialEditTab materialEditTab;
        private readonly List<TabBase> tabs;
        private GameObject originalRoot;
        private int previousTabIndex;
        private int selectedTabIndex;
        private GUIStyle tabStyle;
        private ExpressionSetupWindowViewModel viewModel;

        public HandGestureAnimCreateTab(EditorWindow parentWindow) : base(parentWindow)
        {
            this.blendShapeEditTab = new BlendShapeEditTab(parentWindow);
            this.simpleBodyShapeEditTab = new SimpleBodyShapeEditTab(parentWindow);
            // this.materialEditTab = new MaterialEditTab(parentWindow);
            this.tabs = new List<TabBase>
            {
                this.blendShapeEditTab,
                this.simpleBodyShapeEditTab,
                // this.materialEditTab
            };

            this.Title = "Create Hand Gesture";
        }

        public override void Initialize(ExpressionSetupWindowViewModel viewModel)
        {
            this.viewModel = viewModel;

            if (this.tabStyle == null)
            {
                var skin = AssetDatabase.LoadAssetAtPath<GUISkin>(
                    AssetDatabase.GUIDToAssetPath("e7302c47c6c9907478ffd09600146277"));
                this.tabStyle = skin.GetStyle("Tab");
            }

            foreach (var tab in this.tabs) tab.Initialize(viewModel);
        }

        public override void OnInspectorGUI()
        {
            var tabTitles = this.tabs.Select(x => x.Title).ToArray();
            this.previousTabIndex = this.selectedTabIndex;
            this.selectedTabIndex = GUILayout.Toolbar(this.selectedTabIndex, tabTitles, this.tabStyle);
            this.TabNavigation();
            this.tabs[this.selectedTabIndex].OnInspectorGUI();
            GUILayout.FlexibleSpace();
            
            EditorGUI.BeginDisabledGroup(this.viewModel == null || this.viewModel.AvatarDescriptor == null);
            if (GUILayout.Button("Create Animation Clip"))
            {
                this.CreateAnimationClip();
            }
            EditorGUI.EndDisabledGroup();
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
        }
        
        private void TabNavigation()
        {
            if (this.previousTabIndex == this.selectedTabIndex) return;
            if (this.tabs[this.previousTabIndex] is INavigationAware navigatedTab)
            {
                navigatedTab.OnNavigatedFrom();
            }

            navigatedTab = this.tabs[this.selectedTabIndex] as INavigationAware;
            navigatedTab?.OnNavigatedTo();
        }
        
        public override void OnDestroy()
        {
            foreach (var tab in this.tabs) tab.OnDestroy();
        }

        public void OnNavigatedTo()
        {
            foreach (var tab in this.tabs) tab.Initialize(this.viewModel);
        }

        public void OnNavigatedFrom()
        {
            if (!this.tabs.Any(x => x.Modified)) return;
            if (!EditorUtility.DisplayDialog("", "Discard changes ?", "OK", "Cancel")) return;
            foreach (var tab in this.tabs) tab.OnDestroy();
        }
        
        private void CreateAnimationClip()
        {
            var savePath = EditorUtility.SaveFilePanel("Save Animation Clip", GetCurrentDirectory(), "anim", "anim");
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            var clip = new AnimationClip();
            clip = this.blendShapeEditTab.CreateAnimationClip(clip);
            clip = this.simpleBodyShapeEditTab.CreateAnimationClip(clip);

            AssetDatabase.CreateAsset(clip, FileUtil.GetProjectRelativePath(savePath));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static string GetCurrentDirectory()
        {
            const BindingFlags FLAG = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            var asm = Assembly.Load("UnityEditor.dll");
            var typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
            var projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser);
            return (string)typeProjectBrowser.GetMethod("GetActiveFolderPath", FLAG)?.Invoke(projectBrowserWindow, null); 
        }
    }
}