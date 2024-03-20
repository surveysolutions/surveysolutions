using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class NumericTextFormatter
    {
        private const int maxDigitsInDecimal = 28;
        private const int maxFractionDigits = 15;
        private const string nonLocalizedAndroidDecimalSeparator = ".";

        private readonly Regex numberFormatRegex = new Regex(@"\B(?=(\d{3})+(?!\d))", RegexOptions.None);
        private readonly NumericTextFormatterSettings settings;
        private readonly string allowedSymbols;

        public NumericTextFormatter(NumericTextFormatterSettings settings)
        {
            this.settings = settings;

            this.allowedSymbols = $"0123456789{this.settings.NegativeSign}";
            if (this.settings.IsDecimal)
            {
                allowedSymbols += $"{nonLocalizedAndroidDecimalSeparator}{this.settings.DecimalSeparator}";
            }
        }

        public string FilterFormatted(string addedText, string sourceText, int insertToIndex)
        {
            var hasNonLocalizedAndroidDecimalSeparator = addedText == nonLocalizedAndroidDecimalSeparator && this.settings.DecimalSeparator != nonLocalizedAndroidDecimalSeparator;
            var hasDecimalSeperatorInInteger = (addedText == this.settings.DecimalSeparator || hasNonLocalizedAndroidDecimalSeparator) && !this.settings.IsDecimal;

            if (hasDecimalSeperatorInInteger || !addedText.ToCharArray().All(x => this.allowedSymbols.ToCharArray().Contains(x)))
            {
                return "";
            }

            if (hasNonLocalizedAndroidDecimalSeparator) addedText = this.settings.DecimalSeparator;

            var enteredText = sourceText.Insert(insertToIndex, addedText);

            var hasTextNegativeSign = enteredText.StartsWith(this.settings.NegativeSign);
            string textWithoutSign = hasTextNegativeSign ? enteredText.Length == 1 ? "" : enteredText.Substring(1, enteredText.Length - 1) : enteredText;

            string[] integerAndFraction = textWithoutSign.Split(this.settings.DecimalSeparator.ToCharArray());
            string integer = (integerAndFraction[0] ?? "").Replace(this.settings.GroupingSeparator, "");
            string fraction = integerAndFraction.Length > 1 ? integerAndFraction[1] ?? "" : "";

            var verifiers = new Func<bool>[]
            {
                () =>
                {
                    var countOfNegativeSigns = this.GetSubstringsCount(enteredText, this.settings.NegativeSign);
                    return countOfNegativeSigns > 1 || (countOfNegativeSigns == 1 && !hasTextNegativeSign);
                },
                () => enteredText.StartsWith(this.settings.DecimalSeparator) || enteredText.StartsWith(nonLocalizedAndroidDecimalSeparator),
                () =>
                {
                    int decimalPointPosition = enteredText.IndexOf((string) this.settings.DecimalSeparator);

                    if (this.settings.MaxDigitsAfterDecimal == 0 && decimalPointPosition > 0)
                        return true;
                    
                    return decimalPointPosition > 0 && textWithoutSign.Substring(decimalPointPosition).IndexOf((string) this.settings.GroupingSeparator) > 0;
                },
                () => textWithoutSign.Length == 1 && textWithoutSign == this.settings.DecimalSeparator,
                () => this.GetSubstringsCount(enteredText, this.settings.DecimalSeparator) > 1,
                () => fraction.Length > maxFractionDigits,
                () => (integer.Length + fraction.Length) > maxDigitsInDecimal,
                () => this.settings.MaxDigitsBeforeDecimal > 0 && integer.Length > this.settings.MaxDigitsBeforeDecimal,
                () => this.settings.MaxDigitsAfterDecimal >= 0 && fraction.Length > this.settings.MaxDigitsAfterDecimal,
                () =>
                {
                    if (textWithoutSign.Length <= 2) return false;

                    string lastChar = textWithoutSign[textWithoutSign.Length - 1].ToString();
                    string secToLastChar = textWithoutSign[textWithoutSign.Length - 2].ToString();
                    if (lastChar != this.settings.DecimalSeparator && lastChar != this.settings.GroupingSeparator) return false;

                    return lastChar == secToLastChar;
                }
            };

            return verifiers.Any(isInvalid => isInvalid()) ? "" : hasNonLocalizedAndroidDecimalSeparator ? this.settings.DecimalSeparator : null;
        }

        private int GetSubstringsCount(string text, string substring)
        {
            return (text ?? "").Split(substring.ToCharArray()).Length - 1;
        }

        public string Format(string numberAsText)
        {
            if (!this.settings.UseGroupSeparator) return numberAsText;;

            numberAsText = numberAsText.Replace(this.settings.GroupingSeparator, "");

            var integerAndFraction = numberAsText.Split(this.settings.DecimalSeparator.ToCharArray());
            integerAndFraction[0] = this.numberFormatRegex.Replace(integerAndFraction[0], this.settings.GroupingSeparator);

            return string.Join(this.settings.DecimalSeparator, integerAndFraction);
        }
        
        public static string FormatBytesHumanized(double bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            double bytesAsDouble = bytes;

            return FormatNumberOrSpeed(bytesAsDouble, suffixes);
        }

        public static string FormatSpeedHumanized(double bytes, TimeSpan elapsed)
        {
            string[] suffixes = { "B/s", "KB/s", "MB/s", "GB/s", "TB/s", "PB/s" };
            double bytesAsDouble = bytes / elapsed.TotalSeconds;

            return FormatNumberOrSpeed(bytesAsDouble, suffixes);
        }

        private static string FormatNumberOrSpeed(double bytesAsDouble, string[] suffixes)
        {
            int suffixIndex = 0;
            while (bytesAsDouble >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                bytesAsDouble /= 1024;
                suffixIndex++;
            }

            return $"{bytesAsDouble:0.##} {suffixes[suffixIndex]}";
        }

        public static string FormatTimeHumanized(TimeSpan time, CultureInfo culture = null)
        {
            if (time.TotalSeconds < 1)
            {
                return "less than a second";
            }
            
            var days = time.Days;
            var hours = time.Hours;
            var minutes = time.Minutes;
            var secondsLeft = time.Seconds;

            var result = "";
            if (days > 0)
            {
                result += days + " day" + (days > 1 ? "s" : "") + " ";
            }
            if (hours > 0)
            {
                result += hours + " hour" + (hours > 1 ? "s" : "") + " ";
            }
            if (minutes > 0)
            {
                result += minutes + " minute" + (minutes > 1 ? "s" : "") + " ";
            }
            if (secondsLeft > 0)
            {
                result += secondsLeft + " second" + (secondsLeft > 1 ? "s" : "") + " ";
            }
            return result.Trim();
        }
    }
}
