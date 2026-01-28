namespace Core;

public class Result<TResult>
{
    public bool IsSuccess { get; }
    public TResult? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, TResult? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TResult> Success(TResult value)
    {
        return new Result<TResult>(true, value, null);
    }

    public static Result<TResult> Failure(string code, string message)
    {
        return new Result<TResult>(false, default, new Error(code, message));
    }
}

public class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
}