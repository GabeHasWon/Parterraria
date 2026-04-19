namespace Parterraria.Core.MinigameSystem;

internal class MinigameHooks : ILoadable
{
    public void Load(Mod mod) => On_Player.GetRespawnTime += ModifyDespawnTime;

    private int ModifyDespawnTime(On_Player.orig_GetRespawnTime orig, Player self, bool pvp)
    {
        int vanillaTime = orig(self, pvp);

        if (WorldMinigameSystem.InMinigame && ModContent.GetInstance<WorldMinigameSystem>().playingMinigame.RespawnTime is { } value and > -1)
            return value;

        return vanillaTime;
    }

    public void Unload() { }
}
