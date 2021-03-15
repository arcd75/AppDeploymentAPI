using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPWSAppDeploymentAPINETFX
{
    public partial class Startup
    {
        public static class MyAuthentication
        {
            public const String ApplicationCookie = "ProjectAuthType";
        }
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = MyAuthentication.ApplicationCookie,
                Provider = new CookieAuthenticationProvider(),
                CookieName = "CookieAuth",
                CookieHttpOnly = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(30),
            });
        }
    }
}