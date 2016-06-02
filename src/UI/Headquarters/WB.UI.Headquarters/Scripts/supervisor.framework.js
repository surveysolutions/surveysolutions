String.prototype.format = String.prototype.f = function () {
    var args = arguments;
    return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
        if (m == "{{") { return "{"; }
        if (m == "}}") { return "}"; }
        return args[n];
    });
};

function Supervisor() { }
Supervisor.prototype = {};
Supervisor.Framework = function () { };
Supervisor.Framework.prototype = {};
Supervisor.Framework.Objects = function () { };
Supervisor.Framework.Objects.prototype = {};
Supervisor.Framework.Objects.isUndefined = function (value) {
    return typeof value == "undefined";
};
Supervisor.Framework.Objects.isNull = function (value) {
    return value === null;
};
Supervisor.Framework.Objects.isString = function (value) {
    return typeof value === 'string';
};
Supervisor.Framework.Objects.isEmpty = function (value) {
    if (!value) {
        return true;
    }
    if ($.isArray(value) || Supervisor.Framework.Objects.isString(value)) {
        return !value.length;
    }
    for (var key in value) {
        if (hasOwnProperty.call(value, key)) {
            return false;
        }
    }
    return true;
};
Supervisor.Framework.Objects.Values = function (object) {
    var index = -1,
        props = Object.keys(object),
        length = props.length,
        result = Array(length);

    while (++index < length) {
        result[index] = object[props[index]];
    }
    return result;
};

Supervisor.Framework.Classes = function () { };
Supervisor.Framework.Classes.prototype = {};
Supervisor.Framework.Classes.inherit = function (child, parent) {
    var f = function () { };
    f.prototype = parent.prototype;
    child.prototype = new f();
    child.prototype.constructor = child;
    child.superclass = parent.prototype;
};

Supervisor.Framework.Browser = function () { };
Supervisor.Framework.Browser.prototype = {};
Supervisor.Framework.Browser.isMsie = function() {
    var ua = window.navigator.userAgent;

    var msie = ua.indexOf('MSIE ');
    if (msie > 0) {
        // IE 10 or older
        return true;
    }

    var trident = ua.indexOf('Trident/');
    if (trident > 0) {
        // IE 11
        return true;
    }

    var edge = ua.indexOf('Edge/');
    if (edge > 0) {
        // IE 12
        return true;
    }

    // other browser
    return false;
};


//------ helpers------
Array.prototype.joinArrayOfObjects = function (key, value) {
    var ret = '';
    this.forEach(function (e) { ret = ret.concat(e[key](), ':', e[value](), ', '); });
    return ret.substring(0, ret.length - 2);
};

Function.prototype.intercept = function (callback) {
    var underlyingObservable = this;
    return ko.dependentObservable({
        read: underlyingObservable,
        write: function (value) {
            callback.call(underlyingObservable, value);
        }
    });
};

Supervisor.Config = {};
Supervisor.Config.prototype = {};
Supervisor.Config.QuestionType = {
    SingleOption: "SingleOption",
    MultyOption: "MultyOption",
    Numeric: "Numeric",
    DateTime: "DateTime",
    Text: "Text",
    AutoPropagate: "AutoPropagate",
    GpsCoordinates: "GPS",
    TextList: "TextList"
};