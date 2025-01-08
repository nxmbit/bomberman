using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Bomberman.Server.GameLogic;
using Bomberman.Server.WebSocketHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<LobbyService>();
builder.Services.AddTransient<MainWebSocketHandler>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

app.Map("/ws", async context =>
{
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var handler = context.RequestServices.GetRequiredService<MainWebSocketHandler>();
    await handler.HandleAsync(webSocket);
    
});

app.Run();
