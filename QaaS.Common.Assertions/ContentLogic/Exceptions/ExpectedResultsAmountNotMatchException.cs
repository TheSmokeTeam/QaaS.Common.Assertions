namespace QaaS.Common.Assertions.ContentLogic.Exceptions;

public class ExpectedResultsAmountNotMatchException : Exception
{
    public ExpectedResultsAmountNotMatchException(int expectedResultsAmount, int outputsAmount)
        : base(
            $"The amount of expected results {expectedResultsAmount}) did not match the amount of outputs ({outputsAmount})")
    {
    }
}