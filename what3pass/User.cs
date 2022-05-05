using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace what3pass
{
    public class User
    {
        private int _uniqueId;
        private string _username;
        private string _email;

        public User(string username, int id)
        {
            Username = username;
            Id = id;
        }

        public int Id
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
    }
}
