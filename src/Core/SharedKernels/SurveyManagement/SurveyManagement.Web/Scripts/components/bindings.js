(function(ko) {
    ko.bindingHandlers.enterKey = {
        init: function (element, valueAccessor, allBindings, data, context) {
            var wrapper = function (wrappedData, event) {
                if (event.keyCode === 13) {
                    valueAccessor().call(this, wrappedData, event);
                }
            };
            ko.applyBindingsToNode(element, { event: { keyup: wrapper } }, context);
        }
    };
})(ko);