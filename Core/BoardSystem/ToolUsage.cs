using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI;
using Parterraria.Core.Synchronization;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem;

/// <summary>
/// Handles the usage of all of the tools in <see cref="ToolUIState"/>.
/// </summary>
internal class ToolUsage
{
    internal static string BuildNodeType => NodeLoader.Get(buildNodeIndex).GetType().AssemblyQualifiedName;

    public static BoardNode buildingNode = null;

    internal static int buildNodeIndex = 0;

    private static int _placementStage = 0;

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

            if (Main.netMode == NetmodeID.SinglePlayer)
                board.RemoveNode(node);
            else if (Main.netMode == NetmodeID.MultiplayerClient)
                new RemoveNodeModule(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard, (short)board.nodes.IndexOf(node)).Send(runLocally: false);
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
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface;
        bool rightCLick = Main.mouseRight && Main.mouseRightRelease;

        if (rightCLick && BoardUISystem.ToolUIOpen(out UIState state, false))
        {
            var tool = state as ToolUIState;
            tool.ToggleMinigameNodeList();
        }

        buildingNode ??= Activator.CreateInstance(Type.GetType(BuildNodeType)) as BoardNode;

        if (leftClick && !ToolUIState.HoveringList)
        {
            if (_placementStage == 1)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    var board = WorldBoardSystem.GetBoard(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard);
                    board.AddNode(BoardNode.GenerateNode(board, buildingNode.GetType(), buildingNode.position, buildingNode.halfWidth));
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    string boardName = Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard;
                    new AddNodeModule(boardName, buildingNode.GetType().AssemblyQualifiedName, buildingNode.position, buildingNode.halfWidth).Send();
                }

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
            else if (!Main.mouseMiddle)
            {
                buildingNode.halfWidth = Math.Max(Math.Abs(buildingNode.position.X - Main.MouseWorld.X), Math.Abs(buildingNode.position.Y - Main.MouseWorld.Y));

                if (buildingNode.halfWidth < 40)
                    buildingNode.halfWidth = 40;
            }

            if (Main.mouseMiddle)
                buildingNode.position = Main.MouseWorld - new Vector2(buildingNode.halfWidth);

            if (Main.mouseRight)
            {
                buildingNode = null;
                _placementStage = 0;
            }
        }

        ToolUIState.HoveringList = false;
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
