using Terraria.GameContent;

namespace Parterraria.Core.BoardSystem.Nodes;

internal class NodeDrawing
{
    public static void DrawNodeSquare(Vector2 position, float halfWidth, string nodeName, Color? color = null)
    {
        Texture2D texture = BoardNode.Tex(nodeName).Value;
        Vector2 scale = new Vector2(halfWidth * 2) / texture.Size();

        Main.spriteBatch.Draw(texture, position, null, color ?? Color.White, 0f, texture.Size() / 2f, scale, SpriteEffects.None, 0);
    }

    public static void DrawLink(NodeLinks.Link link, Vector2 position, Color? color = null, bool fancy = false)
    {
        float rot = position.AngleTo(link.ToNode.position);
        float dist = position.Distance(link.ToNode.position);
        DrawLine(position, rot, dist, color, fancy);

        if (!fancy)
        {
            DrawArrow(position, rot - MathHelper.Pi);
            DrawArrow(link.ToNode.position, rot - MathHelper.Pi);
            DrawArrow(Vector2.Lerp(position, link.ToNode.position, 0.5f), rot - MathHelper.Pi);
        }
    }

    public static void DrawLine(Vector2 position, float rot, float dist, Color? color = null, bool fancy = false)
    {
        if (!fancy)
        {
            var pixel = TextureAssets.MagicPixel.Value;
            var col = color ?? Color.White;
            Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, (int)dist, 3), col, rot, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
        else
        {
            var pixel = TextureAssets.MagicPixel.Value;
            var col = color ?? Color.White;
            Main.spriteBatch.Draw(pixel, position - Main.screenPosition, null, col, rot - MathHelper.PiOver2, Vector2.Zero, new Vector2(2, dist / pixel.Height), SpriteEffects.None, 0);
        }
    }

    public static void DrawArrow(Vector2 position, float rot)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 30, 3), Color.Green, rot + 0.7f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 30, 3), Color.Green, rot - 0.7f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }
}
