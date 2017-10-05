using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.API.Model
{
    public class User
    {
        public long Id { get; private set; }

        public string Login { get; private set; }

        public string Location { get; private set; }

        public string Url { get; private set; }

        public long TotalCommits { get; private set; }

        public User(
            long id, 
            string login, 
            string location, 
            string url, 
            long totalCommits)
        {
            Id = id;
            Login = login;
            Location = location;
            Url = url;
            TotalCommits = totalCommits;
        }
    }
}
