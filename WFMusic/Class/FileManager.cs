using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LogManager;
using System.Windows.Forms;

namespace FileManager
{
    /// <summary>
    /// 文件操作类
    /// </summary>
    public class Files
    {

        public enum FileType
        {
            TYPE_TXT = 0,
            TYPE_INI = 1,
            TYPE_XLS = 2,
            TYPE_XLSX = 3,
        };

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="filepath"></param>
        public static bool CreatFile(string filepath)
        {
            try
            {
                if (!Files.ExistFile(filepath))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(filepath)))
                    {
                        DirectoryInfo fdi = Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                        // fdi.Attributes= FileAttributes.ReadOnly;
                    }
                    using (FileStream fs = File.Create(filepath))
                    {
                        
                    }
                    return true;

                }
            }
            catch(Exception e)
            {
                Log.Error(DateTime.Now.ToString("hh:mm:ss") + "：文件创建失败==>" + e.Message);
            }
            return false;
        }

        /// <summary>
        /// 打开文件所在位置
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFolderAndSelectFile(String path)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + path;
            System.Diagnostics.Process.Start(psi);
        }

        /// <summary>
        /// 文件复制
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="destDir"></param>
        public static void CopyFile(string srcFile, string destDir)
        {
            try
            {
                DirectoryInfo destDirectory = new DirectoryInfo(destDir);
                string fileName = Path.GetFileName(srcFile);
                if (!Files.ExistFile(srcFile))
                {
                    Log.Error(DateTime.Now.ToString("hh:mm:ss") + "：文件复制失败，原因==>源文件不存在...");
                    return;
                }
                else
                {
                    Log.Info(DateTime.Now.ToString("hh:mm:ss") + "：找到源文件，开始复制...");
                    Console.WriteLine(Path.GetDirectoryName(destDir));
                }

                Console.WriteLine(Path.GetDirectoryName(destDir));

                if (!Directory.Exists(Path.GetDirectoryName(destDir)))
                {
                    Log.Info(DateTime.Now.ToString("hh:mm:ss") + "：创建目标目录...");
                    DirectoryInfo fdi = Directory.CreateDirectory(Path.GetDirectoryName(destDir));
                    // fdi.Attributes= FileAttributes.ReadOnly;
                }

                File.Copy(srcFile, destDirectory.FullName, true);

                //检测是否完成复制
                if (ExistFile(destDirectory.FullName))
                {
                    Log.Info(DateTime.Now.ToString("hh:mm:ss") + "：文件复制完成...");
                }
            }
            catch (Exception e)
            {
                Log.Error(DateTime.Now.ToString("hh:mm:ss") + "：文件复制失败==>"+e.Message);
            }

        }

        /// <summary>
        /// 判断是否为文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public static void DeleteFile(string filepath)
        {
            //首先判断文件或者文件路径是否存在
            if (Files.ExistFile(filepath))
            {
                //根据路径字符串判断是文件还是文件夹
                if (IsDirectory(filepath))
                {
                    //删除文件夹
                    Directory.Delete(filepath, true);
                }
                else
                {
                    //删除文件
                    File.Delete(filepath);
                }
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="dirpath"></param>
        public static bool DeleteDirectory(string dirpath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dirpath);
                FileAttributes attr = File.GetAttributes(dirpath);
                if (di.Exists)
                {
                    Directory.Delete(dirpath, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + "：文件夹删除失败==>" + e.Message);
            }
            return false;
        }

        /// <summary>
        /// 验证文件是否存在
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool ExistFile(string filepath)
        {
            return File.Exists(filepath);
        }

        /// <summary>
        /// 打开文件选择窗口
        /// </summary>
        /// <returns>返回文件地址</returns>
        public static string[] openFile()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;//多选文件
                dialog.Title = "请选择文件";
                dialog.Filter = "Audio Files (*.mp3;*.ogg;*.wav)|*.mp3;*.ogg;*.wav|All Files(*.*)|*.*";
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    return dialog.FileNames;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <returns></returns>
        public static string openDir()
        {
            try
            {
                FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
                folder.Description = "请选择文件夹";
                if (DialogResult.OK == folder.ShowDialog())
                {
                    return folder.SelectedPath;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return "";
        }

        /// <summary>
        /// 打开文本文件
        /// </summary>
        /// <param name="path"></param>
        public static void openFile(string path, FileType type)
        {
            switch (type)
            {
                case FileType.TYPE_TXT:
                case FileType.TYPE_INI:
                    System.Diagnostics.Process.Start("notepad.exe", path);
                    break;
                case FileType.TYPE_XLS:
                case FileType.TYPE_XLSX:
                    break;
            }
        }

        /// <summary>
        /// 文件保存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        public static void saveFile(string title, string path, string type)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*." + type + ")" + "|*." + type;
            sfd.RestoreDirectory = true;
            sfd.Title = title;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //复制文件到指定位置
                CopyFile(path, sfd.FileName.ToString());
            }
        }

        #region 查找目录下包含子目录的全部文件
        /// <summary>
        /// 获得目录下所有文件或指定文件类型文件地址
        /// </summary>
        public static List<string> fileList = new List<string>();
        public static string[] GetFiles(string fullPath, string extName, bool isFullName = false)
        {
            try
            {
                fileList.Clear();

                DirectoryInfo dirs = new DirectoryInfo(fullPath); //获得程序所在路径的目录对象
                DirectoryInfo[] dir = dirs.GetDirectories();//获得目录下文件夹对象
                FileInfo[] file = dirs.GetFiles();//获得目录下文件对象
                int dircount = dir.Count();//获得文件夹对象数量
                int filecount = file.Count();//获得文件对象数量

                //循环文件夹
                for (int i = 0; i < dircount; i++)
                {
                    string pathNode = fullPath + "\\" + dir[i].Name;
                    GetMultiFile(pathNode, isFullName);
                }

                //循环文件
                for (int j = 0; j < filecount; j++)
                {
                    if (isFullName)
                    {
                        fileList.Add(file[j].FullName);
                    }
                    else
                    {
                        fileList.Add(file[j].Name);
                    }
                }

                return fileList.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n出错的位置为：Form1.PaintTreeView()");
            }

            return null;
        }

        private static bool GetMultiFile(string path, bool isFullName = false)
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

            //循环文件夹
            for (int j = 0; j < dircount; j++)
            {
                string pathNodeB = path + "\\" + dir[j].Name;
                GetMultiFile(pathNodeB, isFullName);
            }

            //循环文件
            for (int j = 0; j < filecount; j++)
            {
                if (isFullName)
                {
                    fileList.Add(file[j].FullName);
                }
                else
                {
                    fileList.Add(file[j].Name);
                }
            }
            return true;
        }

        #endregion

        public static string[] getFileList(string path, string extName, bool isFullName = false)
        {
            try
            {
                List<string> lst = new List<string>();
                string[] dir = Directory.GetDirectories(path); //文件夹列表   
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表  
                 
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空                   
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件   
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0 || extName == "")
                        {
                            if (isFullName)
                            {
                                lst.Add(f.FullName);
                            }
                            else
                            {
                                lst.Add(f.Name);
                            }
                        }
                    }

                    foreach (string d in dir)
                    {
                        getFileList(d, extName, isFullName);//递归   
                    }
                }
                return lst.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
