using Loups_Garoups_de_Thiercelieux_console.Classes;
using Loups_Garoups_de_Thiercelieux_console.Enums;

namespace Loups_Garoups_de_Thiercelieux_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int nbPlayer;
            bool simpleGame;

            ConsoleDisplay.MainTitle();

            // --- TEST ---
            nbPlayer = 5;
            simpleGame = true;
            Console.WriteLine($"Création d'une partie à {nbPlayer} joueurs...\n");

            Game NewGame = new Game(nbPlayer, simpleGame);
            NewGame.Run();
            
        }
    }
}
