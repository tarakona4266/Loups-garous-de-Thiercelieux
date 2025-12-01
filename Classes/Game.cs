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
        private (int aliveWerewolves, int aliveTownfolks) endGameResult;

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
            string debugPattern = @"^Debug$";
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

            if (Regex.IsMatch(allPlayers[0].name, debugPattern))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("(i) debug mode enabled\n");
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleDisplay.printDebug = true;
            }

            ConsoleDisplay.Next();

            // --- loop ---
            while (!endGame)
            {
                List<int> votes = [];
                List<VoteData> voteResults;
                int victimIndex;

                ConsoleDisplay.Narrrate("The night is approaching. Everyone goes to sleep.\n");

                #region FORTUNE_TELLER

                if (fortuneTeller.isAlive)
                {
                    bool canInvokeFT = false;
                    foreach (Player player in allPlayers) // avoid calling FT if everyone's role is already exposed
                    {
                        if (player.role != Role.FortuneTeller)
                        {
                            if (player.isAlive && !discoveredByFT.Contains(player.indexInPlayerList))
                            {
                                canInvokeFT = true;
                            }
                        }
                    }
                    if (canInvokeFT)
                    {
                        ConsoleDisplay.Narrrate("The Fortune Teller awakes.\n");
                        InvokeFortuneTeller(fortuneTeller);
                        ConsoleDisplay.Narrrate("The Fortune Teller goes back to sleep.\n");
                        ConsoleDisplay.Next();
                    }
                }

                #endregion

                #region WEREWOLVES_VOTE

                CheckForEndGame();

                ConsoleDisplay.Narrrate("The full moon is shining. The werewolves are awakening.\n");

                List<Player> werewolves = GetWerewolves();

                if (allPlayers[0].role == Role.Werewolf)
                {
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("Who will you devour this night ?");
                }
                else
                {
                    ConsoleDisplay.Narrrate("Wolf howls echo through the night.");
                    ConsoleDisplay.Narrrate("You struggle to sleep while they are debating about their next victim.");
                    ConsoleDisplay.DebugPrint("", true);
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
                    if (allPlayers[0].role == Role.Werewolf && allPlayers[0].isAlive)
                    {
                        ConsoleDisplay.PrintPlayers(allPlayers, votes);
                        Console.WriteLine("The werewolves couldn't agree. Choose again among those savorous victims :");
                    }

                    votes.Clear();
                    votes = NewVote(voteResults, werewolves);
                    ConsoleDisplay.DebugPrint("", true);
                    voteResults = GetWeightedVotes(votes, allPlayers);
                    victimIndex = GetVictimFromVotes(voteResults);
                }
                allPlayers[victimIndex].isAlive = false; // kill the chosen victim

                if (allPlayers[0].role == Role.Werewolf)
                {
                    ConsoleDisplay.Narrrate($"The werewolves have chosen to devour {allPlayers[victimIndex].name}\n");
                }
                ConsoleDisplay.Narrrate("The werewolves' hunger is satisfied for this night.\n");

                endGameResult = CheckForEndGame();
                if (endGameResult.aliveWerewolves == 1 && endGameResult.aliveTownfolks == 1) { endGame = true; }
                ConsoleDisplay.Next();
                if (endGame) { break; }

                #endregion

                #region TOWN_VOTE

                ConsoleDisplay.Narrrate("The sun rises. The town wakes up.\n");
                ConsoleDisplay.Narrrate($"The werewolves have striked tonight. {allPlayers[victimIndex].name} has been eaten.\n");
                ConsoleDisplay.Narrrate("The townfolks gather at the village square.\n");
                ConsoleDisplay.Narrrate("In an attempt to get rid of the werewolves, everybody vote for a scapegoat to sacrifice.\n");
                Wait();
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

                while (victimIndex == -1) // if voters don't agree
                {
                    if (allPlayers[0].isAlive)
                    {
                        ConsoleDisplay.PrintPlayers(allPlayers, votes);
                        Console.WriteLine("The villagers couldn't agree. Choose again among the suspects :");
                    }
                    votes.Clear();
                    votes = NewVote(voteResults, allPlayers);
                    Console.WriteLine();
                    voteResults = GetWeightedVotes(votes, allPlayers);
                    victimIndex = GetVictimFromVotes(voteResults);
                }
                allPlayers[victimIndex].isAlive = false;
                ConsoleDisplay.Narrrate($"The assembly has spoken : the scapegoat is {allPlayers[victimIndex].name}.");
                ConsoleDisplay.Narrrate($"This person was a ", false);
                allPlayers[victimIndex].PrintRole();
                Console.WriteLine("\n");

                endGameResult = CheckForEndGame();
                ConsoleDisplay.Next();
                if (endGame) { break; }

                #endregion

            }

            #region ENDGAME

            ConsoleDisplay.DebugPrint("end of the game\n");

            if (endGameResult.aliveWerewolves > 0 && endGameResult.aliveTownfolks == 0) // Werewolves win
            {
                if (allPlayers[0].role == Role.Werewolf)
                {
                    Console.WriteLine("Congratulation !\n");
                    Console.WriteLine("The werewolves have slayed all the villagers.\n");
                    Console.ForegroundColor= ConsoleColor.Blue;
                    Console.WriteLine("Your team won !");
                    Console.ForegroundColor= ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("Game over !\n");
                    Console.WriteLine("The werewolves have slayed all the villagers.\n");
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("Your team lost.");
                    Console.ForegroundColor= ConsoleColor.White;
                }
            }
            else if (endGameResult.aliveTownfolks > 0 && endGameResult.aliveWerewolves == 0) // Townfolks win
            {
                if (allPlayers[0].role == Role.Werewolf)
                {
                    Console.WriteLine("Game over !\n");
                    Console.WriteLine("The villagers have slayed the last of your kind.\n");
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("Your team lost.");
                    Console.ForegroundColor= ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("Congratulation !\n");
                    Console.WriteLine("The villagers have slayed all the werewolves. Millers Hollow is saved.\n");
                    Console.ForegroundColor= ConsoleColor.Blue;
                    Console.WriteLine("Your team won !");
                    Console.ForegroundColor= ConsoleColor.White;
                }
            }
            else if (endGameResult.aliveWerewolves == 1 && endGameResult.aliveTownfolks == 1)
            {
                if (allPlayers[0].role == Role.Werewolf)
                {
                    Console.WriteLine("Congratulation !\n");
                    Console.WriteLine("With only a villager left, your kind has seized control of Millers Hollow.\n");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Your team won !");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("Game over !\n");
                    Console.WriteLine("There is only a villager left to fight the werewolves. Sadly, this is a lost cause.\n");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your team lost.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

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

        private void InvokeFortuneTeller(Player? fortuneTeller)
        {
            if (fortuneTeller != null)
            {
                if (fortuneTeller.isHumain)
                {
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("Choose someone's card to see :");
                }
                else
                {
                    ConsoleDisplay.Narrrate("No one knows what happens at the fortune teller's house during the night,\nbut everyone agrees that there are secrets that are better left unrevealed.\n");
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

            foreach (VoteData vote in sortedVotes)
            {
                ConsoleDisplay.DebugPrint($"{vote}");
            }
            ConsoleDisplay.DebugPrint("", true);

            if (sortedVotes.Length == 1 || sortedVotes[0].weight != sortedVotes[1].weight)
            {
                victimIndex = sortedVotes[0].vote;
                ConsoleDisplay.DebugPrint($"the victim will be {allPlayers[victimIndex].name}\n");
            }
            else // need another vote
            {
                victimIndex = -1;
            }
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
                if (player.isAlive)
                {
                    votes.Add(player.VoteFromIndex(NextVoteTargetsIndex));
                }
            }
            return votes;
        }

        private (int aliveWerewolves, int aliveTownFolks) CheckForEndGame()
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

            if (aliveTownFolks == 0 || aliveWerewolves == 0)
            {
                endGame = true;
            }
            return (aliveWerewolves, aliveTownFolks);
        }

        private void Wait(int time = 1000)   // meant to be replaced with something more elegant
        {
            Thread.Sleep(time);
        }

        #endregion
    }
}

