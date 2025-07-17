using APICL.Core;
using APICL.OpenCl;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TextCopy;

namespace APICL.Api
{
    public class Program
    {
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllers();

			// CORS policy
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("BlazorCors", policy =>
				{
					policy.WithOrigins("https://localhost:23300")
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

			// Add services to the container.
			builder.Services.AddSingleton<OpenClService>();
			builder.Services.AddSingleton<ImageCollection>();
			builder.Services.AddSingleton<AudioCollection>();
			builder.Services.InjectClipboard();

			// Swagger/OpenAPI
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "APICL",
					Description = "API + WebApp using OpenCL for media manipulation",
					TermsOfService = new Uri("https://localhost:7116/terms"),
					Contact = new OpenApiContact { Name = "Developer", Email = "marcel.king91299@gmail.com" }
				});

				c.AddServer(new OpenApiServer { Url = "https://localhost:5115" });
				c.DocInclusionPredicate((_, api) => !string.IsNullOrWhiteSpace(api.GroupName));
				c.TagActionsBy(api => [api.GroupName ?? "Default"]);
			});

			// Request Body Size Limits
			builder.WebHost.ConfigureKestrel(options =>
			{
				options.Limits.MaxRequestBodySize = 500_000_000;
			});

			builder.Services.Configure<IISServerOptions>(options =>
			{
				options.MaxRequestBodySize = 500_000_000;
			});

			builder.Services.Configure<FormOptions>(options =>
			{
				options.MultipartBodyLengthLimit = 500_000_000;
			});

			// Logging
			builder.Logging.AddConsole();
			builder.Logging.AddDebug();
			builder.Logging.SetMinimumLevel(LogLevel.Debug);

			var app = builder.Build();

			// Development-only Middlewares
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger(c =>
				{
					c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
				});

				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "APICL v1.0");
				});
			}

			app.UseStaticFiles();
			app.UseHttpsRedirection();
			app.UseCors("BlazorCors");
			app.MapControllers();

			app.Run();
		}
	}
}
