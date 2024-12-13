using System.Collections.Generic;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

internal abstract class Microevent : ILoadable
{
    public enum Quality : byte
    {
        Excellent,
        Great,
        Good,
        Neutral,
        Bad,
        Terrible,
        Abysmal
    }

    public static List<Microevent> Microevents = [];

    public virtual Quality EventQuality => Quality.Neutral;
    public abstract LocalizedText Text { get; }
    public virtual Color PopupColor => Color.LightGray;

    public void Load(Mod mod) => Microevents.Add(this);
    public void Unload() => Microevents.Remove(this);

    public void Invoke(Player player)
    {
        InternalInvoke(player);

        PopupText.NewText(new AdvancedPopupRequest()
        {
            Text = Text.Value,
            Color = PopupColor,
            DurationInFrames = 300,
            Velocity = new Vector2(0, -12)
        }, player.Center);
    }

    protected abstract void InternalInvoke(Player player);
}
