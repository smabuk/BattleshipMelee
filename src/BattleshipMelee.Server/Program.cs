using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

app.MapGet("/players", Ok<List<Player>> (GameService gameService) =>
{
	return TypedResults.Ok(gameService.Clients.Values.Select(p => Player.PublicPlayer(p)).ToList());
});

app.MapGet("/games", Ok<List<Game>> (GameService gameService) =>
{
	return TypedResults.Ok(gameService.Games.Values.ToList());
});

app.MapGet("/leaderboard/{gameId}", Results<Ok<List<LeaderboardEntry>>, NotFound> ([FromRoute] GameId gameId, GameService gameService) =>
{
	if (gameService.Games.TryGetValue(gameId, out Game? game)) {
		return TypedResults.Ok(game.LeaderBoard().ToList());
	}
	return TypedResults.NotFound();
});

app.Run();
