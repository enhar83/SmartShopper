using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core_Layer.Dtos.ProductImagesDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Business_Layer.Managers
{
    public class ProductImageManager : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProductImageManager(IProductImageRepository productImageRepository, IUnitOfWork uow, IMapper mapper)
        {
            _productImageRepository = productImageRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task TAddProductImageAsync(AddProductImageDto addProductImageDto)
        {
            var resourcePath = Directory.GetCurrentDirectory(); //uygulamanın sunucuda çalıştığı kök dizini alır. 
            var extension = Path.GetExtension(addProductImageDto.Photo.FileName); //.jpg mi yoksa .png mi olduğunu öğrenir. 
            var imageName = Guid.NewGuid() + extension; //iki tane aynı isimde foto yüklenirse karışmamaları için

            var directoryPath = Path.Combine(resourcePath, "wwwroot", "images", "products");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var savePath = Path.Combine(resourcePath, "wwwroot", "images", "products", imageName); //PathCombine ile dosya yollarını birleştirir. hem Windows hem Linux sunucularında hatasız çalışmasını sağlar.

            using (var stream = new FileStream(savePath, FileMode.Create)) //dosyayı diske yazmak için bir kanal açar. FileMode.Create ile buraya yeni bir dosya yarat deriz.
            {
                //using ile dosya yazma işlemi biter bitmez kanalı kapatır ve belleği boşaltır. bu sunucunun şişmesini engeller.
                await addProductImageDto.Photo.CopyToAsync(stream); //CopyToAsync: dosya diske yazılırken uygulamanın ana threadini kilitlemez. resim yazılırken sunucu diğer istekleri karşılamaya devam eder.
            }

            if (addProductImageDto.IsMain) //eğer kullanıcı yüklediği resmi main photo olarak işaretlerse SetExitingImagesToNotMain metoduna gider.
                await SetExistingImagesToNotMain(addProductImageDto.ProductId);

            else //eğer kullanıcı kapak yapma demiş olsa bile db üzerinde hiç ürüne ait resim yoksa ilk yüklenen resim otomatik olarak kapak yapılır.
            {
                var anyImage = await _productImageRepository.AnyAsync(x => x.ProductId == addProductImageDto.ProductId);
                if (!anyImage) addProductImageDto.IsMain = true;
            }

            var productImage = _mapper.Map<ProductImage>(addProductImageDto);
            productImage.ImageUrl = "/images/products/" + imageName; //dbye dosyanın sunucudaki fiziksel yolunu değil web yolu kaydolur. Böylece tarayıcıdan yazıldığında da resim görülebilir.

            await _productImageRepository.AddAsync(productImage);
            await _uow.SaveAsync();
        }

        public async Task<List<ProductImageListDto>> TGetProductImagesByProductIdAsync(Guid productId)
        {
            var images = await _productImageRepository.GetAll()
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.IsMain) 
                .ThenByDescending(x => x.CreatedDate) 
                .ToListAsync();

            return _mapper.Map<List<ProductImageListDto>>(images);
        }

        private async Task SetExistingImagesToNotMain(Guid productId)
        {
            var allImages = _productImageRepository.GetAll();
            var mainImages = await allImages.Where(x => x.ProductId == productId && x.IsMain).ToListAsync();
            foreach (var img in mainImages)
            {
                img.IsMain = false;
                _productImageRepository.Update(img);
            }
        }
    }
}
