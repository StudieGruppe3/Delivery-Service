using deliveryServiceAPI.WorkerService;  // Korrekt namespace for Worker
using deliveryServiceAPI.Repository;     // Korrekt namespace for BookingRepository

var builder = WebApplication.CreateBuilder(args);

// 🔹 Registrer BookingRepository som singleton
builder.Services.AddSingleton<BookingRepository>();

// 🔹 Registrer Worker Service
builder.Services.AddHostedService<Worker>();

// 🔹 Tilføj API-controllere
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://0.0.0.0:8080");

var app = builder.Build();

// 🔹 Konfigurer middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
