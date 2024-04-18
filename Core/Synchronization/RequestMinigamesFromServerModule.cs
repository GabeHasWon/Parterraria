using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class RequestMinigamesFromServerModule(int fromWho) : Module
{
    private readonly int _fromWho = fromWho;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            foreach (var game in WorldMinigameSystem.worldMinigames)
            {
                using MemoryStream mem = new();
                using BinaryWriter writer = new(mem);
                game.WriteNetData(writer);
                writer.Flush();
                mem.Position = 0;
                byte[] bytes = mem.ReadBytes(mem.Length);
                new SyncMinigameModule(game.FullName, game.area, game.playerStartLocation, bytes).Send(_fromWho, -1, false);
            }
        }
    }
}
