using Terraria.UI.Chat;

namespace Parterraria.Common;

internal static class DrawCommon
{
    public static void CenteredString(DynamicSpriteFont font, Vector2 position, string text, Color color)
    {
        Vector2 size = font.MeasureString(text);

        for (int i = 0; i < text.Length; ++i)
        {
            if (i > text.Length - 5)
                break;

            if (text.Substring(i, 4) == "[i:")
                size.X += 32;
        }

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, position - size / 2f, color, 0, Vector2.Zero, Vector2.One);
    }
}
