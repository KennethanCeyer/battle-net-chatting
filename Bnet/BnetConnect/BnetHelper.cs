using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bnet.BnetConnect
{
    internal class BnetHelper
    {
        private static BnetHelper instance;

        protected BnetHelper()
        {
        }

        public static BnetHelper getInstance()
        {
            if(instance == null)
            {
                instance = new BnetHelper();
            }
            return instance;
        }

        public String Acsii2Hex(String ascii)
        {
            String hex = "";
            foreach (char c in ascii)
            {
                hex += ((int) c).ToString("X");
            }
            return hex;
        }
    }
}
