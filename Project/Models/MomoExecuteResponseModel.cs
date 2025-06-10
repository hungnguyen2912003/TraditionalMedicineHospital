namespace Models
{
    public class MomoExecuteResponseModel
    {
        public decimal Amount { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
    }
}