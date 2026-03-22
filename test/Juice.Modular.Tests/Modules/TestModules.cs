using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.Modular.Tests.Modules
{
    [Feature(Name = "Alpha")]
    public class AlphaModule : ModuleStartup { }

    [Feature(Name = "Beta", Dependencies = new[] { "Alpha" })]
    public class BetaModule : ModuleStartup { }

    [Feature(Name = "Gamma", IncompatibleFeatures = new[] { "Alpha" })]
    public class GammaModule : ModuleStartup { }

    [Feature(Name = "First")]
    public class FirstModule : ModuleStartup
    {
        public override int StartOrder => 1;

        public override void ConfigureServices(IServiceCollection services, IMvcBuilder mvc,
            IWebHostEnvironment env, IConfiguration configuration)
            => services.AddSingleton(new ModuleCallRecord("First"));
    }

    [Feature(Name = "Second")]
    public class SecondModule : ModuleStartup
    {
        public override int StartOrder => 2;

        public override void ConfigureServices(IServiceCollection services, IMvcBuilder mvc,
            IWebHostEnvironment env, IConfiguration configuration)
            => services.AddSingleton(new ModuleCallRecord("Second"));
    }

    public record ModuleCallRecord(string Name);
}
