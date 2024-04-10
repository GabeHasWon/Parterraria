using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncBoardModule(BoardData boardData) : Module
{
    private readonly BoardData data = boardData;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            return;
        else
        {
            var board = new Board();
            List<Action> setLinkActions = [];

            foreach (BoardData.NodeData data in data.Nodes)
            {
                var node = BoardNode.GenerateNode(board, Type.GetType(data.Type), data.Position, data.HalfWidth);
                board.AddNode(node);

                setLinkActions.Add(() =>
                {
                    foreach (int item in data.Links)
                        node.links.AddLink(board.nodesById[item]);
                });
            }

            foreach (var item in setLinkActions)
                item();

            WorldBoardSystem.Self.worldBoards.Add(data.Key, board);
        }
    }
}
