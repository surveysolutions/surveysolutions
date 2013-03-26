define('utils',
['underscore'],
    function (_) {
        var
            hasProperties = function(obj) {
                for (var prop in obj) {
                    if (obj.hasOwnProperty(prop)) {
                        return true;
                    }
                }
                return false;
            },
            invokeFunctionIfExists = function (callback) {
                if (_.isFunction(callback)) {
                    callback();
                }
            },
            mapMemoToArray = function (items) {
                var underlyingArray = [];
                for (var prop in items) {
                    if (items.hasOwnProperty(prop)) {
                        underlyingArray.push(items[prop]);
                    }
                }
                return underlyingArray;
            },
            regExEscape = function(text) {
                // Removes regEx characters from search filter boxes in our app
                return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
            };

        return {
            hasProperties: hasProperties,
            invokeFunctionIfExists: invokeFunctionIfExists,
            mapMemoToArray: mapMemoToArray,
            regExEscape: regExEscape
        };
    });

