using Loups_Garoups_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Loups_Garoups_de_Thiercelieux_console.Classes
{
    public class Game
    {
        private int nbPlayer;
        private bool simpleGame;
        private List<Player> allPlayers = [];
        private List<Player> werewolves = [];

        // special players
        Player Thief;
        Player Cupido;
        Player FortuneTeller;
        Player Witch;
        Player LittleGirl;
        Player Hunter;
        Player Sheriff;


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
            var answer = Console.ReadLine();
            while (!valid)
            {
                if ()
                {
                    
                }
                else {
            }
            

            for (int i = 0; i < nbPlayer; i++)  // AI players
            {
                allPlayers.Add(new Player(Enum.GetName(typeof(Name), i) , false));
            }

            // --- Assign roles ---
            if (simpleGame)
            {
                List<int> availableRoles = [];
                int nbWerewolf = (int)Math.Round(nbPlayer * 0.2f);
                for (int i = 0; i < nbWerewolf; i++)
                {
                    availableRoles.Add(1);  // werewolves
                }
                availableRoles.Add(2);      // Fortune teller
                int nbOrdinaryTownFolks = nbPlayer - nbWerewolf - 1;
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

        }
    }
}
