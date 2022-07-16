(function ($) {
    function loadedHandler() {
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

        let promoCodeForm = document.getElementById('promo_code_check');

        formToAjax(promoCodeForm, (response) => {
            let messageType = response.Success ? "green" : "yellow";
            showMessage(response.Message, messageType);
            setTimeout(() => { location.href = response.Url; }, 2000);
        }, true);
    }

    return loadedHandler();
})(jQuery);