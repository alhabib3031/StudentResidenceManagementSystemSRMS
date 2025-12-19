using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Abstractions;
using SRMS.Domain.Repositories;

namespace SRMS.Infrastructure.Repositories;

/// <summary>
/// GenericRepository - Repository عام لكل الكيانات
/// يستخدم IDbContextFactory لإنشاء DbContext جديد في كل عملية لتجنب مشاكل Threading في Blazor Server
/// </summary>
/// <typeparam name="T">نوع الكيان (يجب أن يرث من Entity)</typeparam>
public class GenericRepository<T> : IRepositories<T> where T : Entity
{
    protected readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public GenericRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public virtual IQueryable<T> Query()
    {
        var context = _contextFactory.CreateDbContext();
        return context.Set<T>().AsQueryable().Where(e => !e.IsDeleted);
    }

    // ═══════════════════════════════════════════════════════════
    // Query Operations (القراءة)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على كل السجلات
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>()
            .Where(e => !e.IsDeleted)  // Soft Delete Filter
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على سجل بالـ ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    /// <summary>
    /// للـ Identity Users (string IDs)
    /// </summary>
    public virtual Task<T?> GetByIdAsync(string id)
    {
        throw new NotImplementedException("This repository does not support string IDs");
    }

    /// <summary>
    /// البحث بشرط معين
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>()
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على أول سجل يطابق الشرط
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>()
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(predicate);
    }

    // ═══════════════════════════════════════════════════════════
    // Command Operations (الكتابة)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// إنشاء سجل جديد
    /// </summary>
    public virtual async Task<T> CreateAsync(T entity)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;
        entity.IsActive = true;

        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// تحديث سجل
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        entity.UpdatedAt = DateTime.UtcNow;

        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// حذف سجل (Soft Delete)
    /// </summary>
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (entity == null)
            return false;

        // Soft Delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// حفظ التغييرات - لم يعد مستخدماً مع DbContextFactory
    /// </summary>
    public virtual async Task SaveChangesAsync()
    {
        // لا شيء - كل عملية تحفظ التغييرات بشكل منفصل
        await Task.CompletedTask;
    }

    // ═══════════════════════════════════════════════════════════
    // Additional Helper Methods
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// عد السجلات
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>().CountAsync(e => !e.IsDeleted);
    }

    /// <summary>
    /// عد السجلات بشرط
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>()
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .CountAsync();
    }

    /// <summary>
    /// التحقق من وجود سجل
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>().AnyAsync(e => e.Id == id && !e.IsDeleted);
    }

    /// <summary>
    /// التحقق من وجود سجل بشرط
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<T>()
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .AnyAsync();
    }

    /// <summary>
    /// الحصول على سجلات مع Pagination
    /// </summary>
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        IQueryable<T> query = context.Set<T>().Where(e => !e.IsDeleted);

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Hard Delete - حذف نهائي (استخدم بحذر!)
    /// </summary>
    public virtual async Task<bool> HardDeleteAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null)
            return false;

        context.Set<T>().Remove(entity);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// استرجاع سجل محذوف (Soft Delete)
    /// </summary>
    public virtual async Task<bool> RestoreAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.Set<T>()
            .IgnoreQueryFilters()  // تجاهل Query Filter
            .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);

        if (entity == null)
            return false;

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;

        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();

        return true;
    }
}