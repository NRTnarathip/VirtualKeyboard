using Microsoft.Xna.Framework;
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
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Input.ButtonReleased += OnButtonReleased;
            Helper.Events.Input.CursorMoved += OnCursorMoved;
        }

        void OnGameUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (isDontRenderThisFrame() == false)
                refreshAllButtonPosition();
        }
        void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            config = this.Helper.ReadConfig<KeyboardConfig>();
            keysLookup = new();
            keys = new();

            toggleKeyboardButton = AddButton("", "Keyboard", OnKeyDown_ToggleKeyboard, 0);
            toggleKeyboardButton.onKeyUp = OnKeyUp_ToggleKeyboard;
            toggleKeyboardButton.SetIcon("assets/togglebutton.png");
            toggleKeyboardButton.SetSize((int)config.Size);
            toggleKeyboardButton.opacityInOut = new(0f, 0f, .1f);
            SetKeyboardPosition(config.Position);

            //init keys
            AddButtons(config.Layout1, 1);
            AddButtons(config.Layout2, 2);
            AddButtons(config.Layout3, 3);
            refreshAllButtonPosition();

            //set visable keys
            ToggleKeys(false);
            ToggleKeyboardPage(false);

            ItemSpawnerPatch.InitOnGameLaunched();
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
        bool isDontRenderThisFrame()
        {
            //render alway
            TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
            if (titleMenu != null)
            {
                //Improve Skip Load Screen
                if (titleMenu.birds.Count > 0)
                    titleMenu.skipToTitleButtons();

                //check if it's on customize character, so we dont need render
                if (TitleMenu.subMenu != null)
                    return true;
                return false;
            }

            //dont render when active menu & player not ready
            return !Context.IsPlayerFree && Game1.activeClickableMenu?.GetType() != typeof(KeyboardPage);
        }

        void refreshAllButtonPosition()
        {
            const int buttonGapX = 20;
            const int buttonGapY = 12;
            var startX = (int)config.Position.X;
            var startY = (int)config.Position.Y;
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
        //bool isShowKeysHold = false;
        DateTime _lastKeyDownToggleKeyboard;
        bool isMoveKeyboard = false;
        void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
            var now = DateTime.Now;
            var offset = now - _lastKeyDownToggleKeyboard;
            if (toggleKeyboardButton.isHeldDown && offset.TotalMilliseconds >= 300)
            {
                SetKeyboardPosition(e.NewPosition.GetScaledScreenPixels());
                isMoveKeyboard = true;
            }
        }
        bool isNeedToSaveConfig = false;
        //screen position
        void SetKeyboardPosition(Vector2 newScreenPos)
        {
            //protect keyboard overflow
            var posNormalize = new Vector2(newScreenPos.X / Game1.viewport.Width, newScreenPos.Y / Game1.viewport.Height);

            const int minPadding = 20;
            var uiScreenSize = Game1.uiViewport.Size;
            var newPos = new Vector2(uiScreenSize.Width * posNormalize.X, uiScreenSize.Height * posNormalize.Y);
            var uiScreenSizeMax = new Vector2(uiScreenSize.Width - minPadding, uiScreenSize.Height - minPadding);
            //make pos to center to move
            newPos.X -= toggleKeyboardButton.bounds.Width / 2f;
            newPos.Y -= toggleKeyboardButton.bounds.Height / 2f;

            //Left Top Pivot
            var iconSize = toggleKeyboardButton.bounds.Size;
            if (newPos.X < minPadding)
                newPos.X = minPadding;
            else if (newPos.X + iconSize.X > uiScreenSizeMax.X)
                newPos.X = uiScreenSizeMax.X - iconSize.X;

            if (newPos.Y < minPadding)
                newPos.Y = minPadding;
            else if (newPos.Y + iconSize.Y > uiScreenSizeMax.Y)
                newPos.Y = uiScreenSizeMax.Y - iconSize.Y;

            //Right Bottom Pivot
            config.Position = newPos;
            isNeedToSaveConfig = true;
        }
        void OnKeyUp_ToggleKeyboard(KeyButton button)
        {
            if (!isMoveKeyboard)
            {
                isShowKeys = !isShowKeys;
                ToggleKeys(isShowKeys);
                ToggleKeyboardPage(isShowKeys);
            }

            if (isNeedToSaveConfig)
            {
                isNeedToSaveConfig = false;
                this.Helper.WriteConfig<KeyboardConfig>(config);
                Console.WriteLine("Done write config.json");
            }
            isMoveKeyboard = false;
        }
        void OnKeyDown_ToggleKeyboard(KeyButton keyButton)
        {
            _lastKeyDownToggleKeyboard = DateTime.Now;
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
