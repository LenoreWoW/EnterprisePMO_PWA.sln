using System;
using System.Collections.Generic;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Domain.Workflows
{
    public static class ProjectWorkflow
    {
        public enum UpdateStatus
        {
            Draft,
            PendingApproval,
            Approved,
            Rejected
        }

        public enum ChangeRequestStatus
        {
            Draft,
            PendingApproval,
            Approved,
            Rejected
        }

        public static readonly Dictionary<UserRole, ProjectStatus[]> AllowedStatusTransitions = new()
        {
            {
                UserRole.ProjectManager, new[]
                {
                    ProjectStatus.Draft,
                    ProjectStatus.PendingApproval,
                    ProjectStatus.Active,
                    ProjectStatus.OnHold,
                    ProjectStatus.Completed
                }
            },
            {
                UserRole.PMOHead, new[]
                {
                    ProjectStatus.PendingApproval,
                    ProjectStatus.Active,
                    ProjectStatus.Rejected,
                    ProjectStatus.Cancelled
                }
            }
        };

        public static readonly Dictionary<UserRole, UpdateStatus[]> AllowedUpdateStatusTransitions = new()
        {
            {
                UserRole.ProjectManager, new[]
                {
                    UpdateStatus.Draft,
                    UpdateStatus.PendingApproval
                }
            },
            {
                UserRole.PMOHead, new[]
                {
                    UpdateStatus.PendingApproval,
                    UpdateStatus.Approved,
                    UpdateStatus.Rejected
                }
            }
        };

        public static readonly Dictionary<UserRole, ChangeRequestStatus[]> AllowedChangeRequestStatusTransitions = new()
        {
            {
                UserRole.ProjectManager, new[]
                {
                    ChangeRequestStatus.Draft,
                    ChangeRequestStatus.PendingApproval
                }
            },
            {
                UserRole.PMOHead, new[]
                {
                    ChangeRequestStatus.PendingApproval,
                    ChangeRequestStatus.Approved,
                    ChangeRequestStatus.Rejected
                }
            }
        };

        public static bool CanTransitionTo(ProjectStatus currentStatus, ProjectStatus newStatus, UserRole userRole)
        {
            if (!AllowedStatusTransitions.ContainsKey(userRole))
                return false;

            return Array.Exists(AllowedStatusTransitions[userRole], status => status == newStatus);
        }

        public static bool CanTransitionTo(UpdateStatus currentStatus, UpdateStatus newStatus, UserRole userRole)
        {
            if (!AllowedUpdateStatusTransitions.ContainsKey(userRole))
                return false;

            return Array.Exists(AllowedUpdateStatusTransitions[userRole], status => status == newStatus);
        }

        public static bool CanTransitionTo(ChangeRequestStatus currentStatus, ChangeRequestStatus newStatus, UserRole userRole)
        {
            if (!AllowedChangeRequestStatusTransitions.ContainsKey(userRole))
                return false;

            return Array.Exists(AllowedChangeRequestStatusTransitions[userRole], status => status == newStatus);
        }
    }
} 