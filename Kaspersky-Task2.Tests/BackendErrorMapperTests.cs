using System.Net;
using Kaspersky_Task2.Core.Errors;
using Xunit;

namespace Kaspersky_Task2.Tests;

public sealed class BackendErrorMapperTests
{
    [Fact]
    public void Map_BadRequest_ExtractsErrorsFromJson()
    {
        var mapper = new BackendErrorMapper();
        var body = "{\"errors\":{\"Files\":[\"File 'x' does not exist.\"]}}";
        var error = mapper.Map(HttpStatusCode.BadRequest, body);
        Assert.Equal(400, error.StatusCode);
        Assert.Contains("does not exist", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Map_NotFound_DefaultMessage()
    {
        var mapper = new BackendErrorMapper();
        var error = mapper.Map(HttpStatusCode.NotFound, "");
        Assert.Equal(404, error.StatusCode);
        Assert.Equal("Process not found.", error.Message);
    }

    [Fact]
    public void Map_Conflict_DefaultMessage()
    {
        var mapper = new BackendErrorMapper();
        var error = mapper.Map(HttpStatusCode.Conflict, "");
        Assert.Equal(409, error.StatusCode);
        Assert.Equal("Archive is not ready yet.", error.Message);
    }

    [Fact]
    public void Map_Conflict_UsesBodyIfPresent()
    {
        var mapper = new BackendErrorMapper();
        var error = mapper.Map(HttpStatusCode.Conflict, "Archive is not ready yet.");
        Assert.Equal(409, error.StatusCode);
        Assert.Contains("not ready", error.Message, StringComparison.OrdinalIgnoreCase);
    }
}

