namespace Momeants.Shared.Contracts;

public record ErrorDetail(string Field, string Message);

public record ErrorBody(string Code, string Message, List<ErrorDetail> Details);

public record ErrorEnvelope(ErrorBody Error)
{
    public static ErrorEnvelope From(string code, string message, List<ErrorDetail>? details = null)
        => new(new ErrorBody(code, message, details ?? new()));
}
