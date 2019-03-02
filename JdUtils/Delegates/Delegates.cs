namespace JdUtils.Delegates
{
    public delegate void LogHandler(string message, System.Exception exception);

    public delegate void ErrorHandler(System.Exception exception);
}
