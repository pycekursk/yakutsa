var inputObject;

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

function showMessage(text, type) {
    let $msg = $(`<div class="alert alert-${type == undefined ? 'main' : type}" role="alert">${text}<button type="button" class="close" data-dismiss="alert" aria-label="Close"><i class="fas fa-times"></i></button></div>`);
    $('.messages-wrapper').append($msg);

    $msg.find('button').on('click', () => {
        $msg.fadeOut(350, () => $msg.remove());
    });

    setTimeout(() => { $msg.fadeOut(350, () => $msg.remove()); }, 5000);
}

Array.prototype.remove = function (elem) {
    let index = this.indexOf(elem);
    this.splice(index, 1);
}

function sendAjaxForm(data, url, callback, busyTrigger) {
    function getProps(obj) {
        let formData = new FormData();
        for (var i in obj) {
            formData.append(i, obj[i]);
        }
        return formData;
    }
    let isAsync = callback ? true : false;

    data = data instanceof HTMLFormElement ? new FormData(data) : data instanceof FormData ? data : getProps(data);

    if (busyTrigger)
        setTimeout(() => {
            document.body.classList.add('busy');
        }, 0);

    return $.ajax({
        url: url,
        type: "POST",
        data: data,
        async: isAsync,
        processData: false,
        contentType: false,
        success: function (response) {
            if (busyTrigger) setTimeout(() => { document.body.classList.remove('busy'); }, 0);
            try {
                let obj = JSON.parse(response);
                if (callback) callback(obj);
                if (obj.Message != null)
                    showMessage(obj.Message);
            } catch (exc) { console.log(exc); }
        },
        error: function (response) {
            if (busyTrigger) setTimeout(() => { document.body.classList.remove('busy'); }, 0);
            console.log(response);
        },
        progress: function (e) {
            console.log(e);
        }
    });
}

$.fn.loader = function () {
    return this.each((index, element) => {
        if ($(element).find('i.fa-spinner').length == 0) {
            this.append('<i class="fas fa-spinner"></i>')
        }
        let e = $(element).find('.btn');
        if (e.length > 0) {
            e.on('click', (evt) =>
                evt.currentTarget.parentElement.classList.contains('active') ? evt.currentTarget.parentElement.classList.remove('active') : evt.currentTarget.parentElement.classList.add('active'));
        }
    });
};

var detector;

function Initialize($) {
    var json_view = $("#json_view");
    var row_view = $("#row_view");

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
        let carousel = $('#carousel');
        if (carousel == null) return;
        let inner = carousel.find(".carousel-inner");
        let activeElement = inner.find('.carousel-item.playing');

        $('#carousel .carousel-control-next,#carousel .carousel-control-prev').on('click', (evt) => {
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

        document.querySelectorAll('video').forEach(e => {
            e.addEventListener('ended', (evt) => {
                evt.target.play();
                console.log(evt);
                document.querySelector('.carousel-control-next').click();
            }, false);
        });

        checkDevice();

        $('.fancybox').fancybox();
        $('.loader').loader();

        $(window).on('resize', () => checkDevice());
    }

    function scrollHandler() {

        if ($(window).scrollTop() > 90 && !detector.isPhoneSized()) {
            //$('.side-toggler').hide();
        }
        else if ($(window).scrollTop() < 90 && detector.isPhoneSized()) {
            //$('.side-toggler').show();
        }
        if ($(window).scrollTop() > 200) {
            //$('.b24-widget-button-position-bottom-right').fadeOut();
            $('.toTop-toggler').addClass('active');

        } else {
            //$('.b24-widget-button-position-bottom-right').fadeIn();
            $('.toTop-toggler').removeClass('active');

        }
    }

    function loadedHandler() {

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

        videoCarouselInit();

        $('.alert').each((i, s) => {
            setTimeout((e) => {
                e.fadeOut({ complete: () => { e.remove(); } });
            }, 7000, $(s).fadeIn());
        });

        window.onbeforeunload = function () {
            document.body.classList.add('busy');
        }

        $('.loader').loader();
        $(window).on("scroll", scrollHandler);
        $('.card[data-id]').each((i, e) => {
            let $card = $(e);

            $card.on('click', (evt) => {
                if (evt.target.className.includes('fas')) return;

                location.href = `/Product?id=${$card.attr('data-id')}`;
            });

            $card.find('.fas.fa-cart-plus').on('click', function (evt) {
                let id = $card.attr('data-id');
                console.log(evt);
                //sendAjaxForm($this.attr('data-id'), 'ToCart/', (resp) => {
                //    console.log(resp);
                //}, true);


            });
        });
        $('.toTop-toggler .fa.fa-arrow-up').on('click', () => scrollTo(0, 0));
        $('#product_info input[type=radio]').on('click', function () { $('#product_info .btn-anim').removeClass('active'); $(this.parentElement).addClass("active") });

        $('#product_info .btn.bg-gray').on('click', (evt) => {
            let $parent = $('#product_info');
            let productId = $parent.attr('data-id');
            let offerId = $('.btn-group .btn-anim.active input').attr('id');

            if (productId != undefined && offerId != undefined)
                sendAjaxForm({ id: productId, offerId }, 'ToCart', (response) => {
                    $('.fa-shopping-cart').attr("count", response.Html);
                }, true);

            else showMessage('Необходимо выбрать размер');
        });

        let $msg = $(`.alert`);
        $msg.find('button').on('click', () => {
            $msg.fadeOut(350, () => $msg.remove());
        });

        $('.side-toggler').on('click', function (evt) {
            $(document.body).addClass('openmenu');
        });

        document.addEventListener('click', function (evt) {
            let result = evt.path.includes(document.getElementById('menu_links')) || evt.path.includes(document.querySelector('.side-toggler'));
            if ($(document.body).hasClass('openmenu') && !result) $(document.body).removeClass('openmenu');
        });

        $('.ref-increase').on('click', (evt) => {
            let field = $(evt.target.parentElement).find('input[name=count]');
            let value = parseInt(field.val());
            field.val(++value);
        });
        $('.ref-decrease').on('click', (evt) => {
            let field = $(evt.target.parentElement).find('input[name=count]');
            let value = parseInt(field.val());
            if (value > 0) field.val(--value);
        });

        //$('#product_info button.btn[disabled]').on('click', function () { console.log(this); });

        //$('#product_info .btn-group .btn-main:not(.disabled)').on('click', function () {
        //    let btn = $('#product_info button.btn[disabled]');
        //    if (btn.length > 0) btn.removeAttr('disabled');
        //});

        $('#menu_links h3 > i').on('click', (evt) => { $(document.body).removeClass('openmenu') });

        $('.form-select').removeAttr('multiple');


        setTimeout(() => { $msg.fadeOut(350, () => $msg.remove()); }, 5000);
    }
    return loadedHandler();
}