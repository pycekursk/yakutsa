(function ($) {
    function loadedHandler() {
        let $modelInputs = $('#sub_categories > div[role=group] .btn-anim input[type=radio]');
        let $sizeInputs = $('.sizes div[value] .btn-anim > input[type=radio]');

        $modelInputs.on('click', (evt) => {
            $modelInputs.closest('#sub_categories > div[role=group] .btn-anim').removeClass('active');
            $(`#sub_categories .sizes-inner div.active`).removeClass('active');
            $(evt.target).closest('.btn-anim').addClass('active');
            $(`#sub_categories .sizes-inner div[value=${evt.target.value}]`).addClass('active');

            setTimeout(() => {
                $('#sub_categories .sizes').css('height', $('#sub_categories .sizes .sizes-inner').height());
            }, 50);

        });

        $sizeInputs.on('click', function () {
            $sizeInputs.closest('.btn-anim').removeClass('active');
            $(this.parentElement).addClass("active");
        });

        let $carousel = $(`#product_carousel`);
        $carousel.find('.carousel-preview .preview-item').on('click', (evt) => {
            $carousel.find('.carousel-preview .preview-item').removeClass('active');
            $carousel.find('.carousel-item.active').removeClass('active');
            let $this = $(evt.currentTarget).addClass('active');
            $carousel.find(`.carousel-item[index=${$this.attr('index')}]`).addClass('active');
        });

        $carousel.on('slide.bs.carousel', (evt) => {
            console.log(evt.type);
            $(evt.currentTarget).find('.carousel-preview .preview-item.active').removeClass('active');
            $(evt.currentTarget).find(`.carousel-preview .preview-item[index=${$(evt.relatedTarget).attr('index')}]`).addClass('active');
        });

        document.addEventListener('mousemove', (evt) => {
            let target = document.elementFromPoint(evt.clientX, evt.clientY);
            let block = document.querySelector('div.noticed[notify]');
            if (block && evt.path.includes(block)) block.classList.remove('noticed');
        });

        $('#product_info .btn.bg-gray').on('click', (evt) => {
            let $parent = $('#product_info');
            let productId = $parent.attr('data-id');
            let model = $('#sub_categories > .btn-group .btn-anim.active');
            let offerId = $('.sizes .sizes-inner div.active[value] .btn-anim.active input').attr('id');
            if (offerId != undefined)
                sendAjaxForm({ productId: productId, offerId: offerId }, '/ToCart', (response) => {
                    if (response.Success) {
                        $('.fa-shopping-cart').attr("count", response.Html);
                        showMessage(response.Message, 'green');

                        if (!isDevelopment) {
                            try {
                                ym(87733644, 'reachGoal', 'addtocart');
                            } catch (e) {
                                console.log(e);
                            }
                            try {
                                VK.Goal('add_to_cart');
                            } catch (e) {
                                console.log(e);
                            }
                        }
                    }
                }, true);

            else if ($('#sub_categories').length != 0 && model.length == 0) {
                showMessage('Необходимо выбрать модель', 'yellow');
                let block = document.querySelector('#sub_categories > .btn-group[notify]');
                if (block) {
                    block.classList.add('noticed');
                    block.classList.add('noticed');
                    setTimeout(() => {
                        block.classList.remove('noticed');
                    }, 4000);
                }
            }
            else if (offerId == undefined) {
                showMessage('Необходимо выбрать размер', 'yellow');
                let block = document.querySelector('.sizes > .sizes-inner .btn-group > div.active[notify]');
                if (block) {
                    block.classList.add('noticed');
                    setTimeout(() => {
                        block.classList.remove('noticed');
                    }, 4000);
                }
            }
        });

        if (detector.isPhoneSized())
            setTimeout(() => {
                $('#product_carousel .carousel-item img').attr('height', $(window).height() - ($('.top-bar').height() + $('.goback-wrapper').height() + $('#product_carousel .carousel-preview').height() + 32));
            }, 50);
    }

    return loadedHandler();
})(jQuery);