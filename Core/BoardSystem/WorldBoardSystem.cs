using Parterraria.Core.MinigameSystem;
using Parterraria.Core.Synchronization.BoardItemSyncing;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.BoardSystem;

internal partial class WorldBoardSystem : ModSystem
{
    public static WorldBoardSystem Self => ModContent.GetInstance<WorldBoardSystem>();

    /// <summary>
    /// Whether a party is going on; that is, a board is being played.
    /// </summary>
    public static bool PlayingParty => Self.playingBoard is not null;

    public static bool BuildingBoard => !PlayingParty && (Main.netMode == NetmodeID.Server || BoardUISystem.Self.toolUI.CurrentState is not null);

    public static bool GameFinished => Self.turnsGone >= Self.playingBoard.GetData

    public Dictionary<string, Board> worldBoards = [];

    public Board playingBoard = null;
    public BoardNode hoverNode = null;
    public int boardHost = -1;
    public string playingBoardKey = null;
    public int turnsGone = 0;

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
        bool gameOver = Self.turnsGone >= Self.playingBoard.config.TurnMax;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active)
            {
                if (plr.dead)
                    plr.Spawn(PlayerSpawnContext.ReviveFromDeath);

                plr.Center = gameOver ? Self.playingBoard.config.WinIdlePosition.ToWorldCoordinates(0, 0) : plr.GetModPlayer<PlayingBoardPlayer>().connectedNode.position;
                plr.GetModPlayer<PlayingBoardPlayer>().hasGoneOnCurrentTurn = false;
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
