using AltV.Net.Client;
using AltV.Net.Shared.Elements.Entities;
using MarcusCZ.AltV.VTarget.Client.Options;

namespace MarcusCZ.AltV.VTarget.Client;

public class OptionRegistry
{
    private readonly Dictionary<string, VEntityOption> _vehicles;
    private readonly Dictionary<string, VEntityOption> _players;
    private readonly Dictionary<uint, Dictionary<string, VEntityOption>> _objects;
    private readonly Dictionary<string, VEntityOption> _options;

    public OptionRegistry()
    {
        _vehicles = new Dictionary<string, VEntityOption>();
        _players = new Dictionary<string, VEntityOption>();
        _objects = new Dictionary<uint, Dictionary<string, VEntityOption>>();
        _options = new Dictionary<string, VEntityOption>();
    }

    public void Destroy()
    {
        _vehicles.Clear();
        _players.Clear();
        foreach (var opt in _objects.Values)
        {
            opt.Clear();
        }
        _objects.Clear();
        _options.Clear();
    }

    public void RegisterGlobalVehicle(VEntityOption option)
    {
        if (_vehicles.ContainsKey(option.Id))
        {
            Alt.LogWarning($"Option with ID {option.Id} is already registered. Skipping...");
            return;
        }
        _vehicles.Add(option.Id, option);
    }

    public void RegisterGlobalPlayer(VEntityOption option)
    {
        if (_players.ContainsKey(option.Id))
        {
            Alt.LogWarning($"Option with ID {option.Id} is already registered. Skipping...");
            return;
        }
        _players.Add(option.Id, option);
    }

    public void RegisterGlobalObject(uint model, VEntityOption option)
    {
        if (_objects.ContainsKey(model))
        {
            if (_objects[model].ContainsKey(option.Id))
            {
                Alt.LogWarning($"Option with ID {option.Id} is already registered. Skipping...");
                return;
            }
        }
        else
        {
            _objects[model] = new Dictionary<string, VEntityOption>();
        }
        _objects[model].Add(option.Id, option);
    }

    public void Register(VEntityOption option)
    {
        if (_options.ContainsKey(option.Id))
        {
            Alt.LogWarning($"Option with ID {option.Id} is already registered. Skipping...");
            return;
        }
        _options.Add(option.Id, option);
    }

    public Dictionary<string, VEntityOption> GetOptionsForEntity(int entityType, uint entity, ISharedEntity? altEntity)
    {
        var options = new Dictionary<string, VEntityOption>();
        if (entityType == 3)
        {
            uint model = Alt.Natives.GetEntityModel(entity);
            if (_objects.ContainsKey(model))
            {
                options = new Dictionary<string, VEntityOption>(_objects[model]);
            }
        }
        else
        {
            if (entityType == 2)
            {
                options = new Dictionary<string, VEntityOption>(_vehicles);
            } 
            else if (entityType == 1)
            {
                options = new Dictionary<string, VEntityOption>(_players);
            }

            List<string> entityOptions;
            if (altEntity != null && altEntity.HasStreamSyncedMetaData("vtarget") && altEntity.GetStreamSyncedMetaData("vtarget", out entityOptions))
            {
                entityOptions.ForEach(o =>
                {
                    if (_options.ContainsKey(o))
                    {
                        options.Add(o,_options[o]);
                    }
                    else
                    {
                        Alt.LogWarning($"Entity has unknown option: {o}");
                    }
                });
            }
        }

        return options;
    }
}