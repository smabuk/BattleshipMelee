namespace BSMConsole;

internal class NetworkClient
{
	public required string Uri{ get; set; }

	[SetsRequiredMembers]
	public NetworkClient(string uri)
	{
		Uri = $"{uri}/bsm";
	}

	public async Task RegisterPlayer()
	{
		await using var connection = new HubConnectionBuilder().WithUrl(Uri).Build();

		await connection.StartAsync();

		Console.WriteLine("SignalR Stream:");

		try {
			await connection.InvokeAsync("RegisterPlayer", "SignalR player");
		}
		catch (Exception ex) {
			Console.WriteLine($"Error: {ex.Message}");
			throw;
		}

		Console.WriteLine($"");
	}
}
