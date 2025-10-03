using NetEasy;
using Parterraria.Core.BoardSystem;
using Terraria.ID;
using System;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncPlayerNodeInfoModule(byte who, short current, short next) : Module
{
    private readonly byte who = who;
    private readonly short current = current;
    private readonly short next = next;

    protected override void Receive()
    {
        Player player = Main.player[who];
        PlayingBoardPlayer board = player.GetModPlayer<PlayingBoardPlayer>();
        board.connectedNode = WorldBoardSystem.Self.playingBoard.nodesById[current];

        if (next >= 0)
            board.nextNode = WorldBoardSystem.Self.playingBoard.nodesById[next];
        else if (next == -1)
            board.FinishedRolling();

        if (Main.netMode == NetmodeID.Server)
            Send(-1, who, false);
    }
}