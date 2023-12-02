using AltV.Net.Client;
using AltV.Net.Client.Elements.Interfaces;
using MarcusCZ.AltV.VTarget.Client.Options;
using MarcusCZ.AltV.VTarget.Client.Providers;

namespace MarcusCZ.AltV.VTarget.Client;

internal class VEye
{
    private bool _active;
    private bool _activeSync;
    private bool _eyeHit;
    private bool _threadActive = true;

    public readonly OptionRegistry OptionRegistry;
    private readonly FrontEnd _frontEnd;
    private readonly Thread _thread;
    
    private List<IVTargetOption> _currentOptions;
    private List<IVTargetOption> _newOptions;
    private List<IVTargetOption>? _syncOptions;
    
    private readonly List<IVProvider> _providers;
    private readonly List<IVAsyncProvider> _asyncProviders;
    private readonly Dictionary<Type, IVProvider> _types;
    private readonly Dictionary<IVProvider, bool> _taskDone;
    
    public VEye(OptionRegistry optionRegistry)
    {
        OptionRegistry = optionRegistry;
        _frontEnd = new FrontEnd(this);
        _currentOptions = new ();
        _newOptions = new();

        _providers = new ();
        _asyncProviders = new ();
        _types = new();
        _taskDone = new();

        var worldProvider = new WorldShapeTestProvider(optionRegistry);
        // var zoneProvider = new PolyZoneProvider();
        _providers.Add(worldProvider);
        _taskDone.Add(worldProvider, false);
        // _asyncProviders.Add(zoneProvider);

        _types.Add(typeof(VEntityOption), worldProvider);
        // _types.Add(typeof(VZoneOption), zoneProvider);
        
        _thread = new Thread(_runAsync);
        _thread.Start();
    }

    internal IWebView GetWebView() => _frontEnd.GetWebView();

    public void RegisterProvider<T>(IVProvider provider) where T : IVTargetOption
    {
        if (provider is IVAsyncProvider asyncProvider) _asyncProviders.Add(asyncProvider);
        else _providers.Add(provider);
        _types.Add(typeof(T), provider);
    }

    private async void _runAsync()
    {
        while (_threadActive)
        {
            try
            {
                if (!_active)
                {
                    await Task.Delay(100);
                    continue;
                }

                bool block = false;
                foreach (var provider in _asyncProviders)
                {
                    _newOptions.AddRange(await provider.GetOptionsAsync());
                    block = _newOptions.Any() && provider.BlockOthers;
                    if (block) break;
                }
                
                if (!block) _newOptions.AddRange(await _getOptionsFromOnTicks());

                // Alt.Log($"Got {_newOptions.Count} options");
                if (_active && _newOptions.Any() && !_eyeHit)
                {
                    _currentOptions.Clear();
                    _frontEnd.Hit();
                    _eyeHit = true;
                }
                else if (_active && !_newOptions.Any() && _eyeHit)
                {
                    _frontEnd.HitLeft();
                    _eyeHit = false;
                }

                var diff = _getOptionsDifference(_currentOptions, _newOptions, out bool anyDiff);
                if (anyDiff)
                {
                    _currentOptions = new List<IVTargetOption>(_newOptions);
                    _frontEnd.UpdateOptions(diff);
                }

                _newOptions = new();

                await Task.Delay(5);
            }
            catch (Exception e)
            {
                Alt.LogError(e.Message);
                Alt.LogError(e.StackTrace);
            }
        }
    }
    
    private void _runOnTick()
    {
        DisableControlsThisTick();
        
        if (!_activeSync) return;
        _providers.ForEach(provider =>
        {
            if (!_taskDone[provider] && provider.GetOptions(out List<IVTargetOption>? options))
            {
                _syncOptions!.AddRange(options);
                _taskDone[provider] = true;
            }
        });
    }

    private async Task<List<IVTargetOption>> _getOptionsFromOnTicks()
    {
        _syncOptions = new List<IVTargetOption>();
        foreach (var key in _taskDone.Keys)
        {
            _taskDone[key] = false;
        }

        _activeSync = true;
        int i = 0;
        while (_active && !_taskDone.Values.All(value => value))
        {
            i++;
            await Task.Delay(5);
            if (i > 100)
            {
                Alt.LogWarning("[VTarget] Max tries exceeded in _getSyncOptions()");
                break;
            }
        }

        _activeSync = false;
        return _syncOptions;
    }

    public void StartTargeting()
    {
        Alt.OnTick += _runOnTick;
        _active = true;
        _frontEnd.StartTargeting();
    }
    public void StopTargeting()
    {
        Alt.OnTick -= _runOnTick;
        _frontEnd.StopTargeting();
        _frontEnd.ClearOptions();
        // _currentOptions.Clear();
        _active = false;
        _eyeHit = false;
    }

    public bool IsActive() => _active;

    internal void Destroy()
    {
        Alt.OnTick -= _runOnTick;
        _frontEnd.Destroy();
        _threadActive = false;
        _thread.Interrupt();
    }

    internal IVProvider GetProvider(IVTargetOption option)
    {
        return _types[option.GetType()] ?? throw new ArgumentException($"Failed to get provider for {option.GetType()}");
    }
    
    private static Tuple<List<IVTargetOption>, List<string>> _getOptionsDifference(List<IVTargetOption> currentOptions, List<IVTargetOption> newOptions, out bool diff)
    {
        List<IVTargetOption> add = newOptions.Except(currentOptions).ToList();
        List<string> remove = currentOptions.Except(newOptions).Select(option => option.Id).ToList();
        diff = remove.Any() || add.Any();
        return new Tuple<List<IVTargetOption>, List<string>>(add, remove);
    }

    private static void DisableControlsThisTick()
    {
        Alt.Natives.DisableControlAction(0, 24, true);
        Alt.Natives.DisableControlAction(0, 69, true);
        Alt.Natives.DisableControlAction(0, 92, true);
        Alt.Natives.DisableControlAction(0, 106, true);
        Alt.Natives.DisableControlAction(0, 142, true);
        Alt.Natives.DisableControlAction(0, 144, true);
        Alt.Natives.DisableControlAction(0, 257, true);
            
        Alt.Natives.DisableControlAction(0, 25, true);
        Alt.Natives.DisableControlAction(0, 68, true);
        Alt.Natives.DisableControlAction(0, 70, true);
        Alt.Natives.DisableControlAction(0, 91, true);
            
        Alt.Natives.DisableControlAction(0, 14, true);
        Alt.Natives.DisableControlAction(0, 15, true);
        Alt.Natives.DisableControlAction(0, 16, true);
        Alt.Natives.DisableControlAction(0, 17, true);
        Alt.Natives.DisableControlAction(0, 99, true);
        Alt.Natives.DisableControlAction(0, 261, true);
        Alt.Natives.DisableControlAction(0, 262, true);
    }
}