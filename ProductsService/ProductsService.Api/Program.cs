using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using ProductsService.BusinessLogic.Interfaces;
using ProductsService.BusinessLogic.Services;
using ProductsService.Data;
using ProductsService.Data.Migrations;
using ProductsService.BusinessLogic.Mapper;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using Quartz;

namespace ProductsService.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'products_db') BEGIN CREATE DATABASE [products_db]; END", connection);
                command.ExecuteNonQuery();
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(CreateProductsTable).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .AddTransient<FluentMigrator.Runner.Processors.ProcessorOptions>();

            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly);
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey("ProductGroupingJob");

                q.AddJob<ProductGroupingJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("ProductGroupingJob-trigger")
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromMinutes(1))
                        .RepeatForever()));
            });

            builder.Services.AddTransient<ProductService>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Service API V1");
                c.RoutePrefix = string.Empty;
            });

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            var scheduler = await app.Services.GetRequiredService<ISchedulerFactory>().GetScheduler();
            await scheduler.Start();

            app.Run();
        }
    }
}
