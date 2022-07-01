using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRCExpressionSetupTool.Editor.Utility;

namespace VRCExpressionSetupTool.Editor.Models
{
    public class HandControllerCreator
    {
        private class MigrateMotionData
        {
            public string StateName { get; set; }
            public AnimationClip AnimationClip { get; set; }

            public VRCAnimatorTrackingControl VRCAnimatorTrackingControl { get; set; }
        }
        
        private readonly string[] handStateNames = {"Idle", "Fist", "Open", "Point", "Peace", "RockNRoll", "Gun", "Thumbs up"};
        private readonly List<MigrateMotionData> migrateMotionDataList = new List<MigrateMotionData>();
        private readonly AnimatorController animatorController;
        private readonly bool writesDefault;

        public HandControllerCreator(AnimatorController animatorController, bool writesDefault)
        {
            this.animatorController = animatorController;
            this.writesDefault = writesDefault;
        }

        public void Apply()
        {
            this.RemoveExistParameter();
            this.CreateParameter();
            
            var previousLeftIndex = Array.FindIndex(this.animatorController.layers, x => x.name == "Left Hand");
            var previousRightIndex = Array.FindIndex(this.animatorController.layers, x => x.name == "Right Hand");
                
            this.RemoveLayer("Left Hand");
            var leftHandMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(AssetDatabase.GUIDToAssetPath("b95d71977f62d5048acf6aa75e1e2fc9"));
            this.CreateLayer("Left Hand", previousLeftIndex, "GestureLeftWeight", "GestureLeft", leftHandMask);

            this.RemoveLayer("Right Hand");
            var rightHandMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(AssetDatabase.GUIDToAssetPath("a60cc97bf272818438bcdbba72ec5898"));
            this.CreateLayer("Right Hand", previousRightIndex, "GestureRightWeight", "GestureRight", rightHandMask);
        }

        private void RemoveExistParameter()
        {
            foreach (var animatorControllerParameter in this.animatorController.parameters)
            {
                var name = animatorControllerParameter.name;
                if (name == "GestureLeft" || name == "GestureRight" || name == "GestureLeftWeight" ||
                    name == "GestureRightWeight")
                {
                    this.animatorController.RemoveParameter(animatorControllerParameter);
                }
            }
        }

        private void CreateParameter()
        {
            this.animatorController.AddParameter(new AnimatorControllerParameter
            {
                name = "GestureLeft",
                type = AnimatorControllerParameterType.Int,
                defaultInt = 0
            });
            
            this.animatorController.AddParameter(new AnimatorControllerParameter
            {
                name = "GestureRight",
                type = AnimatorControllerParameterType.Int,
                defaultInt = 0
            });
            
            this.animatorController.AddParameter(new AnimatorControllerParameter
            {
                name = "GestureLeftWeight",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 0
            });
            
            this.animatorController.AddParameter(new AnimatorControllerParameter
            {
                name = "GestureRightWeight",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 0
            });
        }

        private void RemoveLayer(string layerName)
        {
            this.migrateMotionDataList.Clear();
            var index = Array.FindIndex(this.animatorController.layers, x => x.name == layerName);
            if(index < 0) return;
            
            foreach (var childAnimatorState in this.animatorController.layers[index].stateMachine.states)
            {
                var migrateMotionData = new MigrateMotionData
                {
                    StateName = childAnimatorState.state.name,
                    VRCAnimatorTrackingControl = ScriptableObject.CreateInstance<VRCAnimatorTrackingControl>()
                };

                var stateMachineBehaviour =
                    (VRCAnimatorTrackingControl)childAnimatorState.state.behaviours.FirstOrDefault(x => x is VRCAnimatorTrackingControl);
                if (stateMachineBehaviour != null)
                {
                    var vrcAnimatorTrackingControl = migrateMotionData.VRCAnimatorTrackingControl;
                    stateMachineBehaviour.DeepCopy(ref vrcAnimatorTrackingControl);
                    migrateMotionData.VRCAnimatorTrackingControl = vrcAnimatorTrackingControl;
                }

                if (childAnimatorState.state.motion is BlendTree tree)
                {
                    if(tree.children.Length > 0) migrateMotionData.AnimationClip = (AnimationClip) tree.children[0].motion;
                }
                else
                {
                    migrateMotionData.AnimationClip = (AnimationClip) childAnimatorState.state.motion;
                }

                this.migrateMotionDataList.Add(migrateMotionData);
            }
            
            this.animatorController.RemoveLayer(index);
        }

        private void CreateLayer(string layerName, int previousIndex, string blendParameter,
            string gestureConditionName, AvatarMask avatarMask)
        {
            this.animatorController.AddLayerDefault(layerName, avatarMask);
            if (previousIndex > 0)
            {
                var nowIndex =  Array.FindIndex(this.animatorController.layers, x => x.name == layerName);
                this.animatorController.layers = this.animatorController.ReorderLayer(nowIndex, previousIndex);
            }
            var layerIndex = Array.FindIndex(this.animatorController.layers, x => x.name == layerName);
            var stateMachine = this.animatorController.layers[layerIndex].stateMachine;
            stateMachine.anyStatePosition = Style.AnyStatePosition;
            stateMachine.entryPosition = Style.EntryPosition;
            stateMachine.exitPosition = Style.ExitPosition;

            var idleState = stateMachine.AddStateDefaultParam(this.handStateNames[0], Style.IdleStatePosition);
            idleState.timeParameter = blendParameter;
            idleState.timeParameterActive = true;
            idleState.writeDefaultValues = this.writesDefault;
            var idleBehaviour = idleState.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            
            var idleMigrateMotionData = this.migrateMotionDataList.FirstOrDefault(x => x.StateName == this.handStateNames[0]);
            idleState.motion = idleMigrateMotionData?.AnimationClip;
            idleMigrateMotionData?.VRCAnimatorTrackingControl.DeepCopy(ref idleBehaviour);
            
            stateMachine.AddAnyTransitionDefaultParam(idleState, AnimatorConditionMode.Equals, 0.0f, gestureConditionName);

            var fistBlendTree =
                this.animatorController.CreateBlendTreeInController(this.handStateNames[1], out var tree, layerIndex,
                    blendParameter, Style.FistStatePosition);
            fistBlendTree.timeParameter = blendParameter;
            fistBlendTree.timeParameterActive = true;
            fistBlendTree.writeDefaultValues = this.writesDefault;
            tree.blendType = BlendTreeType.Direct; 
            tree.SetNormalizedBlendValues(true);
            var fistMigrateMotionData = this.migrateMotionDataList.FirstOrDefault(x => x.StateName == this.handStateNames[1]);
            tree.AddChild(fistMigrateMotionData?.AnimationClip, blendParameter);
            var fistBehaviour = fistBlendTree.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            fistMigrateMotionData?.VRCAnimatorTrackingControl.DeepCopy(ref fistBehaviour);
            stateMachine.AddAnyTransitionDefaultParam(fistBlendTree, AnimatorConditionMode.Equals, 1.0f, gestureConditionName);

            for (var i = 2; i < this.handStateNames.Length; i++)
            {
                var position = Style.FistStatePosition;
                position.y += (i - 1) * 50;
                var handState = stateMachine.AddStateDefaultParam(this.handStateNames[i], position);
                handState.timeParameter = blendParameter;
                handState.timeParameterActive = true;
                handState.writeDefaultValues = this.writesDefault;
                var migrateMotionData = this.migrateMotionDataList.FirstOrDefault(x => x.StateName == this.handStateNames[i]);
                handState.motion = migrateMotionData?.AnimationClip;
                var behaviour = handState.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                migrateMotionData?.VRCAnimatorTrackingControl.DeepCopy(ref behaviour);
                stateMachine.AddAnyTransitionDefaultParam(handState, AnimatorConditionMode.Equals, i, gestureConditionName);
            }
        }
        
        private static class Style
        {
            public static readonly Vector3 AnyStatePosition = new Vector3(0, 250);
            public static readonly Vector3 EntryPosition = new Vector3(0, 0);
            public static readonly Vector3 ExitPosition = new Vector3(0, 100);  
            
            public static readonly Vector3 IdleStatePosition = new Vector3(250, 0);
            public static readonly Vector3 FistStatePosition = new Vector3(450, 100);
        }
    }
}