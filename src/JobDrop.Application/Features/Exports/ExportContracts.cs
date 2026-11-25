using MediatR;
namespace JobDrop.Application.Features.Exports;

public sealed record RequestExportCmd(string Kind) : IRequest<Guid>;
public sealed record GetExportStatusQuery(Guid Id) : IRequest<ExportStatusDto>;
public sealed record ExportStatusDto(Guid Id, string Status, string? StorageKey, DateTime? ExpiresAtUtc);