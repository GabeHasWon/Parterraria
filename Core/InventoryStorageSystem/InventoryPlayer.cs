using Terraria.ID;

namespace Parterraria.Core.InventoryStorageSystem;

internal class InventoryPlayer : ModPlayer
{
    private class StoredInventory(StoredInventory old, Item[] inv, Item[] armorAndAcc)
    {
        public StoredInventory oldInv = old;
        public Item[] inv = inv;
        public Item[] armorAndAcc = armorAndAcc;
    }

    private StoredInventory _inventory = null;

    public override void Load() => On_WorldGen.SaveAndQuitCallBack += ReplaceInventory;

    private void ReplaceInventory(On_WorldGen.orig_SaveAndQuitCallBack orig, object threadContext)
    {
        Main.ActivePlayerFileData.Player.GetModPlayer<InventoryPlayer>().ExitWorld();
        orig(threadContext);
    }

    public void SwitchInventory(Item[] inventory, Item[] armor)
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

        var realArmor = new Item[20];

        for (int i = 0; i < realArmor.Length; ++i)
        {
            if (i < armor.Length)
                realArmor[i] = armor[i];
            else
            {
                var airItem = new Item(ItemID.None);
                airItem.TurnToAir();
                realArmor[i] = airItem;
            }
        }

        if (_inventory is null)
            _inventory = new(new StoredInventory(null, Player.inventory, Player.armor), realInv, realArmor);
        else
            _inventory = new(_inventory, realInv, realArmor);

        Player.inventory = _inventory.inv;
        Player.armor = _inventory.armorAndAcc;
        Player.itemTime = 0;
        Player.itemAnimation = 0;
    }

    public void SwitchInventory(Item[] inventory, bool preserveEquipment) => SwitchInventory(inventory, preserveEquipment ? Player.armor : []);

    public void ReplaceInventory()
    {
        if (_inventory is null || _inventory.oldInv is null)
            return;

        _inventory = _inventory.oldInv;
        Player.inventory = _inventory.inv;
        Player.armor = _inventory.armorAndAcc;
        Player.itemTime = 0;
        Player.itemAnimation = 0;
    }

    internal void ExitWorld()
    {
        while (_inventory is not null && _inventory.oldInv is not null)
            ReplaceInventory();
    }
}
