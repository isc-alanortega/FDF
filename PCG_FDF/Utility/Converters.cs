namespace PCG_FDF.Utility
{
    public static class Converters
    {
        public static decimal? CelsiusToFahrenheit(decimal? celsius)
        {
            if (celsius.HasValue)
            {
                decimal fahrenheit = (celsius.Value * 9 / 5) + 32;
                return Math.Round(fahrenheit, 2, MidpointRounding.ToEven);
            }
            return null;
        }

        public static decimal? FahrenheitToCelsius(decimal? fahrenheit)
        {
            if (fahrenheit.HasValue)
            {
                decimal celsius = (fahrenheit.Value - 32) * 5 / 9;
                return Math.Round(celsius, 2, MidpointRounding.ToEven);
            }
            return null;
        }
    }
}
