-- Add Role column to Users table
ALTER TABLE Users 
ADD Role NVARCHAR(20) NOT NULL DEFAULT 'Employee';

-- Update existing users to have Admin role (you can change this as needed)
-- UPDATE Users SET Role = 'Admin' WHERE Id = 1; -- Uncomment and modify as needed
