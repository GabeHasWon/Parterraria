using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.Synchronization;

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
            return;
        else
            WorldMinigameSystem.TryAddMinigame(_type, _area, _playerSpawnLocation, _data);
    }
}
