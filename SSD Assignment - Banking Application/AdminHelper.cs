using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authentication;

namespace SSD_Assignment___Banking_Application
{
    public class AdminHelper
    {
    public bool AuthenticateUser(string username, string password)
    {
        using (var context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN"))
        {
            return context.ValidateCredentials(username, password);
        }
    }

    public bool IsUserInGroup(string username, string groupName)
    {
        using (var context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN"))
        using (var user = UserPrincipal.FindByIdentity(context, username))
        {
            return user != null && user.GetGroups().Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
}
