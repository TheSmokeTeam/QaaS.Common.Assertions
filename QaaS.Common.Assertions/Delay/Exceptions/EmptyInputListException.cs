namespace QaaS.Common.Assertions.Delay.Exceptions;

/// <summary>
/// Exception raised when a list of input injected to a component is empty and shouldn't be
/// </summary>
public class EmptyInputListException: Exception
{
    /// <summary>
    /// Exception raised when a list of input injected to a component is empty and shouldn't be
    /// </summary>
    /// <param name="exceptionMessage">the exception message that will be thrown when raising the exception</param>
    public EmptyInputListException(string exceptionMessage):base(exceptionMessage)
    {
            
    }
}