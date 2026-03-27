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
        public string? Parts_Detail { get; set; } // JSON: [{"name":"Oil Filter","price":500},...]
    }
}
