using NetEasy;
using Parterraria.Core.BoardSystem;
using System;
using System.Collections.Generic;
using System.Linq;
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
            BoardData.NodeData[] orderedNodes = [.. data.Nodes.OrderBy(x => x.Id)];

            foreach (BoardData.NodeData data in orderedNodes)
            {
                var node = BoardNode.GenerateNode(board, Type.GetType(data.Type), data.Position, data.HalfWidth);
                board.AddNode(node);

                setLinkActions.Add(() =>
                {
                    foreach (int item in data.Links)
                    {
                        if (board.nodesById.TryGetValue(item, out BoardNode newNode))
                            node.links.AddLink(newNode);
                    }
                });
            }

            foreach (var item in setLinkActions)
                item();

            board.config = data.Config;
            WorldBoardSystem.Self.worldBoards.Add(data.Key, board);
        }
    }
}
