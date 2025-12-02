using SRMS.Domain.Rooms.Enums;

namespace SRMS.Application.Rooms.Extensions;

public static class EnumExtensions
{
    // ========== Room Type ==========
    public static string ToArabic(this RoomType type) => type switch
    {
        RoomType.Single => "غرفة فردية",
        RoomType.Double => "غرفة مزدوجة",
        RoomType.Triple => "غرفة ثلاثية",
        RoomType.Quad => "غرفة رباعية",
        RoomType.Dormitory => "مهجع",
        RoomType.Suite => "جناح",
        _ => type.ToString()
    };
}