using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WFMusic
{
    public partial class MusicList : DockContent
    {
        private List<TreeNode> selectNodes = new List<TreeNode>();

        public MusicList()
        {
            InitializeComponent();
        }

        private void MusicList_Load(object sender, EventArgs e)
        {
            
        }

        public void AddFiles(string path)
        {
            if (path.Equals(""))
                return;
            PaintTreeView(this.skinTreeView1, path);
        }

        private void SetTreeNodeTag(TreeNode node, string path)
        {
            node.Tag = path;
        }

        #region 生成目录文件的TreeView
        private void PaintTreeView(TreeView treeView, string fullPath)
        {
            try
            {
                //treeView.Nodes.Clear(); //清空TreeView
                treeView.Nodes.Add(fullPath);

                TreeNode TreeNodeLast = treeView.Nodes[treeView.Nodes.Count-1];
                SetTreeNodeTag(TreeNodeLast, fullPath); //设置TAG

                DirectoryInfo dirs = new DirectoryInfo(fullPath); //获得程序所在路径的目录对象
                DirectoryInfo[] dir = dirs.GetDirectories();//获得目录下文件夹对象
                FileInfo[] file = dirs.GetFiles();//获得目录下文件对象
                int dircount = dir.Count();//获得文件夹对象数量
                int filecount = file.Count();//获得文件对象数量

                //循环文件夹
                for (int i = 0; i < dircount; i++)
                {
                    TreeNodeLast.Nodes.Add(dir[i].Name);
                    string pathNode = dir[i].FullName;
                    GetMultiNode(TreeNodeLast.Nodes[i], pathNode);
                }

                //循环文件
                for (int j = 0; j < filecount; j++)
                {
                    TreeNodeLast.Nodes.Add(file[j].Name);
                    SetTreeNodeTag(TreeNodeLast.Nodes[j + dircount],
                                    file[j].FullName); //设置TAG
                }

                TreeNodeLast.Expand(); //默认展开
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n出错的位置为：Form1.PaintTreeView()");
        #endregion

            }
        }
        #region 遍历TreeView根节点下文件和文件夹
        private bool GetMultiNode(TreeNode treeNode, string path)
        {
            if (Directory.Exists(path) == false)
            { return false; }

            DirectoryInfo dirs = new DirectoryInfo(path); //获得程序所在路径的目录对象
            DirectoryInfo[] dir = dirs.GetDirectories();//获得目录下文件夹对象
            FileInfo[] file = dirs.GetFiles();//获得目录下文件对象
            int dircount = dir.Count();//获得文件夹对象数量
            int filecount = file.Count();//获得文件对象数量
            int sumcount = dircount + filecount;

            if (sumcount == 0)
            { return false; }

            SetTreeNodeTag(treeNode, path); //设置TAG

            //循环文件夹
            for (int j = 0; j < dircount; j++)
            {
                treeNode.Nodes.Add(dir[j].Name);
                string pathNodeB = dir[j].FullName;
                GetMultiNode(treeNode.Nodes[j], pathNodeB);
            }

            //循环文件
            for (int j = 0; j < filecount; j++)
            {
                treeNode.Nodes.Add(file[j].Name);

                SetTreeNodeTag(treeNode.Nodes[j + dircount],
                                file[j].FullName); //设置TAG
            }
            return true;
        }
        #endregion

        #region 鼠标拖动

        private void skinTreeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((sender as TreeView) != null)
            {
                TreeNode node = this.skinTreeView1.GetNodeAt(e.X, e.Y);
                this.skinTreeView1.SelectedNode = node;

                //if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                //{
                //    MessageBox.Show("暂不支持多选！");
                //}

            }
        }

        private void skinTreeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            IDataObject data = new DataObject();

            data.SetData("path", this.skinTreeView1.SelectedNode.Tag);

            // 开始进行拖放操作，并将拖放的效果设置成移动。
            this.DoDragDrop(data, DragDropEffects.Move);
        }

        private void skinTreeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion
    }
}
