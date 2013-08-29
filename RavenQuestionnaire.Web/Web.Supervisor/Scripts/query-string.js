if (!Array.prototype.forEach) {
    Array.prototype.forEach = function (fn, scope) {
        for (var i = 0, len = this.length; i < len; ++i) {
            fn.call(scope, this[i], i, this);
        }
    }
}

location.queryString = {};
location.search.slice(1).split('&').forEach(function(keyValuePair) {
    keyValuePair = keyValuePair.split('=');
    location.queryString[keyValuePair[0]] = keyValuePair[1] || '';
});