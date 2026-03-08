using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Parterraria.Common;

internal static class DrawCommon
{
    public static void CenteredString(DynamicSpriteFont font, Vector2 position, string text, Color color) => CenteredString(font, position, text, color, Vector2.One);

    public static void CenteredString(DynamicSpriteFont font, Vector2 position, string text, Color color, Vector2 scale)
    {
        Vector2 size = font.MeasureString(text) * scale;

        for (int i = 0; i < text.Length; ++i)
        {
            if (i > text.Length - 5)
                break;

            if (text.Substring(i, 4) == "[i:")
                size.X += 32;
        }

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, position - size / 2f, color, 0, Vector2.Zero, scale);
    }

    /// <summary>
    /// Draws a basic position marker (block + text) at a position. Useful for building.
    /// </summary>
    internal static void DrawPositionMarker(Vector2 worldPosition, string text, Color? color = null)
    {
        color ??= Color.Green;

        var position = worldPosition - Main.screenPosition;
        CenteredString(FontAssets.ItemStack.Value, position - new Vector2(0, 4), text, Color.White);

        var rect = new Rectangle((int)position.X, (int)position.Y, 16, 16);
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, color.Value);
    }
}
