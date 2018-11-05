using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Webshop.DAL;

namespace Webshop.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // connection string johet pl. configu fajbol
            ConnectionStringHelper.ConnectionString = @"data source=(localdb)\mssqllocaldb;initial catalog=adatvez;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
        }
    }
}
