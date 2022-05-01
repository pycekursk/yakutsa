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
    }

    return loadedHandler();
})(jQuery);