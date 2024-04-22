using System.Collections.Generic;
using static System.Math;
using ChessEngine.API;
using System;

namespace ChessEngine
{
    public class UCIMatchController
    {
        Chess.Board RawBoard;
        Board board;
        public MyBot bot;
        Board gameOverMeasureBoard;
        

        public UCIMatchController() 
        {
            RawBoard = new();
            RawBoard.StartPositionInfo = Chess.FenUtility.PositionFromFen(ChessEngine.Chess.FenUtility.StartPositionFEN);
            RawBoard.Initialize();
            RawBoard.gameStateHistory = new Stack<Chess.GameState>(capacity: 64);
            RawBoard.AllGameMoves = new List<Chess.Move>();
            board = new Board(RawBoard);
            gameOverMeasureBoard = new Board(RawBoard);
            
            bot = new();
            bot.OnMoveChosen += OnMoveChosen;
            
        }
        public void SetupPosition(string fen)
        {
            RawBoard.LoadPosition(fen); 
            board = new Board(RawBoard);
            gameOverMeasureBoard = new Board(RawBoard);

        }
        public void UpdatePosition(string move)
        {
            if (!isGameOver())
            {
                Chess.Move moveToPlay = new(new Move(move, board).RawValue);
                RawBoard.MakeMove(moveToPlay, false);
                gameOverMeasureBoard.MakeMove(new Move(move, gameOverMeasureBoard));
                board = new Board(RawBoard);
            }
            CheckGameOver();
        }

        public void StartBotThinking(Timer timer)
        {
            bot.Think(board, timer);
        }
        

        public int ChooseThinkTime(int timeRemainingWhiteMs, int timeRemainingBlackMs, int incrementWhiteMs, int incrementBlackMs)
        {
            int myTimeRemainingMs = board.IsWhiteToMove ? timeRemainingWhiteMs : timeRemainingBlackMs;
            int myIncrementMs = board.IsWhiteToMove ? incrementWhiteMs : incrementBlackMs;
            // Get a fraction of remaining time to use for current move
            double thinkTimeMs = myTimeRemainingMs / 40.0;
            // Clamp think time if a maximum limit is imposed
            // Add increment
            if (myTimeRemainingMs > myIncrementMs * 2)
            {
                thinkTimeMs += myIncrementMs * 0.8;
            }

            double minThinkTime = Min(50, myTimeRemainingMs * 0.25);
            return (int)Ceiling(Max(minThinkTime, thinkTimeMs));
        }

        void OnMoveChosen(string move)
        {
            gameOverMeasureBoard.MakeMove(new Move(move, gameOverMeasureBoard));
            CheckGameOver();
        }

        void CheckGameOver()
        {
            if (board.IsInCheckmate())
            {
                Console.WriteLine("Game Over: Checkmate");
            }
            else if (board.IsDraw())
            {
                Console.WriteLine("Game Over: Draw");
            }
        }
        bool isGameOver()
        {
            if (board.IsInCheckmate())
            {
                return true;
            }
            else if (board.IsDraw())
            {
                return true;
            }
            return false;
        }

    }
}
