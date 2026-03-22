namespace Juice.Modular
{
    /// <summary>
    /// Specifies feature information for a module
    /// <para>Default Name of module will be namespace of startup class with underscore</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Feature : Attribute
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public string[] IncompatibleFeatures { get; set; } = Array.Empty<string>();

        public bool Required { get; set; }
    }
}
