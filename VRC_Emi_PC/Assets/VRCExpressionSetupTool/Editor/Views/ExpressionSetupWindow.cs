#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRCExpressionSetupTool.Editor.ViewModels;
using VRCExpressionSetupTool.Editor.Views.Layout;

namespace VRCExpressionSetupTool.Editor.Views
{
    public class ExpressionSetupWindow : EditorWindow
    {
        private readonly ExpressionSetupWindowViewModel viewModel;
        private readonly List<TabBase> tabs;
        private readonly string[] tabTitles;
        
        private int previousTabIndex;
        private int selectedTabIndex;

        private bool avatarFieldFold = true;
        private Vector2 scrollPos;
        
        private GUIStyle tabStyle;

        public ExpressionSetupWindow()
        {
            this.viewModel = new ExpressionSetupWindowViewModel();
            
            this.tabs = new List<TabBase>
            {
                new HandGestureSetTab(this),
                new HandGestureAnimCreateTab(this)
            };

            this.tabTitles = this.tabs.Select(x => x.Title).ToArray();
        }

        [MenuItem("Window/ExpressionSetup")]
        private static void ShowWindow()
        {
            var window = GetWindow<ExpressionSetupWindow>();
            window.minSize = new Vector2(710, 100);
            window.titleContent = new GUIContent("Expression Setup", (Texture2D)EditorGUIUtility.Load("d_ViewToolOrbit"));
            window.Show();
        }
        
        private void OnEnable()
        {
            var skin = AssetDatabase.LoadAssetAtPath<GUISkin>(AssetDatabase.GUIDToAssetPath("e7302c47c6c9907478ffd09600146277"));
            this.tabStyle = skin.GetStyle("Tab");
        }

        private void OnGUI()
        {
            using (var scope = new EditorCustomGUILayout.ObjectFieldFoldGroupScope<VRCAvatarDescriptor>("Avatar",
                this.viewModel.AvatarDescriptor, this.avatarFieldFold, true, true,
                x =>
                {
                    this.viewModel.AvatarDescriptor = x;
                    foreach (var tab in this.tabs) tab.Initialize(this.viewModel);
                }, () => this.viewModel.AvatarDescriptor != null))
            {
                this.avatarFieldFold = scope.Fold;
                if (this.avatarFieldFold)
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        using (new EditorGUI.DisabledScope(this.viewModel.AvatarDescriptor == null))
                        {
                            EditorCustomGUILayout.ObjectField("FX Controller", this.viewModel.FxAnimatorController,
                                false,
                                true,
                                x =>
                                {
                                    this.viewModel.FxAnimatorController = x;
                                    this.tabs[0].Initialize(this.viewModel);
                                },
                                () => this.viewModel.FxAnimatorController != null);

                            EditorCustomGUILayout.ObjectField("Gesture Controller",
                                this.viewModel.GestureAnimatorController, false, true,
                                x =>
                                {
                                    this.viewModel.GestureAnimatorController = x;
                                    this.tabs[0].Initialize(this.viewModel);
                                },
                                () => this.viewModel.GestureAnimatorController != null);
                            
                            this.viewModel.WritesDefault = EditorGUILayout.Toggle("Writes Default", this.viewModel.WritesDefault);
                        }

                        using (new EditorGUI.DisabledGroupScope(!this.viewModel.CreateControllersCommand.CanExecute()))
                        {
                            if (GUILayout.Button("Create Layer"))
                            {
                                this.viewModel.CreateControllersCommand.Execute();
                                this.tabs[0].Initialize(this.viewModel);
                            }
                        }
                        
                    }
                }
            }

            EditorStyles.label.fontStyle = FontStyle.Normal;

            GUILayout.Space(10);
            
            this.previousTabIndex = this.selectedTabIndex;
            this.selectedTabIndex = GUILayout.Toolbar(this.selectedTabIndex, this.tabTitles, this.tabStyle);

            this.TabNavigation();
            
            this.tabs[this.selectedTabIndex].OnInspectorGUI();
            
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
        
        private void OnDestroy()
        {
            foreach (var tab in this.tabs) tab.OnDestroy();
        }
    }
}
#endif