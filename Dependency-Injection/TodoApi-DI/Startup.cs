using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            #region Keretrendszer szolgáltatások típusok/regisztrálása

            // Registers TodoContext DBContext into the container with TodoContext as key.
            // We don't use an interface type as key here (it would not have any benefit). 
            services.AddDbContext<TodoContext>(opt => 
                opt.UseInMemoryDatabase("TodoList"));

            // Registers Mvc based services into the container, needed for Web API
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            #endregion

            #region Saját szolgáltatások típusok/regisztrálása

            // #3.1 REGISTER MAPPINGS
            // We can register mappings three different ways
            // * AddSingleton: returns the same instance for all resolutions
            // * AddTransient:  returns a different instance for different resolutions
            // * AddScoped: returns the same instance for the same scope
            //   (one web api request is served within the context of the same scope)

            // Registers an ILogger->Logger mapping, as singleton.
            // Later at resolution, we will get a Logger instace when we ask for an ILogger implementation
            services.AddSingleton<ILogger, Logger>();
            // Registers an INotificationService->NotificationService mapping, as transient.
            // Later at resolution, we will get a NotificationService instace when we ask for an INotificationService implementation
            services.AddTransient<INotificationService, NotificationService>();
            // Registers an IContactRepository->ContactRepository mapping, as scoped.
            // Later at resolution, we will get a ContactRepository instace when we ask for an IContactRepository implementation
            services.AddScoped<IContactRepository, ContactRepository>();

            /*
           EMailSender will need to be instantiated by the container when resolving IEMailSender, and the constructor 
           parameters must be specified appropriately. The logger parameter is completely "OK", and the container can 
           resolve it based on the ILogger-> Logger container mapping registration. However, there is no way to find out
           the value of the smtpAddress parameter. To solve this problem, ASP.NET Core proposes an "options" mechanism
           for the framework, which allows us to retrieve the value from some configuration. Covering the "options" topic 
           would be a far-reaching thread for us, so for simplification we applied another approach. The AddSingleton 
           (and other Add ... operations) have an overload in which we can specify a lambda expression. This lambda is 
           called by the container later at the resolve step (that is, when we ask the container for an IEMailSender 
           implementation) for each instance. With the help of this lambda we manually create the EMailSender object, 
           so we have the chance to provide the necessary constructor parameters. In fact, the container is really 
           "helpful" with us: it provides an IServiceCollection object as the lambda parameter for us (in this example
           it's called sp), and based on container registrations we can conveniently resolve types with the help of 
           the already covered GetRequiredService and GetService calls.
            */
            services.AddSingleton<IEMailSender, EMailSender>( sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com") );

            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles(); // Required for the UI part
            app.UseStaticFiles();  // Required for the UI part
            app.UseMvc();

            // With the help of IApplicationBuilder.ApplicationServices we can ask for the container
            // which we have access to as IServiceProvide, and we can use it to resolve objects.
            // new ServiceProviderDemos().SimpleResolve(app.ApplicationServices);
            
            // new ServiceProviderDemos().ObjectGraphResolve(app.ApplicationServices);

            //app.UseMvc();
        }
    }
}
