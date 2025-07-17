using APICL.Client;
using APICL.WebApp.Components;
using Radzen;
using Radzen.Blazor;

namespace APICL.WebApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Get api base url
            string? apiBaseUrl = builder.Configuration["ApiBaseUrl"];
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("'" + apiBaseUrl + "' is not configured. Please set the ApiBaseUrl configuration in appsettings.json or environment variables.");
            }

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddMvc();
            builder.Services.AddRadzenComponents();

            // Add ApiUrlConfig
            builder.Services.AddSingleton(new ApiUrlConfig(apiBaseUrl));

            // Add HttpClient
            builder.Services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            // Add ApiClient
            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>();
                return new ApiClient(httpClient.CreateClient("ApiClient"));
            });

            var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}

            // Use-configs
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.UseRouting();

            // Configure endpoints
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.MapRazorPages();

            app.Run();
}
	}

    public class ApiUrlConfig
    {
        public string BaseUrl { get; set; } = string.Empty;

        public ApiUrlConfig(string baseUrl = "")
        {
            this.BaseUrl = baseUrl;
        }
    }
}
