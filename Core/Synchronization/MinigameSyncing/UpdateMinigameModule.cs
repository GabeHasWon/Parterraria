using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class UpdateMinigameModule(byte fromWho, short netId, byte[] data) : Module
{
    private readonly byte _fromWho = fromWho;
    private readonly short _netId = netId;
    private readonly byte[] _data = data;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
            Send(-1, _fromWho, false);
     
        using var stream = _data.ToMemoryStream();
        using var reader = new BinaryReader(stream);
        WorldMinigameSystem.worldMinigamesByNetId[_netId].ReadNetData(reader);
    }
}
