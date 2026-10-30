
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ExternalAttendanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExternalAttendanceDb")));

// Preserve existing in-memory mock state lifetimes.
builder.Services.AddSingleton<EasyMockState>();
builder.Services.AddSingleton<DigiMockState>();
builder.Services.AddSingleton<GenericCustomMockState>();

// Shared storage for writing/reading the daily JSONL API hit files.
builder.Services.AddSingleton<ApiHitLogStore>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply only pending EF Core migrations when enabled in configuration.
// Keep this before middleware and endpoint registration so the schema is ready
// before the API begins handling requests.
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ExternalAttendanceDbContext>();

    app.Logger.LogInformation("Checking and applying pending EF Core migrations...");
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Database migration check completed successfully.");
}

// Keep before the remaining middleware, as in the original application.
app.UseMiddleware<ApiHitLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    status = "OK",
    message = "External Attendance Mock API with DB is running."
}));

// Preserve all existing endpoints, including the attendance receiver.
app.MapStudentEndpoints();
app.MapUhfEndpoints();
app.MapEasyEndpoints();
app.MapDigiEndpoints();
app.MapGenericEndpoints();
app.MapAttendanceReceiverEndpoints();
app.MapApiHitEndpoints();

app.Run();
