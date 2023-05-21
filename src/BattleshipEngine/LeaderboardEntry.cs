namespace BattleshipEngine;

public record LeaderboardEntry(string Name, int Position = 0, int Score = 0, bool IsComputer = false);
