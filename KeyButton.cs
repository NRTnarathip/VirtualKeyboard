using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace VirtualKeyboard
{
    internal class KeyButton : OptionsButton
    {
        public FieldInfo isHeldDownField;
        public bool isHeldDown => (bool)isHeldDownField.GetValue(this);
        public Color color = Color.White;
        public float opacity = 1f;
        public KeyButton(string label, Action callback, int x = -1, int y = -1) : base(label, callback, x, y)
        {
            isHeldDownField = typeof(OptionsButton).GetField("isHeldDown", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public override void receiveLeftClick(int x, int y)
        {
            if (!enabled) return;

            base.receiveLeftClick(x, y);
            ModEntry.Instance.Monitor.Log($"on key down KeyButton: {label}", StardewModdingAPI.LogLevel.Debug);
        }
        public override void releaseLeftClick(int x, int y)
        {
            if (!enabled) return;
            base.releaseLeftClick(x, y);
            ModEntry.Instance.Monitor.Log($"on key up KeyButton: {label}", StardewModdingAPI.LogLevel.Debug);
        }
        public void Draw(SpriteBatch batch)
        {
            var srcRect = new Rectangle(256, 256, 10, 10);
            var iconSrcRect = new Rectangle(-1, -1, -1, -1);
            var drawShadow = !isHeldDown;
            var srcTexture = Game1.mouseCursors;
            //var iconTexture = ModEntry.Instance.Helper.Content.Load<Texture2D>("assets/togglebutton.png");
            Texture2D iconTexture = null;
            var scale = 4; //adjust for default

            drawTextureBoxWithIconAndText(batch, Game1.dialogueFont,
                    srcTexture, srcRect,
                    iconTexture, iconSrcRect, label,
                    bounds.X - (isHeldDown ? 4 : 0),
                    bounds.Y + (isHeldDown ? 4 : 0),
                    button.bounds.Width,
                    button.bounds.Height,
                    enabled ? color * opacity : Color.Gray * opacity,
                    scale, drawShadow,
                    iconLeft: true, isClickable: true,
                    heldDown: false, drawIcon: true,
                    reverseColors: false, bold: true);
        }
    }
}
