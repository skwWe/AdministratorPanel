using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Infrastructure.Security
{
    public sealed class SshSessionService
        : ISshSessionService
    {
        public bool IsAuthenticated { get; private set; }

        public string UserName { get; private set; }
            = string.Empty;

        public string Password { get; private set; }
            = string.Empty;

        public void Login(
            string userName,
            string password)
        {
            UserName = userName;
            Password = password;
            IsAuthenticated = true;
        }

        public void Logout()
        {
            UserName = string.Empty;
            Password = string.Empty;
            IsAuthenticated = false;
        }
    }
}
