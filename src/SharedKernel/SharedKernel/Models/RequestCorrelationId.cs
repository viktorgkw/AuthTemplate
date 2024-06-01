namespace SharedKernel.Models;

public class RequestCorrelationId
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
