using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using FileManager;

namespace LogManager
{
    /// <summary>
    /// 日志存储类
    /// </summary>
    public class Log
    {
        public enum Level
       {
            Info=0,
            Warning,
            Error
       }

        private static Level level = Level.Info;
        //保存log文件到文档
        private static string logfile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WFMusic\\Log\\"+ "log.txt";

        public static void Info(params object[] values)
        {//不管是什么类型，统统输出toString()的结果
            string info = "";
            for (int i = 0; i < values.Length; i++)
            {
                info += "[" + ((values[i].ToString() == null) ? "" : values[i].ToString()) + "]";
            }
            WriteLine(Level.Info, info);
        }
        public static void Warning(params object[] values)
        {//不管是什么类型，统统输出toString()的结果
            string warning = "";
            for (int i = 0; i < values.Length; i++)
            {
                warning += "[" + ((values[i].ToString() == null) ? "" : values[i].ToString()) + "]";
            }
            WriteLine(Level.Warning, warning);
        }
        public static void Error(params object[] values)
        {//不管是什么类型，统统输出toString()的结果
            string error = "";
            for (int i = 0; i < values.Length; i++)
            {
                error += "[" + ((values[i].ToString() == null) ? "" : values[i].ToString()) + "]";
            }
            WriteLine(Level.Error, error);
        }
        public static void Info(string info)
        {
            WriteLine(Level.Info,info);
        }
        public static void Error(string error)
        {
            WriteLine(Level.Error, error);
        }
        public static void Warning (string warning)
        {
            WriteLine(Level.Warning, warning);
        }
        public static void Read(string name)
        {
            Files.openFile(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\recircleBin\\Log\\" + name, Files.FileType.TYPE_TXT);
        }
        private static void WriteLine(Level mlevel, string info)
        {
            try
            {
                string path = Path.GetDirectoryName(logfile) + "\\log.txt";
#if DEBUG
                Console.WriteLine(logfile);
                Console.WriteLine(path);
#endif
                string time = DateTime.Now.ToString();
                if (mlevel >= level)
                {
                    if (!Files.ExistFile(logfile))
                    {
                        if(!Directory.Exists(Path.GetDirectoryName(logfile)))
                        {
                            DirectoryInfo fdi = Directory.CreateDirectory(Path.GetDirectoryName(logfile));
                           // fdi.Attributes= FileAttributes.ReadOnly;
                        }
                        using (FileStream fs = File.Create(logfile))
                        {//新建完然后退出

                        } 
                        // FileAttributes MyAttributes = File.GetAttributes(path);
                    }
                    FileInfo fileinfo = new FileInfo(logfile);
                    if (fileinfo.Length > 1023 * 1024)
                    {
                        File.Move(logfile, System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\recircleBin\\Log\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "log.txt");

                        if (!File.Exists(logfile))
                        {
                            using (File.Create(logfile)) { }
                        }
                    }
                    using (StreamWriter sr = File.AppendText(logfile))
                    {
                        sr.WriteLine("[" + time + "] " + "[" + mlevel.ToString() + "] " + info);
                        sr.Close();
                    }
                    //File.SetAttributes(path, MyAttributes | FileAttributes.Hidden);//设置添加隐藏文件夹 

                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine("logerror: " + e.Message);
#endif
            }
        }
        public static List<string> logSearch()
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\WFMusic\\Log\\";
            List<string> logList = new List<string>();

            DirectoryInfo folder = new DirectoryInfo(path);

            foreach (FileInfo file in folder.GetFiles("*.txt"))
            {
                logList.Add(file.Name);
            }

            return logList;
        }

    }
}
