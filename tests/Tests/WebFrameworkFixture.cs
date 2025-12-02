using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Tests;

/// <summary>
/// Fixture that manages IIS Express/Kestrel lifecycle for all web framework test projects.
/// All three frameworks (ASP.NET MVC, WebAPI, WCF) are started simultaneously and shared across all tests.
/// Configuration is read from each project's launchSettings.json file.
/// </summary>
public class AllWebFrameworksFixture : IDisposable
{
#if DEBUG
    private const string CONFIGURATION = "Debug";
#else
    private const string CONFIGURATION = "Release";
#endif

    public class FrameworkConfig
    {
        public required bool IsNetCore { get; init; }
        public required string Key { get; init; }
        public required int Port { get; init; }
        public required string WebAppPath { get; init; }
        public required string RootPath { get; init; }
        public bool ServerReady { get; set; }
    }

    public static readonly (string Key, string ProjectName)[] ProjectDefinitions =
    [
        ("Asp.Net", "AspNetTest"),
    ];

    public readonly List<FrameworkConfig> Frameworks = [];
    private readonly List<SingleFrameworkInstance> m_instances = [];
    private readonly MessageSinkTestOutputHelper m_output;

    public AllWebFrameworksFixture(IMessageSink messageSink)
    {
        m_output = new MessageSinkTestOutputHelper(messageSink);
        var solutionDir = Helper.FindSolutionDirectory(m_output);

        m_output.WriteLine($"Starting {ProjectDefinitions.Length} server instances...");

        foreach (var (key, projectName) in ProjectDefinitions)
        {
            var fullProjectPath = Path.Combine(solutionDir, "tests\\apps", projectName);
            var (rootPath, port) = ExtractDataFromLaunchSettings(fullProjectPath);

            var framework = new FrameworkConfig
            {
                IsNetCore = key.Contains("Core"),
                Key = key,
                WebAppPath = $@"{solutionDir}\tests\bin\{CONFIGURATION}\net472\_PublishedWebsites\{projectName}",
                Port = port,
                RootPath = rootPath,
            };
            Frameworks.Add(framework);
            m_instances.Add(new(framework, m_output));
        }

        for (int i = 0; i < m_instances.Count; i++)
        {
            int count = 3;
            while (count-- > 0 && !m_instances[i].WaitForServerReady())
            {
                m_instances[i].Dispose();
                m_instances[i] = new(Frameworks[i], m_output);
            }
            Frameworks[i].ServerReady = count >= 0;
        }
    }

    private static (string rootPath, int port) ExtractDataFromLaunchSettings(string projectPath)
    {
        var launchSettingsPath = Path.Combine(projectPath, "Properties", "launchSettings.json");

        if (!File.Exists(launchSettingsPath))
        {
            throw new FileNotFoundException($"launchSettings.json not found at: {launchSettingsPath}");
        }

        var json = File.ReadAllText(launchSettingsPath);
        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        // Read port from iisSettings.iisExpress.applicationUrl
        var applicationUrl = root.GetProperty("iisSettings").GetProperty("iisExpress").GetProperty("applicationUrl").GetString();
        if (applicationUrl == null)
        {
            throw new InvalidOperationException("applicationUrl not found in launchSettings.json");
        }

        var port = new Uri(applicationUrl).Port;

        var pingUrl = root.GetProperty("profiles").GetProperty("Ping (IIS Express)").GetProperty("launchUrl").ToString();
        if (!pingUrl.EndsWith("/ping"))
        {
            throw new InvalidOperationException("Expected launchUrl for Ping (IIS Express) to end with /ping in launchSettings.json");
        }

        return (pingUrl[..^"/ping".Length], port);
    }

    public void Dispose()
    {
        m_output.WriteLine("Disposing AllWebFrameworksFixture - stopping all server instances...");

        foreach (var disposable in m_instances)
        {
            disposable.Dispose();
        }
        m_instances.Clear();

        m_output.WriteLine("All server instances stopped");
    }

    /// <summary>
    /// Helper class to manage a single server instance
    /// </summary>
    private class SingleFrameworkInstance : IDisposable
    {
        private readonly FrameworkConfig m_config;
        private readonly ITestOutputHelper m_output;
        private readonly ConfiguredTaskAwaitable<bool> m_readyTask;

        internal SingleFrameworkInstance(FrameworkConfig config, ITestOutputHelper output)
        {
            m_config = config;
            m_output = output;

            Helper.StartIISExpress(m_config.WebAppPath, m_config.Port, m_output);
            m_readyTask = WaitForServerReadyAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            try
            {
                Helper.StopIISExpress(m_config.WebAppPath, m_config.Port, m_output);
            }
            catch (Exception ex)
            {
                m_output.WriteLine($"Error stopping server for {m_config.Key}: {ex}");
            }
        }

        private Task<bool> WaitForServerReadyAsync() => Helper.WaitForServerReadyAsync($"http://localhost:{m_config.Port}/{m_config.RootPath}/ping", m_output);

        internal bool WaitForServerReady()
        {
            try
            {
                return m_readyTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                m_output.WriteLine($"Error while waiting for server for {m_config.Key} to become ready on port {m_config.Port}: {ex}");
                return false;
            }
        }
    }
}
