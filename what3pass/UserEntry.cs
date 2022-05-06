using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace what3pass
{
    internal class UserEntry
    {
        private string _platform;
        private string _username;
        private string _password;

        public UserEntry(string platform, string username, string password)
        {
            Platform = platform;
            Username = username;
            Password = password;
        }

        public string Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }

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
    }
}
