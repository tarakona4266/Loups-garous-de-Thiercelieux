using Loups_Garous_de_Thiercelieux_console.Classes;
using Loups_Garous_de_Thiercelieux_console.Enums;

namespace Loups_Garous_de_Thiercelieux_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int nbPlayer;
            bool simpleGame;

            ConsoleDisplay.MainTitle();
            Console.WriteLine("Press [ENTER] to start.");
            Console.ReadLine();
            ConsoleDisplay.ClearLine(2);

            // --- Game setup ---
            Console.WriteLine("Choose player number (8 to 13) :");
            while (true)
            {
                string? answer = Console.ReadLine();
                if (int.TryParse(answer, out int nb))
                {
                    if (nb > 7 && nb < 14)
                    {
                        nbPlayer = nb;
                        ConsoleDisplay.ClearLine(2);
                        break;
                    }
                    else
                    {
                        ConsoleDisplay.ClearLine(2);
                        Console.WriteLine("Invalid input : player number must be in between 8 and 13.");
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
