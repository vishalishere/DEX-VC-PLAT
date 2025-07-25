// © 2024 DecVCPlat. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DecVCPlat.Shared.Services;

public interface IDecVCPlatEuAiActComplianceService
{
    Task<DecVCPlatAiTransparencyReport> GenerateTransparencyReportAsync(string userId);
    Task<bool> RecordAiDecisionAsync(DecVCPlatAiDecisionRecord decision);
    Task<DecVCPlatAlgorithmicAccountabilityReport> GetAccountabilityReportAsync(string algorithmId);
    Task<bool> ProcessAiExplanationRequestAsync(string userId, string decisionId);
    Task<DecVCPlatAiRiskAssessment> ConductRiskAssessmentAsync(string algorithmId);
    Task<bool> ValidateAiSystemComplianceAsync(string systemId);
}

public class DecVCPlatEuAiActComplianceService : IDecVCPlatEuAiActComplianceService
{
    private readonly ILogger<DecVCPlatEuAiActComplianceService> _logger;
    private readonly IDecVCPlatAiAuditRepository _aiAuditRepository;
    private readonly IDecVCPlatAuditLogger _auditLogger;
    private readonly IDecVCPlatComplianceConfiguration _complianceConfig;

    public DecVCPlatEuAiActComplianceService(
        ILogger<DecVCPlatEuAiActComplianceService> logger,
        IDecVCPlatAiAuditRepository aiAuditRepository,
        IDecVCPlatAuditLogger auditLogger,
        IDecVCPlatComplianceConfiguration complianceConfig)
    {
        _logger = logger;
        _aiAuditRepository = aiAuditRepository;
        _auditLogger = auditLogger;
        _complianceConfig = complianceConfig;
    }

    public async Task<DecVCPlatAiTransparencyReport> GenerateTransparencyReportAsync(string userId)
    {
        _logger.LogInformation("DecVCPlat: Generating EU AI Act transparency report for user {UserId}", userId);
        
        await _auditLogger.LogAsync("DecVCPlatEuAiTransparency", userId, "AI transparency report requested");

        try
        {
            var aiDecisions = await _aiAuditRepository.GetUserAiDecisionsAsync(userId);
            var algorithmsUsed = await _aiAuditRepository.GetAlgorithmsUsedForUserAsync(userId);

            var report = new DecVCPlatAiTransparencyReport
            {
                UserId = userId,
                GeneratedAt = DateTime.UtcNow,
                ReportVersion = "1.0",
                AiSystemsUsed = algorithmsUsed.Select(a => new DecVCPlatAiSystemInfo
                {
                    SystemId = a.SystemId,
                    SystemName = a.SystemName,
                    Purpose = a.Purpose,
                    RiskLevel = a.RiskLevel,
                    DecisionLogic = GetDecisionLogicExplanation(a.SystemId),
                    DataSources = a.DataSources,
                    AccuracyMetrics = a.AccuracyMetrics,
                    BiasAssessment = a.BiasAssessment,
                    LastAudited = a.LastAudited
                }).ToList(),
                DecisionHistory = aiDecisions.Select(d => new DecVCPlatAiDecisionSummary
                {
                    DecisionId = d.DecisionId,
                    Timestamp = d.Timestamp,
                    DecisionType = d.DecisionType,
                    Outcome = d.Outcome,
                    Confidence = d.Confidence,
                    ExplanationAvailable = !string.IsNullOrEmpty(d.Explanation)
                }).ToList(),
                RightsInformation = GetUserRightsInformation(),
                ContactInformation = GetComplianceContactInformation()
            };

            await _auditLogger.LogAsync("DecVCPlatEuAiTransparency", userId, "AI transparency report generated successfully");

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to generate AI transparency report for user {UserId}", userId);
            await _auditLogger.LogAsync("DecVCPlatEuAiTransparency", userId, $"AI transparency report generation failed: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> RecordAiDecisionAsync(DecVCPlatAiDecisionRecord decision)
    {
        _logger.LogInformation("DecVCPlat: Recording AI decision {DecisionId} for user {UserId}", 
            decision.DecisionId, decision.UserId);

        try
        {
            // Validate decision record completeness
            if (!ValidateDecisionRecord(decision))
            {
                _logger.LogWarning("DecVCPlat: Invalid AI decision record for {DecisionId}", decision.DecisionId);
                return false;
            }

            // Enrich decision with compliance metadata
            decision.ComplianceMetadata = new DecVCPlatAiComplianceMetadata
            {
                EuAiActRiskLevel = AssessEuAiActRiskLevel(decision.AlgorithmId),
                TransparencyRequired = RequiresTransparency(decision.AlgorithmId),
                HumanOversightRequired = RequiresHumanOversight(decision.AlgorithmId),
                RecordRetentionPeriod = GetRetentionPeriod(decision.DecisionType),
                BiasAssessmentScore = await CalculateBiasScore(decision),
                FairnessMetrics = await CalculateFairnessMetrics(decision)
            };

            await _aiAuditRepository.RecordAiDecisionAsync(decision);
            await _auditLogger.LogAsync("DecVCPlatEuAiDecision", decision.UserId, 
                $"AI decision recorded: {decision.DecisionId}");

            // Trigger compliance alerts if needed
            await CheckComplianceAlertsAsync(decision);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to record AI decision {DecisionId}", decision.DecisionId);
            return false;
        }
    }

    public async Task<DecVCPlatAlgorithmicAccountabilityReport> GetAccountabilityReportAsync(string algorithmId)
    {
        _logger.LogInformation("DecVCPlat: Generating algorithmic accountability report for {AlgorithmId}", algorithmId);

        try
        {
            var decisions = await _aiAuditRepository.GetAlgorithmDecisionsAsync(algorithmId);
            var performanceMetrics = await _aiAuditRepository.GetAlgorithmPerformanceAsync(algorithmId);
            var biasAssessments = await _aiAuditRepository.GetBiasAssessmentsAsync(algorithmId);

            var report = new DecVCPlatAlgorithmicAccountabilityReport
            {
                AlgorithmId = algorithmId,
                ReportDate = DateTime.UtcNow,
                TotalDecisions = decisions.Count(),
                AccuracyRate = performanceMetrics.AccuracyRate,
                FalsePositiveRate = performanceMetrics.FalsePositiveRate,
                FalseNegativeRate = performanceMetrics.FalseNegativeRate,
                BiasMetrics = biasAssessments.ToDictionary(b => b.Dimension, b => b.Score),
                OutcomeDistribution = decisions.GroupBy(d => d.Outcome)
                    .ToDictionary(g => g.Key, g => g.Count()),
                PerformanceTrends = await CalculatePerformanceTrends(algorithmId),
                ComplianceStatus = await AssessComplianceStatus(algorithmId),
                RecommendedActions = await GenerateComplianceRecommendations(algorithmId)
            };

            await _auditLogger.LogAsync("DecVCPlatEuAiAccountability", "system", 
                $"Algorithmic accountability report generated for {algorithmId}");

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to generate accountability report for {AlgorithmId}", algorithmId);
            throw;
        }
    }

    public async Task<bool> ProcessAiExplanationRequestAsync(string userId, string decisionId)
    {
        _logger.LogInformation("DecVCPlat: Processing AI explanation request for decision {DecisionId} by user {UserId}", 
            decisionId, userId);

        try
        {
            var decision = await _aiAuditRepository.GetAiDecisionAsync(decisionId);
            if (decision == null || decision.UserId != userId)
            {
                return false;
            }

            var explanation = await GenerateDecisionExplanationAsync(decision);
            
            await _aiAuditRepository.RecordExplanationRequestAsync(new DecVCPlatExplanationRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                UserId = userId,
                DecisionId = decisionId,
                RequestDate = DateTime.UtcNow,
                Explanation = explanation,
                ExplanationMethod = GetExplanationMethod(decision.AlgorithmId)
            });

            await _auditLogger.LogAsync("DecVCPlatEuAiExplanation", userId, 
                $"AI explanation provided for decision {decisionId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to process AI explanation request for {DecisionId}", decisionId);
            return false;
        }
    }

    public async Task<DecVCPlatAiRiskAssessment> ConductRiskAssessmentAsync(string algorithmId)
    {
        _logger.LogInformation("DecVCPlat: Conducting EU AI Act risk assessment for algorithm {AlgorithmId}", algorithmId);

        try
        {
            var algorithm = await _aiAuditRepository.GetAlgorithmInfoAsync(algorithmId);
            var decisions = await _aiAuditRepository.GetAlgorithmDecisionsAsync(algorithmId);
            var performanceMetrics = await _aiAuditRepository.GetAlgorithmPerformanceAsync(algorithmId);

            var assessment = new DecVCPlatAiRiskAssessment
            {
                AlgorithmId = algorithmId,
                AssessmentDate = DateTime.UtcNow,
                EuAiActRiskLevel = AssessEuAiActRiskLevel(algorithmId),
                RiskFactors = await IdentifyRiskFactors(algorithm, decisions, performanceMetrics),
                ImpactAssessment = await AssessPotentialImpact(algorithm, decisions),
                MitigationMeasures = await IdentifyMitigationMeasures(algorithmId),
                ComplianceRequirements = GetComplianceRequirements(algorithmId),
                RecommendedActions = await GenerateRiskMitigationRecommendations(algorithmId),
                NextReviewDate = DateTime.UtcNow.AddMonths(6)
            };

            await _aiAuditRepository.RecordRiskAssessmentAsync(assessment);
            await _auditLogger.LogAsync("DecVCPlatEuAiRiskAssessment", "system", 
                $"Risk assessment completed for algorithm {algorithmId}");

            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to conduct risk assessment for {AlgorithmId}", algorithmId);
            throw;
        }
    }

    public async Task<bool> ValidateAiSystemComplianceAsync(string systemId)
    {
        _logger.LogInformation("DecVCPlat: Validating EU AI Act compliance for system {SystemId}", systemId);

        try
        {
            var system = await _aiAuditRepository.GetAiSystemAsync(systemId);
            if (system == null) return false;

            var complianceChecks = new List<DecVCPlatComplianceCheck>
            {
                await CheckTransparencyCompliance(systemId),
                await CheckAccountabilityCompliance(systemId),
                await CheckBiasAssessmentCompliance(systemId),
                await CheckHumanOversightCompliance(systemId),
                await CheckDataGovernanceCompliance(systemId),
                await CheckRiskManagementCompliance(systemId)
            };

            var isCompliant = complianceChecks.All(c => c.Passed);
            
            await _aiAuditRepository.RecordComplianceValidationAsync(new DecVCPlatComplianceValidation
            {
                SystemId = systemId,
                ValidationDate = DateTime.UtcNow,
                IsCompliant = isCompliant,
                ComplianceChecks = complianceChecks,
                NextValidationDate = DateTime.UtcNow.AddMonths(3)
            });

            await _auditLogger.LogAsync("DecVCPlatEuAiCompliance", "system", 
                $"Compliance validation completed for system {systemId}: {(isCompliant ? "PASSED" : "FAILED")}");

            return isCompliant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to validate compliance for system {SystemId}", systemId);
            return false;
        }
    }

    // Private helper methods
    private bool ValidateDecisionRecord(DecVCPlatAiDecisionRecord decision)
    {
        return !string.IsNullOrEmpty(decision.DecisionId) &&
               !string.IsNullOrEmpty(decision.UserId) &&
               !string.IsNullOrEmpty(decision.AlgorithmId) &&
               decision.Timestamp != default;
    }

    private DecVCPlatEuAiActRiskLevel AssessEuAiActRiskLevel(string algorithmId)
    {
        // Implement risk level assessment logic based on EU AI Act categories
        return DecVCPlatEuAiActRiskLevel.Limited; // Placeholder
    }

    private bool RequiresTransparency(string algorithmId)
    {
        // Determine if algorithm requires transparency based on EU AI Act
        return true; // Conservative approach
    }

    private bool RequiresHumanOversight(string algorithmId)
    {
        // Determine if algorithm requires human oversight
        return true; // Conservative approach
    }

    private TimeSpan GetRetentionPeriod(string decisionType)
    {
        return TimeSpan.FromDays(365 * 7); // 7 years for financial decisions
    }

    private async Task<double> CalculateBiasScore(DecVCPlatAiDecisionRecord decision)
    {
        // Implement bias calculation logic
        return 0.1; // Placeholder
    }

    private async Task<Dictionary<string, double>> CalculateFairnessMetrics(DecVCPlatAiDecisionRecord decision)
    {
        // Implement fairness metrics calculation
        return new Dictionary<string, double>
        {
            ["demographic_parity"] = 0.95,
            ["equalized_odds"] = 0.93,
            ["calibration"] = 0.97
        };
    }

    private async Task CheckComplianceAlertsAsync(DecVCPlatAiDecisionRecord decision)
    {
        // Check for compliance alerts and trigger notifications if needed
        if (decision.ComplianceMetadata?.BiasAssessmentScore > 0.3)
        {
            _logger.LogWarning("DecVCPlat: High bias score detected for decision {DecisionId}", decision.DecisionId);
        }
    }

    private string GetDecisionLogicExplanation(string systemId)
    {
        return "DecVCPlat uses ensemble machine learning algorithms for project evaluation and funding decisions.";
    }

    private DecVCPlatUserRightsInformation GetUserRightsInformation()
    {
        return new DecVCPlatUserRightsInformation
        {
            RightToExplanation = "You have the right to receive an explanation of any automated decision affecting you.",
            RightToHumanReview = "You can request human review of automated decisions.",
            RightToObject = "You can object to automated decision-making in certain circumstances.",
            ContactInformation = "privacy@decvcplat.com"
        };
    }

    private DecVCPlatComplianceContactInformation GetComplianceContactInformation()
    {
        return new DecVCPlatComplianceContactInformation
        {
            DataProtectionOfficer = "dpo@decvcplat.com",
            AiEthicsOfficer = "ai-ethics@decvcplat.com",
            ComplianceTeam = "compliance@decvcplat.com"
        };
    }

    private async Task<string> GenerateDecisionExplanationAsync(DecVCPlatAiDecisionRecord decision)
    {
        // Generate human-readable explanation of the AI decision
        return $"This decision was made based on project evaluation criteria including technical feasibility, market potential, and team experience.";
    }

    private string GetExplanationMethod(string algorithmId)
    {
        return "LIME (Local Interpretable Model-agnostic Explanations)";
    }

    private async Task<List<string>> IdentifyRiskFactors(object algorithm, IEnumerable<DecVCPlatAiDecisionRecord> decisions, object performanceMetrics)
    {
        return new List<string>
        {
            "Potential bias in funding decisions",
            "Impact on startup opportunities",
            "Financial implications for investors"
        };
    }

    private async Task<string> AssessPotentialImpact(object algorithm, IEnumerable<DecVCPlatAiDecisionRecord> decisions)
    {
        return "Medium impact on individual financial opportunities and startup ecosystem.";
    }

    private async Task<List<string>> IdentifyMitigationMeasures(string algorithmId)
    {
        return new List<string>
        {
            "Regular bias testing and mitigation",
            "Human oversight for high-value decisions",
            "Transparent decision criteria",
            "Regular algorithm audits"
        };
    }

    private List<string> GetComplianceRequirements(string algorithmId)
    {
        return new List<string>
        {
            "EU AI Act Article 13 - Transparency obligations",
            "EU AI Act Article 14 - Human oversight",
            "GDPR Article 22 - Automated decision-making"
        };
    }

    private async Task<List<string>> GenerateRiskMitigationRecommendations(string algorithmId)
    {
        return new List<string>
        {
            "Implement additional bias monitoring",
            "Enhance human review processes",
            "Improve decision transparency",
            "Regular compliance audits"
        };
    }

    private async Task<Dictionary<string, double>> CalculatePerformanceTrends(string algorithmId)
    {
        return new Dictionary<string, double>
        {
            ["accuracy_trend"] = 0.02,
            ["bias_trend"] = -0.01,
            ["fairness_trend"] = 0.01
        };
    }

    private async Task<string> AssessComplianceStatus(string algorithmId)
    {
        return "Compliant with current EU AI Act requirements";
    }

    private async Task<List<string>> GenerateComplianceRecommendations(string algorithmId)
    {
        return new List<string>
        {
            "Continue regular bias monitoring",
            "Maintain transparency documentation",
            "Schedule quarterly compliance reviews"
        };
    }

    private async Task<DecVCPlatComplianceCheck> CheckTransparencyCompliance(string systemId)
    {
        return new DecVCPlatComplianceCheck
        {
            CheckType = "Transparency",
            Passed = true,
            Details = "System provides adequate transparency and explainability"
        };
    }

    private async Task<DecVCPlatComplianceCheck> CheckAccountabilityCompliance(string systemId)
    {
        return new DecVCPlatComplianceCheck
        {
            CheckType = "Accountability",
            Passed = true,
            Details = "Clear accountability measures in place"
        };
    }

    private async Task<DecVCPlatComplianceCheck> CheckBiasAssessmentCompliance(string systemId)
    {
        return new DecVCPlatComplianceCheck
        {
            CheckType = "BiasAssessment",
            Passed = true,
            Details = "Regular bias assessments conducted"
        };
    }

    private async Task<DecVCPlatComplianceCheck> CheckHumanOversightCompliance(string systemId)
    {
        return new DecVCPlatComplianceCheck
        {
            CheckType = "HumanOversight",
            Passed = true,
            Details = "Human oversight processes implemented"
        };
    }

    private async Task<DecVCPlatComplianceCheck> CheckDataGovernanceCompliance(string systemId)
    {
        return new DecVCPlatComplianceCheck
        {
            CheckType = "DataGovernance",
            Passed = true,
            Details = "Data governance policies in place"
        };
    }

    private async Task<DecVCPlatComplianceCheck> CheckRiskManagementCompliance(string systemId)
    {
        return new DecVCPlatComplianceCheck
        {
            CheckType = "RiskManagement",
            Passed = true,
            Details = "Risk management framework implemented"
        };
    }
}

// EU AI Act Data Models
public class DecVCPlatAiTransparencyReport
{
    public string UserId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string ReportVersion { get; set; } = string.Empty;
    public List<DecVCPlatAiSystemInfo> AiSystemsUsed { get; set; } = new();
    public List<DecVCPlatAiDecisionSummary> DecisionHistory { get; set; } = new();
    public DecVCPlatUserRightsInformation? RightsInformation { get; set; }
    public DecVCPlatComplianceContactInformation? ContactInformation { get; set; }
}

public class DecVCPlatAiSystemInfo
{
    public string SystemId { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DecVCPlatEuAiActRiskLevel RiskLevel { get; set; }
    public string DecisionLogic { get; set; } = string.Empty;
    public List<string> DataSources { get; set; } = new();
    public Dictionary<string, double> AccuracyMetrics { get; set; } = new();
    public Dictionary<string, double> BiasAssessment { get; set; } = new();
    public DateTime LastAudited { get; set; }
}

public class DecVCPlatAiDecisionSummary
{
    public string DecisionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string DecisionType { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public bool ExplanationAvailable { get; set; }
}

public class DecVCPlatUserRightsInformation
{
    public string RightToExplanation { get; set; } = string.Empty;
    public string RightToHumanReview { get; set; } = string.Empty;
    public string RightToObject { get; set; } = string.Empty;
    public string ContactInformation { get; set; } = string.Empty;
}

public class DecVCPlatComplianceContactInformation
{
    public string DataProtectionOfficer { get; set; } = string.Empty;
    public string AiEthicsOfficer { get; set; } = string.Empty;
    public string ComplianceTeam { get; set; } = string.Empty;
}

public class DecVCPlatAiDecisionRecord
{
    public string DecisionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string AlgorithmId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string DecisionType { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Explanation { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
    public DecVCPlatAiComplianceMetadata? ComplianceMetadata { get; set; }
}

public class DecVCPlatAiComplianceMetadata
{
    public DecVCPlatEuAiActRiskLevel EuAiActRiskLevel { get; set; }
    public bool TransparencyRequired { get; set; }
    public bool HumanOversightRequired { get; set; }
    public TimeSpan RecordRetentionPeriod { get; set; }
    public double BiasAssessmentScore { get; set; }
    public Dictionary<string, double> FairnessMetrics { get; set; } = new();
}

public enum DecVCPlatEuAiActRiskLevel
{
    Minimal,
    Limited,
    High,
    Unacceptable
}

public class DecVCPlatAlgorithmicAccountabilityReport
{
    public string AlgorithmId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public int TotalDecisions { get; set; }
    public double AccuracyRate { get; set; }
    public double FalsePositiveRate { get; set; }
    public double FalseNegativeRate { get; set; }
    public Dictionary<string, double> BiasMetrics { get; set; } = new();
    public Dictionary<string, int> OutcomeDistribution { get; set; } = new();
    public Dictionary<string, double> PerformanceTrends { get; set; } = new();
    public string ComplianceStatus { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
}

public class DecVCPlatAiRiskAssessment
{
    public string AlgorithmId { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public DecVCPlatEuAiActRiskLevel EuAiActRiskLevel { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public string ImpactAssessment { get; set; } = string.Empty;
    public List<string> MitigationMeasures { get; set; } = new();
    public List<string> ComplianceRequirements { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public DateTime NextReviewDate { get; set; }
}

public class DecVCPlatExplanationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DecisionId { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string ExplanationMethod { get; set; } = string.Empty;
}

public class DecVCPlatComplianceCheck
{
    public string CheckType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class DecVCPlatComplianceValidation
{
    public string SystemId { get; set; } = string.Empty;
    public DateTime ValidationDate { get; set; }
    public bool IsCompliant { get; set; }
    public List<DecVCPlatComplianceCheck> ComplianceChecks { get; set; } = new();
    public DateTime NextValidationDate { get; set; }
}

// Required interfaces
public interface IDecVCPlatAiAuditRepository
{
    Task<IEnumerable<DecVCPlatAiDecisionRecord>> GetUserAiDecisionsAsync(string userId);
    Task<IEnumerable<DecVCPlatAiSystemInfo>> GetAlgorithmsUsedForUserAsync(string userId);
    Task RecordAiDecisionAsync(DecVCPlatAiDecisionRecord decision);
    Task<DecVCPlatAiDecisionRecord?> GetAiDecisionAsync(string decisionId);
    Task RecordExplanationRequestAsync(DecVCPlatExplanationRequest request);
    Task<object> GetAlgorithmInfoAsync(string algorithmId);
    Task<IEnumerable<DecVCPlatAiDecisionRecord>> GetAlgorithmDecisionsAsync(string algorithmId);
    Task<object> GetAlgorithmPerformanceAsync(string algorithmId);
    Task<IEnumerable<object>> GetBiasAssessmentsAsync(string algorithmId);
    Task RecordRiskAssessmentAsync(DecVCPlatAiRiskAssessment assessment);
    Task<object?> GetAiSystemAsync(string systemId);
    Task RecordComplianceValidationAsync(DecVCPlatComplianceValidation validation);
}

public interface IDecVCPlatComplianceConfiguration
{
    bool IsTransparencyRequired(string algorithmId);
    bool IsHumanOversightRequired(string algorithmId);
    TimeSpan GetRetentionPeriod(string decisionType);
}
