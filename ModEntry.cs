using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace VirtualKeyboard
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }
        Dictionary<int, List<KeyButton>> keysLookup;
        List<KeyButton> keys;

        bool isShowKeys = false;
        KeyButton keyboardToggleButton;
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
            keysLookup = new();
            keys = new();

            keyboardToggleButton = AddButton(SButton.None, "Keyboard", OnKeyDown_Keyboard);
            keyboardToggleButton.onKeyUp = OnKeyUp_Keyboard;

            //init keys
            AddButton(SButton.I, "I", OnKeyButtonDown, 1);
            AddButton(SButton.P, "P", OnKeyButtonDown, 1);
            AddButton(SButton.None, "Console", OnKeyDown_Console, 2);
            AddButton(SButton.None, "CMD", OnKeyDown_CMD, 2);

            //set visable keys
            ToggleKeys(false);
            ToggleKeyboardPage(false);

            ItemSpawnerPatch.InitOnGameLaunched();
        }

        private void OnKeyDown_CMD(KeyButton button)
        {
        }

        private void OnKeyDown_Console(KeyButton button)
        {
            SetKeyboardActive(false);
            SendCommand("openconsole");
        }
        void SendCommand(string command)
        {
            SCore.Instance.RawCommandQueue.Add(command);
        }

        void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
        }
        bool isDontRenderThisFrame()
        {
            //render alway
            TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
            if (titleMenu != null)
            {
                //Improve Skip Load Screen
                if (titleMenu.birds.Count > 0)
                {
                    titleMenu.skipToTitleButtons();
                }

                //check if it's on customize character, so we dont need render
                if (TitleMenu.subMenu != null)
                    return true;
                return false;
            }

            //dont render when active menu & player not ready
            bool dontRender = !Context.IsPlayerFree && Game1.activeClickableMenu?.GetType() != typeof(KeyboardPage);
            return dontRender;
        }
        void OnRendered(object? sender, RenderedEventArgs e)
        {
            if (isDontRenderThisFrame())
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

            const int buttonGapX = 20;
            const int buttonGapY = 12;
            var startX = Toolbar.toolbarWidth;
            var startY = 20;
            int lastPosY = startY;
            foreach (var buttonLayoutPair in keysLookup)
            {
                var layoutHorizon = buttonLayoutPair.Key;
                var buttons = buttonLayoutPair.Value;
                var lastPosX = startX;
                foreach (var button in buttonLayoutPair.Value)
                {
                    if (!button.enabled)
                        continue;

                    button.bounds.X = lastPosX;
                    button.bounds.Y = lastPosY;
                    button.Draw(e.SpriteBatch);
                    lastPosX += button.bounds.Width + buttonGapX;
                }
                lastPosY += buttons[0].bounds.Height + buttonGapY;
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
            foreach (var btn in keys)
            {
                btn.receiveLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            }
        }
        void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft) return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keys)
            {
                btn.releaseLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            }
        }

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
                if (Game1.activeClickableMenu == null)
                {
                    var keyboardPage = new KeyboardPage();
                    keyboardPage.behaviorBeforeCleanup = OnCloseKeyboardPage;
                    Game1.activeClickableMenu = keyboardPage;
                }
                return;
            }

            //disable
            if (Game1.activeClickableMenu?.GetType() == typeof(KeyboardPage))
            {
                Game1.activeClickableMenu = null;
            }

        }
        //set visable keys
        public void ToggleKeys(bool toggle)
        {
            this.isShowKeys = toggle;
            keyboardToggleButton.opacity = isShowKeys ? 1f : 0.3f;
            foreach (var button in keys)
            {
                if (button != keyboardToggleButton)
                {
                    button.enabled = isShowKeys;
                }
            }
        }
        KeyButton AddButton(SButton key, string label, Action<KeyButton> callback, int horizonLayout = 0)
        {
            var keyButton = new KeyButton(key, label, callback);
            keyButton.buttonHorizonLayout = horizonLayout;

            if (!keysLookup.ContainsKey(horizonLayout))
                keysLookup.Add(horizonLayout, new());
            keysLookup[horizonLayout].Add(keyButton);
            keys.Add(keyButton);

            return keyButton;
        }
        void OnCloseKeyboardPage(IClickableMenu menu)
        {
            ToggleKeyboardPage(false);
        }
    }
}
