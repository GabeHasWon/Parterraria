using rail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.BoardSystem;

public class NodeLinks(BoardNode parent) : IEnumerable<NodeLinks.Link>
{
    public readonly struct Link(BoardNode node, BoardNode parent)
    {
        public readonly BoardNode ToNode = node;
        public readonly BoardNode Parent = parent;
    }

    public readonly List<Link> links = [];
    public readonly BoardNode parent = parent;

    public int LinkCount => links.Count;

    public void AddLink(BoardNode node)
    {
        Link link = new(node, parent);
        links.Add(link);
    }

    public void RemoveLink(BoardNode node) => links.Remove(GetLinkTo(node));

    public void RemoveLink(Link link)
    {
        links.Remove(link);
        link.ToNode.links.links.Remove(link);
    }

    public Link GetLinkTo(BoardNode node) => links.First(x => x.ToNode == node);
    public bool HasLinkTo(BoardNode node) => links.Any(x => x.ToNode == node);

    public IEnumerator<Link> GetEnumerator() => links.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => links.GetEnumerator();

    internal Link GetNearestLink(Vector2 worldPos, out bool noLinks)
    {
        noLinks = false;

        if (links.Count == 0)
        {
            noLinks = true;
            return default;
        }

        float angle = (worldPos - parent.position).ToRotation();
        return links.MinBy(x => Math.Abs((x.ToNode.position - parent.position).ToRotation() - angle));
    }

    internal void Save(TagCompound tag)
    {
        tag.Add(nameof(parent), parent.nodeId);
        int tagCount = links.Count;
        tag.Add("count", tagCount);

        int linkId = 0;

        foreach (var link in links)
            tag.Add("linkTo" + linkId++, link.ToNode.nodeId);
    }

    public static NodeLinks Load(TagCompound tag, string boardKey)
    {
        Board board = WorldBoardSystem.GetBoard(boardKey);
        int parentId = tag.GetInt(nameof(parent));
        var parentNode = board.nodesById[parentId];
        NodeLinks links = new(parentNode);

        int linkCount = tag.GetInt("count");

        for (int i = 0; i < linkCount; ++i)
        {
            var toNode = board.nodesById[tag.GetInt("linkTo" + i)];
            links.AddLink(toNode);
        }

        return links;
    }
}
