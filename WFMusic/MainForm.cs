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
using FileManager;
using BassManager;
using PlayListManager;
using System.Threading;

namespace WFMusic
{
    public partial class MainForm : Form
    {
        private SM sm;
        private PlayList plist;
        private MusicList mlist;
        private PlayLrc plrc;
        private PlaySpectrum pspectrum;

        private bool playstate = false;
        private bool playFirst = false;


        public MainForm()
        {
            InitializeComponent();
        }

        #region 窗口事件

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                plist = new PlayList();
                mlist = new MusicList();
                plrc = new PlayLrc();
                pspectrum = new PlaySpectrum();

                mlist.Show(dockPanel1, DockState.DockTop);
                plist.Show(dockPanel1);
                plrc.Show(dockPanel1, DockState.DockRight);
                pspectrum.Show(dockPanel1, DockState.DockBottom);

                sm = new SM(this);
                sm.SetGainDB(this.skinTrackBar1.Value);
                sm.SpectrumUpdate += pspectrum.SetImg;
                sm.DelegateValueUpdateFun += PlayInfoUpdate;
                sm.ReadProcessComplete += PlayStart;
                sm.EndProcessComplete += PlayEnd;
                sm.SetSpectrumArea(pspectrum.PBoxWidth, pspectrum.PBoxHeight);

                pspectrum.SpectrumTypeUpdate += sm.SetSpectrumType;
                pspectrum.SpectrumAreaUpdate += sm.SetSpectrumArea;

                plist.Play += MusicPlay;

                FormInit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //窗体最小化时
            if (this.WindowState == FormWindowState.Minimized)
            {

            }

            //窗体恢复正常时
            if (this.WindowState == FormWindowState.Normal)
            {

            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (bm != null)
            //  MessageBox.Show("确定退出吗？");
            sm.CloseBass();
        }

        private void FormInit()
        {
            this.toolTip3.SetToolTip(this.pictureBox5, plist.PlayModeName);
            this.label1.Text = (plist.PlayingFileName);
            this.label2.Text = "00:00 / 00:00";
            this.Text = "WFMusic";

            pspectrum.SetImg(null);
            this.skinTrackBar2.Value = 0;
        }

        #endregion

        #region 文件管理

        //添加文件到播放列表
        private void AddFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plist.AddMusic(Files.openFile());

        }

        //添加文件夹到歌曲列表
        private void 添加文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mlist.AddFiles(Files.openDir());

        }

        #endregion

        #region 回调

        //进度及运行时间更新
        private void PlayInfoUpdate(int pos, string time, string cpu)
        {
            try
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    this.skinTrackBar2.Value = pos;
                    this.label2.Text = time;
                    this.Text = string.Format("WFMusic {0}", cpu);
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //播放
        private void PlayStart()
        {
            sm.Start();
            this.pictureBox2.BackgroundImage.Dispose();
            this.pictureBox2.BackgroundImage = Image.FromFile(Application.StartupPath + "/res/icons/pause.png");
            playstate = true;
        }

        private void PlayPause()
        {
            sm.Pause();
            this.pictureBox2.BackgroundImage.Dispose();
            this.pictureBox2.BackgroundImage = Image.FromFile(Application.StartupPath + "/res/icons/play.png");
            playstate = false;
        }

        private void MusicPlay(string file)
        {
            //bm.Pause();
            sm.SelectFile(file);
            this.label1.Text = plist.PlayingFileName;
        }

        private void PlayEnd()
        {
            try
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    FormInit();
                    PlayPause();

                    //判断当前播放模式,并根据设置类型进行后续播放

                    plist.PlayCompleteCallBak();
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            sm.SetPosition(0);

        }

        #endregion

        #region 帮助

        private void 检查更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UpdateForm updateForm = new UpdateForm();
                    updateForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        #endregion

        #region 声音

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            bool state = this.skinTrackBar1.Visible;
            state = !state;
            this.skinTrackBar1.Visible = state;
        }

        private void skinTrackBar1_Scroll(object sender, EventArgs e)
        {
            int val = this.skinTrackBar1.Value;
            int min_val = this.skinTrackBar1.Minimum;
            int max_val = this.skinTrackBar1.Maximum;
            int interval = Math.Abs((max_val - min_val) / 3);
            int level_0 = min_val, level_1 = (min_val + interval), level_2 = (max_val - interval), level_3 = max_val;

            Image img = null;

            if (val == min_val)
            {
                //音量关闭
                img = Image.FromFile(Application.StartupPath + "/res/icons/soundoff.png");
            }
            else if (val > level_0 && val < level_1)
            {
                //小音量
                img = Image.FromFile(Application.StartupPath + "/res/icons/soundmin.png");
            }
            else if (val > level_1 && val < level_2)
            {
                //中音量
                img = Image.FromFile(Application.StartupPath + "/res/icons/soundnormal.png");
            }
            else
            {
                //高音量
                img = Image.FromFile(Application.StartupPath + "/res/icons/soundmax.png");
            }

            sm.SetGainDB(val);

            if (pictureBox6.BackgroundImage != null)
            {
                pictureBox6.BackgroundImage.Dispose();
            }
            this.pictureBox6.BackgroundImage = img;

        }

        private void skinTrackBar1_MouseLeave(object sender, EventArgs e)
        {
            this.skinTrackBar1.Visible = false;
        }

        #endregion

        #region 播放控制

        private void skinTrackBar2_Scroll(object sender, EventArgs e)
        {
            if (plist.PlayingFileName == null)
                return;

            PlayPause();
            Thread.Sleep(5);
            sm.SetPosition(this.skinTrackBar2.Value);
        }

        private void skinTrackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            if (plist.PlayingFileName == null)
                return;
            PlayStart();
        }

        //播放/暂停
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (playstate == false)
            {
                if (playFirst == false)
                {
                    if (plist.PlayingFilePath != null)
                    {
                        MusicPlay(plist.PlayingFilePath);
                        playFirst = true;
                    }
                    else
                    {
                        MessageBox.Show("请选择一首曲目！");
                    }
                }
                else
                {
                    PlayStart();
                }
            }
            else
            {
                PlayPause();
            }
        }

        //上一首
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            plist.PrevMusic();
        }

        //下一首
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            plist.NextMusic();
        }

        //设置播放模式
        private void pictureBox5_Click(object sender, EventArgs e)
        {

            Image img = null;

            plist.PlayMode++;

            if (this.pictureBox5.BackgroundImage != null)
                this.pictureBox5.BackgroundImage.Dispose();
           
            switch (plist.PlayMode)
            {
                case 0:
                    img = Image.FromFile(Application.StartupPath + "/res/icons/singlecycle.png");
                    break;
                case 1:
                    img = Image.FromFile(Application.StartupPath + "/res/icons/playlist.png");
                    break;
                case 2:
                    img = Image.FromFile(Application.StartupPath + "/res/icons/listcycle.png");
                    break;
                case 3:
                    img = Image.FromFile(Application.StartupPath + "/res/icons/randomplay.png");
                    break;
            }

            this.pictureBox5.BackgroundImage = img;
            this.toolTip3.SetToolTip(this.pictureBox5, plist.PlayModeName);

        }

        #endregion
    }
}
