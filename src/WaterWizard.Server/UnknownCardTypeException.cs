namespace WaterWizard.Server;

public class UnknownCardTypeException : Exception
{
    public UnknownCardTypeException() { }

    public UnknownCardTypeException(string? message)
        : base(message) { }
}
