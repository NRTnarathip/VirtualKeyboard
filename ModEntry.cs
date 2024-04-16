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
        KeyButton toggleKeyboardButton;
        KeyboardConfig config;
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            var toggleTexture = Helper.ModContent.Load<Texture2D>("assets/togglebutton.png");
            Helper.Events.Display.Rendered += OnRendered;
            Helper.Events.GameLoop.UpdateTicked += OnGameUpdateTicked;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Input.ButtonReleased += OnButtonReleased;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void OnGameUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (isDontRenderThisFrame() == false)
                refreshAllButtonPosition();
        }

        void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var toolPading = Game1.toolbarPaddingX;
            keysLookup = new();
            keys = new();

            toggleKeyboardButton = AddButton("", "Keyboard", OnKeyDown_ToggleKeyboard, 0);
            toggleKeyboardButton.onKeyUp = OnKeyUp_ToggleKeyboard;
            toggleKeyboardButton.SetIcon("assets/togglebutton.png");
            toggleKeyboardButton.opacityInOut = new(0f, 0f, .1f);

            //init keys
            config = this.Helper.ReadConfig<KeyboardConfig>();
            AddButtons(config.Layout1, 1);
            AddButtons(config.Layout2, 2);
            AddButtons(config.Layout3, 3);
            refreshAllButtonPosition();

            //set visable keys
            ToggleKeys(false);
            ToggleKeyboardPage(false);

            ItemSpawnerPatch.InitOnGameLaunched();
        }
        void refreshAllButtonPosition()
        {
            const int buttonGapX = 20;
            const int buttonGapY = 12;
            var startX = Toolbar.toolbarWidth;
            var startY = 20;
            var lastPosY = startY;
            foreach (var buttonLayoutPair in keysLookup)
            {
                var layoutHorizon = buttonLayoutPair.Key;
                var buttons = buttonLayoutPair.Value;
                var lastPosX = startX;
                foreach (var button in buttonLayoutPair.Value)
                {
                    button.bounds.X = lastPosX;
                    button.bounds.Y = lastPosY;

                    lastPosX += button.bounds.Width + buttonGapX;
                }
                lastPosY += buttons[0].bounds.Height + buttonGapY;
            }
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

        DateTime lastRender = DateTime.Now;
        void OnRendered(object? sender, RenderedEventArgs e)
        {
            var now = DateTime.Now;
            var deltaTime = now - lastRender;
            lastRender = now;

            if (isDontRenderThisFrame())
            {
                if (toggleKeyboardButton.enabled)
                    SetKeyboardActive(false);
                return;
            }
            else
            {
                if (!toggleKeyboardButton.enabled)
                    SetKeyboardActive(true);
            }


            foreach (var buttonLayoutPair in keysLookup)
            {
                var layoutHorizon = buttonLayoutPair.Key;
                var buttons = buttonLayoutPair.Value;
                foreach (var button in buttonLayoutPair.Value)
                {
                    if (!button.enabled)
                        continue;
                    //update
                    button.Draw(e.SpriteBatch, deltaTime);
                }
            }
        }

        void AddButtons(string[] keys, int layout)
        {
            foreach (var keyString in keys)
            {
                var data = keyString.Split(":");
                bool isCommand = data.Length > 1;
                if (isCommand)
                {
                    AddButton(data[1], data[0], OnKeyDown_Command, layout);
                }
                else
                {
                    AddButton(keyString, keyString, OnKeyDown, layout);
                }
            }
        }
        void OnKeyUp_ToggleKeyboard(KeyButton button)
        {
            if (isShowKeys && !isShowKeysHold)
            {
                isShowKeysHold = false;
                isShowKeys = false;
                ToggleKeys(isShowKeys);
                ToggleKeyboardPage(false);
            }
            isShowKeysHold = false;
        }
        bool isShowKeysHold = false;
        void OnKeyDown_ToggleKeyboard(KeyButton keyButton)
        {
            //open first frame click
            if (isShowKeys == false)
            {
                isShowKeysHold = true;
                isShowKeys = true;
                ToggleKeys(isShowKeys);
                ToggleKeyboardPage(true);
            }
        }
        void OnKeyDown(KeyButton button)
        {
            //disable Active Clickable Menu first
            //mod CJB Item Spawner need IsPlayerFree == true;
            SetKeyboardActive(false);
            var input = Game1.input as SInputState;
            var sbutton = (SButton)Enum.Parse(typeof(SButton), button.key);
            input.OverrideButton(sbutton, true);
        }
        void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            //process when Button is Mouse
            if (e.Button != SButton.MouseLeft)
                return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keys)
            {
                btn.receiveLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            }
        }
        void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft)
                return;

            var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            foreach (var btn in keys)
            {
                btn.releaseLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            }
        }
        void OnKeyDown_Command(KeyButton button)
        {
            SetKeyboardActive(false);
            SendCommand(button.key);
        }
        void SendCommand(string command)
        {
            SCore.Instance.RawCommandQueue.Add(command);
        }

        public void SetKeyboardActive(bool enable = true)
        {
            //reset init
            ToggleKeys(false);
            ToggleKeyboardPage(false);
            toggleKeyboardButton.enabled = enable;
        }
        //For block player to touch & tap
        public void ToggleKeyboardPage(bool toggle)
        {
            //enable
            toggleKeyboardButton.opacityInOut.SetTarget(toggle ? 1f : config.Opacity);

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

            foreach (var button in keys)
            {
                if (button != toggleKeyboardButton)
                {
                    button.enabled = isShowKeys;
                }
            }
        }
        KeyButton AddButton(string key, string label, Action<KeyButton> callback, int horizonLayout)
        {
            var keyButton = new KeyButton(key, label, callback);
            keyButton.enabled = false;
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
