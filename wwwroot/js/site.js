

function componentToHex(c) {
    var hex = c.toString(16);
    return hex.length == 1 ? "0" + hex : hex;
}

function rgbToHex(r, g, b) {
    return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

function observe(target) {
    let observer = new MutationObserver(mutationRecords => {
        log(mutationRecords);
        for (let index = 0; index < mutationRecords.length; index++) {
            console.log(mutationRecords[index]);
        }
    });
    observer.observe(target, {
        childList: true, // наблюдать за непосредственными детьми
        subtree: true, // и более глубокими потомками
        characterDataOldValue: false // передавать старое значение в колбэк
    });
}

//var detector;

function Initialize($) {
    var json_view = $("#json_view");
    var row_view = $("#row_view");
    $('.form-select').removeAttr('multiple');
    detector = new MobileDetect();

    function checkDevice() {
        if (detector.isPhoneSized()) {
            document.body.classList.add('mobile')
        }
        else {
            document.body.classList.remove('mobile');
        }
    }

    function videoCarouselInit() {
        var videos = document.querySelectorAll('#video_carousel video');


        let carousel = $('#video_carousel');
        if (carousel == null) return;
        let inner = carousel.find(".carousel-inner");
        let activeElement = inner.find('.carousel-item.playing');


        sizeAdaptation(videos);

        if (!detector.isPhoneSized())
            window.onresize = () => { sizeAdaptation(videos); };


        $('#video_carousel .carousel-control-next,#video_carousel .carousel-control-prev').on('click', (evt) => {
            let nextSlide = null;
            if (evt.currentTarget.getAttribute('data-bs-slide') == 'prev') {
                nextSlide = activeElement[0].previousElementSibling != null ? $(activeElement[0].previousElementSibling) : activeElement[0].nextElementSibling != null ? $(activeElement[0].nextElementSibling) : null;
            }
            else if (evt.currentTarget.getAttribute('data-bs-slide') == 'next') {
                nextSlide = activeElement[0].nextElementSibling != null ? $(activeElement[0].nextElementSibling) : activeElement[0].previousElementSibling != null ? $(activeElement[0].previousElementSibling) : null;
            }

            if (nextSlide == null || nextSlide.length == 0) return;

            activeElement.fadeOut();
            nextSlide.fadeIn();

            activeElement.find('video')[0].pause();
            activeElement.removeClass('playing');
            nextSlide.addClass('playing');
            nextSlide.find('video')[0].play();
            activeElement = nextSlide;
        });

        $('.carousel-item.playing video').on('load', (evt) => { evt.target.play(); });

        document.querySelectorAll('video').forEach(e => {
            e.addEventListener('ended', (evt) => {
                document.querySelector('.carousel-control-next').click();
            }, false);
        });

        checkDevice();


        $('.fancybox').fancybox({
            thumbs: {
                autoStart: false, // Display thumbnails on opening
                hideOnClose: true, // Hide thumbnail grid when closing animation starts
                parentEl: ".fancybox-container", // Container is injected into this element
                axis: "y" // Vertical (y) or horizontal (x) scrolling
            },
            beforeShow: function (evt) {
                $('main, header, footer').addClass('blur');
                console.log('open', evt);
            },
            beforeClose: function (evt) {
                $('main, header, footer').removeClass('blur');
                console.log('close', evt);
            }
        });

        $('.loader').loader();

        $(window).on('resize', () => checkDevice());
    }

    function loadedHandler() {

        window.onload = function () {
            let sizesTable = `<h5>Таблица размеров</h5><div class='container'><div class='row row-cols-8'><div class='col'></div><div class='col'>S</div><div class='col'>M</div><div class='col'>L</div><div class='col'>XL</div><div class='col'>2XL</div><div class='col'>3XL</div><div class='col'>+/- см.</div></div></div>`;
            let newTable = "<div class='tooltip-table'><h5>Таблица размеров</h5><div class='container'><div class='row row-cols-12'><div class='col-5'></div><div class='col'>XS</div><div class='col'>S</div><div class='col'>M</div><div class='col'>L</div><div class='col'>XL</div><div class='col'>+/- см.</div></div><div class='row row-cols-12'><div class='col-5'>Рост</div><div class='col'>??</div><div class='col'>160</div><div class='col'>165</div><div class='col'>170</div><div class='col'>175</div><div class='col'>3</div></div><div class='row row-cols-12'><div class='col-5'>Длина по спинке</div><div class='col'>??</div><div class='col'>68</div><div class='col'>69,5</div><div class='col'>71</div><div class='col'>72,5</div><div class='col'>1,5</div></div><div class='row row-cols-12'><div class='col-5'>Ширина по груди</div><div class='col'>??</div><div class='col'>57</div><div class='col'>59</div><div class='col'>61</div><div class='col'>63</div><div class='col'>1,0</div></div><div class='row row-cols-12'><div class='col-5'>Длина рукава</div><div class='col'>??</div><div class='col'>56</div><div class='col'>57</div><div class='col'>58</div><div class='col'>59</div><div class='col'>1,0</div></div><div class='row row-cols-12'><div class='col-5'>Длина плеча</div><div class='col'>??</div><div class='col'>23</div><div class='col'>23</div><div class='col'>24</div><div class='col'>24,5</div><div class='col'>0,5</div></div></div></div>";
            $('#sizes i.fa-question').tooltip({ container: '#sizes', title: newTable, placement: "bottom", html: true });
            let tooltip = document.querySelector('#sizes i.fa-question');
            if (tooltip) tooltip.addEventListener("touchstart", function (e) { $(e.currentTarget).tooltip('show'); });
        }

        $("#get_buttons button").on("click", (evt) => {
            $.post("GetResponse", { "action": evt.currentTarget.getAttribute("data") != null ? evt.currentTarget.getAttribute("data") : $("#action_field").val() == "" ? $("#get_buttons select")[0].selectedOptions[0].innerText : $("#action_field").val() })
                .then(obj => {
                    json_view.html(!(obj instanceof String) ? `<h6>Json:<h6>${JSON.stringify(obj).trim()}` : `<h6>Json:<h6>${obj.trim()}`);
                    row_view.html("<h6>Rows:<h6>");
                    for (var i in obj) {
                        if (obj[i] instanceof Array) {
                            let text = "<h6>Rows:<h6>";
                            obj[i].forEach((e, i) => {
                                text += `------- ${i} -------<br>`;
                                for (var p in e) {
                                    text += `${p} - ${e[p]}<br>`;
                                }
                            })
                            row_view.html(text);
                        }
                        else {
                            //console.log(`${i} - ${obj[i]}`);
                        }
                    }
                });
        });

        $(".password-wrapper i.fa-eye-slash").on("click", (evt) => {
            let target = evt.currentTarget;
            let input = target.previousElementSibling;
            input.setAttribute("type", input.getAttribute('type') == "text" ? "password" : "text");
            target.classList.toggle("fa-eye");
            target.classList.toggle("fa-eye-slash");
        });

        $(".top-bar i.fa-door-closed")
            .on("mouseenter", (evt) => {
                evt.currentTarget.classList.toggle("fa-door-closed");
                evt.currentTarget.classList.toggle("fa-door-open");
            })
            .on("mouseleave", (evt) => {
                evt.currentTarget.classList.toggle("fa-door-closed");
                evt.currentTarget.classList.toggle("fa-door-open");
            });
        $('video[autoplay]').on('load', (evt) => { console.log(evt) });

        $('.alert').each((i, s) => {
            setTimeout((e) => {
                e.fadeOut({ complete: () => { e.remove(); } });
            }, 7000, $(s).fadeIn());
        });

        window.onbeforeunload = function () {
            document.body.classList.add('busy');
            document.body.classList.add('loading');
        }

        $(window).on("scroll", (evt) => scrollHandler(evt));


        videoCarouselInit();
        $('.loader').loader();
        $('.card[data-id]').each((i, e) => {
            let $card = $(e);
            $card.on('click', (evt) => {
                if (evt.target.className.includes('fas')) return;
                location.href = `/Product?id=${$card.attr('data-id')}`;
            });
        });
        $('.toTop-toggler .fa.fa-arrow-up').on('click', () => scrollTo(0, 0));






        let $msg = $(`.alert`);
        $msg.find('button').on('click', () => {
            $msg.fadeOut(350, () => $msg.remove());
        });

        //$('.side-toggler .btn-anim').on('click', function (evt) {
        //    $(document.body).addClass('openmenu');
        //});

        $('#catalog_toggler').on('click', (evt) => {
            evt.target.toggleClass('active');
            evt.target.previousElementSibling.toggleClass('active');
            evt.target.nextElementSibling.toggleClass('open');
        });

        document.addEventListener('click', function (evt) {
            if (document.getElementById('menu_links') == null) return;
            try {
                let result = evt.path.includes() || evt.path.includes(document.querySelector('.side-toggler'));
                if ($(document.body).hasClass('openmenu') && !result) $(document.body).removeClass('openmenu');
            } catch (e) {
                console.log(e);
            }
        });



        $('.labelholder').labelholder();

        $('#menu_modal li a').on('click', (evt) => {
            $(evt.currentTarget).closest('.modal').modal('hide');
        });

        $('#order_configure_form').on('submit', (evt) => {
            evt.preventDefault();
            sendAjaxForm('', 'CheckOffers', () => {
                sendAjaxForm(evt.currentTarget, 'OrderOptions', (response) => {
                    if (response.Success) {
                        if (!isDevelopment) {
                            try {
                                ym(87733644, 'reachGoal', 'iniciatecheakout');
                                VK.Goal('initiate_checkout');
                            } catch (e) {
                                console.log(e);
                            }
                        }
                        showPaymentModal(response.Url);
                        sendAjaxForm({ 'id': response.Html }, 'PaymentCheck', (resp) => {
                            if (resp.Success) {
                                $('#payment_modal').hide();
                                showMessage(resp.Message, "green");
                                setTimeout(() => { location.href = "/"; }, 2000);
                                if (!isDevelopment) {
                                    try {
                                        ym(87733644, 'reachGoal', 'sell');
                                        VK.Goal('conversion');
                                    } catch (e) {
                                        console.log(e);
                                    }
                                }
                            }
                            else if (!resp.Success) showMessage(resp.Message, "red");
                        }, true);
                    }
                    else if (!response.Success) showMessage(response.Message, 'yellow');
                }, true);
            }, true);
        });

        $('*[suggestion]').suggestion();

        $('#menu_links h3 > i').on('click', (evt) => { $(document.body).removeClass('openmenu') });

        $('#order_configure_form .form-select').on('change', (evt) => { evt.target.previousElementSibling.value = evt.currentTarget.selectedOptions[0].getAttribute('value'); });

        window.oncontextmenu = function (evt) {
            let arr = Array.from(document.querySelectorAll('.top-bar img[alt=logo]'));
            if (arr.includes(evt.srcElement) && (evt.altKey == true || detector.isPhoneSized())) {
                let signInIcon = document.querySelector('.inline-icons .fa-door-closed');
                if (signInIcon) {
                    signInIcon.style.cssText = "display:block;";
                }
                return false;
            }
        }

        $('#footer_accordion button[type=button]').on('click', function () {
            setTimeout(() => {
                scrollTo(0, window.document.body.scrollHeight);
            }, 200);
        });

        var owl = $('.owl-wrapper[loop=true] .owl-carousel');
        owl.owlCarousel({
            loop: true,
            margin: 10,
            stagePadding: 50,
            responsive: {
                300: {
                    items: 1
                },
                500: {
                    items: 2
                },
                800: {
                    items: 3
                },
                1200: {
                    items: 4
                }
            }
        });

        var owl2 = $('.owl-wrapper[loop=false] .owl-carousel');
        owl2.owlCarousel(options = {
            margin: 10,
            loop: false,
            center: false,
            stagePadding: 50,
            responsive: {
                300: {
                    items: 1
                },
                500: {
                    items: 2
                },
                800: {
                    items: 3
                },
                1200: {
                    items: 4
                }
            }
        });

        var previewOwl = $('#product_carousel .owl-carousel');
        previewOwl.owlCarousel(options = {
            dots: false,
            loop: false,
            center: false,
            stagePadding: 30,
            responsive: {
                300: {
                    items: 3
                },
                500: {
                    items: 4
                },
                800: {
                    items: 5
                }
            }
        });

        if (isDevelopment) {

            $('.consultant-widget > .consultant-icon').on('click', function () {
                if (!$(this).hasClass("consultant-icon_opened")) {
                    try {
                        ym(87733644, 'reachGoal', 'openChat')
                    } catch (e) {
                        console.log(e);
                    }
                };
            });




        }



        //$('.toTop-toggler').vibrate(10);

        setTimeout(() => { $msg.fadeOut(350, () => $msg.remove()); }, 5000);
    }

    return loadedHandler();
}