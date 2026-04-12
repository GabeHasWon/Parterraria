using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem.Games;

#nullable enable

internal class ObliterationGame : Minigame
{
    internal class ObliterationPlayer : ModPlayer
    {
        public int damageDealt = 0;

        public override void OnHurt(Player.HurtInfo info)
        {
            if (info.PvP)
            {
                if (info.DamageSource.SourcePlayerIndex != -1)
                {
                    Player other = Main.player[info.DamageSource.SourcePlayerIndex];
                    other.GetModPlayer<ObliterationPlayer>().damageDealt += info.Damage;
                }

                if (info.DamageSource.SourceProjectileLocalIndex != -1)
                {
                    Projectile proj = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];

                    if (proj.friendly && proj.TryGetOwner(out Player? owner))
                        owner.GetModPlayer<ObliterationPlayer>().damageDealt += info.Damage;
                }
            }
        }
    }

    public override MinigamePlayType AvailablePlayType => MinigamePlayType.FreeForAll | MinigamePlayType.Team;
    public override int MaxPlayTime => MinigameTimeInSeconds * 60;
    public override bool PvPGame => true;

    public int MinigameTimeInSeconds = 15;

    [HideFromEdit]
    private int _timer = 0;

    [HideFromEdit]
    private Dictionary<int, int> _leaderboardDisplay = null!;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        RectangleMinimumTiles(ref rectangle, 30, 30, out bool modified);
        return modified;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([new Item(ItemID.Musket), new Item(ItemID.BreakerBlade), new Item(ItemID.MusketBall, 999)], 
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.LuckyHorseshoe), new Item(ItemID.HermesBoots), new Item(ItemID.CloudinaBalloon)],
                [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.DualHook)]);
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        Dictionary<int, float> prio = [];
        HashSet<int> forcedLast = [];

        foreach (Player player in Main.ActivePlayers)
        {
            ObliterationPlayer plr = player.GetModPlayer<ObliterationPlayer>();
            prio.Add(player.whoAmI, plr.damageDealt);

            if (plr.damageDealt == 0)
                forcedLast.Add(player.whoAmI);
        }

        if (prio.Count == 0)
            return MinigameRanking.CompleteTie();

        return MinigameRanking.ByOrderAbsolute([.. prio.OrderBy(x => x.Value).Select(x => x.Key)], forcedLast);
    }

    public override void InternalUpdate()
    {
        _timer++;

        if (_timer > MaxPlayTime)
        {
            Beaten = true;
            return;
        }
    }

    protected override void InternalDrawUI()
    {
        if (WorldMinigameSystem.NotReady)
            return;

        if (PlayTime % (2 * 60) == 0)
        {
            _leaderboardDisplay ??= [];

            foreach (Player player in Main.ActivePlayers)
            {
                ObliterationPlayer plr = player.GetModPlayer<ObliterationPlayer>();

                if (!_leaderboardDisplay.TryAdd(player.whoAmI, plr.damageDealt))
                    _leaderboardDisplay[player.whoAmI] = plr.damageDealt;
            }
        }

        DrawCommon.DrawLeaderboard(_leaderboardDisplay);
    }

    protected override void InternalSave(TagCompound tag) => tag.Add("maxTime", MinigameTimeInSeconds);
    public override void LoadData(TagCompound tag) => MinigameTimeInSeconds = tag.GetInt("maxTime");
    public override void WriteNetData(BinaryWriter writer) => writer.Write((byte)MinigameTimeInSeconds);
    public override void ReadNetData(BinaryReader reader) => MinigameTimeInSeconds = reader.ReadByte();
}
