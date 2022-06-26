(function ($) {
  function loadedHandler() {
    if (document.body.classList.contains('mobile')) return;


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