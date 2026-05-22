using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Newtonsoft.Json;
using ToolsApp.App_Start;
using ToolsApp.Authentication;
using ToolsApp.Utilities;
using System.Web.Script.Serialization;


namespace ToolsApp
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Mapper.RegisterMappings();

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;


            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                        .SerializerSettings
                        .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            #region Check BE
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName + Parameters.CookieName.Cookie_Name];
            if (authCookie != null)
            {
                if (authCookie.Value.Length > 0)
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                  
                    
                    try
                    {
                        CustomPrincipalSerializeModel serializeModel = JsonConvert.DeserializeObject<CustomPrincipalSerializeModel>(authTicket.UserData);
                        if (serializeModel!=null)
                        {
                            CustomPrincipal newUser = new CustomPrincipal(serializeModel.UserName);
                            newUser.UserId = serializeModel.UserId;
                            newUser.UserName = serializeModel.UserName;
                            newUser.FullName = serializeModel.FullName;
                            newUser.IDMADV = serializeModel.IDMADV;
                            newUser.MADV = serializeModel.MADV;
                            newUser.Email = serializeModel.Email;
                            HttpContext.Current.User = newUser;
                        }                        
                    }
                    catch (Exception )
                    {

                    }
                }
            }
            #endregion            
        }

      
    }
}