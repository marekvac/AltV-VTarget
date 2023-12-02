using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using AltV.Net.Client;
using AltV.Net.Shared.Elements.Entities;
using MarcusCZ.AltV.VTarget.Client.Options;

namespace MarcusCZ.AltV.VTarget.Client.Providers;

internal class WorldShapeTestProvider : IVProvider
{
    public Type GetTypeProvided
    {
        get => typeof(VEntityOption);
    }
    public bool BlockOthers
    {
        get => true;
    }
    
    private int _handle;
    private bool _hit;
    private Vector3 _hitPosition;
    private Vector3 _surfaceNormal;
    private uint _hitEntity;
    private int _status = -1;
    private bool _needRefresh;
    private uint _lastEntity;
    private Vector3 _lastHitPos = Vector3.Zero;
    private Vector3? _lastPlayerPos;
    private readonly OptionRegistry _optionRegistry;
    private ISharedEntity? _altEntity;
    private List<IVTargetOption> _lastOptions;

    public WorldShapeTestProvider(OptionRegistry optionRegistry)
    {
        _optionRegistry = optionRegistry;
        _lastOptions = new List<IVTargetOption>();
    }

    public bool GetOptions([MaybeNullWhen(false)] out List<IVTargetOption> options)
    {
        switch (_status)
        {
            case -1 or 0:
                _startNewShapeTest();
                break;
            case 1:
                _status = Alt.Natives.GetShapeTestResult(_handle, ref _hit, ref _hitPosition, ref _surfaceNormal, ref _hitEntity);
                break;
            case 2:
                if (_hit)
                {
                    if (_needRefresh || _hitEntity != _lastEntity || Vector3.Distance(_lastHitPos, _hitPosition) > 0.5 || !_lastPlayerPos.HasValue || Vector3.Distance(_lastPlayerPos.Value, Alt.LocalPlayer.Position) > 1)
                    {
                        _lastHitPos = _hitPosition;
                        _lastEntity = _hitEntity;
                        if (Target.Debug) Alt.Log($"[VTarget:DEBUG] Hit entity model: {Alt.Natives.GetEntityModel(_hitEntity)}");
                        //TODO set alt entity
                        _lastPlayerPos = Alt.LocalPlayer.Position;
                        int type = Alt.Natives.GetEntityType(_hitEntity);
                        var opt = _optionRegistry.GetOptionsForEntity(type, _hitEntity, _altEntity).Values.ToList();
                        options = _evalOptions(opt, _hitEntity, _hitPosition, _altEntity);
                        _lastOptions = new List<IVTargetOption>(options);
                    }
                    else
                    {
                        options = _lastOptions;
                    }
                    if (Target.Debug) Alt.Natives.DrawMarkerSphere(_hitPosition.X, _hitPosition.Y, _hitPosition.Z, 0.2f, 255,255,0,127);
                }
                else
                {
                    _lastEntity = uint.MaxValue;
                    options = new List<IVTargetOption>();
                }

                _status = -1;
                return true;
        }

        options = null;
        return false;
    }

    public void EmitOnClick(IVTargetOption option, Alert alert)
    {
        if (option is VEntityOption entityOption)
        {
            if (option.Interact) _needRefresh = entityOption.OnClick(_hitEntity, _hitPosition, _altEntity, alert);
            else _needRefresh = entityOption.OnDisabledClick(_hitEntity, _hitPosition, _altEntity, alert);
        }
    }

    private void _startNewShapeTest()
    {
        Vector3 camPos = Alt.Natives.GetGameplayCamCoord();
        Vector3 camRot = Alt.Natives.GetGameplayCamRot(0);
        Vector3 endPos = Utils.RayEnd(camPos, Utils.ToRadians(camRot), 100);
        _handle = Alt.Natives.StartShapeTestLosProbe(camPos.X, camPos.Y, camPos.Z, endPos.X, endPos.Y, endPos.Z, 30, Alt.LocalPlayer.ScriptId, 0);
        _status = Alt.Natives.GetShapeTestResult(_handle, ref _hit, ref _hitPosition, ref _surfaceNormal, ref _hitEntity);
    }

    private static List<IVTargetOption> _evalOptions(List<VEntityOption> options, uint entity, Vector3 hitCoords, ISharedEntity? altEntity)
    {
        var newOptions = new List<IVTargetOption>();
        options.ForEach(o =>
        {
            o.Show = o.CanShow(entity, hitCoords, altEntity);
            
            if (!o.EnableInVehicle) o.Show = !Alt.LocalPlayer.IsInVehicle;

            if (o.Show && o.Distance is not null && o.Distance < Vector3.Distance(hitCoords, Alt.LocalPlayer.Position))
            {
                o.Show = false;
            }

            if (o.Show && o.Bones is not null && o.Bones.Count > 0)
            {
                bool found = false;
                o.Bones.ForEach(bone =>
                {
                    if (!found)
                    {
                        int bid = Alt.Natives.GetEntityBoneIndexByName(entity, bone);
                        float dist = Vector3.Distance(Alt.Natives.GetWorldPositionOfEntityBone(entity, bid), hitCoords);
                        found = dist <= 1;
                    }
                });
                o.Show = found;
            }
            
            if (o.Show)
            {
                o.Interact = o.CanInteract(entity, hitCoords, altEntity);
                if (o.Children is not null && o.Children.Count > 0)
                {
                    try
                    {
                        _evalOptions(o.Children.Cast<VEntityOption>().ToList(), entity, hitCoords, altEntity);
                    }
                    catch (InvalidCastException e)
                    {
                        Alt.LogError($"[VTarget] Failed to evaluate children options of option {o.Label} [ID: {o.Id}. Children type must be VTargetEntityOption!");
                        Alt.LogError(e.Message);
                        o.Children = null;
                    }
                }
                newOptions.Add(o);
            }
        });
        return newOptions;
    }
}