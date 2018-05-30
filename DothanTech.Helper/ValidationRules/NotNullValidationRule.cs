using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Dothan.Helpers
{
    public class NotNullValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            
            if (string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, "NotNull");
    
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
