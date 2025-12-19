namespace SRMS.Domain.Identity.Constants;

public static class Roles
{
    public const string SuperRoot = "SuperRoot";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Student = "Student";
    public const string CollegeRegistrar = "CollegeRegistrar";

    public static class Permissions
    {
        // Student Permissions
        public const string ViewOwnProfile = "Permissions.Students.ViewOwn";
        public const string EditOwnProfile = "Permissions.Students.EditOwn";
        public const string SubmitComplaint = "Permissions.Complaints.Create";
        public const string ViewOwnComplaints = "Permissions.Complaints.ViewOwn";
        public const string ViewOwnPayments = "Permissions.Payments.ViewOwn";

        // Manager Permissions
        public const string ManageStudents = "Permissions.Students.Manage";
        public const string ManageRooms = "Permissions.Rooms.Manage";
        public const string ManageComplaints = "Permissions.Complaints.Manage";
        public const string ViewAllPayments = "Permissions.Payments.ViewAll";
        public const string ManageResidence = "Permissions.Residences.Manage";

        // Admin Permissions
        public const string ManageManagers = "Permissions.Managers.Manage";
        public const string ManageResidences = "Permissions.Residences.ManageAll";
        public const string ViewReports = "Permissions.Reports.View";
        public const string ManageSettings = "Permissions.Settings.Manage";
        public const string ViewAuditLogs = "Permissions.Audit.View";

        // SuperRoot Permissions
        public const string ManageRoles = "Permissions.Roles.Manage";
        public const string ManagePermissions = "Permissions.Permissions.Manage";
        public const string ManageUsers = "Permissions.Users.Manage";
        public const string SystemConfiguration = "Permissions.System.Configure";
        public const string DatabaseManagement = "Permissions.Database.Manage";
    }
}