using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CookbookAPI.ApiClients;
using CookbookAPI.ApiClients.Interfaces;
using CookbookAPI.Data;
using CookbookAPI.DTOs.MealDB;
using CookbookAPI.Entities;
using CookbookAPI.Mappers;
using CookbookAPI.Mappers.Interfaces;
using CookbookAPI.Seeders;
using CookbookAPI.Seeders.Interfaces;
using CookbookAPI.Services;
using CookbookAPI.Services.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors.Security;
using RestSharp;

namespace CookbookAPI
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
            services.AddControllers().AddFluentValidation();

            services.AddSwaggerDocument(document =>
            {
                document.Title = "Cookbook API Documentation";
                document.DocumentName = "swagger";
                document.OperationProcessors.Add(new OperationSecurityScopeProcessor("jwt"));
                document.DocumentProcessors.Add(new SecurityDefinitionAppender("jwt", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "JWT Token - remember to add 'Bearer ' before the token",
                }));
            });

            var connectionString = Configuration.GetConnectionString("LocalDb");
            services.AddDbContext<CookbookDbContext>(x => x.UseSqlServer(connectionString));

            services.AddAutoMapper(this.GetType().Assembly);
            services.AddScoped<IMealApiClient, MealApiClient>();
            services.AddScoped<IApiClient,ApiClient>();
            services.AddTransient<IRestClient, RestClient>();
            services.AddScoped<IDtoToEntityMapper<MealRecipeDto, Recipe>, MealRecipeDtoToRecipeMapper>();
            services.AddScoped<ISeeder, MealDbSeeder>();
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextService, UserContextService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISeeder seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseOpenApi(options =>
            {
                options.DocumentName = "swagger";
                options.Path = "/swagger/v1/swagger.json";
                options.PostProcess = (document, _) =>
                {
                    document.Schemes.Add(OpenApiSchema.Https);
                };
            });

            app.UseSwaggerUi3(options =>
            {
                options.DocumentPath = "/swagger/v1/swagger.json";
            });

            ConfigureAsync(seeder).Wait();
        }

        public async Task ConfigureAsync(ISeeder seeder)
        {
            await seeder.Seed().ConfigureAwait(false);
        }
    }
}
