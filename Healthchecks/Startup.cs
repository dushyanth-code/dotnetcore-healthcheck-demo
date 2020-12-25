using Healthchecks.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Healthchecks
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
            services.AddHealthChecks()
                    .AddSqlServer(Configuration["ConnectionStrings:dbConnectionString"], tags: new[] { "db", "all" })
                    .AddAzureBlobStorage(Configuration["ConnectionStrings:blobConnectionString"],tags: new[] { "AzureStorage", "all" })
                    .AddAzureQueueStorage(Configuration["ConnectionStrings:QueueConnectionString"], tags: new[] { "AzureStorage", "all" })
                    .AddUrlGroup(new Uri("https://testdemo.azurewebsites.net/health"),tags: new[] { "testdemoUrl", "all" });

            services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
               
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("all")
                });
                endpoints.MapHealthChecks("/health/AzureStorage", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("AzureStorage")
                });
            });
        }
    }
}
