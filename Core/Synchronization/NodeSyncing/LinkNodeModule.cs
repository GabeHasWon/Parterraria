using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.NodeSyncing;

[Serializable]
public class LinkNodeModule(string boardName, int nodeId, int connectionId) : Module
{
    private readonly string Name = boardName;
    private readonly int NodeId = nodeId;
    private readonly int ConnectionId = connectionId;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(runLocally: false);

        var board = WorldBoardSystem.GetBoard(Name);
        board.nodesById[NodeId].links.AddLink(board.nodesById[ConnectionId]);
    }
}

