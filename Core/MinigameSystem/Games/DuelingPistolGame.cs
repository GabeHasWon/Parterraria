using Parterraria.Common;
using Parterraria.Content.Items.MinigameItems;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

#nullable enable

internal class DuelingPistolGame : Minigame
{
    internal class DuelingPlayer : ModPlayer
    {
        public override void OnHurt(Player.HurtInfo info)
        {
            if (info.DamageSource.SourceProjectileLocalIndex != -1)
            {
                Projectile proj = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];

                if (proj.friendly && proj.TryGetOwner(out Player? owner) && proj.GetGlobalProjectile<DuelingPistol.DuelingPistolShot>().FromDueling)
                    Player.GetModPlayer<MinigameDisablePlayer>().Disable();
            }
        }
    }

    public override MinigamePlayType AvailablePlayType => MinigamePlayType.FreeForAll | MinigamePlayType.Team | MinigamePlayType.Duel;
    public override int MaxPlayTime => 0;
    public override bool PvPGame => true;

    [HideFromEdit]
    private int _waitTimerAfterEnd = 0;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        RectangleMinimumTiles(ref rectangle, 30, 30, out bool modified);
        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([new Item(ModContent.ItemType<DuelingPistol>()), new Item(ItemID.SilverBullet, 999)],
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.EoCShield), new Item(ItemID.HermesBoots), new Item(ItemID.CloudinaBalloon)],
                []);
        }
        else
            plr.GetModPlayer<MinigameDisablePlayer>().Enable(false, true);
    }

    public override void OnStart() => _waitTimerAfterEnd = 0;

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking() => MinigameRanking.ByRemaining(true, true);

    public override void InternalUpdate()
    {
        int livingCount = 0;

        foreach (Player player in Main.ActivePlayers)
        {
            if (!player.GetModPlayer<MinigameDisablePlayer>().Disabled)
                livingCount++;
        }

        // TODO: Team
        if (livingCount == 1)
        {
            _waitTimerAfterEnd++;

            if (_waitTimerAfterEnd > 60)
                Beaten = true;
        }
    }
}
