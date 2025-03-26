import React, { useState, useEffect } from 'react';
import { CheckCircle, XCircle, AlertTriangle, Clock, ArrowRight } from 'lucide-react';

const ProjectApprovalWorkflow = ({ projectId, projectStatus, userRole }) => {
  const [project, setProject] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [comments, setComments] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [actionMessage, setActionMessage] = useState(null);

  useEffect(() => {
    // Fetch project details
    const fetchProject = async () => {
      try {
        setLoading(true);
        const response = await fetch(`/api/projects/${projectId}`);
        if (!response.ok) {
          throw new Error('Failed to fetch project details');
        }
        const data = await response.json();
        setProject(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    if (projectId) {
      fetchProject();
    }
  }, [projectId]);

  // Handle project submission for approval
  const handleSubmitForApproval = async () => {
    try {
      setSubmitting(true);
      const response = await fetch('/api/project-workflow/submit', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ projectId }),
      });

      const result = await response.json();
      
      if (response.ok) {
        setActionMessage({ type: 'success', text: result.message });
        // Update project status locally to avoid refetch
        setProject(prev => ({ ...prev, status: 'Proposed' }));
      } else {
        setActionMessage({ type: 'error', text: result.message });
      }
    } catch (err) {
      setActionMessage({ type: 'error', text: err.message });
    } finally {
      setSubmitting(false);
    }
  };

  // Handle project approval by Sub PMO
  const handleSubPMOApproval = async () => {
    try {
      setSubmitting(true);
      const response = await fetch('/api/project-workflow/approve-sub-pmo', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ projectId, comments }),
      });

      const result = await response.json();
      
      if (response.ok) {
        setActionMessage({ type: 'success', text: result.message });
        // Update project status locally to avoid refetch
        setProject(prev => ({ ...prev, status: 'Active' }));
      } else {
        setActionMessage({ type: 'error', text: result.message });
      }
    } catch (err) {
      setActionMessage({ type: 'error', text: err.message });
    } finally {
      setSubmitting(false);
      setComments('');
    }
  };

  // Handle project rejection by Sub PMO
  const handleSubPMORejection = async () => {
    if (!rejectionReason.trim()) {
      setActionMessage({ type: 'error', text: 'Rejection reason is required' });
      return;
    }

    try {
      setSubmitting(true);
      const response = await fetch('/api/project-workflow/reject-sub-pmo', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ projectId, rejectionReason }),
      });

      const result = await response.json();
      
      if (response.ok) {
        setActionMessage({ type: 'success', text: result.message });
        // Update project status locally to avoid refetch
        setProject(prev => ({ ...prev, status: 'Rejected' }));
      } else {
        setActionMessage({ type: 'error', text: result.message });
      }
    } catch (err) {
      setActionMessage({ type: 'error', text: err.message });
    } finally {
      setSubmitting(false);
      setRejectionReason('');
    }
  };

  // Handle project approval by Main PMO
  const handleMainPMOApproval = async () => {
    try {
      setSubmitting(true);
      const response = await fetch('/api/project-workflow/approve-main-pmo', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ projectId, comments }),
      });

      const result = await response.json();
      
      if (response.ok) {
        setActionMessage({ type: 'success', text: result.message });
        // Update project status locally to avoid refetch
        setProject(prev => ({ ...prev, status: 'Active', approvedDate: new Date().toISOString() }));
      } else {
        setActionMessage({ type: 'error', text: result.message });
      }
    } catch (err) {
      setActionMessage({ type: 'error', text: err.message });
    } finally {
      setSubmitting(false);
      setComments('');
    }
  };

  // Handle project rejection by Main PMO
  const handleMainPMORejection = async () => {
    if (!rejectionReason.trim()) {
      setActionMessage({ type: 'error', text: 'Rejection reason is required' });
      return;
    }

    try {
      setSubmitting(true);
      const response = await fetch('/api/project-workflow/reject-main-pmo', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ projectId, rejectionReason }),
      });

      const result = await response.json();
      
      if (response.ok) {
        setActionMessage({ type: 'success', text: result.message });
        // Update project status locally to avoid refetch
        setProject(prev => ({ ...prev, status: 'Rejected' }));
      } else {
        setActionMessage({ type: 'error', text: result.message });
      }
    } catch (err) {
      setActionMessage({ type: 'error', text: err.message });
    } finally {
      setSubmitting(false);
      setRejectionReason('');
    }
  };

  // Handle project resubmission after rejection
  const handleResubmitProject = async () => {
    if (!comments.trim()) {
      setActionMessage({ type: 'error', text: 'Description of changes is required' });
      return;
    }

    try {
      setSubmitting(true);
      const response = await fetch('/api/project-workflow/resubmit', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ projectId, changesDescription: comments }),
      });

      const result = await response.json();
      
      if (response.ok) {
        setActionMessage({ type: 'success', text: result.message });
        // Update project status locally to avoid refetch
        setProject(prev => ({ ...prev, status: 'Proposed' }));
      } else {
        setActionMessage({ type: 'error', text: result.message });
      }
    } catch (err) {
      setActionMessage({ type: 'error', text: err.message });
    } finally {
      setSubmitting(false);
      setComments('');
    }
  };

  if (loading) {
    return <div className="flex justify-center items-center h-32">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
    </div>;
  }

  if (error) {
    return <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
      <p className="font-bold">Error</p>
      <p>{error}</p>
    </div>;
  }

  const renderProjectSteps = () => {
    const stepStatus = {
      creation: 'complete',
      submission: project?.status !== 'Proposed' ? 'complete' : 'current',
      subPmoApproval: project?.status === 'Active' || project?.status === 'Completed' ? 'complete' : project?.status === 'Rejected' ? 'rejected' : 'pending',
      mainPmoApproval: project?.approvedDate ? 'complete' : 'pending',
      completion: project?.status === 'Completed' ? 'complete' : 'pending',
    };

    return (
      <div className="mb-8">
        <div className="flex items-center justify-between w-full mb-2">
          <div className="flex flex-col items-center">
            <div className={`h-10 w-10 rounded-full flex items-center justify-center ${stepStatus.creation === 'complete' ? 'bg-green-100 text-green-600' : 'bg-gray-100 text-gray-400'}`}>
              {stepStatus.creation === 'complete' ? <CheckCircle size={24} /> : <Clock size={24} />}
            </div>
            <div className="text-sm mt-1">Created</div>
          </div>
          <div className="h-1 flex-1 mx-2 bg-gray-200">
            <div className={`h-full ${stepStatus.submission === 'complete' ? 'bg-green-500' : 'bg-gray-200'}`} style={{ width: '100%' }}></div>
          </div>
          <div className="flex flex-col items-center">
            <div className={`h-10 w-10 rounded-full flex items-center justify-center ${
              stepStatus.submission === 'complete' ? 'bg-green-100 text-green-600' : 
              stepStatus.submission === 'current' ? 'bg-blue-100 text-blue-600' : 
              'bg-gray-100 text-gray-400'
            }`}>
              {stepStatus.submission === 'complete' ? <CheckCircle size={24} /> : 
               stepStatus.submission === 'current' ? <Clock size={24} /> : 
               <Clock size={24} />}
            </div>
            <div className="text-sm mt-1">Submitted</div>
          </div>
          <div className="h-1 flex-1 mx-2 bg-gray-200">
            <div className={`h-full ${stepStatus.subPmoApproval === 'complete' ? 'bg-green-500' : stepStatus.subPmoApproval === 'rejected' ? 'bg-red-500' : 'bg-gray-200'}`} style={{ width: '100%' }}></div>
          </div>
          <div className="flex flex-col items-center">
            <div className={`h-10 w-10 rounded-full flex items-center justify-center ${
              stepStatus.subPmoApproval === 'complete' ? 'bg-green-100 text-green-600' : 
              stepStatus.subPmoApproval === 'rejected' ? 'bg-red-100 text-red-600' : 
              stepStatus.subPmoApproval === 'current' ? 'bg-blue-100 text-blue-600' : 
              'bg-gray-100 text-gray-400'
            }`}>
              {stepStatus.subPmoApproval === 'complete' ? <CheckCircle size={24} /> : 
               stepStatus.subPmoApproval === 'rejected' ? <XCircle size={24} /> : 
               stepStatus.subPmoApproval === 'current' ? <Clock size={24} /> : 
               <Clock size={24} />}
            </div>
            <div className="text-sm mt-1">Sub PMO</div>
          </div>
          <div className="h-1 flex-1 mx-2 bg-gray-200">
            <div className={`h-full ${stepStatus.mainPmoApproval === 'complete' ? 'bg-green-500' : 'bg-gray-200'}`} style={{ width: '100%' }}></div>
          </div>
          <div className="flex flex-col items-center">
            <div className={`h-10 w-10 rounded-full flex items-center justify-center ${
              stepStatus.mainPmoApproval === 'complete' ? 'bg-green-100 text-green-600' : 
              stepStatus.mainPmoApproval === 'current' ? 'bg-blue-100 text-blue-600' : 
              'bg-gray-100 text-gray-400'
            }`}>
              {stepStatus.mainPmoApproval === 'complete' ? <CheckCircle size={24} /> : <Clock size={24} />}
            </div>
            <div className="text-sm mt-1">Main PMO</div>
          </div>
          <div className="h-1 flex-1 mx-2 bg-gray-200">
            <div className={`h-full ${stepStatus.completion === 'complete' ? 'bg-green-500' : 'bg-gray-200'}`} style={{ width: '100%' }}></div>
          </div>
          <div className="flex flex-col items-center">
            <div className={`h-10 w-10 rounded-full flex items-center justify-center ${
              stepStatus.completion === 'complete' ? 'bg-green-100 text-green-600' : 
              'bg-gray-100 text-gray-400'
            }`}>
              {stepStatus.completion === 'complete' ? <CheckCircle size={24} /> : <Clock size={24} />}
            </div>
            <div className="text-sm mt-1">Completed</div>
          </div>
        </div>
      </div>
    );
  };

  const renderActionButtons = () => {
    // Project Manager actions
    if (userRole === 'ProjectManager') {
      if (project?.status === 'Proposed' && !project?.approvedDate) {
        return (
          <div className="mt-4">
            <button 
              className="bg-blue-500 text-white py-2 px-4 rounded hover:bg-blue-600 disabled:opacity-50"
              onClick={handleSubmitForApproval}
              disabled={submitting}
            >
              {submitting ? 'Submitting...' : 'Submit for Approval'}
            </button>
          </div>
        );
      } else if (project?.status === 'Rejected') {
        return (
          <div className="mt-4">
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Describe the changes made to address the rejection
              </label>
              <textarea
                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows="4"
                value={comments}
                onChange={(e) => setComments(e.target.value)}
                placeholder="Describe the changes you've made to address the concerns..."
                disabled={submitting}
              ></textarea>
            </div>
            <button 
              className="bg-blue-500 text-white py-2 px-4 rounded hover:bg-blue-600 disabled:opacity-50"
              onClick={handleResubmitProject}
              disabled={submitting || !comments.trim()}
            >
              {submitting ? 'Resubmitting...' : 'Resubmit Project'}
            </button>
          </div>
        );
      }
      return null;
    }

    // Sub PMO actions
    if (userRole === 'SubPMO' && project?.status === 'Proposed' && !project?.approvedDate) {
      return (
        <div className="mt-4">
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Approval Comments (optional)
            </label>
            <textarea
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="4"
              value={comments}
              onChange={(e) => setComments(e.target.value)}
              placeholder="Add any comments about this approval..."
              disabled={submitting}
            ></textarea>
          </div>
          <div className="flex space-x-4">
            <button 
              className="bg-green-500 text-white py-2 px-4 rounded hover:bg-green-600 disabled:opacity-50"
              onClick={handleSubPMOApproval}
              disabled={submitting}
            >
              {submitting ? 'Approving...' : 'Approve Project'}
            </button>
            <button 
              className="bg-red-500 text-white py-2 px-4 rounded hover:bg-red-600 disabled:opacity-50"
              onClick={() => setRejectionReason('') || document.getElementById('rejectionModal').classList.remove('hidden')}
              disabled={submitting}
            >
              Reject Project
            </button>
          </div>
        </div>
      );
    }

    // Main PMO actions
    if (userRole === 'MainPMO' && project?.status === 'Active' && !project?.approvedDate) {
      return (
        <div className="mt-4">
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Approval Comments (optional)
            </label>
            <textarea
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="4"
              value={comments}
              onChange={(e) => setComments(e.target.value)}
              placeholder="Add any comments about this final approval..."
              disabled={submitting}
            ></textarea>
          </div>
          <div className="flex space-x-4">
            <button 
              className="bg-green-500 text-white py-2 px-4 rounded hover:bg-green-600 disabled:opacity-50"
              onClick={handleMainPMOApproval}
              disabled={submitting}
            >
              {submitting ? 'Approving...' : 'Final Approval'}
            </button>
            <button 
              className="bg-red-500 text-white py-2 px-4 rounded hover:bg-red-600 disabled:opacity-50"
              onClick={() => setRejectionReason('') || document.getElementById('rejectionModal').classList.remove('hidden')}
              disabled={submitting}
            >
              Reject Project
            </button>
          </div>
        </div>
      );
    }

    return null;
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-xl font-semibold mb-4">Project Approval Workflow</h2>
      
      {renderProjectSteps()}
      
      {actionMessage && (
        <div className={`p-4 mb-4 rounded ${actionMessage.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
          <div className="flex items-center">
            {actionMessage.type === 'success' ? <CheckCircle size={20} className="mr-2" /> : <AlertTriangle size={20} className="mr-2" />}
            <span>{actionMessage.text}</span>
          </div>
        </div>
      )}
      
      {project?.status === 'Rejected' && (
        <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <XCircle className="h-5 w-5 text-red-500" />
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-700">
                This project was rejected. Reason: {project.rejectionReason || "No reason provided"}
              </p>
            </div>
          </div>
        </div>
      )}
      
      {renderActionButtons()}
      
      {/* Rejection Modal */}
      <div id="rejectionModal" className="hidden fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
        <div className="bg-white rounded-lg w-full max-w-md mx-4 p-6">
          <h3 className="text-lg font-medium mb-4">Reject Project</h3>
          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Rejection Reason <span className="text-red-500">*</span>
            </label>
            <textarea
              className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="4"
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              placeholder="Please provide a reason for rejecting this project..."
            ></textarea>
          </div>
          <div className="flex justify-end space-x-3">
            <button 
              className="px-4 py-2 border border-gray-300 rounded text-gray-700 hover:bg-gray-100"
              onClick={() => document.getElementById('rejectionModal').classList.add('hidden')}
            >
              Cancel
            </button>
            <button 
              className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
              disabled={!rejectionReason.trim()}
              onClick={() => {
                document.getElementById('rejectionModal').classList.add('hidden');
                if (userRole === 'SubPMO') {
                  handleSubPMORejection();
                } else if (userRole === 'MainPMO') {
                  handleMainPMORejection();
                }
              }}
            >
              Reject Project
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProjectApprovalWorkflow;