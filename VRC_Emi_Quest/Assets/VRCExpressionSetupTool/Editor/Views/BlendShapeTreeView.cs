using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VRCExpressionSetupTool.Editor.Views
{
    internal class BlendShapeTreeView : TreeView
    {
        private readonly Dictionary<string, bool> foldBulkToggleTable = new Dictionary<string, bool>();
        private readonly float[] initialBlendShapeValues;
        private readonly SkinnedMeshRenderer[] skinnedMeshRenderers;

        private TreeViewItem root;

        public BlendShapeTreeView(TreeViewState state) : base(state) { }

        public BlendShapeTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader,
            IEnumerable<SkinnedMeshRenderer> skinnedMeshRenderers) : base(state, multiColumnHeader)
        {
            this.rowHeight = 20;
            this.showAlternatingRowBackgrounds = true;
            this.showBorder = true;

            var meshRenderers = skinnedMeshRenderers as SkinnedMeshRenderer[] ?? skinnedMeshRenderers.ToArray();
            this.skinnedMeshRenderers = meshRenderers.Where(x => x.sharedMesh.blendShapeCount > 0).ToArray();

            var tmp = new List<float>();
            foreach (var skinnedMeshRenderer in meshRenderers)
                for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    tmp.Add(skinnedMeshRenderer.GetBlendShapeWeight(i));

            this.initialBlendShapeValues = tmp.ToArray();

            multiColumnHeader.ResizeToFit();
            this.Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            if (this.skinnedMeshRenderers == null || this.skinnedMeshRenderers.Length == 0 ||
                this.skinnedMeshRenderers.Max(x => x.sharedMesh.blendShapeCount) == 0)
            {
                return null;
            }

            this.root = new TreeViewItem
            {
                depth = -1
            };

            var index = 1;
            foreach (var skinnedMeshRenderer in this.skinnedMeshRenderers)
            {
                var root2 = new TreeViewItem
                {
                    depth = 0,
                    id = index,
                    displayName = skinnedMeshRenderer.name
                };
                this.root.AddChild(root2);
                this.foldBulkToggleTable.Add(skinnedMeshRenderer.name + ":" + index++, true);
            }

            for (var i = 0; i < this.root.children.Count; i++)
            {
                var skinnedMeshRenderer = this.skinnedMeshRenderers[i];
                var item = this.root.children[i];

                for (var i1 = 0; i1 < skinnedMeshRenderer.sharedMesh.blendShapeCount; i1++)
                {
                    var blendShapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i1);
                    item.AddChild(new BlendShapeTreeItem(index++,
                        true,
                        blendShapeName,
                        skinnedMeshRenderer.GetBlendShapeWeight(i1),
                        skinnedMeshRenderer.name,
                        i1)
                    {
                        depth = 1
                    });
                }
            }

            return this.root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is BlendShapeTreeItem blendShapeTreeItem)
            {
                this.BlendShapeItemGUI(args, blendShapeTreeItem);
            }
            else
            {
                this.SkinnedMeshRootGUI(args);
            }
        }

        private void SkinnedMeshRootGUI(RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                var rect = args.GetCellRect(i);
                var column = (BlendShapeTreeColumn) args.GetColumn(i);
                var boldLabelStyle = args.selected ? EditorStyles.whiteBoldLabel : EditorStyles.boldLabel;
                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                var value = this.foldBulkToggleTable[args.item.displayName + ":" + args.item.id];
                switch (column)
                {
                    case BlendShapeTreeColumn.Id:
                        rect.x += 5;
                        EditorGUI.LabelField(rect, args.item.id.ToString(), labelStyle);
                        break;
                    case BlendShapeTreeColumn.CheckBox:
                        rect.x += 15;
                        rect.xMax = rect.x + 15;
                        var toggle = EditorGUI.Toggle(rect, value);
                        if (value != toggle)
                        {
                            var treeItems = this.GetAllRows()
                                .Where(x => x is BlendShapeTreeItem)
                                .Cast<BlendShapeTreeItem>()
                                .Select(x => x.BlendShapeTreeElement)
                                .Where(x => x.MeshName == args.item.displayName);
                            
                            foreach (var element in treeItems)
                            {
                                element.IsExport = toggle;
                            }

                            this.foldBulkToggleTable[args.item.displayName + ":" + args.item.id] = toggle;
                        }

                        break;
                    case BlendShapeTreeColumn.Name:
                        this.columnIndexForTreeFoldouts = 3;

                        rect.x += this.foldoutWidth + 2;
                        EditorGUI.LabelField(rect, args.item.displayName,
                            boldLabelStyle);
                        break;
                }
            }
        }

        private void BlendShapeItemGUI(RowGUIArgs args, BlendShapeTreeItem blendShapeTreeItem)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                var rect = args.GetCellRect(i);
                var column = (BlendShapeTreeColumn) args.GetColumn(i);
                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                var element = blendShapeTreeItem.BlendShapeTreeElement;

                switch (column)
                {
                    case BlendShapeTreeColumn.Id:
                        rect.x += 15;
                        EditorGUI.LabelField(rect, args.item.id.ToString(), labelStyle);
                        break;
                    case BlendShapeTreeColumn.CheckBox:
                        rect.x += 25;
                        element.IsExport =
                            EditorGUI.Toggle(rect, element.IsExport);
                        break;
                    case BlendShapeTreeColumn.Name:
                        rect.x += 35;
                        EditorGUI.LabelField(rect, blendShapeTreeItem.BlendShapeTreeElement.BlendShapeName,
                            labelStyle);
                        break;
                    case BlendShapeTreeColumn.FloatValue:
                        this.SetBlendShapeValue(element, EditorGUI.Slider(rect, element.BlendShapeValue, 0, 100f));
                        break;
                    case BlendShapeTreeColumn.MinButton:
                        if (GUI.Button(rect, "Min")) this.SetBlendShapeValue(element, 0f);
                        break;
                    case BlendShapeTreeColumn.MaxButton:
                        if (GUI.Button(rect, "Max")) this.SetBlendShapeValue(element, 100f);
                        break;
                    case BlendShapeTreeColumn.ResetButton:
                        if (GUI.Button(rect, "Reset"))
                        {
                            this.SetBlendShapeValue(element,
                                this.initialBlendShapeValues[args.item.id - this.skinnedMeshRenderers.Length - 1]);
                        }
                        break;
                }
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return item is BlendShapeTreeItem blendShapeTreeItem &&
                   (blendShapeTreeItem.BlendShapeTreeElement.BlendShapeName.Contains(search) ||
                    blendShapeTreeItem.BlendShapeTreeElement.BlendShapeName.Contains(search));
        }

        private void SetBlendShapeValue(BlendShapeTreeElement element, float value)
        {
            element.BlendShapeValue = value;
            foreach (var skinnedMeshRenderer in this.skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.name != element.MeshName)
                {
                    continue;
                }

                skinnedMeshRenderer.SetBlendShapeWeight(element.BlendShapeId, value);
                break;
            }
        }

        public IList<TreeViewItem> GetAllRows()
        {
            var items = new List<TreeViewItem>();
            foreach (var treeViewItem in this.root.children)
            {
                this.Traverse(treeViewItem, items);
            }
            

            return items;
        }

        private void Traverse(TreeViewItem item, ICollection<TreeViewItem> items)
        {
            items.Add(item);
            if (!item.hasChildren) return;
            
            foreach (var treeViewItem in item.children)
                this.Traverse(treeViewItem, items);
        }
    }
}