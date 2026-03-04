using CPUSimulator;
using Hexa.NET.KittyUI;
using Hexa.NET.KittyUI.UI;
using System.Reflection;

AppBuilder.Create()
    .StyleColorsDark()
    .EnableLogging(true)
    .EnableDebugTools(true)
    .AddWindow<MainWindow>()
    .SetTitle("CPU Simulator")
    .AddTitleBar<TitleBar>()
    .AddDefaultFont()
    .AddFont("CascadiaMono", builder => builder.AddFontFromEmbeddedResource(Assembly.GetExecutingAssembly(), "CPUSimulator.CascadiaMono.ttf", 14))
    .AddFont("CascadiaMonoEven", builder =>
    {
        builder.Config.GlyphMinAdvanceX = 14;
        builder.AddFontFromEmbeddedResource(Assembly.GetExecutingAssembly(), "CPUSimulator.CascadiaMono.ttf", 14);
    })
    .Run();