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

			bool isSwaggerEnabled = builder.Configuration.GetValue<bool>("SwaggerEndpoints");
			int maxUploadSize = builder.Configuration.GetValue<int>("MaxUploadFileSizeMb") * 1_000_000;
			bool saveMemory = builder.Configuration.GetValue<bool>("SaveMemory");
			int spareWorkers = builder.Configuration.GetValue<int>("SpareWorkers");

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
			builder.Services.AddSingleton<OpenClService>(new OpenClService
			{
				PreferredDeviceName = builder.Configuration.GetValue<string>("OpenClDevice") ?? string.Empty
			});
			builder.Services.AddSingleton<ImageCollection>(new ImageCollection
			{
				ImportPath = builder.Configuration.GetValue<string>("InitialDirImageImport") ?? string.Empty,
				ExportPath = builder.Configuration.GetValue<string>("InitialDirImageExport") ?? string.Empty,
				SaveMemory = saveMemory,
			});
			builder.Services.AddSingleton<AudioCollection>(new AudioCollection
			{
				ImportPath = builder.Configuration.GetValue<string>("InitialDirAudioImport") ?? string.Empty,
				ExportPath = builder.Configuration.GetValue<string>("InitialDirAudioExport") ?? string.Empty,
				DefaultPlaybackVolume = builder.Configuration.GetValue<int>("PlaybackVolume", 100),
				AnimationDelay = 1000 / builder.Configuration.GetValue<int>("AnimationFps", 30),
				SaveMemory = saveMemory,
			});
			builder.Services.InjectClipboard();

			// Swagger/OpenAPI
			builder.Services.AddEndpointsApiExplorer();
			if (isSwaggerEnabled)
			{
				// Show full Swagger UI with endpoints
				builder.Services.AddSwaggerGen();
			}
			else
			{
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
			}

			// Request Body Size Limits
			builder.WebHost.ConfigureKestrel(options =>
			{
				options.Limits.MaxRequestBodySize = maxUploadSize;
			});

			builder.Services.Configure<IISServerOptions>(options =>
			{
				options.MaxRequestBodySize = maxUploadSize;
			});

			builder.Services.Configure<FormOptions>(options =>
			{
				options.MultipartBodyLengthLimit = maxUploadSize;
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

				if (isSwaggerEnabled)
				{
					// Show endpoints
					app.UseSwaggerUI();
				}
				else
				{
					// Show only info page about the API
					app.UseSwaggerUI(c =>
					{
						c.SwaggerEndpoint("/swagger/v1/swagger.json", "APICL v1.0");
					});
				}
			}

			app.UseStaticFiles();
			app.UseHttpsRedirection();
			app.UseCors("BlazorCors");
			app.MapControllers();

			app.Run();
		}
	}
}
