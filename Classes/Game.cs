using Loups_Garoups_de_Thiercelieux_console.Classes;
using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private int nbWerewolves;
        private bool simpleGame;
        private bool endGame = false;
        private List<Player> allPlayers = [];
        private List<int> discoveredByFT = [];

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
                allPlayers.Add(new Player(Enum.GetName(typeof(Name), i), false, i + 1));
            }

            // --- Assign roles ---
            if (simpleGame)
            {
                List<int> availableRoles = [];
                nbWerewolves = (int)Math.Round(allPlayers.Count * 0.2f);  // 20% of werewolves
                for (int i = 0; i < nbWerewolves; i++)
                {
                    availableRoles.Add(1);  // werewolves
                }
                availableRoles.Add(2);      // Fortune teller
                int nbOrdinaryTownFolks = allPlayers.Count - nbWerewolves - 1;
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

            Player fortuneTeller = GetSpecialPlayer(Role.FortuneTeller);

            #endregion


            // --- Game start ---

            Console.Write("You are a ");
            allPlayers[0].PrintRole();
            Console.WriteLine(" !\n");
            Wait(1000);

            ConsoleDisplay.Next();

            // --- loop ---
            while (!endGame)
            {
                List<int> votes = [];
                List<VoteData> voteResults;
                int victimIndex;

                ConsoleDisplay.Narrrate("The night is approaching. Everyone goes to sleep.\n");

                #region FORTUNE_TELLER

                bool canInvokeFT = false;
                foreach (Player player in allPlayers)
                {
                    if (player.role != Role.FortuneTeller)
                    {
                        if (player.isAlive && !discoveredByFT.Contains(player.indexInPlayerList))
                        {
                            canInvokeFT = true;
                        }
                    }
                }

                if (fortuneTeller.isAlive && canInvokeFT)
                {
                    ConsoleDisplay.Narrrate("The Fortune Teller awakes.\n");
                    InvokeFortuneTeller(fortuneTeller);
                    ConsoleDisplay.Narrrate("The Fortune Teller goes back to sleep.\n");
                    ConsoleDisplay.Next();
                }

                #endregion

                #region WEREWOLVES_VOTE

                // --- werewolves vote ---

                CheckForEndGame();

                ConsoleDisplay.Narrrate("The werewolves are awakening.\n");

                List<Player> werewolves = GetWerewolves();

                if (allPlayers[0].role == Role.Werewolf)
                {
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("Choose someone to devour :");
                }

                foreach (Player werewolf in werewolves) // first vote
                {
                    if (werewolf.isAlive)
                    {
                        votes.Add(werewolf.Vote(allPlayers));
                    }
                }
                Console.WriteLine();

                voteResults = GetWeightedVotes(votes, allPlayers);
                victimIndex = GetVictimFromVotes(voteResults);

                while (victimIndex == -1) // if voters don't agree
                {
                    if (allPlayers[0].role == Role.Werewolf)
                    {
                        ConsoleDisplay.PrintPlayers(allPlayers, votes);
                        Console.WriteLine("The werewolves couldn't agree. Choose again among the designated victims :");
                    }
                    votes.Clear();
                    votes = NewVote(voteResults, werewolves);
                    Console.WriteLine();
                    voteResults = GetWeightedVotes(votes, allPlayers);
                    victimIndex = GetVictimFromVotes(voteResults);
                }
                allPlayers[victimIndex].isAlive = false; // kill the chosen victim

                ConsoleDisplay.Narrrate($"The werewolves have chosen to kill {allPlayers[victimIndex].name}\n");
                ConsoleDisplay.Narrrate("The werewolves go back to sleep.\n");
                ConsoleDisplay.Next();

                #endregion

                #region TOWN_VOTE
                // --- town vote ---

                CheckForEndGame();

                ConsoleDisplay.Narrrate("The sun rises. The town wakes up.\n");
                ConsoleDisplay.Narrrate($"The werewolves have striked tonight. {allPlayers[victimIndex].name} has been eaten.\n");
                ConsoleDisplay.Narrrate("The townfolks gather at the village square.\n");
                ConsoleDisplay.Narrrate("In an attempt to get rid of the werewolves, everybody vote for a scapegoat to sacrifice.\n");
                ConsoleDisplay.PrintPlayers(allPlayers);

                if (allPlayers[0].isAlive) { Console.WriteLine("Who do you think should be sacrificed ?"); }
                votes.Clear();
                foreach (Player player in allPlayers)
                {
                    if (player.isAlive)
                    {
                        votes.Add(player.Vote(allPlayers));
                    }
                }
                Console.WriteLine();
                voteResults = GetWeightedVotes(votes, allPlayers);
                victimIndex = GetVictimFromVotes(voteResults);

                if (victimIndex > -1)
                {
                    allPlayers[victimIndex].isAlive = false;
                    ConsoleDisplay.Narrrate($"The scapegoat is {allPlayers[victimIndex].name}. This person was a {allPlayers[victimIndex].role}\n");
                }

                CheckForEndGame();
                ConsoleDisplay.Next();

                #endregion


            }


            #region ENDGAME

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[DEBUG] end of the game");
            Console.ForegroundColor = ConsoleColor.White;

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

        private void InvokeFortuneTeller(Player? fortuneTeller)
        {
            if (fortuneTeller != null)
            {
                if (fortuneTeller.isHumain)
                {
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("Choose someone's card to see :");
                }

                var result = fortuneTeller.SeeCard(allPlayers, discoveredByFT);
                Player target = allPlayers[result.index];
                discoveredByFT.Add(result.index);

                if (fortuneTeller.isHumain)
                {
                    Console.Write("This person is a ");
                    target.PrintRole();
                    Console.WriteLine("\n");
                    target.isDiscovered = true;
                }
            }
        }

        private List<VoteData> GetWeightedVotes(List<int> votes, List<Player> targets)
        {
            votes.Sort();
            List<VoteData> weightedVotes = [];
            foreach (int vote in votes)
            {
                if (weightedVotes.Count > 0)
                {
                    VoteData lastElement = weightedVotes.Last();
                    if (lastElement.vote == vote)
                    {
                        int i = weightedVotes.Count - 1;
                        weightedVotes.RemoveAt(i);
                        lastElement.weight++;
                        weightedVotes.Add(lastElement);
                    }
                    else { weightedVotes.Add(new VoteData(vote, 1)); }
                }
                else { weightedVotes.Add(new VoteData(vote, 1)); }
            }
            return weightedVotes;
        }

        private int GetVictimFromVotes(List<VoteData> votes)
        {
            int victimIndex;
            var sort = votes.OrderByDescending(item => item.weight); // sort by weight
            VoteData[] sortedVotes = sort.ToArray();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (VoteData vote in sortedVotes)
            {
                Console.WriteLine("[DEBUG] " + vote);
            }
            Console.WriteLine();

            if (sortedVotes.Length == 1 || sortedVotes[0].weight != sortedVotes[1].weight)
            {
                victimIndex = sortedVotes[0].vote;
                Console.WriteLine($"[DEBUG] the victim will be {allPlayers[victimIndex].name}\n");
            }
            else // need another vote
            {
                victimIndex = -1;
            }
            Console.ForegroundColor = ConsoleColor.White;
            return victimIndex;
        }

        private List<int> NewVote(List<VoteData> previousVote, List<Player> voters)
        {
            List<int> NextVoteTargetsIndex = [];
            List<int> votes = [];
            foreach (VoteData vote in previousVote) // get the players who got voted
            {
                NextVoteTargetsIndex.Add(vote.vote);
            }
            foreach (Player player in voters) // vote again
            {
                votes.Add(player.VoteFromIndex(NextVoteTargetsIndex));
            }
            return votes;
        }

        private int CheckForEndGame()
        {
            int aliveWerewolves = 0;
            int aliveTownFolks = 0;
            foreach (Player player in allPlayers)
            {
                if (player.isAlive)
                {
                    if (player.role == Role.Werewolf)
                    {
                        aliveWerewolves++;
                    }
                    else
                    {
                        aliveTownFolks++;
                    }
                }
            }

            if (aliveTownFolks == 0)
            {
                endGame = true;
                return 0;
            }
            else if (aliveWerewolves == 0)
            {
                endGame = true;
                return 1;
            }
            else { return 2; }
        }

        private void Wait(int time = 1000)   // meant to be replaced with something more elegant
        {
            Thread.Sleep(time);
        }

        #endregion
    }
}

