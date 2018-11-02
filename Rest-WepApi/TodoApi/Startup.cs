using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Models;

namespace TodoApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TodoContext>(opt => 
                opt.UseInMemoryDatabase("TodoList"));
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


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
