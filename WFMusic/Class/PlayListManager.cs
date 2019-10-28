using Shell32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayListManager
{
    class PLM
    {

        public class MusicInfo
        {
            public class MusicItem
            {
                public string 艺术家 { get; set; }
                public string 专辑 { get; set; }
                public string 名字 { get; set; }
                public string 大小 { get; set; }
                public string 比特率 { get; set; }
                public string 时长 { get; set; }
                public string 类型 { get; set; }
            }

            public class OtherInfo
            {
                public string FullName { get; set; }
            }
        }



        public List<MusicInfo.MusicItem> MusicList = new List<MusicInfo.MusicItem>(); //播放列表
        public List<MusicInfo.OtherInfo> OIList = new List<MusicInfo.OtherInfo>(); //其他信息

        private Shell32.Shell sh;

        //构造
        public PLM()
        {
            sh = new Shell();
        }

        //添加多个文件
        public int AddItems(string[] files)
        {
            if (files == null)
                return 0;

            foreach (var file in files)
            {
                MusicInfo.MusicItem mi = new MusicInfo.MusicItem();
                MusicInfo.OtherInfo oi = new MusicInfo.OtherInfo();

                Folder dir = sh.NameSpace(System.IO.Path.GetDirectoryName(file));
                FolderItem item = dir.ParseName(System.IO.Path.GetFileName(file));

                ///
                mi.艺术家 = dir.GetDetailsOf(item, 20);
                mi.专辑 = dir.GetDetailsOf(item, 14);
                mi.名字 = dir.GetDetailsOf(item, 21);
                mi.大小 = dir.GetDetailsOf(item, 1);
                mi.比特率 = dir.GetDetailsOf(item, 28);
                mi.类型 = dir.GetDetailsOf(item, 2);
                string time = dir.GetDetailsOf(item, 27);
                mi.时长 = time.Substring(time.IndexOf(":")+1);

                ///
                oi.FullName = file;

                MusicList.Add(mi);
                OIList.Add(oi);
            }
            return MusicList.Count;
        }

        //获取播放的文件地址
        public string GetFilePlayPath(int id)
        {
            return OIList[id].FullName;
        }

        //获取播放的文件名
        public string GetFileName(int id)
        {
            return MusicList[id].名字;
        }

        //删除音乐
        public void DelteItem(int id)
        {
            MusicList.RemoveAt(id);
            OIList.RemoveAt(id);
        }

        //清空列表
        public void ClearMusicList()
        {
            MusicList.Clear();
        }

        //查询列表当前歌曲数量
        public int ReturnItemCount()
        {
            return MusicList.Count;
        }

    }
}
