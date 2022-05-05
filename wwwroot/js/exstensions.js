function showMessage(text, type) {
    let $msg = $(`<div class="alert alert-${type == undefined ? 'main' : type}" role="alert">${text}<button type="button" class="close" data-dismiss="alert" aria-label="Close"><i class="fas fa-times"></i></button></div>`);
    $('.messages-wrapper').append($msg);

    $msg.find('button').on('click', () => {
        $msg.fadeOut(350, () => $msg.remove());
    });

    setTimeout(() => { $msg.fadeOut(350, () => $msg.remove()); }, 4000);
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
                //if (obj.Message != null)
                //    showMessage(obj.Message);
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

function formToAjax(form, callback, busyTrigger) {
    if (form instanceof HTMLFormElement) {
        form.addEventListener('submit', function (evt) {
            evt.preventDefault();
            evt.stopPropagation();
            sendAjaxForm(new FormData(this), this.action, callback, busyTrigger);
        });
    }
}

HTMLElement.prototype.toggleClass = function (value) {
    this.className = this.className.includes(value) ? this.className.replace(value, '').trim() : `${value} ${this.className}`;
}

HTMLElement.prototype.removeClass = function (value) {
    this.classList.remove(value);
}

HTMLElement.prototype.addClass = function (value) {
    this.classList.add(value);
}

function sizeAdaptation(videos) {
    videos.forEach(e => {
        e.height = window.innerHeight;
        $(e).css("max-height", window.innerHeight);
    });
    if ($('#sub_categories > div[role=group] .btn-anim.active').length > 0) {
        $('#sub_categories .sizes').css('height', $('#sub_categories .sizes .sizes-inner').height());
    }
}

function scrollHandler(evt) {
    return new Promise((resolve) => {
        if ($(window).scrollTop() > 90) {
            $('.side-toggler').hide();
        }
        else if ($(window).scrollTop() < 90) {
            $('.side-toggler').show();
        }
        if ($(window).scrollTop() > $('.top-bar').height()) {
            $('.top-bar').fadeIn(duration_easing = 50);
            $('.toTop-toggler').addClass('active');
        } else {
            if (url == '/' && detector.isPhoneSized())
                $('.top-bar').fadeOut(duration_easing = 50);
            $('.toTop-toggler').removeClass('active');
        }

        if (detector.isPhoneSized()) {
            if ($(window).scrollTop() > 200) {
                $('#retailcrm-consultant-app').hide();
            }
            else if ($(window).scrollTop() < 200) {
                $('#retailcrm-consultant-app').show();
            }
        }
        resolve();
    });
}