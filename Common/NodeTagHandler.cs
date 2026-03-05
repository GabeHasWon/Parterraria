using Parterraria.Core.BoardSystem;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Parterraria.Common;

#nullable enable

public class NodeTagHandler : ITagHandler
{
    private class Loader : ILoadable
    {
        public void Load(Mod mod) => ChatManager.Register<NodeTagHandler>("nodeicon");

        public void Unload()
        {
        }
    }

    private static readonly Dictionary<string, Asset<Texture2D>> TextureLookup = [];

    public TextSnippet Parse(string text, Color baseColor, string options)
    {
        if (!TextureLookup.TryGetValue(text, out Asset<Texture2D>? asset))
        {
            if (!text.Contains(' ') && NodeLoader.TryGet(text, out BoardNode node))
            {
                TextureLookup.Add(text, node.Texture);
                asset = node.Texture;
            }
            else
            {
                List<BoardNode> nodes = ModContent.GetInstance<NodeLoader>().Nodes;
                text = text.ToLower();

                foreach (BoardNode check in nodes)
                {
                    if (check.DisplayName.Value == text)
                    {
                        TextureLookup.Add(text, check.Texture);
                        break;
                    }
                }

                if (TextureLookup.TryGetValue("unknown", out Asset<Texture2D>? unknownTex))
                    asset = unknownTex;
                else
                {
                    Asset<Texture2D> tex = ModContent.Request<Texture2D>("Parterraria/Assets/Textures/Nodes/Unknown_Icon");
                    TextureLookup.Add("unknown", tex);
                    asset = tex;
                }
            }
        }

        return new NodeIconSnippet(asset) { Color = baseColor, Text = "Node" };
    }

    private sealed class NodeIconSnippet : TextSnippet
    {
        private readonly Asset<Texture2D> _tex;

        public NodeIconSnippet(Asset<Texture2D> tex)
        {
            _tex = tex;

            Text = "";
            CheckForHover = true;
        }

        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f)
        {
            Texture2D tex = _tex.Value;
            float padX = 2f;

            // Height at least one chat line so it sits nicely on the baseline
            float lineHeight = FontAssets.MouseText.Value.LineSpacing * scale;
            float texW = tex.Width;
            float texH = tex.Height;
            float allocW = texW + padX;
            float allocH = Math.Max(texH, lineHeight);

            size = new Vector2(allocW, allocH);

            if (justCheckingString)
            {
                return true;
            }

            if (color.R == 0 && color.G == 0 && color.B == 0)
            {
                return true;
            }

            Vector2 drawPos = position + new Vector2(0f, (allocH - texH) * 0.5f);
            drawPos.Floor();

            spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            return true;
        }

        // Not used when UniqueDraw returns true, but good to keep consistent with width
        public override float GetStringLength(DynamicSpriteFont _) => _tex.Value.Width + 2f;

        public override void OnHover()
        {
            if (Text != string.Empty)
            {
                Main.instance.MouseText(Text);
            }
        }
    }
}