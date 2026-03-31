using Parterraria.Common;
using Parterraria.Core.BoardSystem.Events;
using Parterraria.Core.Synchronization.NodeSyncing;
using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

public class EventNode() : EmptyNode
{
    public override void LandOn(Board board, Player player)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.rand.Next(Microevent.Microevents).Invoke(player, null);
        else if (player.whoAmI == Main.myPlayer)
        {
            int slot = Main.rand.Next(Microevent.Microevents.Count);
            byte[] data = NetUtils.WriteAsBytes((writer) => Microevent.Microevents[slot].NetSend(writer));

            new SyncMicroeventModule(slot, player.whoAmI, data).Send(-1, -1, false);
        }
    }
}
