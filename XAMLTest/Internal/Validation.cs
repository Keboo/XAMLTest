﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace XamlTest.Internal;

internal static class Validation
{
    internal static readonly DependencyProperty ErrorProperty = DependencyProperty.RegisterAttached(
        $"Error-{Guid.NewGuid()}", typeof(object), typeof(Validation), new PropertyMetadata(default(object?)));

    internal class Rule : ValidationRule
    {
        private object ErrorContent { get; }

        public Rule(object errorContent)
        {
            ErrorContent = errorContent;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(false, ErrorContent);
        }
    }
}
