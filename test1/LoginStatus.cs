using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp
{
    public enum LoginStatus
    {
        LoggedIn,
        LoggedOut,
        WrongPassword,
        MaxLimit,
        LogInAgain,
        DataLimit,
        NoConnection
    }
}
