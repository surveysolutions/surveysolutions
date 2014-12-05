function Designer() { }
Designer.prototype = {};
Designer.Framework = function () { };
Designer.Framework.prototype = {};

Designer.Framework.Classes = function () { };
Designer.Framework.Classes.prototype = {};
Designer.Framework.Classes.inherit = function (child, parent) {
    var f = function () { };
    f.prototype = parent.prototype;
    child.prototype = new f();
    child.prototype.constructor = child;
    child.superclass = parent.prototype;
};


//------ helpers------
String.prototype.format = String.prototype.f = function () {
    var args = arguments;
    return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
        if (m == "{{") { return "{"; }
        if (m == "}}") { return "}"; }
        return args[n];
    });
};

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

Designer.Config = {};
Designer.Config.prototype = {};