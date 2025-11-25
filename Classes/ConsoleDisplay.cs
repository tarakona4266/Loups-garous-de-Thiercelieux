using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Loups_Garous_de_Thiercelieux_console.Classes
{
    public static class ConsoleDisplay
    {
        public static void MainTitle()
        {
            Console.WriteLine(" +----------------------------------+");
            Console.WriteLine(" | The Werewolves of Millers Hollow |");
            Console.WriteLine(" +----------------------------------+\n");
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
        public static void PrintPlayers (List<Player> Players)
        {
            int nbColumn = Players.Count;

            int columnWidth = 12;
            int wordWidht;
            int comp;

            string sep = "";
            for (int i = 0; i < nbColumn; i++)
            {
                sep += "+------------";
            }
            sep += "+";

            // --- NAME line ---
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(sep);
            Console.Write("|");
            foreach (Player player in Players)
            {
                wordWidht = player.name.Length;
                comp = columnWidth - wordWidht;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(player.name);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                for (int i = 0; i < comp; i++) { Console.Write(" "); }
                Console.Write("|");
            }
            Console.WriteLine();
            Console.WriteLine(sep);

            // --- INDEX line ---
            Console.Write("|");
            for (int i = 0;i < Players.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"ID : {i}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (i < 10) { comp = 6; }
                else { comp = 5; }
                for (int j = 0; j < comp; j++) { Console.Write(" "); }
                Console.Write("|");
            }
            Console.WriteLine();
            Console.WriteLine(sep);
        }
    }
}
