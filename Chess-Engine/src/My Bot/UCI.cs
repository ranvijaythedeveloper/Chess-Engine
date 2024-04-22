using ChessEngine.Chess;
using System;
using ChessEngine;

namespace ChessEngine.src.My_Bot
{
    class UCI
    {
        static readonly string[] positionLabels = new[] { "position", "fen", "moves" };
        static readonly string[] goLabels = new[] { "go", "movetime", "wtime", "btime", "winc", "binc", "movestogo" };

        static UCIMatchController controller;
        public static void Init_UCI()
        {
            controller = new UCIMatchController();
            controller.bot.OnMoveChosen += OnMoveChosen;
        }
        public static void ReceiveCommand(string command)
        {
            command = command.Trim();
            string commandType = command.Split(' ')[0].ToLower();
            switch (commandType)
            {
                case "uci":
                    Respond("uciok");
                    break;
                case "isready":
                    Respond("readyok");
                    break;
                case "ucinewgame":
                    Init_UCI();
                    break;
                case "position":
                    ProcessPositionCommand(command);
                    break;
                case "go":
                    ProcessGoCommand(command);
                    break;
            }
        }

        public static void Respond(string message)
        {
            Console.WriteLine(message);
        }

        static void ProcessGoCommand(string message)
        {
            if (message.Contains("movetime"))
            {
                int moveTimeMs = TryGetLabelledValueInt(message, "movetime", goLabels, 0);
                
                controller.StartBotThinking(new ChessEngine.API.Timer(moveTimeMs));
            }
            else
            {
                int timeRemainingWhiteMs = TryGetLabelledValueInt(message, "wtime", goLabels, 0);
                int timeRemainingBlackMs = TryGetLabelledValueInt(message, "btime", goLabels, 0);
                int incrementWhiteMs = TryGetLabelledValueInt(message, "winc", goLabels, 0);
                int incrementBlackMs = TryGetLabelledValueInt(message, "binc", goLabels, 0);

                int thinkTime = controller.ChooseThinkTime(timeRemainingWhiteMs, timeRemainingBlackMs, incrementWhiteMs, incrementBlackMs);
                controller.StartBotThinking(new ChessEngine.API.Timer(thinkTime));
            }

        }

        static void OnMoveChosen(string move)
        {
            Respond("bestmove " + move);
        }

        public static void ProcessPositionCommand(string message)
        {
            if (message.ToLower().Contains("startpos"))
            {
                controller.SetupPosition(FenUtility.StartPositionFEN);
            }
            else if (message.ToLower().Contains("fen"))
            {
                string customFen = TryGetLabelledValue(message, "fen", positionLabels);
                controller.SetupPosition(customFen);
            }
            else
            {
                Console.WriteLine("Invalid position command (expected 'startpos' or 'fen')");
            }

            // Moves
            string allMoves = TryGetLabelledValue(message, "moves", positionLabels);
            if (!string.IsNullOrEmpty(allMoves))
                //Console.WriteLine(allMoves);
            {
                string[] moveList = allMoves.Split(' ');
                foreach (string move in moveList)
                {
                    controller.UpdatePosition(move);
                }
            }
        }

        static int TryGetLabelledValueInt(string text, string label, string[] allLabels, int defaultValue = 0)
        {
            string valueString = TryGetLabelledValue(text, label, allLabels, defaultValue + "");
            if (int.TryParse(valueString.Split(' ')[0], out int result))
            {
                return result;
            }
            return defaultValue;
        }

        static string TryGetLabelledValue(string text, string label, string[] allLabels, string defaultValue = "")
        {
            text = text.Trim();
            if (text.Contains(label))
            {
                int valueStart = text.IndexOf(label) + label.Length;
                int valueEnd = text.Length;
                foreach (string otherID in allLabels)
                {
                    if (otherID != label && text.Contains(otherID))
                    {
                        int otherIDStartIndex = text.IndexOf(otherID);
                        if (otherIDStartIndex > valueStart && otherIDStartIndex < valueEnd)
                        {
                            valueEnd = otherIDStartIndex;
                        }
                    }
                }

                return text.Substring(valueStart, valueEnd - valueStart).Trim();
            }
            return defaultValue;
        }
    }
}
