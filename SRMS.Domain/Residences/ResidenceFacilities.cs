namespace SRMS.Domain.Residences;

/// <summary>
/// ResidenceFacilities - المرافق المتوفرة في السكن
/// </summary>
public class ResidenceFacilities
{
    public bool HasWifi { get; set; }
    public bool HasParking { get; set; }
    public bool HasLaundry { get; set; }
    public bool HasGym { get; set; }
    public bool HasSwimmingPool { get; set; }
    public bool HasSecurity { get; set; }
    public bool HasKitchen { get; set; }
    public bool HasStudyRoom { get; set; }
    
    public List<string> GetAvailableFacilities()
    {
        var facilities = new List<string>();
        
        if (HasWifi) facilities.Add("WiFi");
        if (HasParking) facilities.Add("Parking");
        if (HasLaundry) facilities.Add("Laundry");
        if (HasGym) facilities.Add("Gym");
        if (HasSwimmingPool) facilities.Add("Swimming Pool");
        if (HasSecurity) facilities.Add("24/7 Security");
        if (HasKitchen) facilities.Add("Shared Kitchen");
        if (HasStudyRoom) facilities.Add("Study Room");
        
        return facilities;
    }
}