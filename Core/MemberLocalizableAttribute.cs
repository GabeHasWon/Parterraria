using System;

namespace Parterraria.Core;

#nullable enable

/// <summary>
/// Allows a field to modify their displayable name and hover description. Useful in <see cref="MinigameSystem.Minigame"/>s for customizing fields.<br/>
/// <paramref name="forceKey"/> can be used to change the key used for the name.<br/>
/// <paramref name="forceDescription"/> can be used to change the key used for the description, or set to "" to skip having a description.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class MemberLocalizableAttribute(string? forceKey = null, string? forceDescription = null) : Attribute
{
    public string? ForceKey = forceKey;
    public string? ForceDescription = forceDescription;
}
