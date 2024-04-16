namespace VirtualKeyboard
{
    public class KeyboardConfig
    {
        public int Size { get; set; } = 100;
        public float Opacity { get; internal set; } = .7f;
        public string[] Layout1 { get; set; } = new string[] { "I", "P" };
        public string[] Layout2 { get; set; } = new string[] { "Console:openconsole" };
        public string[] Layout3 { get; set; } = new string[] { };
    }
}
