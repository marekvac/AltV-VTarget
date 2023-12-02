using MarcusCZ.AltV.VTarget.Client.Data;

namespace MarcusCZ.AltV.VTarget.Client.Options;

public delegate void Alert(Background background, string message);
public interface IVTargetOption
{
    public string Id { get; }
    public string Icon { get; set; }
    public string Label { get; set; }
    public string? Description { get; set; }
    public Background? Background { get; set; }
    public bool EnableInVehicle { get; set; }
    public Position Position { get; set; }
    public bool Show { get; set; }
    public bool Interact { get; set; }
    public List<IVTargetOption>? Children { get; set; }

    public object[] Serialize();
}