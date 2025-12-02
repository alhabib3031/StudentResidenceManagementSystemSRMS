namespace SRMS.Application.Identity.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string Manage = "Permissions.Users.Manage";
        public const string Activate = "Permissions.Users.Activate";
        public const string Deactivate = "Permissions.Users.Deactivate";
    }

    public static class Students
    {
        public const string View = "Permissions.Students.View";
        public const string ViewOwn = "Permissions.Students.ViewOwn";
        public const string Edit = "Permissions.Students.Edit";
        public const string EditOwn = "Permissions.Students.EditOwn";
        public const string Delete = "Permissions.Students.Delete";
        public const string Manage = "Permissions.Students.Manage";
    }

    public static class Managers
    {
        public const string View = "Permissions.Managers.View";
        public const string Create = "Permissions.Managers.Create";
        public const string Edit = "Permissions.Managers.Edit";
        public const string Delete = "Permissions.Managers.Delete";
        public const string Manage = "Permissions.Managers.Manage";
    }

    public static class Rooms
    {
        public const string View = "Permissions.Rooms.View";
        public const string Create = "Permissions.Rooms.Create";
        public const string Edit = "Permissions.Rooms.Edit";
        public const string Delete = "Permissions.Rooms.Delete";
        public const string Manage = "Permissions.Rooms.Manage";
        public const string Assign = "Permissions.Rooms.Assign";
    }

    public static class Payments
    {
        public const string View = "Permissions.Payments.View";
        public const string ViewOwn = "Permissions.Payments.ViewOwn";
        public const string ViewAll = "Permissions.Payments.ViewAll";
        public const string Create = "Permissions.Payments.Create";
        public const string Edit = "Permissions.Payments.Edit";
        public const string Delete = "Permissions.Payments.Delete";
    }

    public static class Complaints
    {
        public const string View = "Permissions.Complaints.View";
        public const string ViewOwn = "Permissions.Complaints.ViewOwn";
        public const string Create = "Permissions.Complaints.Create";
        public const string Edit = "Permissions.Complaints.Edit";
        public const string Delete = "Permissions.Complaints.Delete";
        public const string Manage = "Permissions.Complaints.Manage";
        public const string Resolve = "Permissions.Complaints.Resolve";
    }

    public static class Residences
    {
        public const string View = "Permissions.Residences.View";
        public const string Create = "Permissions.Residences.Create";
        public const string Edit = "Permissions.Residences.Edit";
        public const string Delete = "Permissions.Residences.Delete";
        public const string Manage = "Permissions.Residences.Manage";
        public const string ManageAll = "Permissions.Residences.ManageAll";
    }

    public static class Assets
    {
        public const string View = "Permissions.Assets.View";
        public const string Create = "Permissions.Assets.Create";
        public const string Edit = "Permissions.Assets.Edit";
        public const string Delete = "Permissions.Assets.Delete";
        public const string Manage = "Permissions.Assets.Manage";
    }

    public static class System
    {
        public const string ViewReports = "Permissions.System.ViewReports";
        public const string Settings = "Permissions.System.Settings";
        public const string Audit = "Permissions.System.Audit";
        public const string Configure = "Permissions.System.Configure";
    }

    public static class Database
    {
        public const string Manage = "Permissions.Database.Manage";
    }

    public static List<string> GetAllPermissions()
    {
        var permissions = new List<string>();
        var nestedTypes = typeof(Permissions).GetNestedTypes();

        foreach (var type in nestedTypes)
        {
            foreach (var field in type.GetFields())
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    permissions.Add((string)field.GetValue(null)!);
                }
            }
        }

        return permissions;
    }
}
