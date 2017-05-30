using System;
using System.Linq;
using System.Collections.Generic;

namespace Lohmann.DotEnv
{
    /// <summary>
    /// ValidationResult of the EnvValidator.
    /// </summary>
    public class EnvValidatorResult
    {
        public EnvValidatorResult(IEnumerable<string> validationErrors)
        {
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }

        public bool HasValidationErrors => ValidationErrors.Count > 0;

        public IReadOnlyCollection<string> ValidationErrors { get; private set; }
    }
}