namespace Lyt.Chess.Model.Messaging;

public enum UpdateHint
{
    None = 0,
    EngineReady,
    NewGame,
    EnginePlayed,
    LegalMoves,
    CapturedPiece,
}

public sealed record class ModelUpdatedMessage(UpdateHint Hint, object? Parameter = null)
{
}
