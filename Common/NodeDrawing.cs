using Parterraria.Core.BoardSystem;
using ReLogic.Content;
using System;
using Terraria.GameContent;

namespace Parterraria.Common;

internal static class NodeDrawing
{
    public static Asset<Texture2D> FancyLine = null;

    public static void DrawNodeSquare(Vector2 position, float halfWidth, string nodeName, Color? color = null, bool highlight = false)
    {
        Texture2D texture = BoardNode.Tex(nodeName).Value;
        Vector2 scale = new Vector2(halfWidth * 2) / texture.Size();

        Main.spriteBatch.Draw(texture, position, null, color ?? Color.White, 0f, texture.Size() / 2f, scale, SpriteEffects.None, 0);

        if (highlight)
        {
            Color sine = Color.White * (MathF.Sin(Main.GameUpdateCount * 0.02f) * 0.2f + 0.2f);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, texture.Width, texture.Height), sine, 0f, texture.Size() / 2f, scale, SpriteEffects.None, 0);
        }
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
        Vector2 drawPos = position - Main.screenPosition;

        if (!fancy)
        {
            var pixel = TextureAssets.MagicPixel.Value;
            var col = color ?? Color.White;
            Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, (int)dist, 3), col, rot, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
        else
        {
            FancyLine ??= ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Nodes/Line");
            var col = color ?? Color.White;
            Main.spriteBatch.Draw(FancyLine.Value, drawPos, null, col * 0.4f, rot - MathHelper.PiOver2, Vector2.Zero, new Vector2(2, dist / FancyLine.Height()), SpriteEffects.None, 0);
        }
    }

    public static void DrawArrow(Vector2 position, float rot)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 30, 3), Color.Green, rot + 0.7f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 30, 3), Color.Green, rot - 0.7f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }
}
