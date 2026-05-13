namespace RentGuard.Contracts.Modules.TrustScore;
public record GetTrustScoreResponse(string TenantId, int CurrentScore, string Tier);
