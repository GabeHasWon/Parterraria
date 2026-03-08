using Terraria.ID;

namespace Parterraria.Core.InventoryStorageSystem;

internal class InventoryPlayer : ModPlayer
{
    private class StoredInventory(StoredInventory old, Item[] inv, Item[] armorAndAcc, Item[] miscAccessories)
    {
        public StoredInventory oldInv = old;
        public Item[] inv = inv;
        public Item[] armorAndAcc = armorAndAcc;
        public Item[] miscAccessories = miscAccessories;

        internal StoredInventory Clone() => (StoredInventory)MemberwiseClone();
    }

    internal class InventoryResetSystem : ModSystem
    {
        public override void OnWorldUnload() => Main.LocalPlayer.GetModPlayer<InventoryPlayer>().FullyResetInventory();
    }

    private StoredInventory _inventory = null;

    public override void Load() => On_Player.SavePlayer += DontSaveStoredPlayer;

    private static void DontSaveStoredPlayer(On_Player.orig_SavePlayer orig, Terraria.IO.PlayerFileData playerFile, bool skipMapSave)
    {
        InventoryPlayer invPlr = playerFile.Player.GetModPlayer<InventoryPlayer>();

        if (invPlr._inventory is null)
        {
            orig(playerFile, skipMapSave);
            return;
        }

        var clone = invPlr._inventory.Clone();
        invPlr.FullyResetInventory();

        orig(playerFile, skipMapSave);

        RecursivelyResetInventory(playerFile.Player, clone);
    }

    private static void RecursivelyResetInventory(Player player, StoredInventory clone)
    {
        if (clone.oldInv is not null)
            RecursivelyResetInventory(player, clone.oldInv);

        player.GetModPlayer<InventoryPlayer>().SwitchInventory(clone.inv, clone.armorAndAcc, clone.miscAccessories);
    }

    public void SwitchInventory(Item[] inventory, Item[] armor, Item[] misc)
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

        var realMisc = new Item[Player.SupportedMiscSlotCount];

        for (int i = 0; i < realMisc.Length; ++i)
        {
            if (i < misc.Length)
                realMisc[i] = misc[i];
            else
            {
                var airItem = new Item(ItemID.None);
                airItem.TurnToAir();
                realMisc[i] = airItem;
            }
        }

        if (_inventory is null)
            _inventory = new(new StoredInventory(null, Player.inventory, Player.armor, Player.miscEquips), realInv, realArmor, realMisc);
        else
            _inventory = new(_inventory, realInv, realArmor, realMisc);

        Player.inventory = _inventory.inv;
        Player.armor = _inventory.armorAndAcc;
        Player.miscEquips = _inventory.miscAccessories;
        Player.itemTime = 0;
        Player.itemAnimation = 0;
    }

    public void SwitchInventory(Item[] inventory, bool preserveEquipment, bool preserveMisc = true) 
        => SwitchInventory(inventory, preserveEquipment ? Player.armor : [], preserveMisc ? Player.miscEquips : []);

    public void ReplaceInventory()
    {
        if (_inventory is null || _inventory.oldInv is null)
            return;

        _inventory = _inventory.oldInv;
        Player.inventory = _inventory.inv;
        Player.armor = _inventory.armorAndAcc;
        Player.miscEquips = _inventory.miscAccessories;
        Player.itemTime = 0;
        Player.itemAnimation = 0;
    }

    internal void FullyResetInventory()
    {
        while (_inventory is not null && _inventory.oldInv is not null)
            ReplaceInventory();
    }
}
