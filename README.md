```sql
-- Users Table
CREATE TABLE Users (
  user_id SERIAL PRIMARY KEY,
  username VARCHAR(50) UNIQUE NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  email VARCHAR(100) UNIQUE NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- Sample data for Users
INSERT INTO Users (username, password_hash, email) VALUES
('john_doe', 'hashed_password_1', 'john@example.com'),
('jane_doe', 'hashed_password_2', 'jane@example.com');

-- Roles Table
CREATE TABLE Roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) UNIQUE NOT NULL,
    description VARCHAR(255)
);

-- Sample data for Roles
INSERT INTO Roles (role_name, description) VALUES
('admin', 'Administrator with full access'),
('editor', 'Editor with limited access'),
('viewer', 'Viewer with read-only access');

-- Permissions Table
CREATE TABLE Permissions (
    permission_id SERIAL PRIMARY KEY,
    permission_name VARCHAR(50) UNIQUE NOT NULL,
    description VARCHAR(255)
);

-- Sample data for Permissions
INSERT INTO Permissions (permission_name, description) VALUES
('create_content', 'Permission to create content'),
('edit_content', 'Permission to edit content'),
('delete_content', 'Permission to delete content'),
('view_content', 'Permission to view content');

-- UserRoles Table
CREATE TABLE UserRoles (
    user_id INT,
    role_id INT,
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES Roles(role_id) ON DELETE CASCADE
);

-- Sample data for UserRoles
INSERT INTO UserRoles (user_id, role_id) VALUES
(1, 1), -- john_doe as admin
(2, 2), -- jane_doe as editor
(2, 3); -- jane_doe as viewer

-- RolePermissions Table
CREATE TABLE RolePermissions (
    role_id INT,
    permission_id INT,
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES Roles(role_id) ON DELETE CASCADE,
    FOREIGN KEY (permission_id) REFERENCES Permissions(permission_id) ON DELETE CASCADE
);

-- Sample data for RolePermissions
INSERT INTO RolePermissions (role_id, permission_id) VALUES
(1, 1), -- admin with create_content
(1, 2), -- admin with edit_content
(1, 3), -- admin with delete_content
(1, 4), -- admin with view_content
(2, 2), -- editor with edit_content
(2, 4), -- editor with view_content
(3, 4); -- viewer with view_content

-- UserPermissions Table
CREATE TABLE UserPermissions (
    user_id INT,
    permission_id INT,
    PRIMARY KEY (user_id, permission_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (permission_id) REFERENCES Permissions(permission_id) ON DELETE CASCADE
);

-- Sample data for UserPermissions
INSERT INTO UserPermissions (user_id, permission_id) VALUES
(1, 4); -- john_doe with view_content directly
```