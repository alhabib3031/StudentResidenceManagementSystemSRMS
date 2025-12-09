using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Abstractions;
using SRMS.Domain.Repositories;

namespace SRMS.Infrastructure.Repositories;

/// <summary>
/// GenericRepository - Repository عام لكل الكيانات
/// </summary>
/// <typeparam name="T">نوع الكيان (يجب أن يرث من Entity)</typeparam>
public class GenericRepository<T> : IRepositories<T> where T : Entity
{
    protected readonly IDbContextFactory<ApplicationDbContext> _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(IDbContextFactory<ApplicationDbContext> context)
    {
        _context = context;
        _dbSet = context.CreateDbContext().Set<T>();
    }

    // ═══════════════════════════════════════════════════════════
    // Query Operations (القراءة)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على كل السجلات
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)  // Soft Delete Filter
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على سجل بالـ ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet
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
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    /// <summary>
    /// الحصول على أول سجل يطابق الشرط
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
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
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;
        entity.IsActive = true;

        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// تحديث سجل
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        _dbSet.Update(entity);
        await SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// حذف سجل (Soft Delete)
    /// </summary>
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        // Soft Delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.IsActive = false;

        await UpdateAsync(entity);

        return true;
    }

    /// <summary>
    /// حفظ التغييرات
    /// </summary>
    public virtual async Task SaveChangesAsync()
    {
        await _context.CreateDbContext().SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════
    // Additional Helper Methods
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// عد السجلات
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync(e => !e.IsDeleted);
    }

    /// <summary>
    /// عد السجلات بشرط
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(e => !e.IsDeleted && predicate.Compile()(e));
    }

    /// <summary>
    /// التحقق من وجود سجل
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }

    /// <summary>
    /// التحقق من وجود سجل بشرط
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(e => !e.IsDeleted && predicate.Compile()(e));
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
        IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);

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
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// استرجاع سجل محذوف (Soft Delete)
    /// </summary>
    public virtual async Task<bool> RestoreAsync(Guid id)
    {
        var entity = await _dbSet
            .IgnoreQueryFilters()  // تجاهل Query Filter
            .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);

        if (entity == null)
            return false;

        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await SaveChangesAsync();

        return true;
    }
}