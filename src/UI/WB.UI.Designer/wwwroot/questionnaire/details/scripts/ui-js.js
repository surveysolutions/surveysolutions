$(document).on("shown.bs.dropdown.position-calculator", function (event, data) {
    var $item = $('.dropdown-menu', event.target);
    if (!$item.parent().hasClass('dropup')) {
        var target = data ? data.relatedTarget : event.relatedTarget;

        // reset position
        $item.css({ top: 0, left: 0 });

        // calculate new position
        var calculator = new $.PositionCalculator({
            item: $item,
            target: target,
            itemAt: "top left",
            itemOffset: { y: 3, x: 0, mirror: true },
            targetAt: "bottom left",
            flip: "both"
        });
        var posResult = calculator.calculate();

        // set new position
        $item.css({
            top: posResult.moveBy.y + "px",
            left: posResult.moveBy.x + "px"
        });
    }
});