using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRCExpressionSetupTool.Editor.ViewModels;
using VRCExpressionSetupTool.Editor.Views.Layout;
#pragma warning disable 649

namespace VRCExpressionSetupTool.Editor.Views
{
    internal class SimpleBodyShapeEditTab : TabBase, INavigationAware
    {
        private readonly string[] muscleSpecialNames =
        {
            "LeftHand.Thumb.1 Stretched",
            "LeftHand.Thumb.Spread",
            "LeftHand.Thumb.2 Stretched",
            "LeftHand.Thumb.3 Stretched",
            "LeftHand.Index.1 Stretched",
            "LeftHand.Index.Spread",
            "LeftHand.Index.2 Stretched",
            "LeftHand.Index.3 Stretched",
            "LeftHand.Middle.1 Stretched",
            "LeftHand.Middle.Spread",
            "LeftHand.Middle.2 Stretched",
            "LeftHand.Middle.3 Stretched",
            "LeftHand.Ring.1 Stretched",
            "LeftHand.Ring.Spread",
            "LeftHand.Ring.2 Stretched",
            "LeftHand.Ring.3 Stretched",
            "LeftHand.Little.1 Stretched",
            "LeftHand.Little.Spread",
            "LeftHand.Little.2 Stretched",
            "LeftHand.Little.3 Stretched",
            "RightHand.Thumb.1 Stretched",
            "RightHand.Thumb.Spread",
            "RightHand.Thumb.2 Stretched",
            "RightHand.Thumb.3 Stretched",
            "RightHand.Index.1 Stretched",
            "RightHand.Index.Spread",
            "RightHand.Index.2 Stretched",
            "RightHand.Index.3 Stretched",
            "RightHand.Middle.1 Stretched",
            "RightHand.Middle.Spread",
            "RightHand.Middle.2 Stretched",
            "RightHand.Middle.3 Stretched",
            "RightHand.Ring.1 Stretched",
            "RightHand.Ring.Spread",
            "RightHand.Ring.2 Stretched",
            "RightHand.Ring.3 Stretched",
            "RightHand.Little.1 Stretched",
            "RightHand.Little.Spread",
            "RightHand.Little.2 Stretched",
            "RightHand.Little.3 Stretched"
        };
        
        private Animator animator;
        private HumanPose humanPose;
        private HumanPoseHandler humanPoseHandler;
        private float[] initialHumanPoseValues;
        private Vector3 initialHipPosition;

        private bool isAllOn;
        private bool isInitialized;

        private bool rightHandFold;
        private bool leftHandFold;
        
        private float rightHandValue   = 0.75f;
        private float rightThumbValue  = 0.67f;
        private float rightSpreadValue = -0.5f;
        
        private float rightIndexValue  = 0.75f;
        private float rightMiddleValue = 0.75f;
        private float rightRingValue   = 0.75f;
        private float rightLittleValue = 0.75f;
        
        private readonly int[] rightHandOpen   = {79, 81, 82, 83, 85, 86, 87, 89, 90, 91, 93, 94};
        private readonly int[] rightHandSpread = {80, 84, 88, 92};
        private readonly int[] rightThumbOpen  = {77, 78};
        
        private readonly int[] rightIndexOpen  = {79, 81, 82};
        private readonly int[] rightMiddleOpen = {83, 85, 86};
        private readonly int[] rightRingOpen   = {87, 89, 90};
        private readonly int[] rightLittleOpen = {91, 93, 94};
        
        private float leftHandValue   = 0.75f;
        private float leftThumbValue  = 0.67f;
        private float leftSpreadValue = -0.5f;
        
        private float leftIndexValue  = 0.75f;
        private float leftMiddleValue = 0.75f;
        private float leftRingValue   = 0.75f;
        private float leftLittleValue = 0.75f;
        
        private readonly int[] leftHandOpen   = {59, 61, 62, 63, 65, 66, 67, 69, 70, 71, 73, 74};
        private readonly int[] leftHandSpread = {60, 64, 68, 72};
        private readonly int[] leftThumbOpen  = {57, 58};
        
        private readonly int[] leftIndexOpen  = {59, 61, 62};
        private readonly int[] leftMiddleOpen = {63, 65, 66};
        private readonly int[] leftRingOpen   = {67, 69, 70};
        private readonly int[] leftLittleOpen = {71, 73, 74};
        
        private ExpressionSetupWindowViewModel viewModel;
        private Transform transform;

        private Vector2 scrollPosition;

        public SimpleBodyShapeEditTab(EditorWindow parentWindow) : base(parentWindow)
        {
            this.Title = "Simple Finger Shape";
        }
        
        public override void Initialize(ExpressionSetupWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
            
            if (viewModel == null || viewModel.AvatarDescriptor == null)
            {
                this.isInitialized = false;
                return;
            }
            
            this.animator = viewModel.AvatarDescriptor.GetComponent<Animator>();
            if (this.animator == null)
            {
                this.isInitialized = false;
                return;
            }

            this.transform = this.animator.GetBoneTransform(HumanBodyBones.Hips);
            this.initialHipPosition = this.transform.position;

            var avatar = this.animator.avatar;
            var modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(avatar)) as ModelImporter;
            this.isInitialized = avatar.isHuman && avatar.isValid && modelImporter != null;

            if (!this.isInitialized)
            {
                return;
            }

            this.humanPoseHandler = new HumanPoseHandler(avatar, this.animator.transform);
            this.humanPoseHandler.GetHumanPose(ref this.humanPose);
            
            this.initialHumanPoseValues = this.humanPose.muscles.ToArray();
        }
        
        public override void OnInspectorGUI()
        {
            if (!this.isInitialized)
            {
                EditorGUILayout.LabelField("This is not Humanoid Character.");
                return;
            }

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            
            EditorGUILayout.HelpBox("This is an experimental feature.", MessageType.Warning);

            EditorCustomGUILayout.BeginPropertyFoldGroup("Right Hand Open/Close", this.rightHandFold);
            {
                this.humanPoseHandler.GetHumanPose(ref this.humanPose);
                this.rightThumbValue = PreviewSlider("Thumb Finger", this.rightThumbValue, 0.67f);
                this.rightSpreadValue = PreviewSlider("Spreads", this.rightSpreadValue, -0.5f);

                foreach (var f in this.rightThumbOpen)
                {
                    this.humanPose.muscles[f] =  this.rightThumbValue;
                }
                
                foreach (var f in this.rightHandOpen)
                {
                    this.humanPose.muscles[f] = this.rightHandValue;
                }
                
                foreach (var f in this.rightHandSpread)
                {
                    this.humanPose.muscles[f] = this.rightSpreadValue;
                }

                EditorGUILayout.Space();

                this.rightIndexValue = PreviewSlider("Index", this.rightIndexValue);
                this.rightMiddleValue = PreviewSlider("Middle", this.rightMiddleValue);
                this.rightRingValue = PreviewSlider("Ring", this.rightRingValue);
                this.rightLittleValue = PreviewSlider("Little", this.rightLittleValue);

                foreach (var f in this.rightIndexOpen)
                {
                    this.humanPose.muscles[f] =  this.rightIndexValue;
                }
                
                foreach (var f in this.rightMiddleOpen)
                {
                    this.humanPose.muscles[f] =  this.rightMiddleValue;
                }
                
                foreach (var f in this.rightRingOpen)
                {
                    this.humanPose.muscles[f] =  this.rightRingValue;
                }
                
                foreach (var f in this.rightLittleOpen)
                {
                    this.humanPose.muscles[f] =  this.rightLittleValue;
                }
            }
            EditorCustomGUILayout.EndPropertyFoldGroup();

            EditorCustomGUILayout.BeginPropertyFoldGroup("Left Hand Open/Close", this.leftHandFold);
            {
                this.leftThumbValue = PreviewSlider("Thumb Finger", this.leftThumbValue, 0.67f);
                this.leftSpreadValue = PreviewSlider("Spreads", this.leftSpreadValue, -0.5f);

                foreach (var f in this.leftThumbOpen)
                {
                    this.humanPose.muscles[f] =  this.leftThumbValue;
                }
                
                foreach (var f in this.leftHandOpen)
                {
                    this.humanPose.muscles[f] = this.leftHandValue;
                }
                
                foreach (var f in this.leftHandSpread)
                {
                    this.humanPose.muscles[f] = this.leftSpreadValue;
                }
                
                EditorGUILayout.Space();

                this.leftIndexValue = PreviewSlider("Index", this.leftIndexValue);
                this.leftMiddleValue = PreviewSlider("Middle", this.leftMiddleValue);
                this.leftRingValue = PreviewSlider("Ring", this.leftRingValue);
                this.leftLittleValue = PreviewSlider("Little", this.leftLittleValue);

                foreach (var f in this.leftIndexOpen)
                {
                    this.humanPose.muscles[f] =  this.leftIndexValue;
                }
                
                foreach (var f in this.leftMiddleOpen)
                {
                    this.humanPose.muscles[f] =  this.leftMiddleValue;
                }
                
                foreach (var f in this.leftRingOpen)
                {
                    this.humanPose.muscles[f] =  this.leftRingValue;
                }
                
                foreach (var f in this.leftLittleOpen)
                {
                    this.humanPose.muscles[f] =  this.leftLittleValue;
                }
                this.humanPoseHandler.SetHumanPose(ref this.humanPose);
            }
            EditorCustomGUILayout.EndPropertyFoldGroup();
            EditorGUILayout.EndScrollView();

            this.transform.position = this.initialHipPosition;

            for (var i = 0; i < this.humanPose.muscles.Length; i++)
            {
                this.Modified = !Mathf.Approximately(this.humanPose.muscles[i], this.initialHumanPoseValues[i]);
                if (this.Modified)
                {
                    break;
                }
            }
        }

        private static float PreviewSlider(string title, float value, float defaultValue = 0.75f)
        {
            EditorGUILayout.BeginHorizontal();
            var result = EditorGUILayout.Slider(title, value, -1, 1);
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                result = defaultValue;
            }
            EditorGUILayout.EndHorizontal();
            if (result > -0.100000001490116 && result < 0.100000001490116) return 0.0f;
            return result;
        }
        
        public AnimationClip CreateAnimationClip(AnimationClip animationClip)
        {
            if (!this.isInitialized)
            {
                return animationClip;
            }

            var renameIndex = 0;
            for (var i = 0; i < HumanTrait.MuscleName.Length; i++)
            {
                var muscleName = HumanTrait.MuscleName[i];
                var propertyName = i < 55 ? muscleName : this.muscleSpecialNames[renameIndex++];

                var curveBinding = new EditorCurveBinding
                {
                    type = typeof(Animator),
                    path = "",
                    propertyName = propertyName
                };

                var curve = new AnimationCurve();
                curve.AddKey(0f, this.humanPose.muscles[i]);
                curve.AddKey(0.01f, this.humanPose.muscles[i]);

                AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
            }

            return animationClip;
        }

        public void OnNavigatedTo()
        {
            this.Initialize(this.viewModel);
        }

        public void OnNavigatedFrom()
        {
            if (!this.isInitialized) return;

            this.OnDestroy();
        }

        public override void OnDestroy()
        {
            for (var i = 0; i < this.humanPose.muscles.Length; i++)
            {
                this.humanPose.muscles[i] = this.initialHumanPoseValues[i];
            }

            try
            {
                this.humanPoseHandler.SetHumanPose(ref this.humanPose);
            }
            catch (Exception e) when (e is NullReferenceException && e.Message == "HumanPoseHandler is not initialized properly")
            {
                // ignore exception.
            }
            this.transform.position = this.initialHipPosition;
            this.humanPoseHandler.Dispose();
        }
    }
}