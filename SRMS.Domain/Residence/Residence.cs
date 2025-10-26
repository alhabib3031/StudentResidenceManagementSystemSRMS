namespace SRMS.Domain.Residence;

public class Residence
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public bool IsFull { get; set; }
    
}