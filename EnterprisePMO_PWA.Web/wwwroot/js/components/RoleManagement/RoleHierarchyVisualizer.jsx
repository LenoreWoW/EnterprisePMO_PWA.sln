import React from 'react';

const RoleHierarchyVisualizer = () => {
  // Sample role data - in real implementation, this would come from props
  const roles = [
    {
      id: 1,
      roleName: "Admin",
      description: "System administrator with full access",
      hierarchyLevel: 100,
      permissions: {
        canManageProjects: true,
        canManageUsers: true,
        canApproveRequests: true,
        canManageRoles: true,
        canViewReports: true,
        canViewAuditLogs: true
      }
    },
    {
      id: 2,
      roleName: "Main PMO",
      description: "Enterprise-level PMO with organization-wide oversight",
      hierarchyLevel: 90,
      permissions: {
        canManageProjects: true,
        canManageUsers: true,
        canApproveRequests: true,
        canManageRoles: true,
        canViewReports: true,
        canViewAuditLogs: true
      }
    },
    {
      id: 3,
      roleName: "Executive",
      description: "Executive management with strategic view",
      hierarchyLevel: 80,
      permissions: {
        canManageProjects: false,
        canManageUsers: false,
        canApproveRequests: false,
        canManageRoles: false,
        canViewReports: true,
        canViewAuditLogs: true
      }
    },
    {
      id: 4,
      roleName: "Department Director",
      description: "Department leadership",
      hierarchyLevel: 70,
      permissions: {
        canManageProjects: false,
        canManageUsers: false,
        canApproveRequests: false,
        canManageRoles: false,
        canViewReports: true,
        canViewAuditLogs: false
      }
    },
    {
      id: 5,
      roleName: "Sub PMO",
      description: "Department or division PMO",
      hierarchyLevel: 60,
      permissions: {
        canManageProjects: true,
        canManageUsers: false,
        canApproveRequests: true,
        canManageRoles: false,
        canViewReports: true,
        canViewAuditLogs: false
      }
    },
    {
      id: 6,
      roleName: "Project Manager",
      description: "Manages individual projects",
      hierarchyLevel: 50,
      permissions: {
        canManageProjects: true,
        canManageUsers: false,
        canApproveRequests: false,
        canManageRoles: false,
        canViewReports: true,
        canViewAuditLogs: false
      }
    }
  ];

  // Helper function to get permission icon classNames
  const getPermissionIcon = (permission) => {
    switch(permission) {
      case 'canManageProjects':
        return 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z';
      case 'canManageUsers':
        return 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z';
      case 'canApproveRequests':
        return 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z';
      case 'canManageRoles':
        return 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z';
      case 'canViewReports':
        return 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z';
      case 'canViewAuditLogs':
        return 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01';
      default:
        return 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z';
    }
  };

  // Sort roles by hierarchy level (highest to lowest)
  const sortedRoles = [...roles].sort((a, b) => b.hierarchyLevel - a.hierarchyLevel);

  return (
    <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8 py-8">
      <div className="bg-white shadow-lg rounded-lg p-6 overflow-hidden">
        <h2 className="text-2xl font-bold text-gray-900 mb-6">Role Hierarchy</h2>
        
        {/* Vertical Timeline */}
        <div className="relative">
          {/* Vertical line */}
          <div className="absolute left-8 top-0 h-full w-0.5 bg-gray-200"></div>
          
          {/* Roles */}
          <div className="space-y-8">
            {sortedRoles.map((role, index) => (
              <div key={role.id} className="relative">
                {/* Level indicator */}
                <div className="absolute left-8 -ml-3 flex h-6 w-6 items-center justify-center rounded-full bg-blue-600 ring-2 ring-white z-10">
                  <span className="text-xs font-semibold text-white">{role.hierarchyLevel}</span>
                </div>
                
                {/* Role card */}
                <div className={`ml-16 bg-white rounded-lg border shadow-sm transition-all duration-200 hover:shadow-md`}>
                  <div className="p-4">
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">{role.roleName}</h3>
                        <p className="text-sm text-gray-500">{role.description}</p>
                      </div>
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        Level {role.hierarchyLevel}
                      </span>
                    </div>
                    
                    {/* Permissions */}
                    <div className="mt-4">
                      <h4 className="text-sm font-medium text-gray-700 mb-2">Permissions</h4>
                      <div className="flex flex-wrap gap-1">
                        {Object.entries(role.permissions).map(([key, value]) => (
                          value && (
                            <div 
                              key={key} 
                              className="flex items-center px-2 py-1 rounded-md bg-gray-100 text-xs"
                              title={key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase())}
                            >
                              <svg className="h-4 w-4 mr-1 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                                <path strokeLinecap="round" strokeLinejoin="round" d={getPermissionIcon(key)} />
                              </svg>
                              <span className="font-medium">{key.replace('can', '').replace(/([A-Z])/g, ' $1').trim()}</span>
                            </div>
                          )
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default RoleHierarchyVisualizer;