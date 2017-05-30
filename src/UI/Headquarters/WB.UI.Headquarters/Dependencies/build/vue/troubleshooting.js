(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
module.exports = { "default": require("core-js/library/fn/json/stringify"), __esModule: true };
},{"core-js/library/fn/json/stringify":3}],2:[function(require,module,exports){
module.exports = { "default": require("core-js/library/fn/object/assign"), __esModule: true };
},{"core-js/library/fn/object/assign":4}],3:[function(require,module,exports){
var core  = require('../../modules/_core')
  , $JSON = core.JSON || (core.JSON = {stringify: JSON.stringify});
module.exports = function stringify(it){ // eslint-disable-line no-unused-vars
  return $JSON.stringify.apply($JSON, arguments);
};
},{"../../modules/_core":9}],4:[function(require,module,exports){
require('../../modules/es6.object.assign');
module.exports = require('../../modules/_core').Object.assign;
},{"../../modules/_core":9,"../../modules/es6.object.assign":39}],5:[function(require,module,exports){
module.exports = function(it){
  if(typeof it != 'function')throw TypeError(it + ' is not a function!');
  return it;
};
},{}],6:[function(require,module,exports){
var isObject = require('./_is-object');
module.exports = function(it){
  if(!isObject(it))throw TypeError(it + ' is not an object!');
  return it;
};
},{"./_is-object":22}],7:[function(require,module,exports){
// false -> Array#indexOf
// true  -> Array#includes
var toIObject = require('./_to-iobject')
  , toLength  = require('./_to-length')
  , toIndex   = require('./_to-index');
module.exports = function(IS_INCLUDES){
  return function($this, el, fromIndex){
    var O      = toIObject($this)
      , length = toLength(O.length)
      , index  = toIndex(fromIndex, length)
      , value;
    // Array#includes uses SameValueZero equality algorithm
    if(IS_INCLUDES && el != el)while(length > index){
      value = O[index++];
      if(value != value)return true;
    // Array#toIndex ignores holes, Array#includes - not
    } else for(;length > index; index++)if(IS_INCLUDES || index in O){
      if(O[index] === el)return IS_INCLUDES || index || 0;
    } return !IS_INCLUDES && -1;
  };
};
},{"./_to-index":32,"./_to-iobject":34,"./_to-length":35}],8:[function(require,module,exports){
var toString = {}.toString;

module.exports = function(it){
  return toString.call(it).slice(8, -1);
};
},{}],9:[function(require,module,exports){
var core = module.exports = {version: '2.4.0'};
if(typeof __e == 'number')__e = core; // eslint-disable-line no-undef
},{}],10:[function(require,module,exports){
// optional / simple context binding
var aFunction = require('./_a-function');
module.exports = function(fn, that, length){
  aFunction(fn);
  if(that === undefined)return fn;
  switch(length){
    case 1: return function(a){
      return fn.call(that, a);
    };
    case 2: return function(a, b){
      return fn.call(that, a, b);
    };
    case 3: return function(a, b, c){
      return fn.call(that, a, b, c);
    };
  }
  return function(/* ...args */){
    return fn.apply(that, arguments);
  };
};
},{"./_a-function":5}],11:[function(require,module,exports){
// 7.2.1 RequireObjectCoercible(argument)
module.exports = function(it){
  if(it == undefined)throw TypeError("Can't call method on  " + it);
  return it;
};
},{}],12:[function(require,module,exports){
// Thank's IE8 for his funny defineProperty
module.exports = !require('./_fails')(function(){
  return Object.defineProperty({}, 'a', {get: function(){ return 7; }}).a != 7;
});
},{"./_fails":16}],13:[function(require,module,exports){
var isObject = require('./_is-object')
  , document = require('./_global').document
  // in old IE typeof document.createElement is 'object'
  , is = isObject(document) && isObject(document.createElement);
module.exports = function(it){
  return is ? document.createElement(it) : {};
};
},{"./_global":17,"./_is-object":22}],14:[function(require,module,exports){
// IE 8- don't enum bug keys
module.exports = (
  'constructor,hasOwnProperty,isPrototypeOf,propertyIsEnumerable,toLocaleString,toString,valueOf'
).split(',');
},{}],15:[function(require,module,exports){
var global    = require('./_global')
  , core      = require('./_core')
  , ctx       = require('./_ctx')
  , hide      = require('./_hide')
  , PROTOTYPE = 'prototype';

var $export = function(type, name, source){
  var IS_FORCED = type & $export.F
    , IS_GLOBAL = type & $export.G
    , IS_STATIC = type & $export.S
    , IS_PROTO  = type & $export.P
    , IS_BIND   = type & $export.B
    , IS_WRAP   = type & $export.W
    , exports   = IS_GLOBAL ? core : core[name] || (core[name] = {})
    , expProto  = exports[PROTOTYPE]
    , target    = IS_GLOBAL ? global : IS_STATIC ? global[name] : (global[name] || {})[PROTOTYPE]
    , key, own, out;
  if(IS_GLOBAL)source = name;
  for(key in source){
    // contains in native
    own = !IS_FORCED && target && target[key] !== undefined;
    if(own && key in exports)continue;
    // export native or passed
    out = own ? target[key] : source[key];
    // prevent global pollution for namespaces
    exports[key] = IS_GLOBAL && typeof target[key] != 'function' ? source[key]
    // bind timers to global for call from export context
    : IS_BIND && own ? ctx(out, global)
    // wrap global constructors for prevent change them in library
    : IS_WRAP && target[key] == out ? (function(C){
      var F = function(a, b, c){
        if(this instanceof C){
          switch(arguments.length){
            case 0: return new C;
            case 1: return new C(a);
            case 2: return new C(a, b);
          } return new C(a, b, c);
        } return C.apply(this, arguments);
      };
      F[PROTOTYPE] = C[PROTOTYPE];
      return F;
    // make static versions for prototype methods
    })(out) : IS_PROTO && typeof out == 'function' ? ctx(Function.call, out) : out;
    // export proto methods to core.%CONSTRUCTOR%.methods.%NAME%
    if(IS_PROTO){
      (exports.virtual || (exports.virtual = {}))[key] = out;
      // export proto methods to core.%CONSTRUCTOR%.prototype.%NAME%
      if(type & $export.R && expProto && !expProto[key])hide(expProto, key, out);
    }
  }
};
// type bitmap
$export.F = 1;   // forced
$export.G = 2;   // global
$export.S = 4;   // static
$export.P = 8;   // proto
$export.B = 16;  // bind
$export.W = 32;  // wrap
$export.U = 64;  // safe
$export.R = 128; // real proto method for `library` 
module.exports = $export;
},{"./_core":9,"./_ctx":10,"./_global":17,"./_hide":19}],16:[function(require,module,exports){
module.exports = function(exec){
  try {
    return !!exec();
  } catch(e){
    return true;
  }
};
},{}],17:[function(require,module,exports){
// https://github.com/zloirock/core-js/issues/86#issuecomment-115759028
var global = module.exports = typeof window != 'undefined' && window.Math == Math
  ? window : typeof self != 'undefined' && self.Math == Math ? self : Function('return this')();
if(typeof __g == 'number')__g = global; // eslint-disable-line no-undef
},{}],18:[function(require,module,exports){
var hasOwnProperty = {}.hasOwnProperty;
module.exports = function(it, key){
  return hasOwnProperty.call(it, key);
};
},{}],19:[function(require,module,exports){
var dP         = require('./_object-dp')
  , createDesc = require('./_property-desc');
module.exports = require('./_descriptors') ? function(object, key, value){
  return dP.f(object, key, createDesc(1, value));
} : function(object, key, value){
  object[key] = value;
  return object;
};
},{"./_descriptors":12,"./_object-dp":24,"./_property-desc":29}],20:[function(require,module,exports){
module.exports = !require('./_descriptors') && !require('./_fails')(function(){
  return Object.defineProperty(require('./_dom-create')('div'), 'a', {get: function(){ return 7; }}).a != 7;
});
},{"./_descriptors":12,"./_dom-create":13,"./_fails":16}],21:[function(require,module,exports){
// fallback for non-array-like ES3 and non-enumerable old V8 strings
var cof = require('./_cof');
module.exports = Object('z').propertyIsEnumerable(0) ? Object : function(it){
  return cof(it) == 'String' ? it.split('') : Object(it);
};
},{"./_cof":8}],22:[function(require,module,exports){
module.exports = function(it){
  return typeof it === 'object' ? it !== null : typeof it === 'function';
};
},{}],23:[function(require,module,exports){
'use strict';
// 19.1.2.1 Object.assign(target, source, ...)
var getKeys  = require('./_object-keys')
  , gOPS     = require('./_object-gops')
  , pIE      = require('./_object-pie')
  , toObject = require('./_to-object')
  , IObject  = require('./_iobject')
  , $assign  = Object.assign;

// should work with symbols and should have deterministic property order (V8 bug)
module.exports = !$assign || require('./_fails')(function(){
  var A = {}
    , B = {}
    , S = Symbol()
    , K = 'abcdefghijklmnopqrst';
  A[S] = 7;
  K.split('').forEach(function(k){ B[k] = k; });
  return $assign({}, A)[S] != 7 || Object.keys($assign({}, B)).join('') != K;
}) ? function assign(target, source){ // eslint-disable-line no-unused-vars
  var T     = toObject(target)
    , aLen  = arguments.length
    , index = 1
    , getSymbols = gOPS.f
    , isEnum     = pIE.f;
  while(aLen > index){
    var S      = IObject(arguments[index++])
      , keys   = getSymbols ? getKeys(S).concat(getSymbols(S)) : getKeys(S)
      , length = keys.length
      , j      = 0
      , key;
    while(length > j)if(isEnum.call(S, key = keys[j++]))T[key] = S[key];
  } return T;
} : $assign;
},{"./_fails":16,"./_iobject":21,"./_object-gops":25,"./_object-keys":27,"./_object-pie":28,"./_to-object":36}],24:[function(require,module,exports){
var anObject       = require('./_an-object')
  , IE8_DOM_DEFINE = require('./_ie8-dom-define')
  , toPrimitive    = require('./_to-primitive')
  , dP             = Object.defineProperty;

exports.f = require('./_descriptors') ? Object.defineProperty : function defineProperty(O, P, Attributes){
  anObject(O);
  P = toPrimitive(P, true);
  anObject(Attributes);
  if(IE8_DOM_DEFINE)try {
    return dP(O, P, Attributes);
  } catch(e){ /* empty */ }
  if('get' in Attributes || 'set' in Attributes)throw TypeError('Accessors not supported!');
  if('value' in Attributes)O[P] = Attributes.value;
  return O;
};
},{"./_an-object":6,"./_descriptors":12,"./_ie8-dom-define":20,"./_to-primitive":37}],25:[function(require,module,exports){
exports.f = Object.getOwnPropertySymbols;
},{}],26:[function(require,module,exports){
var has          = require('./_has')
  , toIObject    = require('./_to-iobject')
  , arrayIndexOf = require('./_array-includes')(false)
  , IE_PROTO     = require('./_shared-key')('IE_PROTO');

module.exports = function(object, names){
  var O      = toIObject(object)
    , i      = 0
    , result = []
    , key;
  for(key in O)if(key != IE_PROTO)has(O, key) && result.push(key);
  // Don't enum bug & hidden keys
  while(names.length > i)if(has(O, key = names[i++])){
    ~arrayIndexOf(result, key) || result.push(key);
  }
  return result;
};
},{"./_array-includes":7,"./_has":18,"./_shared-key":30,"./_to-iobject":34}],27:[function(require,module,exports){
// 19.1.2.14 / 15.2.3.14 Object.keys(O)
var $keys       = require('./_object-keys-internal')
  , enumBugKeys = require('./_enum-bug-keys');

module.exports = Object.keys || function keys(O){
  return $keys(O, enumBugKeys);
};
},{"./_enum-bug-keys":14,"./_object-keys-internal":26}],28:[function(require,module,exports){
exports.f = {}.propertyIsEnumerable;
},{}],29:[function(require,module,exports){
module.exports = function(bitmap, value){
  return {
    enumerable  : !(bitmap & 1),
    configurable: !(bitmap & 2),
    writable    : !(bitmap & 4),
    value       : value
  };
};
},{}],30:[function(require,module,exports){
var shared = require('./_shared')('keys')
  , uid    = require('./_uid');
module.exports = function(key){
  return shared[key] || (shared[key] = uid(key));
};
},{"./_shared":31,"./_uid":38}],31:[function(require,module,exports){
var global = require('./_global')
  , SHARED = '__core-js_shared__'
  , store  = global[SHARED] || (global[SHARED] = {});
module.exports = function(key){
  return store[key] || (store[key] = {});
};
},{"./_global":17}],32:[function(require,module,exports){
var toInteger = require('./_to-integer')
  , max       = Math.max
  , min       = Math.min;
module.exports = function(index, length){
  index = toInteger(index);
  return index < 0 ? max(index + length, 0) : min(index, length);
};
},{"./_to-integer":33}],33:[function(require,module,exports){
// 7.1.4 ToInteger
var ceil  = Math.ceil
  , floor = Math.floor;
module.exports = function(it){
  return isNaN(it = +it) ? 0 : (it > 0 ? floor : ceil)(it);
};
},{}],34:[function(require,module,exports){
// to indexed object, toObject with fallback for non-array-like ES3 strings
var IObject = require('./_iobject')
  , defined = require('./_defined');
module.exports = function(it){
  return IObject(defined(it));
};
},{"./_defined":11,"./_iobject":21}],35:[function(require,module,exports){
// 7.1.15 ToLength
var toInteger = require('./_to-integer')
  , min       = Math.min;
module.exports = function(it){
  return it > 0 ? min(toInteger(it), 0x1fffffffffffff) : 0; // pow(2, 53) - 1 == 9007199254740991
};
},{"./_to-integer":33}],36:[function(require,module,exports){
// 7.1.13 ToObject(argument)
var defined = require('./_defined');
module.exports = function(it){
  return Object(defined(it));
};
},{"./_defined":11}],37:[function(require,module,exports){
// 7.1.1 ToPrimitive(input [, PreferredType])
var isObject = require('./_is-object');
// instead of the ES6 spec version, we didn't implement @@toPrimitive case
// and the second argument - flag - preferred type is a string
module.exports = function(it, S){
  if(!isObject(it))return it;
  var fn, val;
  if(S && typeof (fn = it.toString) == 'function' && !isObject(val = fn.call(it)))return val;
  if(typeof (fn = it.valueOf) == 'function' && !isObject(val = fn.call(it)))return val;
  if(!S && typeof (fn = it.toString) == 'function' && !isObject(val = fn.call(it)))return val;
  throw TypeError("Can't convert object to primitive value");
};
},{"./_is-object":22}],38:[function(require,module,exports){
var id = 0
  , px = Math.random();
module.exports = function(key){
  return 'Symbol('.concat(key === undefined ? '' : key, ')_', (++id + px).toString(36));
};
},{}],39:[function(require,module,exports){
// 19.1.3.1 Object.assign(target, source)
var $export = require('./_export');

$export($export.S + $export.F, 'Object', {assign: require('./_object-assign')});
},{"./_export":15,"./_object-assign":23}],40:[function(require,module,exports){
var _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; };

var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) { return typeof obj; } : function (obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; };

/*! flatpickr v2.6.3, @license MIT */
function Flatpickr(element, config) {
	var self = this;

	self._ = {};
	self._.afterDayAnim = afterDayAnim;
	self.changeMonth = changeMonth;
	self.changeYear = changeYear;
	self.clear = clear;
	self.close = close;
	self._createElement = createElement;
	self.destroy = destroy;
	self.isEnabled = isEnabled;
	self.jumpToDate = jumpToDate;
	self.open = open;
	self.redraw = redraw;
	self.set = set;
	self.setDate = setDate;
	self.toggle = toggle;

	function init() {
		self.element = self.input = element;
		self.instanceConfig = config || {};
		self.parseDate = Flatpickr.prototype.parseDate.bind(self);
		self.formatDate = Flatpickr.prototype.formatDate.bind(self);

		setupFormats();
		parseConfig();
		setupLocale();
		setupInputs();
		setupDates();
		setupHelperFunctions();

		self.isOpen = false;

		self.isMobile = !self.config.disableMobile && !self.config.inline && self.config.mode === "single" && !self.config.disable.length && !self.config.enable.length && !self.config.weekNumbers && /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);

		if (!self.isMobile) build();

		bindEvents();

		if (self.selectedDates.length || self.config.noCalendar) {
			if (self.config.enableTime) {
				setHoursFromDate(self.config.noCalendar ? self.latestSelectedDateObj || self.config.minDate : null);
			}
			updateValue();
		}

		if (self.config.weekNumbers) {
			self.calendarContainer.style.width = self.daysContainer.offsetWidth + self.weekWrapper.offsetWidth + "px";
		}

		self.showTimeInput = self.selectedDates.length > 0 || self.config.noCalendar;

		if (!self.isMobile) positionCalendar();

		triggerEvent("Ready");
	}

	/**
  * Binds a function to the current flatpickr instance
  * @param {Function} fn the function
  * @return {Function} the function bound to the instance
  */
	function bindToInstance(fn) {
		return fn.bind(self);
	}

	/**
  * The handler for all events targeting the time inputs
  * @param {Event} e the event - "input", "wheel", "increment", etc
  */
	function updateTime(e) {
		if (self.config.noCalendar && !self.selectedDates.length)
			// picking time only
			self.selectedDates = [self.now];

		timeWrapper(e);

		if (!self.selectedDates.length) return;

		if (!self.minDateHasTime || e.type !== "input" || e.target.value.length >= 2) {
			setHoursFromInputs();
			updateValue();
		} else {
			setTimeout(function () {
				setHoursFromInputs();
				updateValue();
			}, 1000);
		}
	}

	/**
  * Syncs the selected date object time with user's time input
  */
	function setHoursFromInputs() {
		if (!self.config.enableTime) return;

		var hours = (parseInt(self.hourElement.value, 10) || 0) % (self.amPM ? 12 : 24),
		    minutes = (parseInt(self.minuteElement.value, 10) || 0) % 60,
		    seconds = self.config.enableSeconds ? (parseInt(self.secondElement.value, 10) || 0) % 60 : 0;

		if (self.amPM !== undefined) hours = hours % 12 + 12 * (self.amPM.textContent === "PM");

		if (self.minDateHasTime && compareDates(self.latestSelectedDateObj, self.config.minDate) === 0) {

			hours = Math.max(hours, self.config.minDate.getHours());
			if (hours === self.config.minDate.getHours()) minutes = Math.max(minutes, self.config.minDate.getMinutes());
		}

		if (self.maxDateHasTime && compareDates(self.latestSelectedDateObj, self.config.maxDate) === 0) {
			hours = Math.min(hours, self.config.maxDate.getHours());
			if (hours === self.config.maxDate.getHours()) minutes = Math.min(minutes, self.config.maxDate.getMinutes());
		}

		setHours(hours, minutes, seconds);
	}

	/**
  * Syncs time input values with a date
  * @param {Date} dateObj the date to sync with
  */
	function setHoursFromDate(dateObj) {
		var date = dateObj || self.latestSelectedDateObj;

		if (date) setHours(date.getHours(), date.getMinutes(), date.getSeconds());
	}

	/**
  * Sets the hours, minutes, and optionally seconds
  * of the latest selected date object and the
  * corresponding time inputs
  * @param {Number} hours the hour. whether its military
  *                 or am-pm gets inferred from config
  * @param {Number} minutes the minutes
  * @param {Number} seconds the seconds (optional)
  */
	function setHours(hours, minutes, seconds) {
		if (self.selectedDates.length) {
			self.latestSelectedDateObj.setHours(hours % 24, minutes, seconds || 0, 0);
		}

		if (!self.config.enableTime || self.isMobile) return;

		self.hourElement.value = self.pad(!self.config.time_24hr ? (12 + hours) % 12 + 12 * (hours % 12 === 0) : hours);

		self.minuteElement.value = self.pad(minutes);

		if (!self.config.time_24hr) self.amPM.textContent = hours >= 12 ? "PM" : "AM";

		if (self.config.enableSeconds === true) self.secondElement.value = self.pad(seconds);
	}

	/**
  * Handles the year input and incrementing events
  * @param {Event} event the keyup or increment event
  */
	function onYearInput(event) {
		var year = event.target.value;
		if (event.delta) year = (parseInt(year) + event.delta).toString();

		if (year.length === 4 || event.key === "Enter") {
			self.currentYearElement.blur();
			if (!/[^\d]/.test(year)) changeYear(year);
		}
	}

	/**
  * Essentially addEventListener + tracking
  * @param {Element} element the element to addEventListener to
  * @param {String} event the event name
  * @param {Function} handler the event handler
  */
	function bind(element, event, handler) {
		if (event instanceof Array) return event.forEach(function (ev) {
			return bind(element, ev, handler);
		});

		if (element instanceof Array) return element.forEach(function (el) {
			return bind(el, event, handler);
		});

		element.addEventListener(event, handler);
		self._handlers.push({ element: element, event: event, handler: handler });
	}

	/**
  * A mousedown handler which mimics click.
  * Minimizes latency, since we don't need to wait for mouseup in most cases.
  * Also, avoids handling right clicks.
  *
  * @param {Function} handler the event handler
  */
	function onClick(handler) {
		return function (evt) {
			return evt.which === 1 && handler(evt);
		};
	}

	/**
  * Adds all the necessary event listeners
  */
	function bindEvents() {
		self._handlers = [];
		self._animationLoop = [];
		if (self.config.wrap) {
			["open", "close", "toggle", "clear"].forEach(function (evt) {
				Array.prototype.forEach.call(self.element.querySelectorAll("[data-" + evt + "]"), function (el) {
					return bind(el, "mousedown", onClick(self[evt]));
				});
			});
		}

		if (self.isMobile) return setupMobile();

		self.debouncedResize = debounce(onResize, 50);
		self.triggerChange = function () {
			triggerEvent("Change");
		};
		self.debouncedChange = debounce(self.triggerChange, 300);

		if (self.config.mode === "range" && self.daysContainer) bind(self.daysContainer, "mouseover", function (e) {
			return onMouseOver(e.target);
		});

		bind(window.document.body, "keydown", onKeyDown);

		if (!self.config.static) bind(self._input, "keydown", onKeyDown);

		if (!self.config.inline && !self.config.static) bind(window, "resize", self.debouncedResize);

		if (window.ontouchstart !== undefined) bind(window.document, "touchstart", documentClick);

		bind(window.document, "mousedown", onClick(documentClick));
		bind(self._input, "blur", documentClick);

		if (self.config.clickOpens === true) bind(self._input, "focus", self.open);

		if (!self.config.noCalendar) {
			self.monthNav.addEventListener("wheel", function (e) {
				return e.preventDefault();
			});
			bind(self.monthNav, "wheel", debounce(onMonthNavScroll, 10));
			bind(self.monthNav, "mousedown", onClick(onMonthNavClick));

			bind(self.monthNav, ["keyup", "increment"], onYearInput);
			bind(self.daysContainer, "mousedown", onClick(selectDate));

			if (self.config.animate) {
				bind(self.daysContainer, ["webkitAnimationEnd", "animationend"], animateDays);
				bind(self.monthNav, ["webkitAnimationEnd", "animationend"], animateMonths);
			}
		}

		if (self.config.enableTime) {
			var selText = function selText(e) {
				return e.target.select();
			};
			bind(self.timeContainer, ["wheel", "input", "increment"], updateTime);
			bind(self.timeContainer, "mousedown", onClick(timeIncrement));

			bind(self.timeContainer, ["wheel", "increment"], self.debouncedChange);
			bind(self.timeContainer, "input", self.triggerChange);

			bind([self.hourElement, self.minuteElement], "focus", selText);

			if (self.secondElement !== undefined) bind(self.secondElement, "focus", function () {
				return self.secondElement.select();
			});

			if (self.amPM !== undefined) {
				bind(self.amPM, "mousedown", onClick(function (e) {
					updateTime(e);
					self.triggerChange(e);
				}));
			}
		}
	}

	function processPostDayAnimation() {
		for (var i = self._animationLoop.length; i--;) {
			self._animationLoop[i]();
			self._animationLoop.splice(i, 1);
		}
	}

	/**
  * Removes the day container that slided out of view
  * @param {Event} e the animation event
  */
	function animateDays(e) {
		if (self.daysContainer.childNodes.length > 1) {
			switch (e.animationName) {
				case "fpSlideLeft":
					self.daysContainer.lastChild.classList.remove("slideLeftNew");
					self.daysContainer.removeChild(self.daysContainer.firstChild);
					self.days = self.daysContainer.firstChild;
					processPostDayAnimation();

					break;

				case "fpSlideRight":
					self.daysContainer.firstChild.classList.remove("slideRightNew");
					self.daysContainer.removeChild(self.daysContainer.lastChild);
					self.days = self.daysContainer.firstChild;
					processPostDayAnimation();

					break;

				default:
					break;
			}
		}
	}

	/**
  * Removes the month element that animated out of view
  * @param {Event} e the animation event
  */
	function animateMonths(e) {
		switch (e.animationName) {
			case "fpSlideLeftNew":
			case "fpSlideRightNew":
				self.navigationCurrentMonth.classList.remove("slideLeftNew");
				self.navigationCurrentMonth.classList.remove("slideRightNew");
				var nav = self.navigationCurrentMonth;

				while (nav.nextSibling && /curr/.test(nav.nextSibling.className)) {
					self.monthNav.removeChild(nav.nextSibling);
				}while (nav.previousSibling && /curr/.test(nav.previousSibling.className)) {
					self.monthNav.removeChild(nav.previousSibling);
				}self.oldCurMonth = null;
				break;
		}
	}

	/**
  * Set the calendar view to a particular date.
  * @param {Date} jumpDate the date to set the view to
  */
	function jumpToDate(jumpDate) {
		jumpDate = jumpDate ? self.parseDate(jumpDate) : self.latestSelectedDateObj || (self.config.minDate > self.now ? self.config.minDate : self.config.maxDate && self.config.maxDate < self.now ? self.config.maxDate : self.now);

		try {
			self.currentYear = jumpDate.getFullYear();
			self.currentMonth = jumpDate.getMonth();
		} catch (e) {
			/* istanbul ignore next */
			console.error(e.stack);
			/* istanbul ignore next */
			console.warn("Invalid date supplied: " + jumpDate);
		}

		self.redraw();
	}

	/**
  * The up/down arrow handler for time inputs
  * @param {Event} e the click event
  */
	function timeIncrement(e) {
		if (~e.target.className.indexOf("arrow")) incrementNumInput(e, e.target.classList.contains("arrowUp") ? 1 : -1);
	}

	/**
  * Increments/decrements the value of input associ-
  * ated with the up/down arrow by dispatching an
  * "increment" event on the input.
  *
  * @param {Event} e the click event
  * @param {Number} delta the diff (usually 1 or -1)
  * @param {Element} inputElem the input element
  */
	function incrementNumInput(e, delta, inputElem) {
		var input = inputElem || e.target.parentNode.childNodes[0];
		var event = createEvent("increment");
		event.delta = delta;
		input.dispatchEvent(event);
	}

	function createNumberInput(inputClassName) {
		var wrapper = createElement("div", "numInputWrapper"),
		    numInput = createElement("input", "numInput " + inputClassName),
		    arrowUp = createElement("span", "arrowUp"),
		    arrowDown = createElement("span", "arrowDown");

		numInput.type = "text";
		numInput.pattern = "\\d*";

		wrapper.appendChild(numInput);
		wrapper.appendChild(arrowUp);
		wrapper.appendChild(arrowDown);

		return wrapper;
	}

	function build() {
		var fragment = window.document.createDocumentFragment();
		self.calendarContainer = createElement("div", "flatpickr-calendar");
		self.calendarContainer.tabIndex = -1;

		if (!self.config.noCalendar) {
			fragment.appendChild(buildMonthNav());
			self.innerContainer = createElement("div", "flatpickr-innerContainer");

			if (self.config.weekNumbers) self.innerContainer.appendChild(buildWeeks());

			self.rContainer = createElement("div", "flatpickr-rContainer");
			self.rContainer.appendChild(buildWeekdays());

			if (!self.daysContainer) {
				self.daysContainer = createElement("div", "flatpickr-days");
				self.daysContainer.tabIndex = -1;
			}

			buildDays();
			self.rContainer.appendChild(self.daysContainer);

			self.innerContainer.appendChild(self.rContainer);
			fragment.appendChild(self.innerContainer);
		}

		if (self.config.enableTime) fragment.appendChild(buildTime());

		toggleClass(self.calendarContainer, "rangeMode", self.config.mode === "range");
		toggleClass(self.calendarContainer, "animate", self.config.animate);

		self.calendarContainer.appendChild(fragment);

		var customAppend = self.config.appendTo && self.config.appendTo.nodeType;

		if (self.config.inline || self.config.static) {
			self.calendarContainer.classList.add(self.config.inline ? "inline" : "static");

			if (self.config.inline && !customAppend) {
				return self.element.parentNode.insertBefore(self.calendarContainer, self._input.nextSibling);
			}

			if (self.config.static) {
				var wrapper = createElement("div", "flatpickr-wrapper");
				self.element.parentNode.insertBefore(wrapper, self.element);
				wrapper.appendChild(self.element);

				if (self.altInput) wrapper.appendChild(self.altInput);

				wrapper.appendChild(self.calendarContainer);
				return;
			}
		}

		(customAppend ? self.config.appendTo : window.document.body).appendChild(self.calendarContainer);
	}

	function createDay(className, date, dayNumber, i) {
		var dateIsEnabled = isEnabled(date, true),
		    dayElement = createElement("span", "flatpickr-day " + className, date.getDate());

		dayElement.dateObj = date;
		dayElement.$i = i;
		dayElement.setAttribute("aria-label", self.formatDate(date, self.config.ariaDateFormat));

		if (compareDates(date, self.now) === 0) {
			self.todayDateElem = dayElement;
			dayElement.classList.add("today");
		}

		if (dateIsEnabled) {
			dayElement.tabIndex = -1;
			if (isDateSelected(date)) {
				dayElement.classList.add("selected");
				self.selectedDateElem = dayElement;
				if (self.config.mode === "range") {
					toggleClass(dayElement, "startRange", compareDates(date, self.selectedDates[0]) === 0);

					toggleClass(dayElement, "endRange", compareDates(date, self.selectedDates[1]) === 0);
				}
			}
		} else {
			dayElement.classList.add("disabled");
			if (self.selectedDates[0] && date > self.minRangeDate && date < self.selectedDates[0]) self.minRangeDate = date;else if (self.selectedDates[0] && date < self.maxRangeDate && date > self.selectedDates[0]) self.maxRangeDate = date;
		}

		if (self.config.mode === "range") {
			if (isDateInRange(date) && !isDateSelected(date)) dayElement.classList.add("inRange");

			if (self.selectedDates.length === 1 && (date < self.minRangeDate || date > self.maxRangeDate)) dayElement.classList.add("notAllowed");
		}

		if (self.config.weekNumbers && className !== "prevMonthDay" && dayNumber % 7 === 1) {
			self.weekNumbers.insertAdjacentHTML("beforeend", "<span class='disabled flatpickr-day'>" + self.config.getWeek(date) + "</span>");
		}

		triggerEvent("DayCreate", dayElement);

		return dayElement;
	}

	function focusOnDay(currentIndex, offset) {
		var newIndex = currentIndex + offset || 0,
		    targetNode = currentIndex !== undefined ? self.days.childNodes[newIndex] : self.selectedDateElem || self.todayDateElem || self.days.childNodes[0],
		    focus = function focus() {
			targetNode = targetNode || self.days.childNodes[newIndex];
			targetNode.focus();

			if (self.config.mode === "range") onMouseOver(targetNode);
		};

		if (targetNode === undefined && offset !== 0) {
			if (offset > 0) {
				self.changeMonth(1);
				newIndex = newIndex % 42;
			} else if (offset < 0) {
				self.changeMonth(-1);
				newIndex += 42;
			}

			return afterDayAnim(focus);
		}

		focus();
	}

	function afterDayAnim(fn) {
		if (self.config.animate === true) return self._animationLoop.push(fn);
		fn();
	}

	function buildDays(delta) {
		var firstOfMonth = (new Date(self.currentYear, self.currentMonth, 1).getDay() - self.l10n.firstDayOfWeek + 7) % 7,
		    isRangeMode = self.config.mode === "range";

		self.prevMonthDays = self.utils.getDaysinMonth((self.currentMonth - 1 + 12) % 12);
		self.selectedDateElem = undefined;
		self.todayDateElem = undefined;

		var daysInMonth = self.utils.getDaysinMonth(),
		    days = window.document.createDocumentFragment();

		var dayNumber = self.prevMonthDays + 1 - firstOfMonth,
		    dayIndex = 0;

		if (self.config.weekNumbers && self.weekNumbers.firstChild) self.weekNumbers.textContent = "";

		if (isRangeMode) {
			// const dateLimits = self.config.enable.length || self.config.disable.length || self.config.mixDate || self.config.maxDate;
			self.minRangeDate = new Date(self.currentYear, self.currentMonth - 1, dayNumber);
			self.maxRangeDate = new Date(self.currentYear, self.currentMonth + 1, (42 - firstOfMonth) % daysInMonth);
		}

		// prepend days from the ending of previous month
		for (; dayNumber <= self.prevMonthDays; dayNumber++, dayIndex++) {
			days.appendChild(createDay("prevMonthDay", new Date(self.currentYear, self.currentMonth - 1, dayNumber), dayNumber, dayIndex));
		}

		// Start at 1 since there is no 0th day
		for (dayNumber = 1; dayNumber <= daysInMonth; dayNumber++, dayIndex++) {
			days.appendChild(createDay("", new Date(self.currentYear, self.currentMonth, dayNumber), dayNumber, dayIndex));
		}

		// append days from the next month
		for (var dayNum = daysInMonth + 1; dayNum <= 42 - firstOfMonth; dayNum++, dayIndex++) {
			days.appendChild(createDay("nextMonthDay", new Date(self.currentYear, self.currentMonth + 1, dayNum % daysInMonth), dayNum, dayIndex));
		}

		if (isRangeMode && self.selectedDates.length === 1 && days.childNodes[0]) {
			self._hidePrevMonthArrow = self._hidePrevMonthArrow || self.minRangeDate > days.childNodes[0].dateObj;

			self._hideNextMonthArrow = self._hideNextMonthArrow || self.maxRangeDate < new Date(self.currentYear, self.currentMonth + 1, 1);
		} else updateNavigationCurrentMonth();

		var dayContainer = createElement("div", "dayContainer");
		dayContainer.appendChild(days);

		if (!self.config.animate || delta === undefined) clearNode(self.daysContainer);else {
			while (self.daysContainer.childNodes.length > 1) {
				self.daysContainer.removeChild(self.daysContainer.firstChild);
			}
		}

		if (delta >= 0) self.daysContainer.appendChild(dayContainer);else self.daysContainer.insertBefore(dayContainer, self.daysContainer.firstChild);

		self.days = self.daysContainer.firstChild;
		return self.daysContainer;
	}

	function clearNode(node) {
		while (node.firstChild) {
			node.removeChild(node.firstChild);
		}
	}

	function buildMonthNav() {
		var monthNavFragment = window.document.createDocumentFragment();
		self.monthNav = createElement("div", "flatpickr-month");

		self.prevMonthNav = createElement("span", "flatpickr-prev-month");
		self.prevMonthNav.innerHTML = self.config.prevArrow;

		self.currentMonthElement = createElement("span", "cur-month");
		self.currentMonthElement.title = self.l10n.scrollTitle;

		var yearInput = createNumberInput("cur-year");
		self.currentYearElement = yearInput.childNodes[0];
		self.currentYearElement.title = self.l10n.scrollTitle;

		if (self.config.minDate) self.currentYearElement.min = self.config.minDate.getFullYear();

		if (self.config.maxDate) {
			self.currentYearElement.max = self.config.maxDate.getFullYear();

			self.currentYearElement.disabled = self.config.minDate && self.config.minDate.getFullYear() === self.config.maxDate.getFullYear();
		}

		self.nextMonthNav = createElement("span", "flatpickr-next-month");
		self.nextMonthNav.innerHTML = self.config.nextArrow;

		self.navigationCurrentMonth = createElement("span", "flatpickr-current-month");
		self.navigationCurrentMonth.appendChild(self.currentMonthElement);
		self.navigationCurrentMonth.appendChild(yearInput);

		monthNavFragment.appendChild(self.prevMonthNav);
		monthNavFragment.appendChild(self.navigationCurrentMonth);
		monthNavFragment.appendChild(self.nextMonthNav);
		self.monthNav.appendChild(monthNavFragment);

		Object.defineProperty(self, "_hidePrevMonthArrow", {
			get: function get() {
				return this.__hidePrevMonthArrow;
			},
			set: function set(bool) {
				if (this.__hidePrevMonthArrow !== bool) self.prevMonthNav.style.display = bool ? "none" : "block";
				this.__hidePrevMonthArrow = bool;
			}
		});

		Object.defineProperty(self, "_hideNextMonthArrow", {
			get: function get() {
				return this.__hideNextMonthArrow;
			},
			set: function set(bool) {
				if (this.__hideNextMonthArrow !== bool) self.nextMonthNav.style.display = bool ? "none" : "block";
				this.__hideNextMonthArrow = bool;
			}
		});

		updateNavigationCurrentMonth();

		return self.monthNav;
	}

	function buildTime() {
		self.calendarContainer.classList.add("hasTime");
		if (self.config.noCalendar) self.calendarContainer.classList.add("noCalendar");
		self.timeContainer = createElement("div", "flatpickr-time");
		self.timeContainer.tabIndex = -1;
		var separator = createElement("span", "flatpickr-time-separator", ":");

		var hourInput = createNumberInput("flatpickr-hour");
		self.hourElement = hourInput.childNodes[0];

		var minuteInput = createNumberInput("flatpickr-minute");
		self.minuteElement = minuteInput.childNodes[0];

		self.hourElement.tabIndex = self.minuteElement.tabIndex = -1;

		self.hourElement.value = self.pad(self.latestSelectedDateObj ? self.latestSelectedDateObj.getHours() : self.config.defaultHour);

		self.minuteElement.value = self.pad(self.latestSelectedDateObj ? self.latestSelectedDateObj.getMinutes() : self.config.defaultMinute);

		self.hourElement.step = self.config.hourIncrement;
		self.minuteElement.step = self.config.minuteIncrement;

		self.hourElement.min = self.config.time_24hr ? 0 : 1;
		self.hourElement.max = self.config.time_24hr ? 23 : 12;

		self.minuteElement.min = 0;
		self.minuteElement.max = 59;

		self.hourElement.title = self.minuteElement.title = self.l10n.scrollTitle;

		self.timeContainer.appendChild(hourInput);
		self.timeContainer.appendChild(separator);
		self.timeContainer.appendChild(minuteInput);

		if (self.config.time_24hr) self.timeContainer.classList.add("time24hr");

		if (self.config.enableSeconds) {
			self.timeContainer.classList.add("hasSeconds");

			var secondInput = createNumberInput("flatpickr-second");
			self.secondElement = secondInput.childNodes[0];

			self.secondElement.value = self.latestSelectedDateObj ? self.pad(self.latestSelectedDateObj.getSeconds()) : "00";

			self.secondElement.step = self.minuteElement.step;
			self.secondElement.min = self.minuteElement.min;
			self.secondElement.max = self.minuteElement.max;

			self.timeContainer.appendChild(createElement("span", "flatpickr-time-separator", ":"));
			self.timeContainer.appendChild(secondInput);
		}

		if (!self.config.time_24hr) {
			// add self.amPM if appropriate
			self.amPM = createElement("span", "flatpickr-am-pm", ["AM", "PM"][self.hourElement.value > 11 | 0]);
			self.amPM.title = self.l10n.toggleTitle;
			self.amPM.tabIndex = -1;
			self.timeContainer.appendChild(self.amPM);
		}

		return self.timeContainer;
	}

	function buildWeekdays() {
		if (!self.weekdayContainer) self.weekdayContainer = createElement("div", "flatpickr-weekdays");

		var firstDayOfWeek = self.l10n.firstDayOfWeek;
		var weekdays = self.l10n.weekdays.shorthand.slice();

		if (firstDayOfWeek > 0 && firstDayOfWeek < weekdays.length) {
			weekdays = [].concat(weekdays.splice(firstDayOfWeek, weekdays.length), weekdays.splice(0, firstDayOfWeek));
		}

		self.weekdayContainer.innerHTML = "\n\t\t<span class=flatpickr-weekday>\n\t\t\t" + weekdays.join("</span><span class=flatpickr-weekday>") + "\n\t\t</span>\n\t\t";

		return self.weekdayContainer;
	}

	/* istanbul ignore next */
	function buildWeeks() {
		self.calendarContainer.classList.add("hasWeeks");
		self.weekWrapper = createElement("div", "flatpickr-weekwrapper");
		self.weekWrapper.appendChild(createElement("span", "flatpickr-weekday", self.l10n.weekAbbreviation));
		self.weekNumbers = createElement("div", "flatpickr-weeks");
		self.weekWrapper.appendChild(self.weekNumbers);

		return self.weekWrapper;
	}

	function changeMonth(value, is_offset, animate) {
		is_offset = is_offset === undefined || is_offset;
		var delta = is_offset ? value : value - self.currentMonth;
		var skipAnimations = !self.config.animate || animate === false;

		if (delta < 0 && self._hidePrevMonthArrow || delta > 0 && self._hideNextMonthArrow) return;

		self.currentMonth += delta;

		if (self.currentMonth < 0 || self.currentMonth > 11) {
			self.currentYear += self.currentMonth > 11 ? 1 : -1;
			self.currentMonth = (self.currentMonth + 12) % 12;

			triggerEvent("YearChange");
		}

		buildDays(!skipAnimations ? delta : undefined);

		if (skipAnimations) {
			triggerEvent("MonthChange");
			return updateNavigationCurrentMonth();
		}

		// remove possible remnants from clicking too fast
		var nav = self.navigationCurrentMonth;
		if (delta < 0) {
			while (nav.nextSibling && /curr/.test(nav.nextSibling.className)) {
				self.monthNav.removeChild(nav.nextSibling);
			}
		} else if (delta > 0) {
			while (nav.previousSibling && /curr/.test(nav.previousSibling.className)) {
				self.monthNav.removeChild(nav.previousSibling);
			}
		}

		self.oldCurMonth = self.navigationCurrentMonth;

		self.navigationCurrentMonth = self.monthNav.insertBefore(self.oldCurMonth.cloneNode(true), delta > 0 ? self.oldCurMonth.nextSibling : self.oldCurMonth);

		if (delta > 0) {
			self.daysContainer.firstChild.classList.add("slideLeft");
			self.daysContainer.lastChild.classList.add("slideLeftNew");

			self.oldCurMonth.classList.add("slideLeft");
			self.navigationCurrentMonth.classList.add("slideLeftNew");
		} else if (delta < 0) {
			self.daysContainer.firstChild.classList.add("slideRightNew");
			self.daysContainer.lastChild.classList.add("slideRight");

			self.oldCurMonth.classList.add("slideRight");
			self.navigationCurrentMonth.classList.add("slideRightNew");
		}

		self.currentMonthElement = self.navigationCurrentMonth.firstChild;
		self.currentYearElement = self.navigationCurrentMonth.lastChild.childNodes[0];

		updateNavigationCurrentMonth();
		self.oldCurMonth.firstChild.textContent = self.utils.monthToStr(self.currentMonth - delta);

		triggerEvent("MonthChange");

		if (document.activeElement && document.activeElement.$i) {
			var index = document.activeElement.$i;
			afterDayAnim(function () {
				focusOnDay(index, 0);
			});
		}
	}

	function clear(triggerChangeEvent) {
		self.input.value = "";

		if (self.altInput) self.altInput.value = "";

		if (self.mobileInput) self.mobileInput.value = "";

		self.selectedDates = [];
		self.latestSelectedDateObj = undefined;
		self.showTimeInput = false;

		self.redraw();

		if (triggerChangeEvent !== false)
			// triggerChangeEvent is true (default) or an Event
			triggerEvent("Change");
	}

	function close() {
		self.isOpen = false;

		if (!self.isMobile) {
			self.calendarContainer.classList.remove("open");
			self._input.classList.remove("active");
		}

		triggerEvent("Close");
	}

	function destroy() {
		for (var i = self._handlers.length; i--;) {
			var h = self._handlers[i];
			h.element.removeEventListener(h.event, h.handler);
		}

		self._handlers = [];

		if (self.mobileInput) {
			if (self.mobileInput.parentNode) self.mobileInput.parentNode.removeChild(self.mobileInput);
			self.mobileInput = null;
		} else if (self.calendarContainer && self.calendarContainer.parentNode) self.calendarContainer.parentNode.removeChild(self.calendarContainer);

		if (self.altInput) {
			self.input.type = "text";
			if (self.altInput.parentNode) self.altInput.parentNode.removeChild(self.altInput);
			delete self.altInput;
		}

		if (self.input) {
			self.input.type = self.input._type;
			self.input.classList.remove("flatpickr-input");
			self.input.removeAttribute("readonly");
			self.input.value = "";
		}

		["_showTimeInput", "latestSelectedDateObj", "_hideNextMonthArrow", "_hidePrevMonthArrow", "__hideNextMonthArrow", "__hidePrevMonthArrow", "isMobile", "isOpen", "selectedDateElem", "minDateHasTime", "maxDateHasTime", "days", "daysContainer", "_input", "_positionElement", "innerContainer", "rContainer", "monthNav", "todayDateElem", "calendarContainer", "weekdayContainer", "prevMonthNav", "nextMonthNav", "currentMonthElement", "currentYearElement", "navigationCurrentMonth", "selectedDateElem", "config"].forEach(function (k) {
			return delete self[k];
		});
	}

	function isCalendarElem(elem) {
		if (self.config.appendTo && self.config.appendTo.contains(elem)) return true;

		return self.calendarContainer.contains(elem);
	}

	function documentClick(e) {
		if (self.isOpen && !self.config.inline) {
			var isCalendarElement = isCalendarElem(e.target);
			var isInput = e.target === self.input || e.target === self.altInput || self.element.contains(e.target) ||
			// web components
			e.path && e.path.indexOf && (~e.path.indexOf(self.input) || ~e.path.indexOf(self.altInput));

			var lostFocus = e.type === "blur" ? isInput && e.relatedTarget && !isCalendarElem(e.relatedTarget) : !isInput && !isCalendarElement;

			if (lostFocus) {
				e.preventDefault();
				self.close();

				if (self.config.mode === "range" && self.selectedDates.length === 1) {
					self.clear(false);
					self.redraw();
				}
			}
		}
	}

	function changeYear(newYear) {
		if (!newYear || self.currentYearElement.min && newYear < self.currentYearElement.min || self.currentYearElement.max && newYear > self.currentYearElement.max) return;

		var newYearNum = parseInt(newYear, 10),
		    isNewYear = self.currentYear !== newYearNum;

		self.currentYear = newYearNum || self.currentYear;

		if (self.config.maxDate && self.currentYear === self.config.maxDate.getFullYear()) {
			self.currentMonth = Math.min(self.config.maxDate.getMonth(), self.currentMonth);
		} else if (self.config.minDate && self.currentYear === self.config.minDate.getFullYear()) {
			self.currentMonth = Math.max(self.config.minDate.getMonth(), self.currentMonth);
		}

		if (isNewYear) {
			self.redraw();
			triggerEvent("YearChange");
		}
	}

	function isEnabled(date, timeless) {
		if (self.config.minDate && compareDates(date, self.config.minDate, timeless !== undefined ? timeless : !self.minDateHasTime) < 0 || self.config.maxDate && compareDates(date, self.config.maxDate, timeless !== undefined ? timeless : !self.maxDateHasTime) > 0) return false;

		if (!self.config.enable.length && !self.config.disable.length) return true;

		var dateToCheck = self.parseDate(date, null, true); // timeless

		var bool = self.config.enable.length > 0,
		    array = bool ? self.config.enable : self.config.disable;

		for (var i = 0, d; i < array.length; i++) {
			d = array[i];

			if (d instanceof Function && d(dateToCheck)) // disabled by function
				return bool;else if (d instanceof Date && d.getTime() === dateToCheck.getTime())
				// disabled by date
				return bool;else if (typeof d === "string" && self.parseDate(d, null, true).getTime() === dateToCheck.getTime())
				// disabled by date string
				return bool;else if ( // disabled by range
			(typeof d === "undefined" ? "undefined" : _typeof(d)) === "object" && d.from && d.to && dateToCheck >= d.from && dateToCheck <= d.to) return bool;
		}

		return !bool;
	}

	function onKeyDown(e) {
		var isInput = e.target === self._input;
		var calendarElem = isCalendarElem(e.target);
		var allowInput = self.config.allowInput;
		var allowKeydown = self.isOpen && (!allowInput || !isInput);
		var allowInlineKeydown = self.config.inline && isInput && !allowInput;

		if (e.key === "Enter" && allowInput && isInput) {
			self.setDate(self._input.value, true, e.target === self.altInput ? self.config.altFormat : self.config.dateFormat);
			return e.target.blur();
		} else if (calendarElem || allowKeydown || allowInlineKeydown) {
			var isTimeObj = self.timeContainer && self.timeContainer.contains(e.target);
			switch (e.key) {
				case "Enter":
					if (isTimeObj) updateValue();else selectDate(e);

					break;

				case "Escape":
					// escape
					e.preventDefault();
					self.close();
					break;

				case "ArrowLeft":
				case "ArrowRight":
					if (!isTimeObj) {
						e.preventDefault();

						if (self.daysContainer) {
							var _delta = e.key === "ArrowRight" ? 1 : -1;

							if (!e.ctrlKey) focusOnDay(e.target.$i, _delta);else changeMonth(_delta, true);
						} else if (self.config.enableTime && !isTimeObj) self.hourElement.focus();
					}

					break;

				case "ArrowUp":
				case "ArrowDown":
					e.preventDefault();
					var delta = e.key === "ArrowDown" ? 1 : -1;

					if (self.daysContainer) {
						if (e.ctrlKey) {
							changeYear(self.currentYear - delta);
							focusOnDay(e.target.$i, 0);
						} else if (!isTimeObj) focusOnDay(e.target.$i, delta * 7);
					} else if (self.config.enableTime) {
						if (!isTimeObj) self.hourElement.focus();
						updateTime(e);
					}

					break;

				case "Tab":
					if (e.target === self.hourElement) {
						e.preventDefault();
						self.minuteElement.select();
					} else if (e.target === self.minuteElement && (self.secondElement || self.amPM)) {
						e.preventDefault();
						(self.secondElement || self.amPM).focus();
					} else if (e.target === self.secondElement) {
						e.preventDefault();
						self.amPM.focus();
					}

					break;

				case "a":
					if (e.target === self.amPM) {
						self.amPM.textContent = "AM";
						setHoursFromInputs();
						updateValue();
					}
					break;

				case "p":
					if (e.target === self.amPM) {
						self.amPM.textContent = "PM";
						setHoursFromInputs();
						updateValue();
					}
					break;

				default:
					break;

			}

			triggerEvent("KeyDown", e);
		}
	}

	function onMouseOver(elem) {
		if (self.selectedDates.length !== 1 || !elem.classList.contains("flatpickr-day")) return;

		var hoverDate = elem.dateObj,
		    initialDate = self.parseDate(self.selectedDates[0], null, true),
		    rangeStartDate = Math.min(hoverDate.getTime(), self.selectedDates[0].getTime()),
		    rangeEndDate = Math.max(hoverDate.getTime(), self.selectedDates[0].getTime()),
		    containsDisabled = false;

		for (var t = rangeStartDate; t < rangeEndDate; t += self.utils.duration.DAY) {
			if (!isEnabled(new Date(t))) {
				containsDisabled = true;
				break;
			}
		}

		var _loop = function _loop(timestamp, i) {
			var outOfRange = timestamp < self.minRangeDate.getTime() || timestamp > self.maxRangeDate.getTime(),
			    dayElem = self.days.childNodes[i];

			if (outOfRange) {
				self.days.childNodes[i].classList.add("notAllowed");
				["inRange", "startRange", "endRange"].forEach(function (c) {
					dayElem.classList.remove(c);
				});
				return "continue";
			} else if (containsDisabled && !outOfRange) return "continue";

			["startRange", "inRange", "endRange", "notAllowed"].forEach(function (c) {
				dayElem.classList.remove(c);
			});

			var minRangeDate = Math.max(self.minRangeDate.getTime(), rangeStartDate),
			    maxRangeDate = Math.min(self.maxRangeDate.getTime(), rangeEndDate);

			elem.classList.add(hoverDate < self.selectedDates[0] ? "startRange" : "endRange");

			if (initialDate < hoverDate && timestamp === initialDate.getTime()) dayElem.classList.add("startRange");else if (initialDate > hoverDate && timestamp === initialDate.getTime()) dayElem.classList.add("endRange");

			if (timestamp >= minRangeDate && timestamp <= maxRangeDate) dayElem.classList.add("inRange");
		};

		for (var timestamp = self.days.childNodes[0].dateObj.getTime(), i = 0; i < 42; i++, timestamp += self.utils.duration.DAY) {
			var _ret = _loop(timestamp, i);

			if (_ret === "continue") continue;
		}
	}

	function onResize() {
		if (self.isOpen && !self.config.static && !self.config.inline) positionCalendar();
	}

	function open(e) {
		if (self.isMobile) {
			if (e) {
				e.preventDefault();
				e.target.blur();
			}

			setTimeout(function () {
				self.mobileInput.click();
			}, 0);

			triggerEvent("Open");
			return;
		}

		if (self.isOpen || self._input.disabled || self.config.inline) return;

		self.isOpen = true;
		self.calendarContainer.classList.add("open");
		positionCalendar();
		self._input.classList.add("active");

		triggerEvent("Open");
	}

	function minMaxDateSetter(type) {
		return function (date) {
			var dateObj = self.config["_" + type + "Date"] = self.parseDate(date);

			var inverseDateObj = self.config["_" + (type === "min" ? "max" : "min") + "Date"];
			var isValidDate = date && dateObj instanceof Date;

			if (isValidDate) {
				self[type + "DateHasTime"] = dateObj.getHours() || dateObj.getMinutes() || dateObj.getSeconds();
			}

			if (self.selectedDates) {
				self.selectedDates = self.selectedDates.filter(function (d) {
					return isEnabled(d);
				});
				if (!self.selectedDates.length && type === "min") setHoursFromDate(dateObj);
				updateValue();
			}

			if (self.daysContainer) {
				redraw();

				if (isValidDate) self.currentYearElement[type] = dateObj.getFullYear();else self.currentYearElement.removeAttribute(type);

				self.currentYearElement.disabled = inverseDateObj && dateObj && inverseDateObj.getFullYear() === dateObj.getFullYear();
			}
		};
	}

	function parseConfig() {
		var boolOpts = ["utc", "wrap", "weekNumbers", "allowInput", "clickOpens", "time_24hr", "enableTime", "noCalendar", "altInput", "shorthandCurrentMonth", "inline", "static", "enableSeconds", "disableMobile"];

		var hooks = ["onChange", "onClose", "onDayCreate", "onKeyDown", "onMonthChange", "onOpen", "onParseConfig", "onReady", "onValueUpdate", "onYearChange"];

		self.config = Object.create(Flatpickr.defaultConfig);

		var userConfig = _extends({}, self.instanceConfig, JSON.parse(JSON.stringify(self.element.dataset || {})));

		self.config.parseDate = userConfig.parseDate;
		self.config.formatDate = userConfig.formatDate;

		_extends(self.config, userConfig);

		if (!userConfig.dateFormat && userConfig.enableTime) {
			self.config.dateFormat = self.config.noCalendar ? "H:i" + (self.config.enableSeconds ? ":S" : "") : Flatpickr.defaultConfig.dateFormat + " H:i" + (self.config.enableSeconds ? ":S" : "");
		}

		if (userConfig.altInput && userConfig.enableTime && !userConfig.altFormat) {
			self.config.altFormat = self.config.noCalendar ? "h:i" + (self.config.enableSeconds ? ":S K" : " K") : Flatpickr.defaultConfig.altFormat + (" h:i" + (self.config.enableSeconds ? ":S" : "") + " K");
		}

		Object.defineProperty(self.config, "minDate", {
			get: function get() {
				return this._minDate;
			},
			set: minMaxDateSetter("min")
		});

		Object.defineProperty(self.config, "maxDate", {
			get: function get() {
				return this._maxDate;
			},
			set: minMaxDateSetter("max")
		});

		self.config.minDate = userConfig.minDate;
		self.config.maxDate = userConfig.maxDate;

		for (var i = 0; i < boolOpts.length; i++) {
			self.config[boolOpts[i]] = self.config[boolOpts[i]] === true || self.config[boolOpts[i]] === "true";
		}for (var _i = hooks.length; _i--;) {
			if (self.config[hooks[_i]] !== undefined) {
				self.config[hooks[_i]] = arrayify(self.config[hooks[_i]] || []).map(bindToInstance);
			}
		}

		for (var _i2 = 0; _i2 < self.config.plugins.length; _i2++) {
			var pluginConf = self.config.plugins[_i2](self) || {};
			for (var key in pluginConf) {

				if (self.config[key] instanceof Array || ~hooks.indexOf(key)) {
					self.config[key] = arrayify(pluginConf[key]).map(bindToInstance).concat(self.config[key]);
				} else if (typeof userConfig[key] === "undefined") self.config[key] = pluginConf[key];
			}
		}

		triggerEvent("ParseConfig");
	}

	function setupLocale() {
		if (_typeof(self.config.locale) !== "object" && typeof Flatpickr.l10ns[self.config.locale] === "undefined") console.warn("flatpickr: invalid locale " + self.config.locale);

		self.l10n = _extends(Object.create(Flatpickr.l10ns.default), _typeof(self.config.locale) === "object" ? self.config.locale : self.config.locale !== "default" ? Flatpickr.l10ns[self.config.locale] || {} : {});
	}

	function positionCalendar() {
		if (self.calendarContainer === undefined) return;

		var calendarHeight = self.calendarContainer.offsetHeight,
		    calendarWidth = self.calendarContainer.offsetWidth,
		    configPos = self.config.position,
		    inputBounds = self._positionElement.getBoundingClientRect(),
		    distanceFromBottom = window.innerHeight - inputBounds.bottom,
		    showOnTop = configPos === "above" || configPos !== "below" && distanceFromBottom < calendarHeight && inputBounds.top > calendarHeight;

		var top = window.pageYOffset + inputBounds.top + (!showOnTop ? self._positionElement.offsetHeight + 2 : -calendarHeight - 2);

		toggleClass(self.calendarContainer, "arrowTop", !showOnTop);
		toggleClass(self.calendarContainer, "arrowBottom", showOnTop);

		if (self.config.inline) return;

		var left = window.pageXOffset + inputBounds.left;
		var right = window.document.body.offsetWidth - inputBounds.right;
		var rightMost = left + calendarWidth > window.document.body.offsetWidth;

		toggleClass(self.calendarContainer, "rightMost", rightMost);

		if (self.config.static) return;

		self.calendarContainer.style.top = top + "px";

		if (!rightMost) {
			self.calendarContainer.style.left = left + "px";
			self.calendarContainer.style.right = "auto";
		} else {
			self.calendarContainer.style.left = "auto";
			self.calendarContainer.style.right = right + "px";
		}
	}

	function redraw() {
		if (self.config.noCalendar || self.isMobile) return;

		buildWeekdays();
		updateNavigationCurrentMonth();
		buildDays();
	}

	function selectDate(e) {
		e.preventDefault();
		e.stopPropagation();

		if (!e.target.classList.contains("flatpickr-day") || e.target.classList.contains("disabled") || e.target.classList.contains("notAllowed")) return;

		var selectedDate = self.latestSelectedDateObj = new Date(e.target.dateObj.getTime());

		var shouldChangeMonth = selectedDate.getMonth() !== self.currentMonth && self.config.mode !== "range";

		self.selectedDateElem = e.target;

		if (self.config.mode === "single") self.selectedDates = [selectedDate];else if (self.config.mode === "multiple") {
			var selectedIndex = isDateSelected(selectedDate);
			if (selectedIndex) self.selectedDates.splice(selectedIndex, 1);else self.selectedDates.push(selectedDate);
		} else if (self.config.mode === "range") {
			if (self.selectedDates.length === 2) self.clear();

			self.selectedDates.push(selectedDate);

			// unless selecting same date twice, sort ascendingly
			if (compareDates(selectedDate, self.selectedDates[0], true) !== 0) self.selectedDates.sort(function (a, b) {
				return a.getTime() - b.getTime();
			});
		}

		setHoursFromInputs();

		if (shouldChangeMonth) {
			var isNewYear = self.currentYear !== selectedDate.getFullYear();
			self.currentYear = selectedDate.getFullYear();
			self.currentMonth = selectedDate.getMonth();

			if (isNewYear) triggerEvent("YearChange");

			triggerEvent("MonthChange");
		}

		buildDays();

		if (self.minDateHasTime && self.config.enableTime && compareDates(selectedDate, self.config.minDate) === 0) setHoursFromDate(self.config.minDate);

		updateValue();

		if (self.config.enableTime) setTimeout(function () {
			return self.showTimeInput = true;
		}, 50);

		if (self.config.mode === "range") {
			if (self.selectedDates.length === 1) {
				onMouseOver(e.target);

				self._hidePrevMonthArrow = self._hidePrevMonthArrow || self.minRangeDate > self.days.childNodes[0].dateObj;

				self._hideNextMonthArrow = self._hideNextMonthArrow || self.maxRangeDate < new Date(self.currentYear, self.currentMonth + 1, 1);
			} else updateNavigationCurrentMonth();
		}

		triggerEvent("Change");

		// maintain focus
		if (!shouldChangeMonth) focusOnDay(e.target.$i, 0);else afterDayAnim(function () {
			return self.selectedDateElem.focus();
		});

		if (self.config.enableTime) setTimeout(function () {
			return self.hourElement.select();
		}, 451);

		if (self.config.closeOnSelect) {
			var single = self.config.mode === "single" && !self.config.enableTime;
			var range = self.config.mode === "range" && self.selectedDates.length === 2 && !self.config.enableTime;

			if (single || range) self.close();
		}
	}

	function set(option, value) {
		self.config[option] = value;
		self.redraw();
		jumpToDate();
	}

	function setSelectedDate(inputDate, format) {
		if (inputDate instanceof Array) self.selectedDates = inputDate.map(function (d) {
			return self.parseDate(d, format);
		});else if (inputDate instanceof Date || !isNaN(inputDate)) self.selectedDates = [self.parseDate(inputDate, format)];else if (inputDate && inputDate.substring) {
			switch (self.config.mode) {
				case "single":
					self.selectedDates = [self.parseDate(inputDate, format)];
					break;

				case "multiple":
					self.selectedDates = inputDate.split("; ").map(function (date) {
						return self.parseDate(date, format);
					});
					break;

				case "range":
					self.selectedDates = inputDate.split(self.l10n.rangeSeparator).map(function (date) {
						return self.parseDate(date, format);
					});

					break;

				default:
					break;
			}
		}

		self.selectedDates = self.selectedDates.filter(function (d) {
			return d instanceof Date && isEnabled(d, false);
		});

		self.selectedDates.sort(function (a, b) {
			return a.getTime() - b.getTime();
		});
	}

	function setDate(date, triggerChange, format) {
		if (!date) return self.clear(triggerChange);

		setSelectedDate(date, format);

		self.showTimeInput = self.selectedDates.length > 0;
		self.latestSelectedDateObj = self.selectedDates[0];

		self.redraw();
		jumpToDate();

		setHoursFromDate();
		updateValue(triggerChange);

		if (triggerChange) triggerEvent("Change");
	}

	function setupDates() {
		function parseDateRules(arr) {
			for (var i = arr.length; i--;) {
				if (typeof arr[i] === "string" || +arr[i]) arr[i] = self.parseDate(arr[i], null, true);else if (arr[i] && arr[i].from && arr[i].to) {
					arr[i].from = self.parseDate(arr[i].from);
					arr[i].to = self.parseDate(arr[i].to);
				}
			}

			return arr.filter(function (x) {
				return x;
			}); // remove falsy values
		}

		self.selectedDates = [];
		self.now = new Date();

		if (self.config.disable.length) self.config.disable = parseDateRules(self.config.disable);

		if (self.config.enable.length) self.config.enable = parseDateRules(self.config.enable);

		var preloadedDate = self.config.defaultDate || self.input.value;
		if (preloadedDate) setSelectedDate(preloadedDate, self.config.dateFormat);

		var initialDate = self.selectedDates.length ? self.selectedDates[0] : self.config.minDate && self.config.minDate.getTime() > self.now ? self.config.minDate : self.config.maxDate && self.config.maxDate.getTime() < self.now ? self.config.maxDate : self.now;

		self.currentYear = initialDate.getFullYear();
		self.currentMonth = initialDate.getMonth();

		if (self.selectedDates.length) self.latestSelectedDateObj = self.selectedDates[0];

		self.minDateHasTime = self.config.minDate && (self.config.minDate.getHours() || self.config.minDate.getMinutes() || self.config.minDate.getSeconds());

		self.maxDateHasTime = self.config.maxDate && (self.config.maxDate.getHours() || self.config.maxDate.getMinutes() || self.config.maxDate.getSeconds());

		Object.defineProperty(self, "latestSelectedDateObj", {
			get: function get() {
				return self._selectedDateObj || self.selectedDates[self.selectedDates.length - 1];
			},
			set: function set(date) {
				self._selectedDateObj = date;
			}
		});

		if (!self.isMobile) {
			Object.defineProperty(self, "showTimeInput", {
				get: function get() {
					return self._showTimeInput;
				},
				set: function set(bool) {
					self._showTimeInput = bool;
					if (self.calendarContainer) toggleClass(self.calendarContainer, "showTimeInput", bool);
					positionCalendar();
				}
			});
		}
	}

	function setupHelperFunctions() {
		self.utils = {
			duration: {
				DAY: 86400000
			},
			getDaysinMonth: function getDaysinMonth(month, yr) {
				month = typeof month === "undefined" ? self.currentMonth : month;

				yr = typeof yr === "undefined" ? self.currentYear : yr;

				if (month === 1 && (yr % 4 === 0 && yr % 100 !== 0 || yr % 400 === 0)) return 29;

				return self.l10n.daysInMonth[month];
			},
			monthToStr: function monthToStr(monthNumber, shorthand) {
				shorthand = typeof shorthand === "undefined" ? self.config.shorthandCurrentMonth : shorthand;

				return self.l10n.months[(shorthand ? "short" : "long") + "hand"][monthNumber];
			}
		};
	}

	/* istanbul ignore next */
	function setupFormats() {
		["D", "F", "J", "M", "W", "l"].forEach(function (f) {
			self.formats[f] = Flatpickr.prototype.formats[f].bind(self);
		});

		self.revFormat.F = Flatpickr.prototype.revFormat.F.bind(self);
		self.revFormat.M = Flatpickr.prototype.revFormat.M.bind(self);
	}

	function setupInputs() {
		self.input = self.config.wrap ? self.element.querySelector("[data-input]") : self.element;

		/* istanbul ignore next */
		if (!self.input) return console.warn("Error: invalid input element specified", self.input);

		self.input._type = self.input.type;
		self.input.type = "text";

		self.input.classList.add("flatpickr-input");
		self._input = self.input;

		if (self.config.altInput) {
			// replicate self.element
			self.altInput = createElement(self.input.nodeName, self.input.className + " " + self.config.altInputClass);
			self._input = self.altInput;
			self.altInput.placeholder = self.input.placeholder;
			self.altInput.disabled = self.input.disabled;
			self.altInput.type = "text";
			self.input.type = "hidden";

			if (!self.config.static && self.input.parentNode) self.input.parentNode.insertBefore(self.altInput, self.input.nextSibling);
		}

		if (!self.config.allowInput) self._input.setAttribute("readonly", "readonly");

		self._positionElement = self.config.positionElement || self._input;
	}

	function setupMobile() {
		var inputType = self.config.enableTime ? self.config.noCalendar ? "time" : "datetime-local" : "date";

		self.mobileInput = createElement("input", self.input.className + " flatpickr-mobile");
		self.mobileInput.step = "any";
		self.mobileInput.tabIndex = 1;
		self.mobileInput.type = inputType;
		self.mobileInput.disabled = self.input.disabled;
		self.mobileInput.placeholder = self.input.placeholder;

		self.mobileFormatStr = inputType === "datetime-local" ? "Y-m-d\\TH:i:S" : inputType === "date" ? "Y-m-d" : "H:i:S";

		if (self.selectedDates.length) {
			self.mobileInput.defaultValue = self.mobileInput.value = self.formatDate(self.selectedDates[0], self.mobileFormatStr);
		}

		if (self.config.minDate) self.mobileInput.min = self.formatDate(self.config.minDate, "Y-m-d");

		if (self.config.maxDate) self.mobileInput.max = self.formatDate(self.config.maxDate, "Y-m-d");

		self.input.type = "hidden";
		if (self.config.altInput) self.altInput.type = "hidden";

		try {
			self.input.parentNode.insertBefore(self.mobileInput, self.input.nextSibling);
		} catch (e) {
			//
		}

		self.mobileInput.addEventListener("change", function (e) {
			self.setDate(e.target.value, false, self.mobileFormatStr);
			triggerEvent("Change");
			triggerEvent("Close");
		});
	}

	function toggle() {
		if (self.isOpen) return self.close();
		self.open();
	}

	function triggerEvent(event, data) {
		var hooks = self.config["on" + event];

		if (hooks !== undefined && hooks.length > 0) {
			for (var i = 0; hooks[i] && i < hooks.length; i++) {
				hooks[i](self.selectedDates, self.input.value, self, data);
			}
		}

		if (event === "Change") {
			self.input.dispatchEvent(createEvent("change"));

			// many front-end frameworks bind to the input event
			self.input.dispatchEvent(createEvent("input"));
		}
	}

	/**
  * Creates an Event, normalized across browsers
  * @param {String} name the event name, e.g. "click"
  * @return {Event} the created event
  */
	function createEvent(name) {
		if (self._supportsEvents) return new Event(name, { bubbles: true });

		self._[name + "Event"] = document.createEvent("Event");
		self._[name + "Event"].initEvent(name, true, true);
		return self._[name + "Event"];
	}

	function isDateSelected(date) {
		for (var i = 0; i < self.selectedDates.length; i++) {
			if (compareDates(self.selectedDates[i], date) === 0) return "" + i;
		}

		return false;
	}

	function isDateInRange(date) {
		if (self.config.mode !== "range" || self.selectedDates.length < 2) return false;
		return compareDates(date, self.selectedDates[0]) >= 0 && compareDates(date, self.selectedDates[1]) <= 0;
	}

	function updateNavigationCurrentMonth() {
		if (self.config.noCalendar || self.isMobile || !self.monthNav) return;

		self.currentMonthElement.textContent = self.utils.monthToStr(self.currentMonth) + " ";
		self.currentYearElement.value = self.currentYear;

		self._hidePrevMonthArrow = self.config.minDate && (self.currentYear === self.config.minDate.getFullYear() ? self.currentMonth <= self.config.minDate.getMonth() : self.currentYear < self.config.minDate.getFullYear());

		self._hideNextMonthArrow = self.config.maxDate && (self.currentYear === self.config.maxDate.getFullYear() ? self.currentMonth + 1 > self.config.maxDate.getMonth() : self.currentYear > self.config.maxDate.getFullYear());
	}

	/**
  * Updates the values of inputs associated with the calendar
  * @return {void}
  */
	function updateValue(triggerChange) {
		if (!self.selectedDates.length) return self.clear(triggerChange);

		if (self.isMobile) {
			self.mobileInput.value = self.selectedDates.length ? self.formatDate(self.latestSelectedDateObj, self.mobileFormatStr) : "";
		}

		var joinChar = self.config.mode !== "range" ? "; " : self.l10n.rangeSeparator;

		self.input.value = self.selectedDates.map(function (dObj) {
			return self.formatDate(dObj, self.config.dateFormat);
		}).join(joinChar);

		if (self.config.altInput) {
			self.altInput.value = self.selectedDates.map(function (dObj) {
				return self.formatDate(dObj, self.config.altFormat);
			}).join(joinChar);
		}
		triggerEvent("ValueUpdate");
	}

	function mouseDelta(e) {
		return Math.max(-1, Math.min(1, e.wheelDelta || -e.deltaY));
	}

	function onMonthNavScroll(e) {
		e.preventDefault();
		var isYear = self.currentYearElement.parentNode.contains(e.target);

		if (e.target === self.currentMonthElement || isYear) {

			var delta = mouseDelta(e);

			if (isYear) {
				changeYear(self.currentYear + delta);
				e.target.value = self.currentYear;
			} else self.changeMonth(delta, true, false);
		}
	}

	function onMonthNavClick(e) {
		var isPrevMonth = self.prevMonthNav.contains(e.target);
		var isNextMonth = self.nextMonthNav.contains(e.target);

		if (isPrevMonth || isNextMonth) changeMonth(isPrevMonth ? -1 : 1);else if (e.target === self.currentYearElement) {
			e.preventDefault();
			self.currentYearElement.select();
		} else if (e.target.className === "arrowUp") self.changeYear(self.currentYear + 1);else if (e.target.className === "arrowDown") self.changeYear(self.currentYear - 1);
	}

	/**
  * Creates an HTMLElement with given tag, class, and textual content
  * @param {String} tag the HTML tag
  * @param {String} className the new element's class name
  * @param {String} content The new element's text content
  * @return {HTMLElement} the created HTML element
  */
	function createElement(tag, className, content) {
		var e = window.document.createElement(tag);
		className = className || "";
		content = content || "";

		e.className = className;

		if (content !== undefined) e.textContent = content;

		return e;
	}

	function arrayify(obj) {
		if (obj instanceof Array) return obj;
		return [obj];
	}

	function toggleClass(elem, className, bool) {
		if (bool) return elem.classList.add(className);
		elem.classList.remove(className);
	}

	/* istanbul ignore next */
	function debounce(func, wait, immediate) {
		var timeout = void 0;
		return function () {
			var context = this,
			    args = arguments;
			clearTimeout(timeout);
			timeout = setTimeout(function () {
				timeout = null;
				if (!immediate) func.apply(context, args);
			}, wait);
			if (immediate && !timeout) func.apply(context, args);
		};
	}

	/**
  * Compute the difference in dates, measured in ms
  * @param {Date} date1
  * @param {Date} date2
  * @param {Boolean} timeless whether to reset times of both dates to 00:00
  * @return {Number} the difference in ms
  */
	function compareDates(date1, date2, timeless) {
		if (!(date1 instanceof Date) || !(date2 instanceof Date)) return false;

		if (timeless !== false) {
			return new Date(date1.getTime()).setHours(0, 0, 0, 0) - new Date(date2.getTime()).setHours(0, 0, 0, 0);
		}

		return date1.getTime() - date2.getTime();
	}

	function timeWrapper(e) {
		e.preventDefault();

		var isKeyDown = e.type === "keydown",
		    isWheel = e.type === "wheel",
		    isIncrement = e.type === "increment",
		    input = e.target;

		if (self.amPM && e.target === self.amPM) return e.target.textContent = ["AM", "PM"][e.target.textContent === "AM" | 0];

		var min = Number(input.min),
		    max = Number(input.max),
		    step = Number(input.step),
		    curValue = parseInt(input.value, 10),
		    delta = e.delta || (!isKeyDown ? Math.max(-1, Math.min(1, e.wheelDelta || -e.deltaY)) || 0 : e.which === 38 ? 1 : -1);

		var newValue = curValue + step * delta;

		if (typeof input.value !== "undefined" && input.value.length === 2) {
			var isHourElem = input === self.hourElement,
			    isMinuteElem = input === self.minuteElement;

			if (newValue < min) {
				newValue = max + newValue + !isHourElem + (isHourElem && !self.amPM);

				if (isMinuteElem) incrementNumInput(null, -1, self.hourElement);
			} else if (newValue > max) {
				newValue = input === self.hourElement ? newValue - max - !self.amPM : min;

				if (isMinuteElem) incrementNumInput(null, 1, self.hourElement);
			}

			if (self.amPM && isHourElem && (step === 1 ? newValue + curValue === 23 : Math.abs(newValue - curValue) > step)) self.amPM.textContent = self.amPM.textContent === "PM" ? "AM" : "PM";

			input.value = self.pad(newValue);
		}
	}

	init();
	return self;
}

/* istanbul ignore next */
Flatpickr.defaultConfig = {
	mode: "single",

	position: "auto",

	animate: window.navigator.userAgent.indexOf("MSIE") === -1,

	/* if true, dates will be parsed, formatted, and displayed in UTC.
 preloading date strings w/ timezones is recommended but not necessary */
	utc: false,

	// wrap: see https://chmln.github.io/flatpickr/examples/#flatpickr-external-elements
	wrap: false,

	// enables week numbers
	weekNumbers: false,

	// allow manual datetime input
	allowInput: false,

	/*
 	clicking on input opens the date(time)picker.
 	disable if you wish to open the calendar manually with .open()
 */
	clickOpens: true,

	/*
 	closes calendar after date selection,
 	unless 'mode' is 'multiple' or enableTime is true
 */
	closeOnSelect: true,

	// display time picker in 24 hour mode
	time_24hr: false,

	// enables the time picker functionality
	enableTime: false,

	// noCalendar: true will hide the calendar. use for a time picker along w/ enableTime
	noCalendar: false,

	// more date format chars at https://chmln.github.io/flatpickr/#dateformat
	dateFormat: "Y-m-d",

	// date format used in aria-label for days
	ariaDateFormat: "F j, Y",

	// altInput - see https://chmln.github.io/flatpickr/#altinput
	altInput: false,

	// the created altInput element will have this class.
	altInputClass: "form-control input",

	// same as dateFormat, but for altInput
	altFormat: "F j, Y", // defaults to e.g. June 10, 2016

	// defaultDate - either a datestring or a date object. used for datetimepicker"s initial value
	defaultDate: null,

	// the minimum date that user can pick (inclusive)
	minDate: null,

	// the maximum date that user can pick (inclusive)
	maxDate: null,

	// dateparser that transforms a given string to a date object
	parseDate: null,

	// dateformatter that transforms a given date object to a string, according to passed format
	formatDate: null,

	getWeek: function getWeek(givenDate) {
		var date = new Date(givenDate.getTime());
		var onejan = new Date(date.getFullYear(), 0, 1);
		return Math.ceil(((date - onejan) / 86400000 + onejan.getDay() + 1) / 7);
	},


	// see https://chmln.github.io/flatpickr/#disable
	enable: [],

	// see https://chmln.github.io/flatpickr/#disable
	disable: [],

	// display the short version of month names - e.g. Sep instead of September
	shorthandCurrentMonth: false,

	// displays calendar inline. see https://chmln.github.io/flatpickr/#inline-calendar
	inline: false,

	// position calendar inside wrapper and next to the input element
	// leave at false unless you know what you"re doing
	"static": false,

	// DOM node to append the calendar to in *static* mode
	appendTo: null,

	// code for previous/next icons. this is where you put your custom icon code e.g. fontawesome
	prevArrow: "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 17 17'><g></g><path d='M5.207 8.471l7.146 7.147-0.707 0.707-7.853-7.854 7.854-7.853 0.707 0.707-7.147 7.146z' /></svg>",
	nextArrow: "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' viewBox='0 0 17 17'><g></g><path d='M13.207 8.472l-7.854 7.854-0.707-0.707 7.146-7.146-7.146-7.148 0.707-0.707 7.854 7.854z' /></svg>",

	// enables seconds in the time picker
	enableSeconds: false,

	// step size used when scrolling/incrementing the hour element
	hourIncrement: 1,

	// step size used when scrolling/incrementing the minute element
	minuteIncrement: 5,

	// initial value in the hour element
	defaultHour: 12,

	// initial value in the minute element
	defaultMinute: 0,

	// disable native mobile datetime input support
	disableMobile: false,

	// default locale
	locale: "default",

	plugins: [],

	// called every time calendar is closed
	onClose: undefined, // function (dateObj, dateStr) {}

	// onChange callback when user selects a date or time
	onChange: undefined, // function (dateObj, dateStr) {}

	// called for every day element
	onDayCreate: undefined,

	// called every time the month is changed
	onMonthChange: undefined,

	// called every time calendar is opened
	onOpen: undefined, // function (dateObj, dateStr) {}

	// called after the configuration has been parsed
	onParseConfig: undefined,

	// called after calendar is ready
	onReady: undefined, // function (dateObj, dateStr) {}

	// called after input value updated
	onValueUpdate: undefined,

	// called every time the year is changed
	onYearChange: undefined,

	onKeyDown: undefined
};

/* istanbul ignore next */
Flatpickr.l10ns = {
	en: {
		weekdays: {
			shorthand: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
			longhand: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
		},
		months: {
			shorthand: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
			longhand: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]
		},
		daysInMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
		firstDayOfWeek: 0,
		ordinal: function ordinal(nth) {
			var s = nth % 100;
			if (s > 3 && s < 21) return "th";
			switch (s % 10) {
				case 1:
					return "st";
				case 2:
					return "nd";
				case 3:
					return "rd";
				default:
					return "th";
			}
		},
		rangeSeparator: " to ",
		weekAbbreviation: "Wk",
		scrollTitle: "Scroll to increment",
		toggleTitle: "Click to toggle"
	}
};

Flatpickr.l10ns.default = Object.create(Flatpickr.l10ns.en);
Flatpickr.localize = function (l10n) {
	return _extends(Flatpickr.l10ns.default, l10n || {});
};
Flatpickr.setDefaults = function (config) {
	return _extends(Flatpickr.defaultConfig, config || {});
};

Flatpickr.prototype = {
	formats: {
		// get the date in UTC
		Z: function Z(date) {
			return date.toISOString();
		},

		// weekday name, short, e.g. Thu
		D: function D(date) {
			return this.l10n.weekdays.shorthand[this.formats.w(date)];
		},

		// full month name e.g. January
		F: function F(date) {
			return this.utils.monthToStr(this.formats.n(date) - 1, false);
		},

		// padded hour 1-12
		G: function G(date) {
			return Flatpickr.prototype.pad(Flatpickr.prototype.formats.h(date));
		},

		// hours with leading zero e.g. 03
		H: function H(date) {
			return Flatpickr.prototype.pad(date.getHours());
		},

		// day (1-30) with ordinal suffix e.g. 1st, 2nd
		J: function J(date) {
			return date.getDate() + this.l10n.ordinal(date.getDate());
		},

		// AM/PM
		K: function K(date) {
			return date.getHours() > 11 ? "PM" : "AM";
		},

		// shorthand month e.g. Jan, Sep, Oct, etc
		M: function M(date) {
			return this.utils.monthToStr(date.getMonth(), true);
		},

		// seconds 00-59
		S: function S(date) {
			return Flatpickr.prototype.pad(date.getSeconds());
		},

		// unix timestamp
		U: function U(date) {
			return date.getTime() / 1000;
		},

		W: function W(date) {
			return this.config.getWeek(date);
		},

		// full year e.g. 2016
		Y: function Y(date) {
			return date.getFullYear();
		},

		// day in month, padded (01-30)
		d: function d(date) {
			return Flatpickr.prototype.pad(date.getDate());
		},

		// hour from 1-12 (am/pm)
		h: function h(date) {
			return date.getHours() % 12 ? date.getHours() % 12 : 12;
		},

		// minutes, padded with leading zero e.g. 09
		i: function i(date) {
			return Flatpickr.prototype.pad(date.getMinutes());
		},

		// day in month (1-30)
		j: function j(date) {
			return date.getDate();
		},

		// weekday name, full, e.g. Thursday
		l: function l(date) {
			return this.l10n.weekdays.longhand[date.getDay()];
		},

		// padded month number (01-12)
		m: function m(date) {
			return Flatpickr.prototype.pad(date.getMonth() + 1);
		},

		// the month number (1-12)
		n: function n(date) {
			return date.getMonth() + 1;
		},

		// seconds 0-59
		s: function s(date) {
			return date.getSeconds();
		},

		// number of the day of the week
		w: function w(date) {
			return date.getDay();
		},

		// last two digits of year e.g. 16 for 2016
		y: function y(date) {
			return String(date.getFullYear()).substring(2);
		}
	},

	/**
  * Formats a given Date object into a string based on supplied format
  * @param {Date} dateObj the date object
  * @param {String} frmt a string composed of formatting tokens e.g. "Y-m-d"
  * @return {String} The textual representation of the date e.g. 2017-02-03
  */
	formatDate: function formatDate(dateObj, frmt) {
		var _this = this;

		if (this.config !== undefined && this.config.formatDate !== undefined) return this.config.formatDate(dateObj, frmt);

		return frmt.split("").map(function (c, i, arr) {
			return _this.formats[c] && arr[i - 1] !== "\\" ? _this.formats[c](dateObj) : c !== "\\" ? c : "";
		}).join("");
	},


	revFormat: {
		D: function D() {},
		F: function F(dateObj, monthName) {
			dateObj.setMonth(this.l10n.months.longhand.indexOf(monthName));
		},
		G: function G(dateObj, hour) {
			dateObj.setHours(parseFloat(hour));
		},
		H: function H(dateObj, hour) {
			dateObj.setHours(parseFloat(hour));
		},
		J: function J(dateObj, day) {
			dateObj.setDate(parseFloat(day));
		},
		K: function K(dateObj, amPM) {
			var hours = dateObj.getHours();

			if (hours !== 12) dateObj.setHours(hours % 12 + 12 * /pm/i.test(amPM));
		},
		M: function M(dateObj, shortMonth) {
			dateObj.setMonth(this.l10n.months.shorthand.indexOf(shortMonth));
		},
		S: function S(dateObj, seconds) {
			dateObj.setSeconds(seconds);
		},
		U: function U(dateObj, unixSeconds) {
			return new Date(parseFloat(unixSeconds) * 1000);
		},

		W: function W(dateObj, weekNumber) {
			weekNumber = parseInt(weekNumber);
			return new Date(dateObj.getFullYear(), 0, 2 + (weekNumber - 1) * 7, 0, 0, 0, 0, 0);
		},
		Y: function Y(dateObj, year) {
			dateObj.setFullYear(year);
		},
		Z: function Z(dateObj, ISODate) {
			return new Date(ISODate);
		},

		d: function d(dateObj, day) {
			dateObj.setDate(parseFloat(day));
		},
		h: function h(dateObj, hour) {
			dateObj.setHours(parseFloat(hour));
		},
		i: function i(dateObj, minutes) {
			dateObj.setMinutes(parseFloat(minutes));
		},
		j: function j(dateObj, day) {
			dateObj.setDate(parseFloat(day));
		},
		l: function l() {},
		m: function m(dateObj, month) {
			dateObj.setMonth(parseFloat(month) - 1);
		},
		n: function n(dateObj, month) {
			dateObj.setMonth(parseFloat(month) - 1);
		},
		s: function s(dateObj, seconds) {
			dateObj.setSeconds(parseFloat(seconds));
		},
		w: function w() {},
		y: function y(dateObj, year) {
			dateObj.setFullYear(2000 + parseFloat(year));
		}
	},

	tokenRegex: {
		D: "(\\w+)",
		F: "(\\w+)",
		G: "(\\d\\d|\\d)",
		H: "(\\d\\d|\\d)",
		J: "(\\d\\d|\\d)\\w+",
		K: "(\\w+)",
		M: "(\\w+)",
		S: "(\\d\\d|\\d)",
		U: "(.+)",
		W: "(\\d\\d|\\d)",
		Y: "(\\d{4})",
		Z: "(.+)",
		d: "(\\d\\d|\\d)",
		h: "(\\d\\d|\\d)",
		i: "(\\d\\d|\\d)",
		j: "(\\d\\d|\\d)",
		l: "(\\w+)",
		m: "(\\d\\d|\\d)",
		n: "(\\d\\d|\\d)",
		s: "(\\d\\d|\\d)",
		w: "(\\d\\d|\\d)",
		y: "(\\d{2})"
	},

	pad: function pad(number) {
		return ("0" + number).slice(-2);
	},

	/**
  * Parses a date(+time) string into a Date object
  * @param {String} date the date string, e.g. 2017-02-03 14:45
  * @param {String} givenFormat the date format, e.g. Y-m-d H:i
  * @param {Boolean} timeless whether to reset the time of Date object
  * @return {Date} the parsed Date object
  */
	parseDate: function parseDate(date, givenFormat, timeless) {
		if (!date) return null;

		var date_orig = date;

		if (date instanceof Date) {
			date = new Date(date.getTime()); // create a copy
			date.fp_isUTC = date_orig.fp_isUTC;
		} else if (date.toFixed !== undefined) // timestamp
			date = new Date(date);else {
			// date string
			var format = givenFormat || (this.config || Flatpickr.defaultConfig).dateFormat;
			date = String(date).trim();

			if (date === "today") {
				date = new Date();
				timeless = true;
			} else if (/Z$/.test(date) || /GMT$/.test(date)) // datestrings w/ timezone
				date = new Date(date);else if (this.config && this.config.parseDate) date = this.config.parseDate(date, format);else {
				var parsedDate = !this.config || !this.config.noCalendar ? new Date(new Date().getFullYear(), 0, 1, 0, 0, 0, 0) : new Date(new Date().setHours(0, 0, 0, 0));

				var matched = void 0;

				for (var i = 0, matchIndex = 0, regexStr = ""; i < format.length; i++) {
					var token = format[i];
					var isBackSlash = token === "\\";
					var escaped = format[i - 1] === "\\" || isBackSlash;

					if (this.tokenRegex[token] && !escaped) {
						regexStr += this.tokenRegex[token];
						var match = new RegExp(regexStr).exec(date);
						if (match && (matched = true)) {
							parsedDate = this.revFormat[token](parsedDate, match[++matchIndex]) || parsedDate;
						}
					} else if (!isBackSlash) regexStr += "."; // don't really care
				}

				date = matched ? parsedDate : null;
			}
		}

		/* istanbul ignore next */
		if (!(date instanceof Date)) {
			console.warn("flatpickr: invalid date " + date_orig);
			console.info(this.element);
			return null;
		}

		if (this.config && this.config.utc && !date.fp_isUTC) date = date.fp_toUTC();

		if (timeless === true) date.setHours(0, 0, 0, 0);

		return date;
	}
};

/* istanbul ignore next */
function _flatpickr(nodeList, config) {
	var nodes = Array.prototype.slice.call(nodeList); // static list
	var instances = [];
	for (var i = 0; i < nodes.length; i++) {
		try {
			nodes[i]._flatpickr = new Flatpickr(nodes[i], config || {});
			instances.push(nodes[i]._flatpickr);
		} catch (e) {
			console.warn(e, e.stack);
		}
	}

	return instances.length === 1 ? instances[0] : instances;
}

/* istanbul ignore next */
if (typeof HTMLElement !== "undefined") {
	// browser env
	HTMLCollection.prototype.flatpickr = NodeList.prototype.flatpickr = function (config) {
		return _flatpickr(this, config);
	};

	HTMLElement.prototype.flatpickr = function (config) {
		return _flatpickr([this], config);
	};
}

/* istanbul ignore next */
function flatpickr(selector, config) {
	return _flatpickr(window.document.querySelectorAll(selector), config);
}

/* istanbul ignore next */
if (typeof jQuery !== "undefined") {
	jQuery.fn.flatpickr = function (config) {
		return _flatpickr(this, config);
	};
}

Date.prototype.fp_incr = function (days) {
	return new Date(this.getFullYear(), this.getMonth(), this.getDate() + parseInt(days, 10));
};

Date.prototype.fp_isUTC = false;
Date.prototype.fp_toUTC = function () {
	var newDate = new Date(this.getUTCFullYear(), this.getUTCMonth(), this.getUTCDate(), this.getUTCHours(), this.getUTCMinutes(), this.getUTCSeconds());

	newDate.fp_isUTC = true;
	return newDate;
};

if (typeof module !== "undefined") module.exports = Flatpickr;
},{}],41:[function(require,module,exports){
var Vue // late bind
var version
var map = window.__VUE_HOT_MAP__ = Object.create(null)
var installed = false
var isBrowserify = false
var initHookName = 'beforeCreate'

exports.install = function (vue, browserify) {
  if (installed) return
  installed = true

  Vue = vue.__esModule ? vue.default : vue
  version = Vue.version.split('.').map(Number)
  isBrowserify = browserify

  // compat with < 2.0.0-alpha.7
  if (Vue.config._lifecycleHooks.indexOf('init') > -1) {
    initHookName = 'init'
  }

  exports.compatible = version[0] >= 2
  if (!exports.compatible) {
    console.warn(
      '[HMR] You are using a version of vue-hot-reload-api that is ' +
      'only compatible with Vue.js core ^2.0.0.'
    )
    return
  }
}

/**
 * Create a record for a hot module, which keeps track of its constructor
 * and instances
 *
 * @param {String} id
 * @param {Object} options
 */

exports.createRecord = function (id, options) {
  var Ctor = null
  if (typeof options === 'function') {
    Ctor = options
    options = Ctor.options
  }
  makeOptionsHot(id, options)
  map[id] = {
    Ctor: Vue.extend(options),
    instances: []
  }
}

/**
 * Make a Component options object hot.
 *
 * @param {String} id
 * @param {Object} options
 */

function makeOptionsHot (id, options) {
  injectHook(options, initHookName, function () {
    map[id].instances.push(this)
  })
  injectHook(options, 'beforeDestroy', function () {
    var instances = map[id].instances
    instances.splice(instances.indexOf(this), 1)
  })
}

/**
 * Inject a hook to a hot reloadable component so that
 * we can keep track of it.
 *
 * @param {Object} options
 * @param {String} name
 * @param {Function} hook
 */

function injectHook (options, name, hook) {
  var existing = options[name]
  options[name] = existing
    ? Array.isArray(existing)
      ? existing.concat(hook)
      : [existing, hook]
    : [hook]
}

function tryWrap (fn) {
  return function (id, arg) {
    try { fn(id, arg) } catch (e) {
      console.error(e)
      console.warn('Something went wrong during Vue component hot-reload. Full reload required.')
    }
  }
}

exports.rerender = tryWrap(function (id, options) {
  var record = map[id]
  if (!options) {
    record.instances.slice().forEach(function (instance) {
      instance.$forceUpdate()
    })
    return
  }
  if (typeof options === 'function') {
    options = options.options
  }
  record.Ctor.options.render = options.render
  record.Ctor.options.staticRenderFns = options.staticRenderFns
  record.instances.slice().forEach(function (instance) {
    instance.$options.render = options.render
    instance.$options.staticRenderFns = options.staticRenderFns
    instance._staticTrees = [] // reset static trees
    instance.$forceUpdate()
  })
})

exports.reload = tryWrap(function (id, options) {
  var record = map[id]
  if (options) {
    if (typeof options === 'function') {
      options = options.options
    }
    makeOptionsHot(id, options)
    if (version[1] < 2) {
      // preserve pre 2.2 behavior for global mixin handling
      record.Ctor.extendOptions = options
    }
    var newCtor = record.Ctor.super.extend(options)
    record.Ctor.options = newCtor.options
    record.Ctor.cid = newCtor.cid
    record.Ctor.prototype = newCtor.prototype
    if (newCtor.release) {
      // temporary global mixin strategy used in < 2.0.0-alpha.6
      newCtor.release()
    }
  }
  record.instances.slice().forEach(function (instance) {
    if (instance.$vnode && instance.$vnode.context) {
      instance.$vnode.context.$forceUpdate()
    } else {
      console.warn('Root or manually mounted instance modified. Full reload required.')
    }
  })
})

},{}],42:[function(require,module,exports){
(function (global){
;(function(){
'use strict';

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _assign = require('babel-runtime/core-js/object/assign');

var _assign2 = _interopRequireDefault(_assign);

var _stringify = require('babel-runtime/core-js/json/stringify');

var _stringify2 = _interopRequireDefault(_stringify);

var _flatpickr = require('flatpickr');

var _flatpickr2 = _interopRequireDefault(_flatpickr);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

exports.default = {
    props: {
        inputClass: {
            type: String
        },
        placeholder: {
            type: String,
            default: ''
        },
        options: {
            type: Object,
            default: function _default() {
                return {};
            }
        },
        value: {
            type: String,
            default: ''
        }
    },
    data: function data() {
        return {
            fp: null
        };
    },

    computed: {
        fpOptions: function fpOptions() {
            return (0, _stringify2.default)(this.options);
        }
    },
    watch: {
        fpOptions: function fpOptions(newOpt) {
            var option = JSON.parse(newOpt);
            for (var o in option) {
                this.fp.set(o, option[o]);
            }
        }
    },
    mounted: function mounted() {
        var self = this;
        var origOnValUpdate = this.options.onValueUpdate;
        var mergedOptions = (0, _assign2.default)(this.options, {
            wrap: true,
            onValueUpdate: function onValueUpdate() {
                self.onInput(self.$el.querySelector('input').value);
                if (typeof origOnValUpdate === 'function') {
                    origOnValUpdate();
                }
            }
        });

        this.fp = new _flatpickr2.default(this.$el, mergedOptions);
        this.$emit('FlatpickrRef', this.fp);
    },
    destroyed: function destroyed() {
        this.fp.destroy();
        this.fp = null;
    },

    methods: {
        onInput: function onInput(e) {
            var selectedDates = this.fp.selectedDates || [];
            var left = selectedDates.length > 0 ? selectedDates[0] : null;
            var right = selectedDates.length > 1 ? selectedDates[1] : null;
            this.$emit('input', typeof e === 'string' ? e : e.target.value, left, right);

            if (right == null) {
                this.$el.classList.remove("answered");
            } else {
                this.$el.classList.add("answered");
            }
        }
    }
};
})()
if (module.exports.__esModule) module.exports = module.exports.default
var __vue__options__ = (typeof module.exports === "function"? module.exports.options: module.exports)
if (__vue__options__.functional) {console.error("[vueify] functional components are not supported and should be defined in plain js files using render functions.")}
__vue__options__.render = function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('div',{staticClass:"form-date input-group"},[_c('input',{class:_vm.inputClass,attrs:{"type":"text","placeholder":_vm.placeholder,"data-input":""},domProps:{"value":_vm.value},on:{"input":_vm.onInput}}),_vm._v(" "),_vm._m(0),_vm._v(" "),_vm._m(1)])}
__vue__options__.staticRenderFns = [function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('button',{staticClass:"btn btn-link btn-clear",attrs:{"type":"submit","data-clear":""}},[_c('span')])},function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('span',{staticClass:"input-group-addon",attrs:{"data-toggle":""}},[_c('span',{staticClass:"calendar"})])}]
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-50389b9e", __vue__options__)
  } else {
    hotAPI.reload("data-v-50389b9e", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/json/stringify":1,"babel-runtime/core-js/object/assign":2,"flatpickr":40,"vue-hot-reload-api":41}],43:[function(require,module,exports){
(function (global){
;(function(){
'use strict';

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _assign = require('babel-runtime/core-js/object/assign');

var _assign2 = _interopRequireDefault(_assign);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

exports.default = {
    props: {
        addParamsToRequest: {
            type: Function,
            default: function _default(d) {
                return d;
            }
        },
        responseProcessor: {
            type: Function,
            default: function _default(r) {
                return r;
            }
        },
        tableOptions: {
            type: Object,
            default: function _default() {
                return {};
            }
        }
    },
    data: function data() {
        return {
            table: null
        };
    },

    computed: {},
    watch: {},
    methods: {
        reload: function reload(data) {
            this.table.ajax.data = data;
            this.table.ajax.reload();
        },
        onTableInitComplete: function onTableInitComplete() {
            $(this.$el).parent('.dataTables_wrapper').find('.dataTables_filter label').on('click', function (e) {
                if (e.target !== this) return;
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                } else {
                    $(this).addClass("active");
                }
            });
        }
    },
    mounted: function mounted() {
        var self = this;
        var options = (0, _assign2.default)({
            processing: true,
            serverSide: true,
            language: {
                "url": window.input.settings.config.dataTableTranslationsUrl
            },
            searchHighlight: true,
            pagingType: "full_numbers",
            lengthChange: false,
            pageLength: 10,
            dom: "frtp",
            conditionalPaging: true
        }, this.tableOptions);

        options.ajax.data = function (d) {
            self.addParamsToRequest(d);
        };

        options.ajax.complete = function (response) {
            self.responseProcessor(response.responseJSON);
        };

        this.table = $(this.$el).DataTable(options);
        this.table.on('init.dt', this.onTableInitComplete);
        this.$emit('DataTableRef', this.table);
    },
    destroyed: function destroyed() {}
};
})()
if (module.exports.__esModule) module.exports = module.exports.default
var __vue__options__ = (typeof module.exports === "function"? module.exports.options: module.exports)
if (__vue__options__.functional) {console.error("[vueify] functional components are not supported and should be defined in plain js files using render functions.")}
__vue__options__.render = function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _vm._m(0)}
__vue__options__.staticRenderFns = [function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('table',{staticClass:"table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews"},[_c('thead'),_vm._v(" "),_c('tbody')])}]
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-07900422", __vue__options__)
  } else {
    hotAPI.reload("data-v-07900422", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/object/assign":2,"vue-hot-reload-api":41}],44:[function(require,module,exports){
(function (global){
;(function(){
'use strict';

var _assign = require('babel-runtime/core-js/object/assign');

var _assign2 = _interopRequireDefault(_assign);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

module.exports = {
    name: 'user-selector',
    props: ['fetchUrl', 'controlId', 'value', 'placeholder', 'ajaxParams'],
    data: function data() {
        return {
            options: [],
            isLoading: false,
            searchTerm: ''
        };
    },
    computed: {
        inputId: function inputId() {
            return 'sb_' + this.controlId;
        },
        placeholderText: function placeholderText() {
            return this.placeholder || "Select";
        }
    },
    mounted: function mounted() {
        var _this = this;

        var jqEl = $(this.$el);
        var focusTo = jqEl.find('#' + this.inputId);
        jqEl.on('shown.bs.dropdown', function () {
            focusTo.focus();
            _this.fetchOptions(_this.searchTerm);
        });

        jqEl.on('hidden.bs.dropdown', function () {
            _this.searchTerm = "";
        });
    },
    methods: {
        onSearchBoxDownKey: function onSearchBoxDownKey(event) {
            var $firstOptionAnchor = $(this.$refs.dropdownMenu).find('a').first();
            $firstOptionAnchor.focus();
        },
        onOptionUpKey: function onOptionUpKey(event) {
            var isFirstOption = $(event.target).parent().index() === 1;

            if (isFirstOption) {
                this.$refs.searchBox.focus();
                event.stopPropagation();
            }
        },

        fetchOptions: function fetchOptions() {
            var _this2 = this;

            var filter = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : "";

            this.isLoading = true;
            var requestParams = (0, _assign2.default)({ query: filter, cache: false }, this.ajaxParams);
            this.$http.get(this.fetchUrl, { params: requestParams }).then(function (response) {
                _this2.options = response.body.options || [];
                _this2.isLoading = false;
            }, function (response) {

                _this2.isLoading = false;
            });
        },
        clear: function clear() {
            this.$emit('selected', null, this.controlId);
            this.searchTerm = "";
        },
        selectOption: function selectOption(value) {
            this.$emit('selected', value, this.controlId);
        },
        updateOptionsList: function updateOptionsList(e) {
            this.fetchOptions(e.target.value);
        },

        highlight: function highlight(title, searchTerm) {
            var encodedTitle = _.escape(title);
            if (searchTerm) {
                var safeSearchTerm = _.escape(_.escapeRegExp(searchTerm));

                var iQuery = new RegExp(safeSearchTerm, "ig");
                return encodedTitle.replace(iQuery, function (matchedTxt, a, b) {
                    return '<strong>' + matchedTxt + '</strong>';
                });
            }

            return encodedTitle;
        }
    }
};
})()
if (module.exports.__esModule) module.exports = module.exports.default
var __vue__options__ = (typeof module.exports === "function"? module.exports.options: module.exports)
if (__vue__options__.functional) {console.error("[vueify] functional components are not supported and should be defined in plain js files using render functions.")}
__vue__options__.render = function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('div',{staticClass:"combo-box"},[_c('div',{staticClass:"btn-group btn-input clearfix"},[_c('button',{staticClass:"btn dropdown-toggle",attrs:{"type":"button","data-toggle":"dropdown"}},[(_vm.value === null)?_c('span',{staticClass:"gray-text",attrs:{"data-bind":"label"}},[_vm._v(_vm._s(_vm.placeholderText))]):_c('span',{attrs:{"data-bind":"label"}},[_vm._v(_vm._s(_vm.value.value))])]),_vm._v(" "),_c('ul',{ref:"dropdownMenu",staticClass:"dropdown-menu",attrs:{"role":"menu"}},[_c('li',[_c('input',{directives:[{name:"model",rawName:"v-model",value:(_vm.searchTerm),expression:"searchTerm"}],ref:"searchBox",attrs:{"type":"text","id":_vm.inputId,"placeholder":"Search"},domProps:{"value":(_vm.searchTerm)},on:{"input":[function($event){if($event.target.composing){ return; }_vm.searchTerm=$event.target.value},_vm.updateOptionsList],"keyup":function($event){if(!('button' in $event)&&_vm._k($event.keyCode,"down",40)){ return null; }_vm.onSearchBoxDownKey($event)}}})]),_vm._v(" "),_vm._l((_vm.options),function(option){return _c('li',{key:option.key},[_c('a',{attrs:{"href":"javascript:void(0);"},domProps:{"innerHTML":_vm._s(_vm.highlight(option.value, _vm.searchTerm))},on:{"click":function($event){_vm.selectOption(option)},"keydown":function($event){if(!('button' in $event)&&_vm._k($event.keyCode,"up",38)){ return null; }_vm.onOptionUpKey($event)}}})])}),_vm._v(" "),(_vm.isLoading)?_c('li',[_c('a',[_vm._v("Loading...")])]):_vm._e(),_vm._v(" "),(!_vm.isLoading && _vm.options.length === 0)?_c('li',[_c('a',[_vm._v("No results found")])]):_vm._e()],2)]),_vm._v(" "),(_vm.value !== null)?_c('button',{staticClass:"btn btn-link btn-clear",on:{"click":_vm.clear}},[_c('span')]):_vm._e()])}
__vue__options__.staticRenderFns = []
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-25b84282", __vue__options__)
  } else {
    hotAPI.reload("data-v-25b84282", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/object/assign":2,"vue-hot-reload-api":41}],45:[function(require,module,exports){
(function (global){
'use strict';

var _vue = (typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null);

var _vue2 = _interopRequireDefault(_vue);

var _vueResource = (typeof window !== "undefined" ? window['VueResource'] : typeof global !== "undefined" ? global['VueResource'] : null);

var _vueResource2 = _interopRequireDefault(_vueResource);

var _Typeahead = require('./Typeahead.vue');

var _Typeahead2 = _interopRequireDefault(_Typeahead);

var _DatePicker = require('./DatePicker.vue');

var _DatePicker2 = _interopRequireDefault(_DatePicker);

var _InterviewTable = require('./InterviewTable.vue');

var _InterviewTable2 = _interopRequireDefault(_InterviewTable);

var _veeValidate = (typeof window !== "undefined" ? window['VeeValidate'] : typeof global !== "undefined" ? global['VeeValidate'] : null);

var _veeValidate2 = _interopRequireDefault(_veeValidate);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

_vue2.default.use(_veeValidate2.default);
_vue2.default.use(_vueResource2.default);

_vue2.default.component('Flatpickr', _DatePicker2.default);
_vue2.default.component("typeahead", _Typeahead2.default);
_vue2.default.component("interview-table", _InterviewTable2.default);

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"./DatePicker.vue":42,"./InterviewTable.vue":43,"./Typeahead.vue":44}]},{},[45])
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvYmFiZWwtcnVudGltZS9jb3JlLWpzL2pzb24vc3RyaW5naWZ5LmpzIiwibm9kZV9tb2R1bGVzL2JhYmVsLXJ1bnRpbWUvY29yZS1qcy9vYmplY3QvYXNzaWduLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYS1mdW5jdGlvbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYW4tb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19hcnJheS1pbmNsdWRlcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY29mLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jb3JlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jdHguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2RlZmluZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2Rlc2NyaXB0b3JzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kb20tY3JlYXRlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19lbnVtLWJ1Zy1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19leHBvcnQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2ZhaWxzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19nbG9iYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2hhcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faGlkZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faWU4LWRvbS1kZWZpbmUuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lvYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lzLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWFzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWRwLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtZ29wcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWtleXMtaW50ZXJuYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtcGllLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19wcm9wZXJ0eS1kZXNjLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQta2V5LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLWluZGV4LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pbnRlZ2VyLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1sZW5ndGguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8tcHJpbWl0aXZlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL191aWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvZmxhdHBpY2tyL2Rpc3QvZmxhdHBpY2tyLmpzIiwibm9kZV9tb2R1bGVzL3Z1ZS1ob3QtcmVsb2FkLWFwaS9pbmRleC5qcyIsInZ1ZVxcdnVlXFxEYXRlUGlja2VyLnZ1ZT8yNGE2ZDFmMCIsInZ1ZVxcdnVlXFxJbnRlcnZpZXdUYWJsZS52dWU/Yjk4MmE3YzAiLCJ2dWVcXHZ1ZVxcVHlwZWFoZWFkLnZ1ZT9mOWNjZjhiMCIsInZ1ZVxcdnVlXFx0cm91Ymxlc2hvb3RpbmcuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUNBQTs7QUNBQTs7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7O0FDREE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDcEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTs7QUNEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ25CQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzVEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1BBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaENBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2ZBOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDUEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNMQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDWEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDNXZFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDbklBOzs7Ozs7O0FBR0E7QUFDQTtBQUNBO0FBREE7QUFHQTtBQUNBO0FBQ0E7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUFBO0FBQUE7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUZBO0FBWkE7QUFpQkE7QUFDQTtBQUNBO0FBREE7QUFHQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUhBO0FBS0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFQQTs7QUFVQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFFQTtBQUNBO0FBR0E7QUFDQTtBQUNBO0FBZkE7QUF4REE7Ozs7O0FBZkE7QUFBQTs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDV0E7QUFDQTtBQUNBO0FBQ0E7QUFBQTtBQUFBO0FBRkE7QUFJQTtBQUNBO0FBQ0E7QUFBQTtBQUFBO0FBRkE7QUFJQTtBQUNBO0FBQ0E7QUFBQTtBQUFBO0FBRkE7QUFUQTtBQWNBO0FBQ0E7QUFDQTtBQURBO0FBR0E7O0FBQ0E7QUFFQTtBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFFQTtBQUNBO0FBQ0E7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQWhCQTtBQWtCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFFQTtBQURBO0FBR0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBWkE7O0FBZUE7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBdkVBOzs7OztBQVZBO0FBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7QUM2QkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUhBO0FBS0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQU5BO0FBUUE7QUFBQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUNBO0FBQUE7O0FBQUE7O0FBQ0E7QUFDQTtBQUNBO0FBRUE7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQWhEQTtBQTlCQTs7Ozs7QUE3QkE7QUFBQTs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDQUU7Ozs7QUFDRjs7OztBQUNBOzs7O0FBQ0E7Ozs7QUFDQTs7OztBQUNBOzs7Ozs7QUFFQSxjQUFJLEdBQUo7QUFDQSxjQUFJLEdBQUo7O0FBRUEsY0FBSSxTQUFKLENBQWMsV0FBZDtBQUNBLGNBQUksU0FBSixDQUFjLFdBQWQ7QUFDQSxjQUFJLFNBQUosQ0FBYyxpQkFBZCIsImZpbGUiOiJnZW5lcmF0ZWQuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlc0NvbnRlbnQiOlsiKGZ1bmN0aW9uIGUodCxuLHIpe2Z1bmN0aW9uIHMobyx1KXtpZighbltvXSl7aWYoIXRbb10pe3ZhciBhPXR5cGVvZiByZXF1aXJlPT1cImZ1bmN0aW9uXCImJnJlcXVpcmU7aWYoIXUmJmEpcmV0dXJuIGEobywhMCk7aWYoaSlyZXR1cm4gaShvLCEwKTt2YXIgZj1uZXcgRXJyb3IoXCJDYW5ub3QgZmluZCBtb2R1bGUgJ1wiK28rXCInXCIpO3Rocm93IGYuY29kZT1cIk1PRFVMRV9OT1RfRk9VTkRcIixmfXZhciBsPW5bb109e2V4cG9ydHM6e319O3Rbb11bMF0uY2FsbChsLmV4cG9ydHMsZnVuY3Rpb24oZSl7dmFyIG49dFtvXVsxXVtlXTtyZXR1cm4gcyhuP246ZSl9LGwsbC5leHBvcnRzLGUsdCxuLHIpfXJldHVybiBuW29dLmV4cG9ydHN9dmFyIGk9dHlwZW9mIHJlcXVpcmU9PVwiZnVuY3Rpb25cIiYmcmVxdWlyZTtmb3IodmFyIG89MDtvPHIubGVuZ3RoO28rKylzKHJbb10pO3JldHVybiBzfSkiLCJtb2R1bGUuZXhwb3J0cyA9IHsgXCJkZWZhdWx0XCI6IHJlcXVpcmUoXCJjb3JlLWpzL2xpYnJhcnkvZm4vanNvbi9zdHJpbmdpZnlcIiksIF9fZXNNb2R1bGU6IHRydWUgfTsiLCJtb2R1bGUuZXhwb3J0cyA9IHsgXCJkZWZhdWx0XCI6IHJlcXVpcmUoXCJjb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnblwiKSwgX19lc01vZHVsZTogdHJ1ZSB9OyIsInZhciBjb3JlICA9IHJlcXVpcmUoJy4uLy4uL21vZHVsZXMvX2NvcmUnKVxuICAsICRKU09OID0gY29yZS5KU09OIHx8IChjb3JlLkpTT04gPSB7c3RyaW5naWZ5OiBKU09OLnN0cmluZ2lmeX0pO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbiBzdHJpbmdpZnkoaXQpeyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVudXNlZC12YXJzXG4gIHJldHVybiAkSlNPTi5zdHJpbmdpZnkuYXBwbHkoJEpTT04sIGFyZ3VtZW50cyk7XG59OyIsInJlcXVpcmUoJy4uLy4uL21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24nKTtcbm1vZHVsZS5leHBvcnRzID0gcmVxdWlyZSgnLi4vLi4vbW9kdWxlcy9fY29yZScpLk9iamVjdC5hc3NpZ247IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIGlmKHR5cGVvZiBpdCAhPSAnZnVuY3Rpb24nKXRocm93IFR5cGVFcnJvcihpdCArICcgaXMgbm90IGEgZnVuY3Rpb24hJyk7XG4gIHJldHVybiBpdDtcbn07IiwidmFyIGlzT2JqZWN0ID0gcmVxdWlyZSgnLi9faXMtb2JqZWN0Jyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgaWYoIWlzT2JqZWN0KGl0KSl0aHJvdyBUeXBlRXJyb3IoaXQgKyAnIGlzIG5vdCBhbiBvYmplY3QhJyk7XG4gIHJldHVybiBpdDtcbn07IiwiLy8gZmFsc2UgLT4gQXJyYXkjaW5kZXhPZlxuLy8gdHJ1ZSAgLT4gQXJyYXkjaW5jbHVkZXNcbnZhciB0b0lPYmplY3QgPSByZXF1aXJlKCcuL190by1pb2JqZWN0JylcbiAgLCB0b0xlbmd0aCAgPSByZXF1aXJlKCcuL190by1sZW5ndGgnKVxuICAsIHRvSW5kZXggICA9IHJlcXVpcmUoJy4vX3RvLWluZGV4Jyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKElTX0lOQ0xVREVTKXtcbiAgcmV0dXJuIGZ1bmN0aW9uKCR0aGlzLCBlbCwgZnJvbUluZGV4KXtcbiAgICB2YXIgTyAgICAgID0gdG9JT2JqZWN0KCR0aGlzKVxuICAgICAgLCBsZW5ndGggPSB0b0xlbmd0aChPLmxlbmd0aClcbiAgICAgICwgaW5kZXggID0gdG9JbmRleChmcm9tSW5kZXgsIGxlbmd0aClcbiAgICAgICwgdmFsdWU7XG4gICAgLy8gQXJyYXkjaW5jbHVkZXMgdXNlcyBTYW1lVmFsdWVaZXJvIGVxdWFsaXR5IGFsZ29yaXRobVxuICAgIGlmKElTX0lOQ0xVREVTICYmIGVsICE9IGVsKXdoaWxlKGxlbmd0aCA+IGluZGV4KXtcbiAgICAgIHZhbHVlID0gT1tpbmRleCsrXTtcbiAgICAgIGlmKHZhbHVlICE9IHZhbHVlKXJldHVybiB0cnVlO1xuICAgIC8vIEFycmF5I3RvSW5kZXggaWdub3JlcyBob2xlcywgQXJyYXkjaW5jbHVkZXMgLSBub3RcbiAgICB9IGVsc2UgZm9yKDtsZW5ndGggPiBpbmRleDsgaW5kZXgrKylpZihJU19JTkNMVURFUyB8fCBpbmRleCBpbiBPKXtcbiAgICAgIGlmKE9baW5kZXhdID09PSBlbClyZXR1cm4gSVNfSU5DTFVERVMgfHwgaW5kZXggfHwgMDtcbiAgICB9IHJldHVybiAhSVNfSU5DTFVERVMgJiYgLTE7XG4gIH07XG59OyIsInZhciB0b1N0cmluZyA9IHt9LnRvU3RyaW5nO1xuXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIHRvU3RyaW5nLmNhbGwoaXQpLnNsaWNlKDgsIC0xKTtcbn07IiwidmFyIGNvcmUgPSBtb2R1bGUuZXhwb3J0cyA9IHt2ZXJzaW9uOiAnMi40LjAnfTtcbmlmKHR5cGVvZiBfX2UgPT0gJ251bWJlcicpX19lID0gY29yZTsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bmRlZiIsIi8vIG9wdGlvbmFsIC8gc2ltcGxlIGNvbnRleHQgYmluZGluZ1xudmFyIGFGdW5jdGlvbiA9IHJlcXVpcmUoJy4vX2EtZnVuY3Rpb24nKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oZm4sIHRoYXQsIGxlbmd0aCl7XG4gIGFGdW5jdGlvbihmbik7XG4gIGlmKHRoYXQgPT09IHVuZGVmaW5lZClyZXR1cm4gZm47XG4gIHN3aXRjaChsZW5ndGgpe1xuICAgIGNhc2UgMTogcmV0dXJuIGZ1bmN0aW9uKGEpe1xuICAgICAgcmV0dXJuIGZuLmNhbGwodGhhdCwgYSk7XG4gICAgfTtcbiAgICBjYXNlIDI6IHJldHVybiBmdW5jdGlvbihhLCBiKXtcbiAgICAgIHJldHVybiBmbi5jYWxsKHRoYXQsIGEsIGIpO1xuICAgIH07XG4gICAgY2FzZSAzOiByZXR1cm4gZnVuY3Rpb24oYSwgYiwgYyl7XG4gICAgICByZXR1cm4gZm4uY2FsbCh0aGF0LCBhLCBiLCBjKTtcbiAgICB9O1xuICB9XG4gIHJldHVybiBmdW5jdGlvbigvKiAuLi5hcmdzICovKXtcbiAgICByZXR1cm4gZm4uYXBwbHkodGhhdCwgYXJndW1lbnRzKTtcbiAgfTtcbn07IiwiLy8gNy4yLjEgUmVxdWlyZU9iamVjdENvZXJjaWJsZShhcmd1bWVudClcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICBpZihpdCA9PSB1bmRlZmluZWQpdGhyb3cgVHlwZUVycm9yKFwiQ2FuJ3QgY2FsbCBtZXRob2Qgb24gIFwiICsgaXQpO1xuICByZXR1cm4gaXQ7XG59OyIsIi8vIFRoYW5rJ3MgSUU4IGZvciBoaXMgZnVubnkgZGVmaW5lUHJvcGVydHlcbm1vZHVsZS5leHBvcnRzID0gIXJlcXVpcmUoJy4vX2ZhaWxzJykoZnVuY3Rpb24oKXtcbiAgcmV0dXJuIE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh7fSwgJ2EnLCB7Z2V0OiBmdW5jdGlvbigpeyByZXR1cm4gNzsgfX0pLmEgIT0gNztcbn0pOyIsInZhciBpc09iamVjdCA9IHJlcXVpcmUoJy4vX2lzLW9iamVjdCcpXG4gICwgZG9jdW1lbnQgPSByZXF1aXJlKCcuL19nbG9iYWwnKS5kb2N1bWVudFxuICAvLyBpbiBvbGQgSUUgdHlwZW9mIGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQgaXMgJ29iamVjdCdcbiAgLCBpcyA9IGlzT2JqZWN0KGRvY3VtZW50KSAmJiBpc09iamVjdChkb2N1bWVudC5jcmVhdGVFbGVtZW50KTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gaXMgPyBkb2N1bWVudC5jcmVhdGVFbGVtZW50KGl0KSA6IHt9O1xufTsiLCIvLyBJRSA4LSBkb24ndCBlbnVtIGJ1ZyBrZXlzXG5tb2R1bGUuZXhwb3J0cyA9IChcbiAgJ2NvbnN0cnVjdG9yLGhhc093blByb3BlcnR5LGlzUHJvdG90eXBlT2YscHJvcGVydHlJc0VudW1lcmFibGUsdG9Mb2NhbGVTdHJpbmcsdG9TdHJpbmcsdmFsdWVPZidcbikuc3BsaXQoJywnKTsiLCJ2YXIgZ2xvYmFsICAgID0gcmVxdWlyZSgnLi9fZ2xvYmFsJylcbiAgLCBjb3JlICAgICAgPSByZXF1aXJlKCcuL19jb3JlJylcbiAgLCBjdHggICAgICAgPSByZXF1aXJlKCcuL19jdHgnKVxuICAsIGhpZGUgICAgICA9IHJlcXVpcmUoJy4vX2hpZGUnKVxuICAsIFBST1RPVFlQRSA9ICdwcm90b3R5cGUnO1xuXG52YXIgJGV4cG9ydCA9IGZ1bmN0aW9uKHR5cGUsIG5hbWUsIHNvdXJjZSl7XG4gIHZhciBJU19GT1JDRUQgPSB0eXBlICYgJGV4cG9ydC5GXG4gICAgLCBJU19HTE9CQUwgPSB0eXBlICYgJGV4cG9ydC5HXG4gICAgLCBJU19TVEFUSUMgPSB0eXBlICYgJGV4cG9ydC5TXG4gICAgLCBJU19QUk9UTyAgPSB0eXBlICYgJGV4cG9ydC5QXG4gICAgLCBJU19CSU5EICAgPSB0eXBlICYgJGV4cG9ydC5CXG4gICAgLCBJU19XUkFQICAgPSB0eXBlICYgJGV4cG9ydC5XXG4gICAgLCBleHBvcnRzICAgPSBJU19HTE9CQUwgPyBjb3JlIDogY29yZVtuYW1lXSB8fCAoY29yZVtuYW1lXSA9IHt9KVxuICAgICwgZXhwUHJvdG8gID0gZXhwb3J0c1tQUk9UT1RZUEVdXG4gICAgLCB0YXJnZXQgICAgPSBJU19HTE9CQUwgPyBnbG9iYWwgOiBJU19TVEFUSUMgPyBnbG9iYWxbbmFtZV0gOiAoZ2xvYmFsW25hbWVdIHx8IHt9KVtQUk9UT1RZUEVdXG4gICAgLCBrZXksIG93biwgb3V0O1xuICBpZihJU19HTE9CQUwpc291cmNlID0gbmFtZTtcbiAgZm9yKGtleSBpbiBzb3VyY2Upe1xuICAgIC8vIGNvbnRhaW5zIGluIG5hdGl2ZVxuICAgIG93biA9ICFJU19GT1JDRUQgJiYgdGFyZ2V0ICYmIHRhcmdldFtrZXldICE9PSB1bmRlZmluZWQ7XG4gICAgaWYob3duICYmIGtleSBpbiBleHBvcnRzKWNvbnRpbnVlO1xuICAgIC8vIGV4cG9ydCBuYXRpdmUgb3IgcGFzc2VkXG4gICAgb3V0ID0gb3duID8gdGFyZ2V0W2tleV0gOiBzb3VyY2Vba2V5XTtcbiAgICAvLyBwcmV2ZW50IGdsb2JhbCBwb2xsdXRpb24gZm9yIG5hbWVzcGFjZXNcbiAgICBleHBvcnRzW2tleV0gPSBJU19HTE9CQUwgJiYgdHlwZW9mIHRhcmdldFtrZXldICE9ICdmdW5jdGlvbicgPyBzb3VyY2Vba2V5XVxuICAgIC8vIGJpbmQgdGltZXJzIHRvIGdsb2JhbCBmb3IgY2FsbCBmcm9tIGV4cG9ydCBjb250ZXh0XG4gICAgOiBJU19CSU5EICYmIG93biA/IGN0eChvdXQsIGdsb2JhbClcbiAgICAvLyB3cmFwIGdsb2JhbCBjb25zdHJ1Y3RvcnMgZm9yIHByZXZlbnQgY2hhbmdlIHRoZW0gaW4gbGlicmFyeVxuICAgIDogSVNfV1JBUCAmJiB0YXJnZXRba2V5XSA9PSBvdXQgPyAoZnVuY3Rpb24oQyl7XG4gICAgICB2YXIgRiA9IGZ1bmN0aW9uKGEsIGIsIGMpe1xuICAgICAgICBpZih0aGlzIGluc3RhbmNlb2YgQyl7XG4gICAgICAgICAgc3dpdGNoKGFyZ3VtZW50cy5sZW5ndGgpe1xuICAgICAgICAgICAgY2FzZSAwOiByZXR1cm4gbmV3IEM7XG4gICAgICAgICAgICBjYXNlIDE6IHJldHVybiBuZXcgQyhhKTtcbiAgICAgICAgICAgIGNhc2UgMjogcmV0dXJuIG5ldyBDKGEsIGIpO1xuICAgICAgICAgIH0gcmV0dXJuIG5ldyBDKGEsIGIsIGMpO1xuICAgICAgICB9IHJldHVybiBDLmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XG4gICAgICB9O1xuICAgICAgRltQUk9UT1RZUEVdID0gQ1tQUk9UT1RZUEVdO1xuICAgICAgcmV0dXJuIEY7XG4gICAgLy8gbWFrZSBzdGF0aWMgdmVyc2lvbnMgZm9yIHByb3RvdHlwZSBtZXRob2RzXG4gICAgfSkob3V0KSA6IElTX1BST1RPICYmIHR5cGVvZiBvdXQgPT0gJ2Z1bmN0aW9uJyA/IGN0eChGdW5jdGlvbi5jYWxsLCBvdXQpIDogb3V0O1xuICAgIC8vIGV4cG9ydCBwcm90byBtZXRob2RzIHRvIGNvcmUuJUNPTlNUUlVDVE9SJS5tZXRob2RzLiVOQU1FJVxuICAgIGlmKElTX1BST1RPKXtcbiAgICAgIChleHBvcnRzLnZpcnR1YWwgfHwgKGV4cG9ydHMudmlydHVhbCA9IHt9KSlba2V5XSA9IG91dDtcbiAgICAgIC8vIGV4cG9ydCBwcm90byBtZXRob2RzIHRvIGNvcmUuJUNPTlNUUlVDVE9SJS5wcm90b3R5cGUuJU5BTUUlXG4gICAgICBpZih0eXBlICYgJGV4cG9ydC5SICYmIGV4cFByb3RvICYmICFleHBQcm90b1trZXldKWhpZGUoZXhwUHJvdG8sIGtleSwgb3V0KTtcbiAgICB9XG4gIH1cbn07XG4vLyB0eXBlIGJpdG1hcFxuJGV4cG9ydC5GID0gMTsgICAvLyBmb3JjZWRcbiRleHBvcnQuRyA9IDI7ICAgLy8gZ2xvYmFsXG4kZXhwb3J0LlMgPSA0OyAgIC8vIHN0YXRpY1xuJGV4cG9ydC5QID0gODsgICAvLyBwcm90b1xuJGV4cG9ydC5CID0gMTY7ICAvLyBiaW5kXG4kZXhwb3J0LlcgPSAzMjsgIC8vIHdyYXBcbiRleHBvcnQuVSA9IDY0OyAgLy8gc2FmZVxuJGV4cG9ydC5SID0gMTI4OyAvLyByZWFsIHByb3RvIG1ldGhvZCBmb3IgYGxpYnJhcnlgIFxubW9kdWxlLmV4cG9ydHMgPSAkZXhwb3J0OyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oZXhlYyl7XG4gIHRyeSB7XG4gICAgcmV0dXJuICEhZXhlYygpO1xuICB9IGNhdGNoKGUpe1xuICAgIHJldHVybiB0cnVlO1xuICB9XG59OyIsIi8vIGh0dHBzOi8vZ2l0aHViLmNvbS96bG9pcm9jay9jb3JlLWpzL2lzc3Vlcy84NiNpc3N1ZWNvbW1lbnQtMTE1NzU5MDI4XG52YXIgZ2xvYmFsID0gbW9kdWxlLmV4cG9ydHMgPSB0eXBlb2Ygd2luZG93ICE9ICd1bmRlZmluZWQnICYmIHdpbmRvdy5NYXRoID09IE1hdGhcbiAgPyB3aW5kb3cgOiB0eXBlb2Ygc2VsZiAhPSAndW5kZWZpbmVkJyAmJiBzZWxmLk1hdGggPT0gTWF0aCA/IHNlbGYgOiBGdW5jdGlvbigncmV0dXJuIHRoaXMnKSgpO1xuaWYodHlwZW9mIF9fZyA9PSAnbnVtYmVyJylfX2cgPSBnbG9iYWw7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW5kZWYiLCJ2YXIgaGFzT3duUHJvcGVydHkgPSB7fS5oYXNPd25Qcm9wZXJ0eTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQsIGtleSl7XG4gIHJldHVybiBoYXNPd25Qcm9wZXJ0eS5jYWxsKGl0LCBrZXkpO1xufTsiLCJ2YXIgZFAgICAgICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1kcCcpXG4gICwgY3JlYXRlRGVzYyA9IHJlcXVpcmUoJy4vX3Byb3BlcnR5LWRlc2MnKTtcbm1vZHVsZS5leHBvcnRzID0gcmVxdWlyZSgnLi9fZGVzY3JpcHRvcnMnKSA/IGZ1bmN0aW9uKG9iamVjdCwga2V5LCB2YWx1ZSl7XG4gIHJldHVybiBkUC5mKG9iamVjdCwga2V5LCBjcmVhdGVEZXNjKDEsIHZhbHVlKSk7XG59IDogZnVuY3Rpb24ob2JqZWN0LCBrZXksIHZhbHVlKXtcbiAgb2JqZWN0W2tleV0gPSB2YWx1ZTtcbiAgcmV0dXJuIG9iamVjdDtcbn07IiwibW9kdWxlLmV4cG9ydHMgPSAhcmVxdWlyZSgnLi9fZGVzY3JpcHRvcnMnKSAmJiAhcmVxdWlyZSgnLi9fZmFpbHMnKShmdW5jdGlvbigpe1xuICByZXR1cm4gT2JqZWN0LmRlZmluZVByb3BlcnR5KHJlcXVpcmUoJy4vX2RvbS1jcmVhdGUnKSgnZGl2JyksICdhJywge2dldDogZnVuY3Rpb24oKXsgcmV0dXJuIDc7IH19KS5hICE9IDc7XG59KTsiLCIvLyBmYWxsYmFjayBmb3Igbm9uLWFycmF5LWxpa2UgRVMzIGFuZCBub24tZW51bWVyYWJsZSBvbGQgVjggc3RyaW5nc1xudmFyIGNvZiA9IHJlcXVpcmUoJy4vX2NvZicpO1xubW9kdWxlLmV4cG9ydHMgPSBPYmplY3QoJ3onKS5wcm9wZXJ0eUlzRW51bWVyYWJsZSgwKSA/IE9iamVjdCA6IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGNvZihpdCkgPT0gJ1N0cmluZycgPyBpdC5zcGxpdCgnJykgOiBPYmplY3QoaXQpO1xufTsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIHR5cGVvZiBpdCA9PT0gJ29iamVjdCcgPyBpdCAhPT0gbnVsbCA6IHR5cGVvZiBpdCA9PT0gJ2Z1bmN0aW9uJztcbn07IiwiJ3VzZSBzdHJpY3QnO1xuLy8gMTkuMS4yLjEgT2JqZWN0LmFzc2lnbih0YXJnZXQsIHNvdXJjZSwgLi4uKVxudmFyIGdldEtleXMgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWtleXMnKVxuICAsIGdPUFMgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWdvcHMnKVxuICAsIHBJRSAgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LXBpZScpXG4gICwgdG9PYmplY3QgPSByZXF1aXJlKCcuL190by1vYmplY3QnKVxuICAsIElPYmplY3QgID0gcmVxdWlyZSgnLi9faW9iamVjdCcpXG4gICwgJGFzc2lnbiAgPSBPYmplY3QuYXNzaWduO1xuXG4vLyBzaG91bGQgd29yayB3aXRoIHN5bWJvbHMgYW5kIHNob3VsZCBoYXZlIGRldGVybWluaXN0aWMgcHJvcGVydHkgb3JkZXIgKFY4IGJ1Zylcbm1vZHVsZS5leHBvcnRzID0gISRhc3NpZ24gfHwgcmVxdWlyZSgnLi9fZmFpbHMnKShmdW5jdGlvbigpe1xuICB2YXIgQSA9IHt9XG4gICAgLCBCID0ge31cbiAgICAsIFMgPSBTeW1ib2woKVxuICAgICwgSyA9ICdhYmNkZWZnaGlqa2xtbm9wcXJzdCc7XG4gIEFbU10gPSA3O1xuICBLLnNwbGl0KCcnKS5mb3JFYWNoKGZ1bmN0aW9uKGspeyBCW2tdID0gazsgfSk7XG4gIHJldHVybiAkYXNzaWduKHt9LCBBKVtTXSAhPSA3IHx8IE9iamVjdC5rZXlzKCRhc3NpZ24oe30sIEIpKS5qb2luKCcnKSAhPSBLO1xufSkgPyBmdW5jdGlvbiBhc3NpZ24odGFyZ2V0LCBzb3VyY2UpeyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVudXNlZC12YXJzXG4gIHZhciBUICAgICA9IHRvT2JqZWN0KHRhcmdldClcbiAgICAsIGFMZW4gID0gYXJndW1lbnRzLmxlbmd0aFxuICAgICwgaW5kZXggPSAxXG4gICAgLCBnZXRTeW1ib2xzID0gZ09QUy5mXG4gICAgLCBpc0VudW0gICAgID0gcElFLmY7XG4gIHdoaWxlKGFMZW4gPiBpbmRleCl7XG4gICAgdmFyIFMgICAgICA9IElPYmplY3QoYXJndW1lbnRzW2luZGV4KytdKVxuICAgICAgLCBrZXlzICAgPSBnZXRTeW1ib2xzID8gZ2V0S2V5cyhTKS5jb25jYXQoZ2V0U3ltYm9scyhTKSkgOiBnZXRLZXlzKFMpXG4gICAgICAsIGxlbmd0aCA9IGtleXMubGVuZ3RoXG4gICAgICAsIGogICAgICA9IDBcbiAgICAgICwga2V5O1xuICAgIHdoaWxlKGxlbmd0aCA+IGopaWYoaXNFbnVtLmNhbGwoUywga2V5ID0ga2V5c1tqKytdKSlUW2tleV0gPSBTW2tleV07XG4gIH0gcmV0dXJuIFQ7XG59IDogJGFzc2lnbjsiLCJ2YXIgYW5PYmplY3QgICAgICAgPSByZXF1aXJlKCcuL19hbi1vYmplY3QnKVxuICAsIElFOF9ET01fREVGSU5FID0gcmVxdWlyZSgnLi9faWU4LWRvbS1kZWZpbmUnKVxuICAsIHRvUHJpbWl0aXZlICAgID0gcmVxdWlyZSgnLi9fdG8tcHJpbWl0aXZlJylcbiAgLCBkUCAgICAgICAgICAgICA9IE9iamVjdC5kZWZpbmVQcm9wZXJ0eTtcblxuZXhwb3J0cy5mID0gcmVxdWlyZSgnLi9fZGVzY3JpcHRvcnMnKSA/IE9iamVjdC5kZWZpbmVQcm9wZXJ0eSA6IGZ1bmN0aW9uIGRlZmluZVByb3BlcnR5KE8sIFAsIEF0dHJpYnV0ZXMpe1xuICBhbk9iamVjdChPKTtcbiAgUCA9IHRvUHJpbWl0aXZlKFAsIHRydWUpO1xuICBhbk9iamVjdChBdHRyaWJ1dGVzKTtcbiAgaWYoSUU4X0RPTV9ERUZJTkUpdHJ5IHtcbiAgICByZXR1cm4gZFAoTywgUCwgQXR0cmlidXRlcyk7XG4gIH0gY2F0Y2goZSl7IC8qIGVtcHR5ICovIH1cbiAgaWYoJ2dldCcgaW4gQXR0cmlidXRlcyB8fCAnc2V0JyBpbiBBdHRyaWJ1dGVzKXRocm93IFR5cGVFcnJvcignQWNjZXNzb3JzIG5vdCBzdXBwb3J0ZWQhJyk7XG4gIGlmKCd2YWx1ZScgaW4gQXR0cmlidXRlcylPW1BdID0gQXR0cmlidXRlcy52YWx1ZTtcbiAgcmV0dXJuIE87XG59OyIsImV4cG9ydHMuZiA9IE9iamVjdC5nZXRPd25Qcm9wZXJ0eVN5bWJvbHM7IiwidmFyIGhhcyAgICAgICAgICA9IHJlcXVpcmUoJy4vX2hhcycpXG4gICwgdG9JT2JqZWN0ICAgID0gcmVxdWlyZSgnLi9fdG8taW9iamVjdCcpXG4gICwgYXJyYXlJbmRleE9mID0gcmVxdWlyZSgnLi9fYXJyYXktaW5jbHVkZXMnKShmYWxzZSlcbiAgLCBJRV9QUk9UTyAgICAgPSByZXF1aXJlKCcuL19zaGFyZWQta2V5JykoJ0lFX1BST1RPJyk7XG5cbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24ob2JqZWN0LCBuYW1lcyl7XG4gIHZhciBPICAgICAgPSB0b0lPYmplY3Qob2JqZWN0KVxuICAgICwgaSAgICAgID0gMFxuICAgICwgcmVzdWx0ID0gW11cbiAgICAsIGtleTtcbiAgZm9yKGtleSBpbiBPKWlmKGtleSAhPSBJRV9QUk9UTyloYXMoTywga2V5KSAmJiByZXN1bHQucHVzaChrZXkpO1xuICAvLyBEb24ndCBlbnVtIGJ1ZyAmIGhpZGRlbiBrZXlzXG4gIHdoaWxlKG5hbWVzLmxlbmd0aCA+IGkpaWYoaGFzKE8sIGtleSA9IG5hbWVzW2krK10pKXtcbiAgICB+YXJyYXlJbmRleE9mKHJlc3VsdCwga2V5KSB8fCByZXN1bHQucHVzaChrZXkpO1xuICB9XG4gIHJldHVybiByZXN1bHQ7XG59OyIsIi8vIDE5LjEuMi4xNCAvIDE1LjIuMy4xNCBPYmplY3Qua2V5cyhPKVxudmFyICRrZXlzICAgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWtleXMtaW50ZXJuYWwnKVxuICAsIGVudW1CdWdLZXlzID0gcmVxdWlyZSgnLi9fZW51bS1idWcta2V5cycpO1xuXG5tb2R1bGUuZXhwb3J0cyA9IE9iamVjdC5rZXlzIHx8IGZ1bmN0aW9uIGtleXMoTyl7XG4gIHJldHVybiAka2V5cyhPLCBlbnVtQnVnS2V5cyk7XG59OyIsImV4cG9ydHMuZiA9IHt9LnByb3BlcnR5SXNFbnVtZXJhYmxlOyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oYml0bWFwLCB2YWx1ZSl7XG4gIHJldHVybiB7XG4gICAgZW51bWVyYWJsZSAgOiAhKGJpdG1hcCAmIDEpLFxuICAgIGNvbmZpZ3VyYWJsZTogIShiaXRtYXAgJiAyKSxcbiAgICB3cml0YWJsZSAgICA6ICEoYml0bWFwICYgNCksXG4gICAgdmFsdWUgICAgICAgOiB2YWx1ZVxuICB9O1xufTsiLCJ2YXIgc2hhcmVkID0gcmVxdWlyZSgnLi9fc2hhcmVkJykoJ2tleXMnKVxuICAsIHVpZCAgICA9IHJlcXVpcmUoJy4vX3VpZCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihrZXkpe1xuICByZXR1cm4gc2hhcmVkW2tleV0gfHwgKHNoYXJlZFtrZXldID0gdWlkKGtleSkpO1xufTsiLCJ2YXIgZ2xvYmFsID0gcmVxdWlyZSgnLi9fZ2xvYmFsJylcbiAgLCBTSEFSRUQgPSAnX19jb3JlLWpzX3NoYXJlZF9fJ1xuICAsIHN0b3JlICA9IGdsb2JhbFtTSEFSRURdIHx8IChnbG9iYWxbU0hBUkVEXSA9IHt9KTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oa2V5KXtcbiAgcmV0dXJuIHN0b3JlW2tleV0gfHwgKHN0b3JlW2tleV0gPSB7fSk7XG59OyIsInZhciB0b0ludGVnZXIgPSByZXF1aXJlKCcuL190by1pbnRlZ2VyJylcbiAgLCBtYXggICAgICAgPSBNYXRoLm1heFxuICAsIG1pbiAgICAgICA9IE1hdGgubWluO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpbmRleCwgbGVuZ3RoKXtcbiAgaW5kZXggPSB0b0ludGVnZXIoaW5kZXgpO1xuICByZXR1cm4gaW5kZXggPCAwID8gbWF4KGluZGV4ICsgbGVuZ3RoLCAwKSA6IG1pbihpbmRleCwgbGVuZ3RoKTtcbn07IiwiLy8gNy4xLjQgVG9JbnRlZ2VyXG52YXIgY2VpbCAgPSBNYXRoLmNlaWxcbiAgLCBmbG9vciA9IE1hdGguZmxvb3I7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGlzTmFOKGl0ID0gK2l0KSA/IDAgOiAoaXQgPiAwID8gZmxvb3IgOiBjZWlsKShpdCk7XG59OyIsIi8vIHRvIGluZGV4ZWQgb2JqZWN0LCB0b09iamVjdCB3aXRoIGZhbGxiYWNrIGZvciBub24tYXJyYXktbGlrZSBFUzMgc3RyaW5nc1xudmFyIElPYmplY3QgPSByZXF1aXJlKCcuL19pb2JqZWN0JylcbiAgLCBkZWZpbmVkID0gcmVxdWlyZSgnLi9fZGVmaW5lZCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBJT2JqZWN0KGRlZmluZWQoaXQpKTtcbn07IiwiLy8gNy4xLjE1IFRvTGVuZ3RoXG52YXIgdG9JbnRlZ2VyID0gcmVxdWlyZSgnLi9fdG8taW50ZWdlcicpXG4gICwgbWluICAgICAgID0gTWF0aC5taW47XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGl0ID4gMCA/IG1pbih0b0ludGVnZXIoaXQpLCAweDFmZmZmZmZmZmZmZmZmKSA6IDA7IC8vIHBvdygyLCA1MykgLSAxID09IDkwMDcxOTkyNTQ3NDA5OTFcbn07IiwiLy8gNy4xLjEzIFRvT2JqZWN0KGFyZ3VtZW50KVxudmFyIGRlZmluZWQgPSByZXF1aXJlKCcuL19kZWZpbmVkJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIE9iamVjdChkZWZpbmVkKGl0KSk7XG59OyIsIi8vIDcuMS4xIFRvUHJpbWl0aXZlKGlucHV0IFssIFByZWZlcnJlZFR5cGVdKVxudmFyIGlzT2JqZWN0ID0gcmVxdWlyZSgnLi9faXMtb2JqZWN0Jyk7XG4vLyBpbnN0ZWFkIG9mIHRoZSBFUzYgc3BlYyB2ZXJzaW9uLCB3ZSBkaWRuJ3QgaW1wbGVtZW50IEBAdG9QcmltaXRpdmUgY2FzZVxuLy8gYW5kIHRoZSBzZWNvbmQgYXJndW1lbnQgLSBmbGFnIC0gcHJlZmVycmVkIHR5cGUgaXMgYSBzdHJpbmdcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQsIFMpe1xuICBpZighaXNPYmplY3QoaXQpKXJldHVybiBpdDtcbiAgdmFyIGZuLCB2YWw7XG4gIGlmKFMgJiYgdHlwZW9mIChmbiA9IGl0LnRvU3RyaW5nKSA9PSAnZnVuY3Rpb24nICYmICFpc09iamVjdCh2YWwgPSBmbi5jYWxsKGl0KSkpcmV0dXJuIHZhbDtcbiAgaWYodHlwZW9mIChmbiA9IGl0LnZhbHVlT2YpID09ICdmdW5jdGlvbicgJiYgIWlzT2JqZWN0KHZhbCA9IGZuLmNhbGwoaXQpKSlyZXR1cm4gdmFsO1xuICBpZighUyAmJiB0eXBlb2YgKGZuID0gaXQudG9TdHJpbmcpID09ICdmdW5jdGlvbicgJiYgIWlzT2JqZWN0KHZhbCA9IGZuLmNhbGwoaXQpKSlyZXR1cm4gdmFsO1xuICB0aHJvdyBUeXBlRXJyb3IoXCJDYW4ndCBjb252ZXJ0IG9iamVjdCB0byBwcmltaXRpdmUgdmFsdWVcIik7XG59OyIsInZhciBpZCA9IDBcbiAgLCBweCA9IE1hdGgucmFuZG9tKCk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGtleSl7XG4gIHJldHVybiAnU3ltYm9sKCcuY29uY2F0KGtleSA9PT0gdW5kZWZpbmVkID8gJycgOiBrZXksICcpXycsICgrK2lkICsgcHgpLnRvU3RyaW5nKDM2KSk7XG59OyIsIi8vIDE5LjEuMy4xIE9iamVjdC5hc3NpZ24odGFyZ2V0LCBzb3VyY2UpXG52YXIgJGV4cG9ydCA9IHJlcXVpcmUoJy4vX2V4cG9ydCcpO1xuXG4kZXhwb3J0KCRleHBvcnQuUyArICRleHBvcnQuRiwgJ09iamVjdCcsIHthc3NpZ246IHJlcXVpcmUoJy4vX29iamVjdC1hc3NpZ24nKX0pOyIsInZhciBfZXh0ZW5kcyA9IE9iamVjdC5hc3NpZ24gfHwgZnVuY3Rpb24gKHRhcmdldCkgeyBmb3IgKHZhciBpID0gMTsgaSA8IGFyZ3VtZW50cy5sZW5ndGg7IGkrKykgeyB2YXIgc291cmNlID0gYXJndW1lbnRzW2ldOyBmb3IgKHZhciBrZXkgaW4gc291cmNlKSB7IGlmIChPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwoc291cmNlLCBrZXkpKSB7IHRhcmdldFtrZXldID0gc291cmNlW2tleV07IH0gfSB9IHJldHVybiB0YXJnZXQ7IH07XG5cbnZhciBfdHlwZW9mID0gdHlwZW9mIFN5bWJvbCA9PT0gXCJmdW5jdGlvblwiICYmIHR5cGVvZiBTeW1ib2wuaXRlcmF0b3IgPT09IFwic3ltYm9sXCIgPyBmdW5jdGlvbiAob2JqKSB7IHJldHVybiB0eXBlb2Ygb2JqOyB9IDogZnVuY3Rpb24gKG9iaikgeyByZXR1cm4gb2JqICYmIHR5cGVvZiBTeW1ib2wgPT09IFwiZnVuY3Rpb25cIiAmJiBvYmouY29uc3RydWN0b3IgPT09IFN5bWJvbCAmJiBvYmogIT09IFN5bWJvbC5wcm90b3R5cGUgPyBcInN5bWJvbFwiIDogdHlwZW9mIG9iajsgfTtcblxuLyohIGZsYXRwaWNrciB2Mi42LjMsIEBsaWNlbnNlIE1JVCAqL1xuZnVuY3Rpb24gRmxhdHBpY2tyKGVsZW1lbnQsIGNvbmZpZykge1xuXHR2YXIgc2VsZiA9IHRoaXM7XG5cblx0c2VsZi5fID0ge307XG5cdHNlbGYuXy5hZnRlckRheUFuaW0gPSBhZnRlckRheUFuaW07XG5cdHNlbGYuY2hhbmdlTW9udGggPSBjaGFuZ2VNb250aDtcblx0c2VsZi5jaGFuZ2VZZWFyID0gY2hhbmdlWWVhcjtcblx0c2VsZi5jbGVhciA9IGNsZWFyO1xuXHRzZWxmLmNsb3NlID0gY2xvc2U7XG5cdHNlbGYuX2NyZWF0ZUVsZW1lbnQgPSBjcmVhdGVFbGVtZW50O1xuXHRzZWxmLmRlc3Ryb3kgPSBkZXN0cm95O1xuXHRzZWxmLmlzRW5hYmxlZCA9IGlzRW5hYmxlZDtcblx0c2VsZi5qdW1wVG9EYXRlID0ganVtcFRvRGF0ZTtcblx0c2VsZi5vcGVuID0gb3Blbjtcblx0c2VsZi5yZWRyYXcgPSByZWRyYXc7XG5cdHNlbGYuc2V0ID0gc2V0O1xuXHRzZWxmLnNldERhdGUgPSBzZXREYXRlO1xuXHRzZWxmLnRvZ2dsZSA9IHRvZ2dsZTtcblxuXHRmdW5jdGlvbiBpbml0KCkge1xuXHRcdHNlbGYuZWxlbWVudCA9IHNlbGYuaW5wdXQgPSBlbGVtZW50O1xuXHRcdHNlbGYuaW5zdGFuY2VDb25maWcgPSBjb25maWcgfHwge307XG5cdFx0c2VsZi5wYXJzZURhdGUgPSBGbGF0cGlja3IucHJvdG90eXBlLnBhcnNlRGF0ZS5iaW5kKHNlbGYpO1xuXHRcdHNlbGYuZm9ybWF0RGF0ZSA9IEZsYXRwaWNrci5wcm90b3R5cGUuZm9ybWF0RGF0ZS5iaW5kKHNlbGYpO1xuXG5cdFx0c2V0dXBGb3JtYXRzKCk7XG5cdFx0cGFyc2VDb25maWcoKTtcblx0XHRzZXR1cExvY2FsZSgpO1xuXHRcdHNldHVwSW5wdXRzKCk7XG5cdFx0c2V0dXBEYXRlcygpO1xuXHRcdHNldHVwSGVscGVyRnVuY3Rpb25zKCk7XG5cblx0XHRzZWxmLmlzT3BlbiA9IGZhbHNlO1xuXG5cdFx0c2VsZi5pc01vYmlsZSA9ICFzZWxmLmNvbmZpZy5kaXNhYmxlTW9iaWxlICYmICFzZWxmLmNvbmZpZy5pbmxpbmUgJiYgc2VsZi5jb25maWcubW9kZSA9PT0gXCJzaW5nbGVcIiAmJiAhc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGggJiYgIXNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggJiYgIXNlbGYuY29uZmlnLndlZWtOdW1iZXJzICYmIC9BbmRyb2lkfHdlYk9TfGlQaG9uZXxpUGFkfGlQb2R8QmxhY2tCZXJyeXxJRU1vYmlsZXxPcGVyYSBNaW5pL2kudGVzdChuYXZpZ2F0b3IudXNlckFnZW50KTtcblxuXHRcdGlmICghc2VsZi5pc01vYmlsZSkgYnVpbGQoKTtcblxuXHRcdGJpbmRFdmVudHMoKTtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoIHx8IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHtcblx0XHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSB7XG5cdFx0XHRcdHNldEhvdXJzRnJvbURhdGUoc2VsZi5jb25maWcubm9DYWxlbmRhciA/IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqIHx8IHNlbGYuY29uZmlnLm1pbkRhdGUgOiBudWxsKTtcblx0XHRcdH1cblx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzKSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLndpZHRoID0gc2VsZi5kYXlzQ29udGFpbmVyLm9mZnNldFdpZHRoICsgc2VsZi53ZWVrV3JhcHBlci5vZmZzZXRXaWR0aCArIFwicHhcIjtcblx0XHR9XG5cblx0XHRzZWxmLnNob3dUaW1lSW5wdXQgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID4gMCB8fCBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJSZWFkeVwiKTtcblx0fVxuXG5cdC8qKlxuICAqIEJpbmRzIGEgZnVuY3Rpb24gdG8gdGhlIGN1cnJlbnQgZmxhdHBpY2tyIGluc3RhbmNlXG4gICogQHBhcmFtIHtGdW5jdGlvbn0gZm4gdGhlIGZ1bmN0aW9uXG4gICogQHJldHVybiB7RnVuY3Rpb259IHRoZSBmdW5jdGlvbiBib3VuZCB0byB0aGUgaW5zdGFuY2VcbiAgKi9cblx0ZnVuY3Rpb24gYmluZFRvSW5zdGFuY2UoZm4pIHtcblx0XHRyZXR1cm4gZm4uYmluZChzZWxmKTtcblx0fVxuXG5cdC8qKlxuICAqIFRoZSBoYW5kbGVyIGZvciBhbGwgZXZlbnRzIHRhcmdldGluZyB0aGUgdGltZSBpbnB1dHNcbiAgKiBAcGFyYW0ge0V2ZW50fSBlIHRoZSBldmVudCAtIFwiaW5wdXRcIiwgXCJ3aGVlbFwiLCBcImluY3JlbWVudFwiLCBldGNcbiAgKi9cblx0ZnVuY3Rpb24gdXBkYXRlVGltZShlKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgJiYgIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpXG5cdFx0XHQvLyBwaWNraW5nIHRpbWUgb25seVxuXHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGYubm93XTtcblxuXHRcdHRpbWVXcmFwcGVyKGUpO1xuXG5cdFx0aWYgKCFzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSByZXR1cm47XG5cblx0XHRpZiAoIXNlbGYubWluRGF0ZUhhc1RpbWUgfHwgZS50eXBlICE9PSBcImlucHV0XCIgfHwgZS50YXJnZXQudmFsdWUubGVuZ3RoID49IDIpIHtcblx0XHRcdHNldEhvdXJzRnJvbUlucHV0cygpO1xuXHRcdFx0dXBkYXRlVmFsdWUoKTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0c2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNldEhvdXJzRnJvbUlucHV0cygpO1xuXHRcdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdFx0fSwgMTAwMCk7XG5cdFx0fVxuXHR9XG5cblx0LyoqXG4gICogU3luY3MgdGhlIHNlbGVjdGVkIGRhdGUgb2JqZWN0IHRpbWUgd2l0aCB1c2VyJ3MgdGltZSBpbnB1dFxuICAqL1xuXHRmdW5jdGlvbiBzZXRIb3Vyc0Zyb21JbnB1dHMoKSB7XG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSByZXR1cm47XG5cblx0XHR2YXIgaG91cnMgPSAocGFyc2VJbnQoc2VsZi5ob3VyRWxlbWVudC52YWx1ZSwgMTApIHx8IDApICUgKHNlbGYuYW1QTSA/IDEyIDogMjQpLFxuXHRcdCAgICBtaW51dGVzID0gKHBhcnNlSW50KHNlbGYubWludXRlRWxlbWVudC52YWx1ZSwgMTApIHx8IDApICUgNjAsXG5cdFx0ICAgIHNlY29uZHMgPSBzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gKHBhcnNlSW50KHNlbGYuc2Vjb25kRWxlbWVudC52YWx1ZSwgMTApIHx8IDApICUgNjAgOiAwO1xuXG5cdFx0aWYgKHNlbGYuYW1QTSAhPT0gdW5kZWZpbmVkKSBob3VycyA9IGhvdXJzICUgMTIgKyAxMiAqIChzZWxmLmFtUE0udGV4dENvbnRlbnQgPT09IFwiUE1cIik7XG5cblx0XHRpZiAoc2VsZi5taW5EYXRlSGFzVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmosIHNlbGYuY29uZmlnLm1pbkRhdGUpID09PSAwKSB7XG5cblx0XHRcdGhvdXJzID0gTWF0aC5tYXgoaG91cnMsIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSk7XG5cdFx0XHRpZiAoaG91cnMgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSkgbWludXRlcyA9IE1hdGgubWF4KG1pbnV0ZXMsIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TWludXRlcygpKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5tYXhEYXRlSGFzVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmosIHNlbGYuY29uZmlnLm1heERhdGUpID09PSAwKSB7XG5cdFx0XHRob3VycyA9IE1hdGgubWluKGhvdXJzLCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkpO1xuXHRcdFx0aWYgKGhvdXJzID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkpIG1pbnV0ZXMgPSBNYXRoLm1pbihtaW51dGVzLCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1pbnV0ZXMoKSk7XG5cdFx0fVxuXG5cdFx0c2V0SG91cnMoaG91cnMsIG1pbnV0ZXMsIHNlY29uZHMpO1xuXHR9XG5cblx0LyoqXG4gICogU3luY3MgdGltZSBpbnB1dCB2YWx1ZXMgd2l0aCBhIGRhdGVcbiAgKiBAcGFyYW0ge0RhdGV9IGRhdGVPYmogdGhlIGRhdGUgdG8gc3luYyB3aXRoXG4gICovXG5cdGZ1bmN0aW9uIHNldEhvdXJzRnJvbURhdGUoZGF0ZU9iaikge1xuXHRcdHZhciBkYXRlID0gZGF0ZU9iaiB8fCBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iajtcblxuXHRcdGlmIChkYXRlKSBzZXRIb3VycyhkYXRlLmdldEhvdXJzKCksIGRhdGUuZ2V0TWludXRlcygpLCBkYXRlLmdldFNlY29uZHMoKSk7XG5cdH1cblxuXHQvKipcbiAgKiBTZXRzIHRoZSBob3VycywgbWludXRlcywgYW5kIG9wdGlvbmFsbHkgc2Vjb25kc1xuICAqIG9mIHRoZSBsYXRlc3Qgc2VsZWN0ZWQgZGF0ZSBvYmplY3QgYW5kIHRoZVxuICAqIGNvcnJlc3BvbmRpbmcgdGltZSBpbnB1dHNcbiAgKiBAcGFyYW0ge051bWJlcn0gaG91cnMgdGhlIGhvdXIuIHdoZXRoZXIgaXRzIG1pbGl0YXJ5XG4gICogICAgICAgICAgICAgICAgIG9yIGFtLXBtIGdldHMgaW5mZXJyZWQgZnJvbSBjb25maWdcbiAgKiBAcGFyYW0ge051bWJlcn0gbWludXRlcyB0aGUgbWludXRlc1xuICAqIEBwYXJhbSB7TnVtYmVyfSBzZWNvbmRzIHRoZSBzZWNvbmRzIChvcHRpb25hbClcbiAgKi9cblx0ZnVuY3Rpb24gc2V0SG91cnMoaG91cnMsIG1pbnV0ZXMsIHNlY29uZHMpIHtcblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkge1xuXHRcdFx0c2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouc2V0SG91cnMoaG91cnMgJSAyNCwgbWludXRlcywgc2Vjb25kcyB8fCAwLCAwKTtcblx0XHR9XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmVuYWJsZVRpbWUgfHwgc2VsZi5pc01vYmlsZSkgcmV0dXJuO1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKCFzZWxmLmNvbmZpZy50aW1lXzI0aHIgPyAoMTIgKyBob3VycykgJSAxMiArIDEyICogKGhvdXJzICUgMTIgPT09IDApIDogaG91cnMpO1xuXG5cdFx0c2VsZi5taW51dGVFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQobWludXRlcyk7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLnRpbWVfMjRocikgc2VsZi5hbVBNLnRleHRDb250ZW50ID0gaG91cnMgPj0gMTIgPyBcIlBNXCIgOiBcIkFNXCI7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA9PT0gdHJ1ZSkgc2VsZi5zZWNvbmRFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoc2Vjb25kcyk7XG5cdH1cblxuXHQvKipcbiAgKiBIYW5kbGVzIHRoZSB5ZWFyIGlucHV0IGFuZCBpbmNyZW1lbnRpbmcgZXZlbnRzXG4gICogQHBhcmFtIHtFdmVudH0gZXZlbnQgdGhlIGtleXVwIG9yIGluY3JlbWVudCBldmVudFxuICAqL1xuXHRmdW5jdGlvbiBvblllYXJJbnB1dChldmVudCkge1xuXHRcdHZhciB5ZWFyID0gZXZlbnQudGFyZ2V0LnZhbHVlO1xuXHRcdGlmIChldmVudC5kZWx0YSkgeWVhciA9IChwYXJzZUludCh5ZWFyKSArIGV2ZW50LmRlbHRhKS50b1N0cmluZygpO1xuXG5cdFx0aWYgKHllYXIubGVuZ3RoID09PSA0IHx8IGV2ZW50LmtleSA9PT0gXCJFbnRlclwiKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5ibHVyKCk7XG5cdFx0XHRpZiAoIS9bXlxcZF0vLnRlc3QoeWVhcikpIGNoYW5nZVllYXIoeWVhcik7XG5cdFx0fVxuXHR9XG5cblx0LyoqXG4gICogRXNzZW50aWFsbHkgYWRkRXZlbnRMaXN0ZW5lciArIHRyYWNraW5nXG4gICogQHBhcmFtIHtFbGVtZW50fSBlbGVtZW50IHRoZSBlbGVtZW50IHRvIGFkZEV2ZW50TGlzdGVuZXIgdG9cbiAgKiBAcGFyYW0ge1N0cmluZ30gZXZlbnQgdGhlIGV2ZW50IG5hbWVcbiAgKiBAcGFyYW0ge0Z1bmN0aW9ufSBoYW5kbGVyIHRoZSBldmVudCBoYW5kbGVyXG4gICovXG5cdGZ1bmN0aW9uIGJpbmQoZWxlbWVudCwgZXZlbnQsIGhhbmRsZXIpIHtcblx0XHRpZiAoZXZlbnQgaW5zdGFuY2VvZiBBcnJheSkgcmV0dXJuIGV2ZW50LmZvckVhY2goZnVuY3Rpb24gKGV2KSB7XG5cdFx0XHRyZXR1cm4gYmluZChlbGVtZW50LCBldiwgaGFuZGxlcik7XG5cdFx0fSk7XG5cblx0XHRpZiAoZWxlbWVudCBpbnN0YW5jZW9mIEFycmF5KSByZXR1cm4gZWxlbWVudC5mb3JFYWNoKGZ1bmN0aW9uIChlbCkge1xuXHRcdFx0cmV0dXJuIGJpbmQoZWwsIGV2ZW50LCBoYW5kbGVyKTtcblx0XHR9KTtcblxuXHRcdGVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihldmVudCwgaGFuZGxlcik7XG5cdFx0c2VsZi5faGFuZGxlcnMucHVzaCh7IGVsZW1lbnQ6IGVsZW1lbnQsIGV2ZW50OiBldmVudCwgaGFuZGxlcjogaGFuZGxlciB9KTtcblx0fVxuXG5cdC8qKlxuICAqIEEgbW91c2Vkb3duIGhhbmRsZXIgd2hpY2ggbWltaWNzIGNsaWNrLlxuICAqIE1pbmltaXplcyBsYXRlbmN5LCBzaW5jZSB3ZSBkb24ndCBuZWVkIHRvIHdhaXQgZm9yIG1vdXNldXAgaW4gbW9zdCBjYXNlcy5cbiAgKiBBbHNvLCBhdm9pZHMgaGFuZGxpbmcgcmlnaHQgY2xpY2tzLlxuICAqXG4gICogQHBhcmFtIHtGdW5jdGlvbn0gaGFuZGxlciB0aGUgZXZlbnQgaGFuZGxlclxuICAqL1xuXHRmdW5jdGlvbiBvbkNsaWNrKGhhbmRsZXIpIHtcblx0XHRyZXR1cm4gZnVuY3Rpb24gKGV2dCkge1xuXHRcdFx0cmV0dXJuIGV2dC53aGljaCA9PT0gMSAmJiBoYW5kbGVyKGV2dCk7XG5cdFx0fTtcblx0fVxuXG5cdC8qKlxuICAqIEFkZHMgYWxsIHRoZSBuZWNlc3NhcnkgZXZlbnQgbGlzdGVuZXJzXG4gICovXG5cdGZ1bmN0aW9uIGJpbmRFdmVudHMoKSB7XG5cdFx0c2VsZi5faGFuZGxlcnMgPSBbXTtcblx0XHRzZWxmLl9hbmltYXRpb25Mb29wID0gW107XG5cdFx0aWYgKHNlbGYuY29uZmlnLndyYXApIHtcblx0XHRcdFtcIm9wZW5cIiwgXCJjbG9zZVwiLCBcInRvZ2dsZVwiLCBcImNsZWFyXCJdLmZvckVhY2goZnVuY3Rpb24gKGV2dCkge1xuXHRcdFx0XHRBcnJheS5wcm90b3R5cGUuZm9yRWFjaC5jYWxsKHNlbGYuZWxlbWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiW2RhdGEtXCIgKyBldnQgKyBcIl1cIiksIGZ1bmN0aW9uIChlbCkge1xuXHRcdFx0XHRcdHJldHVybiBiaW5kKGVsLCBcIm1vdXNlZG93blwiLCBvbkNsaWNrKHNlbGZbZXZ0XSkpO1xuXHRcdFx0XHR9KTtcblx0XHRcdH0pO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmlzTW9iaWxlKSByZXR1cm4gc2V0dXBNb2JpbGUoKTtcblxuXHRcdHNlbGYuZGVib3VuY2VkUmVzaXplID0gZGVib3VuY2Uob25SZXNpemUsIDUwKTtcblx0XHRzZWxmLnRyaWdnZXJDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdFx0fTtcblx0XHRzZWxmLmRlYm91bmNlZENoYW5nZSA9IGRlYm91bmNlKHNlbGYudHJpZ2dlckNoYW5nZSwgMzAwKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIgJiYgc2VsZi5kYXlzQ29udGFpbmVyKSBiaW5kKHNlbGYuZGF5c0NvbnRhaW5lciwgXCJtb3VzZW92ZXJcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHJldHVybiBvbk1vdXNlT3ZlcihlLnRhcmdldCk7XG5cdFx0fSk7XG5cblx0XHRiaW5kKHdpbmRvdy5kb2N1bWVudC5ib2R5LCBcImtleWRvd25cIiwgb25LZXlEb3duKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuc3RhdGljKSBiaW5kKHNlbGYuX2lucHV0LCBcImtleWRvd25cIiwgb25LZXlEb3duKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuaW5saW5lICYmICFzZWxmLmNvbmZpZy5zdGF0aWMpIGJpbmQod2luZG93LCBcInJlc2l6ZVwiLCBzZWxmLmRlYm91bmNlZFJlc2l6ZSk7XG5cblx0XHRpZiAod2luZG93Lm9udG91Y2hzdGFydCAhPT0gdW5kZWZpbmVkKSBiaW5kKHdpbmRvdy5kb2N1bWVudCwgXCJ0b3VjaHN0YXJ0XCIsIGRvY3VtZW50Q2xpY2spO1xuXG5cdFx0YmluZCh3aW5kb3cuZG9jdW1lbnQsIFwibW91c2Vkb3duXCIsIG9uQ2xpY2soZG9jdW1lbnRDbGljaykpO1xuXHRcdGJpbmQoc2VsZi5faW5wdXQsIFwiYmx1clwiLCBkb2N1bWVudENsaWNrKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5jbGlja09wZW5zID09PSB0cnVlKSBiaW5kKHNlbGYuX2lucHV0LCBcImZvY3VzXCIsIHNlbGYub3Blbik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHtcblx0XHRcdHNlbGYubW9udGhOYXYuYWRkRXZlbnRMaXN0ZW5lcihcIndoZWVsXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRcdHJldHVybiBlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHR9KTtcblx0XHRcdGJpbmQoc2VsZi5tb250aE5hdiwgXCJ3aGVlbFwiLCBkZWJvdW5jZShvbk1vbnRoTmF2U2Nyb2xsLCAxMCkpO1xuXHRcdFx0YmluZChzZWxmLm1vbnRoTmF2LCBcIm1vdXNlZG93blwiLCBvbkNsaWNrKG9uTW9udGhOYXZDbGljaykpO1xuXG5cdFx0XHRiaW5kKHNlbGYubW9udGhOYXYsIFtcImtleXVwXCIsIFwiaW5jcmVtZW50XCJdLCBvblllYXJJbnB1dCk7XG5cdFx0XHRiaW5kKHNlbGYuZGF5c0NvbnRhaW5lciwgXCJtb3VzZWRvd25cIiwgb25DbGljayhzZWxlY3REYXRlKSk7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy5hbmltYXRlKSB7XG5cdFx0XHRcdGJpbmQoc2VsZi5kYXlzQ29udGFpbmVyLCBbXCJ3ZWJraXRBbmltYXRpb25FbmRcIiwgXCJhbmltYXRpb25lbmRcIl0sIGFuaW1hdGVEYXlzKTtcblx0XHRcdFx0YmluZChzZWxmLm1vbnRoTmF2LCBbXCJ3ZWJraXRBbmltYXRpb25FbmRcIiwgXCJhbmltYXRpb25lbmRcIl0sIGFuaW1hdGVNb250aHMpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSB7XG5cdFx0XHR2YXIgc2VsVGV4dCA9IGZ1bmN0aW9uIHNlbFRleHQoZSkge1xuXHRcdFx0XHRyZXR1cm4gZS50YXJnZXQuc2VsZWN0KCk7XG5cdFx0XHR9O1xuXHRcdFx0YmluZChzZWxmLnRpbWVDb250YWluZXIsIFtcIndoZWVsXCIsIFwiaW5wdXRcIiwgXCJpbmNyZW1lbnRcIl0sIHVwZGF0ZVRpbWUpO1xuXHRcdFx0YmluZChzZWxmLnRpbWVDb250YWluZXIsIFwibW91c2Vkb3duXCIsIG9uQ2xpY2sodGltZUluY3JlbWVudCkpO1xuXG5cdFx0XHRiaW5kKHNlbGYudGltZUNvbnRhaW5lciwgW1wid2hlZWxcIiwgXCJpbmNyZW1lbnRcIl0sIHNlbGYuZGVib3VuY2VkQ2hhbmdlKTtcblx0XHRcdGJpbmQoc2VsZi50aW1lQ29udGFpbmVyLCBcImlucHV0XCIsIHNlbGYudHJpZ2dlckNoYW5nZSk7XG5cblx0XHRcdGJpbmQoW3NlbGYuaG91ckVsZW1lbnQsIHNlbGYubWludXRlRWxlbWVudF0sIFwiZm9jdXNcIiwgc2VsVGV4dCk7XG5cblx0XHRcdGlmIChzZWxmLnNlY29uZEVsZW1lbnQgIT09IHVuZGVmaW5lZCkgYmluZChzZWxmLnNlY29uZEVsZW1lbnQsIFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRyZXR1cm4gc2VsZi5zZWNvbmRFbGVtZW50LnNlbGVjdCgpO1xuXHRcdFx0fSk7XG5cblx0XHRcdGlmIChzZWxmLmFtUE0gIT09IHVuZGVmaW5lZCkge1xuXHRcdFx0XHRiaW5kKHNlbGYuYW1QTSwgXCJtb3VzZWRvd25cIiwgb25DbGljayhmdW5jdGlvbiAoZSkge1xuXHRcdFx0XHRcdHVwZGF0ZVRpbWUoZSk7XG5cdFx0XHRcdFx0c2VsZi50cmlnZ2VyQ2hhbmdlKGUpO1xuXHRcdFx0XHR9KSk7XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gcHJvY2Vzc1Bvc3REYXlBbmltYXRpb24oKSB7XG5cdFx0Zm9yICh2YXIgaSA9IHNlbGYuX2FuaW1hdGlvbkxvb3AubGVuZ3RoOyBpLS07KSB7XG5cdFx0XHRzZWxmLl9hbmltYXRpb25Mb29wW2ldKCk7XG5cdFx0XHRzZWxmLl9hbmltYXRpb25Mb29wLnNwbGljZShpLCAxKTtcblx0XHR9XG5cdH1cblxuXHQvKipcbiAgKiBSZW1vdmVzIHRoZSBkYXkgY29udGFpbmVyIHRoYXQgc2xpZGVkIG91dCBvZiB2aWV3XG4gICogQHBhcmFtIHtFdmVudH0gZSB0aGUgYW5pbWF0aW9uIGV2ZW50XG4gICovXG5cdGZ1bmN0aW9uIGFuaW1hdGVEYXlzKGUpIHtcblx0XHRpZiAoc2VsZi5kYXlzQ29udGFpbmVyLmNoaWxkTm9kZXMubGVuZ3RoID4gMSkge1xuXHRcdFx0c3dpdGNoIChlLmFuaW1hdGlvbk5hbWUpIHtcblx0XHRcdFx0Y2FzZSBcImZwU2xpZGVMZWZ0XCI6XG5cdFx0XHRcdFx0c2VsZi5kYXlzQ29udGFpbmVyLmxhc3RDaGlsZC5jbGFzc0xpc3QucmVtb3ZlKFwic2xpZGVMZWZ0TmV3XCIpO1xuXHRcdFx0XHRcdHNlbGYuZGF5c0NvbnRhaW5lci5yZW1vdmVDaGlsZChzZWxmLmRheXNDb250YWluZXIuZmlyc3RDaGlsZCk7XG5cdFx0XHRcdFx0c2VsZi5kYXlzID0gc2VsZi5kYXlzQ29udGFpbmVyLmZpcnN0Q2hpbGQ7XG5cdFx0XHRcdFx0cHJvY2Vzc1Bvc3REYXlBbmltYXRpb24oKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJmcFNsaWRlUmlnaHRcIjpcblx0XHRcdFx0XHRzZWxmLmRheXNDb250YWluZXIuZmlyc3RDaGlsZC5jbGFzc0xpc3QucmVtb3ZlKFwic2xpZGVSaWdodE5ld1wiKTtcblx0XHRcdFx0XHRzZWxmLmRheXNDb250YWluZXIucmVtb3ZlQ2hpbGQoc2VsZi5kYXlzQ29udGFpbmVyLmxhc3RDaGlsZCk7XG5cdFx0XHRcdFx0c2VsZi5kYXlzID0gc2VsZi5kYXlzQ29udGFpbmVyLmZpcnN0Q2hpbGQ7XG5cdFx0XHRcdFx0cHJvY2Vzc1Bvc3REYXlBbmltYXRpb24oKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGRlZmF1bHQ6XG5cdFx0XHRcdFx0YnJlYWs7XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0LyoqXG4gICogUmVtb3ZlcyB0aGUgbW9udGggZWxlbWVudCB0aGF0IGFuaW1hdGVkIG91dCBvZiB2aWV3XG4gICogQHBhcmFtIHtFdmVudH0gZSB0aGUgYW5pbWF0aW9uIGV2ZW50XG4gICovXG5cdGZ1bmN0aW9uIGFuaW1hdGVNb250aHMoZSkge1xuXHRcdHN3aXRjaCAoZS5hbmltYXRpb25OYW1lKSB7XG5cdFx0XHRjYXNlIFwiZnBTbGlkZUxlZnROZXdcIjpcblx0XHRcdGNhc2UgXCJmcFNsaWRlUmlnaHROZXdcIjpcblx0XHRcdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoLmNsYXNzTGlzdC5yZW1vdmUoXCJzbGlkZUxlZnROZXdcIik7XG5cdFx0XHRcdHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aC5jbGFzc0xpc3QucmVtb3ZlKFwic2xpZGVSaWdodE5ld1wiKTtcblx0XHRcdFx0dmFyIG5hdiA9IHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aDtcblxuXHRcdFx0XHR3aGlsZSAobmF2Lm5leHRTaWJsaW5nICYmIC9jdXJyLy50ZXN0KG5hdi5uZXh0U2libGluZy5jbGFzc05hbWUpKSB7XG5cdFx0XHRcdFx0c2VsZi5tb250aE5hdi5yZW1vdmVDaGlsZChuYXYubmV4dFNpYmxpbmcpO1xuXHRcdFx0XHR9d2hpbGUgKG5hdi5wcmV2aW91c1NpYmxpbmcgJiYgL2N1cnIvLnRlc3QobmF2LnByZXZpb3VzU2libGluZy5jbGFzc05hbWUpKSB7XG5cdFx0XHRcdFx0c2VsZi5tb250aE5hdi5yZW1vdmVDaGlsZChuYXYucHJldmlvdXNTaWJsaW5nKTtcblx0XHRcdFx0fXNlbGYub2xkQ3VyTW9udGggPSBudWxsO1xuXHRcdFx0XHRicmVhaztcblx0XHR9XG5cdH1cblxuXHQvKipcbiAgKiBTZXQgdGhlIGNhbGVuZGFyIHZpZXcgdG8gYSBwYXJ0aWN1bGFyIGRhdGUuXG4gICogQHBhcmFtIHtEYXRlfSBqdW1wRGF0ZSB0aGUgZGF0ZSB0byBzZXQgdGhlIHZpZXcgdG9cbiAgKi9cblx0ZnVuY3Rpb24ganVtcFRvRGF0ZShqdW1wRGF0ZSkge1xuXHRcdGp1bXBEYXRlID0ganVtcERhdGUgPyBzZWxmLnBhcnNlRGF0ZShqdW1wRGF0ZSkgOiBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiB8fCAoc2VsZi5jb25maWcubWluRGF0ZSA+IHNlbGYubm93ID8gc2VsZi5jb25maWcubWluRGF0ZSA6IHNlbGYuY29uZmlnLm1heERhdGUgJiYgc2VsZi5jb25maWcubWF4RGF0ZSA8IHNlbGYubm93ID8gc2VsZi5jb25maWcubWF4RGF0ZSA6IHNlbGYubm93KTtcblxuXHRcdHRyeSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyID0ganVtcERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0ganVtcERhdGUuZ2V0TW9udGgoKTtcblx0XHR9IGNhdGNoIChlKSB7XG5cdFx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdFx0Y29uc29sZS5lcnJvcihlLnN0YWNrKTtcblx0XHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0XHRjb25zb2xlLndhcm4oXCJJbnZhbGlkIGRhdGUgc3VwcGxpZWQ6IFwiICsganVtcERhdGUpO1xuXHRcdH1cblxuXHRcdHNlbGYucmVkcmF3KCk7XG5cdH1cblxuXHQvKipcbiAgKiBUaGUgdXAvZG93biBhcnJvdyBoYW5kbGVyIGZvciB0aW1lIGlucHV0c1xuICAqIEBwYXJhbSB7RXZlbnR9IGUgdGhlIGNsaWNrIGV2ZW50XG4gICovXG5cdGZ1bmN0aW9uIHRpbWVJbmNyZW1lbnQoZSkge1xuXHRcdGlmICh+ZS50YXJnZXQuY2xhc3NOYW1lLmluZGV4T2YoXCJhcnJvd1wiKSkgaW5jcmVtZW50TnVtSW5wdXQoZSwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwiYXJyb3dVcFwiKSA/IDEgOiAtMSk7XG5cdH1cblxuXHQvKipcbiAgKiBJbmNyZW1lbnRzL2RlY3JlbWVudHMgdGhlIHZhbHVlIG9mIGlucHV0IGFzc29jaS1cbiAgKiBhdGVkIHdpdGggdGhlIHVwL2Rvd24gYXJyb3cgYnkgZGlzcGF0Y2hpbmcgYW5cbiAgKiBcImluY3JlbWVudFwiIGV2ZW50IG9uIHRoZSBpbnB1dC5cbiAgKlxuICAqIEBwYXJhbSB7RXZlbnR9IGUgdGhlIGNsaWNrIGV2ZW50XG4gICogQHBhcmFtIHtOdW1iZXJ9IGRlbHRhIHRoZSBkaWZmICh1c3VhbGx5IDEgb3IgLTEpXG4gICogQHBhcmFtIHtFbGVtZW50fSBpbnB1dEVsZW0gdGhlIGlucHV0IGVsZW1lbnRcbiAgKi9cblx0ZnVuY3Rpb24gaW5jcmVtZW50TnVtSW5wdXQoZSwgZGVsdGEsIGlucHV0RWxlbSkge1xuXHRcdHZhciBpbnB1dCA9IGlucHV0RWxlbSB8fCBlLnRhcmdldC5wYXJlbnROb2RlLmNoaWxkTm9kZXNbMF07XG5cdFx0dmFyIGV2ZW50ID0gY3JlYXRlRXZlbnQoXCJpbmNyZW1lbnRcIik7XG5cdFx0ZXZlbnQuZGVsdGEgPSBkZWx0YTtcblx0XHRpbnB1dC5kaXNwYXRjaEV2ZW50KGV2ZW50KTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNyZWF0ZU51bWJlcklucHV0KGlucHV0Q2xhc3NOYW1lKSB7XG5cdFx0dmFyIHdyYXBwZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwibnVtSW5wdXRXcmFwcGVyXCIpLFxuXHRcdCAgICBudW1JbnB1dCA9IGNyZWF0ZUVsZW1lbnQoXCJpbnB1dFwiLCBcIm51bUlucHV0IFwiICsgaW5wdXRDbGFzc05hbWUpLFxuXHRcdCAgICBhcnJvd1VwID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJhcnJvd1VwXCIpLFxuXHRcdCAgICBhcnJvd0Rvd24gPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImFycm93RG93blwiKTtcblxuXHRcdG51bUlucHV0LnR5cGUgPSBcInRleHRcIjtcblx0XHRudW1JbnB1dC5wYXR0ZXJuID0gXCJcXFxcZCpcIjtcblxuXHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQobnVtSW5wdXQpO1xuXHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoYXJyb3dVcCk7XG5cdFx0d3JhcHBlci5hcHBlbmRDaGlsZChhcnJvd0Rvd24pO1xuXG5cdFx0cmV0dXJuIHdyYXBwZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZCgpIHtcblx0XHR2YXIgZnJhZ21lbnQgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRG9jdW1lbnRGcmFnbWVudCgpO1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLWNhbGVuZGFyXCIpO1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIudGFiSW5kZXggPSAtMTtcblxuXHRcdGlmICghc2VsZi5jb25maWcubm9DYWxlbmRhcikge1xuXHRcdFx0ZnJhZ21lbnQuYXBwZW5kQ2hpbGQoYnVpbGRNb250aE5hdigpKTtcblx0XHRcdHNlbGYuaW5uZXJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLWlubmVyQ29udGFpbmVyXCIpO1xuXG5cdFx0XHRpZiAoc2VsZi5jb25maWcud2Vla051bWJlcnMpIHNlbGYuaW5uZXJDb250YWluZXIuYXBwZW5kQ2hpbGQoYnVpbGRXZWVrcygpKTtcblxuXHRcdFx0c2VsZi5yQ29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1yQ29udGFpbmVyXCIpO1xuXHRcdFx0c2VsZi5yQ29udGFpbmVyLmFwcGVuZENoaWxkKGJ1aWxkV2Vla2RheXMoKSk7XG5cblx0XHRcdGlmICghc2VsZi5kYXlzQ29udGFpbmVyKSB7XG5cdFx0XHRcdHNlbGYuZGF5c0NvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItZGF5c1wiKTtcblx0XHRcdFx0c2VsZi5kYXlzQ29udGFpbmVyLnRhYkluZGV4ID0gLTE7XG5cdFx0XHR9XG5cblx0XHRcdGJ1aWxkRGF5cygpO1xuXHRcdFx0c2VsZi5yQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlbGYuZGF5c0NvbnRhaW5lcik7XG5cblx0XHRcdHNlbGYuaW5uZXJDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VsZi5yQ29udGFpbmVyKTtcblx0XHRcdGZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYuaW5uZXJDb250YWluZXIpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSBmcmFnbWVudC5hcHBlbmRDaGlsZChidWlsZFRpbWUoKSk7XG5cblx0XHR0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcInJhbmdlTW9kZVwiLCBzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpO1xuXHRcdHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwiYW5pbWF0ZVwiLCBzZWxmLmNvbmZpZy5hbmltYXRlKTtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuYXBwZW5kQ2hpbGQoZnJhZ21lbnQpO1xuXG5cdFx0dmFyIGN1c3RvbUFwcGVuZCA9IHNlbGYuY29uZmlnLmFwcGVuZFRvICYmIHNlbGYuY29uZmlnLmFwcGVuZFRvLm5vZGVUeXBlO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmlubGluZSB8fCBzZWxmLmNvbmZpZy5zdGF0aWMpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChzZWxmLmNvbmZpZy5pbmxpbmUgPyBcImlubGluZVwiIDogXCJzdGF0aWNcIik7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUgJiYgIWN1c3RvbUFwcGVuZCkge1xuXHRcdFx0XHRyZXR1cm4gc2VsZi5lbGVtZW50LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIHNlbGYuX2lucHV0Lm5leHRTaWJsaW5nKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuY29uZmlnLnN0YXRpYykge1xuXHRcdFx0XHR2YXIgd3JhcHBlciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd3JhcHBlclwiKTtcblx0XHRcdFx0c2VsZi5lbGVtZW50LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHdyYXBwZXIsIHNlbGYuZWxlbWVudCk7XG5cdFx0XHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoc2VsZi5lbGVtZW50KTtcblxuXHRcdFx0XHRpZiAoc2VsZi5hbHRJbnB1dCkgd3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLmFsdElucHV0KTtcblxuXHRcdFx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKHNlbGYuY2FsZW5kYXJDb250YWluZXIpO1xuXHRcdFx0XHRyZXR1cm47XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0KGN1c3RvbUFwcGVuZCA/IHNlbGYuY29uZmlnLmFwcGVuZFRvIDogd2luZG93LmRvY3VtZW50LmJvZHkpLmFwcGVuZENoaWxkKHNlbGYuY2FsZW5kYXJDb250YWluZXIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY3JlYXRlRGF5KGNsYXNzTmFtZSwgZGF0ZSwgZGF5TnVtYmVyLCBpKSB7XG5cdFx0dmFyIGRhdGVJc0VuYWJsZWQgPSBpc0VuYWJsZWQoZGF0ZSwgdHJ1ZSksXG5cdFx0ICAgIGRheUVsZW1lbnQgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1kYXkgXCIgKyBjbGFzc05hbWUsIGRhdGUuZ2V0RGF0ZSgpKTtcblxuXHRcdGRheUVsZW1lbnQuZGF0ZU9iaiA9IGRhdGU7XG5cdFx0ZGF5RWxlbWVudC4kaSA9IGk7XG5cdFx0ZGF5RWxlbWVudC5zZXRBdHRyaWJ1dGUoXCJhcmlhLWxhYmVsXCIsIHNlbGYuZm9ybWF0RGF0ZShkYXRlLCBzZWxmLmNvbmZpZy5hcmlhRGF0ZUZvcm1hdCkpO1xuXG5cdFx0aWYgKGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLm5vdykgPT09IDApIHtcblx0XHRcdHNlbGYudG9kYXlEYXRlRWxlbSA9IGRheUVsZW1lbnQ7XG5cdFx0XHRkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJ0b2RheVwiKTtcblx0XHR9XG5cblx0XHRpZiAoZGF0ZUlzRW5hYmxlZCkge1xuXHRcdFx0ZGF5RWxlbWVudC50YWJJbmRleCA9IC0xO1xuXHRcdFx0aWYgKGlzRGF0ZVNlbGVjdGVkKGRhdGUpKSB7XG5cdFx0XHRcdGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcInNlbGVjdGVkXCIpO1xuXHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gPSBkYXlFbGVtZW50O1xuXHRcdFx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRcdFx0dG9nZ2xlQ2xhc3MoZGF5RWxlbWVudCwgXCJzdGFydFJhbmdlXCIsIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pID09PSAwKTtcblxuXHRcdFx0XHRcdHRvZ2dsZUNsYXNzKGRheUVsZW1lbnQsIFwiZW5kUmFuZ2VcIiwgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1sxXSkgPT09IDApO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fSBlbHNlIHtcblx0XHRcdGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcImRpc2FibGVkXCIpO1xuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlc1swXSAmJiBkYXRlID4gc2VsZi5taW5SYW5nZURhdGUgJiYgZGF0ZSA8IHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgc2VsZi5taW5SYW5nZURhdGUgPSBkYXRlO2Vsc2UgaWYgKHNlbGYuc2VsZWN0ZWREYXRlc1swXSAmJiBkYXRlIDwgc2VsZi5tYXhSYW5nZURhdGUgJiYgZGF0ZSA+IHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgc2VsZi5tYXhSYW5nZURhdGUgPSBkYXRlO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdGlmIChpc0RhdGVJblJhbmdlKGRhdGUpICYmICFpc0RhdGVTZWxlY3RlZChkYXRlKSkgZGF5RWxlbWVudC5jbGFzc0xpc3QuYWRkKFwiaW5SYW5nZVwiKTtcblxuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEgJiYgKGRhdGUgPCBzZWxmLm1pblJhbmdlRGF0ZSB8fCBkYXRlID4gc2VsZi5tYXhSYW5nZURhdGUpKSBkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJub3RBbGxvd2VkXCIpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycyAmJiBjbGFzc05hbWUgIT09IFwicHJldk1vbnRoRGF5XCIgJiYgZGF5TnVtYmVyICUgNyA9PT0gMSkge1xuXHRcdFx0c2VsZi53ZWVrTnVtYmVycy5pbnNlcnRBZGphY2VudEhUTUwoXCJiZWZvcmVlbmRcIiwgXCI8c3BhbiBjbGFzcz0nZGlzYWJsZWQgZmxhdHBpY2tyLWRheSc+XCIgKyBzZWxmLmNvbmZpZy5nZXRXZWVrKGRhdGUpICsgXCI8L3NwYW4+XCIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIkRheUNyZWF0ZVwiLCBkYXlFbGVtZW50KTtcblxuXHRcdHJldHVybiBkYXlFbGVtZW50O1xuXHR9XG5cblx0ZnVuY3Rpb24gZm9jdXNPbkRheShjdXJyZW50SW5kZXgsIG9mZnNldCkge1xuXHRcdHZhciBuZXdJbmRleCA9IGN1cnJlbnRJbmRleCArIG9mZnNldCB8fCAwLFxuXHRcdCAgICB0YXJnZXROb2RlID0gY3VycmVudEluZGV4ICE9PSB1bmRlZmluZWQgPyBzZWxmLmRheXMuY2hpbGROb2Rlc1tuZXdJbmRleF0gOiBzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gfHwgc2VsZi50b2RheURhdGVFbGVtIHx8IHNlbGYuZGF5cy5jaGlsZE5vZGVzWzBdLFxuXHRcdCAgICBmb2N1cyA9IGZ1bmN0aW9uIGZvY3VzKCkge1xuXHRcdFx0dGFyZ2V0Tm9kZSA9IHRhcmdldE5vZGUgfHwgc2VsZi5kYXlzLmNoaWxkTm9kZXNbbmV3SW5kZXhdO1xuXHRcdFx0dGFyZ2V0Tm9kZS5mb2N1cygpO1xuXG5cdFx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSBvbk1vdXNlT3Zlcih0YXJnZXROb2RlKTtcblx0XHR9O1xuXG5cdFx0aWYgKHRhcmdldE5vZGUgPT09IHVuZGVmaW5lZCAmJiBvZmZzZXQgIT09IDApIHtcblx0XHRcdGlmIChvZmZzZXQgPiAwKSB7XG5cdFx0XHRcdHNlbGYuY2hhbmdlTW9udGgoMSk7XG5cdFx0XHRcdG5ld0luZGV4ID0gbmV3SW5kZXggJSA0Mjtcblx0XHRcdH0gZWxzZSBpZiAob2Zmc2V0IDwgMCkge1xuXHRcdFx0XHRzZWxmLmNoYW5nZU1vbnRoKC0xKTtcblx0XHRcdFx0bmV3SW5kZXggKz0gNDI7XG5cdFx0XHR9XG5cblx0XHRcdHJldHVybiBhZnRlckRheUFuaW0oZm9jdXMpO1xuXHRcdH1cblxuXHRcdGZvY3VzKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBhZnRlckRheUFuaW0oZm4pIHtcblx0XHRpZiAoc2VsZi5jb25maWcuYW5pbWF0ZSA9PT0gdHJ1ZSkgcmV0dXJuIHNlbGYuX2FuaW1hdGlvbkxvb3AucHVzaChmbik7XG5cdFx0Zm4oKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkRGF5cyhkZWx0YSkge1xuXHRcdHZhciBmaXJzdE9mTW9udGggPSAobmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGgsIDEpLmdldERheSgpIC0gc2VsZi5sMTBuLmZpcnN0RGF5T2ZXZWVrICsgNykgJSA3LFxuXHRcdCAgICBpc1JhbmdlTW9kZSA9IHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIjtcblxuXHRcdHNlbGYucHJldk1vbnRoRGF5cyA9IHNlbGYudXRpbHMuZ2V0RGF5c2luTW9udGgoKHNlbGYuY3VycmVudE1vbnRoIC0gMSArIDEyKSAlIDEyKTtcblx0XHRzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gPSB1bmRlZmluZWQ7XG5cdFx0c2VsZi50b2RheURhdGVFbGVtID0gdW5kZWZpbmVkO1xuXG5cdFx0dmFyIGRheXNJbk1vbnRoID0gc2VsZi51dGlscy5nZXREYXlzaW5Nb250aCgpLFxuXHRcdCAgICBkYXlzID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZURvY3VtZW50RnJhZ21lbnQoKTtcblxuXHRcdHZhciBkYXlOdW1iZXIgPSBzZWxmLnByZXZNb250aERheXMgKyAxIC0gZmlyc3RPZk1vbnRoLFxuXHRcdCAgICBkYXlJbmRleCA9IDA7XG5cblx0XHRpZiAoc2VsZi5jb25maWcud2Vla051bWJlcnMgJiYgc2VsZi53ZWVrTnVtYmVycy5maXJzdENoaWxkKSBzZWxmLndlZWtOdW1iZXJzLnRleHRDb250ZW50ID0gXCJcIjtcblxuXHRcdGlmIChpc1JhbmdlTW9kZSkge1xuXHRcdFx0Ly8gY29uc3QgZGF0ZUxpbWl0cyA9IHNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggfHwgc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGggfHwgc2VsZi5jb25maWcubWl4RGF0ZSB8fCBzZWxmLmNvbmZpZy5tYXhEYXRlO1xuXHRcdFx0c2VsZi5taW5SYW5nZURhdGUgPSBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCAtIDEsIGRheU51bWJlcik7XG5cdFx0XHRzZWxmLm1heFJhbmdlRGF0ZSA9IG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoICsgMSwgKDQyIC0gZmlyc3RPZk1vbnRoKSAlIGRheXNJbk1vbnRoKTtcblx0XHR9XG5cblx0XHQvLyBwcmVwZW5kIGRheXMgZnJvbSB0aGUgZW5kaW5nIG9mIHByZXZpb3VzIG1vbnRoXG5cdFx0Zm9yICg7IGRheU51bWJlciA8PSBzZWxmLnByZXZNb250aERheXM7IGRheU51bWJlcisrLCBkYXlJbmRleCsrKSB7XG5cdFx0XHRkYXlzLmFwcGVuZENoaWxkKGNyZWF0ZURheShcInByZXZNb250aERheVwiLCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCAtIDEsIGRheU51bWJlciksIGRheU51bWJlciwgZGF5SW5kZXgpKTtcblx0XHR9XG5cblx0XHQvLyBTdGFydCBhdCAxIHNpbmNlIHRoZXJlIGlzIG5vIDB0aCBkYXlcblx0XHRmb3IgKGRheU51bWJlciA9IDE7IGRheU51bWJlciA8PSBkYXlzSW5Nb250aDsgZGF5TnVtYmVyKyssIGRheUluZGV4KyspIHtcblx0XHRcdGRheXMuYXBwZW5kQ2hpbGQoY3JlYXRlRGF5KFwiXCIsIG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoLCBkYXlOdW1iZXIpLCBkYXlOdW1iZXIsIGRheUluZGV4KSk7XG5cdFx0fVxuXG5cdFx0Ly8gYXBwZW5kIGRheXMgZnJvbSB0aGUgbmV4dCBtb250aFxuXHRcdGZvciAodmFyIGRheU51bSA9IGRheXNJbk1vbnRoICsgMTsgZGF5TnVtIDw9IDQyIC0gZmlyc3RPZk1vbnRoOyBkYXlOdW0rKywgZGF5SW5kZXgrKykge1xuXHRcdFx0ZGF5cy5hcHBlbmRDaGlsZChjcmVhdGVEYXkoXCJuZXh0TW9udGhEYXlcIiwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggKyAxLCBkYXlOdW0gJSBkYXlzSW5Nb250aCksIGRheU51bSwgZGF5SW5kZXgpKTtcblx0XHR9XG5cblx0XHRpZiAoaXNSYW5nZU1vZGUgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSAmJiBkYXlzLmNoaWxkTm9kZXNbMF0pIHtcblx0XHRcdHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyA9IHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyB8fCBzZWxmLm1pblJhbmdlRGF0ZSA+IGRheXMuY2hpbGROb2Rlc1swXS5kYXRlT2JqO1xuXG5cdFx0XHRzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgfHwgc2VsZi5tYXhSYW5nZURhdGUgPCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsIDEpO1xuXHRcdH0gZWxzZSB1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cblx0XHR2YXIgZGF5Q29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImRheUNvbnRhaW5lclwiKTtcblx0XHRkYXlDb250YWluZXIuYXBwZW5kQ2hpbGQoZGF5cyk7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmFuaW1hdGUgfHwgZGVsdGEgPT09IHVuZGVmaW5lZCkgY2xlYXJOb2RlKHNlbGYuZGF5c0NvbnRhaW5lcik7ZWxzZSB7XG5cdFx0XHR3aGlsZSAoc2VsZi5kYXlzQ29udGFpbmVyLmNoaWxkTm9kZXMubGVuZ3RoID4gMSkge1xuXHRcdFx0XHRzZWxmLmRheXNDb250YWluZXIucmVtb3ZlQ2hpbGQoc2VsZi5kYXlzQ29udGFpbmVyLmZpcnN0Q2hpbGQpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdGlmIChkZWx0YSA+PSAwKSBzZWxmLmRheXNDb250YWluZXIuYXBwZW5kQ2hpbGQoZGF5Q29udGFpbmVyKTtlbHNlIHNlbGYuZGF5c0NvbnRhaW5lci5pbnNlcnRCZWZvcmUoZGF5Q29udGFpbmVyLCBzZWxmLmRheXNDb250YWluZXIuZmlyc3RDaGlsZCk7XG5cblx0XHRzZWxmLmRheXMgPSBzZWxmLmRheXNDb250YWluZXIuZmlyc3RDaGlsZDtcblx0XHRyZXR1cm4gc2VsZi5kYXlzQ29udGFpbmVyO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2xlYXJOb2RlKG5vZGUpIHtcblx0XHR3aGlsZSAobm9kZS5maXJzdENoaWxkKSB7XG5cdFx0XHRub2RlLnJlbW92ZUNoaWxkKG5vZGUuZmlyc3RDaGlsZCk7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGRNb250aE5hdigpIHtcblx0XHR2YXIgbW9udGhOYXZGcmFnbWVudCA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVEb2N1bWVudEZyYWdtZW50KCk7XG5cdFx0c2VsZi5tb250aE5hdiA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItbW9udGhcIik7XG5cblx0XHRzZWxmLnByZXZNb250aE5hdiA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXByZXYtbW9udGhcIik7XG5cdFx0c2VsZi5wcmV2TW9udGhOYXYuaW5uZXJIVE1MID0gc2VsZi5jb25maWcucHJldkFycm93O1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50ID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJjdXItbW9udGhcIik7XG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LnRpdGxlID0gc2VsZi5sMTBuLnNjcm9sbFRpdGxlO1xuXG5cdFx0dmFyIHllYXJJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiY3VyLXllYXJcIik7XG5cdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQgPSB5ZWFySW5wdXQuY2hpbGROb2Rlc1swXTtcblx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC50aXRsZSA9IHNlbGYubDEwbi5zY3JvbGxUaXRsZTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5taW5EYXRlKSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5taW4gPSBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWF4RGF0ZSkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWF4ID0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpO1xuXG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5kaXNhYmxlZCA9IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0fVxuXG5cdFx0c2VsZi5uZXh0TW9udGhOYXYgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1uZXh0LW1vbnRoXCIpO1xuXHRcdHNlbGYubmV4dE1vbnRoTmF2LmlubmVySFRNTCA9IHNlbGYuY29uZmlnLm5leHRBcnJvdztcblxuXHRcdHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aCA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLWN1cnJlbnQtbW9udGhcIik7XG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoLmFwcGVuZENoaWxkKHNlbGYuY3VycmVudE1vbnRoRWxlbWVudCk7XG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoLmFwcGVuZENoaWxkKHllYXJJbnB1dCk7XG5cblx0XHRtb250aE5hdkZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYucHJldk1vbnRoTmF2KTtcblx0XHRtb250aE5hdkZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aCk7XG5cdFx0bW9udGhOYXZGcmFnbWVudC5hcHBlbmRDaGlsZChzZWxmLm5leHRNb250aE5hdik7XG5cdFx0c2VsZi5tb250aE5hdi5hcHBlbmRDaGlsZChtb250aE5hdkZyYWdtZW50KTtcblxuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcIl9oaWRlUHJldk1vbnRoQXJyb3dcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiB0aGlzLl9faGlkZVByZXZNb250aEFycm93O1xuXHRcdFx0fSxcblx0XHRcdHNldDogZnVuY3Rpb24gc2V0KGJvb2wpIHtcblx0XHRcdFx0aWYgKHRoaXMuX19oaWRlUHJldk1vbnRoQXJyb3cgIT09IGJvb2wpIHNlbGYucHJldk1vbnRoTmF2LnN0eWxlLmRpc3BsYXkgPSBib29sID8gXCJub25lXCIgOiBcImJsb2NrXCI7XG5cdFx0XHRcdHRoaXMuX19oaWRlUHJldk1vbnRoQXJyb3cgPSBib29sO1xuXHRcdFx0fVxuXHRcdH0pO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwiX2hpZGVOZXh0TW9udGhBcnJvd1wiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX19oaWRlTmV4dE1vbnRoQXJyb3c7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRpZiAodGhpcy5fX2hpZGVOZXh0TW9udGhBcnJvdyAhPT0gYm9vbCkgc2VsZi5uZXh0TW9udGhOYXYuc3R5bGUuZGlzcGxheSA9IGJvb2wgPyBcIm5vbmVcIiA6IFwiYmxvY2tcIjtcblx0XHRcdFx0dGhpcy5fX2hpZGVOZXh0TW9udGhBcnJvdyA9IGJvb2w7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cblx0XHRyZXR1cm4gc2VsZi5tb250aE5hdjtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkVGltZSgpIHtcblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJoYXNUaW1lXCIpO1xuXHRcdGlmIChzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJub0NhbGVuZGFyXCIpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItdGltZVwiKTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIudGFiSW5kZXggPSAtMTtcblx0XHR2YXIgc2VwYXJhdG9yID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItdGltZS1zZXBhcmF0b3JcIiwgXCI6XCIpO1xuXG5cdFx0dmFyIGhvdXJJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiZmxhdHBpY2tyLWhvdXJcIik7XG5cdFx0c2VsZi5ob3VyRWxlbWVudCA9IGhvdXJJbnB1dC5jaGlsZE5vZGVzWzBdO1xuXG5cdFx0dmFyIG1pbnV0ZUlucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJmbGF0cGlja3ItbWludXRlXCIpO1xuXHRcdHNlbGYubWludXRlRWxlbWVudCA9IG1pbnV0ZUlucHV0LmNoaWxkTm9kZXNbMF07XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnRhYkluZGV4ID0gc2VsZi5taW51dGVFbGVtZW50LnRhYkluZGV4ID0gLTE7XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPyBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRIb3VycygpIDogc2VsZi5jb25maWcuZGVmYXVsdEhvdXIpO1xuXG5cdFx0c2VsZi5taW51dGVFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPyBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRNaW51dGVzKCkgOiBzZWxmLmNvbmZpZy5kZWZhdWx0TWludXRlKTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQuc3RlcCA9IHNlbGYuY29uZmlnLmhvdXJJbmNyZW1lbnQ7XG5cdFx0c2VsZi5taW51dGVFbGVtZW50LnN0ZXAgPSBzZWxmLmNvbmZpZy5taW51dGVJbmNyZW1lbnQ7XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50Lm1pbiA9IHNlbGYuY29uZmlnLnRpbWVfMjRociA/IDAgOiAxO1xuXHRcdHNlbGYuaG91ckVsZW1lbnQubWF4ID0gc2VsZi5jb25maWcudGltZV8yNGhyID8gMjMgOiAxMjtcblxuXHRcdHNlbGYubWludXRlRWxlbWVudC5taW4gPSAwO1xuXHRcdHNlbGYubWludXRlRWxlbWVudC5tYXggPSA1OTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudGl0bGUgPSBzZWxmLm1pbnV0ZUVsZW1lbnQudGl0bGUgPSBzZWxmLmwxMG4uc2Nyb2xsVGl0bGU7XG5cblx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoaG91cklucHV0KTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VwYXJhdG9yKTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQobWludXRlSW5wdXQpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLnRpbWVfMjRocikgc2VsZi50aW1lQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJ0aW1lMjRoclwiKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzKSB7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuY2xhc3NMaXN0LmFkZChcImhhc1NlY29uZHNcIik7XG5cblx0XHRcdHZhciBzZWNvbmRJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiZmxhdHBpY2tyLXNlY29uZFwiKTtcblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudCA9IHNlY29uZElucHV0LmNoaWxkTm9kZXNbMF07XG5cblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC52YWx1ZSA9IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID8gc2VsZi5wYWQoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouZ2V0U2Vjb25kcygpKSA6IFwiMDBcIjtcblxuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50LnN0ZXAgPSBzZWxmLm1pbnV0ZUVsZW1lbnQuc3RlcDtcblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC5taW4gPSBzZWxmLm1pbnV0ZUVsZW1lbnQubWluO1xuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50Lm1heCA9IHNlbGYubWludXRlRWxlbWVudC5tYXg7XG5cblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci10aW1lLXNlcGFyYXRvclwiLCBcIjpcIikpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlY29uZElucHV0KTtcblx0XHR9XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLnRpbWVfMjRocikge1xuXHRcdFx0Ly8gYWRkIHNlbGYuYW1QTSBpZiBhcHByb3ByaWF0ZVxuXHRcdFx0c2VsZi5hbVBNID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItYW0tcG1cIiwgW1wiQU1cIiwgXCJQTVwiXVtzZWxmLmhvdXJFbGVtZW50LnZhbHVlID4gMTEgfCAwXSk7XG5cdFx0XHRzZWxmLmFtUE0udGl0bGUgPSBzZWxmLmwxMG4udG9nZ2xlVGl0bGU7XG5cdFx0XHRzZWxmLmFtUE0udGFiSW5kZXggPSAtMTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChzZWxmLmFtUE0pO1xuXHRcdH1cblxuXHRcdHJldHVybiBzZWxmLnRpbWVDb250YWluZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZFdlZWtkYXlzKCkge1xuXHRcdGlmICghc2VsZi53ZWVrZGF5Q29udGFpbmVyKSBzZWxmLndlZWtkYXlDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXdlZWtkYXlzXCIpO1xuXG5cdFx0dmFyIGZpcnN0RGF5T2ZXZWVrID0gc2VsZi5sMTBuLmZpcnN0RGF5T2ZXZWVrO1xuXHRcdHZhciB3ZWVrZGF5cyA9IHNlbGYubDEwbi53ZWVrZGF5cy5zaG9ydGhhbmQuc2xpY2UoKTtcblxuXHRcdGlmIChmaXJzdERheU9mV2VlayA+IDAgJiYgZmlyc3REYXlPZldlZWsgPCB3ZWVrZGF5cy5sZW5ndGgpIHtcblx0XHRcdHdlZWtkYXlzID0gW10uY29uY2F0KHdlZWtkYXlzLnNwbGljZShmaXJzdERheU9mV2Vlaywgd2Vla2RheXMubGVuZ3RoKSwgd2Vla2RheXMuc3BsaWNlKDAsIGZpcnN0RGF5T2ZXZWVrKSk7XG5cdFx0fVxuXG5cdFx0c2VsZi53ZWVrZGF5Q29udGFpbmVyLmlubmVySFRNTCA9IFwiXFxuXFx0XFx0PHNwYW4gY2xhc3M9ZmxhdHBpY2tyLXdlZWtkYXk+XFxuXFx0XFx0XFx0XCIgKyB3ZWVrZGF5cy5qb2luKFwiPC9zcGFuPjxzcGFuIGNsYXNzPWZsYXRwaWNrci13ZWVrZGF5PlwiKSArIFwiXFxuXFx0XFx0PC9zcGFuPlxcblxcdFxcdFwiO1xuXG5cdFx0cmV0dXJuIHNlbGYud2Vla2RheUNvbnRhaW5lcjtcblx0fVxuXG5cdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdGZ1bmN0aW9uIGJ1aWxkV2Vla3MoKSB7XG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwiaGFzV2Vla3NcIik7XG5cdFx0c2VsZi53ZWVrV3JhcHBlciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd2Vla3dyYXBwZXJcIik7XG5cdFx0c2VsZi53ZWVrV3JhcHBlci5hcHBlbmRDaGlsZChjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci13ZWVrZGF5XCIsIHNlbGYubDEwbi53ZWVrQWJicmV2aWF0aW9uKSk7XG5cdFx0c2VsZi53ZWVrTnVtYmVycyA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd2Vla3NcIik7XG5cdFx0c2VsZi53ZWVrV3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLndlZWtOdW1iZXJzKTtcblxuXHRcdHJldHVybiBzZWxmLndlZWtXcmFwcGVyO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2hhbmdlTW9udGgodmFsdWUsIGlzX29mZnNldCwgYW5pbWF0ZSkge1xuXHRcdGlzX29mZnNldCA9IGlzX29mZnNldCA9PT0gdW5kZWZpbmVkIHx8IGlzX29mZnNldDtcblx0XHR2YXIgZGVsdGEgPSBpc19vZmZzZXQgPyB2YWx1ZSA6IHZhbHVlIC0gc2VsZi5jdXJyZW50TW9udGg7XG5cdFx0dmFyIHNraXBBbmltYXRpb25zID0gIXNlbGYuY29uZmlnLmFuaW1hdGUgfHwgYW5pbWF0ZSA9PT0gZmFsc2U7XG5cblx0XHRpZiAoZGVsdGEgPCAwICYmIHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyB8fCBkZWx0YSA+IDAgJiYgc2VsZi5faGlkZU5leHRNb250aEFycm93KSByZXR1cm47XG5cblx0XHRzZWxmLmN1cnJlbnRNb250aCArPSBkZWx0YTtcblxuXHRcdGlmIChzZWxmLmN1cnJlbnRNb250aCA8IDAgfHwgc2VsZi5jdXJyZW50TW9udGggPiAxMSkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhciArPSBzZWxmLmN1cnJlbnRNb250aCA+IDExID8gMSA6IC0xO1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSAoc2VsZi5jdXJyZW50TW9udGggKyAxMikgJSAxMjtcblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiWWVhckNoYW5nZVwiKTtcblx0XHR9XG5cblx0XHRidWlsZERheXMoIXNraXBBbmltYXRpb25zID8gZGVsdGEgOiB1bmRlZmluZWQpO1xuXG5cdFx0aWYgKHNraXBBbmltYXRpb25zKSB7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJNb250aENoYW5nZVwiKTtcblx0XHRcdHJldHVybiB1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cdFx0fVxuXG5cdFx0Ly8gcmVtb3ZlIHBvc3NpYmxlIHJlbW5hbnRzIGZyb20gY2xpY2tpbmcgdG9vIGZhc3Rcblx0XHR2YXIgbmF2ID0gc2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoO1xuXHRcdGlmIChkZWx0YSA8IDApIHtcblx0XHRcdHdoaWxlIChuYXYubmV4dFNpYmxpbmcgJiYgL2N1cnIvLnRlc3QobmF2Lm5leHRTaWJsaW5nLmNsYXNzTmFtZSkpIHtcblx0XHRcdFx0c2VsZi5tb250aE5hdi5yZW1vdmVDaGlsZChuYXYubmV4dFNpYmxpbmcpO1xuXHRcdFx0fVxuXHRcdH0gZWxzZSBpZiAoZGVsdGEgPiAwKSB7XG5cdFx0XHR3aGlsZSAobmF2LnByZXZpb3VzU2libGluZyAmJiAvY3Vyci8udGVzdChuYXYucHJldmlvdXNTaWJsaW5nLmNsYXNzTmFtZSkpIHtcblx0XHRcdFx0c2VsZi5tb250aE5hdi5yZW1vdmVDaGlsZChuYXYucHJldmlvdXNTaWJsaW5nKTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRzZWxmLm9sZEN1ck1vbnRoID0gc2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoO1xuXG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoID0gc2VsZi5tb250aE5hdi5pbnNlcnRCZWZvcmUoc2VsZi5vbGRDdXJNb250aC5jbG9uZU5vZGUodHJ1ZSksIGRlbHRhID4gMCA/IHNlbGYub2xkQ3VyTW9udGgubmV4dFNpYmxpbmcgOiBzZWxmLm9sZEN1ck1vbnRoKTtcblxuXHRcdGlmIChkZWx0YSA+IDApIHtcblx0XHRcdHNlbGYuZGF5c0NvbnRhaW5lci5maXJzdENoaWxkLmNsYXNzTGlzdC5hZGQoXCJzbGlkZUxlZnRcIik7XG5cdFx0XHRzZWxmLmRheXNDb250YWluZXIubGFzdENoaWxkLmNsYXNzTGlzdC5hZGQoXCJzbGlkZUxlZnROZXdcIik7XG5cblx0XHRcdHNlbGYub2xkQ3VyTW9udGguY2xhc3NMaXN0LmFkZChcInNsaWRlTGVmdFwiKTtcblx0XHRcdHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aC5jbGFzc0xpc3QuYWRkKFwic2xpZGVMZWZ0TmV3XCIpO1xuXHRcdH0gZWxzZSBpZiAoZGVsdGEgPCAwKSB7XG5cdFx0XHRzZWxmLmRheXNDb250YWluZXIuZmlyc3RDaGlsZC5jbGFzc0xpc3QuYWRkKFwic2xpZGVSaWdodE5ld1wiKTtcblx0XHRcdHNlbGYuZGF5c0NvbnRhaW5lci5sYXN0Q2hpbGQuY2xhc3NMaXN0LmFkZChcInNsaWRlUmlnaHRcIik7XG5cblx0XHRcdHNlbGYub2xkQ3VyTW9udGguY2xhc3NMaXN0LmFkZChcInNsaWRlUmlnaHRcIik7XG5cdFx0XHRzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGguY2xhc3NMaXN0LmFkZChcInNsaWRlUmlnaHROZXdcIik7XG5cdFx0fVxuXG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50ID0gc2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoLmZpcnN0Q2hpbGQ7XG5cdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQgPSBzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGgubGFzdENoaWxkLmNoaWxkTm9kZXNbMF07XG5cblx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cdFx0c2VsZi5vbGRDdXJNb250aC5maXJzdENoaWxkLnRleHRDb250ZW50ID0gc2VsZi51dGlscy5tb250aFRvU3RyKHNlbGYuY3VycmVudE1vbnRoIC0gZGVsdGEpO1xuXG5cdFx0dHJpZ2dlckV2ZW50KFwiTW9udGhDaGFuZ2VcIik7XG5cblx0XHRpZiAoZG9jdW1lbnQuYWN0aXZlRWxlbWVudCAmJiBkb2N1bWVudC5hY3RpdmVFbGVtZW50LiRpKSB7XG5cdFx0XHR2YXIgaW5kZXggPSBkb2N1bWVudC5hY3RpdmVFbGVtZW50LiRpO1xuXHRcdFx0YWZ0ZXJEYXlBbmltKGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0Zm9jdXNPbkRheShpbmRleCwgMCk7XG5cdFx0XHR9KTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBjbGVhcih0cmlnZ2VyQ2hhbmdlRXZlbnQpIHtcblx0XHRzZWxmLmlucHV0LnZhbHVlID0gXCJcIjtcblxuXHRcdGlmIChzZWxmLmFsdElucHV0KSBzZWxmLmFsdElucHV0LnZhbHVlID0gXCJcIjtcblxuXHRcdGlmIChzZWxmLm1vYmlsZUlucHV0KSBzZWxmLm1vYmlsZUlucHV0LnZhbHVlID0gXCJcIjtcblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtdO1xuXHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gdW5kZWZpbmVkO1xuXHRcdHNlbGYuc2hvd1RpbWVJbnB1dCA9IGZhbHNlO1xuXG5cdFx0c2VsZi5yZWRyYXcoKTtcblxuXHRcdGlmICh0cmlnZ2VyQ2hhbmdlRXZlbnQgIT09IGZhbHNlKVxuXHRcdFx0Ly8gdHJpZ2dlckNoYW5nZUV2ZW50IGlzIHRydWUgKGRlZmF1bHQpIG9yIGFuIEV2ZW50XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBjbG9zZSgpIHtcblx0XHRzZWxmLmlzT3BlbiA9IGZhbHNlO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5yZW1vdmUoXCJvcGVuXCIpO1xuXHRcdFx0c2VsZi5faW5wdXQuY2xhc3NMaXN0LnJlbW92ZShcImFjdGl2ZVwiKTtcblx0XHR9XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJDbG9zZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGRlc3Ryb3koKSB7XG5cdFx0Zm9yICh2YXIgaSA9IHNlbGYuX2hhbmRsZXJzLmxlbmd0aDsgaS0tOykge1xuXHRcdFx0dmFyIGggPSBzZWxmLl9oYW5kbGVyc1tpXTtcblx0XHRcdGguZWxlbWVudC5yZW1vdmVFdmVudExpc3RlbmVyKGguZXZlbnQsIGguaGFuZGxlcik7XG5cdFx0fVxuXG5cdFx0c2VsZi5faGFuZGxlcnMgPSBbXTtcblxuXHRcdGlmIChzZWxmLm1vYmlsZUlucHV0KSB7XG5cdFx0XHRpZiAoc2VsZi5tb2JpbGVJbnB1dC5wYXJlbnROb2RlKSBzZWxmLm1vYmlsZUlucHV0LnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoc2VsZi5tb2JpbGVJbnB1dCk7XG5cdFx0XHRzZWxmLm1vYmlsZUlucHV0ID0gbnVsbDtcblx0XHR9IGVsc2UgaWYgKHNlbGYuY2FsZW5kYXJDb250YWluZXIgJiYgc2VsZi5jYWxlbmRhckNvbnRhaW5lci5wYXJlbnROb2RlKSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoc2VsZi5jYWxlbmRhckNvbnRhaW5lcik7XG5cblx0XHRpZiAoc2VsZi5hbHRJbnB1dCkge1xuXHRcdFx0c2VsZi5pbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0XHRpZiAoc2VsZi5hbHRJbnB1dC5wYXJlbnROb2RlKSBzZWxmLmFsdElucHV0LnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoc2VsZi5hbHRJbnB1dCk7XG5cdFx0XHRkZWxldGUgc2VsZi5hbHRJbnB1dDtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5pbnB1dCkge1xuXHRcdFx0c2VsZi5pbnB1dC50eXBlID0gc2VsZi5pbnB1dC5fdHlwZTtcblx0XHRcdHNlbGYuaW5wdXQuY2xhc3NMaXN0LnJlbW92ZShcImZsYXRwaWNrci1pbnB1dFwiKTtcblx0XHRcdHNlbGYuaW5wdXQucmVtb3ZlQXR0cmlidXRlKFwicmVhZG9ubHlcIik7XG5cdFx0XHRzZWxmLmlucHV0LnZhbHVlID0gXCJcIjtcblx0XHR9XG5cblx0XHRbXCJfc2hvd1RpbWVJbnB1dFwiLCBcImxhdGVzdFNlbGVjdGVkRGF0ZU9ialwiLCBcIl9oaWRlTmV4dE1vbnRoQXJyb3dcIiwgXCJfaGlkZVByZXZNb250aEFycm93XCIsIFwiX19oaWRlTmV4dE1vbnRoQXJyb3dcIiwgXCJfX2hpZGVQcmV2TW9udGhBcnJvd1wiLCBcImlzTW9iaWxlXCIsIFwiaXNPcGVuXCIsIFwic2VsZWN0ZWREYXRlRWxlbVwiLCBcIm1pbkRhdGVIYXNUaW1lXCIsIFwibWF4RGF0ZUhhc1RpbWVcIiwgXCJkYXlzXCIsIFwiZGF5c0NvbnRhaW5lclwiLCBcIl9pbnB1dFwiLCBcIl9wb3NpdGlvbkVsZW1lbnRcIiwgXCJpbm5lckNvbnRhaW5lclwiLCBcInJDb250YWluZXJcIiwgXCJtb250aE5hdlwiLCBcInRvZGF5RGF0ZUVsZW1cIiwgXCJjYWxlbmRhckNvbnRhaW5lclwiLCBcIndlZWtkYXlDb250YWluZXJcIiwgXCJwcmV2TW9udGhOYXZcIiwgXCJuZXh0TW9udGhOYXZcIiwgXCJjdXJyZW50TW9udGhFbGVtZW50XCIsIFwiY3VycmVudFllYXJFbGVtZW50XCIsIFwibmF2aWdhdGlvbkN1cnJlbnRNb250aFwiLCBcInNlbGVjdGVkRGF0ZUVsZW1cIiwgXCJjb25maWdcIl0uZm9yRWFjaChmdW5jdGlvbiAoaykge1xuXHRcdFx0cmV0dXJuIGRlbGV0ZSBzZWxmW2tdO1xuXHRcdH0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gaXNDYWxlbmRhckVsZW0oZWxlbSkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5hcHBlbmRUbyAmJiBzZWxmLmNvbmZpZy5hcHBlbmRUby5jb250YWlucyhlbGVtKSkgcmV0dXJuIHRydWU7XG5cblx0XHRyZXR1cm4gc2VsZi5jYWxlbmRhckNvbnRhaW5lci5jb250YWlucyhlbGVtKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGRvY3VtZW50Q2xpY2soZSkge1xuXHRcdGlmIChzZWxmLmlzT3BlbiAmJiAhc2VsZi5jb25maWcuaW5saW5lKSB7XG5cdFx0XHR2YXIgaXNDYWxlbmRhckVsZW1lbnQgPSBpc0NhbGVuZGFyRWxlbShlLnRhcmdldCk7XG5cdFx0XHR2YXIgaXNJbnB1dCA9IGUudGFyZ2V0ID09PSBzZWxmLmlucHV0IHx8IGUudGFyZ2V0ID09PSBzZWxmLmFsdElucHV0IHx8IHNlbGYuZWxlbWVudC5jb250YWlucyhlLnRhcmdldCkgfHxcblx0XHRcdC8vIHdlYiBjb21wb25lbnRzXG5cdFx0XHRlLnBhdGggJiYgZS5wYXRoLmluZGV4T2YgJiYgKH5lLnBhdGguaW5kZXhPZihzZWxmLmlucHV0KSB8fCB+ZS5wYXRoLmluZGV4T2Yoc2VsZi5hbHRJbnB1dCkpO1xuXG5cdFx0XHR2YXIgbG9zdEZvY3VzID0gZS50eXBlID09PSBcImJsdXJcIiA/IGlzSW5wdXQgJiYgZS5yZWxhdGVkVGFyZ2V0ICYmICFpc0NhbGVuZGFyRWxlbShlLnJlbGF0ZWRUYXJnZXQpIDogIWlzSW5wdXQgJiYgIWlzQ2FsZW5kYXJFbGVtZW50O1xuXG5cdFx0XHRpZiAobG9zdEZvY3VzKSB7XG5cdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0c2VsZi5jbG9zZSgpO1xuXG5cdFx0XHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSkge1xuXHRcdFx0XHRcdHNlbGYuY2xlYXIoZmFsc2UpO1xuXHRcdFx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBjaGFuZ2VZZWFyKG5ld1llYXIpIHtcblx0XHRpZiAoIW5ld1llYXIgfHwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWluICYmIG5ld1llYXIgPCBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5taW4gfHwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWF4ICYmIG5ld1llYXIgPiBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5tYXgpIHJldHVybjtcblxuXHRcdHZhciBuZXdZZWFyTnVtID0gcGFyc2VJbnQobmV3WWVhciwgMTApLFxuXHRcdCAgICBpc05ld1llYXIgPSBzZWxmLmN1cnJlbnRZZWFyICE9PSBuZXdZZWFyTnVtO1xuXG5cdFx0c2VsZi5jdXJyZW50WWVhciA9IG5ld1llYXJOdW0gfHwgc2VsZi5jdXJyZW50WWVhcjtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlICYmIHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSkge1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSBNYXRoLm1pbihzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1vbnRoKCksIHNlbGYuY3VycmVudE1vbnRoKTtcblx0XHR9IGVsc2UgaWYgKHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IE1hdGgubWF4KHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TW9udGgoKSwgc2VsZi5jdXJyZW50TW9udGgpO1xuXHRcdH1cblxuXHRcdGlmIChpc05ld1llYXIpIHtcblx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJZZWFyQ2hhbmdlXCIpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGlzRW5hYmxlZChkYXRlLCB0aW1lbGVzcykge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5taW5EYXRlICYmIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLmNvbmZpZy5taW5EYXRlLCB0aW1lbGVzcyAhPT0gdW5kZWZpbmVkID8gdGltZWxlc3MgOiAhc2VsZi5taW5EYXRlSGFzVGltZSkgPCAwIHx8IHNlbGYuY29uZmlnLm1heERhdGUgJiYgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuY29uZmlnLm1heERhdGUsIHRpbWVsZXNzICE9PSB1bmRlZmluZWQgPyB0aW1lbGVzcyA6ICFzZWxmLm1heERhdGVIYXNUaW1lKSA+IDApIHJldHVybiBmYWxzZTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCAmJiAhc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGgpIHJldHVybiB0cnVlO1xuXG5cdFx0dmFyIGRhdGVUb0NoZWNrID0gc2VsZi5wYXJzZURhdGUoZGF0ZSwgbnVsbCwgdHJ1ZSk7IC8vIHRpbWVsZXNzXG5cblx0XHR2YXIgYm9vbCA9IHNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggPiAwLFxuXHRcdCAgICBhcnJheSA9IGJvb2wgPyBzZWxmLmNvbmZpZy5lbmFibGUgOiBzZWxmLmNvbmZpZy5kaXNhYmxlO1xuXG5cdFx0Zm9yICh2YXIgaSA9IDAsIGQ7IGkgPCBhcnJheS5sZW5ndGg7IGkrKykge1xuXHRcdFx0ZCA9IGFycmF5W2ldO1xuXG5cdFx0XHRpZiAoZCBpbnN0YW5jZW9mIEZ1bmN0aW9uICYmIGQoZGF0ZVRvQ2hlY2spKSAvLyBkaXNhYmxlZCBieSBmdW5jdGlvblxuXHRcdFx0XHRyZXR1cm4gYm9vbDtlbHNlIGlmIChkIGluc3RhbmNlb2YgRGF0ZSAmJiBkLmdldFRpbWUoKSA9PT0gZGF0ZVRvQ2hlY2suZ2V0VGltZSgpKVxuXHRcdFx0XHQvLyBkaXNhYmxlZCBieSBkYXRlXG5cdFx0XHRcdHJldHVybiBib29sO2Vsc2UgaWYgKHR5cGVvZiBkID09PSBcInN0cmluZ1wiICYmIHNlbGYucGFyc2VEYXRlKGQsIG51bGwsIHRydWUpLmdldFRpbWUoKSA9PT0gZGF0ZVRvQ2hlY2suZ2V0VGltZSgpKVxuXHRcdFx0XHQvLyBkaXNhYmxlZCBieSBkYXRlIHN0cmluZ1xuXHRcdFx0XHRyZXR1cm4gYm9vbDtlbHNlIGlmICggLy8gZGlzYWJsZWQgYnkgcmFuZ2Vcblx0XHRcdCh0eXBlb2YgZCA9PT0gXCJ1bmRlZmluZWRcIiA/IFwidW5kZWZpbmVkXCIgOiBfdHlwZW9mKGQpKSA9PT0gXCJvYmplY3RcIiAmJiBkLmZyb20gJiYgZC50byAmJiBkYXRlVG9DaGVjayA+PSBkLmZyb20gJiYgZGF0ZVRvQ2hlY2sgPD0gZC50bykgcmV0dXJuIGJvb2w7XG5cdFx0fVxuXG5cdFx0cmV0dXJuICFib29sO1xuXHR9XG5cblx0ZnVuY3Rpb24gb25LZXlEb3duKGUpIHtcblx0XHR2YXIgaXNJbnB1dCA9IGUudGFyZ2V0ID09PSBzZWxmLl9pbnB1dDtcblx0XHR2YXIgY2FsZW5kYXJFbGVtID0gaXNDYWxlbmRhckVsZW0oZS50YXJnZXQpO1xuXHRcdHZhciBhbGxvd0lucHV0ID0gc2VsZi5jb25maWcuYWxsb3dJbnB1dDtcblx0XHR2YXIgYWxsb3dLZXlkb3duID0gc2VsZi5pc09wZW4gJiYgKCFhbGxvd0lucHV0IHx8ICFpc0lucHV0KTtcblx0XHR2YXIgYWxsb3dJbmxpbmVLZXlkb3duID0gc2VsZi5jb25maWcuaW5saW5lICYmIGlzSW5wdXQgJiYgIWFsbG93SW5wdXQ7XG5cblx0XHRpZiAoZS5rZXkgPT09IFwiRW50ZXJcIiAmJiBhbGxvd0lucHV0ICYmIGlzSW5wdXQpIHtcblx0XHRcdHNlbGYuc2V0RGF0ZShzZWxmLl9pbnB1dC52YWx1ZSwgdHJ1ZSwgZS50YXJnZXQgPT09IHNlbGYuYWx0SW5wdXQgPyBzZWxmLmNvbmZpZy5hbHRGb3JtYXQgOiBzZWxmLmNvbmZpZy5kYXRlRm9ybWF0KTtcblx0XHRcdHJldHVybiBlLnRhcmdldC5ibHVyKCk7XG5cdFx0fSBlbHNlIGlmIChjYWxlbmRhckVsZW0gfHwgYWxsb3dLZXlkb3duIHx8IGFsbG93SW5saW5lS2V5ZG93bikge1xuXHRcdFx0dmFyIGlzVGltZU9iaiA9IHNlbGYudGltZUNvbnRhaW5lciAmJiBzZWxmLnRpbWVDb250YWluZXIuY29udGFpbnMoZS50YXJnZXQpO1xuXHRcdFx0c3dpdGNoIChlLmtleSkge1xuXHRcdFx0XHRjYXNlIFwiRW50ZXJcIjpcblx0XHRcdFx0XHRpZiAoaXNUaW1lT2JqKSB1cGRhdGVWYWx1ZSgpO2Vsc2Ugc2VsZWN0RGF0ZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJFc2NhcGVcIjpcblx0XHRcdFx0XHQvLyBlc2NhcGVcblx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0c2VsZi5jbG9zZSgpO1xuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJBcnJvd0xlZnRcIjpcblx0XHRcdFx0Y2FzZSBcIkFycm93UmlnaHRcIjpcblx0XHRcdFx0XHRpZiAoIWlzVGltZU9iaikge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG5cdFx0XHRcdFx0XHRpZiAoc2VsZi5kYXlzQ29udGFpbmVyKSB7XG5cdFx0XHRcdFx0XHRcdHZhciBfZGVsdGEgPSBlLmtleSA9PT0gXCJBcnJvd1JpZ2h0XCIgPyAxIDogLTE7XG5cblx0XHRcdFx0XHRcdFx0aWYgKCFlLmN0cmxLZXkpIGZvY3VzT25EYXkoZS50YXJnZXQuJGksIF9kZWx0YSk7ZWxzZSBjaGFuZ2VNb250aChfZGVsdGEsIHRydWUpO1xuXHRcdFx0XHRcdFx0fSBlbHNlIGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lICYmICFpc1RpbWVPYmopIHNlbGYuaG91ckVsZW1lbnQuZm9jdXMoKTtcblx0XHRcdFx0XHR9XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwiQXJyb3dVcFwiOlxuXHRcdFx0XHRjYXNlIFwiQXJyb3dEb3duXCI6XG5cdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdHZhciBkZWx0YSA9IGUua2V5ID09PSBcIkFycm93RG93blwiID8gMSA6IC0xO1xuXG5cdFx0XHRcdFx0aWYgKHNlbGYuZGF5c0NvbnRhaW5lcikge1xuXHRcdFx0XHRcdFx0aWYgKGUuY3RybEtleSkge1xuXHRcdFx0XHRcdFx0XHRjaGFuZ2VZZWFyKHNlbGYuY3VycmVudFllYXIgLSBkZWx0YSk7XG5cdFx0XHRcdFx0XHRcdGZvY3VzT25EYXkoZS50YXJnZXQuJGksIDApO1xuXHRcdFx0XHRcdFx0fSBlbHNlIGlmICghaXNUaW1lT2JqKSBmb2N1c09uRGF5KGUudGFyZ2V0LiRpLCBkZWx0YSAqIDcpO1xuXHRcdFx0XHRcdH0gZWxzZSBpZiAoc2VsZi5jb25maWcuZW5hYmxlVGltZSkge1xuXHRcdFx0XHRcdFx0aWYgKCFpc1RpbWVPYmopIHNlbGYuaG91ckVsZW1lbnQuZm9jdXMoKTtcblx0XHRcdFx0XHRcdHVwZGF0ZVRpbWUoZSk7XG5cdFx0XHRcdFx0fVxuXG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSBcIlRhYlwiOlxuXHRcdFx0XHRcdGlmIChlLnRhcmdldCA9PT0gc2VsZi5ob3VyRWxlbWVudCkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0c2VsZi5taW51dGVFbGVtZW50LnNlbGVjdCgpO1xuXHRcdFx0XHRcdH0gZWxzZSBpZiAoZS50YXJnZXQgPT09IHNlbGYubWludXRlRWxlbWVudCAmJiAoc2VsZi5zZWNvbmRFbGVtZW50IHx8IHNlbGYuYW1QTSkpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdChzZWxmLnNlY29uZEVsZW1lbnQgfHwgc2VsZi5hbVBNKS5mb2N1cygpO1xuXHRcdFx0XHRcdH0gZWxzZSBpZiAoZS50YXJnZXQgPT09IHNlbGYuc2Vjb25kRWxlbWVudCkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0c2VsZi5hbVBNLmZvY3VzKCk7XG5cdFx0XHRcdFx0fVxuXG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSBcImFcIjpcblx0XHRcdFx0XHRpZiAoZS50YXJnZXQgPT09IHNlbGYuYW1QTSkge1xuXHRcdFx0XHRcdFx0c2VsZi5hbVBNLnRleHRDb250ZW50ID0gXCJBTVwiO1xuXHRcdFx0XHRcdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cdFx0XHRcdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwicFwiOlxuXHRcdFx0XHRcdGlmIChlLnRhcmdldCA9PT0gc2VsZi5hbVBNKSB7XG5cdFx0XHRcdFx0XHRzZWxmLmFtUE0udGV4dENvbnRlbnQgPSBcIlBNXCI7XG5cdFx0XHRcdFx0XHRzZXRIb3Vyc0Zyb21JbnB1dHMoKTtcblx0XHRcdFx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGRlZmF1bHQ6XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdH1cblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiS2V5RG93blwiLCBlKTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBvbk1vdXNlT3ZlcihlbGVtKSB7XG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggIT09IDEgfHwgIWVsZW0uY2xhc3NMaXN0LmNvbnRhaW5zKFwiZmxhdHBpY2tyLWRheVwiKSkgcmV0dXJuO1xuXG5cdFx0dmFyIGhvdmVyRGF0ZSA9IGVsZW0uZGF0ZU9iaixcblx0XHQgICAgaW5pdGlhbERhdGUgPSBzZWxmLnBhcnNlRGF0ZShzZWxmLnNlbGVjdGVkRGF0ZXNbMF0sIG51bGwsIHRydWUpLFxuXHRcdCAgICByYW5nZVN0YXJ0RGF0ZSA9IE1hdGgubWluKGhvdmVyRGF0ZS5nZXRUaW1lKCksIHNlbGYuc2VsZWN0ZWREYXRlc1swXS5nZXRUaW1lKCkpLFxuXHRcdCAgICByYW5nZUVuZERhdGUgPSBNYXRoLm1heChob3ZlckRhdGUuZ2V0VGltZSgpLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0uZ2V0VGltZSgpKSxcblx0XHQgICAgY29udGFpbnNEaXNhYmxlZCA9IGZhbHNlO1xuXG5cdFx0Zm9yICh2YXIgdCA9IHJhbmdlU3RhcnREYXRlOyB0IDwgcmFuZ2VFbmREYXRlOyB0ICs9IHNlbGYudXRpbHMuZHVyYXRpb24uREFZKSB7XG5cdFx0XHRpZiAoIWlzRW5hYmxlZChuZXcgRGF0ZSh0KSkpIHtcblx0XHRcdFx0Y29udGFpbnNEaXNhYmxlZCA9IHRydWU7XG5cdFx0XHRcdGJyZWFrO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHZhciBfbG9vcCA9IGZ1bmN0aW9uIF9sb29wKHRpbWVzdGFtcCwgaSkge1xuXHRcdFx0dmFyIG91dE9mUmFuZ2UgPSB0aW1lc3RhbXAgPCBzZWxmLm1pblJhbmdlRGF0ZS5nZXRUaW1lKCkgfHwgdGltZXN0YW1wID4gc2VsZi5tYXhSYW5nZURhdGUuZ2V0VGltZSgpLFxuXHRcdFx0ICAgIGRheUVsZW0gPSBzZWxmLmRheXMuY2hpbGROb2Rlc1tpXTtcblxuXHRcdFx0aWYgKG91dE9mUmFuZ2UpIHtcblx0XHRcdFx0c2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LmFkZChcIm5vdEFsbG93ZWRcIik7XG5cdFx0XHRcdFtcImluUmFuZ2VcIiwgXCJzdGFydFJhbmdlXCIsIFwiZW5kUmFuZ2VcIl0uZm9yRWFjaChmdW5jdGlvbiAoYykge1xuXHRcdFx0XHRcdGRheUVsZW0uY2xhc3NMaXN0LnJlbW92ZShjKTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdHJldHVybiBcImNvbnRpbnVlXCI7XG5cdFx0XHR9IGVsc2UgaWYgKGNvbnRhaW5zRGlzYWJsZWQgJiYgIW91dE9mUmFuZ2UpIHJldHVybiBcImNvbnRpbnVlXCI7XG5cblx0XHRcdFtcInN0YXJ0UmFuZ2VcIiwgXCJpblJhbmdlXCIsIFwiZW5kUmFuZ2VcIiwgXCJub3RBbGxvd2VkXCJdLmZvckVhY2goZnVuY3Rpb24gKGMpIHtcblx0XHRcdFx0ZGF5RWxlbS5jbGFzc0xpc3QucmVtb3ZlKGMpO1xuXHRcdFx0fSk7XG5cblx0XHRcdHZhciBtaW5SYW5nZURhdGUgPSBNYXRoLm1heChzZWxmLm1pblJhbmdlRGF0ZS5nZXRUaW1lKCksIHJhbmdlU3RhcnREYXRlKSxcblx0XHRcdCAgICBtYXhSYW5nZURhdGUgPSBNYXRoLm1pbihzZWxmLm1heFJhbmdlRGF0ZS5nZXRUaW1lKCksIHJhbmdlRW5kRGF0ZSk7XG5cblx0XHRcdGVsZW0uY2xhc3NMaXN0LmFkZChob3ZlckRhdGUgPCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0gPyBcInN0YXJ0UmFuZ2VcIiA6IFwiZW5kUmFuZ2VcIik7XG5cblx0XHRcdGlmIChpbml0aWFsRGF0ZSA8IGhvdmVyRGF0ZSAmJiB0aW1lc3RhbXAgPT09IGluaXRpYWxEYXRlLmdldFRpbWUoKSkgZGF5RWxlbS5jbGFzc0xpc3QuYWRkKFwic3RhcnRSYW5nZVwiKTtlbHNlIGlmIChpbml0aWFsRGF0ZSA+IGhvdmVyRGF0ZSAmJiB0aW1lc3RhbXAgPT09IGluaXRpYWxEYXRlLmdldFRpbWUoKSkgZGF5RWxlbS5jbGFzc0xpc3QuYWRkKFwiZW5kUmFuZ2VcIik7XG5cblx0XHRcdGlmICh0aW1lc3RhbXAgPj0gbWluUmFuZ2VEYXRlICYmIHRpbWVzdGFtcCA8PSBtYXhSYW5nZURhdGUpIGRheUVsZW0uY2xhc3NMaXN0LmFkZChcImluUmFuZ2VcIik7XG5cdFx0fTtcblxuXHRcdGZvciAodmFyIHRpbWVzdGFtcCA9IHNlbGYuZGF5cy5jaGlsZE5vZGVzWzBdLmRhdGVPYmouZ2V0VGltZSgpLCBpID0gMDsgaSA8IDQyOyBpKyssIHRpbWVzdGFtcCArPSBzZWxmLnV0aWxzLmR1cmF0aW9uLkRBWSkge1xuXHRcdFx0dmFyIF9yZXQgPSBfbG9vcCh0aW1lc3RhbXAsIGkpO1xuXG5cdFx0XHRpZiAoX3JldCA9PT0gXCJjb250aW51ZVwiKSBjb250aW51ZTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBvblJlc2l6ZSgpIHtcblx0XHRpZiAoc2VsZi5pc09wZW4gJiYgIXNlbGYuY29uZmlnLnN0YXRpYyAmJiAhc2VsZi5jb25maWcuaW5saW5lKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBvcGVuKGUpIHtcblx0XHRpZiAoc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0aWYgKGUpIHtcblx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRlLnRhcmdldC5ibHVyKCk7XG5cdFx0XHR9XG5cblx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLm1vYmlsZUlucHV0LmNsaWNrKCk7XG5cdFx0XHR9LCAwKTtcblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiT3BlblwiKTtcblx0XHRcdHJldHVybjtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5pc09wZW4gfHwgc2VsZi5faW5wdXQuZGlzYWJsZWQgfHwgc2VsZi5jb25maWcuaW5saW5lKSByZXR1cm47XG5cblx0XHRzZWxmLmlzT3BlbiA9IHRydWU7XG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwib3BlblwiKTtcblx0XHRwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdFx0c2VsZi5faW5wdXQuY2xhc3NMaXN0LmFkZChcImFjdGl2ZVwiKTtcblxuXHRcdHRyaWdnZXJFdmVudChcIk9wZW5cIik7XG5cdH1cblxuXHRmdW5jdGlvbiBtaW5NYXhEYXRlU2V0dGVyKHR5cGUpIHtcblx0XHRyZXR1cm4gZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdHZhciBkYXRlT2JqID0gc2VsZi5jb25maWdbXCJfXCIgKyB0eXBlICsgXCJEYXRlXCJdID0gc2VsZi5wYXJzZURhdGUoZGF0ZSk7XG5cblx0XHRcdHZhciBpbnZlcnNlRGF0ZU9iaiA9IHNlbGYuY29uZmlnW1wiX1wiICsgKHR5cGUgPT09IFwibWluXCIgPyBcIm1heFwiIDogXCJtaW5cIikgKyBcIkRhdGVcIl07XG5cdFx0XHR2YXIgaXNWYWxpZERhdGUgPSBkYXRlICYmIGRhdGVPYmogaW5zdGFuY2VvZiBEYXRlO1xuXG5cdFx0XHRpZiAoaXNWYWxpZERhdGUpIHtcblx0XHRcdFx0c2VsZlt0eXBlICsgXCJEYXRlSGFzVGltZVwiXSA9IGRhdGVPYmouZ2V0SG91cnMoKSB8fCBkYXRlT2JqLmdldE1pbnV0ZXMoKSB8fCBkYXRlT2JqLmdldFNlY29uZHMoKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcykge1xuXHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBzZWxmLnNlbGVjdGVkRGF0ZXMuZmlsdGVyKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRcdFx0cmV0dXJuIGlzRW5hYmxlZChkKTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdGlmICghc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCAmJiB0eXBlID09PSBcIm1pblwiKSBzZXRIb3Vyc0Zyb21EYXRlKGRhdGVPYmopO1xuXHRcdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5kYXlzQ29udGFpbmVyKSB7XG5cdFx0XHRcdHJlZHJhdygpO1xuXG5cdFx0XHRcdGlmIChpc1ZhbGlkRGF0ZSkgc2VsZi5jdXJyZW50WWVhckVsZW1lbnRbdHlwZV0gPSBkYXRlT2JqLmdldEZ1bGxZZWFyKCk7ZWxzZSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5yZW1vdmVBdHRyaWJ1dGUodHlwZSk7XG5cblx0XHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuZGlzYWJsZWQgPSBpbnZlcnNlRGF0ZU9iaiAmJiBkYXRlT2JqICYmIGludmVyc2VEYXRlT2JqLmdldEZ1bGxZZWFyKCkgPT09IGRhdGVPYmouZ2V0RnVsbFllYXIoKTtcblx0XHRcdH1cblx0XHR9O1xuXHR9XG5cblx0ZnVuY3Rpb24gcGFyc2VDb25maWcoKSB7XG5cdFx0dmFyIGJvb2xPcHRzID0gW1widXRjXCIsIFwid3JhcFwiLCBcIndlZWtOdW1iZXJzXCIsIFwiYWxsb3dJbnB1dFwiLCBcImNsaWNrT3BlbnNcIiwgXCJ0aW1lXzI0aHJcIiwgXCJlbmFibGVUaW1lXCIsIFwibm9DYWxlbmRhclwiLCBcImFsdElucHV0XCIsIFwic2hvcnRoYW5kQ3VycmVudE1vbnRoXCIsIFwiaW5saW5lXCIsIFwic3RhdGljXCIsIFwiZW5hYmxlU2Vjb25kc1wiLCBcImRpc2FibGVNb2JpbGVcIl07XG5cblx0XHR2YXIgaG9va3MgPSBbXCJvbkNoYW5nZVwiLCBcIm9uQ2xvc2VcIiwgXCJvbkRheUNyZWF0ZVwiLCBcIm9uS2V5RG93blwiLCBcIm9uTW9udGhDaGFuZ2VcIiwgXCJvbk9wZW5cIiwgXCJvblBhcnNlQ29uZmlnXCIsIFwib25SZWFkeVwiLCBcIm9uVmFsdWVVcGRhdGVcIiwgXCJvblllYXJDaGFuZ2VcIl07XG5cblx0XHRzZWxmLmNvbmZpZyA9IE9iamVjdC5jcmVhdGUoRmxhdHBpY2tyLmRlZmF1bHRDb25maWcpO1xuXG5cdFx0dmFyIHVzZXJDb25maWcgPSBfZXh0ZW5kcyh7fSwgc2VsZi5pbnN0YW5jZUNvbmZpZywgSlNPTi5wYXJzZShKU09OLnN0cmluZ2lmeShzZWxmLmVsZW1lbnQuZGF0YXNldCB8fCB7fSkpKTtcblxuXHRcdHNlbGYuY29uZmlnLnBhcnNlRGF0ZSA9IHVzZXJDb25maWcucGFyc2VEYXRlO1xuXHRcdHNlbGYuY29uZmlnLmZvcm1hdERhdGUgPSB1c2VyQ29uZmlnLmZvcm1hdERhdGU7XG5cblx0XHRfZXh0ZW5kcyhzZWxmLmNvbmZpZywgdXNlckNvbmZpZyk7XG5cblx0XHRpZiAoIXVzZXJDb25maWcuZGF0ZUZvcm1hdCAmJiB1c2VyQ29uZmlnLmVuYWJsZVRpbWUpIHtcblx0XHRcdHNlbGYuY29uZmlnLmRhdGVGb3JtYXQgPSBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyID8gXCJIOmlcIiArIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gXCI6U1wiIDogXCJcIikgOiBGbGF0cGlja3IuZGVmYXVsdENvbmZpZy5kYXRlRm9ybWF0ICsgXCIgSDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpO1xuXHRcdH1cblxuXHRcdGlmICh1c2VyQ29uZmlnLmFsdElucHV0ICYmIHVzZXJDb25maWcuZW5hYmxlVGltZSAmJiAhdXNlckNvbmZpZy5hbHRGb3JtYXQpIHtcblx0XHRcdHNlbGYuY29uZmlnLmFsdEZvcm1hdCA9IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgPyBcImg6aVwiICsgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBcIjpTIEtcIiA6IFwiIEtcIikgOiBGbGF0cGlja3IuZGVmYXVsdENvbmZpZy5hbHRGb3JtYXQgKyAoXCIgaDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpICsgXCIgS1wiKTtcblx0XHR9XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZi5jb25maWcsIFwibWluRGF0ZVwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX21pbkRhdGU7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBtaW5NYXhEYXRlU2V0dGVyKFwibWluXCIpXG5cdFx0fSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZi5jb25maWcsIFwibWF4RGF0ZVwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX21heERhdGU7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBtaW5NYXhEYXRlU2V0dGVyKFwibWF4XCIpXG5cdFx0fSk7XG5cblx0XHRzZWxmLmNvbmZpZy5taW5EYXRlID0gdXNlckNvbmZpZy5taW5EYXRlO1xuXHRcdHNlbGYuY29uZmlnLm1heERhdGUgPSB1c2VyQ29uZmlnLm1heERhdGU7XG5cblx0XHRmb3IgKHZhciBpID0gMDsgaSA8IGJvb2xPcHRzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPSBzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPT09IHRydWUgfHwgc2VsZi5jb25maWdbYm9vbE9wdHNbaV1dID09PSBcInRydWVcIjtcblx0XHR9Zm9yICh2YXIgX2kgPSBob29rcy5sZW5ndGg7IF9pLS07KSB7XG5cdFx0XHRpZiAoc2VsZi5jb25maWdbaG9va3NbX2ldXSAhPT0gdW5kZWZpbmVkKSB7XG5cdFx0XHRcdHNlbGYuY29uZmlnW2hvb2tzW19pXV0gPSBhcnJheWlmeShzZWxmLmNvbmZpZ1tob29rc1tfaV1dIHx8IFtdKS5tYXAoYmluZFRvSW5zdGFuY2UpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdGZvciAodmFyIF9pMiA9IDA7IF9pMiA8IHNlbGYuY29uZmlnLnBsdWdpbnMubGVuZ3RoOyBfaTIrKykge1xuXHRcdFx0dmFyIHBsdWdpbkNvbmYgPSBzZWxmLmNvbmZpZy5wbHVnaW5zW19pMl0oc2VsZikgfHwge307XG5cdFx0XHRmb3IgKHZhciBrZXkgaW4gcGx1Z2luQ29uZikge1xuXG5cdFx0XHRcdGlmIChzZWxmLmNvbmZpZ1trZXldIGluc3RhbmNlb2YgQXJyYXkgfHwgfmhvb2tzLmluZGV4T2Yoa2V5KSkge1xuXHRcdFx0XHRcdHNlbGYuY29uZmlnW2tleV0gPSBhcnJheWlmeShwbHVnaW5Db25mW2tleV0pLm1hcChiaW5kVG9JbnN0YW5jZSkuY29uY2F0KHNlbGYuY29uZmlnW2tleV0pO1xuXHRcdFx0XHR9IGVsc2UgaWYgKHR5cGVvZiB1c2VyQ29uZmlnW2tleV0gPT09IFwidW5kZWZpbmVkXCIpIHNlbGYuY29uZmlnW2tleV0gPSBwbHVnaW5Db25mW2tleV07XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0dHJpZ2dlckV2ZW50KFwiUGFyc2VDb25maWdcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cExvY2FsZSgpIHtcblx0XHRpZiAoX3R5cGVvZihzZWxmLmNvbmZpZy5sb2NhbGUpICE9PSBcIm9iamVjdFwiICYmIHR5cGVvZiBGbGF0cGlja3IubDEwbnNbc2VsZi5jb25maWcubG9jYWxlXSA9PT0gXCJ1bmRlZmluZWRcIikgY29uc29sZS53YXJuKFwiZmxhdHBpY2tyOiBpbnZhbGlkIGxvY2FsZSBcIiArIHNlbGYuY29uZmlnLmxvY2FsZSk7XG5cblx0XHRzZWxmLmwxMG4gPSBfZXh0ZW5kcyhPYmplY3QuY3JlYXRlKEZsYXRwaWNrci5sMTBucy5kZWZhdWx0KSwgX3R5cGVvZihzZWxmLmNvbmZpZy5sb2NhbGUpID09PSBcIm9iamVjdFwiID8gc2VsZi5jb25maWcubG9jYWxlIDogc2VsZi5jb25maWcubG9jYWxlICE9PSBcImRlZmF1bHRcIiA/IEZsYXRwaWNrci5sMTBuc1tzZWxmLmNvbmZpZy5sb2NhbGVdIHx8IHt9IDoge30pO1xuXHR9XG5cblx0ZnVuY3Rpb24gcG9zaXRpb25DYWxlbmRhcigpIHtcblx0XHRpZiAoc2VsZi5jYWxlbmRhckNvbnRhaW5lciA9PT0gdW5kZWZpbmVkKSByZXR1cm47XG5cblx0XHR2YXIgY2FsZW5kYXJIZWlnaHQgPSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLm9mZnNldEhlaWdodCxcblx0XHQgICAgY2FsZW5kYXJXaWR0aCA9IHNlbGYuY2FsZW5kYXJDb250YWluZXIub2Zmc2V0V2lkdGgsXG5cdFx0ICAgIGNvbmZpZ1BvcyA9IHNlbGYuY29uZmlnLnBvc2l0aW9uLFxuXHRcdCAgICBpbnB1dEJvdW5kcyA9IHNlbGYuX3Bvc2l0aW9uRWxlbWVudC5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKSxcblx0XHQgICAgZGlzdGFuY2VGcm9tQm90dG9tID0gd2luZG93LmlubmVySGVpZ2h0IC0gaW5wdXRCb3VuZHMuYm90dG9tLFxuXHRcdCAgICBzaG93T25Ub3AgPSBjb25maWdQb3MgPT09IFwiYWJvdmVcIiB8fCBjb25maWdQb3MgIT09IFwiYmVsb3dcIiAmJiBkaXN0YW5jZUZyb21Cb3R0b20gPCBjYWxlbmRhckhlaWdodCAmJiBpbnB1dEJvdW5kcy50b3AgPiBjYWxlbmRhckhlaWdodDtcblxuXHRcdHZhciB0b3AgPSB3aW5kb3cucGFnZVlPZmZzZXQgKyBpbnB1dEJvdW5kcy50b3AgKyAoIXNob3dPblRvcCA/IHNlbGYuX3Bvc2l0aW9uRWxlbWVudC5vZmZzZXRIZWlnaHQgKyAyIDogLWNhbGVuZGFySGVpZ2h0IC0gMik7XG5cblx0XHR0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcImFycm93VG9wXCIsICFzaG93T25Ub3ApO1xuXHRcdHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwiYXJyb3dCb3R0b21cIiwgc2hvd09uVG9wKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUpIHJldHVybjtcblxuXHRcdHZhciBsZWZ0ID0gd2luZG93LnBhZ2VYT2Zmc2V0ICsgaW5wdXRCb3VuZHMubGVmdDtcblx0XHR2YXIgcmlnaHQgPSB3aW5kb3cuZG9jdW1lbnQuYm9keS5vZmZzZXRXaWR0aCAtIGlucHV0Qm91bmRzLnJpZ2h0O1xuXHRcdHZhciByaWdodE1vc3QgPSBsZWZ0ICsgY2FsZW5kYXJXaWR0aCA+IHdpbmRvdy5kb2N1bWVudC5ib2R5Lm9mZnNldFdpZHRoO1xuXG5cdFx0dG9nZ2xlQ2xhc3Moc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgXCJyaWdodE1vc3RcIiwgcmlnaHRNb3N0KTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5zdGF0aWMpIHJldHVybjtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUudG9wID0gdG9wICsgXCJweFwiO1xuXG5cdFx0aWYgKCFyaWdodE1vc3QpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUubGVmdCA9IGxlZnQgKyBcInB4XCI7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLnJpZ2h0ID0gXCJhdXRvXCI7XG5cdFx0fSBlbHNlIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUubGVmdCA9IFwiYXV0b1wiO1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS5yaWdodCA9IHJpZ2h0ICsgXCJweFwiO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHJlZHJhdygpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhciB8fCBzZWxmLmlzTW9iaWxlKSByZXR1cm47XG5cblx0XHRidWlsZFdlZWtkYXlzKCk7XG5cdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXHRcdGJ1aWxkRGF5cygpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2VsZWN0RGF0ZShlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdGUuc3RvcFByb3BhZ2F0aW9uKCk7XG5cblx0XHRpZiAoIWUudGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcImZsYXRwaWNrci1kYXlcIikgfHwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwiZGlzYWJsZWRcIikgfHwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwibm90QWxsb3dlZFwiKSkgcmV0dXJuO1xuXG5cdFx0dmFyIHNlbGVjdGVkRGF0ZSA9IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gbmV3IERhdGUoZS50YXJnZXQuZGF0ZU9iai5nZXRUaW1lKCkpO1xuXG5cdFx0dmFyIHNob3VsZENoYW5nZU1vbnRoID0gc2VsZWN0ZWREYXRlLmdldE1vbnRoKCkgIT09IHNlbGYuY3VycmVudE1vbnRoICYmIHNlbGYuY29uZmlnLm1vZGUgIT09IFwicmFuZ2VcIjtcblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlRWxlbSA9IGUudGFyZ2V0O1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwic2luZ2xlXCIpIHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtzZWxlY3RlZERhdGVdO2Vsc2UgaWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwibXVsdGlwbGVcIikge1xuXHRcdFx0dmFyIHNlbGVjdGVkSW5kZXggPSBpc0RhdGVTZWxlY3RlZChzZWxlY3RlZERhdGUpO1xuXHRcdFx0aWYgKHNlbGVjdGVkSW5kZXgpIHNlbGYuc2VsZWN0ZWREYXRlcy5zcGxpY2Uoc2VsZWN0ZWRJbmRleCwgMSk7ZWxzZSBzZWxmLnNlbGVjdGVkRGF0ZXMucHVzaChzZWxlY3RlZERhdGUpO1xuXHRcdH0gZWxzZSBpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMikgc2VsZi5jbGVhcigpO1xuXG5cdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMucHVzaChzZWxlY3RlZERhdGUpO1xuXG5cdFx0XHQvLyB1bmxlc3Mgc2VsZWN0aW5nIHNhbWUgZGF0ZSB0d2ljZSwgc29ydCBhc2NlbmRpbmdseVxuXHRcdFx0aWYgKGNvbXBhcmVEYXRlcyhzZWxlY3RlZERhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1swXSwgdHJ1ZSkgIT09IDApIHNlbGYuc2VsZWN0ZWREYXRlcy5zb3J0KGZ1bmN0aW9uIChhLCBiKSB7XG5cdFx0XHRcdHJldHVybiBhLmdldFRpbWUoKSAtIGIuZ2V0VGltZSgpO1xuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cblx0XHRpZiAoc2hvdWxkQ2hhbmdlTW9udGgpIHtcblx0XHRcdHZhciBpc05ld1llYXIgPSBzZWxmLmN1cnJlbnRZZWFyICE9PSBzZWxlY3RlZERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudFllYXIgPSBzZWxlY3RlZERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0gc2VsZWN0ZWREYXRlLmdldE1vbnRoKCk7XG5cblx0XHRcdGlmIChpc05ld1llYXIpIHRyaWdnZXJFdmVudChcIlllYXJDaGFuZ2VcIik7XG5cblx0XHRcdHRyaWdnZXJFdmVudChcIk1vbnRoQ2hhbmdlXCIpO1xuXHRcdH1cblxuXHRcdGJ1aWxkRGF5cygpO1xuXG5cdFx0aWYgKHNlbGYubWluRGF0ZUhhc1RpbWUgJiYgc2VsZi5jb25maWcuZW5hYmxlVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZWN0ZWREYXRlLCBzZWxmLmNvbmZpZy5taW5EYXRlKSA9PT0gMCkgc2V0SG91cnNGcm9tRGF0ZShzZWxmLmNvbmZpZy5taW5EYXRlKTtcblxuXHRcdHVwZGF0ZVZhbHVlKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlVGltZSkgc2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5zaG93VGltZUlucHV0ID0gdHJ1ZTtcblx0XHR9LCA1MCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSkge1xuXHRcdFx0XHRvbk1vdXNlT3ZlcihlLnRhcmdldCk7XG5cblx0XHRcdFx0c2VsZi5faGlkZVByZXZNb250aEFycm93ID0gc2VsZi5faGlkZVByZXZNb250aEFycm93IHx8IHNlbGYubWluUmFuZ2VEYXRlID4gc2VsZi5kYXlzLmNoaWxkTm9kZXNbMF0uZGF0ZU9iajtcblxuXHRcdFx0XHRzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgfHwgc2VsZi5tYXhSYW5nZURhdGUgPCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsIDEpO1xuXHRcdFx0fSBlbHNlIHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblx0XHR9XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cblx0XHQvLyBtYWludGFpbiBmb2N1c1xuXHRcdGlmICghc2hvdWxkQ2hhbmdlTW9udGgpIGZvY3VzT25EYXkoZS50YXJnZXQuJGksIDApO2Vsc2UgYWZ0ZXJEYXlBbmltKGZ1bmN0aW9uICgpIHtcblx0XHRcdHJldHVybiBzZWxmLnNlbGVjdGVkRGF0ZUVsZW0uZm9jdXMoKTtcblx0XHR9KTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSBzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdHJldHVybiBzZWxmLmhvdXJFbGVtZW50LnNlbGVjdCgpO1xuXHRcdH0sIDQ1MSk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuY2xvc2VPblNlbGVjdCkge1xuXHRcdFx0dmFyIHNpbmdsZSA9IHNlbGYuY29uZmlnLm1vZGUgPT09IFwic2luZ2xlXCIgJiYgIXNlbGYuY29uZmlnLmVuYWJsZVRpbWU7XG5cdFx0XHR2YXIgcmFuZ2UgPSBzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMiAmJiAhc2VsZi5jb25maWcuZW5hYmxlVGltZTtcblxuXHRcdFx0aWYgKHNpbmdsZSB8fCByYW5nZSkgc2VsZi5jbG9zZSgpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHNldChvcHRpb24sIHZhbHVlKSB7XG5cdFx0c2VsZi5jb25maWdbb3B0aW9uXSA9IHZhbHVlO1xuXHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0anVtcFRvRGF0ZSgpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0U2VsZWN0ZWREYXRlKGlucHV0RGF0ZSwgZm9ybWF0KSB7XG5cdFx0aWYgKGlucHV0RGF0ZSBpbnN0YW5jZW9mIEFycmF5KSBzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUubWFwKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5wYXJzZURhdGUoZCwgZm9ybWF0KTtcblx0XHR9KTtlbHNlIGlmIChpbnB1dERhdGUgaW5zdGFuY2VvZiBEYXRlIHx8ICFpc05hTihpbnB1dERhdGUpKSBzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZi5wYXJzZURhdGUoaW5wdXREYXRlLCBmb3JtYXQpXTtlbHNlIGlmIChpbnB1dERhdGUgJiYgaW5wdXREYXRlLnN1YnN0cmluZykge1xuXHRcdFx0c3dpdGNoIChzZWxmLmNvbmZpZy5tb2RlKSB7XG5cdFx0XHRcdGNhc2UgXCJzaW5nbGVcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZi5wYXJzZURhdGUoaW5wdXREYXRlLCBmb3JtYXQpXTtcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwibXVsdGlwbGVcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUuc3BsaXQoXCI7IFwiKS5tYXAoZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdFx0XHRcdHJldHVybiBzZWxmLnBhcnNlRGF0ZShkYXRlLCBmb3JtYXQpO1xuXHRcdFx0XHRcdH0pO1xuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJyYW5nZVwiOlxuXHRcdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IGlucHV0RGF0ZS5zcGxpdChzZWxmLmwxMG4ucmFuZ2VTZXBhcmF0b3IpLm1hcChmdW5jdGlvbiAoZGF0ZSkge1xuXHRcdFx0XHRcdFx0cmV0dXJuIHNlbGYucGFyc2VEYXRlKGRhdGUsIGZvcm1hdCk7XG5cdFx0XHRcdFx0fSk7XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRkZWZhdWx0OlxuXHRcdFx0XHRcdGJyZWFrO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IHNlbGYuc2VsZWN0ZWREYXRlcy5maWx0ZXIoZnVuY3Rpb24gKGQpIHtcblx0XHRcdHJldHVybiBkIGluc3RhbmNlb2YgRGF0ZSAmJiBpc0VuYWJsZWQoZCwgZmFsc2UpO1xuXHRcdH0pO1xuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVzLnNvcnQoZnVuY3Rpb24gKGEsIGIpIHtcblx0XHRcdHJldHVybiBhLmdldFRpbWUoKSAtIGIuZ2V0VGltZSgpO1xuXHRcdH0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0RGF0ZShkYXRlLCB0cmlnZ2VyQ2hhbmdlLCBmb3JtYXQpIHtcblx0XHRpZiAoIWRhdGUpIHJldHVybiBzZWxmLmNsZWFyKHRyaWdnZXJDaGFuZ2UpO1xuXG5cdFx0c2V0U2VsZWN0ZWREYXRlKGRhdGUsIGZvcm1hdCk7XG5cblx0XHRzZWxmLnNob3dUaW1lSW5wdXQgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID4gMDtcblx0XHRzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IHNlbGYuc2VsZWN0ZWREYXRlc1swXTtcblxuXHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0anVtcFRvRGF0ZSgpO1xuXG5cdFx0c2V0SG91cnNGcm9tRGF0ZSgpO1xuXHRcdHVwZGF0ZVZhbHVlKHRyaWdnZXJDaGFuZ2UpO1xuXG5cdFx0aWYgKHRyaWdnZXJDaGFuZ2UpIHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwRGF0ZXMoKSB7XG5cdFx0ZnVuY3Rpb24gcGFyc2VEYXRlUnVsZXMoYXJyKSB7XG5cdFx0XHRmb3IgKHZhciBpID0gYXJyLmxlbmd0aDsgaS0tOykge1xuXHRcdFx0XHRpZiAodHlwZW9mIGFycltpXSA9PT0gXCJzdHJpbmdcIiB8fCArYXJyW2ldKSBhcnJbaV0gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0sIG51bGwsIHRydWUpO2Vsc2UgaWYgKGFycltpXSAmJiBhcnJbaV0uZnJvbSAmJiBhcnJbaV0udG8pIHtcblx0XHRcdFx0XHRhcnJbaV0uZnJvbSA9IHNlbGYucGFyc2VEYXRlKGFycltpXS5mcm9tKTtcblx0XHRcdFx0XHRhcnJbaV0udG8gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0udG8pO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cblx0XHRcdHJldHVybiBhcnIuZmlsdGVyKGZ1bmN0aW9uICh4KSB7XG5cdFx0XHRcdHJldHVybiB4O1xuXHRcdFx0fSk7IC8vIHJlbW92ZSBmYWxzeSB2YWx1ZXNcblx0XHR9XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbXTtcblx0XHRzZWxmLm5vdyA9IG5ldyBEYXRlKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGgpIHNlbGYuY29uZmlnLmRpc2FibGUgPSBwYXJzZURhdGVSdWxlcyhzZWxmLmNvbmZpZy5kaXNhYmxlKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoKSBzZWxmLmNvbmZpZy5lbmFibGUgPSBwYXJzZURhdGVSdWxlcyhzZWxmLmNvbmZpZy5lbmFibGUpO1xuXG5cdFx0dmFyIHByZWxvYWRlZERhdGUgPSBzZWxmLmNvbmZpZy5kZWZhdWx0RGF0ZSB8fCBzZWxmLmlucHV0LnZhbHVlO1xuXHRcdGlmIChwcmVsb2FkZWREYXRlKSBzZXRTZWxlY3RlZERhdGUocHJlbG9hZGVkRGF0ZSwgc2VsZi5jb25maWcuZGF0ZUZvcm1hdCk7XG5cblx0XHR2YXIgaW5pdGlhbERhdGUgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID8gc2VsZi5zZWxlY3RlZERhdGVzWzBdIDogc2VsZi5jb25maWcubWluRGF0ZSAmJiBzZWxmLmNvbmZpZy5taW5EYXRlLmdldFRpbWUoKSA+IHNlbGYubm93ID8gc2VsZi5jb25maWcubWluRGF0ZSA6IHNlbGYuY29uZmlnLm1heERhdGUgJiYgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRUaW1lKCkgPCBzZWxmLm5vdyA/IHNlbGYuY29uZmlnLm1heERhdGUgOiBzZWxmLm5vdztcblxuXHRcdHNlbGYuY3VycmVudFllYXIgPSBpbml0aWFsRGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdHNlbGYuY3VycmVudE1vbnRoID0gaW5pdGlhbERhdGUuZ2V0TW9udGgoKTtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IHNlbGYuc2VsZWN0ZWREYXRlc1swXTtcblxuXHRcdHNlbGYubWluRGF0ZUhhc1RpbWUgPSBzZWxmLmNvbmZpZy5taW5EYXRlICYmIChzZWxmLmNvbmZpZy5taW5EYXRlLmdldEhvdXJzKCkgfHwgc2VsZi5jb25maWcubWluRGF0ZS5nZXRNaW51dGVzKCkgfHwgc2VsZi5jb25maWcubWluRGF0ZS5nZXRTZWNvbmRzKCkpO1xuXG5cdFx0c2VsZi5tYXhEYXRlSGFzVGltZSA9IHNlbGYuY29uZmlnLm1heERhdGUgJiYgKHNlbGYuY29uZmlnLm1heERhdGUuZ2V0SG91cnMoKSB8fCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1pbnV0ZXMoKSB8fCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldFNlY29uZHMoKSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZiwgXCJsYXRlc3RTZWxlY3RlZERhdGVPYmpcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiBzZWxmLl9zZWxlY3RlZERhdGVPYmogfHwgc2VsZi5zZWxlY3RlZERhdGVzW3NlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggLSAxXTtcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChkYXRlKSB7XG5cdFx0XHRcdHNlbGYuX3NlbGVjdGVkRGF0ZU9iaiA9IGRhdGU7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcInNob3dUaW1lSW5wdXRcIiwge1xuXHRcdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0XHRyZXR1cm4gc2VsZi5fc2hvd1RpbWVJbnB1dDtcblx0XHRcdFx0fSxcblx0XHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRcdHNlbGYuX3Nob3dUaW1lSW5wdXQgPSBib29sO1xuXHRcdFx0XHRcdGlmIChzZWxmLmNhbGVuZGFyQ29udGFpbmVyKSB0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcInNob3dUaW1lSW5wdXRcIiwgYm9vbCk7XG5cdFx0XHRcdFx0cG9zaXRpb25DYWxlbmRhcigpO1xuXHRcdFx0XHR9XG5cdFx0XHR9KTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cEhlbHBlckZ1bmN0aW9ucygpIHtcblx0XHRzZWxmLnV0aWxzID0ge1xuXHRcdFx0ZHVyYXRpb246IHtcblx0XHRcdFx0REFZOiA4NjQwMDAwMFxuXHRcdFx0fSxcblx0XHRcdGdldERheXNpbk1vbnRoOiBmdW5jdGlvbiBnZXREYXlzaW5Nb250aChtb250aCwgeXIpIHtcblx0XHRcdFx0bW9udGggPSB0eXBlb2YgbW9udGggPT09IFwidW5kZWZpbmVkXCIgPyBzZWxmLmN1cnJlbnRNb250aCA6IG1vbnRoO1xuXG5cdFx0XHRcdHlyID0gdHlwZW9mIHlyID09PSBcInVuZGVmaW5lZFwiID8gc2VsZi5jdXJyZW50WWVhciA6IHlyO1xuXG5cdFx0XHRcdGlmIChtb250aCA9PT0gMSAmJiAoeXIgJSA0ID09PSAwICYmIHlyICUgMTAwICE9PSAwIHx8IHlyICUgNDAwID09PSAwKSkgcmV0dXJuIDI5O1xuXG5cdFx0XHRcdHJldHVybiBzZWxmLmwxMG4uZGF5c0luTW9udGhbbW9udGhdO1xuXHRcdFx0fSxcblx0XHRcdG1vbnRoVG9TdHI6IGZ1bmN0aW9uIG1vbnRoVG9TdHIobW9udGhOdW1iZXIsIHNob3J0aGFuZCkge1xuXHRcdFx0XHRzaG9ydGhhbmQgPSB0eXBlb2Ygc2hvcnRoYW5kID09PSBcInVuZGVmaW5lZFwiID8gc2VsZi5jb25maWcuc2hvcnRoYW5kQ3VycmVudE1vbnRoIDogc2hvcnRoYW5kO1xuXG5cdFx0XHRcdHJldHVybiBzZWxmLmwxMG4ubW9udGhzWyhzaG9ydGhhbmQgPyBcInNob3J0XCIgOiBcImxvbmdcIikgKyBcImhhbmRcIl1bbW9udGhOdW1iZXJdO1xuXHRcdFx0fVxuXHRcdH07XG5cdH1cblxuXHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRmdW5jdGlvbiBzZXR1cEZvcm1hdHMoKSB7XG5cdFx0W1wiRFwiLCBcIkZcIiwgXCJKXCIsIFwiTVwiLCBcIldcIiwgXCJsXCJdLmZvckVhY2goZnVuY3Rpb24gKGYpIHtcblx0XHRcdHNlbGYuZm9ybWF0c1tmXSA9IEZsYXRwaWNrci5wcm90b3R5cGUuZm9ybWF0c1tmXS5iaW5kKHNlbGYpO1xuXHRcdH0pO1xuXG5cdFx0c2VsZi5yZXZGb3JtYXQuRiA9IEZsYXRwaWNrci5wcm90b3R5cGUucmV2Rm9ybWF0LkYuYmluZChzZWxmKTtcblx0XHRzZWxmLnJldkZvcm1hdC5NID0gRmxhdHBpY2tyLnByb3RvdHlwZS5yZXZGb3JtYXQuTS5iaW5kKHNlbGYpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0dXBJbnB1dHMoKSB7XG5cdFx0c2VsZi5pbnB1dCA9IHNlbGYuY29uZmlnLndyYXAgPyBzZWxmLmVsZW1lbnQucXVlcnlTZWxlY3RvcihcIltkYXRhLWlucHV0XVwiKSA6IHNlbGYuZWxlbWVudDtcblxuXHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0aWYgKCFzZWxmLmlucHV0KSByZXR1cm4gY29uc29sZS53YXJuKFwiRXJyb3I6IGludmFsaWQgaW5wdXQgZWxlbWVudCBzcGVjaWZpZWRcIiwgc2VsZi5pbnB1dCk7XG5cblx0XHRzZWxmLmlucHV0Ll90eXBlID0gc2VsZi5pbnB1dC50eXBlO1xuXHRcdHNlbGYuaW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXG5cdFx0c2VsZi5pbnB1dC5jbGFzc0xpc3QuYWRkKFwiZmxhdHBpY2tyLWlucHV0XCIpO1xuXHRcdHNlbGYuX2lucHV0ID0gc2VsZi5pbnB1dDtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5hbHRJbnB1dCkge1xuXHRcdFx0Ly8gcmVwbGljYXRlIHNlbGYuZWxlbWVudFxuXHRcdFx0c2VsZi5hbHRJbnB1dCA9IGNyZWF0ZUVsZW1lbnQoc2VsZi5pbnB1dC5ub2RlTmFtZSwgc2VsZi5pbnB1dC5jbGFzc05hbWUgKyBcIiBcIiArIHNlbGYuY29uZmlnLmFsdElucHV0Q2xhc3MpO1xuXHRcdFx0c2VsZi5faW5wdXQgPSBzZWxmLmFsdElucHV0O1xuXHRcdFx0c2VsZi5hbHRJbnB1dC5wbGFjZWhvbGRlciA9IHNlbGYuaW5wdXQucGxhY2Vob2xkZXI7XG5cdFx0XHRzZWxmLmFsdElucHV0LmRpc2FibGVkID0gc2VsZi5pbnB1dC5kaXNhYmxlZDtcblx0XHRcdHNlbGYuYWx0SW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXHRcdFx0c2VsZi5pbnB1dC50eXBlID0gXCJoaWRkZW5cIjtcblxuXHRcdFx0aWYgKCFzZWxmLmNvbmZpZy5zdGF0aWMgJiYgc2VsZi5pbnB1dC5wYXJlbnROb2RlKSBzZWxmLmlucHV0LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYuYWx0SW5wdXQsIHNlbGYuaW5wdXQubmV4dFNpYmxpbmcpO1xuXHRcdH1cblxuXHRcdGlmICghc2VsZi5jb25maWcuYWxsb3dJbnB1dCkgc2VsZi5faW5wdXQuc2V0QXR0cmlidXRlKFwicmVhZG9ubHlcIiwgXCJyZWFkb25seVwiKTtcblxuXHRcdHNlbGYuX3Bvc2l0aW9uRWxlbWVudCA9IHNlbGYuY29uZmlnLnBvc2l0aW9uRWxlbWVudCB8fCBzZWxmLl9pbnB1dDtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwTW9iaWxlKCkge1xuXHRcdHZhciBpbnB1dFR5cGUgPSBzZWxmLmNvbmZpZy5lbmFibGVUaW1lID8gc2VsZi5jb25maWcubm9DYWxlbmRhciA/IFwidGltZVwiIDogXCJkYXRldGltZS1sb2NhbFwiIDogXCJkYXRlXCI7XG5cblx0XHRzZWxmLm1vYmlsZUlucHV0ID0gY3JlYXRlRWxlbWVudChcImlucHV0XCIsIHNlbGYuaW5wdXQuY2xhc3NOYW1lICsgXCIgZmxhdHBpY2tyLW1vYmlsZVwiKTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnN0ZXAgPSBcImFueVwiO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQudGFiSW5kZXggPSAxO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQudHlwZSA9IGlucHV0VHlwZTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LmRpc2FibGVkID0gc2VsZi5pbnB1dC5kaXNhYmxlZDtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnBsYWNlaG9sZGVyID0gc2VsZi5pbnB1dC5wbGFjZWhvbGRlcjtcblxuXHRcdHNlbGYubW9iaWxlRm9ybWF0U3RyID0gaW5wdXRUeXBlID09PSBcImRhdGV0aW1lLWxvY2FsXCIgPyBcIlktbS1kXFxcXFRIOmk6U1wiIDogaW5wdXRUeXBlID09PSBcImRhdGVcIiA/IFwiWS1tLWRcIiA6IFwiSDppOlNcIjtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSB7XG5cdFx0XHRzZWxmLm1vYmlsZUlucHV0LmRlZmF1bHRWYWx1ZSA9IHNlbGYubW9iaWxlSW5wdXQudmFsdWUgPSBzZWxmLmZvcm1hdERhdGUoc2VsZi5zZWxlY3RlZERhdGVzWzBdLCBzZWxmLm1vYmlsZUZvcm1hdFN0cik7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1pbkRhdGUpIHNlbGYubW9iaWxlSW5wdXQubWluID0gc2VsZi5mb3JtYXREYXRlKHNlbGYuY29uZmlnLm1pbkRhdGUsIFwiWS1tLWRcIik7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWF4RGF0ZSkgc2VsZi5tb2JpbGVJbnB1dC5tYXggPSBzZWxmLmZvcm1hdERhdGUoc2VsZi5jb25maWcubWF4RGF0ZSwgXCJZLW0tZFwiKTtcblxuXHRcdHNlbGYuaW5wdXQudHlwZSA9IFwiaGlkZGVuXCI7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsdElucHV0KSBzZWxmLmFsdElucHV0LnR5cGUgPSBcImhpZGRlblwiO1xuXG5cdFx0dHJ5IHtcblx0XHRcdHNlbGYuaW5wdXQucGFyZW50Tm9kZS5pbnNlcnRCZWZvcmUoc2VsZi5tb2JpbGVJbnB1dCwgc2VsZi5pbnB1dC5uZXh0U2libGluZyk7XG5cdFx0fSBjYXRjaCAoZSkge1xuXHRcdFx0Ly9cblx0XHR9XG5cblx0XHRzZWxmLm1vYmlsZUlucHV0LmFkZEV2ZW50TGlzdGVuZXIoXCJjaGFuZ2VcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHNlbGYuc2V0RGF0ZShlLnRhcmdldC52YWx1ZSwgZmFsc2UsIHNlbGYubW9iaWxlRm9ybWF0U3RyKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNsb3NlXCIpO1xuXHRcdH0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gdG9nZ2xlKCkge1xuXHRcdGlmIChzZWxmLmlzT3BlbikgcmV0dXJuIHNlbGYuY2xvc2UoKTtcblx0XHRzZWxmLm9wZW4oKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHRyaWdnZXJFdmVudChldmVudCwgZGF0YSkge1xuXHRcdHZhciBob29rcyA9IHNlbGYuY29uZmlnW1wib25cIiArIGV2ZW50XTtcblxuXHRcdGlmIChob29rcyAhPT0gdW5kZWZpbmVkICYmIGhvb2tzLmxlbmd0aCA+IDApIHtcblx0XHRcdGZvciAodmFyIGkgPSAwOyBob29rc1tpXSAmJiBpIDwgaG9va3MubGVuZ3RoOyBpKyspIHtcblx0XHRcdFx0aG9va3NbaV0oc2VsZi5zZWxlY3RlZERhdGVzLCBzZWxmLmlucHV0LnZhbHVlLCBzZWxmLCBkYXRhKTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRpZiAoZXZlbnQgPT09IFwiQ2hhbmdlXCIpIHtcblx0XHRcdHNlbGYuaW5wdXQuZGlzcGF0Y2hFdmVudChjcmVhdGVFdmVudChcImNoYW5nZVwiKSk7XG5cblx0XHRcdC8vIG1hbnkgZnJvbnQtZW5kIGZyYW1ld29ya3MgYmluZCB0byB0aGUgaW5wdXQgZXZlbnRcblx0XHRcdHNlbGYuaW5wdXQuZGlzcGF0Y2hFdmVudChjcmVhdGVFdmVudChcImlucHV0XCIpKTtcblx0XHR9XG5cdH1cblxuXHQvKipcbiAgKiBDcmVhdGVzIGFuIEV2ZW50LCBub3JtYWxpemVkIGFjcm9zcyBicm93c2Vyc1xuICAqIEBwYXJhbSB7U3RyaW5nfSBuYW1lIHRoZSBldmVudCBuYW1lLCBlLmcuIFwiY2xpY2tcIlxuICAqIEByZXR1cm4ge0V2ZW50fSB0aGUgY3JlYXRlZCBldmVudFxuICAqL1xuXHRmdW5jdGlvbiBjcmVhdGVFdmVudChuYW1lKSB7XG5cdFx0aWYgKHNlbGYuX3N1cHBvcnRzRXZlbnRzKSByZXR1cm4gbmV3IEV2ZW50KG5hbWUsIHsgYnViYmxlczogdHJ1ZSB9KTtcblxuXHRcdHNlbGYuX1tuYW1lICsgXCJFdmVudFwiXSA9IGRvY3VtZW50LmNyZWF0ZUV2ZW50KFwiRXZlbnRcIik7XG5cdFx0c2VsZi5fW25hbWUgKyBcIkV2ZW50XCJdLmluaXRFdmVudChuYW1lLCB0cnVlLCB0cnVlKTtcblx0XHRyZXR1cm4gc2VsZi5fW25hbWUgKyBcIkV2ZW50XCJdO1xuXHR9XG5cblx0ZnVuY3Rpb24gaXNEYXRlU2VsZWN0ZWQoZGF0ZSkge1xuXHRcdGZvciAodmFyIGkgPSAwOyBpIDwgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRpZiAoY29tcGFyZURhdGVzKHNlbGYuc2VsZWN0ZWREYXRlc1tpXSwgZGF0ZSkgPT09IDApIHJldHVybiBcIlwiICsgaTtcblx0XHR9XG5cblx0XHRyZXR1cm4gZmFsc2U7XG5cdH1cblxuXHRmdW5jdGlvbiBpc0RhdGVJblJhbmdlKGRhdGUpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSAhPT0gXCJyYW5nZVwiIHx8IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPCAyKSByZXR1cm4gZmFsc2U7XG5cdFx0cmV0dXJuIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pID49IDAgJiYgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1sxXSkgPD0gMDtcblx0fVxuXG5cdGZ1bmN0aW9uIHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgfHwgc2VsZi5pc01vYmlsZSB8fCAhc2VsZi5tb250aE5hdikgcmV0dXJuO1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LnRleHRDb250ZW50ID0gc2VsZi51dGlscy5tb250aFRvU3RyKHNlbGYuY3VycmVudE1vbnRoKSArIFwiIFwiO1xuXHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LnZhbHVlID0gc2VsZi5jdXJyZW50WWVhcjtcblxuXHRcdHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyA9IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgKHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKSA/IHNlbGYuY3VycmVudE1vbnRoIDw9IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TW9udGgoKSA6IHNlbGYuY3VycmVudFllYXIgPCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkpO1xuXG5cdFx0c2VsZi5faGlkZU5leHRNb250aEFycm93ID0gc2VsZi5jb25maWcubWF4RGF0ZSAmJiAoc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpID8gc2VsZi5jdXJyZW50TW9udGggKyAxID4gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNb250aCgpIDogc2VsZi5jdXJyZW50WWVhciA+IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSk7XG5cdH1cblxuXHQvKipcbiAgKiBVcGRhdGVzIHRoZSB2YWx1ZXMgb2YgaW5wdXRzIGFzc29jaWF0ZWQgd2l0aCB0aGUgY2FsZW5kYXJcbiAgKiBAcmV0dXJuIHt2b2lkfVxuICAqL1xuXHRmdW5jdGlvbiB1cGRhdGVWYWx1ZSh0cmlnZ2VyQ2hhbmdlKSB7XG5cdFx0aWYgKCFzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSByZXR1cm4gc2VsZi5jbGVhcih0cmlnZ2VyQ2hhbmdlKTtcblxuXHRcdGlmIChzZWxmLmlzTW9iaWxlKSB7XG5cdFx0XHRzZWxmLm1vYmlsZUlucHV0LnZhbHVlID0gc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA/IHNlbGYuZm9ybWF0RGF0ZShzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiwgc2VsZi5tb2JpbGVGb3JtYXRTdHIpIDogXCJcIjtcblx0XHR9XG5cblx0XHR2YXIgam9pbkNoYXIgPSBzZWxmLmNvbmZpZy5tb2RlICE9PSBcInJhbmdlXCIgPyBcIjsgXCIgOiBzZWxmLmwxMG4ucmFuZ2VTZXBhcmF0b3I7XG5cblx0XHRzZWxmLmlucHV0LnZhbHVlID0gc2VsZi5zZWxlY3RlZERhdGVzLm1hcChmdW5jdGlvbiAoZE9iaikge1xuXHRcdFx0cmV0dXJuIHNlbGYuZm9ybWF0RGF0ZShkT2JqLCBzZWxmLmNvbmZpZy5kYXRlRm9ybWF0KTtcblx0XHR9KS5qb2luKGpvaW5DaGFyKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5hbHRJbnB1dCkge1xuXHRcdFx0c2VsZi5hbHRJbnB1dC52YWx1ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5tYXAoZnVuY3Rpb24gKGRPYmopIHtcblx0XHRcdFx0cmV0dXJuIHNlbGYuZm9ybWF0RGF0ZShkT2JqLCBzZWxmLmNvbmZpZy5hbHRGb3JtYXQpO1xuXHRcdFx0fSkuam9pbihqb2luQ2hhcik7XG5cdFx0fVxuXHRcdHRyaWdnZXJFdmVudChcIlZhbHVlVXBkYXRlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gbW91c2VEZWx0YShlKSB7XG5cdFx0cmV0dXJuIE1hdGgubWF4KC0xLCBNYXRoLm1pbigxLCBlLndoZWVsRGVsdGEgfHwgLWUuZGVsdGFZKSk7XG5cdH1cblxuXHRmdW5jdGlvbiBvbk1vbnRoTmF2U2Nyb2xsKGUpIHtcblx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0dmFyIGlzWWVhciA9IHNlbGYuY3VycmVudFllYXJFbGVtZW50LnBhcmVudE5vZGUuY29udGFpbnMoZS50YXJnZXQpO1xuXG5cdFx0aWYgKGUudGFyZ2V0ID09PSBzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQgfHwgaXNZZWFyKSB7XG5cblx0XHRcdHZhciBkZWx0YSA9IG1vdXNlRGVsdGEoZSk7XG5cblx0XHRcdGlmIChpc1llYXIpIHtcblx0XHRcdFx0Y2hhbmdlWWVhcihzZWxmLmN1cnJlbnRZZWFyICsgZGVsdGEpO1xuXHRcdFx0XHRlLnRhcmdldC52YWx1ZSA9IHNlbGYuY3VycmVudFllYXI7XG5cdFx0XHR9IGVsc2Ugc2VsZi5jaGFuZ2VNb250aChkZWx0YSwgdHJ1ZSwgZmFsc2UpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIG9uTW9udGhOYXZDbGljayhlKSB7XG5cdFx0dmFyIGlzUHJldk1vbnRoID0gc2VsZi5wcmV2TW9udGhOYXYuY29udGFpbnMoZS50YXJnZXQpO1xuXHRcdHZhciBpc05leHRNb250aCA9IHNlbGYubmV4dE1vbnRoTmF2LmNvbnRhaW5zKGUudGFyZ2V0KTtcblxuXHRcdGlmIChpc1ByZXZNb250aCB8fCBpc05leHRNb250aCkgY2hhbmdlTW9udGgoaXNQcmV2TW9udGggPyAtMSA6IDEpO2Vsc2UgaWYgKGUudGFyZ2V0ID09PSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudCkge1xuXHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0fSBlbHNlIGlmIChlLnRhcmdldC5jbGFzc05hbWUgPT09IFwiYXJyb3dVcFwiKSBzZWxmLmNoYW5nZVllYXIoc2VsZi5jdXJyZW50WWVhciArIDEpO2Vsc2UgaWYgKGUudGFyZ2V0LmNsYXNzTmFtZSA9PT0gXCJhcnJvd0Rvd25cIikgc2VsZi5jaGFuZ2VZZWFyKHNlbGYuY3VycmVudFllYXIgLSAxKTtcblx0fVxuXG5cdC8qKlxuICAqIENyZWF0ZXMgYW4gSFRNTEVsZW1lbnQgd2l0aCBnaXZlbiB0YWcsIGNsYXNzLCBhbmQgdGV4dHVhbCBjb250ZW50XG4gICogQHBhcmFtIHtTdHJpbmd9IHRhZyB0aGUgSFRNTCB0YWdcbiAgKiBAcGFyYW0ge1N0cmluZ30gY2xhc3NOYW1lIHRoZSBuZXcgZWxlbWVudCdzIGNsYXNzIG5hbWVcbiAgKiBAcGFyYW0ge1N0cmluZ30gY29udGVudCBUaGUgbmV3IGVsZW1lbnQncyB0ZXh0IGNvbnRlbnRcbiAgKiBAcmV0dXJuIHtIVE1MRWxlbWVudH0gdGhlIGNyZWF0ZWQgSFRNTCBlbGVtZW50XG4gICovXG5cdGZ1bmN0aW9uIGNyZWF0ZUVsZW1lbnQodGFnLCBjbGFzc05hbWUsIGNvbnRlbnQpIHtcblx0XHR2YXIgZSA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFbGVtZW50KHRhZyk7XG5cdFx0Y2xhc3NOYW1lID0gY2xhc3NOYW1lIHx8IFwiXCI7XG5cdFx0Y29udGVudCA9IGNvbnRlbnQgfHwgXCJcIjtcblxuXHRcdGUuY2xhc3NOYW1lID0gY2xhc3NOYW1lO1xuXG5cdFx0aWYgKGNvbnRlbnQgIT09IHVuZGVmaW5lZCkgZS50ZXh0Q29udGVudCA9IGNvbnRlbnQ7XG5cblx0XHRyZXR1cm4gZTtcblx0fVxuXG5cdGZ1bmN0aW9uIGFycmF5aWZ5KG9iaikge1xuXHRcdGlmIChvYmogaW5zdGFuY2VvZiBBcnJheSkgcmV0dXJuIG9iajtcblx0XHRyZXR1cm4gW29ial07XG5cdH1cblxuXHRmdW5jdGlvbiB0b2dnbGVDbGFzcyhlbGVtLCBjbGFzc05hbWUsIGJvb2wpIHtcblx0XHRpZiAoYm9vbCkgcmV0dXJuIGVsZW0uY2xhc3NMaXN0LmFkZChjbGFzc05hbWUpO1xuXHRcdGVsZW0uY2xhc3NMaXN0LnJlbW92ZShjbGFzc05hbWUpO1xuXHR9XG5cblx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0ZnVuY3Rpb24gZGVib3VuY2UoZnVuYywgd2FpdCwgaW1tZWRpYXRlKSB7XG5cdFx0dmFyIHRpbWVvdXQgPSB2b2lkIDA7XG5cdFx0cmV0dXJuIGZ1bmN0aW9uICgpIHtcblx0XHRcdHZhciBjb250ZXh0ID0gdGhpcyxcblx0XHRcdCAgICBhcmdzID0gYXJndW1lbnRzO1xuXHRcdFx0Y2xlYXJUaW1lb3V0KHRpbWVvdXQpO1xuXHRcdFx0dGltZW91dCA9IHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0XHR0aW1lb3V0ID0gbnVsbDtcblx0XHRcdFx0aWYgKCFpbW1lZGlhdGUpIGZ1bmMuYXBwbHkoY29udGV4dCwgYXJncyk7XG5cdFx0XHR9LCB3YWl0KTtcblx0XHRcdGlmIChpbW1lZGlhdGUgJiYgIXRpbWVvdXQpIGZ1bmMuYXBwbHkoY29udGV4dCwgYXJncyk7XG5cdFx0fTtcblx0fVxuXG5cdC8qKlxuICAqIENvbXB1dGUgdGhlIGRpZmZlcmVuY2UgaW4gZGF0ZXMsIG1lYXN1cmVkIGluIG1zXG4gICogQHBhcmFtIHtEYXRlfSBkYXRlMVxuICAqIEBwYXJhbSB7RGF0ZX0gZGF0ZTJcbiAgKiBAcGFyYW0ge0Jvb2xlYW59IHRpbWVsZXNzIHdoZXRoZXIgdG8gcmVzZXQgdGltZXMgb2YgYm90aCBkYXRlcyB0byAwMDowMFxuICAqIEByZXR1cm4ge051bWJlcn0gdGhlIGRpZmZlcmVuY2UgaW4gbXNcbiAgKi9cblx0ZnVuY3Rpb24gY29tcGFyZURhdGVzKGRhdGUxLCBkYXRlMiwgdGltZWxlc3MpIHtcblx0XHRpZiAoIShkYXRlMSBpbnN0YW5jZW9mIERhdGUpIHx8ICEoZGF0ZTIgaW5zdGFuY2VvZiBEYXRlKSkgcmV0dXJuIGZhbHNlO1xuXG5cdFx0aWYgKHRpbWVsZXNzICE9PSBmYWxzZSkge1xuXHRcdFx0cmV0dXJuIG5ldyBEYXRlKGRhdGUxLmdldFRpbWUoKSkuc2V0SG91cnMoMCwgMCwgMCwgMCkgLSBuZXcgRGF0ZShkYXRlMi5nZXRUaW1lKCkpLnNldEhvdXJzKDAsIDAsIDAsIDApO1xuXHRcdH1cblxuXHRcdHJldHVybiBkYXRlMS5nZXRUaW1lKCkgLSBkYXRlMi5nZXRUaW1lKCk7XG5cdH1cblxuXHRmdW5jdGlvbiB0aW1lV3JhcHBlcihlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG5cdFx0dmFyIGlzS2V5RG93biA9IGUudHlwZSA9PT0gXCJrZXlkb3duXCIsXG5cdFx0ICAgIGlzV2hlZWwgPSBlLnR5cGUgPT09IFwid2hlZWxcIixcblx0XHQgICAgaXNJbmNyZW1lbnQgPSBlLnR5cGUgPT09IFwiaW5jcmVtZW50XCIsXG5cdFx0ICAgIGlucHV0ID0gZS50YXJnZXQ7XG5cblx0XHRpZiAoc2VsZi5hbVBNICYmIGUudGFyZ2V0ID09PSBzZWxmLmFtUE0pIHJldHVybiBlLnRhcmdldC50ZXh0Q29udGVudCA9IFtcIkFNXCIsIFwiUE1cIl1bZS50YXJnZXQudGV4dENvbnRlbnQgPT09IFwiQU1cIiB8IDBdO1xuXG5cdFx0dmFyIG1pbiA9IE51bWJlcihpbnB1dC5taW4pLFxuXHRcdCAgICBtYXggPSBOdW1iZXIoaW5wdXQubWF4KSxcblx0XHQgICAgc3RlcCA9IE51bWJlcihpbnB1dC5zdGVwKSxcblx0XHQgICAgY3VyVmFsdWUgPSBwYXJzZUludChpbnB1dC52YWx1ZSwgMTApLFxuXHRcdCAgICBkZWx0YSA9IGUuZGVsdGEgfHwgKCFpc0tleURvd24gPyBNYXRoLm1heCgtMSwgTWF0aC5taW4oMSwgZS53aGVlbERlbHRhIHx8IC1lLmRlbHRhWSkpIHx8IDAgOiBlLndoaWNoID09PSAzOCA/IDEgOiAtMSk7XG5cblx0XHR2YXIgbmV3VmFsdWUgPSBjdXJWYWx1ZSArIHN0ZXAgKiBkZWx0YTtcblxuXHRcdGlmICh0eXBlb2YgaW5wdXQudmFsdWUgIT09IFwidW5kZWZpbmVkXCIgJiYgaW5wdXQudmFsdWUubGVuZ3RoID09PSAyKSB7XG5cdFx0XHR2YXIgaXNIb3VyRWxlbSA9IGlucHV0ID09PSBzZWxmLmhvdXJFbGVtZW50LFxuXHRcdFx0ICAgIGlzTWludXRlRWxlbSA9IGlucHV0ID09PSBzZWxmLm1pbnV0ZUVsZW1lbnQ7XG5cblx0XHRcdGlmIChuZXdWYWx1ZSA8IG1pbikge1xuXHRcdFx0XHRuZXdWYWx1ZSA9IG1heCArIG5ld1ZhbHVlICsgIWlzSG91ckVsZW0gKyAoaXNIb3VyRWxlbSAmJiAhc2VsZi5hbVBNKTtcblxuXHRcdFx0XHRpZiAoaXNNaW51dGVFbGVtKSBpbmNyZW1lbnROdW1JbnB1dChudWxsLCAtMSwgc2VsZi5ob3VyRWxlbWVudCk7XG5cdFx0XHR9IGVsc2UgaWYgKG5ld1ZhbHVlID4gbWF4KSB7XG5cdFx0XHRcdG5ld1ZhbHVlID0gaW5wdXQgPT09IHNlbGYuaG91ckVsZW1lbnQgPyBuZXdWYWx1ZSAtIG1heCAtICFzZWxmLmFtUE0gOiBtaW47XG5cblx0XHRcdFx0aWYgKGlzTWludXRlRWxlbSkgaW5jcmVtZW50TnVtSW5wdXQobnVsbCwgMSwgc2VsZi5ob3VyRWxlbWVudCk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLmFtUE0gJiYgaXNIb3VyRWxlbSAmJiAoc3RlcCA9PT0gMSA/IG5ld1ZhbHVlICsgY3VyVmFsdWUgPT09IDIzIDogTWF0aC5hYnMobmV3VmFsdWUgLSBjdXJWYWx1ZSkgPiBzdGVwKSkgc2VsZi5hbVBNLnRleHRDb250ZW50ID0gc2VsZi5hbVBNLnRleHRDb250ZW50ID09PSBcIlBNXCIgPyBcIkFNXCIgOiBcIlBNXCI7XG5cblx0XHRcdGlucHV0LnZhbHVlID0gc2VsZi5wYWQobmV3VmFsdWUpO1xuXHRcdH1cblx0fVxuXG5cdGluaXQoKTtcblx0cmV0dXJuIHNlbGY7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5GbGF0cGlja3IuZGVmYXVsdENvbmZpZyA9IHtcblx0bW9kZTogXCJzaW5nbGVcIixcblxuXHRwb3NpdGlvbjogXCJhdXRvXCIsXG5cblx0YW5pbWF0ZTogd2luZG93Lm5hdmlnYXRvci51c2VyQWdlbnQuaW5kZXhPZihcIk1TSUVcIikgPT09IC0xLFxuXG5cdC8qIGlmIHRydWUsIGRhdGVzIHdpbGwgYmUgcGFyc2VkLCBmb3JtYXR0ZWQsIGFuZCBkaXNwbGF5ZWQgaW4gVVRDLlxuIHByZWxvYWRpbmcgZGF0ZSBzdHJpbmdzIHcvIHRpbWV6b25lcyBpcyByZWNvbW1lbmRlZCBidXQgbm90IG5lY2Vzc2FyeSAqL1xuXHR1dGM6IGZhbHNlLFxuXG5cdC8vIHdyYXA6IHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvZXhhbXBsZXMvI2ZsYXRwaWNrci1leHRlcm5hbC1lbGVtZW50c1xuXHR3cmFwOiBmYWxzZSxcblxuXHQvLyBlbmFibGVzIHdlZWsgbnVtYmVyc1xuXHR3ZWVrTnVtYmVyczogZmFsc2UsXG5cblx0Ly8gYWxsb3cgbWFudWFsIGRhdGV0aW1lIGlucHV0XG5cdGFsbG93SW5wdXQ6IGZhbHNlLFxuXG5cdC8qXG4gXHRjbGlja2luZyBvbiBpbnB1dCBvcGVucyB0aGUgZGF0ZSh0aW1lKXBpY2tlci5cbiBcdGRpc2FibGUgaWYgeW91IHdpc2ggdG8gb3BlbiB0aGUgY2FsZW5kYXIgbWFudWFsbHkgd2l0aCAub3BlbigpXG4gKi9cblx0Y2xpY2tPcGVuczogdHJ1ZSxcblxuXHQvKlxuIFx0Y2xvc2VzIGNhbGVuZGFyIGFmdGVyIGRhdGUgc2VsZWN0aW9uLFxuIFx0dW5sZXNzICdtb2RlJyBpcyAnbXVsdGlwbGUnIG9yIGVuYWJsZVRpbWUgaXMgdHJ1ZVxuICovXG5cdGNsb3NlT25TZWxlY3Q6IHRydWUsXG5cblx0Ly8gZGlzcGxheSB0aW1lIHBpY2tlciBpbiAyNCBob3VyIG1vZGVcblx0dGltZV8yNGhyOiBmYWxzZSxcblxuXHQvLyBlbmFibGVzIHRoZSB0aW1lIHBpY2tlciBmdW5jdGlvbmFsaXR5XG5cdGVuYWJsZVRpbWU6IGZhbHNlLFxuXG5cdC8vIG5vQ2FsZW5kYXI6IHRydWUgd2lsbCBoaWRlIHRoZSBjYWxlbmRhci4gdXNlIGZvciBhIHRpbWUgcGlja2VyIGFsb25nIHcvIGVuYWJsZVRpbWVcblx0bm9DYWxlbmRhcjogZmFsc2UsXG5cblx0Ly8gbW9yZSBkYXRlIGZvcm1hdCBjaGFycyBhdCBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2RhdGVmb3JtYXRcblx0ZGF0ZUZvcm1hdDogXCJZLW0tZFwiLFxuXG5cdC8vIGRhdGUgZm9ybWF0IHVzZWQgaW4gYXJpYS1sYWJlbCBmb3IgZGF5c1xuXHRhcmlhRGF0ZUZvcm1hdDogXCJGIGosIFlcIixcblxuXHQvLyBhbHRJbnB1dCAtIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2FsdGlucHV0XG5cdGFsdElucHV0OiBmYWxzZSxcblxuXHQvLyB0aGUgY3JlYXRlZCBhbHRJbnB1dCBlbGVtZW50IHdpbGwgaGF2ZSB0aGlzIGNsYXNzLlxuXHRhbHRJbnB1dENsYXNzOiBcImZvcm0tY29udHJvbCBpbnB1dFwiLFxuXG5cdC8vIHNhbWUgYXMgZGF0ZUZvcm1hdCwgYnV0IGZvciBhbHRJbnB1dFxuXHRhbHRGb3JtYXQ6IFwiRiBqLCBZXCIsIC8vIGRlZmF1bHRzIHRvIGUuZy4gSnVuZSAxMCwgMjAxNlxuXG5cdC8vIGRlZmF1bHREYXRlIC0gZWl0aGVyIGEgZGF0ZXN0cmluZyBvciBhIGRhdGUgb2JqZWN0LiB1c2VkIGZvciBkYXRldGltZXBpY2tlclwicyBpbml0aWFsIHZhbHVlXG5cdGRlZmF1bHREYXRlOiBudWxsLFxuXG5cdC8vIHRoZSBtaW5pbXVtIGRhdGUgdGhhdCB1c2VyIGNhbiBwaWNrIChpbmNsdXNpdmUpXG5cdG1pbkRhdGU6IG51bGwsXG5cblx0Ly8gdGhlIG1heGltdW0gZGF0ZSB0aGF0IHVzZXIgY2FuIHBpY2sgKGluY2x1c2l2ZSlcblx0bWF4RGF0ZTogbnVsbCxcblxuXHQvLyBkYXRlcGFyc2VyIHRoYXQgdHJhbnNmb3JtcyBhIGdpdmVuIHN0cmluZyB0byBhIGRhdGUgb2JqZWN0XG5cdHBhcnNlRGF0ZTogbnVsbCxcblxuXHQvLyBkYXRlZm9ybWF0dGVyIHRoYXQgdHJhbnNmb3JtcyBhIGdpdmVuIGRhdGUgb2JqZWN0IHRvIGEgc3RyaW5nLCBhY2NvcmRpbmcgdG8gcGFzc2VkIGZvcm1hdFxuXHRmb3JtYXREYXRlOiBudWxsLFxuXG5cdGdldFdlZWs6IGZ1bmN0aW9uIGdldFdlZWsoZ2l2ZW5EYXRlKSB7XG5cdFx0dmFyIGRhdGUgPSBuZXcgRGF0ZShnaXZlbkRhdGUuZ2V0VGltZSgpKTtcblx0XHR2YXIgb25lamFuID0gbmV3IERhdGUoZGF0ZS5nZXRGdWxsWWVhcigpLCAwLCAxKTtcblx0XHRyZXR1cm4gTWF0aC5jZWlsKCgoZGF0ZSAtIG9uZWphbikgLyA4NjQwMDAwMCArIG9uZWphbi5nZXREYXkoKSArIDEpIC8gNyk7XG5cdH0sXG5cblxuXHQvLyBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNkaXNhYmxlXG5cdGVuYWJsZTogW10sXG5cblx0Ly8gc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jZGlzYWJsZVxuXHRkaXNhYmxlOiBbXSxcblxuXHQvLyBkaXNwbGF5IHRoZSBzaG9ydCB2ZXJzaW9uIG9mIG1vbnRoIG5hbWVzIC0gZS5nLiBTZXAgaW5zdGVhZCBvZiBTZXB0ZW1iZXJcblx0c2hvcnRoYW5kQ3VycmVudE1vbnRoOiBmYWxzZSxcblxuXHQvLyBkaXNwbGF5cyBjYWxlbmRhciBpbmxpbmUuIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2lubGluZS1jYWxlbmRhclxuXHRpbmxpbmU6IGZhbHNlLFxuXG5cdC8vIHBvc2l0aW9uIGNhbGVuZGFyIGluc2lkZSB3cmFwcGVyIGFuZCBuZXh0IHRvIHRoZSBpbnB1dCBlbGVtZW50XG5cdC8vIGxlYXZlIGF0IGZhbHNlIHVubGVzcyB5b3Uga25vdyB3aGF0IHlvdVwicmUgZG9pbmdcblx0XCJzdGF0aWNcIjogZmFsc2UsXG5cblx0Ly8gRE9NIG5vZGUgdG8gYXBwZW5kIHRoZSBjYWxlbmRhciB0byBpbiAqc3RhdGljKiBtb2RlXG5cdGFwcGVuZFRvOiBudWxsLFxuXG5cdC8vIGNvZGUgZm9yIHByZXZpb3VzL25leHQgaWNvbnMuIHRoaXMgaXMgd2hlcmUgeW91IHB1dCB5b3VyIGN1c3RvbSBpY29uIGNvZGUgZS5nLiBmb250YXdlc29tZVxuXHRwcmV2QXJyb3c6IFwiPHN2ZyB2ZXJzaW9uPScxLjEnIHhtbG5zPSdodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZycgeG1sbnM6eGxpbms9J2h0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsnIHZpZXdCb3g9JzAgMCAxNyAxNyc+PGc+PC9nPjxwYXRoIGQ9J001LjIwNyA4LjQ3MWw3LjE0NiA3LjE0Ny0wLjcwNyAwLjcwNy03Ljg1My03Ljg1NCA3Ljg1NC03Ljg1MyAwLjcwNyAwLjcwNy03LjE0NyA3LjE0NnonIC8+PC9zdmc+XCIsXG5cdG5leHRBcnJvdzogXCI8c3ZnIHZlcnNpb249JzEuMScgeG1sbnM9J2h0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnJyB4bWxuczp4bGluaz0naHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluaycgdmlld0JveD0nMCAwIDE3IDE3Jz48Zz48L2c+PHBhdGggZD0nTTEzLjIwNyA4LjQ3MmwtNy44NTQgNy44NTQtMC43MDctMC43MDcgNy4xNDYtNy4xNDYtNy4xNDYtNy4xNDggMC43MDctMC43MDcgNy44NTQgNy44NTR6JyAvPjwvc3ZnPlwiLFxuXG5cdC8vIGVuYWJsZXMgc2Vjb25kcyBpbiB0aGUgdGltZSBwaWNrZXJcblx0ZW5hYmxlU2Vjb25kczogZmFsc2UsXG5cblx0Ly8gc3RlcCBzaXplIHVzZWQgd2hlbiBzY3JvbGxpbmcvaW5jcmVtZW50aW5nIHRoZSBob3VyIGVsZW1lbnRcblx0aG91ckluY3JlbWVudDogMSxcblxuXHQvLyBzdGVwIHNpemUgdXNlZCB3aGVuIHNjcm9sbGluZy9pbmNyZW1lbnRpbmcgdGhlIG1pbnV0ZSBlbGVtZW50XG5cdG1pbnV0ZUluY3JlbWVudDogNSxcblxuXHQvLyBpbml0aWFsIHZhbHVlIGluIHRoZSBob3VyIGVsZW1lbnRcblx0ZGVmYXVsdEhvdXI6IDEyLFxuXG5cdC8vIGluaXRpYWwgdmFsdWUgaW4gdGhlIG1pbnV0ZSBlbGVtZW50XG5cdGRlZmF1bHRNaW51dGU6IDAsXG5cblx0Ly8gZGlzYWJsZSBuYXRpdmUgbW9iaWxlIGRhdGV0aW1lIGlucHV0IHN1cHBvcnRcblx0ZGlzYWJsZU1vYmlsZTogZmFsc2UsXG5cblx0Ly8gZGVmYXVsdCBsb2NhbGVcblx0bG9jYWxlOiBcImRlZmF1bHRcIixcblxuXHRwbHVnaW5zOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSBjYWxlbmRhciBpcyBjbG9zZWRcblx0b25DbG9zZTogdW5kZWZpbmVkLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBvbkNoYW5nZSBjYWxsYmFjayB3aGVuIHVzZXIgc2VsZWN0cyBhIGRhdGUgb3IgdGltZVxuXHRvbkNoYW5nZTogdW5kZWZpbmVkLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBjYWxsZWQgZm9yIGV2ZXJ5IGRheSBlbGVtZW50XG5cdG9uRGF5Q3JlYXRlOiB1bmRlZmluZWQsXG5cblx0Ly8gY2FsbGVkIGV2ZXJ5IHRpbWUgdGhlIG1vbnRoIGlzIGNoYW5nZWRcblx0b25Nb250aENoYW5nZTogdW5kZWZpbmVkLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIGNhbGVuZGFyIGlzIG9wZW5lZFxuXHRvbk9wZW46IHVuZGVmaW5lZCwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gY2FsbGVkIGFmdGVyIHRoZSBjb25maWd1cmF0aW9uIGhhcyBiZWVuIHBhcnNlZFxuXHRvblBhcnNlQ29uZmlnOiB1bmRlZmluZWQsXG5cblx0Ly8gY2FsbGVkIGFmdGVyIGNhbGVuZGFyIGlzIHJlYWR5XG5cdG9uUmVhZHk6IHVuZGVmaW5lZCwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gY2FsbGVkIGFmdGVyIGlucHV0IHZhbHVlIHVwZGF0ZWRcblx0b25WYWx1ZVVwZGF0ZTogdW5kZWZpbmVkLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIHRoZSB5ZWFyIGlzIGNoYW5nZWRcblx0b25ZZWFyQ2hhbmdlOiB1bmRlZmluZWQsXG5cblx0b25LZXlEb3duOiB1bmRlZmluZWRcbn07XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5GbGF0cGlja3IubDEwbnMgPSB7XG5cdGVuOiB7XG5cdFx0d2Vla2RheXM6IHtcblx0XHRcdHNob3J0aGFuZDogW1wiU3VuXCIsIFwiTW9uXCIsIFwiVHVlXCIsIFwiV2VkXCIsIFwiVGh1XCIsIFwiRnJpXCIsIFwiU2F0XCJdLFxuXHRcdFx0bG9uZ2hhbmQ6IFtcIlN1bmRheVwiLCBcIk1vbmRheVwiLCBcIlR1ZXNkYXlcIiwgXCJXZWRuZXNkYXlcIiwgXCJUaHVyc2RheVwiLCBcIkZyaWRheVwiLCBcIlNhdHVyZGF5XCJdXG5cdFx0fSxcblx0XHRtb250aHM6IHtcblx0XHRcdHNob3J0aGFuZDogW1wiSmFuXCIsIFwiRmViXCIsIFwiTWFyXCIsIFwiQXByXCIsIFwiTWF5XCIsIFwiSnVuXCIsIFwiSnVsXCIsIFwiQXVnXCIsIFwiU2VwXCIsIFwiT2N0XCIsIFwiTm92XCIsIFwiRGVjXCJdLFxuXHRcdFx0bG9uZ2hhbmQ6IFtcIkphbnVhcnlcIiwgXCJGZWJydWFyeVwiLCBcIk1hcmNoXCIsIFwiQXByaWxcIiwgXCJNYXlcIiwgXCJKdW5lXCIsIFwiSnVseVwiLCBcIkF1Z3VzdFwiLCBcIlNlcHRlbWJlclwiLCBcIk9jdG9iZXJcIiwgXCJOb3ZlbWJlclwiLCBcIkRlY2VtYmVyXCJdXG5cdFx0fSxcblx0XHRkYXlzSW5Nb250aDogWzMxLCAyOCwgMzEsIDMwLCAzMSwgMzAsIDMxLCAzMSwgMzAsIDMxLCAzMCwgMzFdLFxuXHRcdGZpcnN0RGF5T2ZXZWVrOiAwLFxuXHRcdG9yZGluYWw6IGZ1bmN0aW9uIG9yZGluYWwobnRoKSB7XG5cdFx0XHR2YXIgcyA9IG50aCAlIDEwMDtcblx0XHRcdGlmIChzID4gMyAmJiBzIDwgMjEpIHJldHVybiBcInRoXCI7XG5cdFx0XHRzd2l0Y2ggKHMgJSAxMCkge1xuXHRcdFx0XHRjYXNlIDE6XG5cdFx0XHRcdFx0cmV0dXJuIFwic3RcIjtcblx0XHRcdFx0Y2FzZSAyOlxuXHRcdFx0XHRcdHJldHVybiBcIm5kXCI7XG5cdFx0XHRcdGNhc2UgMzpcblx0XHRcdFx0XHRyZXR1cm4gXCJyZFwiO1xuXHRcdFx0XHRkZWZhdWx0OlxuXHRcdFx0XHRcdHJldHVybiBcInRoXCI7XG5cdFx0XHR9XG5cdFx0fSxcblx0XHRyYW5nZVNlcGFyYXRvcjogXCIgdG8gXCIsXG5cdFx0d2Vla0FiYnJldmlhdGlvbjogXCJXa1wiLFxuXHRcdHNjcm9sbFRpdGxlOiBcIlNjcm9sbCB0byBpbmNyZW1lbnRcIixcblx0XHR0b2dnbGVUaXRsZTogXCJDbGljayB0byB0b2dnbGVcIlxuXHR9XG59O1xuXG5GbGF0cGlja3IubDEwbnMuZGVmYXVsdCA9IE9iamVjdC5jcmVhdGUoRmxhdHBpY2tyLmwxMG5zLmVuKTtcbkZsYXRwaWNrci5sb2NhbGl6ZSA9IGZ1bmN0aW9uIChsMTBuKSB7XG5cdHJldHVybiBfZXh0ZW5kcyhGbGF0cGlja3IubDEwbnMuZGVmYXVsdCwgbDEwbiB8fCB7fSk7XG59O1xuRmxhdHBpY2tyLnNldERlZmF1bHRzID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRyZXR1cm4gX2V4dGVuZHMoRmxhdHBpY2tyLmRlZmF1bHRDb25maWcsIGNvbmZpZyB8fCB7fSk7XG59O1xuXG5GbGF0cGlja3IucHJvdG90eXBlID0ge1xuXHRmb3JtYXRzOiB7XG5cdFx0Ly8gZ2V0IHRoZSBkYXRlIGluIFVUQ1xuXHRcdFo6IGZ1bmN0aW9uIFooZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUudG9JU09TdHJpbmcoKTtcblx0XHR9LFxuXG5cdFx0Ly8gd2Vla2RheSBuYW1lLCBzaG9ydCwgZS5nLiBUaHVcblx0XHREOiBmdW5jdGlvbiBEKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLmwxMG4ud2Vla2RheXMuc2hvcnRoYW5kW3RoaXMuZm9ybWF0cy53KGRhdGUpXTtcblx0XHR9LFxuXG5cdFx0Ly8gZnVsbCBtb250aCBuYW1lIGUuZy4gSmFudWFyeVxuXHRcdEY6IGZ1bmN0aW9uIEYoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMudXRpbHMubW9udGhUb1N0cih0aGlzLmZvcm1hdHMubihkYXRlKSAtIDEsIGZhbHNlKTtcblx0XHR9LFxuXG5cdFx0Ly8gcGFkZGVkIGhvdXIgMS0xMlxuXHRcdEc6IGZ1bmN0aW9uIEcoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKEZsYXRwaWNrci5wcm90b3R5cGUuZm9ybWF0cy5oKGRhdGUpKTtcblx0XHR9LFxuXG5cdFx0Ly8gaG91cnMgd2l0aCBsZWFkaW5nIHplcm8gZS5nLiAwM1xuXHRcdEg6IGZ1bmN0aW9uIEgoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0SG91cnMoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGRheSAoMS0zMCkgd2l0aCBvcmRpbmFsIHN1ZmZpeCBlLmcuIDFzdCwgMm5kXG5cdFx0SjogZnVuY3Rpb24gSihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXREYXRlKCkgKyB0aGlzLmwxMG4ub3JkaW5hbChkYXRlLmdldERhdGUoKSk7XG5cdFx0fSxcblxuXHRcdC8vIEFNL1BNXG5cdFx0SzogZnVuY3Rpb24gSyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRIb3VycygpID4gMTEgPyBcIlBNXCIgOiBcIkFNXCI7XG5cdFx0fSxcblxuXHRcdC8vIHNob3J0aGFuZCBtb250aCBlLmcuIEphbiwgU2VwLCBPY3QsIGV0Y1xuXHRcdE06IGZ1bmN0aW9uIE0oZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMudXRpbHMubW9udGhUb1N0cihkYXRlLmdldE1vbnRoKCksIHRydWUpO1xuXHRcdH0sXG5cblx0XHQvLyBzZWNvbmRzIDAwLTU5XG5cdFx0UzogZnVuY3Rpb24gUyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRTZWNvbmRzKCkpO1xuXHRcdH0sXG5cblx0XHQvLyB1bml4IHRpbWVzdGFtcFxuXHRcdFU6IGZ1bmN0aW9uIFUoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0VGltZSgpIC8gMTAwMDtcblx0XHR9LFxuXG5cdFx0VzogZnVuY3Rpb24gVyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5jb25maWcuZ2V0V2VlayhkYXRlKTtcblx0XHR9LFxuXG5cdFx0Ly8gZnVsbCB5ZWFyIGUuZy4gMjAxNlxuXHRcdFk6IGZ1bmN0aW9uIFkoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHR9LFxuXG5cdFx0Ly8gZGF5IGluIG1vbnRoLCBwYWRkZWQgKDAxLTMwKVxuXHRcdGQ6IGZ1bmN0aW9uIGQoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0RGF0ZSgpKTtcblx0XHR9LFxuXG5cdFx0Ly8gaG91ciBmcm9tIDEtMTIgKGFtL3BtKVxuXHRcdGg6IGZ1bmN0aW9uIGgoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0SG91cnMoKSAlIDEyID8gZGF0ZS5nZXRIb3VycygpICUgMTIgOiAxMjtcblx0XHR9LFxuXG5cdFx0Ly8gbWludXRlcywgcGFkZGVkIHdpdGggbGVhZGluZyB6ZXJvIGUuZy4gMDlcblx0XHRpOiBmdW5jdGlvbiBpKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldE1pbnV0ZXMoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGRheSBpbiBtb250aCAoMS0zMClcblx0XHRqOiBmdW5jdGlvbiBqKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldERhdGUoKTtcblx0XHR9LFxuXG5cdFx0Ly8gd2Vla2RheSBuYW1lLCBmdWxsLCBlLmcuIFRodXJzZGF5XG5cdFx0bDogZnVuY3Rpb24gbChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5sMTBuLndlZWtkYXlzLmxvbmdoYW5kW2RhdGUuZ2V0RGF5KCldO1xuXHRcdH0sXG5cblx0XHQvLyBwYWRkZWQgbW9udGggbnVtYmVyICgwMS0xMilcblx0XHRtOiBmdW5jdGlvbiBtKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldE1vbnRoKCkgKyAxKTtcblx0XHR9LFxuXG5cdFx0Ly8gdGhlIG1vbnRoIG51bWJlciAoMS0xMilcblx0XHRuOiBmdW5jdGlvbiBuKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldE1vbnRoKCkgKyAxO1xuXHRcdH0sXG5cblx0XHQvLyBzZWNvbmRzIDAtNTlcblx0XHRzOiBmdW5jdGlvbiBzKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldFNlY29uZHMoKTtcblx0XHR9LFxuXG5cdFx0Ly8gbnVtYmVyIG9mIHRoZSBkYXkgb2YgdGhlIHdlZWtcblx0XHR3OiBmdW5jdGlvbiB3KGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldERheSgpO1xuXHRcdH0sXG5cblx0XHQvLyBsYXN0IHR3byBkaWdpdHMgb2YgeWVhciBlLmcuIDE2IGZvciAyMDE2XG5cdFx0eTogZnVuY3Rpb24geShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gU3RyaW5nKGRhdGUuZ2V0RnVsbFllYXIoKSkuc3Vic3RyaW5nKDIpO1xuXHRcdH1cblx0fSxcblxuXHQvKipcbiAgKiBGb3JtYXRzIGEgZ2l2ZW4gRGF0ZSBvYmplY3QgaW50byBhIHN0cmluZyBiYXNlZCBvbiBzdXBwbGllZCBmb3JtYXRcbiAgKiBAcGFyYW0ge0RhdGV9IGRhdGVPYmogdGhlIGRhdGUgb2JqZWN0XG4gICogQHBhcmFtIHtTdHJpbmd9IGZybXQgYSBzdHJpbmcgY29tcG9zZWQgb2YgZm9ybWF0dGluZyB0b2tlbnMgZS5nLiBcIlktbS1kXCJcbiAgKiBAcmV0dXJuIHtTdHJpbmd9IFRoZSB0ZXh0dWFsIHJlcHJlc2VudGF0aW9uIG9mIHRoZSBkYXRlIGUuZy4gMjAxNy0wMi0wM1xuICAqL1xuXHRmb3JtYXREYXRlOiBmdW5jdGlvbiBmb3JtYXREYXRlKGRhdGVPYmosIGZybXQpIHtcblx0XHR2YXIgX3RoaXMgPSB0aGlzO1xuXG5cdFx0aWYgKHRoaXMuY29uZmlnICE9PSB1bmRlZmluZWQgJiYgdGhpcy5jb25maWcuZm9ybWF0RGF0ZSAhPT0gdW5kZWZpbmVkKSByZXR1cm4gdGhpcy5jb25maWcuZm9ybWF0RGF0ZShkYXRlT2JqLCBmcm10KTtcblxuXHRcdHJldHVybiBmcm10LnNwbGl0KFwiXCIpLm1hcChmdW5jdGlvbiAoYywgaSwgYXJyKSB7XG5cdFx0XHRyZXR1cm4gX3RoaXMuZm9ybWF0c1tjXSAmJiBhcnJbaSAtIDFdICE9PSBcIlxcXFxcIiA/IF90aGlzLmZvcm1hdHNbY10oZGF0ZU9iaikgOiBjICE9PSBcIlxcXFxcIiA/IGMgOiBcIlwiO1xuXHRcdH0pLmpvaW4oXCJcIik7XG5cdH0sXG5cblxuXHRyZXZGb3JtYXQ6IHtcblx0XHREOiBmdW5jdGlvbiBEKCkge30sXG5cdFx0RjogZnVuY3Rpb24gRihkYXRlT2JqLCBtb250aE5hbWUpIHtcblx0XHRcdGRhdGVPYmouc2V0TW9udGgodGhpcy5sMTBuLm1vbnRocy5sb25naGFuZC5pbmRleE9mKG1vbnRoTmFtZSkpO1xuXHRcdH0sXG5cdFx0RzogZnVuY3Rpb24gRyhkYXRlT2JqLCBob3VyKSB7XG5cdFx0XHRkYXRlT2JqLnNldEhvdXJzKHBhcnNlRmxvYXQoaG91cikpO1xuXHRcdH0sXG5cdFx0SDogZnVuY3Rpb24gSChkYXRlT2JqLCBob3VyKSB7XG5cdFx0XHRkYXRlT2JqLnNldEhvdXJzKHBhcnNlRmxvYXQoaG91cikpO1xuXHRcdH0sXG5cdFx0SjogZnVuY3Rpb24gSihkYXRlT2JqLCBkYXkpIHtcblx0XHRcdGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0SzogZnVuY3Rpb24gSyhkYXRlT2JqLCBhbVBNKSB7XG5cdFx0XHR2YXIgaG91cnMgPSBkYXRlT2JqLmdldEhvdXJzKCk7XG5cblx0XHRcdGlmIChob3VycyAhPT0gMTIpIGRhdGVPYmouc2V0SG91cnMoaG91cnMgJSAxMiArIDEyICogL3BtL2kudGVzdChhbVBNKSk7XG5cdFx0fSxcblx0XHRNOiBmdW5jdGlvbiBNKGRhdGVPYmosIHNob3J0TW9udGgpIHtcblx0XHRcdGRhdGVPYmouc2V0TW9udGgodGhpcy5sMTBuLm1vbnRocy5zaG9ydGhhbmQuaW5kZXhPZihzaG9ydE1vbnRoKSk7XG5cdFx0fSxcblx0XHRTOiBmdW5jdGlvbiBTKGRhdGVPYmosIHNlY29uZHMpIHtcblx0XHRcdGRhdGVPYmouc2V0U2Vjb25kcyhzZWNvbmRzKTtcblx0XHR9LFxuXHRcdFU6IGZ1bmN0aW9uIFUoZGF0ZU9iaiwgdW5peFNlY29uZHMpIHtcblx0XHRcdHJldHVybiBuZXcgRGF0ZShwYXJzZUZsb2F0KHVuaXhTZWNvbmRzKSAqIDEwMDApO1xuXHRcdH0sXG5cblx0XHRXOiBmdW5jdGlvbiBXKGRhdGVPYmosIHdlZWtOdW1iZXIpIHtcblx0XHRcdHdlZWtOdW1iZXIgPSBwYXJzZUludCh3ZWVrTnVtYmVyKTtcblx0XHRcdHJldHVybiBuZXcgRGF0ZShkYXRlT2JqLmdldEZ1bGxZZWFyKCksIDAsIDIgKyAod2Vla051bWJlciAtIDEpICogNywgMCwgMCwgMCwgMCwgMCk7XG5cdFx0fSxcblx0XHRZOiBmdW5jdGlvbiBZKGRhdGVPYmosIHllYXIpIHtcblx0XHRcdGRhdGVPYmouc2V0RnVsbFllYXIoeWVhcik7XG5cdFx0fSxcblx0XHRaOiBmdW5jdGlvbiBaKGRhdGVPYmosIElTT0RhdGUpIHtcblx0XHRcdHJldHVybiBuZXcgRGF0ZShJU09EYXRlKTtcblx0XHR9LFxuXG5cdFx0ZDogZnVuY3Rpb24gZChkYXRlT2JqLCBkYXkpIHtcblx0XHRcdGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0aDogZnVuY3Rpb24gaChkYXRlT2JqLCBob3VyKSB7XG5cdFx0XHRkYXRlT2JqLnNldEhvdXJzKHBhcnNlRmxvYXQoaG91cikpO1xuXHRcdH0sXG5cdFx0aTogZnVuY3Rpb24gaShkYXRlT2JqLCBtaW51dGVzKSB7XG5cdFx0XHRkYXRlT2JqLnNldE1pbnV0ZXMocGFyc2VGbG9hdChtaW51dGVzKSk7XG5cdFx0fSxcblx0XHRqOiBmdW5jdGlvbiBqKGRhdGVPYmosIGRheSkge1xuXHRcdFx0ZGF0ZU9iai5zZXREYXRlKHBhcnNlRmxvYXQoZGF5KSk7XG5cdFx0fSxcblx0XHRsOiBmdW5jdGlvbiBsKCkge30sXG5cdFx0bTogZnVuY3Rpb24gbShkYXRlT2JqLCBtb250aCkge1xuXHRcdFx0ZGF0ZU9iai5zZXRNb250aChwYXJzZUZsb2F0KG1vbnRoKSAtIDEpO1xuXHRcdH0sXG5cdFx0bjogZnVuY3Rpb24gbihkYXRlT2JqLCBtb250aCkge1xuXHRcdFx0ZGF0ZU9iai5zZXRNb250aChwYXJzZUZsb2F0KG1vbnRoKSAtIDEpO1xuXHRcdH0sXG5cdFx0czogZnVuY3Rpb24gcyhkYXRlT2JqLCBzZWNvbmRzKSB7XG5cdFx0XHRkYXRlT2JqLnNldFNlY29uZHMocGFyc2VGbG9hdChzZWNvbmRzKSk7XG5cdFx0fSxcblx0XHR3OiBmdW5jdGlvbiB3KCkge30sXG5cdFx0eTogZnVuY3Rpb24geShkYXRlT2JqLCB5ZWFyKSB7XG5cdFx0XHRkYXRlT2JqLnNldEZ1bGxZZWFyKDIwMDAgKyBwYXJzZUZsb2F0KHllYXIpKTtcblx0XHR9XG5cdH0sXG5cblx0dG9rZW5SZWdleDoge1xuXHRcdEQ6IFwiKFxcXFx3KylcIixcblx0XHRGOiBcIihcXFxcdyspXCIsXG5cdFx0RzogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRIOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdEo6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXFxcXHcrXCIsXG5cdFx0SzogXCIoXFxcXHcrKVwiLFxuXHRcdE06IFwiKFxcXFx3KylcIixcblx0XHRTOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdFU6IFwiKC4rKVwiLFxuXHRcdFc6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0WTogXCIoXFxcXGR7NH0pXCIsXG5cdFx0WjogXCIoLispXCIsXG5cdFx0ZDogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRoOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdGk6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0ajogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRsOiBcIihcXFxcdyspXCIsXG5cdFx0bTogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRuOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdHM6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0dzogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHR5OiBcIihcXFxcZHsyfSlcIlxuXHR9LFxuXG5cdHBhZDogZnVuY3Rpb24gcGFkKG51bWJlcikge1xuXHRcdHJldHVybiAoXCIwXCIgKyBudW1iZXIpLnNsaWNlKC0yKTtcblx0fSxcblxuXHQvKipcbiAgKiBQYXJzZXMgYSBkYXRlKCt0aW1lKSBzdHJpbmcgaW50byBhIERhdGUgb2JqZWN0XG4gICogQHBhcmFtIHtTdHJpbmd9IGRhdGUgdGhlIGRhdGUgc3RyaW5nLCBlLmcuIDIwMTctMDItMDMgMTQ6NDVcbiAgKiBAcGFyYW0ge1N0cmluZ30gZ2l2ZW5Gb3JtYXQgdGhlIGRhdGUgZm9ybWF0LCBlLmcuIFktbS1kIEg6aVxuICAqIEBwYXJhbSB7Qm9vbGVhbn0gdGltZWxlc3Mgd2hldGhlciB0byByZXNldCB0aGUgdGltZSBvZiBEYXRlIG9iamVjdFxuICAqIEByZXR1cm4ge0RhdGV9IHRoZSBwYXJzZWQgRGF0ZSBvYmplY3RcbiAgKi9cblx0cGFyc2VEYXRlOiBmdW5jdGlvbiBwYXJzZURhdGUoZGF0ZSwgZ2l2ZW5Gb3JtYXQsIHRpbWVsZXNzKSB7XG5cdFx0aWYgKCFkYXRlKSByZXR1cm4gbnVsbDtcblxuXHRcdHZhciBkYXRlX29yaWcgPSBkYXRlO1xuXG5cdFx0aWYgKGRhdGUgaW5zdGFuY2VvZiBEYXRlKSB7XG5cdFx0XHRkYXRlID0gbmV3IERhdGUoZGF0ZS5nZXRUaW1lKCkpOyAvLyBjcmVhdGUgYSBjb3B5XG5cdFx0XHRkYXRlLmZwX2lzVVRDID0gZGF0ZV9vcmlnLmZwX2lzVVRDO1xuXHRcdH0gZWxzZSBpZiAoZGF0ZS50b0ZpeGVkICE9PSB1bmRlZmluZWQpIC8vIHRpbWVzdGFtcFxuXHRcdFx0ZGF0ZSA9IG5ldyBEYXRlKGRhdGUpO2Vsc2Uge1xuXHRcdFx0Ly8gZGF0ZSBzdHJpbmdcblx0XHRcdHZhciBmb3JtYXQgPSBnaXZlbkZvcm1hdCB8fCAodGhpcy5jb25maWcgfHwgRmxhdHBpY2tyLmRlZmF1bHRDb25maWcpLmRhdGVGb3JtYXQ7XG5cdFx0XHRkYXRlID0gU3RyaW5nKGRhdGUpLnRyaW0oKTtcblxuXHRcdFx0aWYgKGRhdGUgPT09IFwidG9kYXlcIikge1xuXHRcdFx0XHRkYXRlID0gbmV3IERhdGUoKTtcblx0XHRcdFx0dGltZWxlc3MgPSB0cnVlO1xuXHRcdFx0fSBlbHNlIGlmICgvWiQvLnRlc3QoZGF0ZSkgfHwgL0dNVCQvLnRlc3QoZGF0ZSkpIC8vIGRhdGVzdHJpbmdzIHcvIHRpbWV6b25lXG5cdFx0XHRcdGRhdGUgPSBuZXcgRGF0ZShkYXRlKTtlbHNlIGlmICh0aGlzLmNvbmZpZyAmJiB0aGlzLmNvbmZpZy5wYXJzZURhdGUpIGRhdGUgPSB0aGlzLmNvbmZpZy5wYXJzZURhdGUoZGF0ZSwgZm9ybWF0KTtlbHNlIHtcblx0XHRcdFx0dmFyIHBhcnNlZERhdGUgPSAhdGhpcy5jb25maWcgfHwgIXRoaXMuY29uZmlnLm5vQ2FsZW5kYXIgPyBuZXcgRGF0ZShuZXcgRGF0ZSgpLmdldEZ1bGxZZWFyKCksIDAsIDEsIDAsIDAsIDAsIDApIDogbmV3IERhdGUobmV3IERhdGUoKS5zZXRIb3VycygwLCAwLCAwLCAwKSk7XG5cblx0XHRcdFx0dmFyIG1hdGNoZWQgPSB2b2lkIDA7XG5cblx0XHRcdFx0Zm9yICh2YXIgaSA9IDAsIG1hdGNoSW5kZXggPSAwLCByZWdleFN0ciA9IFwiXCI7IGkgPCBmb3JtYXQubGVuZ3RoOyBpKyspIHtcblx0XHRcdFx0XHR2YXIgdG9rZW4gPSBmb3JtYXRbaV07XG5cdFx0XHRcdFx0dmFyIGlzQmFja1NsYXNoID0gdG9rZW4gPT09IFwiXFxcXFwiO1xuXHRcdFx0XHRcdHZhciBlc2NhcGVkID0gZm9ybWF0W2kgLSAxXSA9PT0gXCJcXFxcXCIgfHwgaXNCYWNrU2xhc2g7XG5cblx0XHRcdFx0XHRpZiAodGhpcy50b2tlblJlZ2V4W3Rva2VuXSAmJiAhZXNjYXBlZCkge1xuXHRcdFx0XHRcdFx0cmVnZXhTdHIgKz0gdGhpcy50b2tlblJlZ2V4W3Rva2VuXTtcblx0XHRcdFx0XHRcdHZhciBtYXRjaCA9IG5ldyBSZWdFeHAocmVnZXhTdHIpLmV4ZWMoZGF0ZSk7XG5cdFx0XHRcdFx0XHRpZiAobWF0Y2ggJiYgKG1hdGNoZWQgPSB0cnVlKSkge1xuXHRcdFx0XHRcdFx0XHRwYXJzZWREYXRlID0gdGhpcy5yZXZGb3JtYXRbdG9rZW5dKHBhcnNlZERhdGUsIG1hdGNoWysrbWF0Y2hJbmRleF0pIHx8IHBhcnNlZERhdGU7XG5cdFx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0fSBlbHNlIGlmICghaXNCYWNrU2xhc2gpIHJlZ2V4U3RyICs9IFwiLlwiOyAvLyBkb24ndCByZWFsbHkgY2FyZVxuXHRcdFx0XHR9XG5cblx0XHRcdFx0ZGF0ZSA9IG1hdGNoZWQgPyBwYXJzZWREYXRlIDogbnVsbDtcblx0XHRcdH1cblx0XHR9XG5cblx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdGlmICghKGRhdGUgaW5zdGFuY2VvZiBEYXRlKSkge1xuXHRcdFx0Y29uc29sZS53YXJuKFwiZmxhdHBpY2tyOiBpbnZhbGlkIGRhdGUgXCIgKyBkYXRlX29yaWcpO1xuXHRcdFx0Y29uc29sZS5pbmZvKHRoaXMuZWxlbWVudCk7XG5cdFx0XHRyZXR1cm4gbnVsbDtcblx0XHR9XG5cblx0XHRpZiAodGhpcy5jb25maWcgJiYgdGhpcy5jb25maWcudXRjICYmICFkYXRlLmZwX2lzVVRDKSBkYXRlID0gZGF0ZS5mcF90b1VUQygpO1xuXG5cdFx0aWYgKHRpbWVsZXNzID09PSB0cnVlKSBkYXRlLnNldEhvdXJzKDAsIDAsIDAsIDApO1xuXG5cdFx0cmV0dXJuIGRhdGU7XG5cdH1cbn07XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5mdW5jdGlvbiBfZmxhdHBpY2tyKG5vZGVMaXN0LCBjb25maWcpIHtcblx0dmFyIG5vZGVzID0gQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwobm9kZUxpc3QpOyAvLyBzdGF0aWMgbGlzdFxuXHR2YXIgaW5zdGFuY2VzID0gW107XG5cdGZvciAodmFyIGkgPSAwOyBpIDwgbm9kZXMubGVuZ3RoOyBpKyspIHtcblx0XHR0cnkge1xuXHRcdFx0bm9kZXNbaV0uX2ZsYXRwaWNrciA9IG5ldyBGbGF0cGlja3Iobm9kZXNbaV0sIGNvbmZpZyB8fCB7fSk7XG5cdFx0XHRpbnN0YW5jZXMucHVzaChub2Rlc1tpXS5fZmxhdHBpY2tyKTtcblx0XHR9IGNhdGNoIChlKSB7XG5cdFx0XHRjb25zb2xlLndhcm4oZSwgZS5zdGFjayk7XG5cdFx0fVxuXHR9XG5cblx0cmV0dXJuIGluc3RhbmNlcy5sZW5ndGggPT09IDEgPyBpbnN0YW5jZXNbMF0gOiBpbnN0YW5jZXM7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5pZiAodHlwZW9mIEhUTUxFbGVtZW50ICE9PSBcInVuZGVmaW5lZFwiKSB7XG5cdC8vIGJyb3dzZXIgZW52XG5cdEhUTUxDb2xsZWN0aW9uLnByb3RvdHlwZS5mbGF0cGlja3IgPSBOb2RlTGlzdC5wcm90b3R5cGUuZmxhdHBpY2tyID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRcdHJldHVybiBfZmxhdHBpY2tyKHRoaXMsIGNvbmZpZyk7XG5cdH07XG5cblx0SFRNTEVsZW1lbnQucHJvdG90eXBlLmZsYXRwaWNrciA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0XHRyZXR1cm4gX2ZsYXRwaWNrcihbdGhpc10sIGNvbmZpZyk7XG5cdH07XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5mdW5jdGlvbiBmbGF0cGlja3Ioc2VsZWN0b3IsIGNvbmZpZykge1xuXHRyZXR1cm4gX2ZsYXRwaWNrcih3aW5kb3cuZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChzZWxlY3RvciksIGNvbmZpZyk7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5pZiAodHlwZW9mIGpRdWVyeSAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHRqUXVlcnkuZm4uZmxhdHBpY2tyID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRcdHJldHVybiBfZmxhdHBpY2tyKHRoaXMsIGNvbmZpZyk7XG5cdH07XG59XG5cbkRhdGUucHJvdG90eXBlLmZwX2luY3IgPSBmdW5jdGlvbiAoZGF5cykge1xuXHRyZXR1cm4gbmV3IERhdGUodGhpcy5nZXRGdWxsWWVhcigpLCB0aGlzLmdldE1vbnRoKCksIHRoaXMuZ2V0RGF0ZSgpICsgcGFyc2VJbnQoZGF5cywgMTApKTtcbn07XG5cbkRhdGUucHJvdG90eXBlLmZwX2lzVVRDID0gZmFsc2U7XG5EYXRlLnByb3RvdHlwZS5mcF90b1VUQyA9IGZ1bmN0aW9uICgpIHtcblx0dmFyIG5ld0RhdGUgPSBuZXcgRGF0ZSh0aGlzLmdldFVUQ0Z1bGxZZWFyKCksIHRoaXMuZ2V0VVRDTW9udGgoKSwgdGhpcy5nZXRVVENEYXRlKCksIHRoaXMuZ2V0VVRDSG91cnMoKSwgdGhpcy5nZXRVVENNaW51dGVzKCksIHRoaXMuZ2V0VVRDU2Vjb25kcygpKTtcblxuXHRuZXdEYXRlLmZwX2lzVVRDID0gdHJ1ZTtcblx0cmV0dXJuIG5ld0RhdGU7XG59O1xuXG5pZiAodHlwZW9mIG1vZHVsZSAhPT0gXCJ1bmRlZmluZWRcIikgbW9kdWxlLmV4cG9ydHMgPSBGbGF0cGlja3I7IiwidmFyIFZ1ZSAvLyBsYXRlIGJpbmRcbnZhciB2ZXJzaW9uXG52YXIgbWFwID0gd2luZG93Ll9fVlVFX0hPVF9NQVBfXyA9IE9iamVjdC5jcmVhdGUobnVsbClcbnZhciBpbnN0YWxsZWQgPSBmYWxzZVxudmFyIGlzQnJvd3NlcmlmeSA9IGZhbHNlXG52YXIgaW5pdEhvb2tOYW1lID0gJ2JlZm9yZUNyZWF0ZSdcblxuZXhwb3J0cy5pbnN0YWxsID0gZnVuY3Rpb24gKHZ1ZSwgYnJvd3NlcmlmeSkge1xuICBpZiAoaW5zdGFsbGVkKSByZXR1cm5cbiAgaW5zdGFsbGVkID0gdHJ1ZVxuXG4gIFZ1ZSA9IHZ1ZS5fX2VzTW9kdWxlID8gdnVlLmRlZmF1bHQgOiB2dWVcbiAgdmVyc2lvbiA9IFZ1ZS52ZXJzaW9uLnNwbGl0KCcuJykubWFwKE51bWJlcilcbiAgaXNCcm93c2VyaWZ5ID0gYnJvd3NlcmlmeVxuXG4gIC8vIGNvbXBhdCB3aXRoIDwgMi4wLjAtYWxwaGEuN1xuICBpZiAoVnVlLmNvbmZpZy5fbGlmZWN5Y2xlSG9va3MuaW5kZXhPZignaW5pdCcpID4gLTEpIHtcbiAgICBpbml0SG9va05hbWUgPSAnaW5pdCdcbiAgfVxuXG4gIGV4cG9ydHMuY29tcGF0aWJsZSA9IHZlcnNpb25bMF0gPj0gMlxuICBpZiAoIWV4cG9ydHMuY29tcGF0aWJsZSkge1xuICAgIGNvbnNvbGUud2FybihcbiAgICAgICdbSE1SXSBZb3UgYXJlIHVzaW5nIGEgdmVyc2lvbiBvZiB2dWUtaG90LXJlbG9hZC1hcGkgdGhhdCBpcyAnICtcbiAgICAgICdvbmx5IGNvbXBhdGlibGUgd2l0aCBWdWUuanMgY29yZSBeMi4wLjAuJ1xuICAgIClcbiAgICByZXR1cm5cbiAgfVxufVxuXG4vKipcbiAqIENyZWF0ZSBhIHJlY29yZCBmb3IgYSBob3QgbW9kdWxlLCB3aGljaCBrZWVwcyB0cmFjayBvZiBpdHMgY29uc3RydWN0b3JcbiAqIGFuZCBpbnN0YW5jZXNcbiAqXG4gKiBAcGFyYW0ge1N0cmluZ30gaWRcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKi9cblxuZXhwb3J0cy5jcmVhdGVSZWNvcmQgPSBmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgdmFyIEN0b3IgPSBudWxsXG4gIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIEN0b3IgPSBvcHRpb25zXG4gICAgb3B0aW9ucyA9IEN0b3Iub3B0aW9uc1xuICB9XG4gIG1ha2VPcHRpb25zSG90KGlkLCBvcHRpb25zKVxuICBtYXBbaWRdID0ge1xuICAgIEN0b3I6IFZ1ZS5leHRlbmQob3B0aW9ucyksXG4gICAgaW5zdGFuY2VzOiBbXVxuICB9XG59XG5cbi8qKlxuICogTWFrZSBhIENvbXBvbmVudCBvcHRpb25zIG9iamVjdCBob3QuXG4gKlxuICogQHBhcmFtIHtTdHJpbmd9IGlkXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICovXG5cbmZ1bmN0aW9uIG1ha2VPcHRpb25zSG90IChpZCwgb3B0aW9ucykge1xuICBpbmplY3RIb29rKG9wdGlvbnMsIGluaXRIb29rTmFtZSwgZnVuY3Rpb24gKCkge1xuICAgIG1hcFtpZF0uaW5zdGFuY2VzLnB1c2godGhpcylcbiAgfSlcbiAgaW5qZWN0SG9vayhvcHRpb25zLCAnYmVmb3JlRGVzdHJveScsIGZ1bmN0aW9uICgpIHtcbiAgICB2YXIgaW5zdGFuY2VzID0gbWFwW2lkXS5pbnN0YW5jZXNcbiAgICBpbnN0YW5jZXMuc3BsaWNlKGluc3RhbmNlcy5pbmRleE9mKHRoaXMpLCAxKVxuICB9KVxufVxuXG4vKipcbiAqIEluamVjdCBhIGhvb2sgdG8gYSBob3QgcmVsb2FkYWJsZSBjb21wb25lbnQgc28gdGhhdFxuICogd2UgY2FuIGtlZXAgdHJhY2sgb2YgaXQuXG4gKlxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqIEBwYXJhbSB7U3RyaW5nfSBuYW1lXG4gKiBAcGFyYW0ge0Z1bmN0aW9ufSBob29rXG4gKi9cblxuZnVuY3Rpb24gaW5qZWN0SG9vayAob3B0aW9ucywgbmFtZSwgaG9vaykge1xuICB2YXIgZXhpc3RpbmcgPSBvcHRpb25zW25hbWVdXG4gIG9wdGlvbnNbbmFtZV0gPSBleGlzdGluZ1xuICAgID8gQXJyYXkuaXNBcnJheShleGlzdGluZylcbiAgICAgID8gZXhpc3RpbmcuY29uY2F0KGhvb2spXG4gICAgICA6IFtleGlzdGluZywgaG9va11cbiAgICA6IFtob29rXVxufVxuXG5mdW5jdGlvbiB0cnlXcmFwIChmbikge1xuICByZXR1cm4gZnVuY3Rpb24gKGlkLCBhcmcpIHtcbiAgICB0cnkgeyBmbihpZCwgYXJnKSB9IGNhdGNoIChlKSB7XG4gICAgICBjb25zb2xlLmVycm9yKGUpXG4gICAgICBjb25zb2xlLndhcm4oJ1NvbWV0aGluZyB3ZW50IHdyb25nIGR1cmluZyBWdWUgY29tcG9uZW50IGhvdC1yZWxvYWQuIEZ1bGwgcmVsb2FkIHJlcXVpcmVkLicpXG4gICAgfVxuICB9XG59XG5cbmV4cG9ydHMucmVyZW5kZXIgPSB0cnlXcmFwKGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICB2YXIgcmVjb3JkID0gbWFwW2lkXVxuICBpZiAoIW9wdGlvbnMpIHtcbiAgICByZWNvcmQuaW5zdGFuY2VzLnNsaWNlKCkuZm9yRWFjaChmdW5jdGlvbiAoaW5zdGFuY2UpIHtcbiAgICAgIGluc3RhbmNlLiRmb3JjZVVwZGF0ZSgpXG4gICAgfSlcbiAgICByZXR1cm5cbiAgfVxuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBvcHRpb25zID0gb3B0aW9ucy5vcHRpb25zXG4gIH1cbiAgcmVjb3JkLkN0b3Iub3B0aW9ucy5yZW5kZXIgPSBvcHRpb25zLnJlbmRlclxuICByZWNvcmQuQ3Rvci5vcHRpb25zLnN0YXRpY1JlbmRlckZucyA9IG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zXG4gIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgIGluc3RhbmNlLiRvcHRpb25zLnJlbmRlciA9IG9wdGlvbnMucmVuZGVyXG4gICAgaW5zdGFuY2UuJG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zID0gb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnNcbiAgICBpbnN0YW5jZS5fc3RhdGljVHJlZXMgPSBbXSAvLyByZXNldCBzdGF0aWMgdHJlZXNcbiAgICBpbnN0YW5jZS4kZm9yY2VVcGRhdGUoKVxuICB9KVxufSlcblxuZXhwb3J0cy5yZWxvYWQgPSB0cnlXcmFwKGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICB2YXIgcmVjb3JkID0gbWFwW2lkXVxuICBpZiAob3B0aW9ucykge1xuICAgIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgICAgb3B0aW9ucyA9IG9wdGlvbnMub3B0aW9uc1xuICAgIH1cbiAgICBtYWtlT3B0aW9uc0hvdChpZCwgb3B0aW9ucylcbiAgICBpZiAodmVyc2lvblsxXSA8IDIpIHtcbiAgICAgIC8vIHByZXNlcnZlIHByZSAyLjIgYmVoYXZpb3IgZm9yIGdsb2JhbCBtaXhpbiBoYW5kbGluZ1xuICAgICAgcmVjb3JkLkN0b3IuZXh0ZW5kT3B0aW9ucyA9IG9wdGlvbnNcbiAgICB9XG4gICAgdmFyIG5ld0N0b3IgPSByZWNvcmQuQ3Rvci5zdXBlci5leHRlbmQob3B0aW9ucylcbiAgICByZWNvcmQuQ3Rvci5vcHRpb25zID0gbmV3Q3Rvci5vcHRpb25zXG4gICAgcmVjb3JkLkN0b3IuY2lkID0gbmV3Q3Rvci5jaWRcbiAgICByZWNvcmQuQ3Rvci5wcm90b3R5cGUgPSBuZXdDdG9yLnByb3RvdHlwZVxuICAgIGlmIChuZXdDdG9yLnJlbGVhc2UpIHtcbiAgICAgIC8vIHRlbXBvcmFyeSBnbG9iYWwgbWl4aW4gc3RyYXRlZ3kgdXNlZCBpbiA8IDIuMC4wLWFscGhhLjZcbiAgICAgIG5ld0N0b3IucmVsZWFzZSgpXG4gICAgfVxuICB9XG4gIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgIGlmIChpbnN0YW5jZS4kdm5vZGUgJiYgaW5zdGFuY2UuJHZub2RlLmNvbnRleHQpIHtcbiAgICAgIGluc3RhbmNlLiR2bm9kZS5jb250ZXh0LiRmb3JjZVVwZGF0ZSgpXG4gICAgfSBlbHNlIHtcbiAgICAgIGNvbnNvbGUud2FybignUm9vdCBvciBtYW51YWxseSBtb3VudGVkIGluc3RhbmNlIG1vZGlmaWVkLiBGdWxsIHJlbG9hZCByZXF1aXJlZC4nKVxuICAgIH1cbiAgfSlcbn0pXG4iLCLvu788dGVtcGxhdGU+XHJcbiAgICA8ZGl2IGNsYXNzPVwiZm9ybS1kYXRlIGlucHV0LWdyb3VwXCI+XHJcbiAgICAgICAgPGlucHV0IHR5cGU9XCJ0ZXh0XCIgOmNsYXNzPVwiaW5wdXRDbGFzc1wiICA6cGxhY2Vob2xkZXI9XCJwbGFjZWhvbGRlclwiIDp2YWx1ZT1cInZhbHVlXCIgQGlucHV0PVwib25JbnB1dFwiIGRhdGEtaW5wdXQgLz5cclxuICAgICAgICA8YnV0dG9uIHR5cGU9XCJzdWJtaXRcIiBjbGFzcz1cImJ0biBidG4tbGluayBidG4tY2xlYXJcIiBkYXRhLWNsZWFyPlxyXG4gICAgICAgICAgICA8c3Bhbj48L3NwYW4+XHJcbiAgICAgICAgPC9idXR0b24+XHJcbiAgICAgICAgPHNwYW4gY2xhc3M9XCJpbnB1dC1ncm91cC1hZGRvblwiIGRhdGEtdG9nZ2xlPlxyXG4gICAgICAgICAgICA8c3BhbiBjbGFzcz1cImNhbGVuZGFyXCI+PC9zcGFuPlxyXG4gICAgICAgIDwvc3Bhbj5cclxuICAgIDwvZGl2PlxyXG48L3RlbXBsYXRlPlxyXG5cclxuPHNjcmlwdD5cclxuaW1wb3J0IEZsYXRwaWNrciBmcm9tICdmbGF0cGlja3InXHJcblxyXG5leHBvcnQgZGVmYXVsdCB7XHJcbiAgICBwcm9wczoge1xyXG4gICAgICAgIGlucHV0Q2xhc3M6IHtcclxuICAgICAgICAgICAgdHlwZTogU3RyaW5nXHJcbiAgICAgICAgfSxcclxuICAgICAgICBwbGFjZWhvbGRlcjoge1xyXG4gICAgICAgICAgICB0eXBlOiBTdHJpbmcsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICcnXHJcbiAgICAgICAgfSxcclxuICAgICAgICBvcHRpb25zOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IE9iamVjdCxcclxuICAgICAgICAgICAgZGVmYXVsdDogKCkgPT4geyByZXR1cm4ge30gfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgdmFsdWU6IHtcclxuICAgICAgICAgICAgdHlwZTogU3RyaW5nLFxyXG4gICAgICAgICAgICBkZWZhdWx0OiAnJ1xyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgZGF0YSAoKSB7XHJcbiAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICBmcDogbnVsbFxyXG4gICAgICB9XHJcbiAgfSxcclxuICAgIGNvbXB1dGVkOiB7XHJcbiAgICAgICAgZnBPcHRpb25zICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIEpTT04uc3RyaW5naWZ5KHRoaXMub3B0aW9ucylcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgd2F0Y2g6IHtcclxuICAgICAgICBmcE9wdGlvbnMgKG5ld09wdCkge1xyXG4gICAgICAgICAgICBjb25zdCBvcHRpb24gPSBKU09OLnBhcnNlKG5ld09wdClcclxuICAgICAgICAgICAgZm9yIChsZXQgbyBpbiBvcHRpb24pIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZnAuc2V0KG8sIG9wdGlvbltvXSlcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgbW91bnRlZCAoKSB7XHJcbiAgICAgIGNvbnN0IHNlbGYgPSB0aGlzXHJcbiAgICAgIGNvbnN0IG9yaWdPblZhbFVwZGF0ZSA9IHRoaXMub3B0aW9ucy5vblZhbHVlVXBkYXRlXHJcbiAgICAgIGNvbnN0IG1lcmdlZE9wdGlvbnMgPSBPYmplY3QuYXNzaWduKHRoaXMub3B0aW9ucywge1xyXG4gICAgICAgICAgd3JhcDogdHJ1ZSxcclxuICAgICAgICAgIG9uVmFsdWVVcGRhdGUgKCkge1xyXG4gICAgICAgICAgICAgIHNlbGYub25JbnB1dChzZWxmLiRlbC5xdWVyeVNlbGVjdG9yKCdpbnB1dCcpLnZhbHVlKVxyXG4gICAgICAgICAgICAgIGlmICh0eXBlb2Ygb3JpZ09uVmFsVXBkYXRlID09PSAnZnVuY3Rpb24nKSB7XHJcbiAgICAgICAgICAgICAgICAgIG9yaWdPblZhbFVwZGF0ZSgpXHJcbiAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgfVxyXG4gICAgICB9KVxyXG5cclxuICAgICAgdGhpcy5mcCA9IG5ldyBGbGF0cGlja3IodGhpcy4kZWwsIG1lcmdlZE9wdGlvbnMpXHJcbiAgICAgIHRoaXMuJGVtaXQoJ0ZsYXRwaWNrclJlZicsIHRoaXMuZnApXHJcbiAgfSxcclxuICBkZXN0cm95ZWQgKCkge1xyXG4gICAgICB0aGlzLmZwLmRlc3Ryb3koKVxyXG4gICAgICB0aGlzLmZwID0gbnVsbFxyXG4gIH0sXHJcbiAgICBtZXRob2RzOiB7XHJcbiAgICAgICAgb25JbnB1dCAoZSkge1xyXG4gICAgICAgICAgICBjb25zdCBzZWxlY3RlZERhdGVzID0gdGhpcy5mcC5zZWxlY3RlZERhdGVzIHx8IFtdO1xyXG4gICAgICAgICAgICBjb25zdCBsZWZ0ID0gc2VsZWN0ZWREYXRlcy5sZW5ndGggPiAwID8gc2VsZWN0ZWREYXRlc1swXSA6IG51bGw7XHJcbiAgICAgICAgICAgIGNvbnN0IHJpZ2h0ID0gc2VsZWN0ZWREYXRlcy5sZW5ndGggPiAxID8gc2VsZWN0ZWREYXRlc1sxXSA6IG51bGw7XHJcbiAgICAgICAgICAgIHRoaXMuJGVtaXQoJ2lucHV0JywgKHR5cGVvZiBlID09PSAnc3RyaW5nJyA/IGUgOiBlLnRhcmdldC52YWx1ZSksIGxlZnQsIHJpZ2h0KVxyXG5cclxuICAgICAgICAgICAgaWYgKHJpZ2h0ID09IG51bGwpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGVsLmNsYXNzTGlzdC5yZW1vdmUoXCJhbnN3ZXJlZFwiKVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kZWwuY2xhc3NMaXN0LmFkZChcImFuc3dlcmVkXCIpXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuPC9zY3JpcHQ+Iiwi77u/PHRlbXBsYXRlPlxyXG4gICAgPHRhYmxlIGNsYXNzPVwidGFibGUgdGFibGUtc3RyaXBlZCB0YWJsZS1vcmRlcmVkIHRhYmxlLWJvcmRlcmVkIHRhYmxlLWhvdmVyIHRhYmxlLXdpdGgtY2hlY2tib3hlcyB0YWJsZS13aXRoLXByZWZpbGxlZC1jb2x1bW4gdGFibGUtaW50ZXJ2aWV3c1wiPlxyXG4gICAgICAgIDx0aGVhZD5cclxuICAgICAgICAgICAgXHJcbiAgICAgICAgPC90aGVhZD5cclxuICAgICAgICA8dGJvZHk+PC90Ym9keT5cclxuICAgIDwvdGFibGU+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG5leHBvcnQgZGVmYXVsdCB7XHJcbiAgICBwcm9wczoge1xyXG4gICAgICAgIGFkZFBhcmFtc1RvUmVxdWVzdDoge1xyXG4gICAgICAgICAgICB0eXBlOiBGdW5jdGlvbixcclxuICAgICAgICAgICAgZGVmYXVsdDogKGQpID0+IHsgcmV0dXJuIGQgfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcmVzcG9uc2VQcm9jZXNzb3I6IHtcclxuICAgICAgICAgICAgdHlwZTogRnVuY3Rpb24sXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6IChyKSA9PiB7IHJldHVybiByIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIHRhYmxlT3B0aW9uczoge1xyXG4gICAgICAgICAgICB0eXBlOiBPYmplY3QsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICgpID0+IHsgcmV0dXJuIHt9IH1cclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgZGF0YSAoKSB7XHJcbiAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgdGFibGU6IG51bGxcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgY29tcHV0ZWQ6IHtcclxuICAgIH0sXHJcbiAgICB3YXRjaDoge1xyXG4gICAgfSxcclxuICAgIG1ldGhvZHM6IHtcclxuICAgICAgICByZWxvYWQ6IGZ1bmN0aW9uKGRhdGEpe1xyXG4gICAgICAgICAgICB0aGlzLnRhYmxlLmFqYXguZGF0YSA9IGRhdGE7XHJcbiAgICAgICAgICAgIHRoaXMudGFibGUuYWpheC5yZWxvYWQoKVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgb25UYWJsZUluaXRDb21wbGV0ZTogZnVuY3Rpb24oKXtcclxuICAgICAgICAgICAgJCh0aGlzLiRlbCkucGFyZW50KCcuZGF0YVRhYmxlc193cmFwcGVyJykuZmluZCgnLmRhdGFUYWJsZXNfZmlsdGVyIGxhYmVsJykub24oJ2NsaWNrJywgZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgIGlmIChlLnRhcmdldCAhPT0gdGhpcylcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgICAgICBpZiAoJCh0aGlzKS5oYXNDbGFzcyhcImFjdGl2ZVwiKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICQodGhpcykucmVtb3ZlQ2xhc3MoXCJhY3RpdmVcIik7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAkKHRoaXMpLmFkZENsYXNzKFwiYWN0aXZlXCIpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgbW91bnRlZCAoKSB7XHJcbiAgICAgICAgY29uc3Qgc2VsZiA9IHRoaXNcclxuICAgICAgICB2YXIgb3B0aW9ucyA9IE9iamVjdC5hc3NpZ24oe1xyXG4gICAgICAgICAgICBwcm9jZXNzaW5nOiB0cnVlLFxyXG4gICAgICAgICAgICBzZXJ2ZXJTaWRlOiB0cnVlLFxyXG4gICAgICAgICAgICBsYW5ndWFnZTpcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgXCJ1cmxcIjogd2luZG93LmlucHV0LnNldHRpbmdzLmNvbmZpZy5kYXRhVGFibGVUcmFuc2xhdGlvbnNVcmwsXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHNlYXJjaEhpZ2hsaWdodDogdHJ1ZSxcclxuICAgICAgICAgICAgcGFnaW5nVHlwZTogXCJmdWxsX251bWJlcnNcIixcclxuICAgICAgICAgICAgbGVuZ3RoQ2hhbmdlOiBmYWxzZSwgLy8gZG8gbm90IHNob3cgcGFnZSBzaXplIHNlbGVjdG9yXHJcbiAgICAgICAgICAgIHBhZ2VMZW5ndGg6IDEwLCAvLyBwYWdlIHNpemVcclxuICAgICAgICAgICAgZG9tOiBcImZydHBcIixcclxuICAgICAgICAgICAgY29uZGl0aW9uYWxQYWdpbmc6IHRydWVcclxuICAgICAgICB9LCB0aGlzLnRhYmxlT3B0aW9ucylcclxuXHJcbiAgICAgICAgb3B0aW9ucy5hamF4LmRhdGEgPSBmdW5jdGlvbihkKXtcclxuICAgICAgICAgICAgc2VsZi5hZGRQYXJhbXNUb1JlcXVlc3QoZCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgb3B0aW9ucy5hamF4LmNvbXBsZXRlID0gZnVuY3Rpb24ocmVzcG9uc2Upe1xyXG4gICAgICAgICAgICBzZWxmLnJlc3BvbnNlUHJvY2Vzc29yKHJlc3BvbnNlLnJlc3BvbnNlSlNPTik7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy50YWJsZSA9ICQodGhpcy4kZWwpLkRhdGFUYWJsZShvcHRpb25zKVxyXG4gICAgICAgIHRoaXMudGFibGUub24oJ2luaXQuZHQnLCB0aGlzLm9uVGFibGVJbml0Q29tcGxldGUpO1xyXG4gICAgICAgIHRoaXMuJGVtaXQoJ0RhdGFUYWJsZVJlZicsIHRoaXMudGFibGUpXHJcbiAgICB9LFxyXG4gICAgZGVzdHJveWVkICgpIHtcclxuICAgIH1cclxufVxyXG48L3NjcmlwdD4iLCLvu788dGVtcGxhdGU+XHJcbiAgICA8ZGl2IGNsYXNzPVwiY29tYm8tYm94XCI+XHJcbiAgICAgICAgPGRpdiBjbGFzcz1cImJ0bi1ncm91cCBidG4taW5wdXQgY2xlYXJmaXhcIj5cclxuICAgICAgICAgICAgPGJ1dHRvbiB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4gZHJvcGRvd24tdG9nZ2xlXCIgZGF0YS10b2dnbGU9XCJkcm9wZG93blwiPlxyXG4gICAgICAgICAgICAgICAgPHNwYW4gZGF0YS1iaW5kPVwibGFiZWxcIiB2LWlmPVwidmFsdWUgPT09IG51bGxcIiBjbGFzcz1cImdyYXktdGV4dFwiPnt7cGxhY2Vob2xkZXJUZXh0fX08L3NwYW4+XHJcbiAgICAgICAgICAgICAgICA8c3BhbiBkYXRhLWJpbmQ9XCJsYWJlbFwiIHYtZWxzZT57e3ZhbHVlLnZhbHVlfX08L3NwYW4+XHJcbiAgICAgICAgICAgIDwvYnV0dG9uPlxyXG4gICAgICAgICAgICA8dWwgcmVmPVwiZHJvcGRvd25NZW51XCIgY2xhc3M9XCJkcm9wZG93bi1tZW51XCIgcm9sZT1cIm1lbnVcIj5cclxuICAgICAgICAgICAgICAgIDxsaT5cclxuICAgICAgICAgICAgICAgICAgICA8aW5wdXQgdHlwZT1cInRleHRcIiByZWY9XCJzZWFyY2hCb3hcIiA6aWQ9XCJpbnB1dElkXCIgcGxhY2Vob2xkZXI9XCJTZWFyY2hcIiBAaW5wdXQ9XCJ1cGRhdGVPcHRpb25zTGlzdFwiIHYtb246a2V5dXAuZG93bj1cIm9uU2VhcmNoQm94RG93bktleVwiIHYtbW9kZWw9XCJzZWFyY2hUZXJtXCIgLz5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1mb3I9XCJvcHRpb24gaW4gb3B0aW9uc1wiIDprZXk9XCJvcHRpb24ua2V5XCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGEgaHJlZj1cImphdmFzY3JpcHQ6dm9pZCgwKTtcIiB2LW9uOmNsaWNrPVwic2VsZWN0T3B0aW9uKG9wdGlvbilcIiB2LWh0bWw9XCJoaWdobGlnaHQob3B0aW9uLnZhbHVlLCBzZWFyY2hUZXJtKVwiIHYtb246a2V5ZG93bi51cD1cIm9uT3B0aW9uVXBLZXlcIj48L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtaWY9XCJpc0xvYWRpbmdcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YT5Mb2FkaW5nLi4uPC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWlmPVwiIWlzTG9hZGluZyAmJiBvcHRpb25zLmxlbmd0aCA9PT0gMFwiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhPk5vIHJlc3VsdHMgZm91bmQ8L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICA8L3VsPlxyXG4gICAgICAgIDwvZGl2PlxyXG4gICAgICAgIDxidXR0b24gdi1pZj1cInZhbHVlICE9PSBudWxsXCIgY2xhc3M9XCJidG4gYnRuLWxpbmsgYnRuLWNsZWFyXCIgQGNsaWNrPVwiY2xlYXJcIj5cclxuICAgICAgICAgICAgPHNwYW4+PC9zcGFuPlxyXG4gICAgICAgIDwvYnV0dG9uPlxyXG4gICAgPC9kaXY+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG4gICAgbW9kdWxlLmV4cG9ydHMgPSB7XHJcbiAgICAgICAgbmFtZTogJ3VzZXItc2VsZWN0b3InLFxyXG4gICAgICAgIHByb3BzOiBbJ2ZldGNoVXJsJywgJ2NvbnRyb2xJZCcsICd2YWx1ZScsICdwbGFjZWhvbGRlcicsICdhamF4UGFyYW1zJ10sXHJcbiAgICAgICAgZGF0YTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9uczogW10sXHJcbiAgICAgICAgICAgICAgICBpc0xvYWRpbmc6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgc2VhcmNoVGVybTogJydcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGNvbXB1dGVkOiB7XHJcbiAgICAgICAgICAgIGlucHV0SWQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBgc2JfJHt0aGlzLmNvbnRyb2xJZH1gO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBwbGFjZWhvbGRlclRleHQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLnBsYWNlaG9sZGVyIHx8IFwiU2VsZWN0XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIG1vdW50ZWQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBjb25zdCBqcUVsID0gJCh0aGlzLiRlbClcclxuICAgICAgICAgICAgY29uc3QgZm9jdXNUbyA9IGpxRWwuZmluZChgIyR7dGhpcy5pbnB1dElkfWApXHJcbiAgICAgICAgICAgIGpxRWwub24oJ3Nob3duLmJzLmRyb3Bkb3duJywgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgZm9jdXNUby5mb2N1cygpXHJcbiAgICAgICAgICAgICAgICB0aGlzLmZldGNoT3B0aW9ucyh0aGlzLnNlYXJjaFRlcm0pXHJcbiAgICAgICAgICAgIH0pXHJcblxyXG4gICAgICAgICAgICBqcUVsLm9uKCdoaWRkZW4uYnMuZHJvcGRvd24nLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnNlYXJjaFRlcm0gPSBcIlwiXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgfSxcclxuICAgICAgICBtZXRob2RzOiB7XHJcbiAgICAgICAgICAgIG9uU2VhcmNoQm94RG93bktleShldmVudCkge1xyXG4gICAgICAgICAgICAgICAgdmFyICRmaXJzdE9wdGlvbkFuY2hvciA9ICQodGhpcy4kcmVmcy5kcm9wZG93bk1lbnUpLmZpbmQoJ2EnKS5maXJzdCgpO1xyXG4gICAgICAgICAgICAgICAgJGZpcnN0T3B0aW9uQW5jaG9yLmZvY3VzKCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIG9uT3B0aW9uVXBLZXkoZXZlbnQpIHtcclxuICAgICAgICAgICAgICAgIHZhciBpc0ZpcnN0T3B0aW9uID0gJChldmVudC50YXJnZXQpLnBhcmVudCgpLmluZGV4KCkgPT09IDE7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKGlzRmlyc3RPcHRpb24pIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLiRyZWZzLnNlYXJjaEJveC5mb2N1cygpO1xyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50LnN0b3BQcm9wYWdhdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBmZXRjaE9wdGlvbnM6IGZ1bmN0aW9uIChmaWx0ZXIgPSBcIlwiKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICB2YXIgcmVxdWVzdFBhcmFtcyA9IE9iamVjdC5hc3NpZ24oeyBxdWVyeTogZmlsdGVyLCBjYWNoZTogZmFsc2UgfSwgdGhpcy5hamF4UGFyYW1zKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGh0dHAuZ2V0KHRoaXMuZmV0Y2hVcmwsIHtwYXJhbXM6IHJlcXVlc3RQYXJhbXN9KVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5vcHRpb25zID0gcmVzcG9uc2UuYm9keS5vcHRpb25zIHx8IFtdO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH0sIHJlc3BvbnNlID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBjbGVhcjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kZW1pdCgnc2VsZWN0ZWQnLCBudWxsLCB0aGlzLmNvbnRyb2xJZCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnNlYXJjaFRlcm0gPSBcIlwiO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBzZWxlY3RPcHRpb246IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kZW1pdCgnc2VsZWN0ZWQnLCB2YWx1ZSwgdGhpcy5jb250cm9sSWQpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICB1cGRhdGVPcHRpb25zTGlzdChlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmZldGNoT3B0aW9ucyhlLnRhcmdldC52YWx1ZSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGhpZ2hsaWdodDogZnVuY3Rpb24gKHRpdGxlLCBzZWFyY2hUZXJtKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgZW5jb2RlZFRpdGxlID0gXy5lc2NhcGUodGl0bGUpO1xyXG4gICAgICAgICAgICAgICAgaWYgKHNlYXJjaFRlcm0pIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgc2FmZVNlYXJjaFRlcm0gPSBfLmVzY2FwZShfLmVzY2FwZVJlZ0V4cChzZWFyY2hUZXJtKSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHZhciBpUXVlcnkgPSBuZXcgUmVnRXhwKHNhZmVTZWFyY2hUZXJtLCBcImlnXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBlbmNvZGVkVGl0bGUucmVwbGFjZShpUXVlcnksIChtYXRjaGVkVHh0LCBhLCBiKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBgPHN0cm9uZz4ke21hdGNoZWRUeHR9PC9zdHJvbmc+YDtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZW5jb2RlZFRpdGxlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuPC9zY3JpcHQ+Iiwi77u/77u/aW1wb3J0IFZ1ZSBmcm9tICd2dWUnXHJcbmltcG9ydCBWdWVSZXNvdXJjZSBmcm9tICd2dWUtcmVzb3VyY2UnXHJcbmltcG9ydCBUeXBlYWhlYWQgZnJvbSAnLi9UeXBlYWhlYWQudnVlJ1xyXG5pbXBvcnQgRGF0ZVBpY2tlciBmcm9tICcuL0RhdGVQaWNrZXIudnVlJ1xyXG5pbXBvcnQgSW50ZXJ2aWV3VGFibGUgZnJvbSAnLi9JbnRlcnZpZXdUYWJsZS52dWUnXHJcbmltcG9ydCBWZWVWYWxpZGF0ZSBmcm9tICd2ZWUtdmFsaWRhdGUnO1xyXG5cclxuVnVlLnVzZShWZWVWYWxpZGF0ZSk7XHJcblZ1ZS51c2UoVnVlUmVzb3VyY2UpO1xyXG5cclxuVnVlLmNvbXBvbmVudCgnRmxhdHBpY2tyJywgRGF0ZVBpY2tlcik7XHJcblZ1ZS5jb21wb25lbnQoXCJ0eXBlYWhlYWRcIiwgVHlwZWFoZWFkKTtcclxuVnVlLmNvbXBvbmVudChcImludGVydmlldy10YWJsZVwiLCBJbnRlcnZpZXdUYWJsZSk7Il19
