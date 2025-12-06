// TODO: Add Audit Logic Auto Log for All CRUD Operations in whole application
// TODO: This is the logic for audit logging for all CRUD operations in whole application
// using MediatR;
// using SRMS.Application.AuditLogs.Interfaces;
// using SRMS.Domain.AuditLogs.Enums;

// namespace SRMS.Application.Common.Behaviors;

// public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//     where TRequest : IRequest<TResponse>
// {
//     private readonly IAuditService _auditService;

//     public AuditBehavior(IAuditService auditService)
//     {
//         _auditService = auditService;
//     }

//     public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//     {
//         var requestName = typeof(TRequest).Name;

//         // Only audit Commands (usually modify state)
//         if (!requestName.EndsWith("Command"))
//         {
//             return await next();
//         }

//         TResponse response;

//         try
//         {
//             response = await next();
//         }
//         catch (Exception ex)
//         {
//             await _auditService.LogAsync(
//                 AuditAction.Error,
//                 requestName,
//                 null,
//                 request,
//                 null,
//                 $"Command Failed: {ex.Message}"
//             );
//             throw;
//         }

//         // Determine action type based on command name
//         var action = AuditAction.Update;
//         if (requestName.StartsWith("Create") || requestName.StartsWith("Add") || requestName.StartsWith("Register"))
//             action = AuditAction.Create;
//         else if (requestName.StartsWith("Delete") || requestName.StartsWith("Remove"))
//             action = AuditAction.Delete;
//         else if (requestName.StartsWith("Login"))
//             action = AuditAction.Login;

//         await _auditService.LogAsync(
//             action,
//             requestName,
//             null,
//             request,
//             response,
//             "Command Executed Successfully"
//         );

//         return response;
//     }
// }
