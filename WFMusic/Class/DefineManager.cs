using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DefineManager
{
    public class DM
    {
        public static Version appVersion = new Version(Application.ProductVersion.ToString());

        public static String getId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
