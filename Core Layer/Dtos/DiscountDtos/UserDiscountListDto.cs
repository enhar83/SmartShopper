using System;
using static Entity_Layer.Discount;

namespace Core_Layer.Dtos.DiscountDtos
{
    public class UserDiscountListDto
    {
        public Guid AssignmentId { get; set; }
        public string CampaignName { get; set; } = null!;
        public string? Description { get; set; }
        public DiscountType Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsExpired => DateTime.Now > EndDate;
        public bool IsNotStartedYet => DateTime.Now < StartDate;
    }
}