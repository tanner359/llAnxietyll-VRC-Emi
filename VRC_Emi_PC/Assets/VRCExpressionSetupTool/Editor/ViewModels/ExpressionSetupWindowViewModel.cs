using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRCExpressionSetupTool.Editor.Models;
using VRCExpressionSetupTool.Editor.Mvvm;
using VRCExpressionSetupTool.Editor.Utility;

namespace VRCExpressionSetupTool.Editor.ViewModels
{
    public class ExpressionSetupWindowViewModel : BindableBase
    {
        private VRCAvatarDescriptor avatarDescriptor;
        private AnimatorController fxAnimatorController;
        private AnimatorController gestureAnimatorController;
        private List<ExpressionGestureData> rightExpressionGestureDataList = new List<ExpressionGestureData>();
        private List<ExpressionGestureData> leftExpressionGestureDataList = new List<ExpressionGestureData>();
        private bool writesDefault = true;

        public ExpressionSetupWindowViewModel()
        {
            this.CreateControllersCommand = new DelegateCommand(() =>
            {
                var fxControllerCreator = new HandControllerCreator(this.fxAnimatorController, this.writesDefault);
                fxControllerCreator.Apply();
                
                var gestureControllerCreator = new HandControllerCreator(this.gestureAnimatorController, this.writesDefault);
                gestureControllerCreator.Apply();

                this.RightExpressionGestureDataList.Clear();
                this.RightExpressionGestureDataList = this.CreateExpressionGestureDataList("Right Hand");
                
                this.LeftExpressionGestureDataList.Clear();
                this.LeftExpressionGestureDataList = this.CreateExpressionGestureDataList("Left Hand");
                EditorGUIUtility.PingObject(this.avatarDescriptor);
            }, () => this.AvatarDescriptor != null && this.FxAnimatorController != null &&
                     this.GestureAnimatorController != null);
        }
        
        
        /// <summary>
        /// AvatarDescriptor
        /// </summary>
        public VRCAvatarDescriptor AvatarDescriptor
        {
            get => this.avatarDescriptor;
            set
            {
                if (!this.SetProperty(ref this.avatarDescriptor, value)) return;

                if (value != null)
                {
                    this.FxAnimatorController = value.GetPlayableLayer(VRCAvatarDescriptor.AnimLayerType.FX);
                    this.GestureAnimatorController = value.GetPlayableLayer(VRCAvatarDescriptor.AnimLayerType.Gesture);
                }
                else
                {
                    this.fxAnimatorController = null;
                    this.gestureAnimatorController = null;
                }
                this.RaiseDataListChange();
            }
        }

        private void RaiseDataListChange()
        {
            if (this.GestureAnimatorController == null || this.FxAnimatorController == null)
            {
                this.RightExpressionGestureDataList.Clear();
                this.LeftExpressionGestureDataList.Clear();
            }
            else
            {
                this.RightExpressionGestureDataList = this.CreateExpressionGestureDataList("Right Hand");
                this.LeftExpressionGestureDataList = this.CreateExpressionGestureDataList("Left Hand");
            }
        }

        /// <summary>
        /// FX AnimatorController
        /// </summary>
        public AnimatorController FxAnimatorController
        {
            get => this.fxAnimatorController;
            set
            {
                if (!this.SetProperty(ref this.fxAnimatorController, value)) return;
                this.avatarDescriptor.SetPlayableLayer(value, VRCAvatarDescriptor.AnimLayerType.FX);
                this.RaiseDataListChange();
            }
        }
        
        /// <summary>
        /// Gesture AnimatorController
        /// </summary>
        public AnimatorController GestureAnimatorController
        {
            get => this.gestureAnimatorController;
            set
            {
                if (!this.SetProperty(ref this.gestureAnimatorController, value)) return;
                this.avatarDescriptor.SetPlayableLayer(value, VRCAvatarDescriptor.AnimLayerType.Gesture);
                this.RaiseDataListChange();
            }
        }

        public List<ExpressionGestureData> RightExpressionGestureDataList
        {
            get => this.rightExpressionGestureDataList;
            set => this.SetProperty(ref this.rightExpressionGestureDataList, value);
        }
        
        public List<ExpressionGestureData> LeftExpressionGestureDataList
        {
            get => this.leftExpressionGestureDataList;
            set => this.SetProperty(ref this.leftExpressionGestureDataList, value);
        }

        public bool WritesDefault
        {
            get => this.writesDefault;
            set => this.SetProperty(ref this.writesDefault, value);
        }

        public DelegateCommand CreateControllersCommand { get; }

        private List<ExpressionGestureData> CreateExpressionGestureDataList(string layerName)
        {
            var layerIndex = Array.FindIndex(this.GestureAnimatorController.layers, x => x.name == layerName);
            if(layerIndex == -1) return new List<ExpressionGestureData>();
            var gestureStates = this.GestureAnimatorController.layers[layerIndex].stateMachine.states;
            
            var layerIndex2 = Array.FindIndex(this.FxAnimatorController.layers, x => x.name == layerName);
            var expressionStates = this.FxAnimatorController.layers[layerIndex2].stateMachine.states;
            
            string[] handStateOderNames = {"Idle", "Fist", "Open", "Point", "Peace", "RockNRoll", "Gun", "Thumbs up"};
            var result = expressionStates.Join(gestureStates, state => state.state.name, state => state.state.name,
                (state, animatorState) => new ExpressionGestureData
                {
                    Name = state.state.name, ExpressionState = state.state, GestureState = animatorState.state
                }).OrderBy(x => ArrayUtility.IndexOf(handStateOderNames, x.Name)).ToList();

            return result;
        }
        
        
    }
}