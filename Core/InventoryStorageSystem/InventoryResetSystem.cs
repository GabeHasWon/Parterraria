namespace Parterraria.Core.InventoryStorageSystem;

internal class InventoryResetSystem : ModSystem
{
    public override void OnWorldUnload() => Main.LocalPlayer.GetModPlayer<InventoryPlayer>().ExitWorld();
}
