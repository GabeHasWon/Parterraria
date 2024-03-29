using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;

namespace Parterraria.Core.BoardSystem;

internal class NodeLoader : ILoadable
{
    internal Dictionary<string, BoardNode> NodesByName = null;

    public static BoardNode Get(string name) => ModContent.GetInstance<NodeLoader>().NodesByName[name];

    public void Load(Mod mod)
    {
        NodesByName = [];
        var types = AssemblyManager.GetLoadableTypes(mod.Code).Where(x => !x.IsAbstract && typeof(BoardNode).IsAssignableFrom(x));

        foreach (var type in types)
            NodesByName.Add(type.Name, Activator.CreateInstance(type) as BoardNode);
    }

    public void Unload() => NodesByName = null;
}
