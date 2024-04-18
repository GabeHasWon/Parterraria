using Parterraria.Common;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem;

/// <summary>
/// Handles the usage of all of the tools in <see cref="BoardUI.ToolUIState"/>.
/// </summary>
internal class ToolUsage
{
    private static string BuildNodeType => NodeLoader.Get(_buildNodeIndex).GetType().AssemblyQualifiedName;

    public static BoardNode buildingNode = null;

    private static int _placementStage = 0;
    private static int _buildNodeIndex = 0;

    internal static void UseTool(ToolMode mode)
    {
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
        bool rightClick = Main.mouseRight && Main.mouseRightRelease;

        if (rightClick)
        {
            buildingNode = null;
            _placementStage = 0;
        }

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
                NodeLinks.Link link = buildingNode.links.GetNearestLink(Main.MouseWorld, out bool noLinks);

                if (!noLinks)
                    buildingNode.links.RemoveLink(link);

                buildingNode = null;
                _placementStage = 0;
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

            board.RemoveNode(node);
        }
    }

    private static void LinkNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;
        bool rightClick = Main.mouseRight && Main.mouseRightRelease;

        if (rightClick)
        {
            buildingNode = null;
            _placementStage = 0;
        }

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
                    Main.NewText(Language.GetTextValue(key), CommonColors.Error);
                    return;
                }

                buildingNode.links.AddLink(node);
                buildingNode = null;
                _placementStage = 0;

                Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.ConnectedNodes.Connected"), CommonColors.Success);
            }
        }
    }

    private static void PaintNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;
        bool rightClick = Main.mouseRight && Main.mouseRightRelease;
        buildingNode ??= Activator.CreateInstance(Type.GetType(BuildNodeType)) as BoardNode;

        if (rightClick)
        {
            _buildNodeIndex++;

            if (_buildNodeIndex >= NodeLoader.NodeCount)
                _buildNodeIndex = 0;

            Vector2 position = buildingNode.position;
            float width = buildingNode.halfWidth;
            buildingNode = Activator.CreateInstance(Type.GetType(BuildNodeType)) as BoardNode;
            buildingNode.position = position;
            buildingNode.halfWidth = width;
        }

        if (leftClick)
        {
            if (_placementStage == 1)
            {
                var board = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard);
                board.AddNode(BoardNode.GenerateNode(board, buildingNode.GetType(), buildingNode.position, buildingNode.halfWidth));
                buildingNode = null;
                _placementStage = 0;
            }
            else
            {
                buildingNode.halfWidth = 40;
                _placementStage = 1;
            }
        }

        if (buildingNode is not null)
        {
            if (_placementStage == 0)
                buildingNode.position = Main.MouseWorld;
            else
            {
                buildingNode.halfWidth = Math.Max(Math.Abs(buildingNode.position.X - Main.MouseWorld.X), Math.Abs(buildingNode.position.Y - Main.MouseWorld.Y));

                if (buildingNode.halfWidth < 40)
                    buildingNode.halfWidth = 40;
            }
        }
    }

    internal static void DrawBuilding()
    {
        if (buildingNode is null)
            return;

        buildingNode.Draw();

        var mode = Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().Mode;

        if (mode == ToolMode.Link && _placementStage == 1)
            DrawBoxOnNode(buildingNode);
        else if (mode == ToolMode.Unlink)
        {
            NodeLinks.Link link = buildingNode.links.GetNearestLink(Main.MouseWorld, out bool noLinks);

            if (!noLinks)
                NodeDrawing.DrawLink(link, buildingNode.position, Color.Red);
        }
    }

    public static void DrawBoxOnNode(BoardNode node)
    {
        var src = new Rectangle(0, 0, (int)node.halfWidth * 2, (int)node.halfWidth * 2);
        var drawPos = node.position - new Vector2(node.halfWidth) - Main.screenPosition;
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawPos, src, Color.White * 0.6f);
    }

    internal static void ResetTool()
    {
        buildingNode = null;
        _placementStage = 0;
    }
}
