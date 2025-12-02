using MudBlazor;
using SRMS.Domain.Managers;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Residences;
using SRMS.Domain.Students;
using SRMS.WebUI.Server.Components.Pages.Dialogs;

namespace SRMS.WebUI.Server.Components.Pages.Admin;

public partial class ManagersManagement
{
    private List<Manager> _managers = new();
    private List<Residence> _residences = new();
    private List<Student> _students = new();
    private bool _isLoading = true;
    private string _searchString = "";
    private string _filterStatus = "all";

    private readonly List<BreadcrumbItem> _breadcrumbs = new()
    {
        new BreadcrumbItem("Admin", href: "/admin", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Managers", href: null, disabled: true)
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        _isLoading = true;
        try
        {
            _managers = (await ManagerRepo.GetAllAsync()).ToList();
            _residences = (await ResidenceRepo.GetAllAsync()).ToList();
            _students = (await StudentRepo.GetAllAsync()).ToList();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private IEnumerable<Manager> GetFilteredManagers()
    {
        var filtered = _managers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_searchString))
        {
            filtered = filtered.Where(m =>
                m.FirstName.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                m.LastName.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
                (m.Email?.Value.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.EmployeeNumber?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        filtered = _filterStatus switch
        {
            "active" => filtered.Where(m => m.Status == ManagerStatus.Active),
            "onleave" => filtered.Where(m => m.Status == ManagerStatus.OnLeave),
            "suspended" => filtered.Where(m => m.Status == ManagerStatus.Suspended),
            "terminated" => filtered.Where(m => m.Status == ManagerStatus.Terminated),
            _ => filtered
        };

        return filtered;
    }

    private void ClearFilters()
    {
        _searchString = "";
        _filterStatus = "all";
    }

    private string GetInitials(string firstName, string lastName)
    {
        return $"{firstName.FirstOrDefault()}{lastName.FirstOrDefault()}".ToUpper();
    }

    private string GetTenure(DateTime hireDate)
    {
        var years = (DateTime.Today - hireDate).TotalDays / 365.25;
        if (years < 1)
        {
            var months = (DateTime.Today.Year - hireDate.Year) * 12 + DateTime.Today.Month - hireDate.Month;
            return $"{months} month{(months != 1 ? "s" : "")}";
        }

        return $"{years:F1} years";
    }

    private double GetAverageWorkload()
    {
        if (!_managers.Any()) return 0;
        return (double)_students.Count / _managers.Count(m => m.Status == ManagerStatus.Active);
    }

    private List<ManagerPerformance> GetManagerPerformance()
    {
        return _managers
            .Where(m => m.Status == ManagerStatus.Active)
            .Select(m => new ManagerPerformance
            {
                Name = m.FullName,
                ResidenceCount = m.Residences.Count,
                StudentCount = m.Students.Count,
                OccupancyRate = CalculateOccupancyRate(m)
            })
            .OrderByDescending(p => p.StudentCount)
            .ToList();
    }

    private double CalculateOccupancyRate(Manager manager)
    {
        var totalCapacity = manager.Residences.Sum(r => r.TotalCapacity);
        if (totalCapacity == 0) return 0;
        var occupied = manager.Residences.Sum(r => r.TotalCapacity - r.AvailableCapacity);
        return ((double)occupied / totalCapacity) * 100;
    }

    private Color GetWorkloadColor(double rate)
    {
        return rate >= 90 ? Color.Error : rate >= 70 ? Color.Warning : Color.Success;
    }

    private string GetWorkloadLevel(int studentCount)
    {
        return studentCount switch
        {
            < 20 => "Low",
            < 50 => "Medium",
            _ => "High"
        };
    }

    private Color GetStatusColor(ManagerStatus status)
    {
        return status switch
        {
            ManagerStatus.Active => Color.Success,
            ManagerStatus.OnLeave => Color.Warning,
            ManagerStatus.Suspended => Color.Error,
            ManagerStatus.Terminated => Color.Dark,
            _ => Color.Default
        };
    }

    private Color GetWorkloadColorByLevel(int studentCount)
    {
        var level = GetWorkloadLevel(studentCount);
        return level switch
        {
            "Low" => Color.Success,
            "Medium" => Color.Info,
            "High" => Color.Warning,
            _ => Color.Default
        };
    }

    private void ViewDetails(Manager manager)
    {
        Navigation.NavigateTo($"/admin/managers/{manager.Id}");
    }

    private void EditManager(Manager manager)
    {
        Navigation.NavigateTo($"/admin/managers/edit/{manager.Id}");
    }

    private void AssignResidences(Manager manager)
    {
        Navigation.NavigateTo($"/admin/managers/{manager.Id}/assign-residences");
    }

    private async Task SetOnLeave(Manager manager)
    {
        var parameters = new DialogParameters<ConfirmDialog>
        {
            { x => x.ContentText, $"Set '{manager.FullName}' as On Leave?" },
            { x => x.ButtonText, "Confirm" },
            { x => x.Color, Color.Warning }
        };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirm Status Change", parameters);
        var result = await dialog.Result;

        if (result != null && !result.Canceled)
        {
            manager.Status = ManagerStatus.OnLeave;
            await ManagerRepo.UpdateAsync(manager);
            Snackbar.Add($"{manager.FullName} is now on leave", Severity.Info);
            await LoadData();
        }
    }

    private async Task SetActive(Manager manager)
    {
        manager.Status = ManagerStatus.Active;
        await ManagerRepo.UpdateAsync(manager);
        Snackbar.Add($"{manager.FullName} is now active", Severity.Success);
        await LoadData();
    }

    private async Task DeleteManager(Manager manager)
    {
        var parameters = new DialogParameters<ConfirmDialog>
        {
            {
                x => x.ContentText,
                $"Are you sure you want to delete '{manager.FullName}'? This action cannot be undone."
            },
            { x => x.ButtonText, "Delete" },
            { x => x.Color, Color.Error }
        };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirm Delete", parameters);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            try
            {
                await ManagerRepo.DeleteAsync(manager.Id);
                Snackbar.Add($"Manager '{manager.FullName}' deleted successfully", Severity.Success);
                await LoadData();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
        }
    }

    public class ManagerPerformance
    {
        public string Name { get; set; } = "";
        public int ResidenceCount { get; set; }
        public int StudentCount { get; set; }
        public double OccupancyRate { get; set; }
    }
}