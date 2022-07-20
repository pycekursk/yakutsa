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

    $('#menu_modal li a').on('click', (evt) => {
      $(evt.currentTarget).closest('.modal').modal('hide');
    });

    $('#order_configure_form').on('submit', (evt) => {
      evt.preventDefault();
      sendAjaxForm('', 'CheckOffers', (response) => {
        //TODO: дополнить formdata адресом из аттрибутов
        let data = new FormData(evt.currentTarget);

        if (document.getElementById("self_delivery_trigger").checked) {
          data.append('delivery.code', 'self-delivery');
        }
        else {
          data.append('delivery.code', 'dalli');
          data.append('delivery.integrationCode', 'dalli-service');
        }

        let attributes = Array.from(document.getElementById('deliveryAddress_text').attributes);
        attributes.forEach(attr => { 
          if (attr.name.includes('sd')) {
            let str = attr.name.replace('sd-', '');
            let words = str.split('_');
            let prop = "";
            words.forEach((w, i) => {
              prop += w.charAt(0).toUpperCase() + w.slice(1);
            });
            //prop == 'PostalCode' ? 'index' : prop;
            data.append(`suggestionAddress.${prop}`, attr.value);
          }
        });
        try {
          let attributesArray = Array.from($("option:selected", $('#deliveryPartner'))[0].attributes);
          attributesArray.forEach((a, i) => {
            if (a.name.includes('tariff-')) {
              data.append(`delivery.${a.name.replace('tariff-', '').replaceAll('_', '.')}`, a.value);
            }
          });

          attributesArray = Array.from($("option:selected", $('#PVZList'))[0].attributes);
          attributesArray.forEach((a, i) => {
            if (a.name.includes('pickuppoint')) {
              let propertyName = a.name.replace('pickuppoint', '');
              propertyName = 'pickuppoint' + capitalizeFirstLetter(propertyName);
              data.append(`delivery.data.${propertyName}`, a.value);
              console.log(propertyName, a.value);
            }
          });

          var paymentType = $('option:selected', $('#paymentType'));
          data.append(`delivery.data.extraData.paytype`, paymentType.val());
        } catch (e) {
          console.log(e);
        }

        sendAjaxForm(data, 'OrderOptions', (response) => {
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

    $('#deliveryAddress_text').on('change', (evt) => {
      console.log(evt);

      let $pvzListWrapper = $('.pvzlist-wrapper');
      if ($pvzListWrapper.hasClass('active')) {
        $pvzListWrapper.removeClass('active');
        $pvzListWrapper.animate({ 'height': 'toggle' });
      }

      if (evt.target.value.length <= 10) {
        $('select[name=deliveryTariff] option:not([Value=header]), select[name=deliveryTariff]:not(#deliveryTariff)').remove();
        let tariffs = document.getElementById('deliveryTariff');
        if (tariffs != null) tariffs.setAttribute("disabled", true);
        return;
      }
      else {
        setTimeout(() => {
          let attributes = Array.from(document.getElementById('deliveryAddress_text').attributes);
          let formData = new FormData();
          attributes.forEach(attr => {
            if (attr.name.includes('sd')) {
              let str = attr.name.replace('sd-', '');
              let words = str.split('_');
              let prop = "";
              words.forEach((w, i) => {
                prop += w.charAt(0).toUpperCase() + w.slice(1);
              });
              formData.append(prop, attr.value);
            }
          });

          formData.append('text', document.getElementById('deliveryAddress_text').value);

          sendAjaxForm(formData, "/Products/CalculateDelivery", (response) => {
            $('select[name="deliveryPartner"] option:not([Value=header]), select[name=deliveryTariff]:not(#deliveryTariff)').remove();
            if (response.Success) {
              let $tariffSelect = $('#deliveryPartner');
              let tariffs = JSON.parse(response.Json);
              console.log(tariffs);
              for (var i = 0; i < tariffs.length; i++) {
                for (var x = 0; x < tariffs[i].Price.length; x++) {
                  let rupost = tariffs[i].Partner == "rupost" ? `(${tariffs[i].Price[x].RupostFriendlyName})` : "";
                  let price = tariffs[i].Price[x].Price == 0 ? "" : `<span> - ${tariffs[i].Price[x].Price} руб.</span>`;
                  let infoText = tariffs[i].Price[x].InfoText == "" ? "" : `<span>, ${tariffs[i].Price[x].InfoText}</span>`;
                  //tariff-data_extraData_partner="${tariffs[i].Partner}"
                  $tariffSelect.append(`<option class="py-1" tariff-type='${tariffs[i].Price[x].Type}' data-json='${JSON.stringify(tariffs[i].Price[x])}'  tariff-data_tariff="${tariffs[i].Price[x].Service}" tariff-code="${tariffs[i].Code} " tariff-integrationCode="dalli-service" tariff-data_payerType="sender"><span style="display:block"><strong>${tariffs[i].Price[x].FriendlyName}${rupost}</strong></span>${infoText}${price}</option>`);
                  //$tariffSelect.append(`$<option class="py-1" tariff-data_tariff="${tariffs[i].Price[x].Service}" tariff-code="dalli" tariff-integrationCode="dalli-service" tariff-data_payerType="sender" tariff-cost="${tariffs[i].Price[x].Price}"><span style="display:block"><strong>${tariffs[i].Price[x].FriendlyName}${rupost}</strong></span><span>, ${tariffs[i].Price[x].InfoText}</span><span> - ${tariffs[i].Price[x].Price} руб.</span></option>`);
                }
              }

              $tariffSelect[0].removeAttribute("disabled");

              var select = document.getElementById('deliveryPartner');
              select.addEventListener('change', function () {
                let selectedOption = this.selectedOptions[0];
                let objJson = selectedOption.getAttribute('data-json');
                let obj = JSON.parse(objJson);
                let $pvzListWrapper = $('.pvzlist-wrapper');

                let $PVZList = $('#PVZList');

                if (obj.IsPVZType) {
                  if (!$pvzListWrapper.hasClass('active')) {
                    $pvzListWrapper.addClass('active');
                    $pvzListWrapper.animate({ 'height': 'toggle' });

                    obj.PVZList.Point.forEach(p => {
                      console.log(p);
                      //pickuppointSchedule='${p.Worktime}' pickuppointPhone='${p.Phone}' pickuppointAddress='${p.Address}'
                      $PVZList.append(`<option pickuppointId='${p.Code}' class="py-1">${p.Address} (${p.Worktime})</option>`);
                    });


                  }
                }
                else {
                  if ($pvzListWrapper.hasClass('active')) {
                    $pvzListWrapper.removeClass('active');
                    $pvzListWrapper.animate({ 'height': 'toggle' });
                    $PVZList.html('');
                  }
                }
              });

              //$("#deliveryTariff").click();
            }
            else {
              showMessage(response.Message, "red");
            }
          }, true);
          //console.log(evt.target, evt.currentTarget);
        }, 250);
      }
    });



    $('#self_delivery_trigger').on('change', function (e) {
      var $address_text = $('#address_text');
      if (e.currentTarget.checked) {
        $address_text
          .removeAttr('required')
          .attr('disabled', 'True');

        $('#deliveryPartner')
          .removeAttr('required')
          .attr('disabled', 'True');
      }
      else if (!e.currentTarget.checked) {
        $address_text
          .attr('required', 'True')
          .removeAttr('disabled');
      }
    });

    $('input[name="deliveryAddress.Text"]').on('change', (evt) => {
      let $wrapper = $('#deliveryTariff').closest('.input-wrapper');
      $('#deliveryType :nth-child(1)').prop('selected', true);
      $wrapper.css('height', 0);
      $wrapper.addClass('hidden-wrapper');
      $('#deliveryTariff').html('');
    });

    //let $pvzListWrapper = $('.pvzlist-wrapper')
    //$pvzListWrapper.addClass('active');
    //$pvzListWrapper.animate({ 'height': 'toggle' });

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


    //if (isDevelopment) {

    //    $('.consultant-widget > .consultant-icon').on('click', function () {
    //        if (!$(this).hasClass("consultant-icon_opened")) {
    //            try {
    //                ym(87733644, 'reachGoal', 'openChat')
    //            } catch (e) {
    //                console.log(e);
    //            }
    //        };
    //    });




    //}



    //$('.toTop-toggler').vibrate(10);

    $('.labelholder').labelholder();

    setTimeout(() => { $msg.fadeOut(350, () => $msg.remove()); }, 5000);
  }

  return loadedHandler();
}