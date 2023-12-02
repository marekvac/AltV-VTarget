using MarcusCZ.AltV.VTarget.Client.Options;

namespace MarcusCZ.AltV.VTarget.Client.Providers;

public interface IVAsyncProvider : IVProvider
{
    public new bool Async
    {
        get => true;
    }
    
    public Task<List<IVTargetOption>> GetOptionsAsync();
}