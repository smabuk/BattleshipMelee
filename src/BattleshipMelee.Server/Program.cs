var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHub<GameHub>("/bsm");

app.MapGet("/players", (GameService gameService) =>
{
	return gameService.Clients.Values.Select(p => Player.PublicPlayer(p)).ToList();
});

app.MapGet("/games", (GameService gameService) =>
{
	return gameService.Games.Values.ToList();
});

app.MapGet("/games/{gameId}", (GameId gameId, GameService gameService) =>
{
	return gameService.Games[gameId].LeaderBoard().ToList();
});

app.Run();
