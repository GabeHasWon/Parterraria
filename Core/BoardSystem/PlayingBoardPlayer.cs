using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Core.BoardSystem.Nodes;
using Parterraria.Core.InventoryStorageSystem;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.Synchronization;
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

    /// <summary>
    /// The nodes this player *can* move to in a split path.
    /// </summary>
    public readonly List<BoardNode> splitNodes = [];

    /// <summary>
    /// The current node this player is on.
    /// </summary>
    public BoardNode connectedNode = null;

    /// <summary>
    /// The next node this player needs to move to.
    /// </summary>
    public BoardNode nextNode = null;

    /// <summary>
    /// How many nodes this player needs to move before stopping.
    /// </summary>
    public int storedRoll = 0;

    /// <summary>
    /// Whether this player has gone on the current turn.
    /// </summary>
    public bool hasGoneOnCurrentTurn = false;

    /// <summary>
    /// Whether the player is ready to play the minigame.
    /// </summary>
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
    public bool promptingSplit = false;

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

    private void StopTombstoneOnBoard(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, NetworkText deathText, int hitDirection)
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

        if (CheckNodes is not null && WorldBoardSystem.PlayingParty && !WorldMinigameSystem.InMinigame && !WorldBoardSystem.GameFinished)
        {
            var boardPlayer = CheckNodes.GetModPlayer<PlayingBoardPlayer>();

            if (!boardPlayer.isMoving && boardPlayer.CollideWithNode())
            {
                vel = Vector2.Zero;

                if (Main.netMode != NetmodeID.SinglePlayer && Main.myPlayer == CheckNodes.whoAmI)
                    NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, CheckNodes.whoAmI);
            }
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
        else if (!promptingSplit && Player.talkNPC == -1 && ++moveTimer >= MaxMoveTimer && nextNode is not null)
        {
            Player.Teleport(nextNode.position);
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.fallStart2 = Player.fallStart;
        }

        if (promptingSplit)
        {
            foreach (BoardNode node in splitNodes)
            {
                if (Player.Hitbox.Intersects(node.Bounds))
                {
                    nextNode = node;
                    promptingSplit = false;
                    break;
                }
            }
        }

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

            storedRoll = roll;
        }
        else
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
            FinishedRolling();

            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
                new SyncPlayerNodeInfoModule((byte)Player.whoAmI, (short)connectedNode.nodeId, -1).Send(-1, -1, false);

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
                promptingSplit = true;
                node = null;
                splitNodes.Clear();

                foreach (var link in connectedNode.links)
                    splitNodes.Add(link.ToNode);
            }

            nextNode = node;
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
                foreach (var link in connectedNode.links)
                    splitNodes.Add(link.ToNode);

                promptingSplit = true;
                node = null;
            }

            nextNode = node;
        }

        isMoving = true;

        if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
            new SyncPlayerNodeInfoModule((byte)Player.whoAmI, (short)connectedNode.nodeId, nextNode is null ? (short)-2 : (short)nextNode.nodeId).Send(-1, -1, false);
    }

    public void FinishedRolling()
    {
        storedRoll = 0;
        nextNode = null;
        isMoving = false;
        connectedNode.LandOn(WorldBoardSystem.Self.playingBoard, Player);
        hasGoneOnCurrentTurn = true;
        promptingSplit = false;
        splitNodes.Clear();

#if DEBUG
        Main.NewText("[DEBUG] Done rolling!");
#endif
    }

    internal void ExitParty()
    {
        nextNode = null;
        connectedNode = null;
        moveTimer = 0;
        isMoving = false;
        hasGoneOnCurrentTurn = false;
        minigameReady = false;
        diceCount = 0;
        storedRoll = 0;
        splitNodes.Clear();

        Player.GetModPlayer<InventoryPlayer>().FullyResetInventory();
    }

    internal void DrawBoardInfo()
    {
        if (!WorldMinigameSystem.InMinigame)
        {
            if (WorldBoardSystem.GameFinished && WorldBoardSystem.HasPlacement && !WorldBoardSystem.placements.NotOnPodium(Player.whoAmI))
            {
                int placement = WorldBoardSystem.placements.GetPodium(Player.whoAmI);

                if (!WorldBoardSystem.CanDisplayPlacement(placement))
                    return;

                Color color = CommonColors.GetPlacementColor(placement);
                var podiumPos = Player.Center - new Vector2(0, 40 - Player.gfxOffY) - Main.screenPosition;
                float scale = placement switch
                {
                    0 => 0.7f,
                    1 => 0.5f,
                    _ => 0.3f,
                };

                DrawCommon.CenteredString(FontAssets.DeathText.Value, podiumPos.Floor(), Language.GetTextValue("Mods.Parterraria.MiscUI.Placements." + placement), color, new(scale));
                return;
            }

            var pos = Player.Center - new Vector2(0, 120 - Player.gfxOffY) - Main.screenPosition;
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, $"{Language.GetTextValue("Mods.Parterraria.MiscUI.Roll")} {storedRoll}", Color.White);

            pos = Player.Center - new Vector2(0, 96 - Player.gfxOffY) - Main.screenPosition;
            string coin = $"[i:{ModContent.ItemType<AmethystCoin>()}]: " + Player.CountItem(ModContent.ItemType<AmethystCoin>());
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, coin, Color.White);

            pos = Player.Center - new Vector2(0, 72 - Player.gfxOffY) - Main.screenPosition;
            coin = $"[i:{ModContent.ItemType<CelestialCore>()}]: " + Player.CountItem(ModContent.ItemType<CelestialCore>());
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, coin, Color.White);

            if (isMoving)
            {
                pos = Player.Center - new Vector2(0, 48 - Player.gfxOffY) - Main.screenPosition;

                if (!promptingSplit)
                {
                    float moveTime = Math.Max(MaxMoveTimer / 60f - moveTimer / 60f, 0);
                    string timeLeft = $"{Language.GetTextValue("Mods.Parterraria.MiscUI.MoveTimer")} " + moveTime.ToString("#0.#") + "s";
                    DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, timeLeft, Color.White);
                }
                else
                {
                    string timeLeft = Language.GetTextValue("Mods.Parterraria.MiscUI.NextNode");
                    DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, timeLeft, Color.White);
                }
            }

#if DEBUG
            pos = Player.Center - new Vector2(0, 144 - Player.gfxOffY) - Main.screenPosition;
            BoardNode curNode = Player.GetModPlayer<PlayingBoardPlayer>().connectedNode;
            BoardNode nexNode = Player.GetModPlayer<PlayingBoardPlayer>().nextNode;
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, curNode is null ? "NO C NODE" : "C: " + curNode.nodeId.ToString(), Color.White);

            pos = Player.Center - new Vector2(0, 168 - Player.gfxOffY) - Main.screenPosition;
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, nexNode is null ? "NO N NODE" : "N: " + nexNode.nodeId.ToString(), Color.White);
#endif
        }
        else if (WorldMinigameSystem.NotReady)
        {
            var pos = Player.Center - new Vector2(0, 36 - Player.gfxOffY) - Main.screenPosition;

            if (Player.chatOverhead.timeLeft > 0)
            {
                pos.Y -= 26;
            }

            string text = minigameReady ? Language.GetTextValue("Mods.Parterraria.MiscUI.Ready") : Language.GetTextValue("Mods.Parterraria.MiscUI.NotReady");
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos.Floor(), text, minigameReady ? Color.Green : Color.Orange);
        }
    }

    internal void StartParty()
    {
        connectedNode.LandOn(WorldBoardSystem.Self.playingBoard, Player);
        storedRoll = 0;
        diceCount = 0;
        minigameReady = false;
        moveTimer = 0;
        isMoving = false;
    }
}
