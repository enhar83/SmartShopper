using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Entities
{
    public interface IEntity
    {
    }
}

/*
 
    IEntity Neden Boş?
        - bir sınıfa etiket yapıştırır.
        - IEntity şunu der: ben bir db tablosuyum.

    Neden BaseEntity Miras Alıyor?
        - BaseEntity ortak özellikleri taşırken, IEntity sınıfın kimliğini belirler.
        - eğer BaseEntity, IEntityden miras alırsa; BaseEntityden türeyen tüm sınıflar otomatik olarak IEntity etiketiyle damgalanmış olur.

    GenericConstraits
        - neden boş bir arayüze ihtiyacımız olduğunun en net cevabı, ileride yazılacak repo katmanındadır.
        - bir generic repo yazarken her türden nesnenin oraya girmesi istenilmez. 
            * where T: class yazıldığı zaman string bir ifade bile oraya girebilir.      
            * where T: class, IEntity, new() olarak yazıldığı taktirde ise sadece IEntity etiketine sahip olan sınıflar buraya girebilir.
 
 */
