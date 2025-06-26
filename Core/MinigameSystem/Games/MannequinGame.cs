using Microsoft.CodeAnalysis;
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

        public readonly bool PlayerMatches(Player player) => IsType(player.armor[0], Head) && IsType(player.armor[1], Body) && IsType(player.armor[2], Legs);

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

    public override bool IsLoadingEnabled(Mod mod) => false;

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
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory(
                [
                    new Item(ItemID.LightningBoots),
                    new Item(ItemID.ShinyRedBalloon),
                ], false, false);
        }
        else
            plr.Center = playerStartLocation.ToWorldCoordinates();
    }

    public override void OnStart()
    {
        PriorityQueue<int, float> head = new();
        PriorityQueue<int, float> body = new();
        PriorityQueue<int, float> legs = new();

        foreach (var item in Sets)
        {
            head.Enqueue(item.Head, Main.rand.NextFloat());
            body.Enqueue(item.Body, Main.rand.NextFloat());
            legs.Enqueue(item.Legs, Main.rand.NextFloat());
        }

        Dictionary<int, TEDisplayDoll> displayDolls = [];
        Dictionary<int, DollInventoryCache> dollInventories = [];

        for (int i = area.X; i < area.Right; ++i)
        {
            for (int j = area.Y; j < area.Bottom; ++j)
            {
                if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is TEDisplayDoll mannequin)
                {
                    int count = displayDolls.Count;
                    displayDolls.Add(count, mannequin);
                    dollInventories.Add(count, new DollInventoryCache([new(), new(), new(), new(), new(), new(), new(), new()], false, false, false));
                }
            }
        }

        while (head.Count > displayDolls.Count)
            head.Dequeue();

        while (body.Count > displayDolls.Count)
            body.Dequeue();

        while (legs.Count > displayDolls.Count)
            legs.Dequeue();

        while (head.Count > 0)
        {
            int random = Main.rand.Next(displayDolls.Count);
            dollInventories[random].Inventory[0] = new Item(head.Dequeue());
        }

        while (body.Count > 0)
        {
            int random = Main.rand.Next(displayDolls.Count);
            dollInventories[random].Inventory[1] = new Item(body.Dequeue());
        }

        while (legs.Count > 0)
        {
            int random = Main.rand.Next(displayDolls.Count);
            dollInventories[random].Inventory[2] = new Item(legs.Dequeue());
        }

        //Set set = Main.rand.Next(Sets);
        //Item[] inv = [new(set.Head), new(set.Body), new(set.Legs), new(), new(), new(), new(), new()];

        //teDollInventory.SetValue(mannequin, inv);
    }

    public override void OnStop()
    {
        for (int i = area.X; i < area.Right; ++i)
            for (int j = area.Y; j < area.Bottom; ++j)
                if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is TEDisplayDoll mannequin)
                    teDollInventory.SetValue(mannequin, new Item[] { new(), new(), new(), new(), new(), new(), new(), new() });
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (Sets.Any(x => x.PlayerMatches(plr)))
                return MinigameRanking.ByFirst(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (Sets.Any(x => x.PlayerMatches(plr)))
            {
                Beaten = true;
                return;
            }
        }
    }
}
