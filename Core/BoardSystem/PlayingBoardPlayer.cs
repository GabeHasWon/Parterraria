using System;

namespace Parterraria.Core.BoardSystem;

internal class PlayingBoardPlayer : ModPlayer
{
    public BoardNode connectedNode = null;
    public BoardNode nextNode = null;
    public int storedRoll = 0;

    public override void PreUpdate()
    {
        if (!WorldBoardSystem.PlayingParty)
            connectedNode = null;
    }

    public override void PreUpdateMovement()
    {
        if (WorldBoardSystem.PlayingParty)
            CollideWithNode();
    }

    private void CollideWithNode()
    {
        if (Player.Right.X > connectedNode.Bounds.Right)
        {
            Player.Right = new Vector2(connectedNode.Bounds.Right, Player.Right.Y);
            Player.velocity.X = 0;
        }

        if (Player.Left.X < connectedNode.Bounds.Left)
        {
            Player.Left = new Vector2(connectedNode.Bounds.Left, Player.Left.Y);
            Player.velocity.X = 0;
        }

        if (Player.Bottom.Y > connectedNode.Bounds.Bottom)
        {
            Player.position.Y -= Player.Bottom.Y - connectedNode.Bounds.Bottom;
            Player.velocity.Y = 0;
        }

        if (Player.Top.Y < connectedNode.Bounds.Top)
        {
            Player.position.Y -= Player.Top.Y - connectedNode.Bounds.Top;
            Player.velocity.Y = 0;
        }
    }
}
