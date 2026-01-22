namespace QaaS.Common.Assertions.ContentLogic.FieldValidation.Validators;

public interface IFieldValidator
{
    /// <summary>
    /// A validation method that receives a field's value and its expected value and validates it according to a specific type of validation
    /// </summary>
    /// <param name="fieldValue">The field's value</param>
    /// <param name="expectedValue">The field's expected value</param>
    /// <returns>Is value valid according to the validation type</returns>
    public bool Validate(object? fieldValue, object? expectedValue);
}