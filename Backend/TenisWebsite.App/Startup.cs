using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisWebsite.Data.Sql;
using TenisWebsite.Data.Sql.DAO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TenisWebsite.Api.Validation;
using TenisWebsite.IServices.User;
using TenisWebsite.Services.User;
using TenisWebsite.IData.User;
using TenisWebsite.Data.Sql.Migrations;
using TenisWebsite.Data.Sql.User;
using TenisWebsite.Api.Middlewares;
using TenisWebsite.Api.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication.Certificate;
using System.Web.Http;
using Microsoft.AspNetCore.Cors;

namespace TenisWebsite.Api
{
    public class Startup
    {
        
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
           services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                                  builder =>
                                  {
                                      builder.WithOrigins("file://");
                                      builder.AllowAnyOrigin();
                                      builder.AllowAnyMethod();
                                      builder.AllowAnyHeader();
                                  });
            });
            services.AddAuthentication(
                    CertificateAuthenticationDefaults.AuthenticationScheme)
                     .AddCertificate();
            services.AddHealthChecks();

            services.AddDbContext<TenisWebsiteDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("TenisWebsiteDbContext")));
            services.AddIdentity<IdentityUser, IdentityRole>(options=> 
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 2;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.AllowedUserNameCharacters = "a�bc�de�fghijkl�mn�o�pqrs�tuvwxyz��A�BC�DE�FGHIJKL�MN�O�PRS�TUWYZ��1234567890-_$.";
            }) 
           .AddEntityFrameworkStores<TenisWebsiteDbContext>().AddDefaultTokenProviders();
            services.AddTransient<DatabaseSeed>();
            
            services.AddRouting();
            services.AddScoped<IValidator<IServices.Request.CreatUser>, CreateUserValidator>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddControllers();            
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.UseApiBehavior = false;
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<TenisWebsiteDbContext>();
                var databaseSeed = serviceScope.ServiceProvider.GetRequiredService<DatabaseSeed>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                databaseSeed.Seed();
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           

            app.UseStaticFiles();
           
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();

            });
            app.UseApiVersioning();
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }

    }
}