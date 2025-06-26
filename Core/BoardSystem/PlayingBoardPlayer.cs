using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.BoardSystem.Nodes;
using Parterraria.Core.InventoryStorageSystem;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.Synchronization.BoardItemSyncing;
using Parterraria.Core.Synchronization.MinigameSyncing;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem;

internal class PlayingBoardPlayer : ModPlayer
{
    private static Player CheckNodes = null;

    private static int MaxMoveTimer => WorldBoardSystem.Self.playingBoard.config.MaxMoveTimerInSeconds * 60;

    public BoardNode connectedNode = null;
    public BoardNode nextNode = null;
    public int storedRoll = 0;
    public bool hasGoneOnCurrentTurn = false;
    public bool minigameReady = false;

    /// <summary>
    /// How long the player has until they are teleported to the next node.
    /// </summary>
    public int moveTimer = 0;

    /// <summary>
    /// Whether the player is moving to the next node.
    /// </summary>
    public bool isMoving = false;

    /// <summary>
    /// Whether the UI for prompting a split path is open or not.
    /// </summary>
    public bool prompingSplitPath = false;
    
    /// <summary>
    /// How many dice the player has out.
    /// </summary>
    public int diceCount = 0;

    public override void Load()
    {
        On_Player.ShimmerCollision += HijackShimmer;
        On_Player.UpdateTouchingTiles += UnsetCheckNodes;
        On_Player.Spawn += HijackSpawnPosition;
        On_Collision.TileCollision += HijackTileCollision;
        On_Player.DropTombstone += StopTombstoneOnBoard;
    }

    private void StopTombstoneOnBoard(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, Terraria.Localization.NetworkText deathText, int hitDirection)
    {
        if (!WorldBoardSystem.PlayingParty)
            orig(self, coinsOwned, deathText, hitDirection);
    }

    private void HijackSpawnPosition(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);

        if (!WorldMinigameSystem.InMinigame)
        {
            if (connectedNode != null)
            {
                Board board = WorldBoardSystem.Self.playingBoard;
                BoardNode node = board.nodes.First(x => x is StartNode);
                connectedNode = node;
                self.Center = connectedNode.position;
            }
        }
        else
            self.Center = WorldMinigameSystem.Self.playingMinigame.playerStartLocation.ToWorldCoordinates();

        self.fallStart = (int)(self.position.Y / 16f);
        self.fallStart2 = self.fallStart;
    }

    private Vector2 HijackTileCollision(On_Collision.orig_TileCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough, bool fall2, int gravDir)
    {
        Vector2 vel = orig(Position, Velocity, Width, Height, fallThrough, fall2, gravDir);

        if (CheckNodes is not null && WorldBoardSystem.PlayingParty && !WorldMinigameSystem.InMinigame)
        {
            var boardPlayer = CheckNodes.GetModPlayer<PlayingBoardPlayer>();

            if (!boardPlayer.isMoving && boardPlayer.CollideWithNode())
                vel = Vector2.Zero;
        }

        return vel;
    }

    private void UnsetCheckNodes(On_Player.orig_UpdateTouchingTiles orig, Player self)
    {
        CheckNodes = self;
        orig(self);
    }

    private void HijackShimmer(On_Player.orig_ShimmerCollision orig, Player self, bool fallThrough, bool ignorePlats, bool noCollision)
    {
        CheckNodes = null;
        orig(self, fallThrough, ignorePlats, noCollision);
    }

    public override void PreUpdate()
    {
        if (!WorldBoardSystem.PlayingParty)
            connectedNode = null;
        else if (!prompingSplitPath && Player.talkNPC == -1 && ++moveTimer >= MaxMoveTimer && nextNode is not null)
            Player.Teleport(nextNode.position);

        if (nextNode is not null)
        {
            if (Player.Hitbox.Intersects(nextNode.Bounds))
            {
                connectedNode = nextNode;
                connectedNode.PassBy(WorldBoardSystem.Self.playingBoard, Player);
                CheckNextRoll();
            }
        }

        if (WorldMinigameSystem.InMinigame)
        {
            if (Main.mouseRight && Main.myPlayer == Player.whoAmI)
            {
                minigameReady = true;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    new SyncMinigameReadyModule(Main.myPlayer).Send();
            }
        }
        else
            minigameReady = false;
    }

    private bool CollideWithNode()
    {
        if (Player.Right.X > connectedNode.Bounds.Right)
        {
            Player.Right = new Vector2(connectedNode.Bounds.Right, Player.Right.Y);
            Player.velocity.X = 0;
            return true;
        }

        if (Player.Left.X < connectedNode.Bounds.Left)
        {
            Player.Left = new Vector2(connectedNode.Bounds.Left, Player.Left.Y);
            Player.velocity.X = 0;
            return true;
        }

        if (Player.Bottom.Y > connectedNode.Bounds.Bottom)
        {
            Player.position.Y -= Player.Bottom.Y - connectedNode.Bounds.Bottom;
            Player.velocity.Y = 0;
            return true;
        }

        if (Player.Top.Y < connectedNode.Bounds.Top)
        {
            Player.position.Y -= Player.Top.Y - connectedNode.Bounds.Top;
            Player.velocity.Y = Player.gravity;
            Player.jump = 0;
            return true;
        }

        return false;
    }

    internal void SetDiceCount(int count)
    {
        if (!WorldBoardSystem.PlayingParty)
            return;

        if (Main.netMode == NetmodeID.SinglePlayer)
            diceCount = count;
        else if (Main.netMode == NetmodeID.MultiplayerClient)
            new SyncDieCount(Main.myPlayer, count).Send(-1, -1, false);
    }

    public void RolledDice(int roll)
    {
        if (storedRoll == 0)
        {
            if (roll > 0)
                roll++;
            else
                roll--;
        }

        storedRoll += roll;

        if (--diceCount == 0)
            CheckNextRoll();
    }

    private void CheckNextRoll()
    {
        if (!WorldBoardSystem.PlayingParty)
            return;

        if (storedRoll > 0)
            storedRoll--;
        else
            storedRoll++;

        if (storedRoll == 0)
        {
            nextNode = null;
            isMoving = false;
            connectedNode.LandOn(WorldBoardSystem.Self.playingBoard, Player);
            hasGoneOnCurrentTurn = true;
            return;
        }

        moveTimer = 0;
        BoardNode node;

        if (storedRoll > 0)
        {
            if (connectedNode.links.LinkCount == 1)
                node = connectedNode.links.First().ToNode;
            else
            {
                if (Main.myPlayer == Player.whoAmI)
                    BoardUISystem.SetMiscUI(new PromptSplitPathUIState(connectedNode.links.links, false));

                prompingSplitPath = true;
                node = null;
            }

            nextNode = node;
            isMoving = true;
        }
        else
        {
            List<BoardNode> nodes = [];

            foreach (var item in WorldBoardSystem.Self.playingBoard.nodes.Where(x => x.links.HasLinkTo(connectedNode) && x is not StartNode))
                nodes.Add(item);

            if (nodes.Count == 1)
                node = nodes[0].links.GetLinkTo(connectedNode).Parent;
            else
            {
                List<NodeLinks.Link> links = [];

                foreach (var item in nodes)
                    links.Add(item.links.GetLinkTo(connectedNode));

                if (Main.myPlayer == Player.whoAmI)
                    BoardUISystem.SetMiscUI(new PromptSplitPathUIState(links, true));

                prompingSplitPath = true;
                node = null;
            }

            nextNode = node;
            isMoving = true;
        }
    }

    internal void ExitParty()
    {
        nextNode = null;
        connectedNode = null;
        moveTimer = 0;
        isMoving = false;
        hasGoneOnCurrentTurn = false;
        diceCount = 0;
        storedRoll = 0;

        Player.GetModPlayer<InventoryPlayer>().ReplaceInventory();
    }

    internal void DrawBoardInfo()
    {
        if (!WorldMinigameSystem.InMinigame)
        {
            var pos = Player.Center - new Vector2(0, 120 - Player.gfxOffY) - Main.screenPosition;
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, $"{Language.GetTextValue("Mods.Parterraria.MiscUI.Roll")} " + storedRoll, Color.White);

            pos = Player.Center - new Vector2(0, 96 - Player.gfxOffY) - Main.screenPosition;
            string coin = $"[i:{ModContent.ItemType<AmethystCoin>()}]: " + Player.CountItem(ModContent.ItemType<AmethystCoin>());
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, coin, Color.White);

            pos = Player.Center - new Vector2(0, 72 - Player.gfxOffY) - Main.screenPosition;
            coin = $"[i:{ModContent.ItemType<CelestialCore>()}]: " + Player.CountItem(ModContent.ItemType<CelestialCore>());
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, coin, Color.White);

            if (isMoving && !prompingSplitPath)
            {
                pos = Player.Center - new Vector2(0, 48 - Player.gfxOffY) - Main.screenPosition;
                float moveTime = Math.Max(MaxMoveTimer / 60f - moveTimer / 60f, 0);
                string timeLeft = $"{Language.GetTextValue("Mods.Parterraria.MiscUI.MoveTimer")} " + moveTime.ToString("#0.#") + "s";
                DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, timeLeft, Color.White);
            }
        }
        else if (WorldMinigameSystem.NotReady)
        {
            var pos = Player.Center - new Vector2(0, 48 - Player.gfxOffY) - Main.screenPosition;
            string text = minigameReady ? Language.GetTextValue("Mods.Parterraria.MiscUI.Ready") : Language.GetTextValue("Mods.Parterraria.MiscUI.NotReady");
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos.Floor(), text, minigameReady ? Color.Green : Color.Orange);
        }
    }

    internal void StartParty()
    {
        connectedNode.LandOn(WorldBoardSystem.Self.playingBoard, Player);
        storedRoll = 0;
    }
}
