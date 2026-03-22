using FluentAssertions;
using Juice.Modular.Conventions.StartupDiscovery;
using Juice.Modular.Tests.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Juice.Modular.Tests
{
    public class StartupDiscoveryTest
    {
        private static IWebHostEnvironment CreateEnv() => new TestHostEnvironment();

        private static IConfigurationRoot EmptyConfig() =>
            new ConfigurationBuilder().Build();

        [Fact]
        public Task Should_discover_concrete_module_startups_in_test_assemblyAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var mvc = services.AddControllers();
            mvc.AddApplicationPart(typeof(AlphaModule).Assembly);

            mvc.AddDiscoveredModules(CreateEnv(), EmptyConfig());

            var sp = services.BuildServiceProvider();
            var feature = sp.GetRequiredService<StartupDiscoveryFeature>();

            feature.Startups.Should().Contain(typeof(AlphaModule));
            feature.Startups.Should().Contain(typeof(BetaModule));
            feature.Startups.Should().Contain(typeof(GammaModule));
            feature.Startups.Should().Contain(typeof(FirstModule));
            feature.Startups.Should().Contain(typeof(SecondModule));

            return Task.CompletedTask;
        }

        [Fact]
        public Task Should_not_discover_abstract_classes_or_interfacesAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var mvc = services.AddControllers();
            mvc.AddApplicationPart(typeof(AlphaModule).Assembly);

            mvc.AddDiscoveredModules(CreateEnv(), EmptyConfig());

            var sp = services.BuildServiceProvider();
            var feature = sp.GetRequiredService<StartupDiscoveryFeature>();

            feature.Startups.Should().NotContain(typeof(ModuleStartup));
            feature.Startups.Should().NotContain(typeof(IModuleStartup));

            return Task.CompletedTask;
        }

        [Fact]
        public Task Should_discover_modules_from_explicitly_added_assembly_partAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var mvc = services.AddControllers();
            mvc.AddApplicationPart(typeof(SimpleModule.SimpleModule).Assembly);

            mvc.AddDiscoveredModules(CreateEnv(), EmptyConfig());

            var sp = services.BuildServiceProvider();
            var feature = sp.GetRequiredService<StartupDiscoveryFeature>();

            feature.Startups.Should().Contain(typeof(SimpleModule.SimpleModule));

            return Task.CompletedTask;
        }

        [Fact]
        public Task Should_not_discover_modules_from_assembly_not_added_as_partAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var mvc = services.AddControllers();
            // deliberately NOT adding SimpleModule assembly

            mvc.AddDiscoveredModules(CreateEnv(), EmptyConfig());

            var sp = services.BuildServiceProvider();
            var feature = sp.GetRequiredService<StartupDiscoveryFeature>();

            feature.Startups.Should().NotContain(typeof(SimpleModule.SimpleModule));

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
