using CPUSimulator;
using Hexa.NET.KittyUI;
using Hexa.NET.KittyUI.UI;
using System.Reflection;

AppBuilder.Create()
    .StyleColorsDark()
    .EnableLogging(false)
    .EnableDebugTools(false)
    .AddWindow<MainWindow>(true, true)
    .SetTitle("CPU Simulator")
    .AddTitleBar<TitleBar>()
    .AddDefaultFont()
    .AddFont("CascadiaMono", builder => builder.AddFontFromEmbeddedResource(Assembly.GetExecutingAssembly(), "CPUSimulator.CascadiaMono.ttf", 14))
    .Run();