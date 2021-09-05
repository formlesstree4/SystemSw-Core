using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System8.Communicator;
using SystemCommunicator.Devices;

namespace System8.React
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSingleton<SerialCommunicationDevice>();
            services.AddSingleton<FakeCommunicationDevice>();
            services.AddSingleton<ExtronSystem8Communicator>();
            services.AddSingleton<ICommunicationDevice>((provider) =>
            {
                return Configuration.GetSection("Extron")["Mode"]?.ToLowerInvariant() switch
                {
                    "serial" => provider.GetRequiredService<SerialCommunicationDevice>(),
                    "development" => provider.GetRequiredService<FakeCommunicationDevice>(),
                    _ => throw new System.ArgumentOutOfRangeException("Mode", "Invalid Mode Specified")
                };
            });
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
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
            }
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });
            app.UseCors((b) => {
                b.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
