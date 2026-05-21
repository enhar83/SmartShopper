using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly INotificationService _notificationService;   
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IToxicityPredictionService _toxicityPredictionService;

        public CommentManager(
            ICommentRepository commentRepository,
            IOrderRepository orderRepository,
            INotificationService notificationService,
            IUnitOfWork uow,
            IMapper mapper,
            IToxicityPredictionService toxicityPredictionService)
        {
            _commentRepository = commentRepository;
            _orderRepository = orderRepository;
            _notificationService = notificationService;
            _uow = uow;
            _mapper = mapper;
            _toxicityPredictionService = toxicityPredictionService;
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

            var prediction = await _toxicityPredictionService.TPredictToxicityAsync(createCommentDto.Text);

            comment.CommentAnalysisResult = new CommentAnalysisResult
            {
                ToxicityScore = prediction.ToxicityScore,
                IsToxic = prediction.IsToxic,
                SentimentScore = 0.0 
            };

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
                .Include(c => c.CommentAnalysisResult)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var commentDtos = _mapper.Map<List<CommentListAdminPanelDto>>(comments);

            return commentDtos;
        }

        public async Task TToggleCommentApprovalAsync(Guid commentId)
        {
            var comment = await _commentRepository.Where(x => x.Id == commentId)
                .Include(x => x.Product)
                .FirstOrDefaultAsync();

            if (comment == null)
                throw new LogicException("NotFound", "No comment was found to process.");

            comment.IsApproved = !comment.IsApproved;
            _commentRepository.Update(comment);

            if (comment.IsApproved)
            {
                string productName = comment.Product != null ? comment.Product.Name : "our product";

                var notificationDto = new Core_Layer.Dtos.NotificationDtos.CreateNotificationDto
                {
                    AppUserId = comment.AppUserId,
                    Title = "Comment Approved",
                    Message = $"Your comment on '{productName}' has been approved and is now live. Thank you for your feedback!",
                    NotificationType = "Comment",
                    RelatedUrl = $"/Product/ProductDetails/{comment.ProductId}"
                };

                await _notificationService.TAddAsync(notificationDto);
            }

            await _uow.SaveAsync();
        }

        public async Task<List<UserCommentListDto>> TGetCommentsByUserIdAsync(Guid userId)
        {
            var comments = await _commentRepository
                .Where(c => c.AppUserId == userId && c.IsDeleted == false)
                .Include(c => c.Product)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var commentDtos = _mapper.Map<List<UserCommentListDto>>(comments);

            return commentDtos;
        }
    }
}