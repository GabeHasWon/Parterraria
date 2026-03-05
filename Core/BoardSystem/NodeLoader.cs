using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;

namespace Parterraria.Core.BoardSystem;

internal class NodeLoader : ILoadable
{
    internal Dictionary<string, BoardNode> NodesByName = null;
    internal List<BoardNode> Nodes = null;

    public static int NodeCount => ModContent.GetInstance<NodeLoader>().Nodes.Count;

    public static BoardNode Get(string name) => ModContent.GetInstance<NodeLoader>().NodesByName[name];
    public static BoardNode Get(int index) => ModContent.GetInstance<NodeLoader>().Nodes[index];

    public static bool TryGet(string name, out BoardNode node) => ModContent.GetInstance<NodeLoader>().NodesByName.TryGetValue(name, out node);

    public void Load(Mod mod)
    {
        NodesByName = [];
        Nodes = [];
        var types = AssemblyManager.GetLoadableTypes(mod.Code).Where(x => !x.IsAbstract && typeof(BoardNode).IsAssignableFrom(x));

        foreach (var type in types)
        {
            var node = Activator.CreateInstance(type) as BoardNode;
            NodesByName.Add(type.Name, node);
            Nodes.Add(node);

            _ = node.DisplayName;
            _ = node.Tooltip;
        }
    }

    public void Unload() => NodesByName = null;
}
