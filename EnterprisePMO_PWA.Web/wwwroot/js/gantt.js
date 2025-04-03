// Initialize Gantt chart
function initializeGantt(options) {
    gantt.config.date_format = options.dateFormat;
    gantt.config.row_height = options.rowHeight;
    gantt.config.column_width = options.columnWidth;
    gantt.config.header_height = options.headerHeight;
    gantt.config.view_mode = options.viewMode;
    gantt.config.language = options.language;
    gantt.config.custom_popup_html = options.customPopupHtml;

    // Configure columns
    gantt.config.columns = [
        { name: "text", label: "Task name", tree: true, width: 200 },
        { name: "start_date", label: "Start time", align: "center", width: 100 },
        { name: "end_date", label: "End time", align: "center", width: 100 },
        { name: "progress", label: "Progress", align: "center", width: 80 }
    ];

    // Configure scales
    gantt.config.scales = [
        { unit: "month", step: 1, format: "%F, %Y" },
        { unit: "week", step: 1, format: "Week #%W" },
        { unit: "day", step: 1, format: "%j %D" }
    ];

    // Configure task styling
    gantt.templates.task_class = function(start, end, task) {
        if (task.progress === 100) return "gantt-task-complete";
        if (task.progress > 0) return "gantt-task-in-progress";
        return "gantt-task-not-started";
    };

    // Initialize the chart
    gantt.init(options.container);
    gantt.parse({
        data: options.tasks,
        links: options.dependencies
    });
}

// Zoom in function
function ganttZoomIn() {
    gantt.ext.zoom.zoomIn();
}

// Zoom out function
function ganttZoomOut() {
    gantt.ext.zoom.zoomOut();
}

// Fit to screen function
function ganttFitToScreen() {
    gantt.ext.zoom.fitToScreen();
}

// Add CSS styles
const style = document.createElement('style');
style.textContent = `
    .gantt-container {
        width: 100%;
        height: 600px;
        margin: 20px 0;
    }

    .gantt-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 10px;
    }

    .gantt-controls {
        display: flex;
        gap: 10px;
    }

    .gantt-chart {
        width: 100%;
        height: calc(100% - 50px);
        border: 1px solid #ddd;
        border-radius: 4px;
    }

    .gantt-task-complete {
        background-color: #28a745;
    }

    .gantt-task-in-progress {
        background-color: #ffc107;
    }

    .gantt-task-not-started {
        background-color: #dc3545;
    }

    .gantt-popup {
        padding: 10px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 4px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
`;
document.head.appendChild(style); 