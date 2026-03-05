using Microsoft.Xna.Framework.Graphics.PackedVector;
using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.MinigameSystem.MinigameUI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Parterraria.Core.BoardSystem;

internal class BoardUISystem : ModSystem
{
    public static BoardUISystem Self => ModContent.GetInstance<BoardUISystem>();

    internal UserInterface toolUI = null;
    internal UserInterface miscUI = null;

    private UserInterface _keyboardUI = null;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            toolUI = new UserInterface();
            toolUI.SetState(null);

            _keyboardUI = new UserInterface();
            _keyboardUI.SetState(null);

            miscUI = new UserInterface();
            miscUI.SetState(null);
        }
    }

    public override void ClearWorld()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            toolUI.SetState(null);
            miscUI.SetState(null);
            _keyboardUI.SetState(null);
        }
    }

    public static bool ToolUIOpen(bool? isMinigame = null)
    {
        if (isMinigame == true)
            return Self.toolUI.CurrentState is MinigameEditUI;

        if (isMinigame == false)
            return Self.toolUI.CurrentState is ToolUIState;

        return Self.toolUI.CurrentState is not null;
    }

    public static bool ToolUIOpen(out UIState state, bool? isMinigame = null)
    {
        state = Self.toolUI.CurrentState;
        return ToolUIOpen(isMinigame);
    }

    public static void OpenToolUI(bool? minigame = false, PartyPopper popper = null)
    {
        UIState state = minigame switch
        {
            null => new PopperUI(popper),
            true => new MinigameEditUI(Main.LocalPlayer),
            false => new ToolUIState(Main.LocalPlayer)
        };

        Self.toolUI.SetState(state);
    }

    internal static void CloseToolUI() => Self.toolUI.SetState(null);

    public static void OpenKeyboard(UIVirtualKeyboard.KeyboardSubmitEvent submitEvent, Action cancelAction)
        => Self._keyboardUI.SetState(new UIVirtualKeyboard("Enter Board name", "Party", submitEvent, cancelAction, 0));
    public static void CloseKeyboard() => Self._keyboardUI.SetState(null);

    internal static void SetMiscUI(UIState state) => Self.miscUI.SetState(state);
    internal static void CloseMiscUI() => Self.miscUI.SetState(null);

    internal static void CheckCloseMiscUI<T>() where T : UIState
    {
        if (Self.miscUI.CurrentState is T)
            Self.miscUI.SetState(null);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (_keyboardUI.CurrentState is not null && Main.playerInventory)
            _keyboardUI.SetState(null);

        if (_keyboardUI.CurrentState is null)
            toolUI.Update(gameTime);

        _keyboardUI.Update(gameTime);
        miscUI.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex - 2, new LegacyGameInterfaceLayer(
                "Parterraria: Board",
                DrawBoard,
                InterfaceScaleType.Game)
            );

            layers.Insert(resourceBarIndex + 2, new LegacyGameInterfaceLayer(
                "Parterraria: Misc UI",
                delegate
                {
                    miscUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Parterraria: Tool UI",
                delegate
                {
                    if (_keyboardUI.CurrentState is null)
                        toolUI.Draw(Main.spriteBatch, Main.gameTimeCache);

                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Parterraria: Keyboard UI",
                delegate
                {
                    _keyboardUI.Draw(Main.spriteBatch, Main.gameTimeCache);
                    return true;
                },
                InterfaceScaleType.UI)
            );

            layers.Add(new LegacyGameInterfaceLayer("Parterraria: Minigame In-World UI", DrawMinigame, InterfaceScaleType.Game));
            layers.Add(new LegacyGameInterfaceLayer("Parterraria: Minigame UI UI", DrawMinigameUI, InterfaceScaleType.UI));
        }
    }

    public static bool DrawBoard()
    {
        foreach (var item in WorldBoardSystem.Self.worldBoards.Values)
            item.Draw();

        if (WorldBoardSystem.Self.hoverNode is not null)
            ToolUsage.DrawBoxOnNode(WorldBoardSystem.Self.hoverNode);

        ToolUsage.DrawBuilding();

        if (Main.LocalPlayer.HeldItem.ModItem is MinigameTool tool)
            MinigameTool.DrawTool();

        if (WorldBoardSystem.PlayingParty)
        {
            for (int i = 0; i < Main.maxPlayers; ++i)
            {
                Player plr = Main.player[i];

                if (plr.active && !plr.dead)
                    plr.GetModPlayer<PlayingBoardPlayer>().DrawBoardInfo();
            }

            DrawBoardHUD();
        }

        return true;
    }

    private static void DrawBoardHUD()
    {
        int yPos = 30;

        for (int i = 0; i < Main.CurrentFrameFlags.ActivePlayersCount; ++i)
        {
            Player player = Main.player[i];

            int type = ModContent.ItemType<AmethystCoin>();
            int core = ModContent.ItemType<CelestialCore>();
            var boardPlayer = player.GetModPlayer<PlayingBoardPlayer>();
            string hasToGo = !boardPlayer.hasGoneOnCurrentTurn ? "Ready..." : "Gone";
            BoardNode node = boardPlayer.connectedNode;
            string text = $"{player.name}: {player.CountItem(type, 999)} [i:{type}] {player.CountItem(core, 99)} [i:{core}] - [nodeicon:{node.Name}] {node.DisplayName} - {hasToGo}";

            Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, Vector2.One);
            var headPosition = new Vector2(Main.ScreenSize.X / 2f - size.X / 2f - 64, yPos - 8);
            Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, headPosition, 1f, 1f, Color.White);
            DrawCommon.CenteredString(FontAssets.MouseText.Value, new Vector2(Main.ScreenSize.X / 2f, yPos), text, Color.White);

            if (new Rectangle((int)headPosition.X - 15, (int)headPosition.Y - 15, 30, 30).Contains(Main.MouseScreen.ToPoint()))
            {
                Main.mouseText = true;
                Main.LocalPlayer.cursorItemIconText = player.name;
                Main.LocalPlayer.cursorItemIconID = -1;
                Main.LocalPlayer.noThrow = 2;
                Main.LocalPlayer.cursorItemIconEnabled = true;
            }

            yPos += 30;
        }
    }

    public static bool DrawMinigame()
    {
        WorldMinigameSystem.DrawMinigames();
        return true;
    }

    public static bool DrawMinigameUI()
    {
        WorldMinigameSystem.DrawMinigameUI();
        return true;
    }
}