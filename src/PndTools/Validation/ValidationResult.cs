namespace PndTools.Validation;

/// <summary>The result of a PXML validation operation.</summary>
public sealed class ValidationResult
{
    /// <summary>Initialises a new <see cref="ValidationResult"/>.</summary>
    /// <param name="errors">The validation errors, if any.</param>
    public ValidationResult(IEnumerable<string>? errors = null)
    {
        Errors = errors?.ToList() ?? [];
        IsValid = Errors.Count == 0;
    }

    /// <summary><c>true</c> if the PXML contains no validation errors; otherwise <c>false</c>.</summary>
    public bool IsValid { get; }

    /// <summary>The validation error messages. Empty when <see cref="IsValid"/> is <c>true</c>.</summary>
    public IReadOnlyList<string> Errors { get; }
}
