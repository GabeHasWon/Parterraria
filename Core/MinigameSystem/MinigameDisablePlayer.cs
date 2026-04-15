using Parterraria.Core.Synchronization;

namespace Parterraria.Core.MinigameSystem;

/// <summary>
/// Used solely to disable players in minigames they're considered "out" of without too much effort.
/// </summary>
internal class MinigameDisablePlayer : ModPlayer
{
    public bool Disabled { get; private set; }

    public void Disable(bool fromNet = false)
    {
        Disabled = true;
        Player.hostile = false;

        if (!fromNet && Main.myPlayer == Player.whoAmI)
            new SyncMinigameDisabledModule((byte)Player.whoAmI, false).Send();
    }

    public void Enable(bool fromNet = false, bool forceHostile = false)
    {
        Disabled = false;

        if (forceHostile)
            Player.hostile = true;

        if (!fromNet && Main.myPlayer == Player.whoAmI)
            new SyncMinigameDisabledModule((byte)Player.whoAmI, true).Send();
    }

    public override bool CanUseItem(Item item) => !Disabled;
}
