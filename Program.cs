using Serilog;
using AddressStandartization.Middleware;
using AddressStandartization.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Добавил Serilog
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.WriteTo.Debug()
	.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();

// Serilog
builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
	});
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Настройка документации апи для свагера
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHttpClient<IAddressStandardizationService, AddressStandardizationService>(client =>
{
	client.BaseAddress = new Uri(builder.Configuration["Dadata:ApiUrl"]!);
});
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionMiddleware();

app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.Run();
