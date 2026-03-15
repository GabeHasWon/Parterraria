using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class SyncMinigameModule(string type, Rectangle area, Point playerSpawnLocation, byte[] data) : Module
{
    private readonly string _type = type;
    private readonly Rectangle _area = area;
    private readonly Point _playerSpawnLocation = playerSpawnLocation;
    private readonly byte[] _data = data;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
        
        WorldMinigameSystem.TryAddMinigame(_type, _area, _playerSpawnLocation, _data);
    }
}
