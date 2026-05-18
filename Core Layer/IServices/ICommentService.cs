using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CommentDtos;
using Entity_Layer;

namespace Core_Layer.IServices
{
    public interface ICommentService
    {
        Task TAddAsync(CreateCommentDto createCommentDto, Guid userId);
        Task<bool> TCanUserCommentOnProductAsync(Guid userId, Guid productId);
        Task<List<ResultCommentDto>> TGetApprovedCommentsByProductIdAsync(Guid productId);
    }
}
