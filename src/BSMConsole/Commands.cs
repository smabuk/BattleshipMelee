namespace BSMConsole;

[Description("The game of Battleship")]
internal sealed class BattleshipCommand : Command<BattleshipCommand.Settings> {

	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		BattleshipGame battleshipGame = new() {
			GameType            = settings.GameType,
			PlayerName          = settings.PlayerName,
			RandomShipPlacement = settings.RandomShipPlacement,
		};

		battleshipGame.Play();
		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Battleship game type - classic, melee, or bigbang")]
		[CommandArgument(0, "[TYPE]")]
		public string Type { get; init; } = "classic";

		public BattleshipEngine.GameType GameType => Type.ToLower() switch
		{
			"classic" => BattleshipEngine.GameType.Classic,
			"melee"   => BattleshipEngine.GameType.Melee,
			"bigbang" => BattleshipEngine.GameType.BigBangTheory,
			_ => throw new NotImplementedException(),
		};

		[Description("Place the ships randomly")]
		[CommandOption("-r|--random")]
		[DefaultValue(false)]
		public bool RandomShipPlacement { get; init; }

		[Description("Name of the player")]
		[CommandOption("-u|--user|--username|--player|--playername")]
		[DefaultValue("Human")]
		public string PlayerName { get; init; } = "Human";


		public override ValidationResult Validate()
		{
			string[] validTypes = {
				"classic",
				"bigbang",
				"melee",
			};

			if (!validTypes.Contains(Type.ToLower())) {
				return ValidationResult.Error("Type must be one of classic, melee, or bigbang");
			}

			return base.Validate();
		}
	}
}
