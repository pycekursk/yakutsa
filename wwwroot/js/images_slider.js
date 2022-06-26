(function ($) {
  var touchStart = null; //Точка начала касания
  var touchPosition = null; //Текущая позиция
  const sensitivity = 20;

  function TouchStart(e) {
    //Получаем текущую позицию касания
    touchStart = { x: e.changedTouches[0].clientX, y: e.changedTouches[0].clientY };
    touchPosition = { x: touchStart.x, y: touchStart.y };
  }

  function TouchMove(e) {
    //Получаем новую позицию
    touchPosition = { x: e.changedTouches[0].clientX, y: e.changedTouches[0].clientY };
  }

  function TouchEnd(e, color) {
    CheckAction(e); //Определяем, какой жест совершил пользователь
    //Очищаем позиции
    touchStart = null;
    touchPosition = null;
  }

  function CheckAction(evt) {
    var d = //Получаем расстояния от начальной до конечной точек по обеим осям
    {
      x: touchStart.x - touchPosition.x,
      y: touchStart.y - touchPosition.y
    };

    var target = evt.path.find((e) => e.classList.contains("images-slider"));
    if (!target) return;

    let activeElement = target.querySelector("img.active");
    let activeIndicator = target.querySelector(".indicator.active");

    var msg = ""; //Сообщение

    if (Math.abs(d.x) > Math.abs(d.y)) //Проверяем, движение по какой оси было длиннее
    {
      if (Math.abs(d.x) > sensitivity) //Проверяем, было ли движение достаточно длинным
      {

        let nextElement;
        let nextActiveIndicator;
        if (d.x > 0) //Если значение больше нуля, значит пользователь двигал пальцем справа налево
        {
          nextElement = activeElement.nextElementSibling;
        }
        else //Иначе он двигал им слева направо
        {
          nextElement = activeElement.previousElementSibling;
        }

        if (nextElement && nextElement instanceof HTMLImageElement) {
          activeElement.classList.remove('active');
          nextElement.classList.add('active');
          activeElement = nextElement;

          let url = nextElement.getAttribute('src');
          nextActiveIndicator = target.querySelector(`.indicator[indicator-target="${url}"`);

          activeIndicator.classList.remove('active');
          nextActiveIndicator.classList.add('active');

          activeIndicator = nextActiveIndicator;
        }
      }
    }
    else //Аналогичные проверки для вертикальной оси
    {
      if (Math.abs(d.y) > sensitivity) {
        if (d.y > 0) //Свайп вверх
        {
          msg = "Swipe up";
        }
        else //Свайп вниз
        {
          msg = "Swipe down";
        }
      }
    }
  }


  function loadedHandler() {
    //if (document.body.classList.contains('mobile')) return;

    document.addEventListener("touchstart", function (e) { TouchStart(e); });
    document.addEventListener("touchmove", function (e) { TouchMove(e); });
    document.addEventListener("touchend", function (e) { TouchEnd(e, "green"); });
    document.addEventListener("touchcancel", function (e) { TouchEnd(e, "red"); });


    var cards = document.querySelectorAll('.images-slider');
    cards.forEach(c => {
      var images = c.querySelectorAll('img');
      var indicators = c.querySelectorAll('.indicator');

      var imagesCount = c.getAttribute('images-count');

      var activeElement = c.querySelector('img.active');
      var activeIndicator = c.querySelector('.indicator.active');

      var defaultActiveElement = activeElement;
      var defaultActiveIndicator = activeIndicator;

      if (imagesCount && imagesCount > 0) {
        c.addEventListener('mousemove', (e) => {
          var rect = e.target.getBoundingClientRect();
          var x = e.clientX - rect.left;
          var percent = Math.round(Math.abs((x / c.clientWidth) * 100));
          var activeIndex = Math.round((imagesCount * percent) / 100);
          activeIndex = activeIndex > imagesCount ? imagesCount : activeIndex;

          var newActiveElement = images[activeIndex];
          var newActiveIndicator = indicators[activeIndex];

          if (newActiveElement && activeElement != newActiveElement) {

            //$(activeElement).fadeOut(100);
            //$(newActiveElement).fadeIn(100);

            activeElement.classList.remove('active');
            newActiveElement.classList.add('active');
            activeElement = newActiveElement;

            activeIndicator.classList.remove('active');
            newActiveIndicator.classList.add('active');
            activeIndicator = newActiveIndicator;
          }
        }, {
          capture: true
        });
        c.addEventListener('mouseleave', (e) => {
          activeElement.classList.remove('active');
          defaultActiveElement.classList.add('active');
          activeElement = defaultActiveElement;

          activeIndicator.classList.remove('active');
          defaultActiveIndicator.classList.add('active');
          activeIndicator = defaultActiveIndicator;
        });
      }
    });
  }

  return loadedHandler();
})(jQuery);