using rail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parterraria.Core.BoardSystem;

public class NodeLinks(BoardNode parent) : IEnumerable<NodeLinks.Link>
{
    public readonly struct Link(bool forward, BoardNode node)
    {
        public readonly bool Forward = forward;
        public readonly BoardNode Node = node;
    }

    public readonly List<Link> links = [];
    public readonly BoardNode parent = parent;

    public int LinkCount => links.Count;

    public void AddLink(BoardNode node, bool forward)
    {
        Link link = new(forward, node);
        links.Add(link);
    }

    public void RemoveLink(BoardNode node) => links.Remove(links.First(x => x.Node == node));

    public void RemoveLink(Link link)
    {
        links.Remove(link);
        link.Node.links.links.Remove(link);
    }

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
        return links.MinBy(x => Math.Abs((x.Node.position - parent.position).ToRotation() - angle));
    }
}
