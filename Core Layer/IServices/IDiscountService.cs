using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.DiscountDtos;

namespace Core_Layer.IServices
{
    public interface IDiscountService
    {
        Task TCreateDiscountAsync(DiscountCreateDto createDto);
        Task<List<DiscountListDto>> TGetAllDiscountsAsync();
        Task<DiscountUpdateDto> GetDiscountForUpdateAsync(Guid id);
        Task TUpdateDiscountAsync(DiscountUpdateDto updateDto);
        Task TDeleteDiscountAsync(Guid id);
        Task TAssignDiscountToUserAsync(AssignDiscountDto assignDto);
        Task<List<DiscountAssignedUserDto>> TGetUsersByDiscountIdAsync(Guid discountId);
        Task TRemoveDiscountFromUserAsync(Guid assignmentId);
    }
}
