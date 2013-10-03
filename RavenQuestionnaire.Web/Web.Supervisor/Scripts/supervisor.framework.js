function Supervisor() {}
Supervisor.prototype = {};
Supervisor.Framework = function() {};
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
Supervisor.Framework.Classes = function () { };
Supervisor.Framework.Classes.prototype = {};
Supervisor.Framework.Classes.inherit = function(child, parent) {
    var f = function() {};
    f.prototype = parent.prototype;
    child.prototype = new f();
    child.prototype.constructor = child;
    child.superclass = parent.prototype;
};


//------ helpers------
Array.prototype.joinArrayOfObjects = function(key, value) {
    var ret = '';
    this.forEach(function(e) { ret = ret.concat(e[key](), ':', e[value](), ', '); });
    return ret.substring(0, ret.length - 2);
};