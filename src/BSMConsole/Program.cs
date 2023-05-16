//BattleShipsEngine.Execute();

CommandApp app = new();

app.Configure(config => {

	config.AddCommand<BattleshipCommand>("battleship")
		.WithExample(new[] { "battleship", })
		.WithExample(new[] { "battleship", "classic" })
		.WithExample(new[] { "battleship", "melee" })
		.WithExample(new[] { "battleship", "bigbang" });
});

return app.Run(args);
