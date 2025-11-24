using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loups_Garous_de_Thiercelieux_console.Classes
{
    public class Player
    {
        public bool isHumain;
        public bool isAlive = true;
        public string name;
        public Role role;
        public Player(string name, bool isHumain)
        {
            this.isHumain = isHumain;
            this.name = name;
        }

        public int TownVote(List<Player> allPlayers)
        {
            int choice;
            if (isHumain)
            {

            }
            else
            {
                choice = GlobalRandom.GetRandom(allPlayers.Count);
            }
                return 0;
        }
    }
}
