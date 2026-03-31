using System.Collections.Generic;
using System.IO;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

#nullable enable

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
    public virtual bool UseDefaultPopup => true;

    public void Load(Mod mod) => Microevents.Add(this);
    public void Unload() => Microevents.Remove(this);

    /// <summary>
    /// Run when the microevent is activated. <paramref name="reader"/> is null unless there is data to recieve, at which point data should be read.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="reader"></param>
    public void Invoke(Player player, BinaryReader? reader)
    {
        InternalInvoke(player, reader);

        if (UseDefaultPopup)
            SpawnPopupText(player.Center, Text.Value, PopupColor);
    }

    protected static void SpawnPopupText(Vector2 position, string text, Color color) => PopupText.NewText(new AdvancedPopupRequest()
    {
        Text = text,
        Color = color,
        DurationInFrames = 300,
        Velocity = new Vector2(0, -12)
    }, position);

    protected abstract void InternalInvoke(Player player, BinaryReader? reader);

    public virtual void NetSend(BinaryWriter writer) { }
}
