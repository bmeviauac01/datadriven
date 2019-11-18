using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


namespace TodoApi.Services
{
    public class ServiceProviderDemos
    {
        public void SimpleResolve(IServiceProvider sp)
        {
            // Mivel az ILogger típushoz a Logger osztályt regisztráltuk,
            // egy Logger példánnyal tér vissza.
            var logger1 = sp.GetService(typeof(ILogger));

            // A típus generikus paraméterben is megadhatjuk, kényelmesebb, ezt szoktuk  használni.
            // Ehhez szükség van a Microsoft.Extensions.DependencyInjection névtér using-olására, 
            // mert ez a GetService forma ott definiált extension methodként.
            // Mivel az ILogger típushoz a Logger osztályt regisztráltuk,
            // egy Logger példánnyal tér vissza.
            var logger2 = sp.GetService<ILogger>();
            // Míg a GetService null-t ad vissza, ha nem sikerül feloldani a konténer alapján a hivatkozást, 
            // a GetRequiredService kivételt dob.
            var logger3 = sp.GetRequiredService<ILogger>();
            // ...
        }

        public void ObjectGraphResolve(IServiceProvider sp)
        {
            var notifService = sp.GetService<INotificationService>();
            // ...
        }
    }
}
