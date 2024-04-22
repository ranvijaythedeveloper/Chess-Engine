using ChessEngine.API;
using ChessEngine.src.My_Bot;
using System;
using ChessEngine.Application;

public class MyBot : IChessBot
{
    static readonly int[] pieceValues = { 0, 100, 310, 330, 500, 900, 10000 };
    public Action<string>? OnMoveChosen;

    private static TranspositionTable transpositionTable;
    static Board currentBoard;
    const int transpositionTableSizeMB = 64;

    public MyBot()
    {
        transpositionTable = new TranspositionTable(transpositionTableSizeMB);
        //BitboardHelper.VisualizeBitboard(0x0101010101010101);
        
    }

    public Move Think(Board board, Timer timer)
    {
        currentBoard = board;
        Move[] allMoves = board.GetLegalMoves();
        int depth = (Evaluation.useMopUpScores) ? 5 : 3; // Adjust the depth of the search according to your needs



        // Sort the moves based on heuristics
        OrderMoves(allMoves);

        Move moveToPlay = allMoves[0];
        int bestValue = int.MinValue;

        foreach (Move move in allMoves)
        {
            currentBoard.MakeMove(move); // Make the move on the board
            int value = Minimax(currentBoard, depth, int.MinValue, int.MaxValue, false); // Call the minimax function
            currentBoard.UndoMove(move); // Undo the move

            if (value > bestValue)
            {
                bestValue = value;
                moveToPlay = move;
            }
        }
        OnMoveChosen?.Invoke(moveToPlay.ToString().Substring(6));
        return moveToPlay;
    }


    private int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // Check if we should perform quiescence search
        if (depth <= 0 || board.IsInCheckmate() || board.IsInStalemate() || board.IsInsufficientMaterial())
        {
            // Perform quiescence search
            return QuiescenceSearch(board, alpha, beta, maximizingPlayer);
        }

        ulong hash = board.ZobristKey;

        // Check if the current position is already stored in the transposition table
        if (Settings.useTranspositionTable)
        {
            TranspositionEntry entry = transpositionTable.Retrieve(hash);
            if (entry != null && entry.Depth >= depth)
            {
                if (entry.Flag == TranspositionFlag.Exact)
                {
                    return entry.Score;
                }
                else if (entry.Flag == TranspositionFlag.LowerBound)
                {
                    alpha = Math.Max(alpha, entry.Score);
                }
                else if (entry.Flag == TranspositionFlag.UpperBound)
                {
                    beta = Math.Min(beta, entry.Score);
                }
                if (alpha >= beta)
                {
                    return entry.Score;
                }
            }
        }
        


        Move[] allMoves = board.GetLegalMoves();
        OrderMoves(allMoves);

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Move move in allMoves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, false);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    transpositionTable.Store(hash, depth, maxEval, TranspositionFlag.LowerBound);
                    return maxEval;
                }
            }
            transpositionTable.Store(hash, depth, maxEval, (maxEval >= beta) ? TranspositionFlag.LowerBound : TranspositionFlag.Exact);
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Move move in allMoves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, true);
                board.UndoMove(move);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    transpositionTable.Store(hash, depth, minEval, TranspositionFlag.UpperBound);
                    return minEval;
                }
            }
            transpositionTable.Store(hash, depth, minEval, (minEval <= alpha) ? TranspositionFlag.UpperBound : TranspositionFlag.Exact);
            return minEval;
        }
    }

    private static int QuiescenceSearch(Board board, int alpha, int beta, bool maximizingPlayer, int iterationsDone = 0)
    {
        int standPat = Evaluation.Evaluate(board);
        if (iterationsDone > 6)
        {
            if (maximizingPlayer && standPat >= beta)
            {
                return standPat;
            }
            if (!maximizingPlayer && standPat <= alpha)
            {
                return standPat;
            }
            return alpha;
        }

        // If the stand-pat evaluation is already higher than beta or lower than alpha, return it
        if (maximizingPlayer && standPat >= beta)
        {
            return standPat;
        }
        if (!maximizingPlayer && standPat <= alpha)
        {
            return standPat;
        }

        if (maximizingPlayer)
        {
            int maxEval = standPat;
            Move[] captureMoves = board.GetLegalMoves(true);
            foreach (Move move in captureMoves)
            {
                board.MakeMove(move);
                int eval = QuiescenceSearch(board, alpha, beta, false, iterationsDone + 1);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    return beta;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = standPat;
            Move[] captureMoves = board.GetLegalMoves(true);
            foreach (Move move in captureMoves)
            {
                board.MakeMove(move);
                int eval = QuiescenceSearch(board, alpha, beta, true, iterationsDone + 1);
                board.UndoMove(move);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    return alpha;
                }
            }
            return minEval;
        }
    }
    static void OrderMoves(Move[] moves)
    {
        Array.Sort(moves, CompareMovesByScore);
    }

    static int GetMoveScore(Move move)
    {
        int moveScoreGuess = 0;
        Piece movePiece = currentBoard.GetPiece(move.StartSquare);
        Piece capturePiece = currentBoard.GetPiece(move.TargetSquare);
        if (capturePiece.PieceType != PieceType.None)
        {
            moveScoreGuess += GetPieceValue(capturePiece.PieceType) - GetPieceValue(movePiece.PieceType);
        }
        if (move.IsPromotion)
        {
            moveScoreGuess += GetPieceValue(move.PromotionPieceType);
        }
        

        return moveScoreGuess;
    }
    static int CompareMovesByScore(Move a, Move b)
    {
        return GetMoveScore(a).CompareTo(GetMoveScore(b));
    }
    static int GetPieceValue(PieceType piece)
    {
        switch (piece)
        {
            case PieceType.Pawn:
                return pieceValues[0];
            case PieceType.Knight:
                return pieceValues[1];
            case PieceType.Bishop:
                return pieceValues[2];
            case PieceType.Rook:
                return pieceValues[3];
            case PieceType.Queen:
                return pieceValues[4];
            case PieceType.King:
                return pieceValues[5];
            default:
                break;
        }
        return 0;

    }

}
