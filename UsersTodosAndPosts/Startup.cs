using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersTodosAndPosts.Extensions;
using UsersTodosAndPosts.Middlewares;
using UsersTodosAndPosts.Services;

namespace UsersTodosAndPosts
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
            // Регистрация мидлваре для обработки исключений
            services.AddSingleton<ExceptionHandlingMiddleware>();

            // Регистрация сервиса по выгрузке отчета в файл
            services.AddTransient<IExportReportToFileService, ExportReportToFileService>();

            // Добавим в инфру приложения типизированные HttpClient-ы, которые будут получать данные
            var serviceUrl = Configuration.GetValue<string>("ServiceUrl");
            services.AddUsersClient(serviceUrl);
            services.AddTodosClient(serviceUrl);
            services.AddPostsClient(serviceUrl);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UsersTodosAndPosts", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UsersTodosAndPosts v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
