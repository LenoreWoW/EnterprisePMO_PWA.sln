import React from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { Check, AlertTriangle, Clock, Activity, Calendar, TrendingUp, FileText, Users } from 'lucide-react';

const DashboardOverview = () => {
  // Sample data - in a real implementation, this would be passed as props or fetched
  const stats = {
    activeProjects: 12,
    pendingTasks: 18,
    pendingChanges: 4,
    overallCompletion: 76
  };

  const upcomingDeadlines = [
    {
      id: 1,
      name: "Customer Mobile App",
      deadline: new Date(2025, 3, 23), // April 23, 2025
      status: "At Risk",
      percentComplete: 65,
    },
    {
      id: 2,
      name: "Data Center Migration",
      deadline: new Date(2025, 4, 10), // May 10, 2025
      status: "On Track",
      percentComplete: 40,
    },
    {
      id: 3,
      name: "Security Compliance Audit",
      deadline: new Date(2025, 3, 1), // April 1, 2025
      status: "Urgent",
      percentComplete: 85,
    }
  ];

  const recentProjects = [
    {
      id: 1,
      name: "ERP System Migration",
      client: "Internal IT",
      deadline: new Date(2025, 6, 25), // July 25, 2025
      status: "On Track",
      percentComplete: 30
    },
    {
      id: 2,
      name: "Customer Mobile App",
      client: "Marketing Division",
      deadline: new Date(2025, 3, 23), // April 23, 2025
      status: "At Risk",
      percentComplete: 65
    },
    {
      id: 3,
      name: "Digital Transformation",
      client: "Finance Dept",
      deadline: new Date(2025, 7, 12), // August 12, 2025
      status: "On Track",
      percentComplete: 15
    }
  ];
  
  const projectsByStatus = [
    { name: 'On Track', value: 8 },
    { name: 'At Risk', value: 3 },
    { name: 'Delayed', value: 1 },
    { name: 'Completed', value: 5 }
  ];
  
  // Helper functions
  const formatDate = (date) => {
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric' 
    });
  };
  
  const getStatusColor = (status) => {
    switch (status.toLowerCase()) {
      case 'on track':
        return 'bg-green-100 text-green-800';
      case 'at risk':
        return 'bg-yellow-100 text-yellow-800';
      case 'delayed':
        return 'bg-red-100 text-red-800';
      case 'urgent':
        return 'bg-red-100 text-red-800';
      case 'completed':
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };
  
  const getProgressColor = (percent) => {
    if (percent < 25) return 'bg-red-500';
    if (percent < 75) return 'bg-yellow-500';
    return 'bg-green-500';
  };
  
  const getDaysLeft = (deadline) => {
    const today = new Date();
    const diffTime = deadline.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  return (
    <div className="bg-gray-50 min-h-screen">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="md:flex md:items-center md:justify-between mb-8">
          <div className="flex-1 min-w-0">
            <h1 className="text-2xl font-bold leading-7 text-gray-900 sm:text-3xl sm:truncate">
              Dashboard
            </h1>
            <p className="mt-1 text-sm text-gray-500">
              Welcome to your Enterprise PMO dashboard. Here's an overview of your projects and tasks.
            </p>
          </div>
          <div className="mt-4 flex md:mt-0 md:ml-4">
            <button type="button" className="ml-3 inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              <Calendar className="h-4 w-4 mr-2" />
              View Calendar
            </button>
            <button type="button" className="ml-3 inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              <FileText className="h-4 w-4 mr-2" />
              Reports
            </button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4 mb-8">
          {/* Active Projects Card */}
          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="px-4 py-5 sm:p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0 bg-blue-100 rounded-md p-3">
                  <FileText className="h-6 w-6 text-blue-600" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">Active Projects</dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900">{stats.activeProjects}</div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
            <div className="bg-gray-50 px-4 py-4 sm:px-6">
              <div className="text-sm">
                <a href="/Project/List" className="font-medium text-blue-600 hover:text-blue-500">
                  View all projects
                  <span aria-hidden="true">&rarr;</span>
                </a>
              </div>
            </div>
          </div>

          {/* Pending Tasks Card */}
          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="px-4 py-5 sm:p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0 bg-indigo-100 rounded-md p-3">
                  <Clock className="h-6 w-6 text-indigo-600" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">Pending Tasks</dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900">{stats.pendingTasks}</div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
            <div className="bg-gray-50 px-4 py-4 sm:px-6">
              <div className="text-sm">
                <a href="/Kanban" className="font-medium text-indigo-600 hover:text-indigo-500">
                  View kanban board
                  <span aria-hidden="true">&rarr;</span>
                </a>
              </div>
            </div>
          </div>

          {/* Change Requests Card */}
          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="px-4 py-5 sm:p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0 bg-yellow-100 rounded-md p-3">
                  <AlertTriangle className="h-6 w-6 text-yellow-600" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">Pending Changes</dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900">{stats.pendingChanges}</div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
            <div className="bg-gray-50 px-4 py-4 sm:px-6">
              <div className="text-sm">
                <a href="/ChangeRequests/List" className="font-medium text-yellow-600 hover:text-yellow-500">
                  View change requests
                  <span aria-hidden="true">&rarr;</span>
                </a>
              </div>
            </div>
          </div>

          {/* Progress Card */}
          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="px-4 py-5 sm:p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0 bg-green-100 rounded-md p-3">
                  <Activity className="h-6 w-6 text-green-600" />
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">Overall Progress</dt>
                    <dd>
                      <div className="text-lg font-medium text-gray-900">{stats.overallCompletion}%</div>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
            <div className="bg-gray-50 px-4 py-4 sm:px-6">
              <div className="text-sm">
                <a href="/Reports" className="font-medium text-green-600 hover:text-green-500">
                  View reports
                  <span aria-hidden="true">&rarr;</span>
                </a>
              </div>
            </div>
          </div>
        </div>

        {/* Charts & Lists Section */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
          {/* Project Status Chart */}
          <div className="bg-white shadow rounded-lg lg:col-span-1">
            <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Projects by Status</h3>
            </div>
            <div className="p-6 h-80">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={projectsByStatus}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="value" fill="#3B82F6" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Upcoming Deadlines List */}
          <div className="bg-white shadow rounded-lg lg:col-span-2">
            <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Upcoming Deadlines</h3>
            </div>
            <ul className="divide-y divide-gray-200">
              {upcomingDeadlines.map((deadline) => {
                const daysLeft = getDaysLeft(deadline.deadline);
                const daysLeftClass = daysLeft < 7 ? "text-red-600" : daysLeft < 14 ? "text-yellow-600" : "text-green-600";
                return (
                  <li key={deadline.id} className="px-4 py-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center">
                        <div className="flex-shrink-0">
                          <span className={`inline-flex items-center justify-center h-10 w-10 rounded-md ${
                            deadline.status.toLowerCase() === 'at risk' ? 'bg-yellow-100 text-yellow-600' :
                            deadline.status.toLowerCase() === 'urgent' ? 'bg-red-100 text-red-600' :
                            'bg-green-100 text-green-600'
                          }`}>
                            {deadline.status.toLowerCase() === 'at risk' ? (
                              <AlertTriangle className="h-6 w-6" />
                            ) : deadline.status.toLowerCase() === 'urgent' ? (
                              <Clock className="h-6 w-6" />
                            ) : (
                              <Check className="h-6 w-6" />
                            )}
                          </span>
                        </div>
                        <div className="ml-4">
                          <h4 className="text-sm font-medium text-gray-900">{deadline.name}</h4>
                          <p className="text-sm text-gray-500">
                            Due on {formatDate(deadline.deadline)}
                          </p>
                        </div>
                      </div>
                      <div className="ml-2 flex flex-shrink-0">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(deadline.status)}`}>
                          {deadline.status}
                        </span>
                      </div>
                    </div>
                    <div className="mt-2">
                      <div className="relative pt-1">
                        <div className="flex items-center justify-between">
                          <div>
                            <span className="text-xs font-semibold inline-block text-blue-600">
                              {deadline.percentComplete}% complete
                            </span>
                          </div>
                          <div className={daysLeftClass}>
                            <span className="text-xs font-semibold inline-block">
                              {daysLeft > 0 ? `${daysLeft} days left` : "Overdue"}
                            </span>
                          </div>
                        </div>
                        <div className="overflow-hidden h-2 mb-4 text-xs flex rounded bg-gray-200">
                          <div style={{ width: `${deadline.percentComplete}%` }} className={`shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center ${getProgressColor(deadline.percentComplete)}`}></div>
                        </div>
                      </div>
                    </div>
                  </li>
                );
              })}
            </ul>
            <div className="bg-gray-50 px-4 py-4 sm:px-6 rounded-b-lg">
              <div className="text-sm text-center">
                <a href="/Project/Deadlines" className="font-medium text-blue-600 hover:text-blue-500">
                  View all deadlines
                  <span aria-hidden="true">&rarr;</span>
                </a>
              </div>
            </div>
          </div>
        </div>

        {/* Recent Projects Table */}
        <div className="bg-white shadow rounded-lg mb-8">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Recent Projects</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Project Name
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Client
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Deadline
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Progress
                  </th>
                  <th scope="col" className="relative px-6 py-3">
                    <span className="sr-only">View</span>
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {recentProjects.map((project) => (
                  <tr key={project.id}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{project.name}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500">{project.client}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">{formatDate(project.deadline)}</div>
                      <div className={`text-xs ${getDaysLeft(project.deadline) < 7 ? 'text-red-600' : getDaysLeft(project.deadline) < 14 ? 'text-yellow-600' : 'text-green-600'}`}>
                        {getDaysLeft(project.deadline) > 0 ? `${getDaysLeft(project.deadline)} days left` : "Overdue"}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusColor(project.status)}`}>
                        {project.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="w-full bg-gray-200 rounded-full h-2.5 mb-1">
                        <div className={`h-2.5 rounded-full ${getProgressColor(project.percentComplete)}`} style={{ width: `${project.percentComplete}%` }}></div>
                      </div>
                      <div className="text-xs text-gray-500 text-right">{project.percentComplete}%</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <a href={`/Project/Details/${project.id}`} className="text-blue-600 hover:text-blue-900">View</a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="bg-gray-50 px-4 py-4 sm:px-6 rounded-b-lg">
            <div className="text-sm text-center">
              <a href="/Project/List" className="font-medium text-blue-600 hover:text-blue-500">
                View all projects
                <span aria-hidden="true">&rarr;</span>
              </a>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row justify-center gap-4">
          <a href="/Project/Create" className="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
            <Users className="h-4 w-4 mr-2" />
            Create New Project
          </a>
          <a href="/WeeklyUpdates/Create" className="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500">
            <TrendingUp className="h-4 w-4 mr-2" />
            Submit Weekly Update
          </a>
        </div>
      </div>
    </div>
  );
};

export default DashboardOverview;