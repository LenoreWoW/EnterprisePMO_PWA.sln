import React, { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, ReferenceLine } from 'recharts';
import { AlertCircle } from 'lucide-react';

const ProjectGanttChart = ({ projectId }) => {
  const [tasks, setTasks] = useState([]);
  const [milestones, setMilestones] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [today] = useState(new Date());
  const [dateRange, setDateRange] = useState({ start: null, end: null });
  const [projectData, setProjectData] = useState(null);

  useEffect(() => {
    // Function to fetch project data
    const fetchProjectData = async () => {
      try {
        setLoading(true);
        
        // Fetch project details
        const projectResponse = await fetch(`/api/projects/${projectId}`);
        if (!projectResponse.ok) {
          throw new Error('Failed to fetch project data');
        }
        const project = await projectResponse.json();
        setProjectData(project);
        
        // Fetch tasks for this project
        const tasksResponse = await fetch(`/api/projects/${projectId}/tasks`);
        if (!tasksResponse.ok) {
          throw new Error('Failed to fetch tasks');
        }
        const tasksData = await tasksResponse.json();
        
        // Fetch milestones for this project
        const milestonesResponse = await fetch(`/api/projects/${projectId}/milestones`);
        if (!milestonesResponse.ok) {
          throw new Error('Failed to fetch milestones');
        }
        const milestonesData = await milestonesResponse.json();
        
        // Process tasks for the Gantt chart
        const processedTasks = tasksData.map(task => {
          const startDate = new Date(task.startDate);
          const endDate = new Date(task.endDate);
          
          return {
            name: task.name,
            start: startDate,
            end: endDate,
            duration: Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24)),
            progress: task.progress || 0,
            type: 'task',
            dependsOn: task.dependsOn || [],
            owner: task.assignedTo,
            status: task.status,
            id: task.id
          };
        });
        
        // Process milestones
        const processedMilestones = milestonesData.map(milestone => {
          return {
            name: milestone.name,
            start: new Date(milestone.date),
            end: new Date(milestone.date),
            duration: 0,
            type: 'milestone',
            id: milestone.id
          };
        });
        
        // Combine and sort by date
        const allItems = [...processedTasks, ...processedMilestones].sort((a, b) => a.start - b.start);
        
        // Calculate date range
        if (allItems.length > 0) {
          const startDates = allItems.map(item => item.start);
          const endDates = allItems.map(item => item.end);
          
          const earliestDate = new Date(Math.min(...startDates));
          const latestDate = new Date(Math.max(...endDates));
          
          // Add buffer days
          earliestDate.setDate(earliestDate.getDate() - 7);
          latestDate.setDate(latestDate.getDate() + 7);
          
          setDateRange({ start: earliestDate, end: latestDate });
        } else if (project) {
          // If no tasks/milestones, use project dates
          setDateRange({ 
            start: new Date(project.startDate), 
            end: new Date(project.endDate) 
          });
        }
        
        setTasks(processedTasks);
        setMilestones(processedMilestones);
      } catch (err) {
        console.error('Error fetching data:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    
    if (projectId) {
      fetchProjectData();
    }
  }, [projectId]);

  // Generate data for the Gantt chart
  const generateGanttData = () => {
    if (!dateRange.start || !dateRange.end) return [];
    
    const startDate = dateRange.start;
    const endDate = dateRange.end;
    
    // For demo purposes, we'll use some sample data if the API calls failed
    const demoTasks = [
      { 
        name: "Requirements Gathering", 
        start: new Date("2025-04-01"), 
        end: new Date("2025-04-15"),
        progress: 100,
        status: "Completed"
      },
      { 
        name: "Design Phase", 
        start: new Date("2025-04-16"), 
        end: new Date("2025-05-10"),
        progress: 80,
        status: "In Progress"
      },
      { 
        name: "Development", 
        start: new Date("2025-05-01"), 
        end: new Date("2025-06-30"),
        progress: 50,
        status: "In Progress"
      },
      { 
        name: "Testing", 
        start: new Date("2025-06-15"), 
        end: new Date("2025-07-15"),
        progress: 20,
        status: "In Progress"
      },
      { 
        name: "Deployment", 
        start: new Date("2025-07-16"), 
        end: new Date("2025-07-31"),
        progress: 0,
        status: "Not Started"
      }
    ];
    
    const demoMilestones = [
      { name: "Project Kickoff", start: new Date("2025-04-01"), type: "milestone" },
      { name: "Design Approval", start: new Date("2025-05-10"), type: "milestone" },
      { name: "Alpha Release", start: new Date("2025-06-30"), type: "milestone" },
      { name: "Beta Release", start: new Date("2025-07-15"), type: "milestone" },
      { name: "Final Release", start: new Date("2025-07-31"), type: "milestone" }
    ];
    
    // Use actual data if available, otherwise use demo data
    const tasksToUse = tasks.length > 0 ? tasks : demoTasks;
    const milestonesToUse = milestones.length > 0 ? milestones : demoMilestones;
    
    // Calculate total duration in days
    const totalDays = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24));
    
    // Format data for the chart
    return tasksToUse.map(task => {
      // Calculate position and width percentages
      const startDiff = Math.max(0, (task.start - startDate) / (1000 * 60 * 60 * 24));
      const startPosition = (startDiff / totalDays) * 100;
      
      const duration = Math.ceil((task.end - task.start) / (1000 * 60 * 60 * 24));
      const widthPercentage = (duration / totalDays) * 100;
      
      return {
        name: task.name,
        startPosition,
        duration: widthPercentage,
        progress: task.progress,
        status: task.status,
        startDate: task.start.toLocaleDateString(),
        endDate: task.end.toLocaleDateString()
      };
    });
  };
  
  const ganttData = generateGanttData();

  // Function to determine bar color based on status
  const getBarColor = (status) => {
    switch(status) {
      case 'Completed': return '#4CAF50';
      case 'In Progress': return '#2196F3';
      case 'At Risk': return '#FFC107';
      case 'Delayed': return '#FF5722';
      case 'Not Started': return '#9E9E9E';
      default: return '#2196F3';
    }
  };

  // Custom tooltip for the Gantt chart
  const CustomTooltip = ({ active, payload }) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      return (
        <div className="bg-white p-4 shadow rounded border">
          <p className="font-bold">{data.name}</p>
          <p>Start: {data.startDate}</p>
          <p>End: {data.endDate}</p>
          <p>Progress: {data.progress}%</p>
          <p>Status: {data.status}</p>
        </div>
      );
    }
    
    return null;
  };

  // Today reference line position
  const getTodayPosition = () => {
    if (!dateRange.start || !dateRange.end) return 0;
    
    const totalDays = Math.ceil((dateRange.end - dateRange.start) / (1000 * 60 * 60 * 24));
    const todayDiff = Math.ceil((today - dateRange.start) / (1000 * 60 * 60 * 24));
    
    return (todayDiff / totalDays) * 100;
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-48">
        <div className="text-center">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-blue-600 border-r-transparent"></div>
          <p className="mt-2 text-gray-600">Loading Gantt chart...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded">
        <div className="flex">
          <div className="flex-shrink-0">
            <AlertCircle className="h-5 w-5 text-red-500" />
          </div>
          <div className="ml-3">
            <p className="text-sm text-red-700">
              Error loading Gantt chart: {error}
            </p>
          </div>
        </div>
      </div>
    );
  }

  if (ganttData.length === 0) {
    return (
      <div className="bg-yellow-50 border-l-4 border-yellow-500 p-4 rounded">
        <div className="flex">
          <div className="flex-shrink-0">
            <AlertCircle className="h-5 w-5 text-yellow-500" />
          </div>
          <div className="ml-3">
            <p className="text-sm text-yellow-700">
              No tasks or milestones found for this project. Please add tasks to see the Gantt chart.
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white p-4 rounded-lg shadow">
      <h2 className="text-xl font-semibold mb-4">Project Timeline</h2>
      
      <div className="mb-4">
        <div className="flex items-center text-sm text-gray-600 mb-2">
          <div className="w-3 h-3 rounded-full bg-blue-500 mr-2"></div>
          <span>In Progress</span>
          
          <div className="w-3 h-3 rounded-full bg-green-500 mx-4 mr-2"></div>
          <span>Completed</span>
          
          <div className="w-3 h-3 rounded-full bg-yellow-500 mx-4 mr-2"></div>
          <span>At Risk</span>
          
          <div className="w-3 h-3 rounded-full bg-red-500 mx-4 mr-2"></div>
          <span>Delayed</span>
          
          <div className="w-3 h-3 rounded-full bg-gray-500 mx-4 mr-2"></div>
          <span>Not Started</span>
        </div>
        
        <div className="text-sm text-gray-600 mb-2">
          <span>Project Duration: </span>
          <span className="font-medium">
            {projectData ? 
              `${new Date(projectData.startDate).toLocaleDateString()} to ${new Date(projectData.endDate).toLocaleDateString()}` :
              "Loading..."}
          </span>
        </div>
      </div>
      
      <div className="h-96">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart
            layout="vertical"
            data={ganttData}
            margin={{ top: 20, right: 30, left: 150, bottom: 5 }}
            barSize={20}
          >
            <CartesianGrid strokeDasharray="3 3" horizontal={false} />
            <XAxis 
              type="number" 
              domain={[0, 100]}
              ticks={[0, 25, 50, 75, 100]}
              allowDataOverflow
              tickFormatter={(value) => `${value}%`}
            />
            <YAxis 
              dataKey="name" 
              type="category" 
              width={120}
              tick={{ fontSize: 12 }}
            />
            <Tooltip content={<CustomTooltip />} />
            <Bar 
              dataKey="duration" 
              stackId="a" 
              fill="#2196F3" 
              background={{ fill: '#eee' }}
              radius={[4, 4, 4, 4]}
            >
              {ganttData.map((entry, index) => (
                <rect 
                  key={`rect-${index}`} 
                  x={`${entry.startPosition}%`} 
                  width={`${entry.duration}%`} 
                  height={15} 
                  fill={getBarColor(entry.status)}
                  rx={4}
                  ry={4}
                />
              ))}
            </Bar>
            <ReferenceLine 
              x={getTodayPosition()}
              stroke="red" 
              strokeWidth={2}
              label={{ value: 'Today', position: 'top', fill: 'red', fontSize: 12 }}
            />
          </BarChart>
        </ResponsiveContainer>
      </div>
      
      <div className="mt-4 text-xs text-gray-500">
        * Dates are shown in percentage of total project duration. The red line represents today's date.
      </div>
    </div>
  );
};

export default ProjectGanttChart;