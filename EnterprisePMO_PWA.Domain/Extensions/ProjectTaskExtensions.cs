using DomainTaskStatus = EnterprisePMO_PWA.Domain.Enums.TaskStatus;

namespace EnterprisePMO_PWA.Domain.Extensions;

public static class ProjectTaskExtensions
{
    public static string GetStatusClass(this DomainTaskStatus status)
    {
        return status switch
        {
            DomainTaskStatus.ToDo => "bg-gray-100 text-gray-800",
            DomainTaskStatus.InProgress => "bg-blue-100 text-blue-800",
            DomainTaskStatus.Completed => "bg-green-100 text-green-800",
            DomainTaskStatus.Review => "bg-yellow-100 text-yellow-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }
} 