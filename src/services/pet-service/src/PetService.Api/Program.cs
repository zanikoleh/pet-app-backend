using PetService.Application;
using PetService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var serviceBusConnectionString = builder.Configuration.GetConnectionString("AzureServiceBus")
    ?? throw new InvalidOperationException("Connection string 'AzureServiceBus' not found.");

// Add application layer
builder.Services.AddApplication();

// Add infrastructure layer with database context
builder.Services.AddInfrastructure(connectionString);

// Add event bus
builder.Services.AddAzureServiceBusEventBus(serviceBusConnectionString, "pet-service-subscription");
builder.Services.AddEventBusHostedService();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add logging
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();

}
