namespace JobDrop.Application.Features.Messaging;

public sealed record ConversationDto(Guid Id, Guid? ApplicationId, Guid? ReverseDropId, Guid CompanyId, DateTime LastMessageAtUtc);
public sealed record MessageDto(Guid Id, Guid ConversationId, Guid SenderUserId, string Body, DateTime CreatedAtUtc);
public sealed record SendMessageCmd(Guid ConversationId, string Body) : MediatR.IRequest<Guid>;
public sealed record ListConversationsQuery : MediatR.IRequest<IReadOnlyList<ConversationDto>>;
public sealed record ListMessagesQuery(Guid ConversationId) : MediatR.IRequest<IReadOnlyList<MessageDto>>;
public sealed record OpenConversationForApplicationCmd(Guid ApplicationId) : MediatR.IRequest<Guid>;
public sealed record ProposeInterviewCmd(Guid ConversationId, string CallType, string[] SlotsIso) : MediatR.IRequest<Guid>;