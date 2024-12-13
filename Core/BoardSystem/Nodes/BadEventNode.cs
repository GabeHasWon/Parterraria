using Parterraria.Core.BoardSystem.Events;
using Parterraria.Core.Synchronization.BoardItemSyncing;
using System.Linq;
using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

public class BadEventNode() : EmptyNode
{
    public override void LandOn(Board board, Player player)
    {
        var events = Microevent.Microevents.Where(x => x.EventQuality is Microevent.Quality.Bad or Microevent.Quality.Terrible or Microevent.Quality.Abysmal);
        var ev = events.ElementAt(Main.rand.Next(events.Count()));

        if (Main.netMode == NetmodeID.SinglePlayer)
            ev.Invoke(player);
        else if (player.whoAmI == Main.myPlayer)
        {
            int slot = Microevent.Microevents.IndexOf(ev);
            new SyncEventModule(slot, player.whoAmI).Send(-1, -1, false);
        }
    }
}
