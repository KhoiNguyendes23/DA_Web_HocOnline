using System.ComponentModel.DataAnnotations;

public class OrderInfoModel
{
    public string? OrderId { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    [Range(1000, int.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    public int Amount { get; set; }

    [Required]
    public string OrderInfo { get; set; }
}
