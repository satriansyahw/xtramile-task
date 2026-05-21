using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherApp.Shared.Results;

public class Result
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? Errors { get; set; }

    public Result() { }

    public static Result Success() => new Result { IsSuccess = true };
    
    public static Result Failure(string errorMessage, IEnumerable<string>? errors = null) 
        => new Result { IsSuccess = false, ErrorMessage = errorMessage, Errors = errors };
}

public class Result<T> : Result
{
    public T? Value { get; set; }

    public Result() { }

    public static Result<T> Success(T value) 
        => new Result<T> { IsSuccess = true, Value = value };

    public new static Result<T> Failure(string errorMessage, IEnumerable<string>? errors = null) 
        => new Result<T> { IsSuccess = false, ErrorMessage = errorMessage, Errors = errors };
}
