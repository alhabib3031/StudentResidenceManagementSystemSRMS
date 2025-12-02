namespace SRMS.Domain.Payments.Enums;

public enum PaymentMethod
{
    Cash = 1,           // نقداً
    BankTransfer = 2,   // تحويل بنكي
    CreditCard = 3,     // بطاقة ائتمان
    DebitCard = 4,      // بطاقة خصم
    MobilePayment = 5,  // دفع عبر الموبايل
    Check = 6           // شيك
}