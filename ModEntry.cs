using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Input;
using StardewModdingAPI.Mobile;
using StardewValley;
using StardewValley.Menus;

namespace VirtualKeyboard;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; }
    Dictionary<int, List<KeyButton>> keysLookup = new();
    List<KeyButton> keys = new();

    bool isShowKeyboard = false;
    KeyButton floatingKeyboard;
    KeyboardConfig config;
    public override void Entry(IModHelper helper)
    {
        Instance = this;


        Helper.Events.GameLoop.UpdateTicked += OnGameUpdateTicked;
    }

    void OnGameUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        //check if game loaded asset
        if (Game1.dialogueFont == null)
            return;

        if (floatingKeyboard == null)
            InitKeyboard();

        if (isShouldRenderThisFrame())
        {
            refreshAllButtonPosition();
        }

        TryRestoreBlockInputThisFrame();
    }
    void InitKeyboard()
    {
        if (floatingKeyboard != null)
            return;

        //setup
        config = this.Helper.ReadConfig<KeyboardConfig>();
        floatingKeyboard = AddButton("", "Keyboard", OnFloatingKeyboard_KeyDown, 0);
        floatingKeyboard.onKeyUp = OnFloatingKeyboard_KeyUp;
        floatingKeyboard.SetIcon("assets/togglebutton.png");
        floatingKeyboard.SetSize((int)config.Size);
        floatingKeyboard.opacityInOut = new(0f, 0f, .1f);
        SetKeyboardPosition(config.Position);

        //init keys
        AddButtons(config.Layout1, 1);
        AddButtons(config.Layout2, 2);
        AddButtons(config.Layout3, 3);
        refreshAllButtonPosition();

        //set visable keys
        ToggleKeys(false);
        ToggleKeyboardPage(false);


        CommandMobile.Init();
        Helper.Events.Display.Rendered += OnRendered;
        Helper.Events.Display.RenderedHud += Display_RenderedHud; ;
        Helper.Events.Input.ButtonPressed += OnGame_ButtonPressed;
        Helper.Events.Input.ButtonReleased += OnGame_ButtonReleased;
        Helper.Events.Input.CursorMoved += OnCursorMoved;
        Console.WriteLine("Done init keyboard");
    }

    private void Display_RenderedHud(object? sender, RenderedHudEventArgs e)
    {
        RenderKeyboard(e.SpriteBatch);
    }

    DateTime lastRender = DateTime.Now;

    void TryRestoreBlockInputThisFrame()
    {
        if (isBlockPlayerInputThisFrame)
        {
            blockFrameCountDown--;
            if (blockFrameCountDown <= 0)
            {
                isBlockPlayerInputThisFrame = false;
                SetBlockPlayerInput(false);
            }
        }
    }
    void OnRendered(object? sender, RenderedEventArgs e)
    {
    }

    void RenderKeyboard(SpriteBatch b)
    {
        try
        {
            var now = DateTime.Now;
            var deltaTime = now - lastRender;
            lastRender = now;

            if (isDontRenderThisFrame())
            {
                if (floatingKeyboard.enabled)
                    SetKeyboardActive(false);
            }
            else
            {
                if (!floatingKeyboard.enabled)
                    SetKeyboardActive(true);

                foreach (var buttonLayoutPair in keysLookup)
                {
                    var layoutHorizon = buttonLayoutPair.Key;
                    var buttons = buttonLayoutPair.Value;
                    foreach (var button in buttons)
                    {
                        if (!button.enabled)
                            continue;
                        //update
                        button.Draw(b, deltaTime);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
    bool isShouldRenderThisFrame() => !isDontRenderThisFrame();

    void refreshAllButtonPosition()
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
                AddButton(data[1], data[0], OnKeyButtonDown_Command, layout);
            }
            else
            {
                AddButton(keyString, keyString, OnKeyButtonDown, layout);
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
        const int startDragTime = 150;
        if (floatingKeyboard.isHeldDown && offset.TotalMilliseconds >= startDragTime)
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
        newPos.X -= floatingKeyboard.bounds.Width / 2f;
        newPos.Y -= floatingKeyboard.bounds.Height / 2f;

        //Left Top Pivot
        var iconSize = floatingKeyboard.bounds.Size;
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
    void OnFloatingKeyboard_KeyUp(KeyButton button)
    {
        if (!isMoveKeyboard)
        {
            isShowKeyboard = !isShowKeyboard;
            ToggleKeys(isShowKeyboard);
            ToggleKeyboardPage(isShowKeyboard);
        }

        if (isNeedToSaveConfig)
        {
            isNeedToSaveConfig = false;
            Helper.WriteConfig(config);
        }
        isMoveKeyboard = false;
        if (!isBlockPlayerInputThisFrame)
            SetBlockPlayerInput(false);
    }
    void OnFloatingKeyboard_KeyDown(KeyButton keyButton)
    {
        _lastKeyDownToggleKeyboard = DateTime.Now;
        SetBlockPlayerInput(true);
    }
    void OnKeyButtonDown(KeyButton button)
    {
        //disable Active Clickable Menu first
        //mod CJB Item Spawner need IsPlayerFree == true;
        SetBlockPlayerInputThisFrame();
        SetKeyboardActive(false);

        var sButton = (SButton)Enum.Parse(typeof(SButton), button.key);
        MobileInputTool.OverrideButton(sButton, true);
    }
    void OnKeyButtonDown_Command(KeyButton button)
    {
        SetBlockPlayerInputThisFrame();
        SetKeyboardActive(false);

        SendCommand(button.key);
    }
    void OnGame_ButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        //process when Button is Mouse
        if (e.Button != SButton.MouseLeft)
            return;

        var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
        bool isAnyClick = false;
        foreach (var btn in keys)
        {
            btn.receiveLeftClick((int)screenPixels.X, (int)screenPixels.Y);
            if (btn.isHeldDown)
                isAnyClick = true;
        }

        //close keyboard when click any background
        if (!isAnyClick & isShowKeyboard)
        {
            SetKeyboardActive(false);
            SetBlockPlayerInputThisFrame();
        }
    }
    void OnGame_ButtonReleased(object? sender, ButtonReleasedEventArgs e)
    {
        if (e.Button != SButton.MouseLeft)
            return;

        var screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
        foreach (var btn in keys)
        {
            btn.releaseLeftClick((int)screenPixels.X, (int)screenPixels.Y);
        }
    }
    void SendCommand(string command)
    {
        MobileConsoleTool.WriteLine(command);
    }

    public void SetKeyboardActive(bool enable = true)
    {
        //reset init
        ToggleKeys(false);
        ToggleKeyboardPage(false);
        floatingKeyboard.enabled = enable;
    }
    //For block player to touch & tap
    public void ToggleKeyboardPage(bool toggle)
    {
        //enable
        floatingKeyboard.opacityInOut.SetTarget(toggle ? 1f : config.Opacity);

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
        if (IsCurrentGameMenuKeyboardPage)
        {
            Game1.activeClickableMenu = null;
            if (!isBlockPlayerInputThisFrame)
                SetBlockPlayerInput(false);
        }
    }
    public bool IsCurrentGameMenuKeyboardPage => Game1.activeClickableMenu?.GetType() == typeof(KeyboardPage);
    //set visable keys
    public void ToggleKeys(bool toggle)
    {
        this.isShowKeyboard = toggle;

        foreach (var button in keys)
        {
            if (button != floatingKeyboard)
            {
                button.enabled = isShowKeyboard;
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

    static void SetBlockPlayerInput(bool isBlock)
    {
        Game1.freezeControls = isBlock;
    }
    static bool isBlockPlayerInputThisFrame = false;
    static int blockFrameCountDown;
    static void SetBlockPlayerInputThisFrame()
    {
        blockFrameCountDown = 10;
        isBlockPlayerInputThisFrame = true;
        SetBlockPlayerInput(true);
    }
}
