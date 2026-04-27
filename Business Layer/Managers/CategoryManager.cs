using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.ProductDtos;
using Core_Layer.Exceptions;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Data_Access_Layer.Repositories;
using Entity_Layer;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Business_Layer.Managers
{
    public class CategoryManager : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CategoryManager(ICategoryRepository categoryRepository,IUnitOfWork uow, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TAddCategory(AddCategoryDto addCategoryDto)
        {
            var isExist = await _categoryRepository.AnyAsync(x => x.Name == addCategoryDto.Name);
            if (isExist)
                throw new LogicException(nameof(addCategoryDto.Name),"This category name is already exists.");

            var category = _mapper.Map<Category>(addCategoryDto);
            category.CreatedDate = DateTime.Now;
            category.IsDeleted = false;

            await _categoryRepository.AddAsync(category);
            await _uow.SaveAsync();
        }

        public async Task<List<CategoryListDto>> TGetAllCategories()
        {
            var categories = await _categoryRepository.GetAll().ToListAsync();

            return _mapper.Map<List<CategoryListDto>>(categories);
        }

        public async Task<CategoryListDto> TGetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new LogicException(nameof(CategoryListDto.Id), "The category not found");

            return _mapper.Map<CategoryListDto>(category); 
        }

        public async Task TUpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(updateCategoryDto.Id);
            if (category == null)
                throw new LogicException(nameof(updateCategoryDto.Id), "The category not found.");

            var isOtherCategoryExists = await _categoryRepository.AnyAsync(c=>c.Id !=  updateCategoryDto.Id && c.Name==updateCategoryDto.Name);
            if (isOtherCategoryExists)
                throw new LogicException(nameof(updateCategoryDto.Name), "This category name is already exists.");

            _mapper.Map(updateCategoryDto,category);
            category.UpdatedDate = DateTime.Now;

            _categoryRepository.Update(category);
            await _uow.SaveAsync();
        }
    }
}
