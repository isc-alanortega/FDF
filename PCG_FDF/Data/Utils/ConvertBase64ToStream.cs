namespace PCG_FDF.Data.Utils
{
    public static class ConvertBase64ToStream
    {
        public static Stream? TryConvert(string base64)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                return new MemoryStream(bytes);
            }
            catch (Exception)
            {
                return Stream.Null;
            }
        }
    }
}
