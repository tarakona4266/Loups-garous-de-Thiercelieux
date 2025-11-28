using Loups_Garous_de_Thiercelieux_console.Classes;
using Loups_Garous_de_Thiercelieux_console.Enums;
using System;

namespace Loups_Garous_de_Thiercelieux_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int nbPlayer;
            bool simpleGame;
            int maxPlayerNb = 20;

            Console.ForegroundColor = ConsoleColor.White;
            ConsoleDisplay.MainTitle();
            ConsoleDisplay.Next();

            // --- Game setup ---
            Console.WriteLine($"Choose player number (8 to {maxPlayerNb}) :");
            while (true)
            {
                string? answer = Console.ReadLine();
                if (int.TryParse(answer, out int nb))
                {
                    if (nb > 7 && nb <= maxPlayerNb)
                    {
                        nbPlayer = nb;
                        ConsoleDisplay.ClearLine(2);
                        break;
                    }
                    else
                    {
                        ConsoleDisplay.ClearLine(2);
                        Console.WriteLine($"Invalid input : player number must be in between 8 and {maxPlayerNb}.");
                    }
                }
                else
                {
                    ConsoleDisplay.ClearLine(2);
                    Console.WriteLine("Invalid input : please enter a number.");
                }
            }

            // --- TEST ---
            simpleGame = true;
            Console.WriteLine($"Creating a new game with {nbPlayer} players...\n");

            Game NewGame = new Game(nbPlayer, simpleGame);
            NewGame.Run();
            
        }
    }
}
