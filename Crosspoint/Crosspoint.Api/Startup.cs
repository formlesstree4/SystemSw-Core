using Crosspoint.Communicator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SystemCommunicator.Devices;

namespace Crosspoint.Api
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Crosspoint.Api", Version = "v1" });
            });
            services.AddSingleton<SerialCommunicationDevice>();
            services.AddSingleton<TelnetCommunicationDevice>();
            services.AddSingleton<ICommunicationDevice>((provider) =>
            {
                return Configuration.GetSection("Extron")["Mode"]?.ToLowerInvariant() switch
                {
                    "serial" => provider.GetRequiredService<SerialCommunicationDevice>(),
                    "telnet" => provider.GetRequiredService<TelnetCommunicationDevice>(),
                    _ => throw new System.ArgumentOutOfRangeException("Mode", "Invalid Mode Specified")
                };
            });
            services.AddSingleton<ExtronCrosspointCommunicator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crosspoint.Api v1"));
            }
            app.UseCors((b) => {
                b.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
