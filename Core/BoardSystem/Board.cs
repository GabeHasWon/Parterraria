using Parterraria.Content.Items.Board.Create;
using Parterraria.Content.Items.Board.PartyItems;
using Parterraria.Core.BoardSystem.Nodes;
using Parterraria.Core.InventoryStorageSystem;
using Parterraria.Core.Synchronization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.BoardSystem;

public class Board
{
    public readonly List<BoardNode> nodes = [];
    public readonly Dictionary<int, BoardNode> nodesById = [];

    public BoardConfig config = new();

    public bool CanStart([NotNullWhen(false)] out string denialKey)
    {
        denialKey = null;

        if (Main.CurrentFrameFlags.AnyActiveBossNPC)
        {
            denialKey = "Mods.Parterraria.ToolInfo.Board.BossActive";
            return false;
        }

        if (nodes.Count == 0)
        {
            denialKey = "Mods.Parterraria.ToolInfo.Board.BoardEmpty";
            return false;
        }

        if (!nodes.Any(x => x is StartNode))
        {
            denialKey = "Mods.Parterraria.ToolInfo.Board.NoStart";
            return false;
        }

        if (config.WinIdlePosition == Point.Zero || config.FirstPlacePosition == Point.Zero || config.SecondPlacePosition == Point.Zero || config.ThirdPlacePosition == Point.Zero)
        {
            denialKey = "Mods.Parterraria.ToolInfo.Board.NoWin";
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
                plr.fallStart = (int)(node.position.Y / 16f);

                if (i == WorldBoardSystem.Self.boardHost)
                    plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                        [
                            new Item(ItemID.GoldPickaxe),
                            new Item(ItemID.BladeofGrass),
                            new Item(ModContent.ItemType<BoardTool>()),
                            new Item(ModContent.ItemType<NormalDice>()),
                        ], true);
                else
                    plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                        [
                            new Item(ItemID.GoldPickaxe),
                            new Item(ItemID.BladeofGrass),
                            new Item(ModContent.ItemType<NormalDice>()),
                        ], true);

                plr.GetModPlayer<PlayingBoardPlayer>().connectedNode = node;
                plr.GetModPlayer<PlayingBoardPlayer>().StartParty();
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

        TagCompound configTag = [];
        config.Save(configTag);
        boardCompound.Add(nameof(config), configTag);
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

        board.config = BoardConfig.Load(boardCompound.GetCompound(nameof(config)));
        return board;
    }

    /// <summary>
    /// Converts this instance into a <see cref="BoardData"/> for synchronization.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal BoardData GetData(string key)
    {
        var nodeData = new BoardData.NodeData[nodes.Count];

        for (int i = 0; i < nodes.Count; ++i)
        {
            BoardNode node = nodes[i];
            int[] links = new int[node.links.LinkCount];

            for (int j = 0; j < links.Length; ++j)
                links[j] = node.links.links[j].ToNode.nodeId;

            nodeData[i] = new BoardData.NodeData(node.nodeId, node.GetType().AssemblyQualifiedName, node.position, node.halfWidth, links);
        }

        var data = new BoardData(key, config, nodeData);
        return data;
    }

    public void Draw() 
    {
        foreach (var node in nodes)
            node.Draw();
    }
}
