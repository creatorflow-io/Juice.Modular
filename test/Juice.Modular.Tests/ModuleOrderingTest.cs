using FluentAssertions;
using Juice.Modular.Tests.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Juice.Modular.Tests
{
    public class ModuleOrderingTest
    {
        private static IWebHostEnvironment CreateEnv() => new TestHostEnvironment();

        [Fact]
        public Task ConfigureServices_is_called_in_StartOrder_ascending_orderAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var mvc = services.AddControllers();
            mvc.AddApplicationPart(typeof(FirstModule).Assembly);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["App:Features:0"] = "First",
                    ["App:Features:1"] = "Second"
                })
                .Build();

            mvc.AddDiscoveredModules(CreateEnv(), config);

            var sp = services.BuildServiceProvider();

            // Each module registers a ModuleCallRecord singleton in ConfigureServices.
            // Registration order reflects the StartOrder call sequence.
            var records = sp.GetServices<ModuleCallRecord>().ToList();

            records.Should().HaveCount(2);
            records[0].Name.Should().Be("First");   // StartOrder = 1
            records[1].Name.Should().Be("Second");  // StartOrder = 2

            return Task.CompletedTask;
        }

        [Fact]
        public Task IModuleStartup_instances_are_retrieved_in_StartOrder_ascending_orderAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var mvc = services.AddControllers();
            mvc.AddApplicationPart(typeof(FirstModule).Assembly);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["App:Features:0"] = "Second",  // registered in reverse order in config
                    ["App:Features:1"] = "First"
                })
                .Build();

            mvc.AddDiscoveredModules(CreateEnv(), config);

            var sp = services.BuildServiceProvider();
            var startups = sp.GetServices<IModuleStartup>()
                .OrderBy(s => s.StartOrder)
                .ToList();

            startups.Should().HaveCount(2);
            startups[0].Should().BeOfType<FirstModule>();   // StartOrder = 1
            startups[1].Should().BeOfType<SecondModule>();  // StartOrder = 2

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
