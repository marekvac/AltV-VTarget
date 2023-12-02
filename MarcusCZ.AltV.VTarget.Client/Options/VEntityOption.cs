using System.Numerics;
using AltV.Net.Shared.Elements.Entities;
using MarcusCZ.AltV.VTarget.Client.Data;

namespace MarcusCZ.AltV.VTarget.Client.Options;

public delegate bool VEntityOptionCheckCallback(uint entity, Vector3 pos, ISharedEntity? altEntity);

public delegate bool VEntityOptionCallback(uint entity, Vector3 pos, ISharedEntity? altEntity, Alert alert);
public class VEntityOption : IVTargetOption
{
    public string Id { get; }
    public string Icon { get; set; }
    public string Label { get; set; }
    public string? Description { get; set; }
    public Background? Background { get; set; }
    public List<string>? Bones { get; set; }
    public bool EnableInVehicle { get; set; }
    public Position Position { get; set; }
    public bool Show { get; set; }
    public bool Interact { get; set; }
    public float? Distance { get; set; }
    public List<IVTargetOption>? Children { get; set; }
    public VEntityOptionCheckCallback CanShow { get; set; }
    public VEntityOptionCheckCallback CanInteract { get; set; }
    public VEntityOptionCallback OnClick { get; set; }
    public VEntityOptionCallback OnDisabledClick { get; set; }

    public VEntityOption(string icon, string label, string id) : this(icon, label)
    {
        Id = id;
    }
    
    public VEntityOption(string icon, string label)
    {
        Id = Utils.RandomId();
        Icon = icon;
        Label = label;
        CanShow = (_,_,_) => true;
        CanInteract = (_,_,_) => true;
        OnClick = (_,_,_,_) => false;
        OnDisabledClick = (_,_,_,_) => false;
        Distance = 7;
        Position = Position.RIGHT;
    }

    public object[] Serialize()
    {
        string desc = Description ?? "";
        string bg = Background.ToString() ?? "";
        bg = bg.ToLower();
        object[] children = Array.Empty<object>();
        if (Children != null && Children.Count > 0)
        {
            children = new object[Children.Count];
            for (int i = 0; i < Children.Count; i++)
            {
                children[i] = Children[i].Serialize();
            }
        }
        object[] obj = {Id,Icon,Label,desc,bg,Interact,Position.ToString(),children};
        return obj;
    }

    
}