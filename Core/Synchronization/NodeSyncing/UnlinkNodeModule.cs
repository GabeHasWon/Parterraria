using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.NodeSyncing;

[Serializable]
public class UnlinkNodeModule(string boardName, NodeLinks.Link link) : Module
{
    private readonly string Name = boardName;
    private readonly int NodeId = link.Parent.nodeId;
    private readonly int ConnectionId = link.ToNode.nodeId;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(runLocally: false);

        var board = WorldBoardSystem.GetBoard(Name);
        board.nodesById[NodeId].links.RemoveLink(board.nodesById[ConnectionId]);
    }
}

