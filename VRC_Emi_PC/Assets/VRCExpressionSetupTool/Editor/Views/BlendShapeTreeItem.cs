using UnityEditor.IMGUI.Controls;

namespace VRCExpressionSetupTool.Editor.Views
{
    internal class BlendShapeTreeItem : TreeViewItem
    {
        public BlendShapeTreeItem(int id, bool isExport, string blendShapeName, float blendShapeValue, string meshName,
            int blendShapeId) : base(id)
        {
            this.BlendShapeTreeElement =
                new BlendShapeTreeElement(isExport, blendShapeName, blendShapeValue, meshName, blendShapeId);
        }

        public BlendShapeTreeElement BlendShapeTreeElement { get; set; }
    }
}