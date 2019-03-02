namespace JdUtils.Delegates
{
    public delegate void LogHandler(string message, System.Exception exception);

    public delegate void FailureHandler(System.Exception exception);
}
