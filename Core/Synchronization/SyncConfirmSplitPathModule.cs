using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncConfirmSplitPathModule(int fromWho, int toNode) : Module
{
    private readonly int _fromWho = fromWho;
    private readonly int _toNode = toNode;

    protected override void Receive()
    {
        var boardPlayer = Main.player[_fromWho].GetModPlayer<PlayingBoardPlayer>();
        boardPlayer.prompingSplitPath = false;
        boardPlayer.nextNode = WorldBoardSystem.Self.playingBoard.nodesById[_toNode];

        if (Main.netMode == NetmodeID.Server)
            Send(-1, _fromWho, false);
    }
}
