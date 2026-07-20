using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalTrainer.Api.Tests.Integration.TestSupport;

public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
