using Parterraria.Core.Synchronization.BoardItemSyncing;

namespace Parterraria.Core.Synchronization;

internal class SyncPlayer : ModPlayer 
{
    public override void OnEnterWorld()
    {
        new RequestBoardsFromServerModule(Main.myPlayer).Send(-1, -1, false);
        new RequestMinigamesFromServerModule(Main.myPlayer).Send(-1, -1, false);
        new SyncDieCount(Player.whoAmI, 0).Send(-1, -1, false);
    }
}
