using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace VirtualKeyboard
{
    public class EaseInOutFloat
    {
        float target;
        float start;
        float duration;
        float currentTime;
        public float Value { get; private set; }
        public EaseInOutFloat(float start, float target, float duration)
        {
            this.start = start;
            this.Value = start;
            this.target = target;
            this.duration = duration;
            this.currentTime = 0f;
        }
        public void SetTarget(float target)
        {
            start = Value;
            this.target = target;
            currentTime = 0;
        }

        public void Update(float deltaTime)
        {
            float normalizedTime = currentTime / duration;
            float easedTime = EaseInOutQuad(normalizedTime);
            Value = start + (target - start) * easedTime;

            currentTime += deltaTime;
            if (currentTime > duration)
                currentTime = duration;
        }
        float EaseInOutQuad(float t)
        {
            return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }

        internal void SetTarget(object value)
        {
            throw new NotImplementedException();
        }
    }
    internal class KeyButton : OptionsButton
    {
        public FieldInfo isHeldDownField;
        public bool isHeldDown => (bool)isHeldDownField.GetValue(this);
        public Color color = Color.White;

        public float opacity
        {
            get => opacityInOut != null ? opacityInOut.Value : _opacity;
            set
            {
                if (opacityInOut != null)
                    opacityInOut.SetTarget(value);
                else
                    this._opacity = value;
            }
        }
        float _opacity = 1;
        public EaseInOutFloat opacityInOut;

        public string key;
        public Action<KeyButton> onKeyDown;
        public Action<KeyButton> onKeyUp;
        public int buttonHorizonLayout = 0;//vertical layout
        private Texture2D icon;
        string iconPath;

        public KeyButton(string key, string label, Action<KeyButton> onKeyDown) : base(label, null, -1, -1)
        {
            this.key = key;
            this.onKeyDown = onKeyDown;
            isHeldDownField = typeof(OptionsButton).GetField("isHeldDown", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public void SetIcon(string iconPath)
        {
            this.iconPath = iconPath;
            this.icon = ModEntry.Instance.Helper.ModContent.Load<Texture2D>(iconPath);
            //for 16x16
            this.bounds.Width = 100;
            this.bounds.Height = 100;
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            if (enabled && bounds.Contains(x, y))
            {
                onKeyDown?.Invoke(this);
            }
        }
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            if (enabled & bounds.Contains(x, y))
            {
                onKeyUp?.Invoke(this);
            }
        }

        public void Draw(SpriteBatch batch, TimeSpan deltaTime)
        {
            if (opacityInOut != null)
            {
                var delta = (float)deltaTime.TotalMilliseconds / 1000f;
                opacityInOut.Update(delta);
            }

            if (string.IsNullOrEmpty(iconPath) == false)
            {
                var iconRect = bounds;
                iconRect.Y += isHeldDown ? 16 : 0;

                var color = Color.White * opacity;
                batch.Draw(icon, iconRect, color);
            }
            else
            {
                //get texture with tile pos x,z
                var drawShadow = !isHeldDown;
                var scale = 4; //adjust for default
                var srcTexture = Game1.mouseCursors;
                var srcRect = new Rectangle(256, 256, 10, 10);
                drawTextureBoxWithIconAndText(batch, Game1.dialogueFont,
                        srcTexture, srcRect,
                        null, srcRect,
                        label,
                        bounds.X - (isHeldDown ? 4 : 0),
                        bounds.Y + (isHeldDown ? 4 : 0),
                        button.bounds.Width,
                        button.bounds.Height,
                        Color.White * opacity,
                        scale, drawShadow,
                        iconLeft: true, isClickable: true,
                        heldDown: false, drawIcon: false,
                        reverseColors: false, bold: true);
            }
        }
    }
}
