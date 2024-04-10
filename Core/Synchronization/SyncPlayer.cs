namespace Parterraria.Core.Synchronization;

internal class SyncPlayer : ModPlayer 
{
    public override void OnEnterWorld()
    {
        new RequestBoardsFromServerModule(Main.myPlayer).Send(-1, -1, false);
    }
}
