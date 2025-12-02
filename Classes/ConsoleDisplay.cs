using Loups_Garoups_de_Thiercelieux_console.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Loups_Garous_de_Thiercelieux_console.Classes
{
    public static class ConsoleDisplay // this class exist so it will be easier to switch to a graphical interface later
    {
        public static bool printDebug = false;

        public static void MainTitle()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" +----------------------------------+");
            Console.Write(" | ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("The ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Werewolves");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" of ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Millers Hollow");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" |\n");
            Console.WriteLine(" +----------------------------------+\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void ClearLine(int nbLines = 1)
        {
            int lineNb = Console.CursorTop;
            if (lineNb >= nbLines)
            {
                lineNb -= nbLines;
            }
            Console.SetCursorPosition(0, lineNb);
            for (int i = 0; i <= nbLines; i++)
            {
                Console.Write(new string(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, lineNb + i);
            }
            Console.SetCursorPosition(0, lineNb);

        }

        public static void PrintPlayers(List<Player> Players, bool debug = false)
        {
            int nbColumn = Players.Count;
            int columnWidth = 12;
            int wordWidht;
            int comp;
            
            foreach (Player player in Players)
            {
                if (player.isAlive)
                {
                    if (player.indexInPlayerList < 10) { Console.Write(" "); } // for alignment
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" [{player.indexInPlayerList}] ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" dead ");
                }
                Console.Write($"- {player.name}");
                if (!player.isAlive || debug || player.isDiscovered)
                {
                    Console.Write(" : ");
                    player.PrintRole();
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintPlayers(List<Player> allPlayers, List<int> IndexesToPrint)
        {
            int nbColumn = allPlayers.Count;
            int columnWidth = 12;
            int wordWidht;
            int comp;

            foreach (Player player in allPlayers)
            {
                if (IndexesToPrint.Contains(player.indexInPlayerList))
                {
                    if (player.indexInPlayerList < 10) { Console.Write(" "); } // for alignment
                    Console.Write($" [{player.indexInPlayerList}] ");
                    Console.Write($"- {player.name}");
                    if (player.isDiscovered)
                    {
                        Console.Write(" : ");
                        player.PrintRole();
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        public static void Narrrate(string text, bool newLine = true)
        {
            Thread.Sleep(1000); // ugly but will do the job for now
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[Narrator] ");
            Console.ForegroundColor = ConsoleColor.White;
            if (newLine)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.Write(text);
            }
        }

        public static void DebugPrint(string text, bool prefix = false)
        {
            if (printDebug)
            {
                if (!prefix)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[DEBUG] {text}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }

        public static void PrintVotes(List<VoteData> voteList, List<Player> allPlayers)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("\n ----- ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Result");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(" -----\n");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (VoteData vote in voteList)
            {
                Console.WriteLine("test");
            }
        }

        public static void Next()
        {
            Thread.Sleep(750);
            Console.Write("Press ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[ENTER]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" to continue.");
            Console.ReadLine();
            Console.Clear();
            MainTitle();
        }

        public static void PrintSeparation()
        {
            ClearLine();
            Console.ForegroundColor= ConsoleColor.Blue;
            Console.Write("\n ----- ");
            Console.ForegroundColor= ConsoleColor.White;
            Console.Write("Next vote");
            Console.ForegroundColor= ConsoleColor.Blue;
            Console.WriteLine(" -----\n");
            Console.ForegroundColor= ConsoleColor.White;
        }
    }
}
