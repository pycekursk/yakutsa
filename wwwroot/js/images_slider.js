(function ($) {
  var touchStart = null; //Точка начала касания
  var touchPosition = null; //Текущая позиция
  const sensitivity = 25;

  function TouchStart(e) {
    //Получаем текущую позицию касания
    touchStart = { x: e.changedTouches[0].clientX, y: e.changedTouches[0].clientY };
    touchPosition = { x: touchStart.x, y: touchStart.y };
  }

  function TouchMove(e) {
    //Получаем новую позицию
    touchPosition = { x: e.changedTouches[0].clientX, y: e.changedTouches[0].clientY };
  }

  function TouchEnd(e) {
    CheckAction(e);  //Определяем, какой жест совершил пользователь
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

    let activeElement = evt.currentTarget.querySelector("img.active");
    let activeIndicator = evt.currentTarget.querySelector(".indicator.active");

    if (Math.abs(d.x) > Math.abs(d.y) && !(Math.abs(d.x) < sensitivity && Math.abs(d.y) < sensitivity)) //Проверяем, движение по какой оси было длиннее
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

      //showMessage(`${nextElement.src}`);

      evt.preventDefault();

      if (nextElement && nextElement instanceof HTMLImageElement) {
        activeElement.classList.remove('active');
        nextElement.classList.add('active');
        activeElement = nextElement;

        let url = nextElement.getAttribute('src');
        nextActiveIndicator = evt.currentTarget.querySelector(`.indicator[indicator-target="${url}"`);

        activeIndicator.classList.remove('active');
        nextActiveIndicator.classList.add('active');

        activeIndicator = nextActiveIndicator;
      }
    }
  }

  function loadedHandler() {
    document.querySelectorAll('.images-slider').forEach(e => {
      e.addEventListener("touchstart", function (evt) { return TouchStart(evt); }, { passive: false });
      e.addEventListener("touchmove", function (evt) { return TouchMove(evt); }, { passive: false });
      e.addEventListener("touchend", function (evt) { return TouchEnd(evt); }, { passive: false });
      e.addEventListener("touchcancel", function (e) { TouchEnd(e); }, { passive: false });
    });







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