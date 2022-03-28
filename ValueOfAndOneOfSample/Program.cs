using static System.Console;
using LanguageExt;
using OneOf;
using ValueOf;

Write($"[Using {nameof(ValidateAndPrintAsync)}] Type something in please: ");
var result = await ValidateAndPrintAsync(ReadLine());
result.Switch(
    unit => WriteLine("Print is successfull"),
    valueIsNotValid => WriteLine($"The value '{valueIsNotValid.Value}' was not valid for an unknown reason."),
    valueLengthIsNotValid => WriteLine($"The value '{valueLengthIsNotValid.Value}' was not valid. It must be at least {valueLengthIsNotValid.MinLength} characters and a maximum of {valueLengthIsNotValid.MaxLength} characters.")
);

Write($"[Using {nameof(ValidateAndPrintUsingPrintValueAsync)}] Type something else in please: ");
try
{
    var printValue = PrintValue.From(ReadLine() ?? string.Empty);
    await ValidateAndPrintUsingPrintValueAsync(printValue);
    WriteLine("Print is successfull");
}
catch (ValueIsNotValidException valueIsNotValidException)
{
    WriteLine($"The value '{valueIsNotValidException.Value}' was not valid for an unknown reason.");
}
catch (ValueLengthIsNotValidException valueLengthIsNotValidException)
{
    WriteLine($"The value '{valueLengthIsNotValidException.Value}' was not valid. It must be at least {valueLengthIsNotValidException.MinLength} characters and a maximum of {valueLengthIsNotValidException.MaxLength} characters.");
}

async Task<OneOf<Unit, ValueIsNotValid, ValueLengthIsNotValid>> ValidateAndPrintAsync(string? value)
{
    if (value.IsNullOrEmpty()) return new ValueIsNotValid();

    var minLength = Constants.MinLength;
    var maxLength = Constants.MaxLength;
    if (!value!.LengthBetween(minLength, maxLength)) return new ValueLengthIsNotValid(value!, minLength, maxLength);

    WriteLine("Waiting to print...");
    await Task.Delay(2000);

    WriteLine(value);

    return Unit.Default;
}

async Task<Unit> ValidateAndPrintUsingPrintValueAsync(PrintValue printValue)
{
    WriteLine("Waiting to print...");
    await Task.Delay(2000);

    WriteLine(printValue.Value);

    return Unit.Default;
}


public static class Constants
{
    public static int MinLength { get; } = 5;
    public static int MaxLength { get; } = 25;
}


record struct ValueIsNotValid(string Value);
record struct ValueLengthIsNotValid(string Value, int MinLength, int MaxLength);

[Serializable]
public class ValueIsNotValidException : Exception
{
    public ValueIsNotValidException(string value) { Value = value; }
    public ValueIsNotValidException(string value, string message) : base(message) { Value = value; }
    public ValueIsNotValidException(string value, string message, Exception inner) : base(message, inner) { Value = value; }
    protected ValueIsNotValidException(
      string value,
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { Value = value; }

    public string Value { get; }
}


[Serializable]
public class ValueLengthIsNotValidException : Exception
{
#pragma warning disable CS8618 // I'm setting the properties inside SetProperties method.
    public ValueLengthIsNotValidException(string value, int minLength, int maxLength) { SetProperties(value, minLength, maxLength); }
    public ValueLengthIsNotValidException(string value, int minLength, int maxLength, string message) : base(message) { SetProperties(value, minLength, maxLength); }
    public ValueLengthIsNotValidException(string value, int minLength, int maxLength, string message, Exception inner) : base(message, inner) { SetProperties(value, minLength, maxLength); }
    protected ValueLengthIsNotValidException(
      string value, int minLength, int maxLength,
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { SetProperties(value, minLength, maxLength); }
#pragma warning restore CS8618 

    private void SetProperties(string value, int minLength, int maxLength)
    {
        Value = value;
        MinLength = minLength;
        MaxLength = maxLength;
    }
    public string Value { get; private set; }
    public int MinLength { get; private set; }
    public int MaxLength { get; private set; }
}

class PrintValue : ValueOf<string, PrintValue>
{
    protected override bool TryValidate()
    {
        return ValidateFunctionalWay().Match(_ => true, _ => false, _ => false);
    }

    protected override void Validate()
    {
        ValidateFunctionalWay().Switch(
            unit => { },
            valueIsNotValid => throw new ValueIsNotValidException(valueIsNotValid.Value),
            valueLengthIsNotValid => throw new ValueLengthIsNotValidException(valueLengthIsNotValid.Value, valueLengthIsNotValid.MinLength, valueLengthIsNotValid.MaxLength)
        );
    }

    private OneOf<Unit, ValueIsNotValid, ValueLengthIsNotValid> ValidateFunctionalWay()
    {
        if (Value.IsNullOrEmpty()) return new ValueIsNotValid();

        var minLength = Constants.MinLength;
        var maxLength = Constants.MaxLength;
        if (!Value!.LengthBetween(minLength, maxLength)) return new ValueLengthIsNotValid(Value, minLength, maxLength);

        return Unit.Default;
    }
}

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
    public static bool LengthBetween(this string value, int min, int max) => value.Length >= min && value.Length <= max;
}