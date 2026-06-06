namespace JobBank.Validation
{
    using System.ComponentModel.DataAnnotations;

    public class RequiredIfOtherPropertyHasValueAttribute : ValidationAttribute
    {
        private readonly string[] _dependentProperty;

        public RequiredIfOtherPropertyHasValueAttribute(string[] dependentProperty)
        {
            _dependentProperty = dependentProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;

            foreach (var dependentProp in _dependentProperty)
            {
                var dependentPropertyValue = instance.GetType().GetProperty(dependentProp)?.GetValue(instance, null);
                // if any of the dependent properties is provided (not null/empty)
                // then the current property is required.
                if (dependentPropertyValue != null && !string.IsNullOrWhiteSpace(dependentPropertyValue.ToString()))
                {
                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                    {
                        // Return a validation error if the condition is met and the current property is empty
                        return new ValidationResult(ErrorMessage ?? 
                            $"{validationContext.DisplayName} is required when {dependentProp} is provided.", new[] { validationContext.MemberName! });
                    }
                }
            }

            // Return success if the condition isn't met or if the value is valid
            return ValidationResult.Success;
        }
    }

}
