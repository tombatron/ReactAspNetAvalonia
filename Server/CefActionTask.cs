using System;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Server;

public sealed class CefActionTask : CefTask
{
    private readonly Action _action;

    public CefActionTask(Action action)
    {
        _action = action;
    }

    protected override void Execute()
    {
        _action();
    }
}
