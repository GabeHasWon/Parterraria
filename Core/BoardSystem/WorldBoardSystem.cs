using Parterraria.Content.Items.Board;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.Synchronization.BoardItemSyncing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.BoardSystem;

internal partial class WorldBoardSystem : ModSystem
{
    public readonly struct WinPlacements(HashSet<int> winner, HashSet<int> second, HashSet<int> third)
    {
        public readonly HashSet<int> Winners = winner;
        public readonly HashSet<int> Seconds = second;
        public readonly HashSet<int> Thirds = third;

        /// <summary>
        /// Gets podium names, in the format "name, name2, ... nameN".<br/>
        /// 0 = first place, 1 = second place, anything else is third place.
        /// </summary>
        public readonly string GetPodiumNames(byte placement)
        {
            HashSet<int> podiumPlace = placement switch
            {
                0 => Winners,
                1 => Seconds,
                _ => Thirds
            };

            string names = "";

            foreach (int i in podiumPlace)
                names += Main.player[i].name + ", ";

            return names[..^2];
        }

        /// <summary>
        /// Returns the podium of the given player:<br/>
        /// 1st place: 0<br/>
        /// 2nd place: 1<br/>
        /// 3rd place: 2<br/>
        /// Otherwise: 3
        /// </summary>
        public readonly int GetPodium(int player)
        {
            if (Winners.Contains(player))
                return 0;

            if (Seconds.Contains(player))
                return 1;

            if (Thirds.Contains(player))
                return 2;

            return 3;
        }

        /// <summary>
        /// Returns true if the player is not in at least first, second or third, or tied for any of the positions.
        /// </summary>
        public readonly bool NotOnPodium(int player) => !Winners.Contains(player) && !Seconds.Contains(player) && !Thirds.Contains(player);
    }

    public readonly struct PlayerWins(int who, int cores, int coins) : IComparable
    {
        public readonly int WhoAmI = who;
        public readonly int Cores = cores;
        public readonly int Coins = coins;

        public int CompareTo(object obj)
        {
            if (obj is not PlayerWins wins)
                return -1;

            if (wins.Cores != Cores)
                return wins.Cores.CompareTo(Cores);

            return wins.Coins.CompareTo(Coins);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is not PlayerWins wins)
                return false;

            return wins.Coins == Coins && wins.Cores == Cores;
        }

        public override int GetHashCode() => Coins.GetHashCode() ^ Cores.GetHashCode();
    }

    public const int ThirdPlaceWaitTime = 400;
    public const int SecondPlaceWaitTime = ThirdPlaceWaitTime + 200;
    public const int FirstPlaceWaitTime = ThirdPlaceWaitTime + 400;

    public static WorldBoardSystem Self => ModContent.GetInstance<WorldBoardSystem>();

    /// <summary>
    /// Whether a party is going on; that is, a board is being played.
    /// </summary>
    public static bool PlayingParty => Self.playingBoard is not null;

    /// <summary>
    /// If the current player is building a board. Used only for node debug visuals.
    /// </summary>
    public static bool BuildingBoard => !PlayingParty && (Main.netMode == NetmodeID.Server || BoardUISystem.Self.toolUI.CurrentState is not null);

    /// <summary>
    /// If the current board is finished. Will throw if accessed outside of a party; that is, if <see cref="playingBoard"/> is null.
    /// </summary>
    public static bool GameFinished => Self.playingBoard is not null && Self.turnsGone >= Self.playingBoard.config.TurnMax;

    public static bool HasPlacement => GameFinished && finishedTimer > 10;

    internal static int finishedTimer = 0;
    internal static WinPlacements placements = default;

    public Dictionary<string, Board> worldBoards = [];

    public Board playingBoard = null;
    public BoardNode hoverNode = null;
    public int boardHost = -1;
    public string playingBoardKey = null;
    public int turnsGone = 0;

    /// <summary>
    /// If the given placemnet can display. 0 = first, 1 = second, otherwise it's third. Always false if the game isn't finished.
    /// </summary>
    /// <param name="placement"></param>
    /// <returns></returns>
    public static bool CanDisplayPlacement(int placement)
    {
        if (!GameFinished)
            return false;

        if (placement == 0)
            return finishedTimer >= FirstPlaceWaitTime;
        else if (placement == 1)
            return finishedTimer >= SecondPlaceWaitTime;

        return finishedTimer >= ThirdPlaceWaitTime;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag.Add("boardCount", worldBoards.Count);
        int boardId = 0;

        foreach (var item in worldBoards)
        {
            TagCompound boardTag = [];
            boardTag.Add("boardKey", item.Key);
            item.Value.Save(boardTag);
            tag.Add("board" + boardId++, boardTag);
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        int boardCount = tag.GetInt("boardCount");

        for (int i = 0; i < boardCount; i++)
        {
            TagCompound boardTag = tag.GetCompound("board" + i);
            string key = boardTag.GetString("boardKey");
            var board = Board.Load(boardTag, key, out var links);
            worldBoards.Add(key, board);

            foreach (var item in links)
                item();
        }
    }

    public override void ClearWorld()
    {
        worldBoards.Clear();
        worldBoards = [];
        playingBoard = null;
    }

    internal string GetUnrepeatedKey(string value)
    {
        if (!worldBoards.ContainsKey(value))
            return value;

        int copy = 1;

        while (worldBoards.ContainsKey(value + copy))
            copy++;

        return value + copy;
    }

    // Board stuff

    public static Board GetBoard(string key) => Self.worldBoards[key];

    internal static void PlayParty(string boardKey)
    {
        Self.playingBoardKey = boardKey;
        Self.playingBoard = GetBoard(boardKey);
        Self.playingBoard.Start();
        Self.turnsGone = 0;
    }

    internal static void StopParty()
    {
        Self.playingBoard = null;

        WorldMinigameSystem.Self.StopParty();

        foreach (Player player in Main.ActivePlayers)
            player.GetModPlayer<PlayingBoardPlayer>().ExitParty();

        if (Main.netMode != NetmodeID.Server)
            BoardUISystem.CloseMiscUI();
    }

    internal static void CompleteGameFunctionality()
    {
        finishedTimer++;

        if (finishedTimer == 10)
        {
            List<PlayerWins> wins = [];

            foreach (var player in Main.ActivePlayers)
            {
                int coins = player.CountItem(ModContent.ItemType<AmethystCoin>(), 9999);
                int cores = player.CountItem(ModContent.ItemType<CelestialCore>(), 999);

                wins.Add(new PlayerWins(player.whoAmI, cores, coins));
            }

            wins.Sort();

            Queue<PlayerWins> orderedWins = new(wins);
            HashSet<int> winners = GetNextPodium(orderedWins);
            HashSet<int> seconds = GetNextPodium(orderedWins);
            HashSet<int> thirds = GetNextPodium(orderedWins);

            placements = new WinPlacements(winners, seconds, thirds);
        }

        if (finishedTimer == ThirdPlaceWaitTime)
        {
            foreach (int player in placements.Thirds)
            {
                Player plr = Main.player[player];
                plr.Teleport(Self.playingBoard.config.ThirdPlacePosition.ToWorldCoordinates(0, 0));

                for (int i = 0; i < 6; ++i)
                    Dust.NewDust(plr.position, plr.width, plr.head, DustID.Copper);
            }
        }
        else if (finishedTimer == SecondPlaceWaitTime)
        {
            foreach (int player in placements.Seconds)
            {
                Player plr = Main.player[player];
                plr.Teleport(Self.playingBoard.config.SecondPlacePosition.ToWorldCoordinates(0, 0));

                for (int i = 0; i < 8; ++i)
                {
                    int dust = Dust.NewDust(plr.position, plr.width, plr.head, DustID.Silver);
                    Main.dust[dust].velocity.Y -= 4;
                }
            }
        }
        else if (finishedTimer >= FirstPlaceWaitTime)
        {
            if (finishedTimer != FirstPlaceWaitTime && (finishedTimer % 15 != 0 || finishedTimer > FirstPlaceWaitTime + 120))
                return;

            foreach (int player in placements.Winners)
            {
                Player plr = Main.player[player];

                if (finishedTimer == FirstPlaceWaitTime)
                {
                    plr.Teleport(Self.playingBoard.config.FirstPlacePosition.ToWorldCoordinates(0, 0));

                    for (int i = 0; i < 16; ++i)
                    {
                        int dust = Dust.NewDust(plr.position, plr.width, plr.head, DustID.Gold);
                        Main.dust[dust].velocity.Y = Main.rand.NextFloat(-6, -3f);
                        Main.dust[dust].velocity.X = Main.rand.NextFloat(-2, 2);
                    }
                }

                if (Main.myPlayer == plr.whoAmI)
                    SpawnFireworkForPlayer(plr);
            }
        }
    }

    private static void SpawnFireworkForPlayer(Player plr)
    {
        var vel = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-9, -6f));
        Projectile.NewProjectile(plr.GetSource_FromThis(), plr.Center, vel, Main.rand.Next(7) switch
        {
            0 => ProjectileID.RocketFireworkBlue,
            1 => ProjectileID.RocketFireworkGreen,
            2 => ProjectileID.RocketFireworkRed,
            3 => ProjectileID.RocketFireworksBoxBlue,
            4 => ProjectileID.RocketFireworksBoxGreen,
            5 => ProjectileID.RocketFireworksBoxRed,
            _ => ProjectileID.RocketFireworksBoxYellow
        }, 0, 0, plr.whoAmI);
    }

    private static HashSet<int> GetNextPodium(Queue<PlayerWins> orderedWins)
    {
        if (orderedWins.Count == 0)
            return [];

        PlayerWins winner = orderedWins.Dequeue();
        HashSet<int> winners = [winner.WhoAmI];

        while (orderedWins.Count > 0 && orderedWins.Peek().Equals(winner))
            winners.Add(orderedWins.Dequeue().WhoAmI);
        return winners;
    }

    internal static bool CanPlayParty(string boardKey, out string denialKey)
    {
        if (!Self.worldBoards.ContainsKey(boardKey))
        {
            denialKey = "Mods.Parterraria.ToolInfo.Board.InvalidBoard";
            return false;
        }

        Board board = GetBoard(boardKey);
        return board.CanStart(out denialKey);
    }

    internal static void CompleteMinigame(Minigame finishedGame, MinigameRanking rankings)
    {
        Self.turnsGone++;
        finishedTimer = 0;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active)
            {
                if (plr.dead)
                    plr.Spawn(PlayerSpawnContext.ReviveFromDeath);

                PlayingBoardPlayer boardPlr = plr.GetModPlayer<PlayingBoardPlayer>();
                plr.Center = GameFinished ? Self.playingBoard.config.WinIdlePosition.ToWorldCoordinates(0, 0) : boardPlr.connectedNode.position;
                boardPlr.hasGoneOnCurrentTurn = false;
                finishedGame.ResetPlayer(plr);

                plr.fallStart = (int)(plr.position.Y / 16f);
                plr.fallStart2 = plr.fallStart;

                if (Main.myPlayer == i)
                    new ForceResetInformation((byte)i).Send();

                finishedGame.Reward(rankings, plr);
            }
        }
    }
}
