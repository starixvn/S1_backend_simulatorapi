using Microsoft.AspNetCore.HttpLogging;
using Refit;
using SabaSimulator.API.ApiClient;
using SabaSimulator.API.ApiClient.Handler;
using SabaSimulator.API.CronJob;
using SabaSimulator.API.DependencyInjection;
using SabaSimulator.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging(o => { });

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
	// ...
	options.SuppressModelStateInvalidFilter = true;
	// ...
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache(options =>
{
	options.ExpirationScanFrequency = TimeSpan.FromMinutes(10);
});
builder.Services.AddSwaggerGen();
builder.Services.AddSabaPlusApiCore();
builder.Services.AddCors();
builder.Services
	.AddHttpLogging(logging => logging.LoggingFields = HttpLoggingFields.All)
	.AddHealthChecks();

builder.Services.AddScoped<LoggingHandler>();
builder.Services.AddRefitClient<ISabaSportApi>()
	.ConfigureHttpClient(client =>
	{
		client.BaseAddress = new Uri(builder.Configuration["BetService:BaseUrl"]!);
		client.Timeout = TimeSpan.FromSeconds(30);
	}).AddHttpMessageHandler<LoggingHandler>();

builder.Services.AddScoped<SabaPlusHandler>();
builder.Services.AddRefitClient<ISabaPlusApi>()
	.ConfigureHttpClient(client => client.BaseAddress = new Uri(builder.Configuration["BetService:BaseSabaPlusUrl"]!))
	.AddHttpMessageHandler<SabaPlusHandler>();

builder.Services.AddHttpClient<SabaPlusEndGameService>();
builder.Services.AddHttpClient<SabaPlusFreeSpinService>();
builder.Services.AddHttpClient<SabaPlusPlaceBetAndSettleService>();
builder.Services.AddHttpClient<SabaPlusVoidGameService>();

builder.Services.AddHostedService<ClearMemoryDbJob>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(x =>
	x.AllowAnyMethod()
		.AllowAnyHeader()
		.SetIsOriginAllowed(origin => true)
		.AllowCredentials());

app.UseHttpsRedirection();
app.UseHttpLogging();

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/");
app.Run();