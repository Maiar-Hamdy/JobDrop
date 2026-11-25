namespace JobDrop.Domain.Common;

public class DomainRuleException(string message) : Exception(message);
public class NotFoundException(string message) : Exception(message);
public class ConcurrencyConflictException(string message) : Exception(message);