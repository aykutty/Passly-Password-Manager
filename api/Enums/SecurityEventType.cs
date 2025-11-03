namespace Passly.Enums;

public enum SecurityEventType
{
    LoginFailed = 1,
    AccountLocked = 2,
    SuspiciousLoginAttempt = 3, 
    TokenReuse = 4,
    LoginOtpRequested = 5
}