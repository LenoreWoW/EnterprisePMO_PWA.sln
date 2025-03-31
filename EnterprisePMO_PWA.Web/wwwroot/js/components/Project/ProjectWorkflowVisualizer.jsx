import React from 'react';

const ProjectWorkflowVisualizer = () => {
  // Sample project data - in a real implementation, this would come from props
  const project = {
    id: "a1b2c3",
    name: "Digital Transformation Initiative",
    description: "Enterprise-wide digital transformation project",
    creationDate: "2025-01-15T09:00:00",
    submissionDate: "2025-01-18T14:30:00",
    subPmoApprovalDate: "2025-01-20T10:15:00",
    mainPmoApprovalDate: "2025-01-25T16:45:00",
    completionDate: null, // Not completed yet
    status: "Active", // Proposed, Active, Completed, Rejected
    percentComplete: 35,
    projectManager: {
      id: "pm123",
      name: "Sarah Johnson"
    },
    department: {
      id: "dept456",
      name: "Information Technology"
    }
  };

  // Define workflow steps
  const workflowSteps = [
    {
      id: 'created',
      title: 'Created',
      description: 'Project proposal created',
      icon: 'M12 6v6m0 0v6m0-6h6m-6 0H6',
      date: project.creationDate,
      isComplete: true,
      isCurrent: false
    },
    {
      id: 'submitted',
      title: 'Submitted',
      description: 'Submitted for review',
      icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2',
      date: project.submissionDate,
      isComplete: !!project.submissionDate,
      isCurrent: !!project.submissionDate && !project.subPmoApprovalDate
    },
    {
      id: 'subPmoApproval',
      title: 'Sub PMO Review',
      description: 'Department PMO approval',
      icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
      date: project.subPmoApprovalDate,
      isComplete: !!project.subPmoApprovalDate,
      isCurrent: !!project.subPmoApprovalDate && !project.mainPmoApprovalDate
    },
    {
      id: 'mainPmoApproval',
      title: 'Main PMO Approval',
      description: 'Final approval',
      icon: 'M5 13l4 4L19 7',
      date: project.mainPmoApprovalDate,
      isComplete: !!project.mainPmoApprovalDate,
      isCurrent: !!project.mainPmoApprovalDate && !project.completionDate && project.status === 'Active'
    },
    {
      id: 'completed',
      title: 'Completed',
      description: 'Project finished',
      icon: 'M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9',
      date: project.completionDate,
      isComplete: !!project.completionDate,
      isCurrent: false
    }
  ];

  // Format date
  const formatDate = (dateString) => {
    if (!dateString) return 'Pending';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  };

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-4 py-5 sm:px-6 bg-gradient-to-r from-blue-600 to-blue-800">
        <h3 className="text-lg font-medium leading-6 text-white">Project Workflow Status</h3>
        <p className="mt-1 max-w-2xl text-sm text-blue-100">
          Current status: <span className="font-semibold">{project.status}</span>
        </p>
      </div>
      <div className="border-t border-gray-200">
        <div className="px-4 py-5 sm:p-6">
          {/* Progress bar */}
          <div className="mb-8">
            <div className="flex justify-between items-center mb-1">
              <span className="text-sm font-medium text-gray-700">Overall completion</span>
              <span className="text-sm font-medium text-gray-700">{project.percentComplete}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2.5">
              <div 
                className="bg-blue-600 h-2.5 rounded-full transition-all duration-500 ease-in-out" 
                style={{ width: `${project.percentComplete}%` }}
              ></div>
            </div>
          </div>
          
          {/* Workflow steps */}
          <nav aria-label="Progress">
            <ol className="overflow-hidden">
              {workflowSteps.map((step, stepIdx) => (
                <li key={step.id} className={`relative ${stepIdx !== workflowSteps.length - 1 ? 'pb-10' : ''}`}>
                  {stepIdx !== workflowSteps.length - 1 ? (
                    <div className={`absolute left-4 top-4 -ml-px mt-0.5 h-full w-0.5 ${step.isComplete ? 'bg-blue-600' : 'bg-gray-300'}`} aria-hidden="true" />
                  ) : null}
                  
                  <div className="relative flex items-start group">
                    <span className="h-9 flex items-center">
                      <span className={`relative z-10 w-8 h-8 flex items-center justify-center rounded-full ${
                        step.isComplete ? 'bg-blue-600' : step.isCurrent ? 'bg-blue-200 border-2 border-blue-600' : 'bg-gray-200'
                      }`}>
                        {step.isComplete ? (
                          <svg className="w-5 h-5 text-white" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                            <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                          </svg>
                        ) : (
                          <svg className={`w-5 h-5 ${step.isCurrent ? 'text-blue-600' : 'text-gray-500'}`} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={step.icon} />
                          </svg>
                        )}
                      </span>
                    </span>
                    
                    <span className="ml-4 min-w-0 flex flex-col">
                      <span className={`text-sm font-medium ${step.isComplete ? 'text-blue-600' : step.isCurrent ? 'text-blue-800' : 'text-gray-500'}`}>
                        {step.title}
                      </span>
                      <span className="text-sm text-gray-500">{step.description}</span>
                      <span className={`text-sm ${step.isComplete ? 'text-gray-700' : 'text-gray-400'}`}>
                        {formatDate(step.date)}
                      </span>
                    </span>
                  </div>
                </li>
              ))}
            </ol>
          </nav>
          
          {/* Status information card */}
          <div className={`mt-6 rounded-md p-4 ${
            project.status === 'Proposed' ? 'bg-yellow-50 border border-yellow-100' : 
            project.status === 'Active' ? 'bg-blue-50 border border-blue-100' : 
            project.status === 'Completed' ? 'bg-green-50 border border-green-100' : 
            'bg-red-50 border border-red-100'
          }`}>
            <div className="flex">
              <div className="flex-shrink-0">
                {project.status === 'Proposed' && (
                  <svg className="h-5 w-5 text-yellow-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                )}
                {project.status === 'Active' && (
                  <svg className="h-5 w-5 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                  </svg>
                )}
                {project.status === 'Completed' && (
                  <svg className="h-5 w-5 text-green-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                )}
                {project.status === 'Rejected' && (
                  <svg className="h-5 w-5 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                )}
              </div>
              <div className="ml-3">
                <h3 className={`text-sm font-medium ${
                  project.status === 'Proposed' ? 'text-yellow-800' : 
                  project.status === 'Active' ? 'text-blue-800' : 
                  project.status === 'Completed' ? 'text-green-800' : 
                  'text-red-800'
                }`}>
                  {project.status === 'Proposed' && 'Awaiting Approvals'}
                  {project.status === 'Active' && 'Project In Progress'}
                  {project.status === 'Completed' && 'Project Successfully Completed'}
                  {project.status === 'Rejected' && 'Project Rejected'}
                </h3>
                <div className={`mt-2 text-sm ${
                  project.status === 'Proposed' ? 'text-yellow-700' : 
                  project.status === 'Active' ? 'text-blue-700' : 
                  project.status === 'Completed' ? 'text-green-700' : 
                  'text-red-700'
                }`}>
                  <p>
                    {project.status === 'Proposed' && 'This project is currently awaiting necessary approvals before work can begin.'}
                    {project.status === 'Active' && 'This project has been approved and is currently being executed.'}
                    {project.status === 'Completed' && 'This project has been successfully completed.'}
                    {project.status === 'Rejected' && 'This project has been rejected. Please review feedback and resubmit if appropriate.'}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProjectWorkflowVisualizer;