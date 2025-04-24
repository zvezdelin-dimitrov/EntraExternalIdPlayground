namespace MsalClientLib;

public static class Exception<TException> where TException : Exception, new()
{
    public static void ThrowOn(Func<bool> predicate, string message = null)
    {
        if (predicate())
        {
            TException toThrow = Activator.CreateInstance(typeof(TException), message) as TException;
            throw toThrow;
        }
    }
}
