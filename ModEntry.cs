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
        void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var toolPading = Game1.toolbarPaddingX;
            keyboardToggleButton = AddButton(SButton.None, "Keyboard", OnKeyDown_Keyboard);
            keyboardToggleButton.onKeyUp = OnKeyUp_Keyboard;
            ToggleKeys(false);
            ToggleKeyboardPage(false);

            AddButton(SButton.I, "I", OnKeyButtonDown);
            AddButton(SButton.P, "P", OnKeyButtonDown);


            ItemSpawnerPatch.InitOnGameLaunched();
        }
        void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
        }
        void OnRendered(object? sender, RenderedEventArgs e)
        {
            if (!Context.IsPlayerFree && Game1.activeClickableMenu != keyboardPage)
            {
                if (keyboardToggleButton.enabled)
                    SetKeyboardActive(false);
                return;
            }
            else
            {
                if (!keyboardToggleButton.enabled)
                    SetKeyboardActive(true);
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

        void OnKeyUp_Keyboard(KeyButton button)
        {
            if (!isShowKeys)
            {
                ToggleKeyboardPage(false);
            }
        }
        void OnKeyDown_Keyboard(KeyButton keyButton)
        {
            ToggleKeys(!isShowKeys);
            ToggleKeyboardPage(true);
        }
        void OnKeyButtonDown(KeyButton button)
        {
            //disable Active Clickable Menu first
            //mod CJB Item Spawner need IsPlayerFree == true;
            SetKeyboardActive(false);

            var input = Game1.input as SInputState;
            input.OverrideButton(button.key, true);
        }
        void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            //process when Button is Mouse
            if (e.Button != SButton.MouseLeft) return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keyButtons)
            {
                btn.receiveLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            }
        }
        void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft) return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keyButtons)
            {
                btn.releaseLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            }
        }


        IClickableMenu oldClickableMenu;
        public void SetKeyboardActive(bool enable = true)
        {
            //reset init
            ToggleKeys(false);
            ToggleKeyboardPage(false);
            keyboardToggleButton.enabled = enable;
        }
        //For block player to touch & tap
        public void ToggleKeyboardPage(bool toggle)
        {
            //enable
            if (toggle)
            {
                if (keyboardPage == null)
                {
                    keyboardPage = new KeyboardPage();
                    keyboardPage.behaviorBeforeCleanup = OnCloseKeyboardPage;
                    oldClickableMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = keyboardPage;
                }
                return;
            }

            //disable
            if (Game1.activeClickableMenu == keyboardPage)
            {
                keyboardPage = null;
                Game1.activeClickableMenu = oldClickableMenu;
            }

        }
        //set visable keys
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
        }
        KeyButton AddButton(SButton key, string label, Action<KeyButton> callback)
        {
            var keyButton = new KeyButton(key, label, callback);
            keyButtons.Add(keyButton);
            return keyButton;
        }
        void OnCloseKeyboardPage(IClickableMenu menu)
        {
            ToggleKeyboardPage(false);
        }
    }
}
