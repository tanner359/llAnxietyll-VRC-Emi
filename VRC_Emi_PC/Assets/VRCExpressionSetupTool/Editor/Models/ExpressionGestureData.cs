using UnityEditor.Animations;
using UnityEngine;

namespace VRCExpressionSetupTool.Editor.Models
{
    public class ExpressionGestureData
    {
        public string Name { get; set; }
        
        public AnimatorState ExpressionState { get; set; }
        
        public AnimatorState GestureState { get; set; }
    }
}