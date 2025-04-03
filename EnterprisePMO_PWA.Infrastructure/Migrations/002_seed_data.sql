-- Insert initial departments
INSERT INTO departments (id, name, description) VALUES
    ('00000000-0000-0000-0000-000000000001', 'Administration', 'Administrative department'),
    ('00000000-0000-0000-0000-000000000002', 'Holding', 'Holding department for new users');

-- Insert admin user (will be updated with actual Supabase ID after first login)
INSERT INTO users (id, username, role, department_id) VALUES
    ('00000000-0000-0000-0000-000000000001', 'admin@test.com', 'Admin', '00000000-0000-0000-0000-000000000001'); 