using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace VirtualKeyboard
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }
        List<KeyButton> keyButtons = new();
        bool isShowKeys = false;
        KeyButton keyboardToggleButton;
        bool isEnableKeyboard = false;
        KeyboardPage keyboardPage;
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            var toggleTexture = Helper.ModContent.Load<Texture2D>("assets/togglebutton.png");
            Helper.Events.Display.Rendered += OnRendered;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Input.ButtonReleased += OnButtonReleased;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }
        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var toolPading = Game1.toolbarPaddingX;
            keyboardToggleButton = AddButton("Keyboard", OnClick_Keyboard);
            ToggleKeys(false);

            AddButton("I", OnClick_I);
        }

        private void OnClick_I()
        {
            //disable Active Clickable Menu first
            //mod CJB Item Spawner need IsPlayerFree == true;
            EnableKeyboard(false);

            var input = Game1.input as SInputState;
            input.OverrideButton(SButton.I, true);
        }
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            //process when Button is Mouse
            if (e.Button != SButton.MouseLeft) return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keyButtons)
            {
                if (!btn.enabled) continue;

                var clicked = btn.bounds.Contains(screenPixels);
                if (!clicked) continue;

                btn.receiveLeftClick((int)screenPixels.X, (int)screenPixels.Y);
                break;
            }
        }
        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft) return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keyButtons)
            {
                if (!btn.enabled) continue;

                var clicked = btn.bounds.Contains(screenPixels);
                if (!clicked) continue;

                btn.releaseLeftClick((int)screenPixels.X, (int)screenPixels.Y);
                break;
            }
        }
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
        }
        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            if (!Context.IsPlayerFree && Game1.activeClickableMenu != keyboardPage)
            {
                if (isEnableKeyboard)
                    EnableKeyboard(false);
                return;
            }
            else
            {
                if (!isEnableKeyboard) EnableKeyboard(true);
            }

            const int buttonGap = 20;
            var startWidth = Toolbar.toolbarWidth;
            var lastPosX = startWidth;
            var lastPosY = 0;
            foreach (var button in keyButtons)
            {
                if (!button.enabled)
                    continue;

                button.bounds.X = lastPosX;
                button.bounds.Y = lastPosY;
                button.Draw(e.SpriteBatch);
                lastPosX += button.bounds.Width + buttonGap;

                if (button == keyboardToggleButton)
                {
                    lastPosY = keyboardToggleButton.bounds.Height + buttonGap;
                    lastPosX = startWidth;
                }
            }
        }
        private void OnClick_Keyboard()
        {
            ToggleKeys(!isShowKeys);
        }
        IClickableMenu oldClickableMenu;
        public void EnableKeyboard(bool enable = true)
        {
            isEnableKeyboard = enable;
            //reset init
            ToggleKeys(false);
            ToggleKeyboardPage(false);
            keyboardToggleButton.enabled = enable;
        }
        void ToggleKeyboardPage(bool toggle)
        {
            if (toggle)
            {
                keyboardPage = new KeyboardPage();
                keyboardPage.behaviorBeforeCleanup = OnCloseKeyboardPage;
                oldClickableMenu = Game1.activeClickableMenu;
                Game1.activeClickableMenu = keyboardPage;
            }
            else
            {
                if (Game1.activeClickableMenu == keyboardPage)
                {
                    keyboardPage = null;
                    Game1.activeClickableMenu = oldClickableMenu;
                }
            }

        }
        public void ToggleKeys(bool toggle)
        {
            this.isShowKeys = toggle;
            keyboardToggleButton.opacity = isShowKeys ? 1f : 0.3f;
            foreach (var button in keyButtons)
            {
                if (button != keyboardToggleButton)
                {
                    button.enabled = isShowKeys;
                }
            }

            ToggleKeyboardPage(toggle);
        }
        KeyButton AddButton(string label, Action callback)
        {
            var keyButton = new KeyButton(label, callback);
            keyButtons.Add(keyButton);
            return keyButton;
        }
        void OnCloseKeyboardPage(IClickableMenu menu)
        {
            ToggleKeyboardPage(false);
        }
    }
}
