using Bomberman.Server.GameLogic;
using Bomberman.Server.Services;
using Bomberman.Server.WebSocketHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<LobbyService>();
builder.Services.AddSingleton<GameHandler>();
builder.Services.AddSingleton<LobbyHandler>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<ScoreboardService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddTransient<MainWebSocketHandler>();
builder.Services.AddControllers();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ScoreboardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Use CORS policy
app.UseCors("AllowAll");

app.Map("/ws", async context =>
{
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var handler = context.RequestServices.GetRequiredService<MainWebSocketHandler>();
    await handler.HandleAsync(webSocket);
});

// Serve the default page
app.MapFallbackToFile("index.html");

app.Run();