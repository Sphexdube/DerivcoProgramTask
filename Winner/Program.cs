using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Winner
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputTextFile, outputTextFile = string.Empty;

            try
            {
                (inputTextFile, outputTextFile) = GetTextFiles(args);

                var retrivedCardData = DataFileParser.ReadInputFile(inputTextFile);

                var players = retrivedCardData.Select(cardData => { return new Player(cardData); });

                var results = Winner.AddEmUp(players.ToList());

                DataFileParser.WriteOutputFile(outputTextFile, results);

            }
            catch (Exception ex)
            {
                File.WriteAllText(outputTextFile, $"Error occured: {ex.Message}");
            }
        }

        private static (string inputTextFile, string outputTextFile) GetTextFiles(string[] commandParacters)
        {
            int inputIndex = Array.IndexOf(commandParacters, "--in");
            int outputIndex = Array.IndexOf(commandParacters, "--out");

            if (inputIndex > -1 && outputIndex > -1)
            {
                return (commandParacters[inputIndex + 1], commandParacters[outputIndex + 1]);
            }
            throw new Exception("Invalid command parameters.");
        }
    }

    static class DataFileParser
    {
        internal static IEnumerable<string> ReadInputFile(string path)
        {
            if (File.Exists(path))
            {
                var result = File.ReadLines(path);
                return result;
            }
            throw new Exception("Data file not found.");
        }

        internal static void WriteOutputFile(string path, string results)
        {
            if (File.Exists(path))
            {
                File.WriteAllText(path, results);
                return;
            }
            throw new Exception("Data file not found.");
        }
    }

    class Score
    {
        public int FaceTotal { get; }
        public int SuitTotal { get; }
        public Score(int face, int suit)
        {
            FaceTotal = face;
            SuitTotal = suit;
        }
    }

    class Player
    {
        public string Name { get; }
        public Score Scores { get; }

        public Player(string cardData)
        {
            var playerDetails = cardData.Split(":");

            Name = playerDetails.FirstOrDefault();
            Scores = GetFaceSuitTotal(playerDetails.LastOrDefault());
        }

        private Score GetFaceSuitTotal(string data)
        {
            int faceTotal = 0;
            int suitTotal = 0;

            var cards = data.Split(',');

            foreach (var card in cards)
            {
                faceTotal += GetFaceTotalSum(card);
                suitTotal += GetSuitTotalSum(card);
            }

            return new Score(faceTotal, suitTotal);
        }

        private int GetFaceTotalSum(string card)
        {
            switch (card[0])
            {
                case 'A':
                    return 1;
                case 'J':
                    return 11;
                case 'Q':
                    return 12;
                case 'K':
                    return 13;
                default:
                    {
                        int cardNumber = int.Parse(card.Substring(0, card.Length - 1));
                        if (cardNumber >= 2 && cardNumber <= 10)
                        {
                            return cardNumber;
                        }
                        throw new NotSupportedException("Card - '" + cardNumber + "' not supported.");
                    }
            }
        }

        private int GetSuitTotalSum(string card)
        {
            switch (card[card.Length - 1])
            {
                case 'C':
                    return 1;
                case 'D':
                    return 2;
                case 'H':
                    return 3;
                case 'S':
                    return 4;
                default:
                    throw new NotSupportedException("Suit - '" + card[1] + "' not supported.");
            }
        }
    }

    class Winner
    {
        internal static string AddEmUp(List<Player> players)
        {
            var winnerPlayer = players.First();
            var tiePlayers = new List<string>();

            for (int i = 1; i < players.Count(); i++)
            {
                if (players.ElementAt(i).Scores.FaceTotal > winnerPlayer.Scores.FaceTotal)
                {
                    winnerPlayer = players.ElementAt(i);
                    tiePlayers.Clear();
                }
                else if (winnerPlayer.Scores.FaceTotal == players.ElementAt(i).Scores.FaceTotal)
                {
                    if (players.ElementAt(i).Scores.SuitTotal > winnerPlayer.Scores.SuitTotal)
                    {
                        winnerPlayer = players.ElementAt(i);
                        tiePlayers.Clear();
                    }
                    else if (players.ElementAt(i).Scores.SuitTotal == winnerPlayer.Scores.SuitTotal)
                    {
                        tiePlayers.Add(players.ElementAt(i).Name);
                    }
                }
            }

            return GetOutputData(winnerPlayer, tiePlayers);
        }

        private static string GetOutputData(Player winnerPlayer, List<string> tiePlayers)
        {
            var result = new StringBuilder(winnerPlayer.Name);
            if (tiePlayers.Count > 0)
            {
                result.Append("," + string.Join(",", tiePlayers));
            }
            result.Append(":" + winnerPlayer.Scores.FaceTotal);
            return result.ToString();
        }
    }
}
