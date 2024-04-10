using System;

namespace Parterraria.Core.Synchronization;

[Serializable]
public readonly struct BoardData(string key, BoardData.NodeData[] nodes)
{
    [Serializable]
    public readonly struct NodeData(int id, string type, Vector2 position, float halfWidth, int[] links)
    {
        public readonly int Id = id;
        public readonly string Type = type;
        public readonly Vector2 Position = position;
        public readonly float HalfWidth = halfWidth;
        public readonly int[] Links = links;
    }

    public readonly NodeData[] Nodes = nodes;
    public readonly string Key = key;
}
