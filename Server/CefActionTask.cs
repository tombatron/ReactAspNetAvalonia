using System;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Server;

public sealed class CefActionTask(Action action) : CefTask
{
    protected override void Execute() => action();
}
