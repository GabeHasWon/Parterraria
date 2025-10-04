using Parterraria.Content.NPCs;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.InventoryStorageSystem;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class BugChaseGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.Last;

    public override int MaxPlayTime => 0;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        bool modified = false;

        if (rectangle.Width < 40 * 16)
        {
            rectangle.Width = 40 * 16;
            modified = true;
        }

        if (rectangle.Height < 30 * 16)
        {
            rectangle.Height = 30 * 16;
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
                    new Item(ItemID.BugNet),
                    new Item(ItemID.DontHurtComboBook),
                    new Item(ItemID.LunarHook),
                    new Item(ItemID.TerrasparkBoots),
                    new Item(ItemID.AngelWings)
                ], false);
        }
    }

    public override void OnStart()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            var src = new EntitySource_Minigame(WorldBoardSystem.Self.playingBoard, WorldMinigameSystem.Self.playingMinigame);
            var area = WorldMinigameSystem.Self.playingMinigame.area;
            var pos = new Vector2(Main.rand.Next(area.Left, area.Right), Main.rand.Next(area.Top, area.Bottom));
            NPC.NewNPC(src, (int)pos.X, (int)pos.Y, ModContent.NPCType<GoldFaerie>());
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.HasItem(Mod.Find<ModItem>(nameof(GoldFaerie) + "Item").Type))
                return MinigameRanking.ByFirst(plr.whoAmI);
        }

        return null;
    }

    public override void InternalUpdate()
    {
        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.HasItem(Mod.Find<ModItem>(nameof(GoldFaerie) + "Item").Type))
            {
                Beaten = true;
                return;
            }
        }
    }
}
