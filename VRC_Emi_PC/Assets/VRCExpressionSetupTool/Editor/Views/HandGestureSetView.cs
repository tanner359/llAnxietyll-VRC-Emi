using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VRCExpressionSetupTool.Editor.Models;
using VRCExpressionSetupTool.Editor.Utility;
using VRCExpressionSetupTool.Editor.Views.Layout;
using BlendTree = UnityEditor.Animations.BlendTree;

namespace VRCExpressionSetupTool.Editor.Views
{
    public class HandGestureSetView
    {
        private readonly string title;
        private readonly List<ExpressionGestureData> expressionGestureDataList;
        private readonly List<bool> foldList;

        private readonly ReorderableList reorderableList;
        
        private static Color BackgroundColor => EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f, 0.2f) :
        new Color(1f, 1f, 1f, 0.2f);

        public HandGestureSetView(string title, List<ExpressionGestureData> expressionGestureDataList)
        {
            this.title = title;
            this.expressionGestureDataList = expressionGestureDataList;
            this.foldList = new bool[expressionGestureDataList.Count].ToList();
            
            this.reorderableList = new ReorderableList(this.expressionGestureDataList, typeof(ExpressionGestureData), false, true, false, false);
            this.reorderableList.drawElementCallback += this.DrawElement;
            this.reorderableList.drawHeaderCallback += this.DrawHeader;
            this.reorderableList.elementHeightCallback += index =>
                this.foldList[index]
                    ? EditorGUIUtility.singleLineHeight * 3 + 32
                    : EditorGUIUtility.singleLineHeight + 6;
            this.reorderableList.drawElementBackgroundCallback += (rect, index, active, focused) =>
            {
                if(index % 2 == 0) EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.2f));
            };
        }

        private void DrawHeader(Rect position)
        {
            var titleRect = new Rect(position)
            {
                width = 100,
                x = position.x + 4,
                height = EditorGUIUtility.singleLineHeight,
            };

            var expressionRect = new Rect(position)
            {
                width = position.width / 2 - titleRect.width / 2 - 12,
                x = position.x + titleRect.width + 4,
                height = EditorGUIUtility.singleLineHeight,
            };
            
            var gestureRect = new Rect(expressionRect)
            {
                x = expressionRect.x + expressionRect.width + 16
            };
            
            EditorGUI.LabelField(titleRect, this.title, EditorStyles.boldLabel);
            EditorGUI.LabelField(expressionRect, "Expression(表情)");
            EditorGUI.LabelField(gestureRect, "Gesture(手)");
        }

        private void DrawElement(Rect position, int index, bool isActive, bool isFocused)
        {
            var foldToggleRect = new Rect(position)
            {
                width = 14,
                x = position.x + 4,
                height = EditorGUIUtility.singleLineHeight,
            };
            
            var nameRect = new Rect(position)
            {
                width = 100,
                x = position.x + foldToggleRect.width + 4,
                height = EditorGUIUtility.singleLineHeight
            };

            var expressionRect = new Rect(position)
            {
                width = position.width / 2 - nameRect.width / 2 - 12,
                x = position.x + nameRect.width + 4,
                height = EditorGUIUtility.singleLineHeight,
                y = position.y + 3
            };
            
            var gestureRect = new Rect(expressionRect)
            {
                width = expressionRect.width - 4,
                x = expressionRect.x + expressionRect.width + 16
            };

            this.foldList[index] = FoldToggleField(foldToggleRect, this.foldList[index]);
            EditorGUI.LabelField(nameRect, this.expressionGestureDataList[index].Name);
            MotionField(expressionRect, this.expressionGestureDataList[index].ExpressionState);
            MotionField(gestureRect, this.expressionGestureDataList[index].GestureState);

            if (!this.foldList[index]) return;
            
            var eyeTrackingRect = new Rect(expressionRect)
            {
                y = expressionRect.y + EditorGUIUtility.singleLineHeight + 3
            };
            
            var mouthTrackingRect = new Rect(eyeTrackingRect)
            {
                y = eyeTrackingRect.y + EditorGUIUtility.singleLineHeight + 1
            };
            
            var rightFingersTrackingRect = new Rect(gestureRect)
            {
                y = gestureRect.y + EditorGUIUtility.singleLineHeight + 3
            };
            
            var leftFingersTrackingRect = new Rect(rightFingersTrackingRect)
            {
                y = rightFingersTrackingRect.y + EditorGUIUtility.singleLineHeight + 1
            };
                
            TrackingEyeControlField(eyeTrackingRect, "Eyes & Eyelids(目)", this.expressionGestureDataList[index].ExpressionState);
            TrackingMouthControlField(mouthTrackingRect, "Mouth & Jaw(口)", this.expressionGestureDataList[index].ExpressionState);
            
            TrackingRightFingersControlField(rightFingersTrackingRect, "Right Fingers(右指s)", this.expressionGestureDataList[index].GestureState);
            TrackingLeftFingersControlField(leftFingersTrackingRect, "Left Fingers(左指s)", this.expressionGestureDataList[index].GestureState);
        }
        
        private static void TrackingLeftFingersControlField(Rect position, string title, AnimatorState state)
        {
            var rect = new Rect(position) {width = position.width - 48};
            var iconRect = new Rect(position)
            {
                x = position.x + rect.width + 4,
                y = position.y + 2,
                width = 12,
                height = 12
            };
            var trackingControl = GetTrackingControl(state);

            EditorCustomGUI.EnumPopup<VRC_AnimatorTrackingControl.TrackingType>(rect, title, trackingControl.trackingLeftFingers,
                x =>
                {
                    trackingControl.trackingLeftFingers = x;
                });
            EditorGUI.DrawRect(iconRect, GetTrackingControlImageColor(trackingControl.trackingLeftFingers));
        }
        
        private static void TrackingRightFingersControlField(Rect position, string title, AnimatorState state)
        {
            var rect = new Rect(position) {width = position.width - 48};
            var iconRect = new Rect(position)
            {
                x = position.x + rect.width + 4,
                y = position.y + 2,
                width = 12,
                height = 12
            };
            var trackingControl = GetTrackingControl(state);

            EditorCustomGUI.EnumPopup<VRC_AnimatorTrackingControl.TrackingType>(rect, title, trackingControl.trackingRightFingers,
                x =>
                {
                    trackingControl.trackingRightFingers = x;
                });
            EditorGUI.DrawRect(iconRect, GetTrackingControlImageColor(trackingControl.trackingRightFingers));
        }

        private static void TrackingEyeControlField(Rect position, string title, AnimatorState state)
        {
            var rect = new Rect(position) {width = position.width - 48};
            var iconRect = new Rect(position)
            {
                x = position.x + rect.width + 4,
                y = position.y + 2,
                width = 12,
                height = 12
            };
            var trackingControl = GetTrackingControl(state);

            EditorCustomGUI.EnumPopup<VRC_AnimatorTrackingControl.TrackingType>(rect, title, trackingControl.trackingEyes,
                x =>
                {
                    trackingControl.trackingEyes = x;
                });
            EditorGUI.DrawRect(iconRect, GetTrackingControlImageColor(trackingControl.trackingEyes));
        }
        
        private static void TrackingMouthControlField(Rect position, string title, AnimatorState state)
        {
            var rect = new Rect(position) {width = position.width - 48};
            var iconRect = new Rect(position)
            {
                x = position.x + rect.width + 4,
                y = position.y + 2,
                width = 12,
                height = 12
            };
            var trackingControl = GetTrackingControl(state);

            EditorCustomGUI.EnumPopup<VRC_AnimatorTrackingControl.TrackingType>(rect, title, trackingControl.trackingMouth,
                x =>
                {
                    trackingControl.trackingMouth = x;
                });
            EditorGUI.DrawRect(iconRect, GetTrackingControlImageColor(trackingControl.trackingMouth));
        }

        private static Color GetTrackingControlImageColor(VRC_AnimatorTrackingControl.TrackingType type)
        {
            if (type == VRC_AnimatorTrackingControl.TrackingType.NoChange)
                return Color.clear;
            
            return type == VRC_AnimatorTrackingControl.TrackingType.Tracking
                    ? new Color(1, 0.5f, 0, 0.3f)
                    : new Color(0, 0, 1, 0.3f);
        }

        private static VRCAnimatorTrackingControl GetTrackingControl(AnimatorState state)
        {
            var trackingControl = state.behaviours.OfType<VRCAnimatorTrackingControl>()
                .FirstOrDefault();

            if (trackingControl == null)
            {
                trackingControl = state.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            }

            return trackingControl;
        }

        private static void MotionField(Rect position, AnimatorState state)
        {
            if(state == null) return;
            
            if (state.motion is BlendTree expressionTree)
            {
                if (expressionTree.children.Length > 0)
                {
                    EditorCustomGUI.ObjectField(position, "", (AnimationClip) expressionTree.children[0].motion, true,
                        false,
                        x => expressionTree.SetMotion(x, 0));
                }
            }
            else
            {
                EditorCustomGUI.ObjectField(position, "",
                    (AnimationClip) state.motion, true, false,
                    x => state.motion = x);
            }
        }

        private static bool FoldToggleField(Rect position, bool value)
        {
            value = GUI.Toggle(position, value, GUIContent.none, EditorStyles.foldout);
            var e = Event.current;
            if (e.type != EventType.MouseDown) return value;
            if (!position.Contains(e.mousePosition)) return value;
            if (e.button == 0)
            {
                value = !value;
            }
            e.Use();
            return value;
        }


        public void OnGUI()
        {
            this.reorderableList.DoLayoutList();
        }
    }
}