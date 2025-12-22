using SRMS.Application.Dashboards.DTOs;
using SRMS.Application.Dashboards.Interfaces;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Managers;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Payments;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Repositories;
using SRMS.Domain.Residences;
using SRMS.Domain.Students;
using SRMS.Domain.Reservations;
using SRMS.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Reservations.Enums;

namespace SRMS.Infrastructure.Configurations.Services;

public class DashboardStatisticsService : IDashboardStatisticsService
{
    private readonly IRepositories<Student> _studentRepo;
    private readonly IRepositories<Manager> _managerRepo;
    private readonly IRepositories<Residence> _residenceRepo;
    private readonly IRepositories<Payment> _paymentRepo;
    private readonly IRepositories<Complaint> _complaintRepo;
    private readonly IRepositories<Reservation> _reservationRepo;
    private readonly IRepositories<Notification> _notificationRepo;

    public DashboardStatisticsService(
        IRepositories<Student> studentRepo,
        IRepositories<Manager> managerRepo,
        IRepositories<Residence> residenceRepo,
        IRepositories<Payment> paymentRepo,
        IRepositories<Complaint> complaintRepo,
        IRepositories<Reservation> reservationRepo,
        IRepositories<Notification> notificationRepo)
    {
        _studentRepo = studentRepo;
        _managerRepo = managerRepo;
        _residenceRepo = residenceRepo;
        _paymentRepo = paymentRepo;
        _complaintRepo = complaintRepo;
        _reservationRepo = reservationRepo;
        _notificationRepo = notificationRepo;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
    {
        var students = await _studentRepo.GetAllAsync();
        var managers = await _managerRepo.GetAllAsync();
        var residences = await _residenceRepo.GetAllAsync();
        var payments = await _paymentRepo.GetAllAsync();
        var complaints = await _complaintRepo.GetAllAsync();

        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        var totalCapacity = residences.Sum(r => r.TotalCapacity);
        var occupiedCapacity = residences.Sum(r => r.TotalCapacity - r.AvailableCapacity);

        return new DashboardOverviewDto
        {
            TotalResidences = residences.Count(),
            ActiveManagers = managers.Count(m => m.Status == ManagerStatus.Active),
            TotalStudents = students.Count(),
            TotalCapacity = totalCapacity,
            OccupiedCapacity = occupiedCapacity,
            OccupancyRate = totalCapacity > 0 ? (double)occupiedCapacity / totalCapacity * 100 : 0,

            PendingComplaints = complaints.Count(c =>
                c.Status == ComplaintStatus.Open || c.Status == ComplaintStatus.InProgress),
            ResolvedComplaints = complaints.Count(c => c.Status == ComplaintStatus.Resolved),

            ResidencesGrowth = residences.Count(r => r.CreatedAt >= lastMonth),
            ManagerAvailability = managers.Any() ?
                (double)managers.Count(m => m.Status == ManagerStatus.Active) / managers.Count() * 100 : 0,

            MonthlyRevenue = payments
                .Where(p => p.CreatedAt.Month == now.Month &&
                           p.CreatedAt.Year == now.Year &&
                           p.Status == PaymentStatus.Paid)
                .Sum(p => p.Amount?.Amount ?? 0),

            RevenueGrowth = CalculateRevenueGrowth(payments.ToList(), now)
        };
    }

    public async Task<ManagerStatisticsDto> GetManagerStatisticsAsync()
    {
        var managers = await _managerRepo.GetAllAsync();
        var students = await _studentRepo.GetAllAsync();

        var activeManagers = managers.Count(m => m.Status == ManagerStatus.Active);

        return new ManagerStatisticsDto
        {
            TotalManagers = managers.Count(),
            ActiveManagers = activeManagers,
            OnLeaveManagers = managers.Count(m => m.Status == ManagerStatus.OnLeave),
            SuspendedManagers = managers.Count(m => m.Status == ManagerStatus.Suspended),
            AverageWorkload = activeManagers > 0 ?
                (double)students.Count() / activeManagers : 0
        };
    }

    public async Task<ResidenceStatisticsDto> GetResidenceStatisticsAsync()
    {
        var residences = await _residenceRepo.GetAllAsync();

        var totalCapacity = residences.Sum(r => r.TotalCapacity);
        var availableCapacity = residences.Sum(r => r.AvailableCapacity);

        return new ResidenceStatisticsDto
        {
            TotalResidences = residences.Count(),
            TotalCapacity = totalCapacity,
            AvailableCapacity = availableCapacity,
            OccupiedCapacity = totalCapacity - availableCapacity,
            AverageOccupancy = totalCapacity > 0 ?
                (double)(totalCapacity - availableCapacity) / totalCapacity * 100 : 0,
            FullResidences = residences.Count(r => r.IsFull)
        };
    }

    public async Task<List<ChartDataPointDto>> GetStudentRegistrationTrendAsync(string period)
    {
        var students = await _studentRepo.GetAllAsync();
        var now = DateTime.UtcNow;

        return period switch
        {
            "week" => GetWeeklyTrend(students.ToList(), now),
            "month" => GetMonthlyTrend(students.ToList(), now),
            "year" => GetYearlyTrend(students.ToList(), now),
            _ => GetMonthlyTrend(students.ToList(), now)
        };
    }

    public async Task<List<ChartDataPointDto>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate)
    {
        var payments = await _paymentRepo.GetAllAsync();
        var filteredPayments = payments
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.Status == PaymentStatus.Paid)
            .ToList();

        var data = new List<ChartDataPointDto>();
        var days = (endDate - startDate).Days;
        var interval = Math.Max(1, days / 10);

        for (var date = startDate; date <= endDate; date = date.AddDays(interval))
        {
            var endRange = date.AddDays(interval);
            var revenue = filteredPayments
                .Where(p => p.CreatedAt.Date >= date && p.CreatedAt.Date < endRange)
                .Sum(p => p.Amount?.Amount ?? 0);

            data.Add(new ChartDataPointDto
            {
                Label = date.ToString("MMM dd"),
                Value = (double)revenue
            });
        }

        return data;
    }

    public async Task<List<OccupancyDataDto>> GetOccupancyByResidenceAsync()
    {
        var residences = await _residenceRepo.GetAllAsync();

        return residences.Select(r => new OccupancyDataDto
        {
            Name = r.Name,
            Total = r.TotalCapacity,
            Occupied = r.TotalCapacity - r.AvailableCapacity,
            Rate = r.TotalCapacity > 0 ?
                ((double)(r.TotalCapacity - r.AvailableCapacity) / r.TotalCapacity) * 100 : 0
        }).ToList();
    }

    public async Task<List<TopResidenceDataDto>> GetTopRevenueResidencesAsync(int count = 5)
    {
        var residences = await _residenceRepo.GetAllAsync();
        var payments = await _paymentRepo.GetAllAsync();
        var students = await _studentRepo.GetAllAsync();

        return residences.Select(r => new TopResidenceDataDto
        {
            Name = r.Name,
            Revenue = payments
                    .Where(p => p.Reservation != null && p.Reservation.Room != null && p.Reservation.Room.ResidenceId == r.Id && p.Status == PaymentStatus.Paid)
                .Sum(p => p.Amount?.Amount ?? 0),
            Students = students.Count(s => s.Reservations.Any(res => res.Room.ResidenceId == r.Id))
        })
        .OrderByDescending(r => r.Revenue)
        .Take(count)
        .ToList();
    }

    public async Task<StudentDashboardDataDto> GetStudentDashboardDataAsync(Guid studentId)
    {
        var student = await _studentRepo.GetByIdAsync(studentId);
        if (student == null) return new StudentDashboardDataDto();

        var reservations = (await _reservationRepo.FindAsync(r => r.StudentId == studentId)).ToList();
        var reservationIds = reservations.Select(r => r.Id).ToList();
        var activeReservation = reservations.OrderByDescending(r => r.CreatedAt).FirstOrDefault(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Confirmed);

        var complaints = await _complaintRepo.FindAsync(c => reservationIds.Contains(c.ReservationId));
        var payments = await _paymentRepo.FindAsync(p => p.ReservationId.HasValue && reservationIds.Contains(p.ReservationId.Value)); // تم اصلاح مشكلة عدم القدرة علي تمرير قيمة يحتمل ان تكون فارغة بوضع الشرط
        var notifications = await _notificationRepo.FindAsync(n => n.UserId == studentId || (student.Email != null && n.UserEmail == student.Email.Value));

        var dto = new StudentDashboardDataDto
        {
            Status = student.Status,
            ActiveComplaintsCount = complaints.Count(c => c.Status != ComplaintStatus.Resolved && c.Status != ComplaintStatus.Closed),
            DueAmount = payments.Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Overdue).Sum(p => p.Amount?.Amount ?? 0)
        };

        if (activeReservation != null)
        {
            var room = await _reservationRepo.Query().Where(r => r.Id == activeReservation.Id).Select(r => r.Room).FirstOrDefaultAsync();
            if (room != null)
            {
                dto.RoomNumber = room.RoomNumber;
                var residence = await _residenceRepo.GetByIdAsync(room.ResidenceId);
                dto.ResidenceName = residence?.Name;
            }
        }

        // Add recent activities
        var recentNotifications = notifications.OrderByDescending(n => n.CreatedAt).Take(5).Select(n => new RecentActivityDto
        {
            Title = n.Title,
            Timestamp = n.CreatedAt,
            Type = "Notification"
        });

        var recentPayments = payments.OrderByDescending(p => p.CreatedAt).Take(5).Select(p => new RecentActivityDto
        {
            Title = $"Payment of {p.Amount?.Amount} LYD",
            Timestamp = p.CreatedAt,
            Type = "Payment"
        });

        dto.RecentActivities = recentNotifications.Concat(recentPayments)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();

        return dto;
    }

    // Private helper methods
    private double CalculateRevenueGrowth(List<Payment> payments, DateTime now)
    {
        var currentMonth = payments
            .Where(p => p.CreatedAt.Month == now.Month &&
                       p.CreatedAt.Year == now.Year &&
                       p.Status == PaymentStatus.Paid)
            .Sum(p => p.Amount?.Amount ?? 0);

        var lastMonth = payments
            .Where(p => p.CreatedAt.Month == now.AddMonths(-1).Month &&
                       p.CreatedAt.Year == now.AddMonths(-1).Year &&
                       p.Status == PaymentStatus.Paid)
            .Sum(p => p.Amount?.Amount ?? 0);

        if (lastMonth == 0) return 0;

        return ((double)(currentMonth - lastMonth) / (double)lastMonth) * 100;
    }

    private List<ChartDataPointDto> GetWeeklyTrend(List<Student> students, DateTime now)
    {
        var data = new List<ChartDataPointDto>();

        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i);
            var count = students.Count(s => s.CreatedAt.Date == date.Date);
            data.Add(new ChartDataPointDto
            {
                Label = date.ToString("ddd"),
                Value = count
            });
        }

        return data;
    }

    private List<ChartDataPointDto> GetMonthlyTrend(List<Student> students, DateTime now)
    {
        var data = new List<ChartDataPointDto>();
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var interval = Math.Max(1, daysInMonth / 7);

        for (int i = 1; i <= daysInMonth; i += interval)
        {
            var startDate = new DateTime(now.Year, now.Month, i);
            var endDate = new DateTime(now.Year, now.Month, Math.Min(i + interval - 1, daysInMonth));
            var count = students.Count(s => s.CreatedAt.Date >= startDate && s.CreatedAt.Date <= endDate);

            data.Add(new ChartDataPointDto
            {
                Label = $"{i}-{Math.Min(i + interval - 1, daysInMonth)}",
                Value = count
            });
        }

        return data;
    }

    private List<ChartDataPointDto> GetYearlyTrend(List<Student> students, DateTime now)
    {
        var data = new List<ChartDataPointDto>();
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        for (int i = 0; i < 12; i++)
        {
            var month = i + 1;
            var count = students.Count(s => s.CreatedAt.Month == month && s.CreatedAt.Year == now.Year);
            data.Add(new ChartDataPointDto
            {
                Label = monthNames[i],
                Value = count
            });
        }

        return data;
    }
}