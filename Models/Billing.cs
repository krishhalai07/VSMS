namespace VSMS.Models
{
    public class Billing
    {
        public int B_id { get; set; }
        public int? J_id { get; set; }
        public int? Part_cost { get; set; }
        public int? Gst { get; set; }
        public int? Total_cost { get; set; }
        public string? Status { get; set; }
        public string? Payment_type { get; set; }
    }
}
