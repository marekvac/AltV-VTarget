using System.Numerics;
using AltV.Net.Client;
using AltV.Net.Client.Elements.Interfaces;
using MarcusCZ.AltV.VTarget.Client.Data;
using MarcusCZ.AltV.VTarget.Client.Options;

namespace MarcusCZ.AltV.VTarget.Client;

internal class FrontEnd
{
    private readonly IWebView _webView;
    private readonly Dictionary<string, IVTargetOption> _options;

    public FrontEnd(VEye vEye)
    {
        _webView = Alt.CreateWebView("http://resource/web/index.html");
        _options = new();
        
        _webView.On<string>("clicked", id =>
        {
            var option = _findOption(_options.Values.ToList(), id);
            if (option == null) return;
            vEye.GetProvider(option).EmitOnClick(option, _alert);
        });
        
        _webView.On("showCursor", () =>
        {
            Alt.GameControlsEnabled = false;
            Alt.ShowCursor(true);
            Alt.SetCursorPos(new Vector2(0.5f,0.5f),true);
        });
    }

    public IWebView GetWebView() => _webView;
    
    private static IVTargetOption? _findOption(List<IVTargetOption> options, string id)
    {
        var option = options.Find(option => option.Id == id);
        if (option == null)
        {
            options.ForEach(o =>
            {
                if (o.Children != null)
                {
                    option = _findOption(o.Children, id);
                }
            });
        }

        return option;
    }
    
    private void _alert(Background background, string message)
    {
        _webView.Emit("alert", background.ToString().ToLower(), message);
    }

    public void UpdateOptions(Tuple<List<IVTargetOption>, List<string>> update)
    {
        _removeOptions(update.Item2);
        _addOptions(update.Item1);
    }
    
    private void _addOptions(List<IVTargetOption> options)
    {
        options.ForEach(option =>
        {
            if (_options.ContainsKey(option.Id)) return;
            _options.Add(option.Id, option);
            _webView.Emit("addOption", option.Serialize());
        });
    }

    private void _removeOptions(List<string> names)
    {
        names.ForEach(name =>
        {
            if (!_options.ContainsKey(name)) return;
            _options.Remove(name);
            _webView.Emit("removeOption", name);
        });
    }

    public void ClearOptions()
    {
        _options.Clear();
        _webView.Emit("clearOptions");
    }

    public void Hit()
    {
        _webView.Emit("hit");
        _webView.Focus();
    }

    public void HitLeft()
    {
        ClearOptions();
        _webView.Emit("hitLeft");
        _webView.Unfocus();
    }
    
    public void StartTargeting() => _webView.Emit("startTargeting");

    public void StopTargeting()
    {
        _webView.Emit("stopTargeting");
        _webView.Unfocus();
        if (!Alt.GameControlsEnabled)
        {
            Alt.GameControlsEnabled = true;
        }

        if (Alt.IsCursorVisible)
        {
            Alt.ShowCursor(false);
        }
    }
    
    public void Destroy()
    {
        _webView.Remove();
    }
}