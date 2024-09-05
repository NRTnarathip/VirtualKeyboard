using Microsoft.Xna.Framework;

namespace VirtualKeyboard
{
    public class KeyboardConfig
    {
        public Vector2 Position { get; set; } = new Vector2(60, 20);
        public float Size { get; set; } = 90f;
        public float Opacity { get; internal set; } = .7f;
        public string[] Layout1 { get; set; } = ["I", "P"];
        public string[] Layout2 { get; set; } = ["Console:openconsole", $"Use CMD:{CommandMobile.CMD_opencommand}"];
        public string[] Layout3 { get; set; } = [];
    }
}
