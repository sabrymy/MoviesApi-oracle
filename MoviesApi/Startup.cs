using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoviesApi.Filters;
using MoviesApi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.OleDb;
using MoviesApi.Controllers;

namespace MoviesApi
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
            // services.AddControllers(options=> options.Filters.Add(typeof(MyExceptionFilter) ));
            //add MyActionFilte as global filter
            //support xml format
           

           

            

            services.AddDbContext<ApplicationDbContext>(options => options.UseOracle(Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddControllers(options => options.Filters.Add(typeof(MyExceptionFilter))).AddXmlDataContractSerializerFormatters()
                .AddNewtonsoftJson();
            //scan for all mapping profiles statring from the root class
            services.AddAutoMapper(typeof(Startup));
            //save to azure
            services.AddTransient<IFileStorageService, AzureStorageService>();
            //save to folder on wwwroot
            // services.AddTransient<IFileStorageService, InAppStorageService>();
            services.AddHttpContextAccessor();
            
            //api service

            //custom filter service
            // services.AddTransient(typeof(MyActionFilter));


            //AddScoped is used Actually in most wep applications
            //    services.AddScoped<Irepository, InMemoryRepository>();
        }

       

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) 
        {
            //logging all the responses body 
           

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();


            app.UseStaticFiles();
            app.UseRouting();

            
            app.UseAuthentication();
            //   app.UseAuthorization();
            
            
            app.UseEndpoints(endpoints =>
            {
     //           endpoints.MapControllerRoute(
     //      name: "DefaultApi",
      //     pattern: "api/{Controller}/{Id?}"
       //   );

              endpoints.MapControllers();
               

            });
        }
    }
}
