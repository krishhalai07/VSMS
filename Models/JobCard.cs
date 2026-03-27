namespace VSMS.Models
{
    public class JobCard
    {
        public int J_id { get; set; }
        public int? Vech_id { get; set; }
        public string? Problem { get; set; }
        public DateTime? Date_in { get; set; }
        public DateTime? Date_out { get; set; }
        public string? Mech_Name { get; set; }
        public string? Status { get; set; }
        // Extra fields for customer view
        public string? Car_Name { get; set; }
        public string? RegId { get; set; }
    }
}
