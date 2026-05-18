using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CommentDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class CommentManager : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CommentManager(ICommentRepository commentRepository, IOrderRepository orderRepository, IUnitOfWork uow, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _orderRepository = orderRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TAddAsync(CreateCommentDto createCommentDto, Guid userId)
        {
            bool canComment = await TCanUserCommentOnProductAsync(userId, createCommentDto.ProductId);

            if (!canComment)
                throw new LogicException("Error", "You cannot leave a review for this product before receiving your order.");

            var comment = _mapper.Map<Comment>(createCommentDto);

            comment.AppUserId = userId;
            comment.IsApproved = false; 
            comment.CreatedDate = DateTime.Now;
            comment.IsDeleted = false;

            await _commentRepository.AddAsync(comment);
            await _uow.SaveAsync();
        }

        public async Task<bool> TCanUserCommentOnProductAsync(Guid userId, Guid productId)
        {
            var hasBoughtProduct = await _orderRepository.AnyAsync(order =>
                order.AppUserId == userId &&
                order.Status == OrderStatus.Delivered && 
                order.OrderItems.Any(oi => oi.ProductId == productId)
            );

            return hasBoughtProduct;
        }

        public async Task<List<ResultCommentDto>> TGetApprovedCommentsByProductIdAsync(Guid productId)
        {
            var comments = await _commentRepository
                .Where(c => c.ProductId == productId && c.IsApproved == true && c.IsDeleted == false)
                .Include(c => c.AppUser)
                .OrderByDescending(c => c.CreatedDate) 
                .ToListAsync();

            var commentDtos = _mapper.Map<List<ResultCommentDto>>(comments);

            return commentDtos;
        }

        public async Task<List<CommentListAdminPanelDto>> TGetAllCommentsForAdminAsync()
        {
            var comments = await _commentRepository.GetAll()
                .Include(c => c.Product) 
                .Include(c => c.AppUser)
                .OrderByDescending(c => c.CreatedDate) 
                .ToListAsync();

            var commentDtos = _mapper.Map<List<CommentListAdminPanelDto>>(comments);

            return commentDtos;
        }

        public async Task TToggleCommentApprovalAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                throw new LogicException("NotFound", "No comment was found to process.\r\n");

            comment.IsApproved = !comment.IsApproved;

            _commentRepository.Update(comment);

            await _uow.SaveAsync();
        }
    }
}
