using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.MinigameSystem.MinigameUI;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
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
            layers.Add(new LegacyGameInterfaceLayer("Parterraria: Board/Minigame UI", DrawMiscUI, InterfaceScaleType.UI));
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
        }

        return true;
    }

    private static void DrawBoardHUD()
    {
        if (WorldBoardSystem.GameFinished)
        {
            DrawPodium();
            return;
        }

        if (Main.npcShop > 0 && Main.LocalPlayer.GetModPlayer<PlayingBoardPlayer>().hasGoneOnCurrentTurn)
        {
            string shop = Language.GetTextValue("Mods.Parterraria.MiscUI.CloseShop");
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.DeathText.Value, shop, new Vector2(190, 436), Color.White, 0f, Vector2.Zero, new(0.35f));
        }

        int yPos = GetYForHUD() + 36;

        string partyPlayers = Language.GetTextValue("Mods.Parterraria.MiscUI.PartyPlayers");
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.DeathText.Value, partyPlayers, new Vector2(25, yPos - 50), Color.White, 0f, Vector2.Zero, new(0.5f));
        string turnCountText = Language.GetTextValue("Mods.Parterraria.MiscUI.Turn", 1 + WorldBoardSystem.Self.turnsGone);
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.DeathText.Value, turnCountText, new Vector2(25, yPos - 76), new Color(200, 200, 200), 0f, Vector2.Zero, new(0.4f));

        for (int i = 0; i < Main.CurrentFrameFlags.ActivePlayersCount; ++i)
        {
            Player player = Main.player[i];

            int type = ModContent.ItemType<AmethystCoin>();
            int core = ModContent.ItemType<CelestialCore>();
            var boardPlayer = player.GetModPlayer<PlayingBoardPlayer>();
            bool goneOnCurrentTurn = WorldMinigameSystem.InMinigame ? boardPlayer.minigameReady : boardPlayer.hasGoneOnCurrentTurn;
            string playerName = $"[c/{(goneOnCurrentTurn ? Color.Green : Color.White).Hex3()}:{player.name}]";
            BoardNode node = boardPlayer.connectedNode;
            string text = $"{playerName}: {player.CountItem(type, 999)} [i:{type}] {player.CountItem(core, 99)} [i:{core}] - [nodeicon:{node.Name}] {node.DisplayName}";

            var headPosition = new Vector2(30, yPos - 8);
            Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, headPosition, 1f, 1f, Color.White);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text, new Vector2(64, yPos - 18), Color.White, 0f, Vector2.Zero, Vector2.One);

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

    private static void DrawPodium()
    {
        int timer = WorldBoardSystem.finishedTimer;
        Vector2 topPosition = new(Main.screenWidth / 2f, 200);
        WorldBoardSystem.WinPlacements place = WorldBoardSystem.placements;

        if (WorldBoardSystem.CanDisplayPlacement(2) && place.Thirds.Count > 0)
        {
            Color col = Color.SaddleBrown * Math.Min((timer - WorldBoardSystem.ThirdPlaceWaitTime) / 60f, 1);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, topPosition + new Vector2(0, 100), PlacementText(2), col, new Vector2(0.6f));
        }

        if (WorldBoardSystem.CanDisplayPlacement(1) && place.Seconds.Count > 0)
        {
            Color col = Color.Silver * Math.Min((timer - WorldBoardSystem.SecondPlaceWaitTime) / 60f, 1);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, topPosition + new Vector2(0, 50), PlacementText(1), col, new Vector2(0.8f));
        }

        if (WorldBoardSystem.CanDisplayPlacement(0))
        {
            Color col = Color.Gold * Math.Min((timer - WorldBoardSystem.FirstPlaceWaitTime) / 60f, 1);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, topPosition, PlacementText(0), col);
        }

        if (timer >= WorldBoardSystem.FirstPlaceWaitTime + 200)
        {
            Color col = Color.Gray * Math.Min((timer - (WorldBoardSystem.FirstPlaceWaitTime + 200)) / 180f, 1) * 0.8f;
            DrawCommon.CenteredString(FontAssets.DeathText.Value, topPosition - new Vector2(0, 44), Language.GetTextValue("Mods.Parterraria.MiscUI.HostExit"), col, new Vector2(0.4f));
        }

        string PlacementText(int placement) => Language.GetText("Mods.Parterraria.MiscUI.Placements." + placement) + ": " + place.GetPodiumNames((byte)placement);
    }

    private static int GetYForHUD()
    {
        if (!Main.playerInventory)
            return Main.LocalPlayer.buffType[0] != 0 ? 166 : 126;

        if (Main.LocalPlayer.tileEntityAnchor.InUse)
            return 425;

        if (Main.npcShop > 0 || Main.LocalPlayer.chest != -1)
            return 485;

        return 315;
    }

    public static bool DrawMinigame()
    {
        WorldMinigameSystem.DrawMinigames();
        return true;
    }

    public static bool DrawMiscUI()
    {
        if (WorldBoardSystem.PlayingParty)
            DrawBoardHUD();

        WorldMinigameSystem.DrawMinigameUI();
        return true;
    }
}