namespace BattleshipEngine;

public record AttackResult(Coordinate AttackCoordinate, AttackResultType HitOrMiss, ShipType? ShipType = null, Player? TargetedPlayer = null);
