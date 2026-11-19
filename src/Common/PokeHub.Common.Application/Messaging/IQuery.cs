using PokeHub.Common.Domain.Abstractions;
using MediatR;

namespace PokeHub.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
