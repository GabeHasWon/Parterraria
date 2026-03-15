using NetEasy;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.Synchronization.MinigameSyncing;
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
            {
                byte[] bytes = game.GetNetBytes();
                new SyncMinigameModule(game.FullName, game.area, game.playerStartLocation, bytes).Send(_fromWho, -1, false);
            }
        }
    }
}
