const elastic = app => {
    app.directive('elastic', {
        mounted(element) {
            var elasticElement = element,
                $elasticElement = $(element),
                initialHeight = initialHeight || $elasticElement.height(),
                delta =
                    parseInt($elasticElement.css('paddingBottom')) +
                        parseInt($elasticElement.css('paddingTop')) || 0,
                resize = function() {
                    $elasticElement.height(Math.max(20, initialHeight));
                    $elasticElement.height(
                        Math.max(20, elasticElement.scrollHeight - delta)
                    );
                };

            $elasticElement.on('input change keyup', resize);
            resize();
        }
    });
};

export default elastic;
