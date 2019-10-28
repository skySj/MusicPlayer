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

namespace WFMusic
{
    public partial class PlaySpectrum : DockContent
    {
        public int PBoxWidth
        {
            get
            {
                return this.pictureBox1.Width;
            }

            set
            {
                this.pictureBox1.Width = value;
            }
        }
        public int PBoxHeight
        {
            get
            {
                return this.pictureBox1.Height;
            }

            set
            {
                this.pictureBox1.Height = value;
            }
        }

        public PlaySpectrum()
        {
            InitializeComponent();
        }

        private void PlaySpectrum_Resize(object sender, EventArgs e)
        {
            SpectrumAreaUpdate?.Invoke(this.pictureBox1.Width, this.pictureBox1.Height);

        }

        public void SetImg(Bitmap img)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            this.pictureBox1.Image = img;
        }

        #region SpectrumAreaUpdate
        public delegate void SpectrumAreaDelegate(int width, int height);
        public SpectrumAreaDelegate SpectrumAreaUpdate;

        #endregion

        #region SpectrumTypeUpdate
        public delegate void SpectrumTypeDelegate(int type);
        public SpectrumTypeDelegate SpectrumTypeUpdate;

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;
        }

        private void 效果1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }

        private void 效果12ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tm = sender as ToolStripMenuItem;
            SpectrumTypeUpdate?.Invoke(Convert.ToInt32(tm.Tag));
            this.Text = "频谱-效果" + tm.Tag;

        }
        #endregion

    }
}
