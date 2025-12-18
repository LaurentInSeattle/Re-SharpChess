namespace Lyt.Chess.Workflow.Play;

internal class Score
{
    // See: https://en.wikipedia.org/wiki/Chess_piece_relative_value 

    // Use Larry Kaufman's 2021 system ? 
    // Larry Kaufman in 2021 gives a more detailed system based on his experience working with chess engines,
    // depending on the presence or absence of queens.He uses "middlegame" to mean positions where both queens
    // are on the board, "threshold" for positions where there is an imbalance (one queen versus none, or two
    // queens versus one), and "endgame" for positions without queens. (Kaufman did not give the queen's value
    // in the middlegame or endgame cases, since in these cases both sides have the same number of queens and
    // their values cancel.)[42] 
}
