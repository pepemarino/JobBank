namespace JobBank.Validation
{
    using System.ComponentModel.DataAnnotations;

    public class RequiredIfOtherPropertyHasValueAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;

        public RequiredIfOtherPropertyHasValueAttribute(string dependentProperty)
        {
            _dependentProperty = dependentProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var dependentPropertyValue = instance.GetType().GetProperty(_dependentProperty)?.GetValue(instance, null);

            // if the dependent property is provided (not null/empty)
            // then the current property is required.
            if (dependentPropertyValue != null && !string.IsNullOrWhiteSpace(dependentPropertyValue.ToString()))
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    // Return a validation error if the condition is met and the current property is empty
                    return new ValidationResult(ErrorMessage ?? 
                        $"{validationContext.DisplayName} is required when {_dependentProperty} is provided.", new[] { validationContext.MemberName! });
                }
            }

            // Return success if the condition isn't met or if the value is valid
            return ValidationResult.Success;
        }
    }

}
