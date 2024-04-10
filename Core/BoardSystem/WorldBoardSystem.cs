using Parterraria.Common;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.MinigameSystem;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Parterraria.Core.BoardSystem;

internal class WorldBoardSystem : ModSystem
{
    public static WorldBoardSystem Self => ModContent.GetInstance<WorldBoardSystem>();
    public static bool PlayingParty => Self.playingBoard is not null;
    public static bool InMinigame => Self.playingMinigame is not null;
    public static bool BuildingBoard => !PlayingParty && Self._toolUI.CurrentState is not null;

    public Dictionary<string, Board> worldBoards = [];

    public Board playingBoard = null;
    public Minigame playingMinigame = null;
    public BoardNode hoverNode = null;

    private UserInterface _toolUI = null;
    private UserInterface _keyboardUI = null;
    private UserInterface _miscUI = null;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            _toolUI = new UserInterface();
            _toolUI.SetState(null);

            _keyboardUI = new UserInterface();
            _keyboardUI.SetState(null);

            _miscUI = new UserInterface();
            _miscUI.SetState(null);
        }
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

        if (Main.netMode != NetmodeID.Server)
        {
            _toolUI.SetState(null);
            _keyboardUI.SetState(null);
            _miscUI.SetState(null);
        }
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

    // UI stuff

    public static bool ToolUIOpen() => Self._toolUI.CurrentState is ToolUIState;
    public static void OpenToolUI() => Self._toolUI.SetState(new ToolUIState(Main.LocalPlayer));
    internal static void CloseToolUI() => Self._toolUI.SetState(null);

    public static void OpenKeyboard(UIVirtualKeyboard.KeyboardSubmitEvent submitEvent, Action cancelAction) 
        => Self._keyboardUI.SetState(new UIVirtualKeyboard("Enter Board name", "Party", submitEvent, cancelAction, 0));
    public static void CloseKeyboard() => Self._keyboardUI.SetState(null);

    internal static void SetMiscUI(UIState state) => Self._miscUI.SetState(state);
    internal static void CloseMiscUI() => Self._miscUI.SetState(null);

    public override void UpdateUI(GameTime gameTime)
    {
        if (_keyboardUI.CurrentState is not null && Main.playerInventory)
            _keyboardUI.SetState(null);

        hoverNode = null;

        _toolUI.Update(gameTime);
        _keyboardUI.Update(gameTime);
        _miscUI.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex - 1, new LegacyGameInterfaceLayer(
                "Parterraria: Board",
                DrawBoard,
                InterfaceScaleType.Game)
            );

            layers.Insert(resourceBarIndex - 1, new LegacyGameInterfaceLayer(
                "Parterraria: Misc UI",
                delegate
                {
                    _miscUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Parterraria: Tool UI",
                delegate
                {
                    _toolUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                "Parterraria: Keyboard UI",
                delegate
                {
                    _keyboardUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Add(new LegacyGameInterfaceLayer("Parterraria: Minigame UI", DrawMinigame, InterfaceScaleType.UI));
        }
    }

    public static bool DrawBoard()
    {
        foreach (var item in Self.worldBoards.Values)
            item.Draw();

        if (Self.hoverNode is not null)
            ToolUsage.DrawBoxOnNode(Self.hoverNode);

        ToolUsage.DrawBuilding();

        if (Main.LocalPlayer.HeldItem.ModItem is MinigameTool tool)
            tool.DrawTool();

        if (PlayingParty)
            Main.LocalPlayer.GetModPlayer<PlayingBoardPlayer>().DrawBoardInfo();
        return true;
    }

    public static bool DrawMinigame()
    {
        if (!InMinigame)
            return true;

        Self.playingMinigame.Draw();

        if (Self.playingMinigame.PlayTime < 240)
            DrawCommon.CenteredString(FontAssets.DeathText.Value, Main.ScreenSize.ToVector2() / 2f, Self.playingMinigame.DisplayName.Value, Color.White);

        return true;
    }

    // Board stuff

    public static Board GetBoard(string key) => Self.worldBoards[key];

    internal static void PlayParty(string boardKey)
    {
        Self.playingBoard = GetBoard(boardKey);
        Self.playingBoard.Start();
    }

    internal static void StopParty()
    {
        Self.playingBoard = null;

        Main.LocalPlayer.GetModPlayer<PlayingBoardPlayer>().ExitParty();
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

    public override void PreUpdatePlayers()
    {
        if (InMinigame || WorldMinigameSystem.worldMinigames.Count == 0)
            return;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && !plr.GetModPlayer<PlayingBoardPlayer>().hasGoneOnCurrentTurn)
                return;
        }

        playingMinigame = Main.rand.Next(WorldMinigameSystem.worldMinigames);
    }
}
