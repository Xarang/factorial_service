using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//debug
using System.Diagnostics;

using FactorialService;


namespace aspnetcoreapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            service = new FactorialService.Service();
        }

        public IConfiguration Configuration { get; }
        private FactorialService.Service service { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                /*
                 * call factorial service
                 */
                endpoints.MapGet("/{n:int}", async context =>
                {
                    int n = Int32.Parse((string)context.Request.RouteValues["n"]);
                    FactorialService.Service.FactorialDTO res = service.getFactorial(n).Task.Result;
                    await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes(res.ToJson()));
                });

                /*
                 * print currently stored values
                 */
                endpoints.MapGet("/values", async context =>
                {
                    string values = service.getResults();
                    await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes(values));
                });

                /*
                 * make a backup of computed values into a .json file
                 */
                endpoints.MapGet("/save", async context =>
                {
                    string res = service.Save();
                    await context.Response.Body.WriteAsync(Encoding.ASCII.GetBytes(res));
                });
            });

        }
    }
}
