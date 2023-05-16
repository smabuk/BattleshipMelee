namespace BSMConsole;

[Description("The game of Battleship")]
public sealed class BattleshipCommand : Command<BattleshipCommand.Settings> {

	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		BattleshipGame battleshipGame = new() {
			GameType = settings.GameType,
			PlayerName = settings.PlayerName,
			RandomShipPlacement = settings.RandomShipPlacement,
			Verbose = settings.Verbose
		};

		if (settings.Play) {
			battleshipGame.Play();
		} else {
			//battleshipGame.DisplayBattleship(settings.Verbose);
		}

		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Battleship game type - classic, melee, or bigbang")]
		[CommandArgument(0, "[TYPE]")]
		public string Type { get; init; } = "classic";

		public BattleshipEngine.GameType GameType => Type.ToLower() switch
		{
			"classic" => BattleshipEngine.GameType.Classic,
			"melee" => BattleshipEngine.GameType.Classic,
			"bigbang" => BattleshipEngine.GameType.BigBangTheory,
			_ => throw new NotImplementedException(),
		};

		[Description("Display more information")]
		[CommandOption("-v|--verbose")]
		[DefaultValue(false)]
		public bool Verbose { get; init; }

		[Description("Place the ships randomly")]
		[CommandOption("-r|--random")]
		[DefaultValue(false)]
		public bool RandomShipPlacement { get; init; }

		[Description("Play")]
		[CommandOption("-p|--play")]
		[DefaultValue(false)]
		public bool Play { get; init; }

		[Description("Name of the player")]
		[CommandOption("-u|--user|--username|--player|--playername")]
		public string PlayerName { get; init; } = "Human";


		public override ValidationResult Validate()
		{
			string[] validTypes = {
				"classic",
				"big",
				"deluxe",
				"superbig",
				"new",
				//"challenge",
			};

			if (!validTypes.Contains(Type.ToLower())) {
				return ValidationResult.Error("Type must be one of classic, melee, or bigbang");
			}

			return base.Validate();
		}
	}
}
