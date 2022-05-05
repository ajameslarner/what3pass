using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace what3pass
{
    internal class Entry
    {
        private string _username;
        private string _password;
        private string _platform;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }
    }
}
