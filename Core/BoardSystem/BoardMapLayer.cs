using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.Map;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem;

internal class BoardMapLayer : ModMapLayer
{
    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        foreach (var item in WorldBoardSystem.Self.worldBoards)
        {
            Board board = item.Value;
            //Vector2 position = Vector2.Zero;

            foreach (var node in board.nodes)
            {
                var tex = BoardNode.Tex(node, true).Value;

                if (context.Draw(tex, (node.position / 16f).Floor(), Color.White, new SpriteFrame(1, 1, 0, 0), 1f, 1f, Alignment.Center).IsMouseOver)
                    text = Language.GetTextValue(item.Key);

                //position += node.position;
            }
        }
    }
}