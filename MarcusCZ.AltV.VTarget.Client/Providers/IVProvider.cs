using System.Diagnostics.CodeAnalysis;
using MarcusCZ.AltV.VTarget.Client.Options;

namespace MarcusCZ.AltV.VTarget.Client.Providers;

public interface IVProvider
{
    public Type GetTypeProvided { get; }
    public bool BlockOthers { get; }

    public bool Async
    {
        get => false;
    }
    public bool GetOptions([MaybeNullWhen(false)]out List<IVTargetOption> options);

    public void EmitOnClick(IVTargetOption option, Alert alert);
}