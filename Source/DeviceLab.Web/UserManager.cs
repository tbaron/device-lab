using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfoSpace.DeviceLab.Web
{
    public class UserManager
    {
        private static readonly Lazy<UserManager> instance = new Lazy<UserManager>();

        public static UserManager Instance
        {
            get { return instance.Value; }
        }


    }
}