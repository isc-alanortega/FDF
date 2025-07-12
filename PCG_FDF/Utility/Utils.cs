namespace PCG_FDF.Utility
{
    public static class Utils
    {
        // Taken from https://stackoverflow.com/questions/16063520/how-do-you-create-an-asynchronous-method-in-c
        public static Task<T> RunAsync<T>(Func<T> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            var tcs = new TaskCompletionSource<T>();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    T result = function();
                    tcs.SetResult(result);
                }
                catch (Exception exc) { tcs.SetException(exc); }
            });
            return tcs.Task;
        }
    }
}
