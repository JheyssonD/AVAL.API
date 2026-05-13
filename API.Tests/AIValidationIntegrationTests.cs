using RentGuard.Contracts.Modules.AIValidation;
using RentGuard.Core.Business.Modules.AIValidation.ProcessValidation;
using FluentAssertions;
using Xunit;

namespace RentGuard.API.Tests;

public class AIValidationIntegrationTests
{
    private readonly ProcessValidationHandler _handler = new();

    [Fact]
    public async Task ProcessValidation_WithValidData_ShouldNotRequireReview()
    {
        var data = new ExtractedData(1000m, DateTime.Now, "REF", 0.99);
        var response = await _handler.Handle(data, 1000m);

        response.RequiresManualReview.Should().BeFalse();
    }
}
