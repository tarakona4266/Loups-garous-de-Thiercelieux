using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
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

        public Game(int nbPlayer, bool simpleGame = false)
        {
            this.nbPlayer = nbPlayer;
            this.simpleGame = simpleGame;
        }

        public void Run()
        {
            #region SETUP

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
            Console.WriteLine($"Your name is {allPlayers[0].name}.");
            
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

            #endregion

            // --- Game start ---

            Console.Write("You are a ");
            allPlayers[0].PrintRole();
            Console.WriteLine(" !\n");

            ConsoleDisplay.Narrrate("The night is approaching. Everyone goes to sleep.\n");
            ConsoleDisplay.Narrrate("The werewolves are awakening.");

            // fortune teller
            if (allPlayers[0].role == Role.FortuneTeller)
            {
                ConsoleDisplay.PrintPlayers(allPlayers);
                Console.WriteLine("Choose someone's card to see :");
            }
            Player? fortuneTeller = GetSpecialPlayer(Role.FortuneTeller);
            if (fortuneTeller != null)
            {
                var result = fortuneTeller.SeeCard(allPlayers);
                Player target = allPlayers[result.index];
                if (fortuneTeller.isHumain)
                {
                    Console.Write("\n This person is a ");
                    target.PrintRole();
                    Console.WriteLine();
                }
            }

            // werewolves vote
            ConsoleDisplay.Narrrate("The werewolves are awakening.");

            List<Player> werewolves = GetWerewolves();
            List<int> townfolks = GetTownfolksIndex();


        }


        private Player? GetSpecialPlayer(Role role)
        {
            Player targetPlayer = null;
            foreach (Player player in allPlayers)
            {
                if (player.role == role)
                {
                    targetPlayer = player;
                }
            }
            return targetPlayer;
        }
        private List<Player> GetWerewolves()
        {
            List<Player> werewolves = [];
            foreach (Player player in allPlayers)
            {
                if (player.role == Role.Werewolf) { werewolves.Add(player); }
            }
            return werewolves;
        }

        private List<int> GetTownfolksIndex()
        {
            List<int> townfolks = [];
            int i = 0;
            foreach (Player player in allPlayers)
            {
                if (player.role != Role.Werewolf) { townfolks.Add(i); }
                i++;
            }
            return townfolks;
        }

        private void Wait(int time = 1000)   // meant to be replaced with something more elegant
        {
            Thread.Sleep(time);
        }
    }
}
