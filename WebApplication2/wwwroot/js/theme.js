// js Document

(function($) {
    "use strict";
    
    
    $(document).on ('ready', function (){

        $.fn.visible = function(partial) {
            var $t            = $(this),
                $w            = $(window),
                viewTop       = $w.scrollTop(),
                viewBottom    = viewTop + $w.height(),
                _top          = $t.offset().top,
                _bottom       = _top + $t.height(),
                compareTop    = partial === true ? _bottom : _top,
                compareBottom = partial === true ? _top : _bottom;
          
          return ((compareBottom <= viewBottom) && (compareTop >= viewTop));
        };

// -------------------------- Modal Toggle Classs
        // toggleClass
        $(document).on('click', '[data-toggle-class]', function (e) {
          var $self = $(this);
          var attr = $self.attr('data-toggle-class');
          var target = $self.attr('data-toggle-class-target') || $self.attr('data-target');
          var closest = $self.attr('data-target-closest');
          var classes = ( attr && attr.split(',')) || '',
            targets = (target && target.split(',')) || Array($self),
            key = 0;
          $.each(classes, function( index, value ) {
            var target = closest ? $self.closest(targets[(targets.length == 1 ? 0 : key)]) : $( targets[(targets.length == 1 ? 0 : key)] ),
                      current = target.attr('data-class'),
                      _class = classes[index];
                  (current != _class) && target.removeClass( target.attr('data-class') );
            target.toggleClass(classes[index]);
            target.attr('data-class', _class);
            key++;
          });
          $self.toggleClass('active');
          $self.attr('href') == "#" ? e.preventDefault() : '';
        });

// -------------------------- scroll animate
        var links = $('a.scroll-target');
        links.on('click', function() {
            if (location.pathname.replace(/^\//,'') == this.pathname.replace(/^\//,'') || location.hostname == this.hostname) {
            var target = $(this.hash);
                target = target.length ? target : $('[name=' + this.hash.slice(1) +']');
                if (target.length) {
                $('html,body').animate({
                    scrollTop: target.offset().top - 75,
                    }, 1000);
                    return false;
                }
            }
        });


// ------------------------ Navigation Scroll
        $(window).on('scroll', function (){   
          var sticky = $('.sticky-menu'),
          scroll = $(window).scrollTop();
          if (scroll >= 100) sticky.addClass('fixed');
          else sticky.removeClass('fixed');

        });

// -------------------- Remove Placeholder When Focus Or Click
        $("input,textarea").each( function(){
            $(this).data('holder',$(this).attr('placeholder'));
            $(this).on('focusin', function() {
                $(this).attr('placeholder','');
            });
            $(this).on('focusout', function() {
                $(this).attr('placeholder',$(this).data('holder'));
            });     
        });
        
// -------------------- From Bottom to Top Button
            //Check to see if the window is top if not then display button
        $(window).on('scroll', function (){
          if ($(this).scrollTop() > 200) {
            $('.scroll-top').fadeIn();
          } else {
            $('.scroll-top').fadeOut();
          }
        });

//---------------------- Click event to scroll to top
        $('.scroll-top').on('click', function() {
          $('html, body').animate({scrollTop : 0},1500);
          return false;
        });
        
// ------------------------- Tooltips
        $('[data-toggle="tooltip"]').tooltip();



// --------------------- SVG convert Function
        $('img.svg').each(function(){
        var $img = $(this);
        var imgID = $img.attr('id');
        var imgClass = $img.attr('class');
        var imgURL = $img.attr('src');
    
        $.get(imgURL, function(data) {
            // Get the SVG tag, ignore the rest
            var $svg = $(data).find('svg');
    
            // Add replaced image's ID to the new SVG
            if(typeof imgID !== 'undefined') {
                $svg = $svg.attr('id', imgID);
            }
            // Add replaced image's classes to the new SVG
            if(typeof imgClass !== 'undefined') {
                $svg = $svg.attr('class', imgClass+' replaced-svg');
            }
    
            // Remove any invalid XML tags as per http://validator.w3.org
            $svg = $svg.removeAttr('xmlns:a');
            
            // Check if the viewport is set, else we gonna set it if we can.
            if(!$svg.attr('viewBox') && $svg.attr('height') && $svg.attr('width')) {
                $svg.attr('viewBox', '0 0 ' + $svg.attr('height') + ' ' + $svg.attr('width'))
            }
    
            // Replace image with new SVG
            $img.replaceWith($svg);
    
            }, 'xml');
    
        });

// -------------------------- Sidebar Menu/E-commerce
        var subMenu = $ (".main-menu-list ul li.dropdown-holder>a"),
          expender = $ (".main-menu-list ul li.dropdown-holder .expander");

        if ($('.sidebar-menu-open').length) {
          $('.sidebar-menu-open').on('click', function () {
            $('#sidebar-menu').addClass("show-menu");
          });
        }

        if ($('.close-button').length) {
          $('.close-button').on('click', function () {
            $('#sidebar-menu').removeClass("show-menu");
          });
        }
        subMenu.on("click", function (e) {
            e.preventDefault();
        });

        subMenu.append(function () {
          return '<span class="expander"><i class="fa fa-chevron-down" aria-hidden="true"></i></span>';
        });

        subMenu.on('click', function () {
          if ( $(this).parent('li').children('ul').hasClass('show') ) {
              $(this).parent('li').children('ul').removeClass('show');
          } else {
              $('.sub-menu.show').removeClass('show');
              $(this).parent('li').children('ul').addClass('show');    
          }
       });

// ---------------------------- Select Dropdown
        if($('select').length) {
          $('.theme-select-menu').selectize();
        }

      
// ----------------------------- Counter Function
        var timer = $('.timer');
        if(timer.length) {
            timer.appear(function () {
              timer.countTo();
          });
        }

// ------------------------ Hover Tilt effect
        var tiltBlock = $('.js-tilt');
          if(tiltBlock.length) {
            $('.js-tilt').tilt({
                glare: true,
                maxGlare: 0.4
            });
        }

// ------------------------ Modal box
        if ($(".iziModal").length) { 
          $(".iziModal").iziModal({
            width: 2550,
            overlayColor: 'rgba(255, 255, 255, 0.95)',
            fullscreen: true,
          });
        }
        

// ----------------------- Progress Bar
        $('.progress-bar').each(function(){
            var width = $(this).data('percent');
            $(this).css({'transition': 'width 3s'});
            $(this).appear(function() {
                console.log('hello');
                $(this).css('width', width + '%');
                $(this).find('.count').countTo({
                    from: 0,
                    to: width,
                    speed: 3000,
                    refreshInterval: 50,
                });
            });
        });


// --------------------------------- Contact Form
          // init the validator
          // validator files are included in the download package
          // otherwise download from http://1000hz.github.io/bootstrap-validator

        if($("#contact-form").length) {
            $('#contact-form').validator();
            // when the form is submitted
            $('#contact-form').on('submit', function (e) {

                // if the validator does not prevent form submit
                if (!e.isDefaultPrevented()) {
                    var url = "inc/contact.php";

                    // POST values in the background the the script URL
                    $.ajax({
                        type: "POST",
                        url: url,
                        data: $(this).serialize(),
                        success: function (data)
                        {
                            // data = JSON object that contact.php returns

                            // we recieve the type of the message: success x danger and apply it to the
                            var messageAlert = 'alert-' + data.type;
                            var messageText = data.message;

                            // let's compose Bootstrap alert box HTML
                            var alertBox = '<div class="alert ' + messageAlert + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + messageText + '</div>';

                            // If we have messageAlert and messageText
                            if (messageAlert && messageText) {
                                // inject the alert to .messages div in our form
                                $('#contact-form').find('.messages').html(alertBox);
                                // empty the form
                                $('#contact-form')[0].reset();
                            }
                        }
                    });
                    return false;
                }
            });
          }


// -------------------- Site Preloader
        $('#ctn-preloader').fadeOut(); // will first fade out the loading animation
        $('#preloader').delay(350).fadeOut('slow'); // will fade out the white DIV that covers the website.
        $('body').delay(350).css({'overflow':'visible'});


// ------------------------------- AOS Animation
        if ($("[data-aos]").length) { 
            AOS.init({
            duration: 1000,
            mirror: true
          });
        }

// ------------------------------- WOW Animation 
        if ($(".wow").length) { 
            var wow = new WOW({
            boxClass:     'wow',      // animated element css class (default is wow)
            animateClass: 'animated', // animation css class (default is animated)
            offset:       20,          // distance to the element when triggering the animation (default is 0)
            mobile:       true,       // trigger animations on mobile devices (default is true)
            live:         true,       // act on asynchronously loaded content (default is true)
          });
          wow.init();
        }



// ----------------------------- isotop gallery
        if ($(".masnory-blog-wrapper").length) {
            var $grid = $('.masnory-blog-wrapper').isotope({
              // options
              itemSelector: '.isotop-item',
              percentPosition: true,
              masonry: {
                // use element for option
                columnWidth: '.grid-sizer'
              }

            });

            // filter items on button click
            $('.isotop-menu-wrapper').on( 'click', 'li', function() {
              var filterValue = $(this).attr('data-filter');
              $grid.isotope({ filter: filterValue });
            });

             // change is-checked class on buttons
              $('.isotop-menu-wrapper').each( function( i, buttonGroup ) {
                var $buttonGroup = $( buttonGroup );
                $buttonGroup.on( 'click', 'li', function() {
                  $buttonGroup.find('.is-checked').removeClass('is-checked');
                  $( this ).addClass('is-checked');
                });
              });
        }

        
// ------------------------------------- Fancybox
        var fancy = $ (".fancybox");
        if(fancy.length) {
          fancy.fancybox({
            arrows: true,
            buttons: [
              "zoom",
              //"share",
              "slideShow",
              //"fullScreen",
              //"download",
              "thumbs",
              "close"
            ],
            animationEffect: "zoom-in-out",
            transitionEffect: "zoom-in-out",
          });
        }



    });  //End On Load Function


    $(window).on ('scroll', function (){ // makes sure the whole site is loaded

// --------------------- Viewport Animation 
        $(".hide-pr").each(function(i, el) {
          var el = $(el);
          if (el.visible(true)) {
            el.addClass("show-pr"); 
          } else {
            el.removeClass("show-pr");
          }
        });

    });  //End On Scroll Function


    
})(jQuery);