import React, { useState } from 'react';
import { 
  Plus, 
  MoreHorizontal, 
  User, 
  Calendar, 
  Clock, 
  Tag,
  CheckCircle2,
  XCircle,
  AlertCircle
} from 'lucide-react';

const ProjectKanbanBoard = () => {
  // Sample data - in a real implementation this would be fetched from an API
  const initialColumns = {
    todo: {
      id: 'todo',
      title: 'To Do',
      taskIds: ['task1', 'task2', 'task3'],
      color: 'bg-gray-200'
    },
    inProgress: {
      id: 'inProgress',
      title: 'In Progress',
      taskIds: ['task4', 'task5'],
      color: 'bg-blue-200'
    },
    review: {
      id: 'review',
      title: 'Review',
      taskIds: ['task6'],
      color: 'bg-yellow-200'
    },
    done: {
      id: 'done',
      title: 'Done',
      taskIds: ['task7', 'task8'],
      color: 'bg-green-200'
    }
  };

  const initialTasks = {
    'task1': {
      id: 'task1',
      title: 'Research existing ERP solutions',
      description: 'Compare features, pricing, and compatibility with current systems',
      priority: 'High',
      dueDate: '2025-02-10',
      assignee: 'Sarah Chen',
      tags: ['Research', 'Planning']
    },
    'task2': {
      id: 'task2',
      title: 'Draft project requirements document',
      description: 'Gather requirements from all departments and document them',
      priority: 'Medium',
      dueDate: '2025-02-15',
      assignee: 'David Kim',
      tags: ['Documentation']
    },
    'task3': {
      id: 'task3',
      title: 'Create budget proposal',
      description: 'Estimate costs for implementation, licenses, and ongoing maintenance',
      priority: 'High',
      dueDate: '2025-02-20',
      assignee: 'Alex Johnson',
      tags: ['Finance', 'Planning']
    },
    'task4': {
      id: 'task4',
      title: 'Develop data migration strategy',
      description: 'Plan for extraction, transformation, and loading of data from legacy systems',
      priority: 'Medium',
      dueDate: '2025-03-05',
      assignee: 'Miguel Rodriguez',
      tags: ['Technical', 'Data']
    },
    'task5': {
      id: 'task5',
      title: 'Conduct vendor meetings',
      description: 'Meet with shortlisted vendors for demos and Q&A',
      priority: 'Medium',
      dueDate: '2025-03-10',
      assignee: 'Sarah Chen',
      tags: ['Vendor', 'Meetings']
    },
    'task6': {
      id: 'task6',
      title: 'Review vendor proposals',
      description: 'Evaluate proposals against requirements and budget constraints',
      priority: 'High',
      dueDate: '2025-03-15',
      assignee: 'Alex Johnson',
      tags: ['Vendor', 'Evaluation']
    },
    'task7': {
      id: 'task7',
      title: 'Complete stakeholder interviews',
      description: 'Interview key stakeholders from all departments to gather requirements',
      priority: 'Medium',
      dueDate: '2025-02-05',
      assignee: 'Priya Patel',
      tags: ['Research', 'Meetings']
    },
    'task8': {
      id: 'task8',
      title: 'Finalize project charter',
      description: 'Complete and get approval for project charter document',
      priority: 'High',
      dueDate: '2025-02-01',
      assignee: 'Alex Johnson',
      tags: ['Documentation', 'Approval']
    }
  };

  // State management
  const [columns, setColumns] = useState(initialColumns);
  const [tasks, setTasks] = useState(initialTasks);
  const [draggedTask, setDraggedTask] = useState(null);
  const [editingTask, setEditingTask] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [newTaskColumn, setNewTaskColumn] = useState(null);

  // Get priority styling
  const getPriorityStyles = (priority) => {
    switch (priority.toLowerCase()) {
      case 'high':
        return 'bg-red-100 text-red-800';
      case 'medium':
        return 'bg-yellow-100 text-yellow-800';
      case 'low':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  // Format date
  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  };

  // Calculate if date is overdue
  const isOverdue = (dateString) => {
    const today = new Date();
    const dueDate = new Date(dateString);
    return dueDate < today;
  };

  // Handle drag start
  const handleDragStart = (taskId) => {
    setDraggedTask(taskId);
  };

  // Handle drag over
  const handleDragOver = (event, columnId) => {
    event.preventDefault();
  };

  // Handle drop
  const handleDrop = (event, columnId) => {
    event.preventDefault();

    if (draggedTask) {
      // Find the source column
      const sourceColumnId = Object.keys(columns).find(colId => 
        columns[colId].taskIds.includes(draggedTask)
      );

      // Don't do anything if dropping in the same column
      if (sourceColumnId === columnId) {
        setDraggedTask(null);
        return;
      }

      // Create new column states
      const newColumns = {...columns};
      
      // Remove from source column
      newColumns[sourceColumnId].taskIds = columns[sourceColumnId].taskIds
        .filter(id => id !== draggedTask);
      
      // Add to destination column
      newColumns[columnId].taskIds = [...columns[columnId].taskIds, draggedTask];
      
      // Update state
      setColumns(newColumns);
      setDraggedTask(null);
    }
  };

  // Handle open modal for new task
  const handleAddTask = (columnId) => {
    setNewTaskColumn(columnId);
    setEditingTask({
      id: `task${Date.now()}`,
      title: '',
      description: '',
      priority: 'Medium',
      dueDate: '',
      assignee: '',
      tags: []
    });
    setShowModal(true);
  };

  // Handle open modal for edit task
  const handleEditTask = (task) => {
    setEditingTask({...task});
    setShowModal(true);
  };

  // Save task
  const handleSaveTask = () => {
    if (editingTask) {
      // Update task if it already exists
      const newTasks = {...tasks, [editingTask.id]: editingTask};
      setTasks(newTasks);

      // If it's a new task, add it to the column
      if (newTaskColumn && !Object.values(columns).some(col => col.taskIds.includes(editingTask.id))) {
        const newColumns = {...columns};
        newColumns[newTaskColumn].taskIds.push(editingTask.id);
        setColumns(newColumns);
        setNewTaskColumn(null);
      }

      setShowModal(false);
      setEditingTask(null);
    }
  };

  return (
    <div className="bg-gray-50 min-h-screen p-4 sm:p-6 lg:p-8">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Project Task Board</h1>
        <p className="mt-1 text-sm text-gray-500">
          Drag and drop tasks between columns to update their status
        </p>
      </div>
      
      {/* Filters and Actions */}
      <div className="mb-6 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex flex-wrap gap-2">
          <div className="relative">
            <select className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md">
              <option>All Tasks</option>
              <option>My Tasks</option>
              <option>Unassigned</option>
            </select>
          </div>
          
          <div className="relative">
            <select className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md">
              <option>All Priorities</option>
              <option>High</option>
              <option>Medium</option>
              <option>Low</option>
            </select>
          </div>
        </div>
        
        <div>
          <button className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
            <Plus className="h-4 w-4 mr-2" />
            New Task
          </button>
        </div>
      </div>
      
      {/* Kanban Board */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {Object.values(columns).map(column => (
          <div 
            key={column.id}
            className="bg-white rounded-lg shadow"
            onDragOver={(e) => handleDragOver(e, column.id)}
            onDrop={(e) => handleDrop(e, column.id)}
          >
            <div className={`px-4 py-3 rounded-t-lg ${column.color}`}>
              <div className="flex justify-between items-center">
                <h3 className="text-sm font-semibold text-gray-900">
                  {column.title}
                  <span className="ml-2 text-xs font-normal text-gray-500">
                    ({column.taskIds.length})
                  </span>
                </h3>
                <button
                  onClick={() => handleAddTask(column.id)} 
                  className="p-1 rounded-full hover:bg-gray-200"
                >
                  <Plus className="h-4 w-4 text-gray-600" />
                </button>
              </div>
            </div>
            
            <div className="p-2 h-full max-h-[calc(100vh-220px)] overflow-y-auto">
              {column.taskIds.length === 0 ? (
                <div className="flex items-center justify-center h-24 border-2 border-dashed border-gray-200 rounded-lg">
                  <p className="text-sm text-gray-500">No tasks</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {column.taskIds.map(taskId => {
                    const task = tasks[taskId];
                    return (
                      <div 
                        key={task.id}
                        draggable
                        onDragStart={() => handleDragStart(task.id)}
                        className={`bg-white border rounded-lg shadow-sm p-3 cursor-move
                          ${draggedTask === task.id ? 'opacity-50' : 'opacity-100'}`}
                      >
                        <div className="flex justify-between items-start mb-2">
                          <h4 className="text-sm font-medium text-gray-900">{task.title}</h4>
                          <div className="flex">
                            <button 
                              onClick={() => handleEditTask(task)}
                              className="text-gray-400 hover:text-gray-600"
                            >
                              <MoreHorizontal className="h-4 w-4" />
                            </button>
                          </div>
                        </div>
                        
                        {task.description && (
                          <p className="text-xs text-gray-500 mb-2">{task.description}</p>
                        )}
                        
                        <div className="flex flex-wrap gap-1 mb-2">
                          {task.tags && task.tags.map(tag => (
                            <span 
                              key={tag} 
                              className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800"
                            >
                              <Tag className="h-3 w-3 mr-1" />
                              {tag}
                            </span>
                          ))}
                        </div>
                        
                        <div className="flex items-center justify-between">
                          <div className="flex items-center">
                            <div className="h-6 w-6 rounded-full bg-gray-100 flex items-center justify-center mr-2 text-xs font-medium text-gray-800">
                              {task.assignee ? task.assignee.split(' ').map(n => n[0]).join('') : '?'}
                            </div>
                            <span className="text-xs text-gray-500">{task.assignee || 'Unassigned'}</span>
                          </div>
                          
                          <div className="flex items-center">
                            <span className={`flex items-center text-xs font-medium px-2 py-0.5 rounded-full ${getPriorityStyles(task.priority)}`}>
                              {task.priority === 'High' ? (
                                <AlertCircle className="h-3 w-3 mr-1" />
                              ) : task.priority === 'Medium' ? (
                                <Clock className="h-3 w-3 mr-1" />
                              ) : (
                                <CheckCircle2 className="h-3 w-3 mr-1" />
                              )}
                              {task.priority}
                            </span>
                          </div>
                        </div>
                        
                        {task.dueDate && (
                          <div className={`text-xs mt-2 ${isOverdue(task.dueDate) ? 'text-red-600' : 'text-gray-500'}`}>
                            <Calendar className="h-3 w-3 inline mr-1" />
                            Due: {formatDate(task.dueDate)}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
      
      {/* Task Edit Modal */}
      {showModal && (
        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 transition-opacity" aria-hidden="true">
              <div className="absolute inset-0 bg-gray-500 opacity-75"></div>
            </div>
            
            <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>
            
            <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="sm:flex sm:items-start">
                  <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                    <h3 className="text-lg leading-6 font-medium text-gray-900">
                      {editingTask.id.includes('task') && !tasks[editingTask.id] ? 'New Task' : 'Edit Task'}
                    </h3>
                    
                    <div className="mt-2">
                      <div className="mb-4">
                        <label htmlFor="title" className="block text-sm font-medium text-gray-700">Title</label>
                        <input
                          type="text"
                          id="title"
                          className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                          value={editingTask.title}
                          onChange={(e) => setEditingTask({...editingTask, title: e.target.value})}
                        />
                      </div>
                      
                      <div className="mb-4">
                        <label htmlFor="description" className="block text-sm font-medium text-gray-700">Description</label>
                        <textarea
                          id="description"
                          rows="3"
                          className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                          value={editingTask.description}
                          onChange={(e) => setEditingTask({...editingTask, description: e.target.value})}
                        ></textarea>
                      </div>
                      
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                        <div>
                          <label htmlFor="assignee" className="block text-sm font-medium text-gray-700">Assignee</label>
                          <select
                            id="assignee"
                            className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md"
                            value={editingTask.assignee}
                            onChange={(e) => setEditingTask({...editingTask, assignee: e.target.value})}
                          >
                            <option value="">Unassigned</option>
                            <option value="Alex Johnson">Alex Johnson</option>
                            <option value="Sarah Chen">Sarah Chen</option>
                            <option value="Miguel Rodriguez">Miguel Rodriguez</option>
                            <option value="Priya Patel">Priya Patel</option>
                            <option value="David Kim">David Kim</option>
                          </select>
                        </div>
                        
                        <div>
                          <label htmlFor="priority" className="block text-sm font-medium text-gray-700">Priority</label>
                          <select
                            id="priority"
                            className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md"
                            value={editingTask.priority}
                            onChange={(e) => setEditingTask({...editingTask, priority: e.target.value})}
                          >
                            <option>High</option>
                            <option>Medium</option>
                            <option>Low</option>
                          </select>
                        </div>
                      </div>
                      
                      <div className="mb-4">
                        <label htmlFor="dueDate" className="block text-sm font-medium text-gray-700">Due Date</label>
                        <input
                          type="date"
                          id="dueDate"
                          className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                          value={editingTask.dueDate}
                          onChange={(e) => setEditingTask({...editingTask, dueDate: e.target.value})}
                        />
                      </div>
                      
                      <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700">Tags</label>
                        <div className="mt-1 flex flex-wrap gap-2">
                          {['Research', 'Planning', 'Technical', 'Documentation', 'Meetings', 'Finance', 'Vendor', 'Approval', 'Data', 'Evaluation'].map(tag => (
                            <button
                              key={tag}
                              type="button"
                              onClick={() => {
                                const currentTags = editingTask.tags || [];
                                if (currentTags.includes(tag)) {
                                  setEditingTask({
                                    ...editingTask, 
                                    tags: currentTags.filter(t => t !== tag)
                                  });
                                } else {
                                  setEditingTask({
                                    ...editingTask,
                                    tags: [...currentTags, tag]
                                  });
                                }
                              }}
                              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium 
                                ${editingTask.tags && editingTask.tags.includes(tag) 
                                  ? 'bg-blue-100 text-blue-800' 
                                  : 'bg-gray-100 text-gray-800'}`}
                            >
                              {tag}
                            </button>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                <button
                  type="button"
                  className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm"
                  onClick={handleSaveTask}
                >
                  Save
                </button>
                <button
                  type="button"
                  className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                  onClick={() => {
                    setShowModal(false);
                    setEditingTask(null);
                    setNewTaskColumn(null);
                  }}
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectKanbanBoard;