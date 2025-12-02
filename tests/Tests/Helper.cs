using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xunit;

namespace Tests;

#pragma warning disable CA1416 // Validate platform compatibility

public static class Helper
{
    public static string FindSolutionDirectory(ITestOutputHelper? output = null)
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        output?.WriteLine($"Current directory: {currentDir}");

        while (currentDir != null)
        {
            var slnFiles = Directory.GetFiles(currentDir, "*.sln");
            if (slnFiles.Length > 0)
            {
                output?.WriteLine($"Found solution directory: {currentDir}");
                return currentDir;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        throw new InvalidOperationException("Could not find solution directory");
    }

    public static void StartIISExpress(string path, int port, ITestOutputHelper output)
    {
        if (!Directory.Exists(path))
        {
            throw new InvalidOperationException($"Test project not found at: {path}");
        }

        var iisExpressPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express\\iisexpress.exe");

        if (!File.Exists(iisExpressPath))
        {
            // Try x86 path
            iisExpressPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "IIS Express\\iisexpress.exe");
            if (!File.Exists(iisExpressPath))
            {
                throw new InvalidOperationException("IIS Express not found. Please install IIS Express to run this test.");
            }
        }

        output.WriteLine($"Starting IIS Express on port {port} for {path}");

        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = iisExpressPath,
                Arguments = $"/path:\"{path}\" /port:{port}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        p.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.WriteLine($"[IIS] [{port}] {args.Data}");
            }
        };
        p.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.WriteLine($"[IIS ERROR] [{port}] {args.Data}");
            }
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
    }

    public static async Task<bool> WaitForServerReadyAsync(string healthCheckUrl, ITestOutputHelper output)
    {
        output.WriteLine($"Waiting for {healthCheckUrl} to be available");

        var maxAttempts = 15;
        var attempt = 0;

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        ExceptionDispatchInfo? edi = null;
        string? errorContent = null;
        while (attempt < maxAttempts)
        {
            attempt++;
            try
            {
                var response = await client.GetAsync(healthCheckUrl);
                if (response.IsSuccessStatusCode)
                {
                    output.WriteLine($"Server ready after {attempt} attempts");
                    return true;
                }
                output.WriteLine($"Attempt {attempt} out of {maxAttempts} - server not ready - {response.StatusCode}");
                var stream = await response.Content.ReadAsStreamAsync();
                errorContent = await new StreamReader(stream).ReadToEndAsync();
                edi = null;
            }
            catch (Exception exc)
            {
                output.WriteLine($"Attempt {attempt} out of {maxAttempts} - server not ready - {exc.Message}");
                edi = ExceptionDispatchInfo.Capture(exc);
                errorContent = null;
            }

            await Task.Delay(1000);
        }

        if (errorContent != null)
        {
            output.WriteLine("Last error content from server:");
            output.WriteLine(errorContent);
        }
        else
        {
            edi?.Throw();
        }
        return false;
    }

    public static void StopIISExpress(string path, int port, ITestOutputHelper output)
    {
        // Find the IIS Express process for the given path and port
        var processes = Process.GetProcessesByName("iisexpress");
        foreach (var process in processes)
        {
            try
            {
                var commandLine = GetCommandLine(process);
                if (commandLine.Contains($"/path:\"{path}\"") && commandLine.Contains($"/port:{port}"))
                {
                    output.WriteLine($"Stopping IIS Express for: {path} on port {port}");
                    process.Kill();
                    process.WaitForExit(5000);
                    output.WriteLine("IIS Express stopped");
                    break;
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"Error stopping IIS Express process: {ex.Message}");
            }
        }
    }

    public static void StartKestrel(string projectPath, int port, ITestOutputHelper output)
    {
        if (!Directory.Exists(projectPath))
        {
            throw new InvalidOperationException($"Test project not found at: {projectPath}");
        }

        output.WriteLine($"Starting Kestrel on port {port} for {projectPath}");

        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --no-build --urls \"http://localhost:{port}\" --project \"{projectPath}\"",
                WorkingDirectory = projectPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        p.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.WriteLine($"[Kestrel] [{port}] {args.Data}");
            }
        };
        p.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.WriteLine($"[Kestrel ERROR] [{port}] {args.Data}");
            }
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
    }

    public static void StopKestrel(string projectPath, int port, ITestOutputHelper output)
    {
        // Find the dotnet process for the given project and port
        var processes = Process.GetProcessesByName("dotnet");
        foreach (var process in processes)
        {
            try
            {
                var commandLine = GetCommandLine(process);
                if (commandLine.Contains($"run --no-build --urls \"http://localhost:{port}\"") && commandLine.Contains($"\"{projectPath}\""))
                {
                    output.WriteLine($"Stopping Kestrel for: {projectPath} on port {port}");
                    process.Kill();
                    process.WaitForExit(5000);
                    output.WriteLine("Kestrel stopped");
                    break;
                }
            }
            catch (Exception ex)
            {
                output.WriteLine($"Error stopping Kestrel process: {ex.Message}");
            }
        }
    }

    private static string GetCommandLine(Process process)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
            using var objects = searcher.Get();
            var result = objects.Cast<ManagementBaseObject>().SingleOrDefault();
            return result?["CommandLine"]?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting command line for process {process.Id}: {ex.Message}");
            return string.Empty;
        }
    }

    internal static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K key, out V value)
    {
        key = kvp.Key;
        value = kvp.Value;
    }
}
