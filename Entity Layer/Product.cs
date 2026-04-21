using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer.Common;

namespace Entity_Layer
{
    public class Product:BaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public GenderType? Gender { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public Guid SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }
    }
}

public enum GenderType
{
    Children,
    Male,
    Female,
    Unisex
}

/*
 
    burada subcategory olarak alınmalıdır, eğer category koyulursa mantıklı olmaz.
        - zaten subcategory categorynin child'ı.
        - eğer product içerisinde category olursa, duplicatte ve inconsistency sıkıntı yaşanır.
        - önce category seçilsin ardından subcategory olarak düşünülürse bu bir frontend logictir, database logic değildir. 
        - akış içerisinde ilk olarak kullanıcı category'i çeker ardından ajax ile subcategory çekilebilir olarak düşünülebilir.
 
 */