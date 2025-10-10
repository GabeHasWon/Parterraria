using Parterraria.Content.NPCs;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class PhotoOpGame : Minigame
{
    public override MinigameWinType WinType => MinigameWinType.First;

    public override int MaxPlayTime => secondsBetweenPhotos * totalPhotos * 60;

    [HideFromEdit]
    private readonly Dictionary<int, float> PlayerScoreByWhoAmI = [];

    int secondsBetweenPhotos = 4;
    int totalPhotos = 3;

    public override bool ValidateRectangle(ref Rectangle rectangle)
    {
        Rectangle original = rectangle;

        if (rectangle.Width < 40 * 16)
            rectangle.Width = 40 * 16;

        if (rectangle.Height < 20 * 16)
            rectangle.Height = 30 * 16;

        return original != rectangle;
    }

    public override void OnPlace()
    {
        secondsBetweenPhotos = 4;
        totalPhotos = 3;
    }

    public override void SetupPlayer(Player plr, bool playing)
    {
        if (!playing)
        {
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([], [new Item(0), new Item(0), new Item(0), new Item(ItemID.EoCShield), new Item(ItemID.HermesBoots)], []);
        }
    }

    public override void OnStart()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            var src = new EntitySource_Minigame(WorldBoardSystem.Self.playingBoard, WorldMinigameSystem.Self.playingMinigame);
            var area = WorldMinigameSystem.Self.playingMinigame.area;
            var pos = new Vector2(area.Center.X, area.Top + 20);
            NPC.NewNPC(src, (int)pos.X, (int)pos.Y, ModContent.NPCType<SlimeOfTerraria>());
        }
    }

    public override void ResetPlayer(Player plr) => plr.GetModPlayer<InventoryPlayer>().ReplaceInventory();

    public override MinigameRanking GetRanking()
    {
        var sorted = PlayerScoreByWhoAmI.OrderBy(x => x.Value).Select(x => x.Key);
        return MinigameRanking.ByOrderAbsolute([.. sorted]);
    }

    public override void InternalUpdate()
    {
        if (PlayTime % (secondsBetweenPhotos * 60) == 0)
            UpdatePlayerScores();

        if (PlayTime >= MaxPlayTime)
            Beaten = true;
    }

    private void UpdatePlayerScores()
    {
        NPC terrariaSlime = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SlimeOfTerraria>())];

        foreach (Player player in Main.ActivePlayers)
        {
            PlayerScoreByWhoAmI.TryAdd(player.whoAmI, 0);
            PlayerScoreByWhoAmI[player.whoAmI] += terrariaSlime.Distance(player.Center);
        }
    }
}
