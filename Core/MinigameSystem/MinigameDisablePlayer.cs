namespace Parterraria.Core.MinigameSystem;

/// <summary>
/// Used solely to disable players in minigames they're considered "out" of without too much effort.
/// </summary>
internal class MinigameDisablePlayer : ModPlayer
{
    public bool Disabled { get; private set; }

    public void Disable()
    {
        Disabled = true;

        Player.hostile = false;
    }

    public void Enable() => Disabled = false;

    public override bool CanUseItem(Item item) => !Disabled;
}
