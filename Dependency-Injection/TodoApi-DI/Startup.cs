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
            // Ez beregisztrálja a konténerbe a TodoContext DBContext-et TodoContext típusként
            // , vagyis itt nem használunk iterfészt (de az interfész bevezetése itt nem is adna
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
            // Három különböző módon regisztrálhatunk típusokat
            // * AddSingleton: minden feloldás során ugyanazt az objektumot adja vissza
            // * AddTransient: minden feloldás során új objektumot ad vissza
            // * AddScoped: egy hatókörön belül ugyanazt az objektumot adja vissza 
            //   (Web API esetén adott kérésen belül mindig ugyanazt adja vissza)

            // A Logger implementációt ILogger interfészként regisztráljuk be, a példa kedvvért Singleton-ként
            services.AddSingleton<ILogger, Logger>();
            // TODO-BZ: ne adjunk át neki egy DBContext-et?
            services.AddTransient<IContactRepository, ContactRepository>();
            services.AddTransient<INotificationService, NotificationService>();

            // Ez "trükkös", mert az EMailSender második paramétere egy string, az smtp szerver címe,
            // ezt a resolve (GetServive) nem tudja a string típus alapján feloldani. 
            // A megoldásunk az, hogy egy lambda kifejezéésel megadjuk, hogy kell a resolve során
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

            //app.UseMvc();
        }
    }
}
