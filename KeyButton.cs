using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace VirtualKeyboard
{
    internal class KeyButton : OptionsButton
    {
        Texture2D texture;
        public KeyButton(string label, Action callback, int x = -1, int y = -1) : base(label, callback, x, y)
        {

        }
        public void SetIcon(Texture2D texture)
        {
            this.texture = texture;
        }
    }
}
