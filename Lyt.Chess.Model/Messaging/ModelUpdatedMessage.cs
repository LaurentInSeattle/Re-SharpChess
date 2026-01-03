namespace Lyt.Chess.Model.Messaging;

public enum UpdateHint
{
    None = 0,
    EngineReady,
    NewGame,
    Move,
    LegalMoves,
    Capture,
    UpdateScores,
    IsChecked,
    IsCheckmate,
    IsStalemate,
    SuggestedMove,
    EngineError,
    UnexpectedError,
}

public sealed record class ModelUpdatedMessage(UpdateHint Hint, object? Parameter = null); 
