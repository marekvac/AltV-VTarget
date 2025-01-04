using AltV.Net.Client.Async;

namespace MarcusCZ.AltV.VTarget.Client;

public class VTargetResource : AsyncResource
{
    private Target _target;
    public override void OnStart()
    {
        _target = new Target();
        _target.OnStart();
    }

    public override void OnStop()
    {
        _target.OnStop();
    }
}