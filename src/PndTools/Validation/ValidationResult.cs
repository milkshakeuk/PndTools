using System.Collections.Generic;
using System.Linq;

namespace PndTools.Validation
{
    public class ValidationResult
    {
        public ValidationResult(IEnumerable<string> errors = null)
        {
            this.IsValid = !errors?.Any() ?? errors == null;
            this.Errors = errors ?? new List<string>();
        }

        public bool IsValid { get; }

        public IEnumerable<string> Errors { get; }
    }
}
