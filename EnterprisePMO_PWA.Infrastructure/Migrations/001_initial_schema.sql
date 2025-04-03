-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create departments table
CREATE TABLE departments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username TEXT UNIQUE NOT NULL,
    supabase_id TEXT UNIQUE,
    role TEXT NOT NULL,
    department_id UUID REFERENCES departments(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create audit_logs table
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entity_name TEXT NOT NULL,
    entity_id UUID,
    action TEXT NOT NULL,
    change_summary TEXT,
    username TEXT NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    ip_address TEXT
);

-- Create projects table
CREATE TABLE projects (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    description TEXT,
    status TEXT NOT NULL,
    department_id UUID REFERENCES departments(id),
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create project_members table
CREATE TABLE project_members (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    user_id UUID REFERENCES users(id),
    role TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    UNIQUE(project_id, user_id)
);

-- Create weekly_updates table
CREATE TABLE weekly_updates (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    user_id UUID REFERENCES users(id),
    week_ending DATE NOT NULL,
    progress_summary TEXT,
    next_steps TEXT,
    risks_issues TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Create change_requests table
CREATE TABLE change_requests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    project_id UUID REFERENCES projects(id),
    title TEXT NOT NULL,
    description TEXT,
    status TEXT NOT NULL,
    priority TEXT NOT NULL,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

-- Enable Row Level Security
ALTER TABLE departments ENABLE ROW LEVEL SECURITY;
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE projects ENABLE ROW LEVEL SECURITY;
ALTER TABLE project_members ENABLE ROW LEVEL SECURITY;
ALTER TABLE weekly_updates ENABLE ROW LEVEL SECURITY;
ALTER TABLE change_requests ENABLE ROW LEVEL SECURITY;

-- Create policies for departments
CREATE POLICY "Departments are viewable by authenticated users"
    ON departments FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Departments can be managed by service role"
    ON departments FOR ALL
    TO service_role
    USING (true);

-- Create policies for users
CREATE POLICY "Users are viewable by authenticated users"
    ON users FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Users can be managed by service role"
    ON users FOR ALL
    TO service_role
    USING (true);

-- Create policies for audit_logs
CREATE POLICY "Audit logs are viewable by authenticated users"
    ON audit_logs FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Audit logs can be inserted by service role"
    ON audit_logs FOR INSERT
    TO service_role
    WITH CHECK (true);

-- Create policies for projects
CREATE POLICY "Projects are viewable by authenticated users"
    ON projects FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Projects can be managed by service role"
    ON projects FOR ALL
    TO service_role
    USING (true);

-- Create policies for project_members
CREATE POLICY "Project members are viewable by authenticated users"
    ON project_members FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Project members can be managed by service role"
    ON project_members FOR ALL
    TO service_role
    USING (true);

-- Create policies for weekly_updates
CREATE POLICY "Weekly updates are viewable by authenticated users"
    ON weekly_updates FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Weekly updates can be managed by service role"
    ON weekly_updates FOR ALL
    TO service_role
    USING (true);

-- Create policies for change_requests
CREATE POLICY "Change requests are viewable by authenticated users"
    ON change_requests FOR SELECT
    TO authenticated
    USING (true);

CREATE POLICY "Change requests can be managed by service role"
    ON change_requests FOR ALL
    TO service_role
    USING (true);

-- Create indexes for better performance
CREATE INDEX idx_users_supabase_id ON users(supabase_id);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_audit_logs_entity_id ON audit_logs(entity_id);
CREATE INDEX idx_projects_department_id ON projects(department_id);
CREATE INDEX idx_project_members_project_id ON project_members(project_id);
CREATE INDEX idx_project_members_user_id ON project_members(user_id);
CREATE INDEX idx_weekly_updates_project_id ON weekly_updates(project_id);
CREATE INDEX idx_weekly_updates_user_id ON weekly_updates(user_id);
CREATE INDEX idx_change_requests_project_id ON change_requests(project_id); 