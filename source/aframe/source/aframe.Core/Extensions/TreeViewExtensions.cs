using System.Linq;
using System.Windows.Controls;

namespace aframe
{
    public static class TreeViewExtensions
    {
        public static void ExpandAllNodes(
           this TreeView treeView)
        {
            foreach (var item in treeView.Items.OfType<TreeViewItem>())
            {
                ExpandAllNodes(item);
            }
        }

        public static void ExpandAllNodes(
            this TreeViewItem treeItem)
        {
            treeItem.IsExpanded = true;
            foreach (var child in treeItem.Items.OfType<TreeViewItem>())
            {
                ExpandAllNodes(child);
            }
        }
    }
}
