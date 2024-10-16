using Microsoft.AspNetCore.HttpLogging;
using SabaSimulator.API.DependencyInjection;
using SabaSimulator.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddHttpClient<ConfirmBetService>();
builder.Services.AddHttpClient<CancelBetService>();
builder.Services.AddHttpClient<PlaceBetService>();
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

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/");
app.Run();
