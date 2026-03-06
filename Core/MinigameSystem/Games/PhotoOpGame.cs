using Parterraria.Common;
using Parterraria.Common.CameraModifiers;
using Parterraria.Content.NPCs;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.InventoryStorageSystem;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Parterraria.Core.MinigameSystem.Games;

internal class PhotoOpGame : Minigame
{
    internal class PhotoOpPlayer : ModPlayer
    {
        public override void ModifyScreenPosition()
        {
            if (!WorldMinigameSystem.InMinigame || WorldMinigameSystem.Self.playingMinigame is not PhotoOpGame game)
                return;

            int secondsBetween = game.secondsBetweenPhotos * 60;
            int mod = game.PlayTime % secondsBetween;
            int slime = NPC.FindFirstNPC(ModContent.NPCType<SlimeOfTerraria>());

            if (slime > -1 && mod == secondsBetween - 3 * 60 && game.PlayTime < secondsBetween * game.totalPhotos)
            {
                NPC terrariaSlime = Main.npc[slime];
                Main.instance.CameraModifiers.Add(new ZoomTrackNPCModifier("TerrarianSlimeZoom", terrariaSlime, 2 * 60, 60, 2 * 60, 0.3f));
            }
        }
    }

    public override MinigameWinType WinType => MinigameWinType.First;

    public override int MaxPlayTime => secondsBetweenPhotos * totalPhotos * 60;

    private static Asset<Texture2D> Camera = null;

    [HideFromEdit]
    private readonly Dictionary<int, float> PlayerScoreByWhoAmI = [];

    int secondsBetweenPhotos = 6;
    int totalPhotos = 3;

    public override void Load() => Camera = ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Misc/CameraFlash");

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
            plr.GetModPlayer<InventoryPlayer>().SwitchInventory([], [ItemHelper.Air(), ItemHelper.Air(), ItemHelper.Air(), new Item(ItemID.EoCShield), new Item(ItemID.HermesBoots)], []);
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
        Rectangle cameraPos = new((int)terrariaSlime.position.X - 100, (int)terrariaSlime.position.Y - 60, 200, 120);
        Dictionary<int, float> players = [];

        foreach (Player player in Main.ActivePlayers)
        {
            if (!player.Hitbox.Intersects(cameraPos))
                continue;

            PlayerScoreByWhoAmI.TryAdd(player.whoAmI, 0);
            float dist = terrariaSlime.Distance(player.Center);
            PlayerScoreByWhoAmI[player.whoAmI] += dist;
            players.Add(player.whoAmI, dist);
        }

        var sort = players.OrderBy(x => x.Value);
        int placement = 0;

        foreach (var pair in sort)
        {
            Player player = Main.player[pair.Key];

            AdvancedPopupRequest pop = new()
            {
                Color = placement switch
                {
                    0 => Color.Gold,
                    1 => Color.Silver,
                    2 => Color.SaddleBrown,
                    _ => Color.White
                },
                DurationInFrames = 60,
                Text = player.name,
                Velocity = new Vector2(0, -18)
            };

            PopupText.NewText(pop, player.Top);
            placement++;
        }
    }

    protected override void InternalDraw(bool debug)
    {
        if (debug)
            return;

        int secondsBetween = secondsBetweenPhotos * 60;
        int mod = PlayTime % secondsBetween;
        int slime = NPC.FindFirstNPC(ModContent.NPCType<SlimeOfTerraria>());

        if (slime == -1)
            return;

        if (mod > secondsBetween - 20 && PlayTime < secondsBetween * totalPhotos)
            Main.spriteBatch.Draw(Camera.Value, Main.npc[slime].Center - Main.screenPosition, null, Color.White, 0f, Camera.Size() / 2f, 1f, SpriteEffects.None, 0);
        else if (PlayTime > secondsBetweenPhotos * 60 && mod < 20)
            Main.spriteBatch.Draw(Camera.Value, Main.npc[slime].Center - Main.screenPosition, null, Color.White * (1 - mod / 20f), 0f, Camera.Size() / 2f, 1f, SpriteEffects.None, 0);
    }
}
