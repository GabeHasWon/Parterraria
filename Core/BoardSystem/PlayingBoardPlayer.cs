using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.BoardSystem.Nodes;
using Parterraria.Core.Synchronization.BoardItemSyncing;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Parterraria.Core.BoardSystem;

internal class PlayingBoardPlayer : ModPlayer
{
    private static Player CheckNodes = null;

    private static int MaxMoveTimer => WorldBoardSystem.Self.playingBoard.config.MaxMoveTimer;

    public BoardNode connectedNode = null;
    public BoardNode nextNode = null;
    public int storedRoll = 0;
    public bool hasGoneOnCurrentTurn = false;

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
        On_Collision.TileCollision += HijackTileCollision;
    }

    private Vector2 HijackTileCollision(On_Collision.orig_TileCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough, bool fall2, int gravDir)
    {
        Vector2 vel = orig(Position, Velocity, Width, Height, fallThrough, fall2, gravDir);

        if (CheckNodes is not null && WorldBoardSystem.PlayingParty)
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

        if (!prompingSplitPath && Player.talkNPC == -1 && ++moveTimer >= MaxMoveTimer && nextNode is not null)
            Player.Teleport(nextNode.position);

        if (nextNode is not null)
        {
            if (Player.Hitbox.Intersects(nextNode.Bounds))
            {
                connectedNode = nextNode;
                connectedNode.PassBy(WorldBoardSystem.Self.playingBoard, Player);
                CheckNextRoll();

                if (nextNode is null)
                    hasGoneOnCurrentTurn = true;
            }
        }
    }

    public override void OnRespawn()
    {
        if (connectedNode != null)
        {
            Board board = WorldBoardSystem.Self.playingBoard;
            BoardNode node = board.nodes.First(x => x is StartNode);
            connectedNode = node;
        }
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
        if (Main.netMode == NetmodeID.SinglePlayer)
            diceCount = count;
        else
            new SyncDieCount(Main.myPlayer, count).Send();
    }

    public void RolledDice(int roll)
    {
        if (roll > 0 && storedRoll == 0)
            roll++;

        storedRoll += roll;

        if (--diceCount == 0)
            CheckNextRoll();
    }

    private void CheckNextRoll()
    {
        if (!WorldBoardSystem.PlayingParty)
            return;

        storedRoll--;

        if (storedRoll <= 0)
        {
            nextNode = null;
            isMoving = false;
            connectedNode.LandOn(WorldBoardSystem.Self.playingBoard, Player);
            return;
        }

        moveTimer = 0;

        BoardNode node;

        if (connectedNode.links.LinkCount == 1)
            node = connectedNode.links.First().ToNode;
        else
        {
            if (Main.myPlayer == Player.whoAmI)
                WorldBoardSystem.SetMiscUI(new PromptSplitPathUIState(connectedNode.links.links));

            prompingSplitPath = true;
            node = null;
        }

        nextNode = node;
        isMoving = true;
    }

    internal void ExitParty()
    {
        nextNode = null;
        connectedNode = null;
        moveTimer = 0;
        isMoving = false;
        hasGoneOnCurrentTurn = false;
    }

    internal void DrawBoardInfo()
    {
        var pos = Player.Center - new Vector2(0, 120) - Main.screenPosition;
        DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, "Roll: " + storedRoll, Color.White);

        pos = Player.Center - new Vector2(0, 96) - Main.screenPosition;
        string coin = $"[i:{ModContent.ItemType<AmethystCoin>()}]: " + Player.CountItem(ModContent.ItemType<AmethystCoin>());
        DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, coin, Color.White);

        pos = Player.Center - new Vector2(0, 72) - Main.screenPosition;
        coin = $"[i:{ModContent.ItemType<CelestialCore>()}]: " + Player.CountItem(ModContent.ItemType<CelestialCore>());
        DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, coin, Color.White);

        if (isMoving && !prompingSplitPath)
        {
            pos = Player.Center - new Vector2(0, 48) - Main.screenPosition;
            float moveTime = Math.Max(MaxMoveTimer / 60f - moveTimer / 60f, 0);
            string timeLeft = $"Move timer: " + moveTime.ToString("#0.#") + "s";
            DrawCommon.CenteredString(FontAssets.ItemStack.Value, pos, timeLeft, Color.White);
        }
    }
}
