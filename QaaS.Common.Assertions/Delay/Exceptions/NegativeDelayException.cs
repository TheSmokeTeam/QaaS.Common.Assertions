namespace QaaS.Common.Assertions.Delay.Exceptions;

/// <summary>
/// Negative delay received after subtracting output timestamp by input timestamp
/// can be caused by incorrect output time stamp or incorrect input timestamp
/// </summary>
public class NegativeDelayException: Exception
{
    /// <summary>
    /// Negative delay received after subtracting output timestamp by input timestamp
    /// can be caused by incorrect output time stamp or incorrect input timestamp
    /// </summary>
    /// <param name="exceptionMessage">the exception message that will be thrown when raising the exception</param>
    public NegativeDelayException(string exceptionMessage):base(exceptionMessage)
    {
            
    }
}