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
        private bool gameEnd;
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
                    allPlayers[0].isDiscovered = true; // allow to print role
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
                int nbWerewolf = (int)Math.Round(allPlayers.Count * 0.2f);  // 20% of werewolves
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
                    player.role = (Role)availableRoles[i];
                    availableRoles.RemoveAt(i);
                }
            }
            if (allPlayers[0].role == Role.Werewolf)
            {
                foreach (Player player in allPlayers)
                {
                    if (player.role == Role.Werewolf)
                    {
                        player.isDiscovered = true;
                    }
                }
            }

            #endregion

            #region GAME LOOP

            // --- Game start ---

            Console.Write("You are a ");
            allPlayers[0].PrintRole();
            Console.WriteLine(" !\n");
            Wait(1000);

            ConsoleDisplay.Next();

            ConsoleDisplay.Narrrate("The night is approaching. Everyone goes to sleep.\n");


                // fortune teller
            ConsoleDisplay.Narrrate("The Fortune Teller awakes.\n");
            InvokeFortuneTeller();
            ConsoleDisplay.Narrrate("The Fortune Teller goes back to sleep.\n");
            ConsoleDisplay.Next();

                // werewolves vote
            ConsoleDisplay.Narrrate("The werewolves are awakening.\n");

            List<Player> werewolves = GetWerewolves();

            if (allPlayers[0].role == Role.Werewolf)
            {
                ConsoleDisplay.PrintPlayers(allPlayers);
                Console.WriteLine("Choose someone to murder :");
            }
            foreach (Player werewolf in werewolves)
            {
                werewolf.Vote(allPlayers);
            }
            Console.WriteLine();

            ConsoleDisplay.Narrrate("The werewolves go back to sleep.\n");
            ConsoleDisplay.Next();


            // town vote
            ConsoleDisplay.Narrrate("The sun rises. The town wakes up.\n");

            ConsoleDisplay.PrintPlayers(allPlayers);

            #endregion
        }

        #region METHODS
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
                if (player.role == Role.Werewolf && player.isAlive) { werewolves.Add(player); }
            }
            return werewolves;
        }

        private List<Player> GetTownfolks()
        {
            List<Player> folks = [];
            foreach (Player player in allPlayers)
            {
                if (player.role != Role.Werewolf && player.isAlive) { folks.Add(player); }
            }
            return folks;
        }

        private List<int> GetTownfolksIndex()
        {
            List<int> townfolks = [];
            int i = 0;
            foreach (Player player in allPlayers)
            {
                if (player.role != Role.Werewolf && player.isAlive) { townfolks.Add(i); }
                i++;
            }
            return townfolks;
        }

        private void InvokeFortuneTeller()
        {
            Player? fortuneTeller = GetSpecialPlayer(Role.FortuneTeller);
            if (fortuneTeller != null)
            {
                if (fortuneTeller.isHumain)
                {
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("\nChoose someone's card to see :");
                }

                var result = fortuneTeller.SeeCard(allPlayers);
                Player target = allPlayers[result.index];
                if (fortuneTeller.isHumain)
                {
                    Console.Write("This person is a ");
                    target.PrintRole();
                    Console.WriteLine("\n");
                    target.isDiscovered = true;
                }
            }
        }

        private void Wait(int time = 1000)   // meant to be replaced with something more elegant
        {
            Thread.Sleep(time);
        }
        #endregion
    }
}

