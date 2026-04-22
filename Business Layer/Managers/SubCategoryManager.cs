using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.SubCategoryDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class SubCategoryManager : ISubCategoryService
    {
        private readonly ISubCategoryRepository _subCategoryRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public SubCategoryManager(ISubCategoryRepository subCategoryRepository, IUnitOfWork uow,IMapper mapper)
        {
            _subCategoryRepository = subCategoryRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TAddSubCategoryAsync(AddSubCategoryDto addSubCategoryDto)
        {
            var isExist = await _subCategoryRepository.AnyAsync(x => x.Name == addSubCategoryDto.Name);
            if (isExist)
                throw new LogicException(nameof(addSubCategoryDto.Name), "This subcategory name is already exists.");

            var subCategory = _mapper.Map<SubCategory>(addSubCategoryDto);
            subCategory.CreatedDate = DateTime.Now;
            subCategory.IsDeleted = false;

            await _subCategoryRepository.AddAsync(subCategory);
            await _uow.SaveAsync();
        }

        public async Task<List<SubCategoryListDto>> TGetAllSubCategoriesByParentAsync(Guid parentId)
        {
            var subCategories = await _subCategoryRepository.Where(s=>s.CategoryId == parentId).ToListAsync();

            return _mapper.Map<List<SubCategoryListDto>>(subCategories);
        }

        public async Task<SubCategoryListDto> TGetByIdAsync(Guid id)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(id);
            if (subCategory == null)
                throw new LogicException(nameof(SubCategoryListDto.Id), "The subcategory not found");

            return _mapper.Map<SubCategoryListDto>(subCategory);
        }

        public async Task TUpdateSubCategoryAsync(UpdateSubCategoryDto updateSubCategoryDto)
        {
            var subCategory = await _subCategoryRepository.GetByIdAsync(updateSubCategoryDto.Id);
            if (subCategory == null)
                throw new LogicException(nameof(updateSubCategoryDto.Id), "The subcategory not found.");

            var isOtherSubCategoryExists = await _subCategoryRepository.AnyAsync(c => c.Id != updateSubCategoryDto.Id && c.Name == updateSubCategoryDto.Name);
            if (isOtherSubCategoryExists)
                throw new LogicException(nameof(updateSubCategoryDto.Name), "This subcategory name is already exists.");

            _mapper.Map(updateSubCategoryDto, subCategory);
            subCategory.UpdatedDate = DateTime.Now;

            _subCategoryRepository.Update(subCategory);
            await _uow.SaveAsync();
        }
    }
}
