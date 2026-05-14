-- UpdateTrustInsights.sql
-- Set-based calculation of Trust Score Trajectory using Linear Regression

DECLARE @CurrentDate DATETIME2 = GETDATE();

-- 1. Create a CTE for the calculation
WITH TrajectoryData AS (
    SELECT 
        UserId,
        ScoreValue,
        -- Index for linear regression (0, 1, 2...)
        ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY ScoreDate) - 1 AS x,
        ScoreValue AS y
    FROM TrustScoreSnapshots
    -- In a real scenario, we would filter by TenantId if available in the snapshot table
),
Stats AS (
    SELECT
        UserId,
        COUNT(*) AS n,
        SUM(x) AS sumX,
        SUM(y) AS sumY,
        SUM(x * y) AS sumXY,
        SUM(x * x) AS sumX2,
        MAX(y) AS CurrentScore,
        MIN(ScoreDate) AS FirstScoreDate,
        MAX(ScoreDate) AS LastScoreDate
    FROM (
        SELECT t.*, s.ScoreDate 
        FROM TrajectoryData t 
        JOIN TrustScoreSnapshots s ON t.UserId = s.UserId AND t.y = s.ScoreValue -- This join is simplified
    ) d
    GROUP BY UserId
),
Regression AS (
    SELECT
        UserId,
        n,
        CurrentScore,
        CASE 
            WHEN (n * sumX2 - sumX * sumX) = 0 THEN 0 
            ELSE (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX) 
        END AS TrendVector,
        DATEDIFF(MONTH, FirstScoreDate, LastScoreDate) AS MaturityLevel
    FROM Stats
)
-- 2. Update TrustInsights
MERGE INTO TrustInsights AS target
USING Regression AS source
ON target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET 
        CurrentScore = source.CurrentScore,
        TrendVector = source.TrendVector,
        MaturityLevel = source.MaturityLevel,
        LastUpdated = @CurrentDate,
        -- Volatility and Level would ideally be calculated here too or in a second pass
        AdjustedTrustLevel = CASE 
            WHEN source.CurrentScore >= 900 AND source.TrendVector >= 0 THEN 5 -- Elite
            WHEN source.CurrentScore >= 700 AND source.TrendVector >= 0 THEN 4 -- High
            WHEN source.CurrentScore >= 400 THEN 3 -- Medium
            ELSE 1 -- Critical
        END
WHEN NOT MATCHED THEN
    INSERT (UserId, CurrentScore, TrendVector, MaturityLevel, VolatilityFlag, AdjustedTrustLevel, LastUpdated)
    VALUES (source.UserId, source.CurrentScore, source.TrendVector, source.MaturityLevel, 0, 3, @CurrentDate);
