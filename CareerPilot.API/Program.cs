using CareerPilot.API;
using CareerPilot.API.AgentOrchestration;
using CareerPilot.API.CareerPIlotDbContext;
using CareerPilot.API.Service.Implementation;
using CareerPilot.API.Service.Interface;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddDbContext<CareerPilotDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("CareerPilotDb")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStorageService, StorageService>();

// Register Cloudinary with configuration
builder.Services.AddSingleton(sp =>
{
	var configuration = sp.GetRequiredService<IConfiguration>();
	var cloudName = configuration["Cloudinary:CloudName"];
	var apiKey = configuration["Cloudinary:ApiKey"];
	var apiSecret = configuration["Cloudinary:ApiSecret"];

	if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
	{
		throw new InvalidOperationException("Cloudinary configuration is missing in appsettings.json.");
	}

	var account = new Account(cloudName, apiKey, apiSecret);
	return new Cloudinary(account);
});

#pragma warning disable SKEXP0110 // Suppress experimental warning for JobAssistantSystem
builder.Services.AddSingleton<JobAssistantSystem>();
#pragma warning restore SKEXP0110

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();