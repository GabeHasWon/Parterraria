using Parterraria.Core.BoardSystem.Nodes;
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

    public Rectangle Bounds => new((int)(position.X - halfWidth + 4), (int)(position.Y - halfWidth + 4), (int)halfWidth * 2 - 8, (int)halfWidth * 2 - 8);

    public BoardNode()
    {
        links = new NodeLinks(this);
    }

    public static Asset<Texture2D> Tex(BoardNode node, bool icon = false) => Tex(node.GetType().Name.Replace("Node", "") + (icon ? "_Icon" : ""));
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

        if (links.Any(x => x.ToNode == node))
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

    public void Draw()
    {
        if (WorldBoardSystem.BuildingBoard) // Debug, plain visuals for building
        {
            DrawLinks(false);

            var font = FontAssets.DeathText.Value;
            var namePos = position + new Vector2(-halfWidth + 6) - Main.screenPosition;
            string text = GetType().Name;
            float size = font.MeasureString(text).X * 0.8f;
            Vector2 nameScale = Vector2.Min(new(halfWidth / size), Vector2.One);

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, namePos, Color.White, 0, Vector2.Zero, nameScale);

            NodeDrawing.DrawLine(position - new Vector2(halfWidth), 0, halfWidth * 2);
            NodeDrawing.DrawLine(position - new Vector2(halfWidth), MathHelper.PiOver2, halfWidth * 2);
            NodeDrawing.DrawLine(position + new Vector2(halfWidth), -MathHelper.Pi, halfWidth * 2);
            NodeDrawing.DrawLine(position + new Vector2(halfWidth), -MathHelper.PiOver2, halfWidth * 2);
        }
        else // Nicer visuals for playing
        {
            DrawLinks(true);
            FancyDraw();
        }
    }

    public virtual void FancyDraw()
    {
        NodeDrawing.DrawNodeSquare(position - Main.screenPosition, halfWidth, GetType().Name, Color.LightGray);
    }

    public virtual void DrawLinks(bool fancy)
    {
        foreach (var link in links)
            NodeDrawing.DrawLink(link, position, null, fancy);
    }
}
