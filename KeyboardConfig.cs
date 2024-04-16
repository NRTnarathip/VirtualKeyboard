using Microsoft.Xna.Framework;

namespace VirtualKeyboard
{
    public class KeyboardConfig
    {
        public Vector2 Position { get; set; } = new Vector2(60, 20);
        public float Size { get; set; } = 90f;
        public float Opacity { get; internal set; } = .7f;
        public string[] Layout1 { get; set; } = new string[] { "I", "P" };
        public string[] Layout2 { get; set; } = new string[] { "Console:openconsole" };
        public string[] Layout3 { get; set; } = new string[] { };
    }
}
