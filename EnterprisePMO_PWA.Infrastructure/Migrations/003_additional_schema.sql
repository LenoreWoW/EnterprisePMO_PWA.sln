-- Create strategic_goals table
CREATE TABLE strategic_goals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title TEXT NOT NULL,
    description TEXT,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    status TEXT NOT NULL,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create annual_goals table
CREATE TABLE annual_goals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    strategic_goal_id UUID REFERENCES strategic_goals(id),
    title TEXT NOT NULL,
    description TEXT,
    year INTEGER NOT NULL,
    status TEXT NOT NULL,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_tasks table
CREATE TABLE project_tasks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    title TEXT NOT NULL,
    description TEXT,
    status TEXT NOT NULL,
    priority TEXT NOT NULL,
    start_date DATE,
    end_date DATE,
    assigned_to UUID REFERENCES users(id),
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_milestones table
CREATE TABLE project_milestones (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    title TEXT NOT NULL,
    description TEXT,
    due_date DATE NOT NULL,
    status TEXT NOT NULL,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_attachments table
CREATE TABLE project_attachments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    file_name TEXT NOT NULL,
    file_path TEXT NOT NULL,
    file_type TEXT NOT NULL,
    file_size INTEGER NOT NULL,
    uploaded_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_kpis table
CREATE TABLE project_kpis (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    name TEXT NOT NULL,
    description TEXT,
    target_value NUMERIC,
    actual_value NUMERIC,
    unit TEXT,
    frequency TEXT NOT NULL,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_charters table
CREATE TABLE project_charters (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    content TEXT NOT NULL,
    version INTEGER NOT NULL,
    status TEXT NOT NULL,
    approved_by UUID REFERENCES users(id),
    approved_at TIMESTAMP WITH TIME ZONE,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_financials table
CREATE TABLE project_financials (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    budget_amount NUMERIC NOT NULL,
    spent_amount NUMERIC NOT NULL,
    forecast_amount NUMERIC NOT NULL,
    currency TEXT NOT NULL,
    period TEXT NOT NULL,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_dependencies table
CREATE TABLE project_dependencies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    source_project_id UUID REFERENCES projects(id),
    target_project_id UUID REFERENCES projects(id),
    dependency_type TEXT NOT NULL,
    description TEXT,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    UNIQUE(source_project_id, target_project_id)
);

-- Create project_risks table
CREATE TABLE project_risks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    title TEXT NOT NULL,
    description TEXT,
    impact TEXT NOT NULL,
    probability TEXT NOT NULL,
    status TEXT NOT NULL,
    mitigation_plan TEXT,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_issues table
CREATE TABLE project_issues (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    title TEXT NOT NULL,
    description TEXT,
    severity TEXT NOT NULL,
    status TEXT NOT NULL,
    resolution TEXT,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Enable Row Level Security
ALTER TABLE strategic_goals ENABLE ROW LEVEL SECURITY;
ALTER TABLE annual_goals ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_tasks ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_milestones ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_attachments ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_kpis ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_charters ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_financials ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_dependencies ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_risks ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_issues ENABLE ROW LEVEL SECURITY;

-- Create policies for strategic_goals
CREATE POLICY "Strategic goals are viewable by authenticated users"
    ON strategic_goals FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Strategic goals can be managed by service role"
    ON strategic_goals FOR ALL
    TO service_role
    USING (true);

-- Create policies for annual_goals
CREATE POLICY "Annual goals are viewable by authenticated users"
    ON annual_goals FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Annual goals can be managed by service role"
    ON annual_goals FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_tasks
CREATE POLICY "Project tasks are viewable by authenticated users"
    ON project_tasks FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project tasks can be managed by service role"
    ON project_tasks FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_milestones
CREATE POLICY "Project milestones are viewable by authenticated users"
    ON project_milestones FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project milestones can be managed by service role"
    ON project_milestones FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_attachments
CREATE POLICY "Project attachments are viewable by authenticated users"
    ON project_attachments FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project attachments can be managed by service role"
    ON project_attachments FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_kpis
CREATE POLICY "Project KPIs are viewable by authenticated users"
    ON project_kpis FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project KPIs can be managed by service role"
    ON project_kpis FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_charters
CREATE POLICY "Project charters are viewable by authenticated users"
    ON project_charters FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project charters can be managed by service role"
    ON project_charters FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_financials
CREATE POLICY "Project financials are viewable by authenticated users"
    ON project_financials FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project financials can be managed by service role"
    ON project_financials FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_dependencies
CREATE POLICY "Project dependencies are viewable by authenticated users"
    ON project_dependencies FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project dependencies can be managed by service role"
    ON project_dependencies FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_risks
CREATE POLICY "Project risks are viewable by authenticated users"
    ON project_risks FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project risks can be managed by service role"
    ON project_risks FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_issues
CREATE POLICY "Project issues are viewable by authenticated users"
    ON project_issues FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project issues can be managed by service role"
    ON project_issues FOR ALL
    TO service_role
    USING (true);

-- Create indexes for better performance
CREATE INDEX idx_strategic_goals_status ON strategic_goals(status);
CREATE INDEX idx_annual_goals_year ON annual_goals(year);
CREATE INDEX idx_project_tasks_project_id ON project_tasks(project_id);
CREATE INDEX idx_project_tasks_assigned_to ON project_tasks(assigned_to);
CREATE INDEX idx_project_milestones_project_id ON project_milestones(project_id);
CREATE INDEX idx_project_attachments_project_id ON project_attachments(project_id);
CREATE INDEX idx_project_kpis_project_id ON project_kpis(project_id);
CREATE INDEX idx_project_charters_project_id ON project_charters(project_id);
CREATE INDEX idx_project_financials_project_id ON project_financials(project_id);
CREATE INDEX idx_project_dependencies_source ON project_dependencies(source_project_id);
CREATE INDEX idx_project_dependencies_target ON project_dependencies(target_project_id);
CREATE INDEX idx_project_risks_project_id ON project_risks(project_id);
CREATE INDEX idx_project_issues_project_id ON project_issues(project_id); 