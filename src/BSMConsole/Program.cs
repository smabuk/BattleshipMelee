//BattleShipsEngine.Execute();

CommandApp<BattleshipCommand> app = new ();

app.Configure(config => {
	config.AddCommand<BattleshipCommand>("battleship")
		.WithExample(new[] { "battleship", })
		.WithExample(new[] { "battleship", "--random" })
		.WithExample(new[] { "battleship", "-u name" });
});

return await app.RunAsync(args);
