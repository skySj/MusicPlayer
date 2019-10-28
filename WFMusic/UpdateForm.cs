using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using httpTool;
using NetTool;
using ConsoleTool;
using FileManager;
using DefineManager;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WFMusic
{
    public partial class UpdateForm : Form
    {
        private bool downLoadState = false;
        private Version appVersion = DM.appVersion;
        private Version newVersion;

        //C:\Users\userName\AppData\Local\Temp
        static string temp = System.Environment.GetEnvironmentVariable("TEMP");
        string loadFilefolder = new DirectoryInfo(temp).FullName;

        HttpHelper downLoadManager;

        public UpdateForm()
        {
            InitializeComponent();
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            ConsoleHelper.WriteWarningLine("开始检查更新...");

            //检测本地网络
            if (!Net.LocalConnectionStatus())
            {
                ConsoleHelper.WriteErrorLine("本地网络连接已断开...");
                MessageBox.Show("本地网络连接异常，请检查！");
                this.Close();
                return;
            }
            //获取服务器端版本
            try
            {
                string requestid = DM.getId();
                string url = "http://www.wiyixiao4.com/api/MusicPlayer/index.php";
                string postData = "requestid=" + requestid + "&" + "version=" + appVersion.ToString();
                string ret = HttpPost(url, postData);

                ResultProcess(ret);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string targetUrl = e.Link.LinkData as string;
            if (string.IsNullOrEmpty(targetUrl))
                ConsoleHelper.WriteErrorLine("没有链接地址！");
            else
                System.Diagnostics.Process.Start("explorer.exe", targetUrl);
            this.Close();
        }

        public void processShow(string totalNum, string num, int proc, string speed, string remainTime, string msg)
        {
            this.skinLabel3.Text = string.Format("文件大小：{0}/{1}", num, totalNum);
            this.skinLabel2.Text = string.Format("下载进度：{0}%", proc);
            this.skinProgressBar1.Value = proc;
        }

        private void processComplete()
        {
            if (MessageBox.Show("下载完成，是否退出当前应用进行安装？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                Application.Exit();

                Files.OpenFolderAndSelectFile(loadFilefolder);
            }

        }

        private void processFailed(string msg)
        {
            ConsoleHelper.WriteErrorLine(msg);
            this.Close();
        }

        private bool KillProcess()
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName.ToUpper() == "WFMUSIC")
                {
                    p.Kill();
                    return true;
                }
            }
            return false;
        }

        //更新暂停/继续
        private void skinButton1_Click(object sender, EventArgs e)
        {

            if (downLoadState == true)
            {
                //this.skinButton1.Text = "继续";
                downLoadManager.DownLoadPause();
            }
            else
            {
                //this.skinButton1.Text = "暂停";
                downLoadManager.DownLoadContinue();

            }
            downLoadState = !downLoadState;
        }

        /********************************检测更新******************************************/
        public static string HttpPost(string Url, string postDataStr)
        {
            System.Net.HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "HTTP";
            request.ContentLength = postDataStr.Length;
            request.UseDefaultCredentials = true;
            request.ServicePoint.Expect100Continue = false;  //直接发送请求
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            writer.Write(postDataStr);
            writer.Flush();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码
            }
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            string retString = reader.ReadToEnd();
            return retString;
        }

        private void ResultProcess(string ret)
        {
            JObject retjson = JObject.Parse(ret);

            ConsoleHelper.WriteInfoLine(retjson.ToString());

            JArray jList = (JArray)JsonConvert.DeserializeObject(retjson["detail"].ToString());

            int result = Convert.ToInt32(retjson["result"]);

            if (result == 0)
            {
                string url = (string)retjson["url"];
                string name = (string)retjson["name"];
                string extension = (string)retjson["extension"];
                newVersion = new Version();
                string fileName = "";

                //版本比较
                foreach (var obj in jList)
                {
                    JObject tempo = JObject.Parse(obj.ToString());
                    Version v = new Version(tempo["version"].ToString());
                    newVersion = appVersion < v ? v : appVersion;
                }

                if (newVersion > appVersion)
                {
                    ConsoleHelper.WriteInfoLine("检测到新版本，开始下载...");

                    if (MessageBox.Show("检测到新版本，是否下载？", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {

                        //获取文件名
                        fileName = string.Format("{0}_{1}{2}", name, newVersion, extension);
                        //获取下载地址
                        string downLoadPath = string.Format("{0}{1}", url, fileName);

                        //下载目录
                        loadFilefolder = loadFilefolder + @"\" + fileName;
                        Files.DeleteFile(loadFilefolder);

                        //开始下载更新
                        Update(newVersion, downLoadPath, loadFilefolder);
                    }
                    else
                    {
                        Console.WriteLine("下载已取消!");
                        this.Close();
                    }
                }
                else
                {
                    ConsoleHelper.WriteInfoLine("当前为最新版本！");
                    //关闭窗口
                    this.Close();
                }

            }
            else
            {
                //其他错误
            }

        }

        private void Update(Version v, string url, string filePath)
        {
            downLoadState = true;
            

            this.skinLabel1.Text = string.Format("最新版本：{0}", v);

            //下载初始化
            downLoadManager = new HttpHelper(url, filePath);
            downLoadManager.timer = new System.Windows.Forms.Timer();
            downLoadManager.processShow += processShow;
            downLoadManager.processCompleted += processComplete;
            downLoadManager.processFailed += processFailed;
            downLoadManager.init();
            Thread.Sleep(1000);
            downLoadManager.DownLoadStart();
        }

        private void UpdateForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
