using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace VirtualKeyboard
{
    internal class ItemSpawnerPatch
    {
        public ItemSpawnerPatch Instance { get; private set; }
        public static void InitOnGameLaunched()
        {
            new ItemSpawnerPatch();
        }
        public ItemSpawnerPatch()
        {
            Instance = this;

            var harmony = new Harmony(nameof(ItemSpawnerPatch));
            var centerWidth = Game1.clientBounds.Width;
            var centerHeight = Game1.clientBounds.Height;

            int x = (Game1.viewport.Width / 2) - (800 + IClickableMenu.borderWidth * 2) / 2;
            int y = (Game1.viewport.Height / 2) - (600 + IClickableMenu.borderWidth * 2) / 2;

            int width = 800 + IClickableMenu.borderWidth * 2;
            int heighti = 600 + IClickableMenu.borderWidth * 2;

            var CJBItemSpawner = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(asm => asm.GetName().Name.Contains("CJBItemSpawner"));
            //check if you have mods
            if (CJBItemSpawner == null)
                return;

            var ItemMenuWithInventoryType = CJBItemSpawner.GetType("CJBItemSpawner.Framework.ItemMenu");
            var ctor = ItemMenuWithInventoryType.GetConstructors()[0];

            harmony.Patch(ctor,
                prefix: new(GetType().GetMethod(nameof(PrefixCtor), BindingFlags.NonPublic | BindingFlags.Static)),
                postfix: new(GetType().GetMethod(nameof(PostfixCtor), BindingFlags.NonPublic | BindingFlags.Static)));
            AndroidLog.Log("Done patching CJBItemSpawner.");
        }

        static Rectangle oldClientBounds;
        static xTile.Dimensions.Rectangle oldViewport;
        static void PrefixCtor()
        {
            ModEntry.Instance.Monitor.Log("On Prefix ItemMenu");

            oldClientBounds = Game1.clientBounds;
            oldViewport = Game1.viewport;
            //info debug for device resolution: W.2400, H.1080 POCO F3
            Game1.viewport.Width = 1300;
            Game1.viewport.Height = 680;
            ModEntry.Instance.Monitor.Log("set temp viewport: height=" + Game1.viewport.Height);
            ModEntry.Instance.Monitor.Log("set temp viewport: width=" + Game1.viewport.Width);
        }
        static void PostfixCtor()
        {
            Game1.clientBounds = oldClientBounds;
            Game1.viewport = oldViewport;

            ModEntry.Instance.Monitor.Log("On PostfixCtor ItemMenuWithInventory");
        }
    }
}
