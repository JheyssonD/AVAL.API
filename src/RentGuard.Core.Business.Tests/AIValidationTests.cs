using RentGuard.Contracts.Modules.AIValidation;
using RentGuard.Core.Business.Modules.AIValidation.Domain;
using Xunit;

namespace RentGuard.Core.Business.Tests;

public class AIValidationTests
{
    private readonly ValidationPolicy _policy = new();

    [Fact]
    public void LowConfidence_ShouldRequireReview()
    {
        var data = new ExtractedData(1000, DateTime.Now, "REF", 0.80); // 80% < 85%
        var result = _policy.Evaluate(data, 1000);
        Assert.True(result.RequiresReview);
        Assert.Contains("Low confidence", result.Reason);
    }

    [Fact]
    public void AmountDiscrepancy_ShouldRequireReview()
    {
        var data = new ExtractedData(1010, DateTime.Now, "REF", 0.95); // 1% de diferencia
        var result = _policy.Evaluate(data, 1000); // Umbral es 0.5% (5)
        Assert.True(result.RequiresReview);
        Assert.Contains("Amount discrepancy", result.Reason);
    }

    [Fact]
    public void ValidData_ShouldPass()
    {
        var data = new ExtractedData(1000, DateTime.Now, "REF", 0.99);
        var result = _policy.Evaluate(data, 1000);
        Assert.False(result.RequiresReview);
    }
}
