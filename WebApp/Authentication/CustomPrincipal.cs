using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace ToolsApp.Authentication
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }


        public bool IsInRole(string role)
        {
            return true;
        }

        public CustomPrincipal(string UserName)
        {
            this.Identity = new GenericIdentity(UserName);
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string IDMADV { get; set; }
        public string MADV { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class CustomPrincipalSerializeModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string IDMADV { get; set; }
        public string MADV { get; set; }
        public string Email { get; set; }
        public string FORM { get; set; }
   
    }
}