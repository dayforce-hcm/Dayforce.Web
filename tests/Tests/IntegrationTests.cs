using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TestModels;
using Xunit;

namespace Tests;

[CollectionDefinition("Web Framework Tests")]
public class WebFrameworkCollection : ICollectionFixture<AllWebFrameworksFixture> { }

[Collection("Web Framework Tests")]
public class IntegrationTests(AllWebFrameworksFixture m_fixture, ITestOutputHelper m_output)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public record TestRequestDefinition(
        object Expected,
        object? Model = null,
        Dictionary<string, string>? Headers = null);

    private static readonly Dictionary<string, TestRequestDefinition> s_getTestCases = new()
    {
        ["Ping"] = new(TestModel.New("Get Ping")),
    };

    private static readonly Dictionary<string, TestRequestDefinition> s_postTestCases = new()
    {
        ["Pong"] = new(TestModel.New("Post Pong")),
    };

    public static TheoryData<string, string> GetTestData() => TestData(s_getTestCases);
    public static TheoryData<string, string> PostTestData() => TestData(s_postTestCases);

    private static TheoryData<string, string> TestData(Dictionary<string, TestRequestDefinition> testCases)
    {
        var data = new TheoryData<string, string>();
        foreach (var (frameworkKey, _) in AllWebFrameworksFixture.ProjectDefinitions)
        {
            foreach (var testRequest in testCases.Keys)
            {
                data.Add(frameworkKey, testRequest);
            }
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public async Task Get(string webStack, string testName)
    {
        var testRequest = s_getTestCases[testName];

        // Arrange
        var config = m_fixture.Frameworks.First(o => o.Key == webStack);
        config.ServerReady.Should().BeTrue();

        // Act
        var response = await GetTestResultAsync(testName, testRequest.Headers, config);

        // Assert
        response.Should().BeEquivalentTo(testRequest.Expected);
    }

    [Theory]
    [MemberData(nameof(PostTestData))]
    public async Task Post(string webStack, string testName)
    {
        var testRequest = s_postTestCases[testName];

        // Arrange
        var config = m_fixture.Frameworks.First(o => o.Key == webStack);
        config.ServerReady.Should().BeTrue();

        // Act
        var response = await PostTestResultAsync(testName, testRequest.Model, config);

        // Assert
        response.Should().BeEquivalentTo(testRequest.Expected);
    }

    private async Task<TestModel> GetTestResultAsync(string testName, Dictionary<string, string>? headers, AllWebFrameworksFixture.FrameworkConfig config)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                client.DefaultRequestHeaders.Add(key, value);
            }
        }

        var response = await client.GetAsync($"http://localhost:{config.Port}/Test/{testName}");
        var content = await response.Content.ReadAsStringAsync();
        m_output.WriteLine($"Response from {testName} endpoint:");
        m_output.WriteLine(content);

        response.IsSuccessStatusCode.Should().BeTrue();
        return JsonSerializer.Deserialize<TestModel>(content, s_jsonSerializerOptions)!;
    }

    private async Task<TestModel> PostTestResultAsync(string testName, object? model, AllWebFrameworksFixture.FrameworkConfig config)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var response = await client.PostAsync($"http://localhost:{config.Port}/Test/{testName}",
            new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        m_output.WriteLine($"Response from {testName} endpoint:");
        m_output.WriteLine(content);

        response.IsSuccessStatusCode.Should().BeTrue();
        return JsonSerializer.Deserialize<TestModel>(content, s_jsonSerializerOptions)!;
    }
}
