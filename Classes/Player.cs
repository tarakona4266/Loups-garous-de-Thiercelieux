using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public void PrintRole()
        {
            Console.Write(name + " is a ");
            switch (role)
            {
                case Role.Werewolf:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(role);
                    break;
                case Role.Cupido:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(role);
                    break;
                case Role.FortuneTeller:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(role);
                    break;
                case Role.Hunter:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(role);
                    break;
                case Role.Witch:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(role);
                    break;
                case Role.LittleGirl:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(role);
                    break;
                case Role.Sheriff:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(role);
                    break;
                default: Console.Write(role);
                    break;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }
    }
}
