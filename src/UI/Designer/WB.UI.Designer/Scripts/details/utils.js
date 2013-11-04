define('utils',
['underscore', 'config'],
    function (_, config) {
        var
            findChildById = function (collection, id) {
                var item = _.find(collection, { 'id': id });
                var index = _.indexOf(collection, item);
                return {
                    item: item,
                    index: index
                };
            }
        hasProperties = function(obj) {
            for (var prop in obj) {
                if (obj.hasOwnProperty(prop)) {
                    return true;
                }
            }
            return false;
        },
        invokeFunctionIfExists = function(callback) {
            if (_.isFunction(callback)) {
                callback();
            }
        },
        mapMemoToArray = function(items) {
            var underlyingArray = [];
            for (var prop in items) {
                if (items.hasOwnProperty(prop)) {
                    underlyingArray.push(items[prop]);
                }
            }
            return underlyingArray;
        },
        questionUrl = function(id) {
            return config.hashes.detailsQuestion + "/" + id;
        },
        groupUrl = function (id) {
            return config.hashes.detailsGroup + "/" + id;
        };

        return {
            findById: findChildById,
            hasProperties: hasProperties,
            invokeFunctionIfExists: invokeFunctionIfExists,
            mapMemoToArray: mapMemoToArray,
            questionUrl: questionUrl,
            groupUrl: groupUrl
    };
    });

