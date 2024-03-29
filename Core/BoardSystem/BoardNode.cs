using ReLogic.Content;
using System;
using System.Runtime.InteropServices;
using Terraria.GameContent;

namespace Parterraria.Core.BoardSystem;

public abstract class BoardNode
{
    public Vector2 position;
    public float radius;
    public NodeLinks links;

    public BoardNode()
    {
        links = new NodeLinks(this);
    }

    public static Asset<Texture2D> Tex(BoardNode node) => Tex(node.GetType().Name.Replace("Node", ""));
    public static Asset<Texture2D> Tex(string node) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Nodes/" + node.Replace("Node", ""));

    public abstract void LandOn(Board board, Player player);

    public virtual void Draw()
    {
        DrawLinks();

        Texture2D tex = Tex(GetType().Name).Value;
        Main.spriteBatch.Draw(tex, position - Main.screenPosition, null, Color.White with { A = 0 }, 0f, tex.Size() / 2f, radius / tex.Width, SpriteEffects.None, 0);
    }

    public virtual void DrawLinks()
    {
        foreach (var link in links)
        {
            if (!link.Forward)
                continue;

            var pixel = TextureAssets.MagicPixel.Value;
            float rot = position.AngleTo(link.Node.position);
            float dist = position.Distance(link.Node.position);
            Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, (int)dist, 1), Color.White, rot, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
    }
}
