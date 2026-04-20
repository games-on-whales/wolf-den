namespace WolfLeash.Components.Classes;

public class CompatibilityToolsOptions
{
    public const string SectionName = "CompatibilityTools";

    public List<CompatibilityToolTarget> Targets { get; set; } = new();
}

public class CompatibilityToolTarget
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
}
