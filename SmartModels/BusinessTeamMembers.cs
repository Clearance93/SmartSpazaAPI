using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class BusinessTeamMembers
    {
        [Key]
        public Guid BusinessTeamMembersId { get; set; }

        public Guid SupplierBusinessInfoId { get; set; }

        public string? MemberFullNames { get; set; }

        public string? MemberEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}
