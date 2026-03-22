using FluentAssertions;
using Juice.Modular.Tests.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Juice.Modular.Tests
{
    public class ModuleRegistrationTest
    {
        private static IWebHostEnvironment CreateEnv() => new TestHostEnvironment();

        private static IConfigurationRoot Config(params (string key, string value)[] entries) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(entries.ToDictionary(e => e.key, e => (string?)e.value))
                .Build();

        private static IMvcBuilder SetupMvc(IServiceCollection services, bool includeTestModules = true)
        {
            services.AddLogging();
            var mvc = services.AddControllers();
            if (includeTestModules)
                mvc.AddApplicationPart(typeof(AlphaModule).Assembly);
            return mvc;
        }

        [Fact]
        public Task Required_module_registers_without_App_Features_configAsync()
        {
            var services = new ServiceCollection();
            var mvc = SetupMvc(services, includeTestModules: false);
            mvc.AddApplicationPart(typeof(SimpleModule.SimpleModule).Assembly);

            mvc.AddDiscoveredModules(CreateEnv(), Config());

            var sp = services.BuildServiceProvider();
            var startups = sp.GetServices<IModuleStartup>();

            startups.Should().ContainSingle(s => s is SimpleModule.SimpleModule);

            return Task.CompletedTask;
        }

        [Fact]
        public Task Optional_module_does_not_register_without_configAsync()
        {
            var services = new ServiceCollection();
            var mvc = SetupMvc(services);

            mvc.AddDiscoveredModules(CreateEnv(), Config());

            var sp = services.BuildServiceProvider();
            var startups = sp.GetServices<IModuleStartup>();

            startups.Should().NotContain(s => s is AlphaModule);

            return Task.CompletedTask;
        }

        [Fact]
        public Task Module_registers_when_in_App_FeaturesAsync()
        {
            var services = new ServiceCollection();
            var mvc = SetupMvc(services);

            mvc.AddDiscoveredModules(CreateEnv(), Config(("App:Features:0", "Alpha")));

            var sp = services.BuildServiceProvider();
            var startups = sp.GetServices<IModuleStartup>();

            startups.Should().ContainSingle(s => s is AlphaModule);

            return Task.CompletedTask;
        }

        [Fact]
        public Task Dependency_module_is_auto_enabled_when_dependent_is_enabledAsync()
        {
            var services = new ServiceCollection();
            var mvc = SetupMvc(services);

            // Enable Beta which declares Dependencies = ["Alpha"]
            mvc.AddDiscoveredModules(CreateEnv(), Config(("App:Features:0", "Beta")));

            var sp = services.BuildServiceProvider();
            var startups = sp.GetServices<IModuleStartup>().Select(s => s.GetType()).ToList();

            startups.Should().Contain(typeof(BetaModule));
            startups.Should().Contain(typeof(AlphaModule));

            return Task.CompletedTask;
        }

        [Fact]
        public Task Excluded_module_does_not_register_even_if_in_App_FeaturesAsync()
        {
            var services = new ServiceCollection();
            var mvc = SetupMvc(services);

            mvc.AddDiscoveredModules(CreateEnv(), Config(
                ("App:Features:0", "Alpha"),
                ("App:ExcludedFeatures:0", "Alpha")
            ));

            var sp = services.BuildServiceProvider();
            var startups = sp.GetServices<IModuleStartup>();

            startups.Should().NotContain(s => s is AlphaModule);

            return Task.CompletedTask;
        }

        [Fact]
        public Task Incompatible_features_throw_on_registrationAsync()
        {
            var services = new ServiceCollection();
            var mvc = SetupMvc(services);

            var act = () => mvc.AddDiscoveredModules(CreateEnv(), Config(
                ("App:Features:0", "Alpha"),
                ("App:Features:1", "Gamma")   // Gamma declares IncompatibleFeatures = ["Alpha"]
            ));

            act.Should().Throw<Exception>();

            return Task.CompletedTask;
        }

        private sealed class TestHostEnvironment : IWebHostEnvironment
        {
            public string ApplicationName { get; set; } = "TestApp";
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
            public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
            public string EnvironmentName { get; set; } = "Test";
            public string WebRootPath { get; set; } = "";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}
