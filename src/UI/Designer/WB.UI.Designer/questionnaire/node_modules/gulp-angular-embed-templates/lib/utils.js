/**
 * @param {Function} ParentClass class to extend from
 * @param {Object} prototype methods to be added in child class prototype
 * @returns {Function} new "class" extending from ParentClass
 */
function extend(ParentClass, prototype) {
    function ChildClass() {}

    ChildClass.prototype = Object.create ? Object.create(ParentClass.prototype) : new ParentClass();
    ChildClass.prototype.constructor = ChildClass;
    ChildClass.prototype._super = ParentClass.prototype;

    if (prototype) {
        for (var key in prototype) if (prototype.hasOwnProperty(key)) {
            ChildClass.prototype[key] = prototype[key];
        }
    }
    return ChildClass;
}

/**
 * Helper function to walk recursively through arr
 *
 * @param arr
 * @param onIteration
 * @param onEnd
 */
function recursiveCycle(arr, onIteration, onEnd) {
    var i=0;
    function next() {
        if (i >= arr.length) {
            onEnd();
            return;
        }
        var item = arr[i];
        i++;
        onIteration(item, next, onEnd);
    }
    next();
}

/**
 * create a logger object based on passed logger. If passed logger has no some methods then add them
 *
 * @param {Object} [logger] object with methods .debug, .warn, .error. Can be
 * @return {Object}
 */
function createLogger(logger) {
    var result = logger ? Object.assign({}, logger) : {};
    if (!result.debug) result.debug = console.log;
    if (!result.info) result.info = console.info;
    if (!result.warn) result.warn = console.warn;
    if (!result.error) result.error = console.error;
    return result;
}

module.exports = {
    extend: extend,
    recursiveCycle: recursiveCycle,
    createLogger: createLogger
};