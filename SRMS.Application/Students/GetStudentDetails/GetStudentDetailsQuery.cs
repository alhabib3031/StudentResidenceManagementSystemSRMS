using MediatR;
using SRMS.Application.Students.DTOs;

namespace SRMS.Application.Students.GetStudentDetails;

//  Query جديد للحصول على التفاصيل الكاملة
// ✅ Query للحصول على التفاصيل الكاملة - باستخدام Repository فقط
public class GetStudentDetailsQuery : IRequest<StudentDetailsDto?>
{
    public Guid Id { get; set; }
}