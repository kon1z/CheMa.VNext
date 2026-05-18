using System;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformException : Exception
{
    public string ErrorCode { get; }

    public int StatusCode { get; }

    public OpenPlatformException(string errorCode, string message, int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
