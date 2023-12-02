using System.Reflection;
using AltV.Net.Client;
using AltV.Net.Client.Async;
using AltV.Net.Client.Elements.Data;
using MarcusCZ.AltV.VTarget.Client.Data;
using MarcusCZ.AltV.VTarget.Client.Options;
using MarcusCZ.AltV.VTarget.Client.Providers;

namespace MarcusCZ.AltV.VTarget.Client;
public class Target : AsyncResource
{
    private static VEye _eye;
    private static OptionRegistry _optionRegistry;
    private MethodInfo _func;
    public static bool Debug;

    public override void OnStart()
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
        
        _registerTestOptions();
        Alt.Log("[VTarget] Started");
    }

    public override void OnStop()
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

    private void _registerTestOptions()
    {
        VEntityOption o1 = new("fas fa-times", "Door");
        o1.Background = Background.SUCCESS;
        VEntityOption o2 = new("fas fa-clock", "Option2");
        o2.CanInteract = (_, _, _) => false;
        o2.OnDisabledClick = (_, _, _, alert) => { alert(Background.DANGER, "This option is disabled");
            return false;
        };
        VEntityOption c1 = new("fas fa-check", "children1");
        c1.OnClick = (_, _, _, _) =>
        {
            Alt.Log("cliiick");
            return false;
        };
        VEntityOption c2 = new("fas fa-check", "children2");
        VEntityOption cc1 = new("fas fa-clock", "children21");
        VEntityOption cc2 = new("fas fa-clock", "children22");
        cc2.OnClick = (_, _, _, _) =>
        {
            Alt.Log("C2 cklicke");
            return false;
        };
        o1.Bones = new List<string> {"door_dside_f"};
        o1.Children = new List<IVTargetOption> {c1, c2};
        c2.Children = new List<IVTargetOption> {cc1, cc2};
        
        _eye.OptionRegistry.RegisterGlobalVehicle(o1);
        _eye.OptionRegistry.RegisterGlobalVehicle(o2);

        VEntityOption oo = new("fas fa-cube", "Object option");
        _eye.OptionRegistry.RegisterGlobalObject(1363150739, oo);
    }

    public static void RegisterGlobalVehicle(VEntityOption option) => _optionRegistry.RegisterGlobalVehicle(option);
    public static void RegisterGlobalPlayer(VEntityOption option) => _optionRegistry.RegisterGlobalPlayer(option);
    public static void RegisterGlobalObject(uint model, VEntityOption option) => _optionRegistry.RegisterGlobalObject(model, option);
    public static void Register(VEntityOption option) => _optionRegistry.Register(option);
}