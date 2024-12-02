using Business;
using DataAccess;
using DataAccess.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace ProjectAPI.API
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
            //Servicios                      
            services.AddScoped<JujuTestContext, JujuTestContext>();
            services.AddScoped<BaseService<Customer>, BaseService<Customer>>();
            services.AddScoped<BaseModel<Customer>, BaseModel<Customer>>();
            services.AddScoped<BaseService<Post>, BaseService<Post>>();            
            services.AddScoped<BaseModel<Post>, BaseModel<Post>>();


            //Agregar cadena de conexion al contexto
            //services.AddDbContext<JujuTestContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("Development")));
            services.AddDbContext<JujuTestContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("Development"), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // Número máximo de reintentos
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Tiempo máximo entre reintentos
                    errorNumbersToAdd: null
                );
            }));


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            // ======== CONFIGURACIÓN DE SWAGGER =========
            ConfigureSwagger(services);

            services.AddHttpContextAccessor();
            services.AddSession();
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "TestAPI Ivan Dario Velez Vergara", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // ======== CONFIGURACIÓN DE SWAGGER =========
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "TestAPI v1");
            });

            app.UseCors(options =>
            {
                if (env.IsDevelopment())
                {
                    options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else
                {
                    options.WithOrigins("Agregar dominio cuando salga a produccion").AllowAnyMethod().AllowAnyHeader();
                }
            });

            app.UseAuthentication();


            app.UseSession();
            app.UseMvc();
        }
    }
}