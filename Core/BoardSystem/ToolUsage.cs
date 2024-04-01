using Parterraria.Core.BoardSystem.Nodes;
using System;
using System.Linq;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem;

internal class ToolUsage
{
    public static BoardNode buildingNode = null;
    public static bool blockUse = false;

    private static int _placementStage = 0;

    internal static void UseTool(ToolMode mode)
    {
        if (blockUse)
        {
            blockUse = false;
            return;
        }

        if (Main.LocalPlayer.lastMouseInterface)
            return;

        switch (mode)
        {
            case ToolMode.Paint:
                PaintNodes();
                break;
            case ToolMode.Link:
                LinkNodes();
                break;
            case ToolMode.Erase:
                EraseNodes();
                break;
            case ToolMode.Unlink:
                UnlinkNodes();
                break;
            default:
                buildingNode = null;
                break;
        }
    }

    private static void UnlinkNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;

        if (leftClick)
        {
            var node = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard).
                nodes.FirstOrDefault(x => x.position.DistanceSQ(Main.MouseWorld) < x.halfWidth * x.halfWidth);

            if (node is null)
                return;

            if (_placementStage == 0)
            {
                buildingNode = node;
                _placementStage = 1;
            }
            else
            {
                NodeLinks.Link link = buildingNode.links.GetNearestLink(Main.MouseWorld);
                buildingNode.links.RemoveLink(link);
            }
        }
    }

    private static void EraseNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;

        if (leftClick)
        {
            var board = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard);
            var node = board.nodes.FirstOrDefault(x => x.position.DistanceSQ(Main.MouseWorld) < x.halfWidth * x.halfWidth);

            if (node is null)
                return;

            foreach (var item in board.nodes.Where(x => x.links.Any(x => x.Node == node)))
                item.links.RemoveLink(node);

            board.nodes.Remove(node);
        }
    }

    private static void LinkNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;

        if (leftClick)
        {
            var node = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard).
                nodes.FirstOrDefault(x => x.position.DistanceSQ(Main.MouseWorld) < x.halfWidth * x.halfWidth);

            if (node is null)
                return;

            if (_placementStage == 0)
            {
                buildingNode = node;
                _placementStage = 1;
            }
            else
            {
                if (!buildingNode.CanLink(node, out string key))
                {
                    Main.NewText(Language.GetTextValue(key), 255, 180, 180);
                    return;
                }

                buildingNode.links.AddLink(node, true);
                node.links.AddLink(buildingNode, false);
                buildingNode = null;
                _placementStage = 0;

                Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.ConnectedNodes.Connected"), 180, 255, 180);
            }
        }
    }

    private static void PaintNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;
        buildingNode ??= new EmptyNode();

        if (leftClick)
        {
            if (_placementStage == 1)
            {
                var board = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard);
                board.nodes.Add(GenerateNode());
                buildingNode = null;
                _placementStage = 0;
            }
            else
            {
                buildingNode.halfWidth = 120;
                _placementStage = 1;
            }
        }

        if (buildingNode is not null)
        {
            if (_placementStage == 0)
                buildingNode.position = Main.MouseWorld;
            else
                buildingNode.halfWidth = Math.Max(Math.Abs(buildingNode.position.X - Main.MouseWorld.X), Math.Abs(buildingNode.position.Y - Main.MouseWorld.Y));
        }
    }

    private static BoardNode GenerateNode()
    {
        var type = buildingNode.GetType();
        var node = Activator.CreateInstance(type) as BoardNode;
        node.position = buildingNode.position;
        node.halfWidth = buildingNode.halfWidth;
        return node;
    }

    internal static void DrawBuilding()
    {
        if (buildingNode is null)
            return;

        buildingNode.Draw();
    }

    internal static void ResetTool()
    {
        buildingNode = null;
        _placementStage = 0;
    }
}
