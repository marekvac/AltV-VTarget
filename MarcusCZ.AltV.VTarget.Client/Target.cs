using AltV.Net.Client;
using AltV.Net.Client.Elements.Data;
using MarcusCZ.AltV.VTarget.Client.Options;
using MarcusCZ.AltV.VTarget.Client.Providers;

namespace MarcusCZ.AltV.VTarget.Client;
public class Target
{
    private static VEye _eye;
    private static OptionRegistry _optionRegistry;
    public static bool Debug;

    public void OnStart()
    {
        _optionRegistry = new OptionRegistry();
        _eye = new VEye(_optionRegistry);

        Alt.OnKeyDown += OnKeyDown;
        Alt.OnKeyUp += OnKeyUp;

        Alt.OnConsoleCommand += (name, args) =>
        {
            if (!Alt.IsDebug || name != "vtarget" || args.Length == 0) return;
            
            if (args[0] == "debug") Debug = !Debug;
        };
    }

    public void OnStop()
    {
        Alt.OnKeyDown -= OnKeyDown;
        Alt.OnKeyUp -= OnKeyUp;
        _eye.Destroy();
        _optionRegistry.Destroy();
    }

    public static void RegisterProvider<T>(IVProvider provider) where T : IVTargetOption
    {
        _eye.RegisterProvider<T>(provider);
    }

    private void OnKeyUp(Key key)
    {
        if (key == Key.Menu && _eye.IsActive())
        {
            _eye.StopTargeting();
        }
    }

    private void OnKeyDown(Key key)
    {
        if (key == Key.Menu)
        {
            _eye.StartTargeting();
        }
    }

    public static void RegisterGlobalVehicle(VEntityOption option) => _optionRegistry.RegisterGlobalVehicle(option);
    public static void RegisterGlobalPlayer(VEntityOption option) => _optionRegistry.RegisterGlobalPlayer(option);
    public static void RegisterGlobalObject(uint model, VEntityOption option) => _optionRegistry.RegisterGlobalObject(model, option);
    public static void Register(VEntityOption option) => _optionRegistry.Register(option);
}