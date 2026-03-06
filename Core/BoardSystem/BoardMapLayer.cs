using ReLogic.Content;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.Map;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem;

internal class BoardMapLayer : ModMapLayer
{
    private static Asset<Texture2D> MapArrow = null;

    public override void Load() => MapArrow = ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Nodes/MapArrow");

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        foreach (var item in WorldBoardSystem.Self.worldBoards)
        {
            Board board = item.Value;

            foreach (var node in board.nodes)
            {
                var tex = BoardNode.Tex(node, "_Icon").Value;
                float zoom = (Main.mapFullscreen ? Main.mapFullscreenScale : Main.mapMinimapScale * 1.5f) / MathF.Max(tex.Width, tex.Height) * node.halfWidth / 8f;

                if (context.Draw(tex, (node.position / 16f).Floor(), Color.White, new SpriteFrame(1, 1, 0, 0), zoom, zoom, Alignment.Center).IsMouseOver)
                    text = Language.GetTextValue(item.Key) + ": " + node.DisplayName.Value;

                foreach (NodeLinks.Link link in node.links)
                {
                    Vector2 distanceToLink = link.ToNode.position - link.Parent.position;
                    Vector2 normal = Vector2.Normalize(distanceToLink);
                    var scale = new Vector2(zoom, distanceToLink.Length() * 0.007f * zoom);
                    Draw(ref context, MapArrow.Value, (link.Parent.position / 16f + normal * 4).Floor(), Color.White, scale, link.AngleToConnector + MathHelper.PiOver2);
                }
            }
        }
    }

    public static void Draw(ref MapOverlayDrawContext context, Texture2D texture, Vector2 position, Color color, Vector2 scale, float rotation)
    {
        position = (position - GetMapPosition(ref context)) * GetMapScale(ref context) + GetMapOffset(ref context);
        Rectangle? _clippingRect = GetClippingRectangle(ref context);

        if (_clippingRect.HasValue && !_clippingRect.Value.Contains(position.ToPoint()))
            return;

        Main.spriteBatch.Draw(texture, position, null, color, rotation, new Vector2(7, 24), scale, SpriteEffects.None, 0f);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_clippingRect")]
    private static extern ref Rectangle? GetClippingRectangle(ref MapOverlayDrawContext context);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mapScale")]
    private static extern ref float GetMapScale(ref MapOverlayDrawContext context);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mapOffset")]
    private static extern ref Vector2 GetMapOffset(ref MapOverlayDrawContext context);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mapPosition")]
    private static extern ref Vector2 GetMapPosition(ref MapOverlayDrawContext context);
}