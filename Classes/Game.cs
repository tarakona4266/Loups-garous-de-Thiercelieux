using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Loups_Garous_de_Thiercelieux_console.Classes
{
    public class Game
    {
        private int nbPlayer;
        private bool simpleGame;
        private List<Player> allPlayers = [];

        // special players
        private List<Player> werewolves = [];
        private Player Thief;
        private Player Cupido;
        private Player FortuneTeller;
        private Player Witch;
        private Player LittleGirl;
        private Player Hunter;
        private Player Sheriff;


        public Game(int nbPlayer, bool simpleGame = false)
        {
            this.nbPlayer = nbPlayer;
            this.simpleGame = simpleGame;
        }

        public void Run()
        {
            // --- Players creation ---
            bool valid = false;
            Console.WriteLine("What is your name ?");
            string pattern = @"^(\w){3,12}$";
            while (!valid)
            {
                string? answer = Console.ReadLine();
                if (answer != null && Regex.IsMatch(answer, pattern, RegexOptions.CultureInvariant))
                {
                    ConsoleDisplay.ClearLine(2);
                    allPlayers.Add(new Player(answer, true, 0));
                    valid = true;
                }
                else
                {
                    ConsoleDisplay.ClearLine(2);
                    Console.WriteLine("Invalid input : please enter 3 to 12 alphanumerical characters.");
                    continue;
                }
            }
            Console.WriteLine($"Your name is {allPlayers[0].name}.\n");
            
            for (int i = 0; i < nbPlayer - 1; i++)  // AI players
            {
                allPlayers.Add(new Player(Enum.GetName(typeof(Name), i) , false, i+1));
            }

            // --- Assign roles ---
            if (simpleGame)
            {
                List<int> availableRoles = [];
                int nbWerewolf = (int)Math.Round(allPlayers.Count * 0.2f);
                for (int i = 0; i < nbWerewolf; i++)
                {
                    availableRoles.Add(1);  // werewolves
                }
                availableRoles.Add(2);      // Fortune teller
                int nbOrdinaryTownFolks = allPlayers.Count - nbWerewolf - 1;
                for (int i = 0; i < nbOrdinaryTownFolks; i++)
                {
                    availableRoles.Add(0);  // ordinary townfolks
                }
                foreach (Player player in allPlayers)
                {
                    int i = GlobalRandom.GetRandom(availableRoles.Count);
                    player.role = (Enums.Role)availableRoles[i];
                    availableRoles.RemoveAt(i);
                }
            }
            else
            {
                throw new Exception("Classic game not implemented yet !");
            }

            // --- Start game ---

            // TEST
            foreach (Player player in allPlayers) { player.PrintRole(); }
            Console.WriteLine();
            ConsoleDisplay.PrintPlayers(allPlayers);
        }

        private static void Wait(int time = 1000)
        {
            Thread.Sleep(time);
        }
    }
}
