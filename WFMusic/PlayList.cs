using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using PlayListManager;
using Un4seen.Bass.AddOn.Tags;
using FileManager;

namespace WFMusic
{
    public partial class PlayList : DockContent
    {
        private PLM plm;
        private int rowSelect;
        private int playMode = 0; //默认单曲循环
        private string playingFileName; //当前播放的文件名
        private string playingFilePath;

        public int PlayMode
        {
            get
            {
                return playMode;
            }

            set
            {
                //int val = Convert.ToInt32(Enum.Parse(typeof(PlayModes), value.ToString()));
                int max = Enum.GetNames(typeof(PlayModes)).GetLength(0);
                playMode = value >= max ? 0 : value;
            }
        }

        public string PlayModeName
        {
            get
            {
                return Enum.GetName(typeof(PlayModes), playMode);
            }
        }

        public string PlayingFileName
        {
            get
            {
                return playingFileName;
            }

            set
            {
                playingFileName = value;
            }
        }

        public string PlayingFilePath
        {
            get
            {
                return playingFilePath;
            }

            set
            {
                playingFilePath = value;
            }
        }

        public enum PlayModes:int
        {
            单曲循环 = 0,
            顺序播放 = 1,
            列表循环 = 2,
            随机播放 = 3,
        };
        
        public PlayList()
        {
            InitializeComponent();
        }

        private void PlayList_Load(object sender, EventArgs e)
        {
            plm = new PLM();

        }

        public void AddMusic(string[] list)
        {
            try
            {
                if (list == null)
                    return;

                plm.AddItems(list);
                MusicListUpdate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void MusicListUpdate()
        {
            if (this.skinDataGridView1.DataSource != null)
            {
                this.skinDataGridView1.DataSource = null;
            }

            this.skinDataGridView1.DataSource = plm.MusicList;
        }

        #region 播放控制

        public delegate void PlayDelegate(string file);
        public PlayDelegate Play;

        private void ListPlay()
        {
            skinDataGridView1.ClearSelection();
            skinDataGridView1.Rows[rowSelect].Selected = true;

            this.PlayingFileName = plm.GetFileName(rowSelect);
            this.playingFilePath = plm.GetFilePlayPath(rowSelect);
            Play?.Invoke(playingFilePath);
        }

        private void DataGridViewScroll()
        {
            skinDataGridView1.FirstDisplayedScrollingRowIndex = rowSelect;
            skinDataGridView1.DisplayedRowCount(true);
        }

        //设置上一首
        public void PrevMusic()
        {
            rowSelect--;

            if (rowSelect < 0)
            {
                if (plm.ReturnItemCount() != 0)
                {
                    rowSelect = plm.ReturnItemCount() - 1;
                }
                else
                {
                    return;
                }
            }

            DataGridViewScroll();

            ListPlay();
        }

        //设置下一首
        public void NextMusic()
        {
            rowSelect++;
            if (rowSelect > (plm.ReturnItemCount() - 1))
            {
                if (plm.ReturnItemCount() != 0)
                {
                    rowSelect=0;
                }
            }
            DataGridViewScroll();
            ListPlay();
        }


        //双击播放
        private void skinDataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //选中多个时默认播放第一个
            rowSelect = ReturnSelected()[0];
            ListPlay();
        }

        #endregion

        #region contextMenuStrip1右键菜单事件

        //右键菜单
        private void skinDataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            rowSelect = e.RowIndex;
            this.PlayingFileName = plm.GetFileName(rowSelect);
            this.playingFilePath = plm.GetFilePlayPath(rowSelect);

            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    if (skinDataGridView1.Rows[e.RowIndex].Selected == false)
                    {
                        skinDataGridView1.ClearSelection();
                        skinDataGridView1.Rows[e.RowIndex].Selected = true;
                    }

                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private int[] ReturnSelected()
        {
            return this.skinDataGridView1.SelectedRows
                         .OfType<DataGridViewRow>()
                         .Select(x => x.Index)
                         .OrderBy(x => x)
                         .ToArray();
        }

        private void 播放选中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //选中多个时默认播放第一个
            rowSelect = ReturnSelected()[0];
            ListPlay();
        }

        private void 删除选中ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int[] selIndexes = ReturnSelected();

            if (selIndexes[0] == plm.ReturnItemCount()-1)
            {
                rowSelect -= 1;
            }

            foreach (var n in selIndexes.Reverse()) //倒序删除
            {
                plm.DelteItem(n);
            }

            MusicListUpdate();

            if (selIndexes[0] > 0 && selIndexes.Length > 1)
            {
                rowSelect = selIndexes[0] - 1;
            }
            else if (plm.ReturnItemCount() != 0 && selIndexes[0] == 0)
            {
                rowSelect = 0;
            }
            else if (selIndexes.Length == 1 && plm.ReturnItemCount() != 0)
            {
                ;
            }
            else
            {
                rowSelect = 0;
                return;
            }

            skinDataGridView1.ClearSelection();
            skinDataGridView1.Rows[rowSelect].Selected = true;
        }

        private void 清空列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plm.ClearMusicList();
            MusicListUpdate();
        }

        #endregion

        #region 鼠标拖拽事件

        private void skinDataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("path"))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void skinDataGridView1_DragDrop(object sender, DragEventArgs e)
        {

            Task.Run(() =>
            {
                try
                {
                    string path = e.Data.GetData("path").ToString();

                    if (Files.IsDirectory(path))
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            this.AddMusic(Files.GetFiles(path, "", true));
                        }));


                    }
                    else
                    {
                        if (Files.ExistFile(path))
                        {

                            this.Invoke(new MethodInvoker(() =>
                            {
                                this.AddMusic(new string[] { path });
                            }));

                        }
                        else
                        {
                            MessageBox.Show("当前路径无效！");
                        }
                    }
                    this.PlayingFilePath = plm.GetFilePlayPath(0);
                    this.PlayingFileName = plm.GetFileName(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });
        }

        #endregion

        #region 播放类型

        //类型一：单曲循环 = 0,
        //类型二：顺序播放 = 1,
        //类型三：列表循环 = 2,
        //类型四：随机播放 = 3,

        public void PlayCompleteCallBak()
        {
            switch (this.PlayMode)
            {
                case 0:
                    {
                        //单曲播放
                        ListPlay();
                    }
                    break;
                case 1:
                    {
                        //顺序播放
                        if (rowSelect < (plm.ReturnItemCount() - 1))
                        {
                            NextMusic();
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                case 2:
                    {
                        //列表循环
                         NextMusic();
                    }
                    break;
                case 3:
                    {
                        //随机播放
                        rowSelect = new Random(Guid.NewGuid().GetHashCode()).Next(0, plm.ReturnItemCount());
                        ListPlay();
                    }
                    break;
            }
        }


        #endregion
    }
}
