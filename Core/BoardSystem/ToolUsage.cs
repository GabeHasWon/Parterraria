using System;

namespace Parterraria.Core.BoardSystem;

internal class ToolUsage
{
    public static BuildNode buildingNode = null;

    internal static void UseTool(BoardToolPlayer.ToolMode mode)
    {
        switch (mode)
        {
            case BoardToolPlayer.ToolMode.Paint:
                PaintNodes();
                break;
            default:
                break;
        }
    }

    private static void PaintNodes()
    {
        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;

        if (leftClick)
        {
            buildingNode ??= new BuildNode(Main.MouseWorld, 100, "EmptyNode");

            if (buildingNode.settingWidth)
            {

            }

            buildingNode.settingWidth = true;
        }

        if (!buildingNode.settingWidth)
            buildingNode.position = Main.MouseWorld;
        else
            buildingNode.width = Main.MouseWorld.Distance(buildingNode.position);
    }

    internal static void DrawBuilding()
    {
        if (buildingNode is null)
            return;

        Texture2D tex = BoardNode.Tex(buildingNode.nodeType).Value;
        Main.spriteBatch.Draw(tex, buildingNode.position - Main.screenPosition, null, Color.White, 0f, tex.Size() / 2f, buildingNode.width / tex.Width, SpriteEffects.None, 0);
    }
}
