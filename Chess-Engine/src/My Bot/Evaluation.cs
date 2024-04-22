using ChessEngine.API;
using Raylib_cs;
using System;

namespace ChessEngine.src.My_Bot
{
    class Evaluation
    {
        static readonly int[] pieceValues = { 0, 100, 310, 330, 500, 900, 10000 };

        public static bool useMopUpScores = false;

        static readonly int[,] whitePawnActivityMap = { {0,  0,  0,  0,  0,  0,  0,  0 },
                                               { 50, 50, 50, 50, 50, 50, 50, 50 },
                                               { 10, 10, 20, 30, 30, 20, 10, 10 },
                                               { 5,  5, 10, 25, 25, 10,  5,  5 },
                                               { 0,  0,  0, 20, 20,  0,  0,  0 },
                                               { 5, -5,-10,  0,  0,-10, -5,  5 },
                                               { 5, 10, 10,-20,-20, 10, 10,  5 },
                                               { 0,  0,  0,  0,  0,  0,  0,  0 } };


        static readonly int[,] blackPawnActivityMap = { { 0,  0,  0,  0,  0,  0,  0,  0 },
                                               { 5, 10, 10,-20,-20, 10, 10,  5 },
                                               { 5, -5,-10,  0,  0,-10, -5,  5 },
                                               { 0,  0,  0, 20, 20,  0,  0,  0 },
                                               { 5,  5, 10, 25, 25, 10,  5,  5 },
                                               { 10, 10, 20, 30, 30, 20, 10, 10 },
                                               { 50, 50, 50, 50, 50, 50, 50, 50 },
                                               { 0,  0,  0,  0,  0,  0,  0,  0 } };


        static readonly int[,] knightActivityMap = { {-50,-40,-30,-30,-30,-30,-40, -50 },
                                            { -40,-20,  0,  0,  0,  0,-20,-40 },
                                            { -30,  0, 10, 15, 15, 10,  0,-30 },
                                            { -30,  5, 15, 20, 20, 15,  5,-30 },
                                            { -30,  0, 15, 20, 20, 15,  0,-30 },
                                            { -30,  5, 10, 15, 15, 10,  5,-30 },
                                            { -40,-20,  0,  5,  5,  0,-20,-40 },
                                            { -50,-40,-30,-30,-30,-30,-40,-50,} };

        static readonly int[,] whiteBishopsActivityMap = { {-40,-20,-20,-20,-20,-20,-20,-40 },
                                                  { -20,  0,  0,  0,  0,  0,  0,-20 },
                                                  {-20,  0,  10, 20, 20,  10,  0,-20 },
                                                  { -20,  10,  10, 20, 20,  10,  10,-20 },
                                                  { -20,  0, 20, 20, 20, 20,  0,-20 },
                                                  { -20, 20, 20, 20, 20, 20, 20,-20 },
                                                  { -20,  10,  0,  0,  0,  0,  10,-20 },
                                                  { -40,-20,-20,-20,-20,-20,-20,-40, } };



        static readonly int[,] blackBishopsActivityMap = { { -40, -20, -20, -10, -10, -20, -20, -40 },
                                                  { -20,  0,   0,  0,  0,   0,   0, -20 },
                                                  { -20,  0,   10,  10,  10,   10,   0, -20 },
                                                  {  -10,  0,   10,  10,  10,   10,   0,  -10 },
                                                  {   0,  0,   10,  10,  10,   10,   0,  -10 },
                                                  { -20,  10,   10,  10,  10,   10,   0, -20 },
                                                  { -20,  0,  10,  0,  0,   0,   0, -20 },
                                                  { -40, -20, -20, -10, -10, -20, -20, -40 } };

        static readonly int[,] whiteRooksActivityMap = { { 0,  0,  0,  0,  0,  0,  0,  0 },
                                                { 50, 100, 100, 100, 100, 100, 100,  50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { 0,  0,  0,  50,  50,  0,  0,  0 } };

        static readonly int[,] blackRooksActivityMap = { { 0,  0,  0,  50,  50,  0,  0,  0 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { -50,  0,  0,  0,  0,  0,  0, -50 },
                                                { 50, 100, 100, 100, 100, 100, 100,  50 },
                                                { 0,  0,  0,  0,  0,  0,  0,  0 } };

        static readonly int[,] queenActivityMap = {  { -140, -70, -70, -35, -35, -70, -70, -140 },
                                        { -70,   0,   0,  0,  0,   0,   0, -70 },
                                        { -70,   0,   35,  35,  35,   35,   0, -70 },
                                        {  -35,   0,   35,  35,  35,   35,   0,  -35 },
                                        {   0,   0,   35,  35,  35,   35,   0,  -35 },
                                        { -70,   35,   35,  35,  35,   35,   0, -70 },
                                        { -70,   0,   35,  0,  0,   0,   0, -70 },
                                        { -140, -70, -70, -35, -35, -70, -70, -140 }  };

        static readonly int[,] whitekingActivityMap = {  {-150,-200,-200,-250,-250,-200,-200,-150},
                                            {-150,-200, -200, -250,-250, -200, -200,-150},
                                            {-150, -200, -200, -250,-250, -200, -200,-150},
                                            {-150, -200, -200, -250,-250, -200, -200,-150},
                                            {-100,-150,-150,-200,-200,-150,-150,-100},
                                            {-50,-100,100,-100,-100,-100,-100,-50},
                                            { 100, 100,  0,  0,  0,  0, 100, 100},
                                            { 100, 150, 50,  0,  0, 50, 150, 100} };

        static readonly int[,] blackKingActivityMap = { { 100, 150, 50, 0, 0, 50, 150, 100 },
                                               { 100, 100,  0,  0,  0,  0, 100, 100},
                                               {-50,-100,-100,-100,-100,-100,-100,-50},
                                               {-100,-150,-150,-200,-200,-150,-150,-100},
                                               {-150, -200, -200, -250,-250, -200, -200,-150},
                                               {-150, -200, -200, -250,-250, -200, -200,-150},
                                               {-150, -200, -200, -250,-250, -200, -200,-150},
                                               {-150, -200, -200, -250,-250, -200, -200,-3150} };

        public static int Evaluate(Board board)
        {
            // Adjust evaluation based on whose turn it is
            int perspective = (board.IsWhiteToMove) ? 1 : -1;


            if (board.IsInCheckmate())
            {
                return int.MaxValue; // Return checkmate score
            }
            // Check for winning endgames
            if (IsInWinningEndgame(board, board.IsWhiteToMove))
            {
                // Calculate mop-up evaluation
                int mopUpEval = MopUpEvaluation(board, board.IsWhiteToMove);
                useMopUpScores = true;
                return mopUpEval * perspective; // Return mop-up evaluation

            }
            if (board.IsInStalemate() || board.IsInsufficientMaterial())
            {
                return 0; // Return draw score
            }

            // If not in a winning endgame, use existing evaluation metrics
            int whiteEval = CountMaterial(board, true);
            int blackEval = CountMaterial(board, false);

            int whitePositionalIntegrity = PositionalIntegrity(board, true);
            int blackPositionalIntegrity = PositionalIntegrity(board, false);

            int evaluation = whiteEval - blackEval;

            bool castlingRight = board.HasKingsideCastleRight(board.IsWhiteToMove) && board.HasQueensideCastleRight(board.IsWhiteToMove);
            bool kingNotOnCastlingSquare = (board.IsWhiteToMove) ? board.GetKingSquare(board.IsWhiteToMove).Index == 2 || board.GetKingSquare(board.IsWhiteToMove).Index == 6 : board.GetKingSquare(board.IsWhiteToMove).Index == 62 || board.GetKingSquare(board.IsWhiteToMove).Index == 58;
            if (board.GameMoveHistory.Length < 15 && !castlingRight && kingNotOnCastlingSquare)
            {
                evaluation -= 150;
            }

            int positionalEvaluation = whitePositionalIntegrity - blackPositionalIntegrity;

            return (evaluation + positionalEvaluation) * perspective;
        }


        static int CountMaterial(Board board, bool forWhite)
        {
            int material = 0;
            PieceList pawns = board.GetPieceList(PieceType.Pawn, forWhite);
            PieceList knights = board.GetPieceList(PieceType.Knight, forWhite);
            PieceList bishops = board.GetPieceList(PieceType.Bishop, forWhite);
            PieceList rooks = board.GetPieceList(PieceType.Rook, forWhite);
            PieceList queens = board.GetPieceList(PieceType.Queen, forWhite);


            material += pawns.Count * pieceValues[1];
            material += knights.Count * pieceValues[2];
            material += bishops.Count * pieceValues[3];
            material += rooks.Count * pieceValues[4];
            material += queens.Count * pieceValues[5];

            return material;
        }

        static int PositionalIntegrity(Board board, bool forWhite)
        {
            int positionalIntegrity = 0;

            PieceList pawns = board.GetPieceList(PieceType.Pawn, forWhite);
            PieceList knights = board.GetPieceList(PieceType.Knight, forWhite);
            PieceList bishops = board.GetPieceList(PieceType.Bishop, forWhite);
            PieceList rooks = board.GetPieceList(PieceType.Rook, forWhite);
            PieceList queens = board.GetPieceList(PieceType.Queen, forWhite);
            if (forWhite)
            {
                foreach (Piece piece in pawns)
                {
                    positionalIntegrity += whitePawnActivityMap[7 - piece.Square.Rank, piece.Square.File];
                }
                foreach (Piece piece in bishops)
                {
                    positionalIntegrity += whiteBishopsActivityMap[7 - piece.Square.Rank, piece.Square.File];
                }
                foreach (Piece piece in rooks)
                {
                    positionalIntegrity += whiteRooksActivityMap[7 - piece.Square.Rank, piece.Square.File];
                }
                positionalIntegrity += whitekingActivityMap[7 - board.GetKingSquare(forWhite).Rank, board.GetKingSquare(forWhite).File];

            }
            else
            {
                foreach (Piece piece in pawns)
                {
                    positionalIntegrity += blackPawnActivityMap[7 - piece.Square.Rank, piece.Square.File];
                }
                foreach (Piece piece in bishops)
                {
                    positionalIntegrity += blackBishopsActivityMap[7 - piece.Square.Rank, piece.Square.File];
                }
                foreach (Piece piece in rooks)
                {
                    positionalIntegrity += blackRooksActivityMap[7 - piece.Square.Rank, piece.Square.File];
                }
                positionalIntegrity += blackKingActivityMap[7 - board.GetKingSquare(forWhite).Rank, board.GetKingSquare(forWhite).File];
            }

            foreach (Piece piece in knights)
            {
                positionalIntegrity += knightActivityMap[7 - piece.Square.Rank, piece.Square.File];
            }
            foreach (Piece piece in queens)
            {
                positionalIntegrity += queenActivityMap[7 - piece.Square.Rank, piece.Square.File];
            }


            return positionalIntegrity;
        }
        static int MopUpEvaluation(Board board, bool forWhite)
        {
            int mopUpEval = 0;

            Square ownKingSquare = board.GetKingSquare(forWhite);
            Square oppKingSquare = board.GetKingSquare(!forWhite);

            // Calculate Manhattan distance of the losing king from the center
            int oppKingCenterManhattanDistance = Math.Abs(oppKingSquare.Rank - 3) + Math.Abs(oppKingSquare.File - 3);
            mopUpEval += (4 - oppKingCenterManhattanDistance) * 10; // Bonus for being closer to the center

            // Calculate minimum distance between both kings using a combination of Chebyshev and Manhattan distance
            int kingDistance = Math.Max(Math.Abs(ownKingSquare.Rank - oppKingSquare.Rank), Math.Abs(ownKingSquare.File - oppKingSquare.File));
            mopUpEval += (8 - kingDistance) * 10; // Bonus for closer proximity of kings

            return mopUpEval;
        }
        static bool IsInWinningEndgame(Board board, bool forWhite)
        {
            // checking if the position is eligible for MopUp scores
            if (board.GetPieceList(PieceType.Pawn,!forWhite).Count == 0)
            {
                // see if the oponent has barely any pieces left
                if (CountMaterial(board, !forWhite) <= 100)
                {
                    return true;
                }
            }
            return false;
        }


    }

}




