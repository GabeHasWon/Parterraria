using ReLogic.Content;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace Parterraria.Core.BoardSystem;

public abstract class BoardNode
{
    public Vector2 position;
    public float halfWidth;
    public NodeLinks links;
    
    /// <summary>
    /// Used for IO and for multiplayer syncing.
    /// </summary>
    public int nodeId;

    public Rectangle Bounds => new Rectangle((int)(position.X - halfWidth), (int)(position.Y - halfWidth), (int)halfWidth * 2, (int)halfWidth * 2);

    public BoardNode()
    {
        links = new NodeLinks(this);
    }

    public static Asset<Texture2D> Tex(BoardNode node) => Tex(node.GetType().Name.Replace("Node", ""));
    public static Asset<Texture2D> Tex(string node) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Nodes/" + node.Replace("Node", ""));

    public abstract void LandOn(Board board, Player player);

    internal virtual bool CanLink(BoardNode node, out string denialReasonKey)
    {
        denialReasonKey = null;

        if (node == this)
        {
            denialReasonKey = "Mods.Parterraria.ToolInfo.CantConnectNode.SameNode";
            return false;
        }

        if (links.Any(x => x.Node == node))
        {
            denialReasonKey = "Mods.Parterraria.ToolInfo.CantConnectNode.AlreadyConnected";
            return false;
        }
        return true;
    }

    internal virtual void Save(TagCompound tag)
    {
        tag.Add(nameof(position), position);
        tag.Add(nameof(halfWidth), halfWidth);
        tag.Add(nameof(nodeId), nodeId);

        if (links.LinkCount == 0)
            return;

        TagCompound linksTag = [];
        links.Save(linksTag);
        tag.Add(nameof(links), linksTag);
    }

    /// <summary>
    /// Loads the given node. <paramref name="loadLinks"/> is used to not set links before all nodes are loaded.
    /// </summary>
    /// <param name="tag">Tag to load this node from.</param>
    /// <param name="boardKey">The key of the board that this node is associated with.</param>
    /// <param name="loadLinks">The load action for setting <see cref="links"/>.</param>
    internal virtual void Load(TagCompound tag, string boardKey, out Action loadLinks)
    {
        position = tag.Get<Vector2>(nameof(position));
        halfWidth = tag.GetFloat(nameof(halfWidth));
        nodeId = tag.GetInt(nameof(nodeId));

        if (tag.TryGet(nameof(links), out TagCompound linkTag))
        {
            TagCompound localTag = linkTag;
            loadLinks = () => links = NodeLinks.Load(localTag, boardKey);
        }
        else
            loadLinks = () => { };
    }

    public virtual void Draw()
    {
        if (Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().IsEditing())
            DrawLinks();

        var font = FontAssets.DeathText.Value;
        var namePos = position + new Vector2(-halfWidth + 6) - Main.screenPosition;
        string text = GetType().Name;
        float size = font.MeasureString(text).X * 0.8f;
        Vector2 nameScale = Vector2.Min(new(halfWidth / size), Vector2.One);

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, namePos, Color.White, 0, Vector2.Zero, nameScale);

        DrawLine(position - new Vector2(halfWidth), 0, halfWidth * 2);
        DrawLine(position - new Vector2(halfWidth), MathHelper.PiOver2, halfWidth * 2);
        DrawLine(position + new Vector2(halfWidth), -MathHelper.Pi, halfWidth * 2);
        DrawLine(position + new Vector2(halfWidth), -MathHelper.PiOver2, halfWidth * 2);
    }

    public virtual void DrawLinks()
    {
        foreach (var link in links)
            DrawLink(link, position);
    }

    public static void DrawLink(NodeLinks.Link link, Vector2 position, Color? color = null)
    {
        float rot = position.AngleTo(link.Node.position);
        float dist = position.Distance(link.Node.position);
        DrawLine(position, rot, dist, color);
        DrawArrow(position, rot - MathHelper.Pi);
        DrawArrow(link.Node.position, rot - MathHelper.Pi);
        DrawArrow(Vector2.Lerp(position, link.Node.position, 0.5f), rot - MathHelper.Pi);
    }

    private static void DrawLine(Vector2 position, float rot, float dist, Color? color = null)
    {
        var pixel = TextureAssets.MagicPixel.Value;
        var col = color ?? Color.White;
        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, (int)dist, 3), col, rot, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }

    private static void DrawArrow(Vector2 position, float rot)
    {
        var pixel = TextureAssets.MagicPixel.Value;

        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 30, 3), Color.Green, rot + 0.7f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(pixel, position - Main.screenPosition, new Rectangle(0, 0, 30, 3), Color.Green, rot - 0.7f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }
}
