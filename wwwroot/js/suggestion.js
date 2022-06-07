$.fn.suggestion = function () {
    document.addEventListener('click', (evt) => {
        if (!(evt.currentTarget.activeElement instanceof HTMLInputElement) && !(evt.currentTarget.activeElement instanceof HTMLOptionElement))
            $('.dropdown-menu.show').removeClass('show');
    });

    $('*[suggestion]').on('click', (evt) => {
        let $dropdown = $(evt.currentTarget.nextElementSibling);
        let $dropdownMenu = $dropdown.find('.dropdown-menu');
        if ($dropdown.find('.drop-down-row').length > 0 && !$dropdownMenu.hasClass('show')) $dropdownMenu.addClass('show');
    });

    function suggestion(evt) {
        var $dropdown = $(evt.currentTarget.nextElementSibling);
        if ($dropdown.length == 0) {
            $dropdown = $('<div class="dropdown"><div class="dropdown-menu"></div></div>');
            $dropdown.insertAfter($(evt.currentTarget));
        }
        var dropdownMenu = $dropdown.find('.dropdown-menu')[0];
        var $input = $(evt.currentTarget);

        if (evt.type == "keydown" && evt.key == 'Enter' && dropdownMenu.querySelector('.selected')) {
            evt.preventDefault();
            evt.currentTarget.value = dropdownMenu.querySelector('.selected').innerText;
            dropdownMenu.classList.remove('show');
            $(evt.currentTarget).trigger("change");
        }
        else if (evt.key == 'Tab') {
            dropdownMenu.classList.remove('show');
        }
        else {
            let query = evt.currentTarget.value;
            if (!query || query.length < 3) {
                if (evt.key == "Backspace") {
                    dropdownMenu.innerHTML = "";
                }
                return;
            }
            var url = evt.currentTarget.getAttribute('suggestion') == "address" ? "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/address" : "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/fio";
            var token = "aa9b411a0851eb8344a4fe5fc9cfc272a994c6ab";
            var options = {
                method: "POST",
                mode: "cors",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json",
                    "Authorization": "Token " + token
                },
                body: JSON.stringify({ query: query })
            }

            //if (isDevelopment) console.log(JSON.stringify({ query: query }));

            if (evt.key == 'ArrowDown') {
                let currItem = dropdownMenu.querySelector('.selected');
                if (!currItem) {
                    dropdownMenu.children[0].classList.add('selected');
                }
                else {
                    let nextItem = currItem.nextElementSibling;
                    if (!nextItem) return;
                    currItem.classList.remove('selected');
                    nextItem.classList.add('selected');
                }


                let $input = $($dropdown).parent().find('input');
                let $option = $dropdown.find('.selected');

                Array.from($input[0].attributes).forEach(prop => {
                    if (prop.name.includes('sd')) {
                        $input[0].removeAttribute(prop.name);
                    }
                });

                let attributes = Array.from($option[0].attributes);
                attributes.forEach(attr => {
                    if (attr.name.includes('sd')) {
                        $input.attr(attr.name, attr.value);
                    }
                });
                $input.val($option.text());
            }
            else if (evt.key == 'ArrowUp') {
                let currItem = dropdownMenu.querySelector('.selected');
                if (currItem) {
                    currItem.classList.remove('selected');
                    currItem.previousElementSibling.classList.add('selected');
                }

                let $input = $($dropdown).parent().find('input');
                let $option = $dropdown.find('.selected');

                //TODO: перенос аттрибутов в инпут

                Array.from($input[0].attributes).forEach(prop => {
                    if (prop.name.includes('sd')) {
                        $input[0].removeAttribute(prop.name);
                    }
                });

                let attributes = Array.from($option[0].attributes);
                attributes.forEach(attr => {
                    if (attr.name.includes('sd')) {
                        $input.attr(attr.name, attr.value);
                    }
                });

                $input.val($option.text());
            }
            else {
                fetch(url, options)
                    .then(response => response.json())
                    .then(result => {
                        dropdownMenu.innerHTML = '';
                        if (!dropdownMenu.classList.contains('show')) {
                            dropdownMenu.className = `${dropdownMenu.className} show`;
                        }

                        var arr = Array.from(result.suggestions).sort(function (a, b) {
                            return a.value.length - b.value.length;
                        });

                        for (var i = 0; i < arr.length; i++) {
                            let opt = document.createElement('div');
                            opt.classList.add('drop-down-row');
                            for (var p in arr[i].data) {
                                var property = p;
                                var value = arr[i].data[p];
                                if (value != null) {
                                    opt.setAttribute(`sd-${property}`, value);
                                }
                            }
                            opt.innerText = arr[i].value;
                            dropdownMenu.append(opt);
                            opt.addEventListener('click', function (evt) {
                                this.classList.add('.selected');

                                $input.val(this.innerText);

                                Array.from($input[0].attributes).forEach(prop => {
                                    if (prop.name.includes('sd')) {
                                        $input[0].removeAttribute(prop.name);
                                    }
                                });

                                let attributes = Array.from(this.attributes);
                                attributes.forEach(attr => {
                                    if (attr.name.includes('sd')) {
                                        $input.attr(attr.name, attr.value);
                                    }
                                });

                                dropdownMenu.classList.remove('show');
                            });
                        }
                    })
                    .catch(error => console.log("error", error));
            }
        }
    }
    return this.each((index, element) => {
        $(element).on('keyup, keydown', suggestion);
    });
};