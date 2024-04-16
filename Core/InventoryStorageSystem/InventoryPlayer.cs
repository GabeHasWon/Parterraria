using System;
using Terraria.ID;

namespace Parterraria.Core.InventoryStorageSystem;

internal class InventoryPlayer : ModPlayer
{
    private class StoredInventory(StoredInventory old, Item[] inv)
    {
        public StoredInventory oldInv = old;
        public Item[] inv = inv;
    }

    private StoredInventory _inventory = null;

    public override void Load() => On_WorldGen.SaveAndQuitCallBack += ReplaceInventory;

    private void ReplaceInventory(On_WorldGen.orig_SaveAndQuitCallBack orig, object threadContext)
    {
        Main.ActivePlayerFileData.Player.GetModPlayer<InventoryPlayer>().ExitWorld();
        orig(threadContext);
    }

    public void SwitchInventory(Item[] inventory)
    {
        var realInv = new Item[59];

        for (int i = 0; i < realInv.Length; ++i)
        {
            if (i < inventory.Length)
                realInv[i] = inventory[i];
            else
            {
                var airItem = new Item(ItemID.None);
                airItem.TurnToAir();
                realInv[i] = airItem;
            }
        }

        if (_inventory is null)
            _inventory = new(new StoredInventory(null, Player.inventory), realInv);
        else
            _inventory = new(_inventory, realInv);

        Player.inventory = _inventory.inv;
    }

    public void ReplaceInventory()
    {
        if (_inventory is null || _inventory.oldInv is null)
            return;

        _inventory = _inventory.oldInv;
        Player.inventory = _inventory.inv;
    }

    internal void ExitWorld()
    {
        while (_inventory is not null && _inventory.oldInv is not null)
            ReplaceInventory();
    }
}
