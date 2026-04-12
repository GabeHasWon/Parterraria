using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Draws a leaderboard from an unordered dictionary of whoAmI mapped to values. Higher values = winning.
    /// </summary>
    internal static void DrawLeaderboard(IDictionary<int, int> playerValuesUnordered, int whoAmIOffset = 0)
    {
        var ordered = playerValuesUnordered.OrderByDescending(x => x.Value);
        int num = 0;

        foreach (var pair in ordered)
        {
            Player player = Main.player[pair.Key - whoAmIOffset];
            DrawCenteredTextFromTop($"{player.name}: #" + (num + 1), 60 + num * 30);
            num++;

            if (num >= Main.CurrentFrameFlags.ActivePlayersCount)
                return;
        }
    }

    /// <summary>
    /// Draws a centered string from the top of the screen.
    /// </summary>
    internal static void DrawCenteredTextFromTop(string text, float yOffset, float scale = 0.5f, Color? color = null)
    {
        var position = new Vector2(Main.screenWidth / 2, yOffset);
        CenteredString(FontAssets.DeathText.Value, position, text, color ?? Color.White, Vector2.One * scale);
    }
}
