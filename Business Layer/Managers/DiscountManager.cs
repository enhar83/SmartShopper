using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.DiscountDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class DiscountManager : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IUnitOfWork _uow; 
        private readonly IMapper _mapper;

        public DiscountManager(IDiscountRepository discountRepository, IUnitOfWork uow, IMapper mapper)
        {
            _discountRepository = discountRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TCreateDiscountAsync(DiscountCreateDto createDto)
        {
            bool isDiscountNameExists = await _discountRepository.AnyAsync(x => x.Name == createDto.Name);

            if (isDiscountNameExists)
                throw new LogicException("Name", "A discount campaign with this name already exists. Please choose a different name.");

            var discountEntity = _mapper.Map<Discount>(createDto);
            discountEntity.CreatedDate = DateTime.Now;
            discountEntity.IsDeleted = false;

            await _discountRepository.AddAsync(discountEntity);

            await _uow.SaveAsync();
        }

        public async Task<List<DiscountListDto>> TGetAllDiscountsAsync()
        {
            var discounts = await _discountRepository.GetAll().ToListAsync();
            return _mapper.Map<List<DiscountListDto>>(discounts);
        }

        public async Task<DiscountUpdateDto> GetDiscountForUpdateAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
                throw new LogicException("NotFound", "No discount campaign was found to update.");

            return _mapper.Map<DiscountUpdateDto>(discount);
        }

        public async Task TUpdateDiscountAsync(DiscountUpdateDto updateDto)
        {
            bool isDiscountNameExists = await _discountRepository.AnyAsync(x => x.Name == updateDto.Name && x.Id != updateDto.Id);

            if (isDiscountNameExists)
                throw new LogicException("Name", "There is already another discount campaign with this name. Please choose a different name.");

            var existingDiscount = await _discountRepository.GetByIdAsync(updateDto.Id);
            if (existingDiscount == null)
                throw new LogicException("NotFound", "No discount campaign was found to update.");

            _mapper.Map(updateDto, existingDiscount);
            existingDiscount.UpdatedDate = DateTime.Now; 

            _discountRepository.Update(existingDiscount);
            await _uow.SaveAsync();
        }

        public async Task TDeleteDiscountAsync(Guid id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
                throw new LogicException("NotFound", "No campaign was found to delete.");

            _discountRepository.Remove(discount);
            await _uow.SaveAsync();
        }
    }
}
