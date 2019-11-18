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
            // Returns an instance of the Logger class, as we have
            // registered the Logger implementation type for our ILogger abstraction.
            var logger1 = sp.GetService(typeof(ILogger));

            // Same as the previous example. The difference is that we have provided
            // the type as a generic parameter. This is a more convenient approach.
            // To use this we have to import the Microsoft.Extensions.DependencyInjection 
            // namespace via the using statement.
            // Returns an instance of the Logger class, see explanation above.
            var logger2 = sp.GetService<ILogger>();
            // GetService returns null if no type mapping is found for the specific type (ILogger)
            // GetRequiredService throws an exception instead.
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
