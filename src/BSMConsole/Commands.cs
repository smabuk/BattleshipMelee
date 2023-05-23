namespace BSMConsole;

[Description("The game of Battleship")]
internal sealed class BattleshipCommand : Command<BattleshipCommand.Settings> {

	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		BattleshipGame battleshipGame = new() {
			GameType            = settings.GameType,
			PlayerName          = settings.PlayerName,
			RandomShipPlacement = settings.RandomShipPlacement,
			NetworkPlay         = settings.NetworkPlay,
			Uri                 = $"https://{settings.Host}:{settings.Port}",
			Theme               = settings.Theme,
		};

		if (settings.NetworkPlay) {
			battleshipGame.PlayNetworkGame().Wait();
		} else {
			battleshipGame.Play();
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
			"melee"   => throw new NotImplementedException("Melee is not implemented yet"),
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


		[Description("Network")]
		[CommandOption("-n|--network")]
		[DefaultValue(false)]
		public bool NetworkPlay { get; init; } = false;

		[Description("Network host")]
		[CommandOption("--host")]
		public string? Host { get; init; }

		[Description("Network port")]
		[CommandOption("--port")]
		public int? Port { get; init; }

		[Description("Battleship game type - classic, melee, or bigbang")]
		[CommandOption("-t|--theme")]
		public string? ThemeName { get; init; } = "default";

		public ITheme Theme => ThemeName?.ToLower() switch
		{
			"default" => new DefaultTheme(),
			"emoji" => new EmojiTheme(),
			"animals" => new AnimalsTheme(),
			_ => throw new NotImplementedException(),
		};

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

			if (NetworkPlay && (Host is null || Port is null)) {
				return ValidationResult.Error("When connecting over the network you must specify the host and the port");
			}

			return base.Validate();
		}
	}
}
