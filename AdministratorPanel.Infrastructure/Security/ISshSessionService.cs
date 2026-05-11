using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministratorPanel.Infrastructure.Security
{
    public interface ISshSessionService
    {
        bool IsAuthenticated { get; }

        string UserName { get; }

        string Password { get; }

        void Login(
            string userName,
            string password);

        void Logout();
    }
}

