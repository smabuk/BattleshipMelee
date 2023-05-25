namespace BattleshipEngine;

public record AttackResult(Coordinate AttackCoordinate, AttackResultType AttackResultType, ShipType? ShipType = null, PlayerId? TargetedPlayerId = null);
