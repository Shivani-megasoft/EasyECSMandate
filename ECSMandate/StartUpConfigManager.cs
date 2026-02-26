using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECSMandate
{
    public class StartUpConfigManager
    {
        private static IConfigStartUp configStartUp;
        private static object lk = new object();
        private static object lk2 = new object();
        private StartUpConfigManager()
        {
        }

        public static IConfigStartUp ConfigStartUp
        {
            get
            {
                lock (lk)
                {
                    lock (lk2)
                    {

                        if (null == configStartUp)
                        {
                            configStartUp = new WebConfigStartUp();
                        }
                    }
                }
                return configStartUp;
            }

        }
    }
}