(function ($) {
    "use strict";
    
// JS Index
//----------------------------------------
// 1. mobile-menu(mean-menu)
// 2. sticky menu
// 3. background image
// 4. quantity input arrow
// 5. mobile-menu-sidebar
// 6. header-search
// 7. header-shopping-cart
// 8. header-setting
// 9. mobile header-setting
// 10. home12 menu bar
// 11. slider - active
// 12. Price filter active 
// 13. accordion(sidebar) 
// 14. tooltip
// 15. product-active
// 16. product-home10-active
// 17. blog-post-active
// 18. product-new-arrivals-active
// 19. product-new-arrivals-active
// 20. top-product-category-home12-active
// 21. product-new-arrivals-active
// 22. home5-product-active
// 23. product-tab-thamb-active
// 24. home5-product-active
// 25. wow animation  active
// 26. showlogin toggle function
// 27. showcoupon toggle function
// 28. Create an account toggle function
// 29. Create an account toggle function
// 30. lsotope activation
// 31. Animate the scroll to top
// 32. preloader activation
// 33. Audio player activation
//-------------------------------------------------


// 1. mobile-menu(mean-menu)
//---------------------------------------------------------------------------
$("#mobile-menu").meanmenu({
    meanMenuContainer: ".mobile-menu",
    meanScreenWidth: "991",
});


// 2.sticky menu
//---------------------------------------------------------------------------
var wind = $(window);
var sticky = $("#header-sticky");
wind.on('scroll', function () {
    var scroll = $(wind).scrollTop();
    if (scroll < 50) {
        sticky.removeClass("sticky-menu");
    } else {
        $("#header-sticky").addClass("sticky-menu");
    }
});


// 3. background image
//---------------------------------------------------------------------------
$("[data-background]").each(function (){
    $(this).css("background-image","url(" + $(this).attr("data-background") + ")");
});


// 4 quantity input arrow
//---------------------------------------------------------------------------      
$('.quantity-input-2').inputarrow({
    renderPrev: function(input) {
        return $('<span class="custom-next"><i class="fas fa-minus"></i></span>').insertAfter(input);
    },
    renderNext: function(input) {
        return $('<span class="custom-prev"><i class="fas fa-plus"></i></span>').insertBefore(input);
    },
    disabledClassName: 'enable'
});



// 5. mobile-menu-sidebar
//---------------------------------------------------------------------------
$(".mobile-menubar").on("click", function(){
    $(".side-mobile-menu").addClass('open-menubar');
    $(".body-overlay").addClass("opened");
});
$(".close-icon").click(function(){
    $(".side-mobile-menu").removeClass('open-menubar');
    $(".body-overlay").removeClass("opened");
});

$(".body-overlay").on("click", function () {
    $(".side-mobile-menu").removeClass('open-menubar');
    $(".body-overlay").removeClass("opened");
});



// 6. header-search
//---------------------------------------------------------------------------
$(".header-search").on("click",function(){
    $(".header-search-details").addClass('open-search-info');
});
$(".close-icon").click(function(){
    $(".header-search-details").removeClass('open-search-info');

});



// 7. header-shopping-cart
//---------------------------------------------------------------------------
$(".header-shopping-cart").on('click',function(){
    $(".header-shopping-cart-details").toggle();
});


// 8. header-setting
//---------------------------------------------------------------------------
$(".header-setting").on('click',function(){
    $(".header-setting-content").toggle();
});



// 9. mobile header-setting
//---------------------------------------------------------------------------
$(".setting-more").on("click",function(){
    $(".mobile-h-setting-more-content").addClass('mobile-h-setting-info');
});
$(".setting-more-close-icon").click(function(){
    $(".mobile-h-setting-more-content").removeClass('mobile-h-setting-info');

});


// 10. home12 menu bar
// //---------------------------------------------------------------------------
$(".home12-menu").click(function() {
    $(".home12-menu").toggleClass("show-menu");
    $(".menu-bar-home12").toggleClass("show-menu");
});


// 11 slider - active
//---------------------------------------------------------------------------
function mainSlider() {
    var BasicSlider = $('.slider-active');

    BasicSlider.on('init', function (e, slick) {
        var $firstAnimatingElements = $('.single-slider:first-child').find('[data-animation]');
        doAnimations($firstAnimatingElements);
    });

    BasicSlider.on('beforeChange', function (e, slick, currentSlide, nextSlide) {
        var $animatingElements = $('.single-slider[data-slick-index="' + nextSlide + '"]').find('[data-animation]');
        doAnimations($animatingElements);
    });

    BasicSlider.slick({
        dots: true,
        fade: true,
        autoplay: true,
        autoplaySpeed: 5000,
        arrows: false,
        prevArrow:'<b><i class="l-a"><img src="images/slider/prev.png" alt=""></i></b>',
        nextArrow:'<b><i class="r-a"><img src="images/slider/next.png" alt=""></i></b>',
        responsive: [
            { breakpoint: 767, settings: {} }
        ]
    });

    function doAnimations(elements) {
        var animationEndEvents = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
        elements.each(function () {
            var $this = $(this);
            var $animationDelay = $this.data('delay');
            var $animationType = 'animated ' + $this.data('animation');
            $this.css({
                'animation-delay': $animationDelay,
                '-webkit-animation-delay': $animationDelay
            });
            $this.addClass($animationType).one(animationEndEvents, function () {
                $this.removeClass($animationType);
            });
        });
    }
}
mainSlider();


// 12. Price filter active 
//---------------------------------------------------------------------------
if ($("#slider-range").length) {
    $("#slider-range").slider({
        range: true,
        min: 20,
        max: 420,
        values: [20,420],
        slide: function (event, ui) {
            $("#amount").val("$" + ui.values[0] + " - $" + ui.values[1]);
        }
    });
    $("#amount").val("$" + $("#slider-range").slider("values", 0) +
        " - $" + $("#slider-range").slider("values", 1));


    $('#filter-btn').on('click', function () {
        $('.filter-widget').slideToggle(1000);
    });

}


// 13. accordion(sidebar) 
//---------------------------------------------------------------------------
var acc = document.getElementsByClassName("accordion");
var i;

for (i = 0; i < acc.length; i++) {
  acc[i].addEventListener("click", function() {
    this.classList.toggle("active");
    var panel = this.nextElementSibling;
    if (panel.style.maxHeight) {
      panel.style.maxHeight = null;
    } else {
      panel.style.maxHeight = panel.scrollHeight + "px";
    } 
  });
}





// 14. tooltip
//---------------------------------------------------------------------------
$('[data-toggle="tooltip"]').tooltip();


// 15. product-active
//---------------------------------------------------------------------------
$('.product-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 4,
    slidesToScroll: 2,
    responsive: [
        {
            breakpoint: 1199,
            settings: {
                slidesToShow: 3,
            }
        },
        {
            breakpoint: 687,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 475,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});

// 16. product-home10-active
//---------------------------------------------------------------------------
$('.product-home10-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 5,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 991,
            settings: {
                slidesToShow: 4,
            }
        },
        {
            breakpoint: 787,
            settings: {
                slidesToShow: 3,
            }
        },
        {
            breakpoint: 687,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 475,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});

// 17. blog-post-active
//---------------------------------------------------------------------------
$('.blog-post-active').slick({
    dots: false,
    arrows: true,
    infinite: true,
    slidesToShow: 3,
    prevArrow:'<b><span class="icon-chevron-left l-a"></span></b>',
    nextArrow:'<b><span class="icon-chevron-right r-a"></span></b>',
    responsive: [
        {
            breakpoint: 800,
            settings: {
                slidesToShow: 2,
                arrows: false,
            }
        },
        {
            breakpoint: 475,
            settings: {
                slidesToShow: 1,
                arrows: false,
            }
        }
    ]
});


// 18. product-new-arrivals-active
//---------------------------------------------------------------------------
$('.product-new-arrivals-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 3,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 991,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 575,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});


// 19. product-new-arrivals-active
//---------------------------------------------------------------------------
$('.product-new-arrivals-home11-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 2,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 575,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});


// 20. top-product-category-home12-active
//---------------------------------------------------------------------------
$('.top-product-category-home12-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 3,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 767,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 475,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});


// 21. product-new-arrivals-active
//---------------------------------------------------------------------------
$('.top-trending-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 4,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 991,
            settings: {
                slidesToShow: 3,
            }
        },
        {
            breakpoint: 575,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});


// 22. home5-product-active
//---------------------------------------------------------------------------
$('.home5-product-active').slick({
    dots: false,
    arrows: false,
    infinite: false,
    slidesToShow: 3,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 687,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 575,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});


// 23. product-tab-thamb-active
//---------------------------------------------------------------------------
$('.product-tab-thamb-active').slick({
    dots: false,
    infinite: false,
    slidesToShow: 4,
    slidesToScroll: 1,
    arrows: true,
    prevArrow:'<b><span class="icon-chevron-up l-a"></span></b>',
    nextArrow:'<b><span class="icon-chevron-down r-a"></span></b>',
    vertical:true,
    verticalSwiping:true,
    speed:500,
    swipe:false,
    responsive: [
        {
            breakpoint: 587,
            settings: {
                slidesToShow: 3,
            }
        }
    ]
});

// 24. home5-product-active
//---------------------------------------------------------------------------
$('.p-bottom-thamb-active').slick({
    dots: false,
    infinite: false,
    arrows: true,
    prevArrow:'<b><span class="icon-chevron-left l-a"></span></b>',
    nextArrow:'<b><span class="icon-chevron-right r-a"></span></b>',
    slidesToShow: 4,
    slidesToScroll: 1,
    responsive: [
        {
            breakpoint: 587,
            settings: {
                slidesToShow: 3,
            }
        }
    ]
});


// 25. wow animation  active
// ---------------------------------------------------------------------------
new WOW().init();


// 26. showlogin toggle function
// ---------------------------------------------------------------------------
$('#login').on('click', function () {
	$('#checkout-login').slideToggle(900);
});


// 27. showcoupon toggle function
// ---------------------------------------------------------------------------
$('#couponshow').on('click', function () {
	$('#checkout-coupon').slideToggle(900);
});


//28. Create an account toggle function
// ---------------------------------------------------------------------------
$('#cbox-account').on('click', function () {
	$('#cbox-account-info').slideToggle(900);
});


// 29. Create an account toggle function
// ---------------------------------------------------------------------------
$('#ship-box').on('click', function () {
	$('#ship-box-info').slideToggle(1000);
});


// 30. lsotope activation
// ---------------------------------------------------------------------------
var $grid = $('.grid').isotope({
    itemSelector: '.grid-item',
    percentPosition: true,
    masonry: {
      // use outer width of grid-sizer for columnWidth
      columnWidth: '.grid-item'
    }
});

// filter items on button click
$('.portfolio-menu').on( 'click', 'button', function() {
    var filterValue = $(this).attr('data-filter');
    $grid.isotope({ filter: filterValue });
});


// 31. Animate the scroll to top
// --------------------------------------------------------------------------
$('#scroll').on('click', function(event) {
    event.preventDefault();
    
    $('html, body').animate({
        scrollTop: 0,
    }, 1500);
});


// 32. preloader activation
//---------------------------------------------------------------------------
$(window).load(function(){
    $('#preloader').fadeOut('slow',function(){$(this).remove();});
});


// 33. Audio player activation
// ---------------------------------------------------------------------------
$( '.audio' ).audioPlayer();




})(jQuery);	  
