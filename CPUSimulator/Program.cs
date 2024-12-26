using CPUSimulator;
using Hexa.NET.KittyUI;

AppBuilder.Create()
    .StyleColorsDark()
    .AddWindow<MainWindow>(true, true)
    .AddDefaultFont()
    .AddFont("CascadiaMono", builder => builder.AddFontFromFileTTF("CascadiaMono.ttf", 14))
    .Run();