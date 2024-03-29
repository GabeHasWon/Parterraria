using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.BoardSystem.Nodes;
using System;
using System.Linq;
using Terraria.Achievements;

namespace Parterraria.Core.BoardSystem;

internal class ToolUsage
{
    public static BoardNode buildingNode = null;

    private static bool _confirmClick = false;

    internal static void UseTool(BoardToolPlayer.ToolMode mode)
    {
        switch (mode)
        {
            case BoardToolPlayer.ToolMode.Paint:
                PaintNodes();
                break;
            case BoardToolPlayer.ToolMode.Link:
                LinkNodes();
                break;
            default:
                buildingNode = null;
                break;
        }
    }

    private static void LinkNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;

        if (leftClick)
        {
            var node = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard).
                nodes.First(x => x.position.DistanceSQ(Main.MouseWorld) < x.radius * x.radius);

            if (node is null)
                return;

            if (!_confirmClick)
            {
                buildingNode = node;
                _confirmClick = true;
            }
            else
            {
                buildingNode.links.AddLink(node, true);
                node.links.AddLink(buildingNode, false);
                buildingNode = null;

                _confirmClick = false;
            }
        }
    }

    private static void PaintNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;
        buildingNode ??= new EmptyNode();

        if (leftClick)
        {
            if (_confirmClick)
            {
                var board = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard);
                board.nodes.Add(GenerateNode());
                buildingNode = null;
                _confirmClick = false;
            }
            else
            {
                buildingNode.radius = 120;
                _confirmClick = true;
            }
        }

        if (buildingNode is not null)
        {
            if (!_confirmClick)
                buildingNode.position = Main.MouseWorld;
            else
                buildingNode.radius = Main.MouseWorld.Distance(buildingNode.position) * 2f;
        }
    }

    private static BoardNode GenerateNode()
    {
        var type = buildingNode.GetType();
        var node = Activator.CreateInstance(type) as BoardNode;
        node.position = buildingNode.position;
        node.radius = buildingNode.radius;
        return node;
    }

    internal static void DrawBuilding()
    {
        if (buildingNode is null)
            return;

        buildingNode.Draw();
    }
}
