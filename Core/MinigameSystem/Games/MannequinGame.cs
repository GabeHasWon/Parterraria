using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Input;
using Parterraria.Common;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class MannequinGame : Minigame
{
    private readonly struct Set(int Head, int Body, int Legs)
    {
        public readonly int Head = Head; 
        public readonly int Body = Body; 
        public readonly int Legs = Legs;

        public readonly bool PlayerMatches(Player player) => (IsType(player.armor[0], Head) && IsType(player.armor[1], Body) && IsType(player.armor[2], Legs))
            || (IsType(player.armor[10], Head) && IsType(player.armor[11], Body) && IsType(player.armor[12], Legs));

        static bool IsType(Item item, int type) => item is not null && !item.IsAir && item.type == type;
    }

    private record struct DollInventoryCache(Item[] Inventory, bool HasHead, bool HasBody, bool HasLegs);

    internal static FieldInfo teDollInventory = null;

    private static readonly Set[] Sets = [new Set(ItemID.CyborgHelmet, ItemID.CyborgShirt, ItemID.CyborgPants), new Set(ItemID.RobotHat, ItemID.RobotShirt, ItemID.RobotPants),
        new Set(ItemID.VampireMask, ItemID.VampireShirt, ItemID.VampirePants), new Set(ItemID.CatMask, ItemID.CatShirt, ItemID.CatPants), 
        new Set(ItemID.KarateTortoiseMask, ItemID.KarateTortoiseShirt, ItemID.KarateTortoisePants), 
        new Set(ItemID.SpaceCreatureMask, ItemID.SpaceCreatureShirt, ItemID.SpaceCreaturePants),
        new Set(ItemID.WitchHat, ItemID.WitchDress, ItemID.WitchBoots), new Set(ItemID.RedHat, ItemID.ClothierJacket, ItemID.ClothierPants), 
        new Set(ItemID.LeprechaunHat, ItemID.LeprechaunShirt, ItemID.LeprechaunPants), new Set(ItemID.PumpkinMask, ItemID.PumpkinShirt, ItemID.PumpkinPants),
        new Set(ItemID.WolfMask, ItemID.WolfShirt, ItemID.WolfPants), new Set(ItemID.CreeperMask, ItemID.CreeperShirt, ItemID.CreeperPants),
        new Set(ItemID.FoxMask, ItemID.FoxShirt, ItemID.FoxPants), new Set(ItemID.NurseHat, ItemID.NurseShirt, ItemID.NursePants),
        new Set(ItemID.UnicornMask, ItemID.UnicornShirt, ItemID.UnicornPants)];

    public override MinigameWinType WinType => MinigameWinType.First;
    public override int MaxPlayTime => 0;

    public override void Load() => teDollInventory = typeof(TEDisplayDoll).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 20 * 16)
        {
            rectangle.Width = 40 * 16;
            modified = true;
        }

        if (rectangle.Height < 20 * 16)
        {
            rectangle.Height = 20 * 16;
            modified = true;
        }

        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([], 
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.LightningBoots), new Item(ItemID.EoCShield)], []);
        }
        else
            plr.Center = playerStartLocation.ToWorldCoordinates();
    }

    public override void OnStart()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        PriorityQueue<int, float> head = new();
        PriorityQueue<int, float> body = new();
        PriorityQueue<int, float> legs = new();
        Dictionary<Point16, TEDisplayDoll> displayDolls = [];
        Dictionary<Point16, DollInventoryCache> dollInventories = [];

        for (int i = area.X; i < area.Right; ++i)
        {
            for (int j = area.Y; j < area.Bottom; ++j)
            {
                int x = i / 16;
                int y = j / 16;

                if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity te) && te is TEDisplayDoll mannequin)
                {
                    if (displayDolls.TryAdd(new Point16(x, y), mannequin))
                        dollInventories.Add(new Point16(x, y), new DollInventoryCache([new(), new(), new(), new(), new(), new(), new(), new()], false, false, false));
                }
            }
        }

        for (int i = 0; i < displayDolls.Count; ++i)
        {
            Set set = Main.rand.Next(Sets);
            head.Enqueue(set.Head, Main.rand.NextFloat());
            body.Enqueue(set.Body, Main.rand.NextFloat());
            legs.Enqueue(set.Legs, Main.rand.NextFloat());
        }

        Point16[] keys = [.. displayDolls.Keys];

        while (head.Count > 0)
        {
            Point16 random = Main.rand.Next(keys);
            dollInventories[random].Inventory[0] = new Item(head.Dequeue());
        }

        while (body.Count > 0)
        {
            Point16 random = Main.rand.Next(keys);
            dollInventories[random].Inventory[1] = new Item(body.Dequeue());
        }

        while (legs.Count > 0)
        {
            Point16 random = Main.rand.Next(keys);
            dollInventories[random].Inventory[2] = new Item(legs.Dequeue());
        }

        foreach (Point16 key in keys)
        {
            Item[] inv = dollInventories[key].Inventory;
            teDollInventory.SetValue(displayDolls[key], inv);

            if (Main.netMode == NetmodeID.Server)
                SyncMannequin(displayDolls[key]);
        }
    }

    private static void SyncMannequin(TEDisplayDoll te) => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, te.ID, te.Position.X, te.Position.Y);

    public override void OnStop()
    {
        for (int i = area.X; i < area.Right; ++i)
        {
            for (int j = area.Y; j < area.Bottom; ++j)
            {
                int x = i / 16;
                int y = j / 16;

                if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity te) && te is TEDisplayDoll mannequin)
                {
                    teDollInventory.SetValue(mannequin, (Item[])[new(), new(), new(), new(), new(), new(), new(), new()]);

                    if (Main.netMode == NetmodeID.Server)
                        SyncMannequin(mannequin);
                }
            }
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && Sets.Any(x => x.PlayerMatches(plr)))
                return MinigameRanking.ByFirst(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        foreach (Player plr in Main.ActivePlayers)
        {
            //plr.GetModPlayer<SlipperyPlayer>().Slippery = true;

            if (Sets.Any(x => x.PlayerMatches(plr)))
            {
                Beaten = true;
                return;
            }
        }
    }
}

internal class SlipperyPlayer : ModPlayer
{
    public bool Slippery = false;
    public bool LastSlippery = false;

    public override void ResetEffects()
    {
        LastSlippery = Slippery;
        Slippery = false;
    }
}

public class SlipperyItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    private int _slipTimer = 0;

    public override void UpdateEquip(Item item, Player player)
    {
        if (!player.GetModPlayer<SlipperyPlayer>().LastSlippery)
            return;

        SlipFunctionality(item, player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (!player.GetModPlayer<SlipperyPlayer>().LastSlippery)
            return;

        _slipTimer--;
    }

    private void SlipFunctionality(Item item, Player player)
    {
        _slipTimer++;

        if (_slipTimer > 40 * 60)
        {
            _slipTimer = 0;
            item.noGrabDelay = 60;
            player.QuickSpawnItem(player.GetSource_DropAsItem(), item);
            item.TurnToAir();
        }
    }

    public override void UpdateInventory(Item item, Player player)
    {
        if (!player.GetModPlayer<SlipperyPlayer>().LastSlippery)
            return;

        SlipFunctionality(item, player);
    }
}
