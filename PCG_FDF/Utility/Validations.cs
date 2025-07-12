using System.Text.RegularExpressions;

namespace PCG_FDF.Utility
{
    public static class Validations
    {
        private static IDictionary<char, int> ISO6346_Charmap = new Dictionary<char, int>()
        {
            { 'A', 10 },
            { 'B', 12 },
            { 'C', 13 },
            { 'D', 14 },
            { 'E', 15 },
            { 'F', 16 },
            { 'G', 17 },
            { 'H', 18 },
            { 'I', 19 },
            { 'J', 20 },
            { 'K', 21 },
            { 'L', 23 },
            { 'M', 24 },
            { 'N', 25 },
            { 'O', 26 },
            { 'P', 27 },
            { 'Q', 28 },
            { 'R', 29 },
            { 'S', 30 },
            { 'T', 31 },
            { 'U', 32 },
            { 'V', 34 },
            { 'W', 35 },
            { 'X', 36 },
            { 'Y', 37 },
            { 'Z', 38 }
        };

        public static readonly string Single_Email_REGEX = @"^(?!.*\.\.)[^`'""\x00-\x1F\x7F-\xFF@]{1,254}@[A-Za-z0-9.-]{1,63}\.[A-Za-z]{2,63}$";

        public static bool IsValidContainerSerial(string container_serial)
        {
            try
            {

                var digits = container_serial.Substring(0, 10)
                    .Select(character => char.IsDigit(character) ? int.Parse(character.ToString()) : ISO6346_Charmap[character])
                    .ToArray();

                var checkDigit = int.Parse(container_serial[10].ToString());

                var sum = digits
                                .Select((digit, idx) => digit * (int)Math.Pow(2, idx))
                                .Sum();

                var remainder = Math.Floor((double)sum / 11);

                var multiplied = remainder * 11;

                var calculated_check = sum - multiplied;

                return calculated_check == 10 ? checkDigit == 0 : checkDigit == calculated_check;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ValidateString(string value, string pattern)
        {
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            return regex.IsMatch(value);
        }

    }
}
