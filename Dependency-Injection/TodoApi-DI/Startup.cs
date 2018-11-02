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
        
            // Beregisztrálja a konténerbe a TodoContext DBContext-et TodoContext típusként
            // , vagyis itt nem használunk interfészt (de az interfész bevezetése itt nem is adna
            // hozzá semmi pluszt, a TodoContext-et nem akarjuk absztrahálni, ő maga már több
            // DB providerrel tud dolgozni (pl. memória, MQSQL, stb)
            services.AddDbContext<TodoContext>(opt => 
                opt.UseInMemoryDatabase("TodoList"));

            // Az Mcv keretrendszer szolgáltatásait regisztrálja be, a Web Api miatt használjuk
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            #endregion

            #region Saját szolgáltatások típusok/regisztrálása

            // #3.1 SZOLGÁLTATÁSOK BEREGISZTRÁLÁSA
            // Három különböző módon regisztrálhatunk leképezéseket
            // * AddSingleton: minden feloldás során ugyanazt az objektumot adja vissza
            // * AddTransient: minden feloldás során új objektumot ad vissza
            // * AddScoped: a feloldás során egy hatókörön belül ugyanazt az objektumot adja vissza 
            //   (Web API esetén adott kérésen belül mindig ugyanazt adja vissza)

            // A Logger implementációt ILogger interfészként regisztráljuk be, a példa kedvvért Singleton-ként
            // Később a konténertől ILogger-t kérve egy Logger objektumot ad vissza
            services.AddSingleton<ILogger, Logger>();
            // A NotificationService implementációt INotificationService interfészként regisztráljuk be, a példa
            // kedvéért tranzens módon.
            // Később a konténertől INotificationService-t kérve egy NotificationService objektumot ad vissza, 
            // minden lekérdezésre újat.
            services.AddTransient<INotificationService, NotificationService>();
            // A ContactRepository implementációt IContactRepository interfészként regisztráljuk be, a példa
            // kedvéért scope-olt módon.
            // Később a konténertől IContactRepository-t kérve egy ContactRepository objektumot ad vissza, 
            // egy hatókörön belül (esetünkben egy API kérésen belül) ugyanazt.
            services.AddScoped<IContactRepository, ContactRepository>();

            // Ez "trükkös", mert az EMailSender második paramétere egy string, az smtp szerver címe,
            // ezt a resolve (GetServive) nem tudja a string típus alapján feloldani. 
            // A megoldásunk az, hogy egy lambda kifejezésel megadjuk, hogy kell a resolve során
            // példányosítani az objektumot. A resolve során a rendszer meghívja a lambdát, sp paraméterben
            // egy ServiceProvider-t kapunk, aminek a GetRequiredService hívásával szerzünk egy már 
            // beregisztrált ILogger implementációt. 
            // A gyakorlatban az smtp szerver címét konfigurációból olvassuk fel, ehhez az "options" 
            // technikát ajánja az ASP.NET Core, mi ezt a megközelítést nem valósítjuk meg. 
            services.AddSingleton<IEMailSender, EMailSender>( sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com") );

            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles(); // Required for the UI part
            app.UseStaticFiles();  // Required for the UI part
            app.UseMvc();

            // Az IApplicationBuilder.ApplicationServices segítségével itt el tudjuk érni a konténert 
            // IServiceProvider-ként, a feloldás (resolve) tesztelésére.
            // new ServiceProviderDemos().SimpleResolve(app.ApplicationServices);
            
            // new ServiceProviderDemos().ObjectGraphResolve(app.ApplicationServices);

            //app.UseMvc();
        }
    }
}
