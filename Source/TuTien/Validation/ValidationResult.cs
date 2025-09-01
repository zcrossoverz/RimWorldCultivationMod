using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TuTien.Validation
{
    /// <summary>
    /// Result of a validation operation containing errors, warnings, and info messages
    /// </summary>
    public class ValidationResult
    {
        #region Properties
        
        /// <summary>List of validation errors</summary>
        public List<ValidationMessage> Errors { get; private set; } = new List<ValidationMessage>();
        
        /// <summary>List of validation warnings</summary>
        public List<ValidationMessage> Warnings { get; private set; } = new List<ValidationMessage>();
        
        /// <summary>List of informational messages</summary>
        public List<ValidationMessage> InfoMessages { get; private set; } = new List<ValidationMessage>();
        
        /// <summary>True if validation passed (no errors)</summary>
        public bool IsValid => Errors.Count == 0;
        
        /// <summary>True if there are any warnings</summary>
        public bool HasWarnings => Warnings.Count > 0;
        
        /// <summary>True if there are any info messages</summary>
        public bool HasInfo => InfoMessages.Count > 0;
        
        /// <summary>Total number of messages</summary>
        public int TotalMessages => Errors.Count + Warnings.Count + InfoMessages.Count;
        
        #endregion
        
        #region Add Messages
        
        /// <summary>Add an error message</summary>
        public void AddError(ValidationErrorType type, string message, object context = null)
        {
            Errors.Add(new ValidationMessage(ValidationSeverity.Error, type.ToString(), message, context));
        }
        
        /// <summary>Add a warning message</summary>
        public void AddWarning(ValidationWarningType type, string message, object context = null)
        {
            Warnings.Add(new ValidationMessage(ValidationSeverity.Warning, type.ToString(), message, context));
        }
        
        /// <summary>Add an informational message</summary>
        public void AddInfo(ValidationInfoType type, string message, object context = null)
        {
            InfoMessages.Add(new ValidationMessage(ValidationSeverity.Info, type.ToString(), message, context));
        }
        
        #endregion
        
        #region Merge and Query
        
        /// <summary>Merge another validation result into this one</summary>
        public void MergeWith(ValidationResult other)
        {
            if (other == null) return;
            
            Errors.AddRange(other.Errors);
            Warnings.AddRange(other.Warnings);
            InfoMessages.AddRange(other.InfoMessages);
        }
        
        /// <summary>Get all messages of a specific type</summary>
        public IEnumerable<ValidationMessage> GetMessagesOfType(ValidationErrorType type)
        {
            return Errors.Where(e => e.Type == type.ToString());
        }
        
        /// <summary>Get all messages of a specific type</summary>
        public IEnumerable<ValidationMessage> GetMessagesOfType(ValidationWarningType type)
        {
            return Warnings.Where(w => w.Type == type.ToString());
        }
        
        /// <summary>Get all messages with context matching a predicate</summary>
        public IEnumerable<ValidationMessage> GetMessagesWithContext<T>(Func<T, bool> predicate) where T : class
        {
            return GetAllMessages().Where(m => m.Context is T context && predicate(context));
        }
        
        /// <summary>Get all messages regardless of severity</summary>
        public IEnumerable<ValidationMessage> GetAllMessages()
        {
            return Errors.Cast<ValidationMessage>()
                .Concat(Warnings.Cast<ValidationMessage>())
                .Concat(InfoMessages.Cast<ValidationMessage>());
        }
        
        /// <summary>Check if contains error of specific type</summary>
        public bool HasErrorOfType(ValidationErrorType type)
        {
            return Errors.Any(e => e.Type == type.ToString());
        }
        
        /// <summary>Check if contains warning of specific type</summary>
        public bool HasWarningOfType(ValidationWarningType type)
        {
            return Warnings.Any(w => w.Type == type.ToString());
        }
        
        #endregion
        
        #region Formatting
        
        /// <summary>Get a formatted summary of all validation results</summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();
            
            if (Errors.Count > 0)
            {
                sb.AppendLine($"❌ {Errors.Count} Error(s):");
                foreach (var error in Errors)
                {
                    sb.AppendLine($"  • {error.Message}");
                }
            }
            
            if (Warnings.Count > 0)
            {
                sb.AppendLine($"⚠️ {Warnings.Count} Warning(s):");
                foreach (var warning in Warnings)
                {
                    sb.AppendLine($"  • {warning.Message}");
                }
            }
            
            if (InfoMessages.Count > 0)
            {
                sb.AppendLine($"ℹ️ {InfoMessages.Count} Info:");
                foreach (var info in InfoMessages)
                {
                    sb.AppendLine($"  • {info.Message}");
                }
            }
            
            if (TotalMessages == 0)
            {
                sb.AppendLine("✅ Validation passed with no issues");
            }
            
            return sb.ToString();
        }
        
        /// <summary>Get errors only as formatted string</summary>
        public string GetErrorsOnly()
        {
            if (Errors.Count == 0) return string.Empty;
            
            var sb = new StringBuilder();
            foreach (var error in Errors)
            {
                sb.AppendLine($"❌ {error.Message}");
            }
            return sb.ToString();
        }
        
        /// <summary>Get warnings only as formatted string</summary>
        public string GetWarningsOnly()
        {
            if (Warnings.Count == 0) return string.Empty;
            
            var sb = new StringBuilder();
            foreach (var warning in Warnings)
            {
                sb.AppendLine($"⚠️ {warning.Message}");
            }
            return sb.ToString();
        }
        
        /// <summary>Get info messages only as formatted string</summary>
        public string GetInfoOnly()
        {
            if (InfoMessages.Count == 0) return string.Empty;
            
            var sb = new StringBuilder();
            foreach (var info in InfoMessages)
            {
                sb.AppendLine($"ℹ️ {info.Message}");
            }
            return sb.ToString();
        }
        
        /// <summary>Get one-line status summary</summary>
        public string GetStatusLine()
        {
            if (!IsValid)
            {
                return $"❌ Failed: {Errors.Count} errors, {Warnings.Count} warnings";
            }
            else if (HasWarnings)
            {
                return $"⚠️ Passed with warnings: {Warnings.Count} warnings";
            }
            else
            {
                return "✅ Validation passed";
            }
        }
        
        #endregion
        
        #region Static Helpers
        
        /// <summary>Create a successful validation result</summary>
        public static ValidationResult Success()
        {
            return new ValidationResult();
        }
        
        /// <summary>Create a failed validation result with error</summary>
        public static ValidationResult Error(ValidationErrorType type, string message, object context = null)
        {
            var result = new ValidationResult();
            result.AddError(type, message, context);
            return result;
        }
        
        /// <summary>Create a validation result with warning</summary>
        public static ValidationResult Warning(ValidationWarningType type, string message, object context = null)
        {
            var result = new ValidationResult();
            result.AddWarning(type, message, context);
            return result;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Individual validation message
    /// </summary>
    public class ValidationMessage
    {
        /// <summary>Severity of the message</summary>
        public ValidationSeverity Severity { get; set; }
        
        /// <summary>Type/category of the message</summary>
        public string Type { get; set; }
        
        /// <summary>Human-readable message</summary>
        public string Message { get; set; }
        
        /// <summary>Optional context object related to the message</summary>
        public object Context { get; set; }
        
        /// <summary>Timestamp when message was created</summary>
        public DateTime Timestamp { get; set; }
        
        public ValidationMessage(ValidationSeverity severity, string type, string message, object context = null)
        {
            Severity = severity;
            Type = type;
            Message = message;
            Context = context;
            Timestamp = DateTime.Now;
        }
        
        public override string ToString()
        {
            string severityIcon = Severity switch
            {
                ValidationSeverity.Error => "❌",
                ValidationSeverity.Warning => "⚠️",
                ValidationSeverity.Info => "ℹ️",
                _ => "•"
            };
            
            return $"{severityIcon} [{Type}] {Message}";
        }
    }
    
    /// <summary>Severity levels for validation messages</summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
    
    /// <summary>Types of validation errors</summary>
    public enum ValidationErrorType
    {
        NullReference,
        InvalidState,
        InvalidData,
        MissingComponent,
        RequirementNotMet,
        PrerequisiteNotMet,
        InsufficientResources,
        MaximumReached,
        SkillUsageFailed,
        TechniquePracticeFailed,
        OnCooldown,
        InvalidReference,
        DataCorruption
    }
    
    /// <summary>Types of validation warnings</summary>
    public enum ValidationWarningType
    {
        ReducedCapability,
        DataInconsistency,
        SuboptimalConditions,
        AutoCultivationRestricted,
        InsufficientResources,
        MaximumReached,
        MissingComponent,
        OrphanedData,
        PerformanceIssue,
        DeprecatedFeature
    }
    
    /// <summary>Types of informational validation messages</summary>
    public enum ValidationInfoType
    {
        RealmAdvancement,
        SkillUnlocked,
        TechniqueDiscovered,
        OptimizationSuggestion,
        SystemStatus
    }
}
