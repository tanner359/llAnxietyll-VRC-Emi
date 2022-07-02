using VRC.SDK3.Avatars.Components;

namespace VRCExpressionSetupTool.Editor.Utility
{
    public static class VRCAnimatorTrackingControlExtension
    {
        public static void DeepCopy(this VRCAnimatorTrackingControl trackingControl, ref VRCAnimatorTrackingControl target)
        {
            target.debugString = trackingControl.debugString;
            target.hideFlags = trackingControl.hideFlags;
            target.name = trackingControl.name;
            target.trackingEyes = trackingControl.trackingEyes;
            target.trackingHead = trackingControl.trackingHead;
            target.trackingHip = trackingControl.trackingHip;
            target.trackingLeftFingers = trackingControl.trackingLeftFingers;
            target.trackingLeftFoot = trackingControl.trackingLeftFoot;
            target.trackingLeftHand = trackingControl.trackingLeftHand;
            target.trackingMouth = trackingControl.trackingMouth;
            target.trackingRightFingers = trackingControl.trackingRightFingers;
            target.trackingRightFoot = trackingControl.trackingRightFoot;
            target.trackingRightHand = trackingControl.trackingRightHand;
        }
    }
}