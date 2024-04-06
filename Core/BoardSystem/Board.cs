using Parterraria.Core.BoardSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.BoardSystem;

public class Board
{
    public readonly List<BoardNode> nodes = [];
    public readonly Dictionary<int, BoardNode> nodesById = [];

    public bool CanStart(out string denialKey)
    {
        denialKey = null;

        if (!nodes.Any())
        {
            denialKey = "Mods.Parterraria.ToolInfo.BoardEmpty";
            return false;
        }

        if (!nodes.Any(x => x is StartNode))
        {
            denialKey = "Mods.Parterraria.ToolInfo.NoStart";
            return false;
        }

        return true;
    }

    public void Start()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            var plr = Main.player[i];

            if (plr.active)
            {
                if (plr.dead)
                    plr.Spawn(PlayerSpawnContext.ReviveFromDeath);

                BoardNode node = nodes.First(x => x is StartNode);
                plr.Center = node.position;
                plr.GetModPlayer<PlayingBoardPlayer>().connectedNode = node;
                plr.GetModPlayer<PlayingBoardPlayer>().connectedNode.LandOn(WorldBoardSystem.Self.playingBoard, plr);
                plr.GetModPlayer<PlayingBoardPlayer>().storedRoll = 0;
                plr.GetModPlayer<BoardToolPlayer>().Mode = ToolMode.None;

                if (i == Main.myPlayer)
                    ToolUsage.ResetTool();
            }
        }
    }

    internal void AddNode(BoardNode boardNode)
    {
        if (boardNode is null)
            throw null;

        nodes.Add(boardNode);
        nodesById.Add(boardNode.nodeId, boardNode);
    }

    internal void RemoveNode(BoardNode node)
    {
        foreach (var item in nodes.Where(x => x.links.Any(x => x.ToNode == node)))
            item.links.RemoveLink(node);

        nodes.Remove(node);
        nodesById.Remove(node.nodeId);
    }

    public void Save(TagCompound boardCompound)
    {
        boardCompound.Add("nodeCount", nodes.Count);

        int nodeId = 0;

        foreach (var node in nodes)
        {
            TagCompound nodeTag = [];
            nodeTag.Add("nodeType", node.GetType().AssemblyQualifiedName);
            node.Save(nodeTag);
            boardCompound.Add("node" + nodeId++, nodeTag);
        }
    }

    public static Board Load(TagCompound boardCompound, string boardKey, out List<Action> linkActions)
    {
        Board board = new();
        int nodeCount = boardCompound.GetInt("nodeCount");
        linkActions = [];

        for (int i = 0; i < nodeCount; ++i)
        {
            TagCompound nodeTag = boardCompound.GetCompound("node" + i);
            var nodeType = Type.GetType(nodeTag.GetString("nodeType"));

            if (nodeType is null || Activator.CreateInstance(nodeType) is not BoardNode node)
                continue;

            node.Load(nodeTag, boardKey, out var linkAction);
            linkActions.Add(linkAction);
            board.AddNode(node);
        }

        return board;
    }

    public void Draw() 
    {
        foreach (var node in nodes)
            node.Draw();
    }
}
