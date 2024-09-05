using Microsoft.Xna.Framework.Input;

namespace VirtualKeyboard;

internal class CommandMobile
{
    static CommandMobile Instance;
    public static void Init()
    {
        Instance = new CommandMobile();
    }
    public const string CMD_opencommand = "opencommand";
    CommandMobile()
    {
        ModEntry.Instance.Helper.ConsoleCommands.Add(
            CMD_opencommand, "open command mobile by ImproveGame",
            (cmd, args) => Open()
        );

    }
    public void Open()
    {
        var MessageBoxTitle = "SMAPI Commmand";
        var MessageBoxDescription = "in this it's cmd for SMAPI";
        Task.Run(async () =>
        {
            var result = await KeyboardInput.Show(MessageBoxTitle, MessageBoxDescription, "", false);
            if (result != null)
            {
                var split = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 0)
                {
                    SendCMD(split[0], split.Length >= 2 ? split[1..] : []);
                }
                else
                {
                    SendCMD(result, []);
                }
            }
        });
    }
    void SendCMD(string cmd, string[] args)
    {
        ModEntry.Instance.Helper.ConsoleCommands.Trigger(cmd, args);
    }
}
