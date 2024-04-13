using Parterraria.Common;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem.MinigameUI;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem;

internal class WorldMinigameSystem : ModSystem
{
    public static WorldMinigameSystem Self => ModContent.GetInstance<WorldMinigameSystem>();
    public static bool InMinigame => Self.playingMinigame is not null;
    public static bool NotReady { get; private set; } = false;

    internal static readonly List<Minigame> worldMinigames = [];

    private static int _minigameOverTimer = 0;
    private static int _minigamePreviewTimer = 0;
    private static bool _selectingMinigame = false;

    public Minigame playingMinigame = null;

    public static bool TryAddMinigame(string name, Rectangle rectangle)
    {
        Minigame.MinigamesByModAndName[name].ValidateRectangle(ref rectangle);

        if (worldMinigames.Any(x => x.area.Intersects(rectangle)))
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Minigame.Intersecting"));
            return false;
        }

        var game = Minigame.MinigamesByModAndName[name].Clone();
        game.area = rectangle;
        worldMinigames.Add(game);
        return true;
    }

    internal static void DrawMinigames()
    {
        if (Main.LocalPlayer.HeldItem.ModItem is MinigameTool)
        {
            foreach (var item in worldMinigames)
                DebugDrawMinigames(item);
        }

        if (!InMinigame)
            return;

        Self.playingMinigame.Draw();

        if (_minigamePreviewTimer++ < 240)
        {
            Color alpha = Color.White;

            if (_minigamePreviewTimer > 120)
                alpha *= 1 - (_minigamePreviewTimer - 120f) / 120f;

            DrawCommon.CenteredString(FontAssets.DeathText.Value, Main.ScreenSize.ToVector2() / new Vector2(2f, 4f), Self.playingMinigame.DisplayName.Value, alpha);

            var descPos = Main.ScreenSize.ToVector2() / new Vector2(2f, 4f) + new Vector2(0, 40);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, descPos, Self.playingMinigame.Description.Value, alpha, new Vector2(0.5f));
        }
    }

    private static void DebugDrawMinigames(Minigame game)
    {
        var loc = game.area;
        loc.Location -= Main.screenPosition.ToPoint();

        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, loc, Color.White * 0.1f);
        DrawCommon.CenteredString(FontAssets.DeathText.Value, loc.Location.ToVector2() + new Vector2(loc.Width / 2, 20), game.DisplayName.Value, Color.White);
    }

    public override void PreUpdatePlayers()
    {
        if (InMinigame)
        {
            if (NotReady)
            {
                bool ready = true;

                for (int i = 0; i < Main.maxPlayers; ++i)
                {
                    Player plr = Main.player[i];

                    if (plr.active && !plr.GetModPlayer<PlayingBoardPlayer>().minigameReady)
                    {
                        ready = false;
                        break;
                    }
                }

                if (ready)
                {
                    NotReady = false;
                    playingMinigame.OnStart();
                }
            }
            else
                playingMinigame.Update();

            if (playingMinigame.Beaten && _minigameOverTimer++ > 240)
            {
                for (int i = 0; i < Main.maxPlayers; ++i)
                {
                    Player plr = Main.player[i];

                    if (plr.active)
                    {
                        if (plr.dead)
                            plr.Spawn(PlayerSpawnContext.ReviveFromDeath);

                        plr.Center = plr.GetModPlayer<PlayingBoardPlayer>().connectedNode.position;
                        plr.GetModPlayer<PlayingBoardPlayer>().hasGoneOnCurrentTurn = false;
                        playingMinigame.ResetPlayer(plr);
                    }
                }

                playingMinigame.OnStop();
                playingMinigame = null;
            }
        }

        if (InMinigame || worldMinigames.Count == 0)
            return;

        if (_selectingMinigame)
            return;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && (!plr.GetModPlayer<PlayingBoardPlayer>().hasGoneOnCurrentTurn || plr.GetModPlayer<PlayingBoardPlayer>().isMoving))
                return;
        }

        BoardUISystem.SetMiscUI(new MinigameSelectionUIState(StartMinigame));
        _selectingMinigame = true;
    }

    private void StartMinigame(string minigameName)
    {
        _minigamePreviewTimer = 0;
        _minigameOverTimer = 0;
        playingMinigame = Main.rand.Next(worldMinigames.Where(x => x.FullName == minigameName).ToArray()).Clone();
        playingMinigame.OnSet();
        NotReady = true;
        _selectingMinigame = false;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.active)
                continue;

            plr.Center = playingMinigame.area.Center();
            plr.GetModPlayer<PlayingBoardPlayer>().minigameReady = false;
            playingMinigame.SetupPlayer(plr);
        }

        BoardUISystem.CloseMiscUI();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag.Add("minigameCount", (short)worldMinigames.Count);

        for (int i = 0; i < worldMinigames.Count; i++)
        {
            Minigame item = worldMinigames[i];
            TagCompound game = [];
            item.SaveData(game);
            tag.Add("game" + i, game);
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        short count = tag.GetShort("minigameCount");

        for (int i = 0; i < count; ++i)
        {
            TagCompound game = tag.GetCompound("game" + i);
            worldMinigames.Add(Minigame.LoadMinigame(game));
        }
    }

    internal void StopParty()
    {
        playingMinigame = null;
        _selectingMinigame = false;
        NotReady = true;
    }
}
