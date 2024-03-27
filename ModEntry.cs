using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace VirtualKeyboard
{
    public class ModEntry : Mod
    {
        List<KeyButton> keyButtons = new();
        bool isToggle = false;
        public override void Entry(IModHelper helper)
        {
            var toggleTexture = Helper.ModContent.Load<Texture2D>("assets/togglebutton.png");
            Helper.Events.Display.Rendered += OnRendered;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Input.ButtonReleased += OnButtonReleased;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keyButtons)
            {
                var clicked = btn.bounds.Contains(screenPixels);
                if (!clicked) continue;
                btn.releaseLeftClick((int)screenPixels.X, (int)screenPixels.Y);
                break;
            }
        }

        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var toolPading = Game1.toolbarPaddingX;
            AddButton("Keyboard", OnClick_Keyboard);
            AddButton("P", OnClick_P);
            AddButton("X", OnClick_X);
        }

        private void OnClick_X()
        {
        }

        private void OnClick_P()
        {
        }

        private void OnClick_Keyboard()
        {
            isToggle = !isToggle;
        }
        KeyButton AddButton(string label, Action callback, Vector2 pos)
        {
            var keyButton = new KeyButton(label, callback, (int)pos.X, (int)pos.Y);
            keyButtons.Add(keyButton);
            return keyButton;
        }
        KeyButton AddButton(string label, Action callback) => AddButton(label, callback, new Vector2(-1, -1));

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keyButtons)
            {
                var clicked = btn.bounds.Contains(screenPixels);
                if (!clicked) continue;

                btn.receiveLeftClick((int)screenPixels.X, (int)screenPixels.Y);
                break;
            }
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
        }

        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            var lastPosX = Toolbar.toolbarWidth;
            var buttonGap = 20;
            foreach (var button in keyButtons)
            {
                if (isToggle == false && button.label != "Keyboard")
                {
                    continue;
                }

                //final render
                //adjust pos
                button.bounds.X = lastPosX;
                button.draw(e.SpriteBatch, 0, 0);
                //next
                lastPosX += button.bounds.Width + buttonGap;
            }
        }
    }
}
