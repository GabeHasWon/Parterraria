using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;

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
                new SyncMinigameModule(game.FullName, game.area).Send(_fromWho, -1, false);
        }
    }
}
