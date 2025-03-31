import React, { useState } from 'react';
import { 
  Clock, 
  Calendar, 
  DollarSign, 
  Users, 
  FileText, 
  Tag, 
  Briefcase, 
  BarChart2, 
  CheckCircle, 
  AlertCircle, 
  Edit, 
  Download,
  Link
} from 'lucide-react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';

const ProjectDetailsView = () => {
  // In a real implementation, this would be fetched from an API
  const project = {
    id: "proj123",
    name: "Enterprise Resource Planning System Migration",
    description: "Upgrade and migrate the existing ERP system to the latest version with enhanced modules for finance, HR, and operations. This project aims to improve efficiency, integrate systems, and provide better reporting capabilities.",
    startDate: "2025-01-15",
    endDate: "2025-07-25",
    status: "Active",
    percentComplete: 30,
    budget: 450000,
    actualCost: 120000,
    estimatedCost: 430000,
    client: "Internal IT",
    strategic_goal: "Digital Transformation",
    annual_goal: "Modernize Core Systems",
    department: {
      id: "dept1",
      name: "Information Technology"
    },
    projectManager: {
      id: "pm1",
      name: "Alex Johnson"
    },
    team: [
      { id: "user1", name: "Sarah Chen", role: "Business Analyst" },
      { id: "user2", name: "Miguel Rodriguez", role: "Lead Developer" },
      { id: "user3", name: "Priya Patel", role: "QA Lead" },
      { id: "user4", name: "David Kim", role: "Infrastructure Specialist" }
    ],
    risks: [
      { id: "risk1", description: "Data migration issues", impact: "High", mitigation: "Comprehensive testing plan and rehearsals" },
      { id: "risk2", description: "User adoption challenges", impact: "Medium", mitigation: "Enhanced training program and champions network" }
    ],
    milestones: [
      { id: "m1", name: "Requirements Gathering", dueDate: "2025-02-15", status: "Completed" },
      { id: "m2", name: "System Design", dueDate: "2025-03-30", status: "Completed" },
      { id: "m3", name: "Development Phase 1", dueDate: "2025-05-15", status: "In Progress" },
      { id: "m4", name: "Testing", dueDate: "2025-06-30", status: "Not Started" },
      { id: "m5", name: "Deployment", dueDate: "2025-07-20", status: "Not Started" }
    ],
    weeklyUpdates: [
      { 
        id: "upd1", 
        date: "2025-03-24", 
        accomplishments: "Completed system design documentation and reviews", 
        nextSteps: "Begin development sprint 1", 
        issuesOrRisks: "None", 
        percentComplete: 25 
      },
      { 
        id: "upd2", 
        date: "2025-03-31", 
        accomplishments: "Started development sprint 1, completed database schema design", 
        nextSteps: "Continue development of core modules", 
        issuesOrRisks: "Potential delay in vendor API documentation", 
        percentComplete: 28 
      },
      { 
        id: "upd3", 
        date: "2025-04-07", 
        accomplishments: "Completed 40% of core modules development", 
        nextSteps: "Continue module development, prepare for first integration test", 
        issuesOrRisks: "None", 
        percentComplete: 30 
      }
    ],
    dependencies: [
      { id: "dep1", projectName: "Infrastructure Upgrade", relationship: "Depends on", status: "On Track" },
      { id: "dep2", projectName: "Data Warehouse Project", relationship: "Required by", status: "At Risk" }
    ]
  };

  // Format date
  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  // Calculate days remaining
  const calculateDaysRemaining = () => {
    const today = new Date();
    const endDate = new Date(project.endDate);
    const diffTime = endDate - today;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  // Tabs state
  const [activeTab, setActiveTab] = useState('overview');

  // Budget data for pie chart
  const budgetData = [
    { name: 'Spent', value: project.actualCost },
    { name: 'Remaining', value: project.budget - project.actualCost }
  ];
  
  const COLORS = ['#3B82F6', '#E5E7EB'];

  return (
    <div className="bg-gray-50 min-h-screen pb-10">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Project Header */}
        <div className="bg-white shadow-sm rounded-lg overflow-hidden border border-gray-200 mb-6">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200 bg-gradient-to-r from-blue-600 to-blue-800">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between">
              <div>
                <h1 className="text-2xl font-bold text-white">{project.name}</h1>
                <p className="mt-1 max-w-2xl text-sm text-blue-100">
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-200 text-blue-800 mr-2">
                    {project.status}
                  </span>
                  <span className="text-blue-100">{project.percentComplete}% Complete</span>
                </p>
              </div>
              <div className="mt-4 md:mt-0 flex space-x-3">
                <button className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-blue-600 bg-white hover:bg-blue-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                  <Edit className="h-4 w-4 mr-2" />
                  Edit Project
                </button>
                <button className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-700 hover:bg-blue-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                  <Download className="h-4 w-4 mr-2" />
                  Export
                </button>
              </div>
            </div>
          </div>
          
          {/* Key Project Info */}
          <div className="px-4 py-5 sm:p-6 bg-white">
            <div className="md:flex md:justify-between">
              <div className="md:w-2/3 mb-6 md:mb-0 md:pr-8">
                <h3 className="text-lg font-medium text-gray-900 mb-2">Project Description</h3>
                <p className="text-gray-600 text-sm">{project.description}</p>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
                  <div className="flex items-center">
                    <Calendar className="h-5 w-5 text-gray-400 mr-2" />
                    <div>
                      <p className="text-xs text-gray-500">Timeline</p>
                      <p className="text-sm font-medium text-gray-900">
                        {formatDate(project.startDate)} — {formatDate(project.endDate)}
                      </p>
                    </div>
                  </div>
                  
                  <div className="flex items-center">
                    <Briefcase className="h-5 w-5 text-gray-400 mr-2" />
                    <div>
                      <p className="text-xs text-gray-500">Department</p>
                      <p className="text-sm font-medium text-gray-900">{project.department.name}</p>
                    </div>
                  </div>
                  
                  <div className="flex items-center">
                    <Users className="h-5 w-5 text-gray-400 mr-2" />
                    <div>
                      <p className="text-xs text-gray-500">Project Manager</p>
                      <p className="text-sm font-medium text-gray-900">{project.projectManager.name}</p>
                    </div>
                  </div>
                  
                  <div className="flex items-center">
                    <DollarSign className="h-5 w-5 text-gray-400 mr-2" />
                    <div>
                      <p className="text-xs text-gray-500">Budget</p>
                      <p className="text-sm font-medium text-gray-900">
                        ${project.budget.toLocaleString()}
                      </p>
                    </div>
                  </div>
                  
                  <div className="flex items-center">
                    <Tag className="h-5 w-5 text-gray-400 mr-2" />
                    <div>
                      <p className="text-xs text-gray-500">Strategic Goal</p>
                      <p className="text-sm font-medium text-gray-900">{project.strategic_goal}</p>
                    </div>
                  </div>
                  
                  <div className="flex items-center">
                    <FileText className="h-5 w-5 text-gray-400 mr-2" />
                    <div>
                      <p className="text-xs text-gray-500">Annual Goal</p>
                      <p className="text-sm font-medium text-gray-900">{project.annual_goal}</p>
                    </div>
                  </div>
                </div>
              </div>
              
              <div className="md:w-1/3 bg-gray-50 rounded-lg p-4">
                <div className="mb-4">
                  <p className="text-sm font-medium text-gray-500 mb-1">Project Progress</p>
                  <div className="w-full bg-gray-200 rounded-full h-2.5">
                    <div 
                      className="bg-blue-600 h-2.5 rounded-full" 
                      style={{ width: `${project.percentComplete}%` }}
                    ></div>
                  </div>
                  <div className="flex justify-between mt-1">
                    <span className="text-xs text-gray-500">0%</span>
                    <span className="text-xs font-medium text-blue-600">{project.percentComplete}%</span>
                    <span className="text-xs text-gray-500">100%</span>
                  </div>
                </div>
                
                <div className="flex items-center mb-4">
                  <Clock className="h-5 w-5 text-gray-400 mr-2" />
                  <div>
                    <p className="text-xs text-gray-500">Time Remaining</p>
                    <p className="text-sm font-medium text-gray-900">
                      {calculateDaysRemaining()} days left
                    </p>
                  </div>
                </div>
                
                <div className="flex items-center mb-4">
                  <BarChart2 className="h-5 w-5 text-gray-400 mr-2" />
                  <div>
                    <p className="text-xs text-gray-500">Budget Utilization</p>
                    <p className="text-sm font-medium text-gray-900">
                      ${project.actualCost.toLocaleString()} of ${project.budget.toLocaleString()} ({Math.round((project.actualCost / project.budget) * 100)}%)
                    </p>
                  </div>
                </div>
                
                <div className="h-32">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={budgetData}
                        cx="50%"
                        cy="50%"
                        innerRadius={30}
                        outerRadius={50}
                        paddingAngle={5}
                        dataKey="value"
                      >
                        {budgetData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(value) => `$${value.toLocaleString()}`} />
                      <Legend />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        {/* Tabs Navigation */}
        <div className="border-b border-gray-200 mb-6">
          <nav className="-mb-px flex space-x-8" aria-label="Tabs">
            <button
              onClick={() => setActiveTab('overview')}
              className={`${
                activeTab === 'overview'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              Overview
            </button>
            <button
              onClick={() => setActiveTab('team')}
              className={`${
                activeTab === 'team'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              Team
            </button>
            <button
              onClick={() => setActiveTab('milestones')}
              className={`${
                activeTab === 'milestones'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              Milestones
            </button>
            <button
              onClick={() => setActiveTab('updates')}
              className={`${
                activeTab === 'updates'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              Weekly Updates
            </button>
            <button
              onClick={() => setActiveTab('risks')}
              className={`${
                activeTab === 'risks'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              Risks
            </button>
            <button
              onClick={() => setActiveTab('dependencies')}
              className={`${
                activeTab === 'dependencies'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              Dependencies
            </button>
          </nav>
        </div>
        
        {/* Tab Content */}
        <div className="grid grid-cols-1 gap-6">
          {/* Overview Tab */}
          {activeTab === 'overview' && (
            <>
              {/* Project Workflow Status */}
              <div className="bg-white shadow rounded-lg overflow-hidden border border-gray-200">
                <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                  <h3 className="text-lg font-medium leading-6 text-gray-900">Project Workflow Status</h3>
                </div>
                <div className="px-4 py-5 sm:p-6">
                  <ol className="relative border-l border-gray-200">
                    <li className="mb-10 ml-4">
                      <div className="absolute w-3 h-3 bg-green-500 rounded-full mt-1.5 -left-1.5 border border-white"></div>
                      <time className="mb-1 text-sm font-normal leading-none text-gray-500">January 15, 2025</time>
                      <h3 className="text-lg font-semibold text-gray-900">Project Created</h3>
                      <p className="mb-4 text-sm font-normal text-gray-500">Project proposal created and submitted for initial review</p>
                    </li>
                    <li className="mb-10 ml-4">
                      <div className="absolute w-3 h-3 bg-green-500 rounded-full mt-1.5 -left-1.5 border border-white"></div>
                      <time className="mb-1 text-sm font-normal leading-none text-gray-500">January 20, 2025</time>
                      <h3 className="text-lg font-semibold text-gray-900">Sub PMO Approval</h3>
                      <p className="mb-4 text-sm font-normal text-gray-500">Project approved by Department PMO</p>
                    </li>
                    <li className="mb-10 ml-4">
                      <div className="absolute w-3 h-3 bg-green-500 rounded-full mt-1.5 -left-1.5 border border-white"></div>
                      <time className="mb-1 text-sm font-normal leading-none text-gray-500">January 25, 2025</time>
                      <h3 className="text-lg font-semibold text-gray-900">Main PMO Approval</h3>
                      <p className="mb-4 text-sm font-normal text-gray-500">Project received final approval from Enterprise PMO</p>
                    </li>
                    <li className="ml-4">
                      <div className="absolute w-3 h-3 bg-blue-500 rounded-full mt-1.5 -left-1.5 border border-white"></div>
                      <time className="mb-1 text-sm font-normal leading-none text-gray-500">In Progress</time>
                      <h3 className="text-lg font-semibold text-gray-900">Project Execution</h3>
                      <p className="text-sm font-normal text-gray-500">Project is currently in the execution phase</p>
                    </li>
                  </ol>
                </div>
              </div>
              
              {/* Recent Updates & Key Milestones */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white shadow rounded-lg overflow-hidden border border-gray-200">
                  <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                    <h3 className="text-lg font-medium leading-6 text-gray-900">Recent Updates</h3>
                  </div>
                  <div className="p-4">
                    <ul className="divide-y divide-gray-200">
                      {project.weeklyUpdates.slice(0, 2).map((update) => (
                        <li key={update.id} className="py-4">
                          <div className="flex space-x-3">
                            <div className="flex-shrink-0">
                              <div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center text-blue-600">
                                <FileText className="h-5 w-5" />
                              </div>
                            </div>
                            <div className="min-w-0 flex-1">
                              <p className="text-sm font-medium text-gray-900">Weekly Update - {formatDate(update.date)}</p>
                              <p className="text-sm text-gray-500">{update.accomplishments}</p>
                            </div>
                          </div>
                        </li>
                      ))}
                    </ul>
                    <div className="mt-4 text-center">
                      <button 
                        onClick={() => setActiveTab('updates')}
                        className="text-sm font-medium text-blue-600 hover:text-blue-500"
                      >
                        View all updates
                      </button>
                    </div>
                  </div>
                </div>
                
                <div className="bg-white shadow rounded-lg overflow-hidden border border-gray-200">
                  <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                    <h3 className="text-lg font-medium leading-6 text-gray-900">Key Milestones</h3>
                  </div>
                  <div className="p-4">
                    <ul className="divide-y divide-gray-200">
                      {project.milestones.map((milestone) => (
                        <li key={milestone.id} className="py-4">
                          <div className="flex items-center justify-between">
                            <div className="flex items-center">
                              {milestone.status === 'Completed' ? (
                                <CheckCircle className="h-5 w-5 text-green-500 mr-3" />
                              ) : milestone.status === 'In Progress' ? (
                                <Clock className="h-5 w-5 text-blue-500 mr-3" />
                              ) : (
                                <div className="h-5 w-5 rounded-full border-2 border-gray-300 mr-3"></div>
                              )}
                              <div>
                                <p className="text-sm font-medium text-gray-900">{milestone.name}</p>
                                <p className="text-xs text-gray-500">Due: {formatDate(milestone.dueDate)}</p>
                              </div>
                            </div>
                            <span 
                              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
                                ${milestone.status === 'Completed' ? 'bg-green-100 text-green-800' : 
                                  milestone.status === 'In Progress' ? 'bg-blue-100 text-blue-800' : 
                                  'bg-gray-100 text-gray-800'}`}
                            >
                              {milestone.status}
                            </span>
                          </div>
                        </li>
                      ))}
                    </ul>
                    <div className="mt-4 text-center">
                      <button 
                        onClick={() => setActiveTab('milestones')}
                        className="text-sm font-medium text-blue-600 hover:text-blue-500"
                      >
                        View all milestones
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </>
          )}
          
          {/* Team Tab */}
          {activeTab === 'team' && (
            <div className="bg-white shadow rounded-lg overflow-hidden border border-gray-200">
              <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                <h3 className="text-lg font-medium leading-6 text-gray-900">Project Team</h3>
                <p className="mt-1 max-w-2xl text-sm text-gray-500">
                  Team members and their roles on this project
                </p>
              </div>
              <div className="px-4 py-5 sm:p-6">
                <div className="mb-6">
                  <h4 className="text-base font-semibold text-gray-900 mb-3">Project Manager</h4>
                  <div className="flex items-center p-4 bg-gray-50 rounded-lg border border-gray-200">
                    <div className="flex-shrink-0 mr-4">
                      <div className="h-12 w-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600">
                        <Users className="h-6 w-6" />
                      </div>
                    </div>