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

/*! flatpickr v2.4.7, @license MIT */
function Flatpickr(element, config) {
	var self = this;

	self.changeMonth = changeMonth;
	self.changeYear = changeYear;
	self.clear = clear;
	self.close = close;
	self._createElement = createElement;
	self.destroy = destroy;
	self.formatDate = formatDate;
	self.isEnabled = isEnabled;
	self.jumpToDate = jumpToDate;
	self.open = open;
	self.redraw = redraw;
	self.set = set;
	self.setDate = setDate;
	self.toggle = toggle;

	function init() {
		if (element._flatpickr) destroy(element._flatpickr);

		element._flatpickr = self;

		self.element = element;
		self.instanceConfig = config || {};
		self.parseDate = Flatpickr.prototype.parseDate.bind(self);

		setupFormats();
		parseConfig();
		setupLocale();
		setupInputs();
		setupDates();
		setupHelperFunctions();

		self.isOpen = self.config.inline;

		self.isMobile = !self.config.disableMobile && !self.config.inline && self.config.mode === "single" && !self.config.disable.length && !self.config.enable.length && !self.config.weekNumbers && /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);

		if (!self.isMobile) build();

		bind();

		if (self.selectedDates.length || self.config.noCalendar) {
			if (self.config.enableTime) setHoursFromDate(self.config.noCalendar ? self.config.minDate : null);
			updateValue();
		}

		if (self.config.weekNumbers) {
			self.calendarContainer.style.width = self.days.clientWidth + self.weekWrapper.clientWidth + "px";
		}

		self.showTimeInput = self.selectedDates.length > 0 || self.config.noCalendar;

		if (!self.isMobile) positionCalendar();
		triggerEvent("Ready");
	}

	function bindToInstance(fn) {
		if (fn && fn.bind) return fn.bind(self);
		return fn;
	}

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

	function setHoursFromInputs() {
		if (!self.config.enableTime) return;

		var hours = parseInt(self.hourElement.value, 10) || 0,
		    minutes = parseInt(self.minuteElement.value, 10) || 0,
		    seconds = self.config.enableSeconds ? parseInt(self.secondElement.value, 10) || 0 : 0;

		if (self.amPM) hours = hours % 12 + 12 * (self.amPM.textContent === "PM");

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

	function setHoursFromDate(dateObj) {
		var date = dateObj || self.latestSelectedDateObj;

		if (date) setHours(date.getHours(), date.getMinutes(), date.getSeconds());
	}

	function setHours(hours, minutes, seconds) {
		if (self.selectedDates.length) {
			self.latestSelectedDateObj.setHours(hours % 24, minutes, seconds || 0, 0);
		}

		if (!self.config.enableTime || self.isMobile) return;

		self.hourElement.value = self.pad(!self.config.time_24hr ? (12 + hours) % 12 + 12 * (hours % 12 === 0) : hours);

		self.minuteElement.value = self.pad(minutes);

		if (!self.config.time_24hr) self.amPM.textContent = hours >= 12 ? "PM" : "AM";

		if (self.config.enableSeconds) self.secondElement.value = self.pad(seconds);
	}

	function onYearInput(event) {
		var year = event.target.value;
		if (event.delta) year = (parseInt(year) + event.delta).toString();

		if (year.length === 4) {
			self.currentYearElement.blur();
			if (!/[^\d]/.test(year)) changeYear(year);
		}
	}

	function onMonthScroll(e) {
		e.preventDefault();
		self.changeMonth(Math.max(-1, Math.min(1, e.wheelDelta || -e.deltaY)));
	}

	function bind() {
		if (self.config.wrap) {
			["open", "close", "toggle", "clear"].forEach(function (el) {
				var toggles = self.element.querySelectorAll("[data-" + el + "]");
				for (var i = 0; i < toggles.length; i++) {
					toggles[i].addEventListener("click", self[el]);
				}
			});
		}

		if (window.document.createEvent !== undefined) {
			self.changeEvent = window.document.createEvent("HTMLEvents");
			self.changeEvent.initEvent("change", false, true);
		}

		if (self.isMobile) return setupMobile();

		self.debouncedResize = debounce(onResize, 50);
		self.triggerChange = function () {
			triggerEvent("Change");
		};
		self.debouncedChange = debounce(self.triggerChange, 300);

		if (self.config.mode === "range" && self.days) self.days.addEventListener("mouseover", onMouseOver);

		self.calendarContainer.addEventListener("keydown", onKeyDown);

		if (!self.config.static) (self.altInput || self.input).addEventListener("keydown", onKeyDown);

		if (!self.config.inline && !self.config.static) window.addEventListener("resize", self.debouncedResize);

		if (window.ontouchstart) window.document.addEventListener("touchstart", documentClick);

		window.document.addEventListener("click", documentClick);
		(self.altInput || self.input).addEventListener("blur", documentClick);

		if (self.config.clickOpens) (self.altInput || self.input).addEventListener("focus", open);

		if (!self.config.noCalendar) {
			self.prevMonthNav.addEventListener("click", function () {
				return changeMonth(-1);
			});
			self.nextMonthNav.addEventListener("click", function () {
				return changeMonth(1);
			});

			self.currentMonthElement.addEventListener("wheel", function (e) {
				return debounce(onMonthScroll(e), 50);
			});
			self.currentYearElement.addEventListener("wheel", function (e) {
				return debounce(yearScroll(e), 50);
			});
			self.currentYearElement.addEventListener("focus", function () {
				self.currentYearElement.select();
			});

			self.currentYearElement.addEventListener("input", onYearInput);
			self.currentYearElement.addEventListener("increment", onYearInput);

			self.days.addEventListener("click", selectDate);
		}

		if (self.config.enableTime) {
			self.timeContainer.addEventListener("transitionend", positionCalendar);
			self.timeContainer.addEventListener("wheel", function (e) {
				return debounce(updateTime(e), 5);
			});
			self.timeContainer.addEventListener("input", updateTime);
			self.timeContainer.addEventListener("increment", updateTime);
			self.timeContainer.addEventListener("increment", self.debouncedChange);

			self.timeContainer.addEventListener("wheel", self.debouncedChange);
			self.timeContainer.addEventListener("input", self.triggerChange);

			self.hourElement.addEventListener("focus", function () {
				self.hourElement.select();
			});
			self.minuteElement.addEventListener("focus", function () {
				self.minuteElement.select();
			});

			if (self.secondElement) {
				self.secondElement.addEventListener("focus", function () {
					self.secondElement.select();
				});
			}

			if (self.amPM) {
				self.amPM.addEventListener("click", function (e) {
					updateTime(e);
					self.triggerChange(e);
				});
			}
		}
	}

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

	function incrementNumInput(e, delta, inputElem) {
		var input = inputElem || e.target.parentNode.childNodes[0];
		var ev = void 0;

		try {
			ev = new Event("increment", { "bubbles": true });
		} catch (err) {
			ev = window.document.createEvent("CustomEvent");
			ev.initCustomEvent("increment", true, true, {});
		}

		ev.delta = delta;
		input.dispatchEvent(ev);
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

		arrowUp.addEventListener("click", function (e) {
			return incrementNumInput(e, 1);
		});
		arrowDown.addEventListener("click", function (e) {
			return incrementNumInput(e, -1);
		});
		return wrapper;
	}

	function build() {
		var fragment = window.document.createDocumentFragment();
		self.calendarContainer = createElement("div", "flatpickr-calendar");
		self.numInputType = navigator.userAgent.indexOf("MSIE 9.0") > 0 ? "text" : "number";

		if (!self.config.noCalendar) {
			fragment.appendChild(buildMonthNav());
			self.innerContainer = createElement("div", "flatpickr-innerContainer");

			if (self.config.weekNumbers) self.innerContainer.appendChild(buildWeeks());

			self.rContainer = createElement("div", "flatpickr-rContainer");
			self.rContainer.appendChild(buildWeekdays());

			if (!self.days) {
				self.days = createElement("div", "flatpickr-days");
				self.days.tabIndex = -1;
			}

			buildDays();
			self.rContainer.appendChild(self.days);

			self.innerContainer.appendChild(self.rContainer);
			fragment.appendChild(self.innerContainer);
		}

		if (self.config.enableTime) fragment.appendChild(buildTime());

		if (self.config.mode === "range") self.calendarContainer.classList.add("rangeMode");

		self.calendarContainer.appendChild(fragment);

		var customAppend = self.config.appendTo && self.config.appendTo.nodeType;

		if (self.config.inline || self.config.static) {
			self.calendarContainer.classList.add(self.config.inline ? "inline" : "static");

			if (self.config.inline && !customAppend) {
				return self.element.parentNode.insertBefore(self.calendarContainer, (self.altInput || self.input).nextSibling);
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

	function createDay(className, date, dayNumber) {
		var dateIsEnabled = isEnabled(date, true),
		    dayElement = createElement("span", "flatpickr-day " + className, date.getDate());

		dayElement.dateObj = date;

		toggleClass(dayElement, "today", compareDates(date, self.now) === 0);

		if (dateIsEnabled) {

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

	function buildDays(year, month) {
		var firstOfMonth = (new Date(self.currentYear, self.currentMonth, 1).getDay() - self.l10n.firstDayOfWeek + 7) % 7,
		    isRangeMode = self.config.mode === "range";

		self.prevMonthDays = self.utils.getDaysinMonth((self.currentMonth - 1 + 12) % 12);

		var daysInMonth = self.utils.getDaysinMonth(),
		    days = window.document.createDocumentFragment();

		var dayNumber = self.prevMonthDays + 1 - firstOfMonth;

		if (self.config.weekNumbers && self.weekNumbers.firstChild) self.weekNumbers.textContent = "";

		if (isRangeMode) {
			// const dateLimits = self.config.enable.length || self.config.disable.length || self.config.mixDate || self.config.maxDate;
			self.minRangeDate = new Date(self.currentYear, self.currentMonth - 1, dayNumber);
			self.maxRangeDate = new Date(self.currentYear, self.currentMonth + 1, (42 - firstOfMonth) % daysInMonth);
		}

		if (self.days.firstChild) self.days.textContent = "";

		// prepend days from the ending of previous month
		for (; dayNumber <= self.prevMonthDays; dayNumber++) {
			days.appendChild(createDay("prevMonthDay", new Date(self.currentYear, self.currentMonth - 1, dayNumber), dayNumber));
		}

		// Start at 1 since there is no 0th day
		for (dayNumber = 1; dayNumber <= daysInMonth; dayNumber++) {
			days.appendChild(createDay("", new Date(self.currentYear, self.currentMonth, dayNumber), dayNumber));
		}

		// append days from the next month
		for (var dayNum = daysInMonth + 1; dayNum <= 42 - firstOfMonth; dayNum++) {
			days.appendChild(createDay("nextMonthDay", new Date(self.currentYear, self.currentMonth + 1, dayNum % daysInMonth), dayNum));
		}

		if (isRangeMode && self.selectedDates.length === 1 && days.childNodes[0]) {
			self._hidePrevMonthArrow = self._hidePrevMonthArrow || self.minRangeDate > days.childNodes[0].dateObj;

			self._hideNextMonthArrow = self._hideNextMonthArrow || self.maxRangeDate < new Date(self.currentYear, self.currentMonth + 1, 1);
		} else updateNavigationCurrentMonth();

		self.days.appendChild(days);
		return self.days;
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

	function changeMonth(value, is_offset) {
		is_offset = typeof is_offset === "undefined" || is_offset;
		var delta = is_offset ? value : value - self.currentMonth;

		if (delta < 0 && self._hidePrevMonthArrow || delta > 0 && self._hideNextMonthArrow) return;

		self.currentMonth += delta;

		if (self.currentMonth < 0 || self.currentMonth > 11) {
			self.currentYear += self.currentMonth > 11 ? 1 : -1;
			self.currentMonth = (self.currentMonth + 12) % 12;

			triggerEvent("YearChange");
		}

		updateNavigationCurrentMonth();
		buildDays();

		if (!self.config.noCalendar) self.days.focus();

		triggerEvent("MonthChange");
	}

	function clear(triggerChangeEvent) {
		self.input.value = "";

		if (self.altInput) self.altInput.value = "";

		if (self.mobileInput) self.mobileInput.value = "";

		self.selectedDates = [];
		self.latestSelectedDateObj = null;
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
			(self.altInput || self.input).classList.remove("active");
		}

		triggerEvent("Close");
	}

	function destroy(instance) {
		instance = instance || self;
		instance.clear(false);

		window.removeEventListener("resize", instance.debouncedResize);

		window.document.removeEventListener("click", documentClick);
		window.document.removeEventListener("touchstart", documentClick);
		window.document.removeEventListener("blur", documentClick);

		if (instance.timeContainer) instance.timeContainer.removeEventListener("transitionend", positionCalendar);

		if (instance.mobileInput) {
			if (instance.mobileInput.parentNode) instance.mobileInput.parentNode.removeChild(instance.mobileInput);
			delete instance.mobileInput;
		} else if (instance.calendarContainer && instance.calendarContainer.parentNode) instance.calendarContainer.parentNode.removeChild(instance.calendarContainer);

		if (instance.altInput) {
			instance.input.type = "text";
			if (instance.altInput.parentNode) instance.altInput.parentNode.removeChild(instance.altInput);
			delete instance.altInput;
		}

		instance.input.type = instance.input._type;
		instance.input.classList.remove("flatpickr-input");
		instance.input.removeEventListener("focus", open);
		instance.input.removeAttribute("readonly");

		delete instance.input._flatpickr;
	}

	function isCalendarElem(elem) {
		if (self.config.appendTo && self.config.appendTo.contains(elem)) return true;

		var e = elem;
		while (e) {

			if (e === self.calendarContainer) return true;
			e = e.parentNode;
		}

		return false;
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
					self.clear();
					self.redraw();
				}
			}
		}
	}

	function formatDate(frmt, dateObj) {
		if (self.config.formatDate) return self.config.formatDate(frmt, dateObj);

		var chars = frmt.split("");
		return chars.map(function (c, i) {
			return self.formats[c] && chars[i - 1] !== "\\" ? self.formats[c](dateObj) : c !== "\\" ? c : "";
		}).join("");
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
		var ltmin = compareDates(date, self.config.minDate, typeof timeless !== "undefined" ? timeless : !self.minDateHasTime) < 0;
		var gtmax = compareDates(date, self.config.maxDate, typeof timeless !== "undefined" ? timeless : !self.maxDateHasTime) > 0;

		if (ltmin || gtmax) return false;

		if (!self.config.enable.length && !self.config.disable.length) return true;

		var dateToCheck = self.parseDate(date, true); // timeless

		var bool = self.config.enable.length > 0,
		    array = bool ? self.config.enable : self.config.disable;

		for (var i = 0, d; i < array.length; i++) {
			d = array[i];

			if (d instanceof Function && d(dateToCheck)) // disabled by function
				return bool;else if (d instanceof Date && d.getTime() === dateToCheck.getTime())
				// disabled by date
				return bool;else if (typeof d === "string" && self.parseDate(d, true).getTime() === dateToCheck.getTime())
				// disabled by date string
				return bool;else if ( // disabled by range
			(typeof d === "undefined" ? "undefined" : _typeof(d)) === "object" && d.from && d.to && dateToCheck >= d.from && dateToCheck <= d.to) return bool;
		}

		return !bool;
	}

	function onKeyDown(e) {

		if (e.target === (self.altInput || self.input) && e.which === 13) selectDate(e);else if (self.isOpen || self.config.inline) {
			switch (e.key) {
				case "Enter":
					if (self.timeContainer && self.timeContainer.contains(e.target)) updateValue();else selectDate(e);

					break;

				case "Escape":
					// escape
					self.close();
					break;

				case "ArrowLeft":
					if (e.target !== self.input & e.target !== self.altInput) {
						e.preventDefault();
						changeMonth(-1);
						self.currentMonthElement.focus();
					}
					break;

				case "ArrowUp":
					if (!self.timeContainer || !self.timeContainer.contains(e.target)) {
						e.preventDefault();
						self.currentYear++;
						self.redraw();
					} else updateTime(e);

					break;

				case "ArrowRight":
					if (e.target !== self.input & e.target !== self.altInput) {
						e.preventDefault();
						changeMonth(1);
						self.currentMonthElement.focus();
					}
					break;

				case "ArrowDown":
					if (!self.timeContainer || !self.timeContainer.contains(e.target)) {
						e.preventDefault();
						self.currentYear--;
						self.redraw();
					} else updateTime(e);

					break;

				case "Tab":
					if (e.target === self.hourElement) {
						e.preventDefault();
						self.minuteElement.select();
					} else if (e.target === self.minuteElement && self.amPM) {
						e.preventDefault();
						self.amPM.focus();
					}

					break;

				default:
					break;

			}

			triggerEvent("KeyDown", e);
		}
	}

	function onMouseOver(e) {
		if (self.selectedDates.length !== 1 || !e.target.classList.contains("flatpickr-day")) return;

		var hoverDate = e.target.dateObj,
		    initialDate = self.parseDate(self.selectedDates[0], true),
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
			var outOfRange = timestamp < self.minRangeDate.getTime() || timestamp > self.maxRangeDate.getTime();

			if (outOfRange) {
				self.days.childNodes[i].classList.add("notAllowed");
				["inRange", "startRange", "endRange"].forEach(function (c) {
					self.days.childNodes[i].classList.remove(c);
				});
				return "continue";
			} else if (containsDisabled && !outOfRange) return "continue";

			["startRange", "inRange", "endRange", "notAllowed"].forEach(function (c) {
				self.days.childNodes[i].classList.remove(c);
			});

			var minRangeDate = Math.max(self.minRangeDate.getTime(), rangeStartDate),
			    maxRangeDate = Math.min(self.maxRangeDate.getTime(), rangeEndDate);

			e.target.classList.add(hoverDate < self.selectedDates[0] ? "startRange" : "endRange");

			if (initialDate > hoverDate && timestamp === initialDate.getTime()) self.days.childNodes[i].classList.add("endRange");else if (initialDate < hoverDate && timestamp === initialDate.getTime()) self.days.childNodes[i].classList.add("startRange");else if (timestamp >= minRangeDate && timestamp <= maxRangeDate) self.days.childNodes[i].classList.add("inRange");
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

		if (self.isOpen || (self.altInput || self.input).disabled || self.config.inline) return;

		self.isOpen = true;
		self.calendarContainer.classList.add("open");
		positionCalendar();
		(self.altInput || self.input).classList.add("active");

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

			if (self.days) {
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
		}for (var _i = 0; _i < hooks.length; _i++) {
			self.config[hooks[_i]] = arrayify(self.config[hooks[_i]] || []).map(bindToInstance);
		}for (var _i2 = 0; _i2 < self.config.plugins.length; _i2++) {
			var pluginConf = self.config.plugins[_i2](self) || {};
			for (var key in pluginConf) {

				if (Array.isArray(self.config[key]) || ~hooks.indexOf(key)) self.config[key] = arrayify(pluginConf[key]).map(bindToInstance).concat(self.config[key]);else if (typeof userConfig[key] === "undefined") self.config[key] = pluginConf[key];
			}
		}

		triggerEvent("ParseConfig");
	}

	function setupLocale() {
		if (_typeof(self.config.locale) !== "object" && typeof Flatpickr.l10ns[self.config.locale] === "undefined") console.warn("flatpickr: invalid locale " + self.config.locale);

		self.l10n = _extends(Object.create(Flatpickr.l10ns.default), _typeof(self.config.locale) === "object" ? self.config.locale : self.config.locale !== "default" ? Flatpickr.l10ns[self.config.locale] || {} : {});
	}

	function positionCalendar(e) {
		if (e && e.target !== self.timeContainer) return;

		var calendarHeight = self.calendarContainer.offsetHeight,
		    calendarWidth = self.calendarContainer.offsetWidth,
		    configPos = self.config.position,
		    input = self.altInput || self.input,
		    inputBounds = input.getBoundingClientRect(),
		    distanceFromBottom = window.innerHeight - inputBounds.bottom + input.offsetHeight,
		    showOnTop = configPos === "above" || configPos !== "below" && distanceFromBottom < calendarHeight && inputBounds.top > calendarHeight;

		var top = window.pageYOffset + inputBounds.top + (!showOnTop ? input.offsetHeight + 2 : -calendarHeight - 2);

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

		if (self.config.allowInput && e.key === "Enter" && e.target === (self.altInput || self.input)) {
			self.setDate((self.altInput || self.input).value, true, e.target === self.altInput ? self.config.altFormat : self.config.dateFormat);
			return e.target.blur();
		}

		if (!e.target.classList.contains("flatpickr-day") || e.target.classList.contains("disabled") || e.target.classList.contains("notAllowed")) return;

		var selectedDate = self.latestSelectedDateObj = new Date(e.target.dateObj.getTime());

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

		if (selectedDate.getMonth() !== self.currentMonth && self.config.mode !== "range") {
			var isNewYear = self.currentYear !== selectedDate.getFullYear();
			self.currentYear = selectedDate.getFullYear();
			self.currentMonth = selectedDate.getMonth();

			if (isNewYear) triggerEvent("YearChange");

			triggerEvent("MonthChange");
		}

		buildDays();

		if (self.minDateHasTime && self.config.enableTime && compareDates(selectedDate, self.config.minDate) === 0) setHoursFromDate(self.config.minDate);

		updateValue();

		setTimeout(function () {
			return self.showTimeInput = true;
		}, 50);

		if (self.config.mode === "range") {
			if (self.selectedDates.length === 1) {
				onMouseOver(e);

				self._hidePrevMonthArrow = self._hidePrevMonthArrow || self.minRangeDate > self.days.childNodes[0].dateObj;

				self._hideNextMonthArrow = self._hideNextMonthArrow || self.maxRangeDate < new Date(self.currentYear, self.currentMonth + 1, 1);
			} else {
				updateNavigationCurrentMonth();
				self.close();
			}
		}

		if (self.config.enableTime) setTimeout(function () {
			self.hourElement.select();
		}, 451);

		if (self.config.mode === "single" && !self.config.enableTime) self.close();

		triggerEvent("Change");
	}

	function set(option, value) {
		self.config[option] = value;
		self.redraw();
		jumpToDate();
	}

	function setSelectedDate(inputDate, format) {
		if (Array.isArray(inputDate)) self.selectedDates = inputDate.map(function (d) {
			return self.parseDate(d, false, format);
		});else if (inputDate instanceof Date || !isNaN(inputDate)) self.selectedDates = [self.parseDate(inputDate)];else if (inputDate && inputDate.substring) {
			switch (self.config.mode) {
				case "single":
					self.selectedDates = [self.parseDate(inputDate, false, format)];
					break;

				case "multiple":
					self.selectedDates = inputDate.split("; ").map(function (date) {
						return self.parseDate(date, false, format);
					});
					break;

				case "range":
					self.selectedDates = inputDate.split(self.l10n.rangeSeparator).map(function (date) {
						return self.parseDate(date, false, format);
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
		if (!date) return self.clear();

		setSelectedDate(date, format);

		self.showTimeInput = self.selectedDates.length > 0;
		self.latestSelectedDateObj = self.selectedDates[0];

		self.redraw();
		jumpToDate();

		setHoursFromDate();
		updateValue();

		if (triggerChange) triggerEvent("Change");
	}

	function setupDates() {
		function parseDateRules(arr) {
			for (var i = arr.length; i--;) {
				if (typeof arr[i] === "string" || +arr[i]) arr[i] = self.parseDate(arr[i], true);else if (arr[i] && arr[i].from && arr[i].to) {
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

		setSelectedDate(self.config.defaultDate || self.input.value);

		var initialDate = self.selectedDates.length ? self.selectedDates[0] : self.config.minDate && self.config.minDate.getTime() > self.now ? self.config.minDate : self.config.maxDate && self.config.maxDate.getTime() < self.now ? self.config.maxDate : self.now;

		self.currentYear = initialDate.getFullYear();
		self.currentMonth = initialDate.getMonth();

		if (self.selectedDates.length) self.latestSelectedDateObj = self.selectedDates[0];

		self.minDateHasTime = self.config.minDate && (self.config.minDate.getHours() || self.config.minDate.getMinutes() || self.config.minDate.getSeconds());

		self.maxDateHasTime = self.config.maxDate && (self.config.maxDate.getHours() || self.config.maxDate.getMinutes() || self.config.maxDate.getSeconds());

		Object.defineProperty(self, "latestSelectedDateObj", {
			get: function get() {
				return self._selectedDateObj || self.selectedDates[self.selectedDates.length - 1] || null;
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

		if (self.config.altInput) {
			// replicate self.element
			self.altInput = createElement(self.input.nodeName, self.input.className + " " + self.config.altInputClass);
			self.altInput.placeholder = self.input.placeholder;
			self.altInput.type = "text";
			self.input.type = "hidden";

			if (!self.config.static && self.input.parentNode) self.input.parentNode.insertBefore(self.altInput, self.input.nextSibling);
		}

		if (!self.config.allowInput) (self.altInput || self.input).setAttribute("readonly", "readonly");
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
			self.mobileInput.defaultValue = self.mobileInput.value = formatDate(self.mobileFormatStr, self.selectedDates[0]);
		}

		if (self.config.minDate) self.mobileInput.min = formatDate("Y-m-d", self.config.minDate);

		if (self.config.maxDate) self.mobileInput.max = formatDate("Y-m-d", self.config.maxDate);

		self.input.type = "hidden";
		if (self.config.altInput) self.altInput.type = "hidden";

		try {
			self.input.parentNode.insertBefore(self.mobileInput, self.input.nextSibling);
		} catch (e) {
			//
		}

		self.mobileInput.addEventListener("change", function (e) {
			self.latestSelectedDateObj = self.parseDate(e.target.value);
			self.setDate(self.latestSelectedDateObj);
			triggerEvent("Change");
			triggerEvent("Close");
		});
	}

	function toggle() {
		if (self.isOpen) self.close();else self.open();
	}

	function triggerEvent(event, data) {
		var hooks = self.config["on" + event];

		if (hooks) {
			for (var i = 0; hooks[i] && i < hooks.length; i++) {
				hooks[i](self.selectedDates, self.input && self.input.value, self, data);
			}
		}

		if (event === "Change") {
			if (typeof Event === "function" && Event.constructor) {
				self.input.dispatchEvent(new Event("change", { "bubbles": true }));

				// many front-end frameworks bind to the input event
				self.input.dispatchEvent(new Event("input", { "bubbles": true }));
			}

			/* istanbul ignore next */
			else {
					if (window.document.createEvent !== undefined) return self.input.dispatchEvent(self.changeEvent);

					self.input.fireEvent("onchange");
				}
		}
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

	function updateValue() {
		if (!self.selectedDates.length) return self.clear();

		if (self.isMobile) {
			self.mobileInput.value = self.selectedDates.length ? formatDate(self.mobileFormatStr, self.latestSelectedDateObj) : "";
		}

		var joinChar = self.config.mode !== "range" ? "; " : self.l10n.rangeSeparator;

		self.input.value = self.selectedDates.map(function (dObj) {
			return formatDate(self.config.dateFormat, dObj);
		}).join(joinChar);

		if (self.config.altInput) {
			self.altInput.value = self.selectedDates.map(function (dObj) {
				return formatDate(self.config.altFormat, dObj);
			}).join(joinChar);
		}

		triggerEvent("ValueUpdate");
	}

	function yearScroll(e) {
		e.preventDefault();

		var delta = Math.max(-1, Math.min(1, e.wheelDelta || -e.deltaY)),
		    newYear = parseInt(e.target.value, 10) + delta;

		changeYear(newYear);
		e.target.value = self.currentYear;
	}

	function createElement(tag, className, content) {
		var e = window.document.createElement(tag);
		className = className || "";
		content = content || "";

		e.className = className;

		if (content) e.textContent = content;

		return e;
	}

	function arrayify(obj) {
		if (Array.isArray(obj)) return obj;
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

		if (e.type !== "input" && !isKeyDown && (e.target.value || e.target.textContent).length >= 2 // typed two digits
		) {
				e.target.focus();
				e.target.blur();
			}

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

	position: "top",

	/* if true, dates will be parsed, formatted, and displayed in UTC.
 preloading date strings w/ timezones is recommended but not necessary */
	utc: false,

	// wrap: see https://chmln.github.io/flatpickr/#strap
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

	// display time picker in 24 hour mode
	time_24hr: false,

	// enables the time picker functionality
	enableTime: false,

	// noCalendar: true will hide the calendar. use for a time picker along w/ enableTime
	noCalendar: false,

	// more date format chars at https://chmln.github.io/flatpickr/#dateformat
	dateFormat: "Y-m-d",

	// altInput - see https://chmln.github.io/flatpickr/#altinput
	altInput: false,

	// the created altInput element will have this class.
	altInputClass: "flatpickr-input form-control input",

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
		date.setHours(0, 0, 0, 0);

		// Thursday in current week decides the year.
		date.setDate(date.getDate() + 3 - (date.getDay() + 6) % 7);
		// January 4 is always in week 1.
		var week1 = new Date(date.getFullYear(), 0, 4);
		// Adjust to Thursday in week 1 and count number of weeks from date to week1.
		return 1 + Math.round(((date.getTime() - week1.getTime()) / 86400000 - 3 + (week1.getDay() + 6) % 7) / 7);
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
	static: false,

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
	onClose: [], // function (dateObj, dateStr) {}

	// onChange callback when user selects a date or time
	onChange: [], // function (dateObj, dateStr) {}

	// called for every day element
	onDayCreate: [],

	// called every time the month is changed
	onMonthChange: [],

	// called every time calendar is opened
	onOpen: [], // function (dateObj, dateStr) {}

	// called after the configuration has been parsed
	onParseConfig: [],

	// called after calendar is ready
	onReady: [], // function (dateObj, dateStr) {}

	// called after input value updated
	onValueUpdate: [],

	// called every time the year is changed
	onYearChange: [],

	onKeyDown: []
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

	revFormat: {
		D: function D() {},
		F: function F(dateObj, monthName) {
			dateObj.setMonth(this.l10n.months.longhand.indexOf(monthName));
		},
		H: function H(dateObj, hour) {
			return dateObj.setHours(parseFloat(hour));
		},
		J: function J(dateObj, day) {
			return dateObj.setDate(parseFloat(day));
		},
		K: function K(dateObj, amPM) {
			var hours = dateObj.getHours();

			if (hours !== 12) dateObj.setHours(hours % 12 + 12 * /pm/i.test(amPM));
		},
		M: function M(dateObj, shortMonth) {
			dateObj.setMonth(this.l10n.months.shorthand.indexOf(shortMonth));
		},
		S: function S(dateObj, seconds) {
			return dateObj.setSeconds(seconds);
		},
		W: function W() {},
		Y: function Y(dateObj, year) {
			return dateObj.setFullYear(year);
		},
		Z: function Z(dateObj, ISODate) {
			return dateObj = new Date(ISODate);
		},

		d: function d(dateObj, day) {
			return dateObj.setDate(parseFloat(day));
		},
		h: function h(dateObj, hour) {
			return dateObj.setHours(parseFloat(hour));
		},
		i: function i(dateObj, minutes) {
			return dateObj.setMinutes(parseFloat(minutes));
		},
		j: function j(dateObj, day) {
			return dateObj.setDate(parseFloat(day));
		},
		l: function l() {},
		m: function m(dateObj, month) {
			return dateObj.setMonth(parseFloat(month) - 1);
		},
		n: function n(dateObj, month) {
			return dateObj.setMonth(parseFloat(month) - 1);
		},
		s: function s(dateObj, seconds) {
			return dateObj.setSeconds(parseFloat(seconds));
		},
		w: function w() {},
		y: function y(dateObj, year) {
			return dateObj.setFullYear(2000 + parseFloat(year));
		}
	},

	tokenRegex: {
		D: "(\\w+)",
		F: "(\\w+)",
		H: "(\\d\\d|\\d)",
		J: "(\\d\\d|\\d)\\w+",
		K: "(\\w+)",
		M: "(\\w+)",
		S: "(\\d\\d|\\d)",
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

	parseDate: function parseDate(date, timeless, givenFormat) {
		if (!date) return null;

		var date_orig = date;

		if (date.toFixed) // timestamp
			date = new Date(date);else if (typeof date === "string") {
			var format = typeof givenFormat === "string" ? givenFormat : this.config.dateFormat;
			date = date.trim();

			if (date === "today") {
				date = new Date();
				timeless = true;
			} else if (this.config && this.config.parseDate) date = this.config.parseDate(date);else if (/Z$/.test(date) || /GMT$/.test(date)) // datestrings w/ timezone
				date = new Date(date);else {
				var parsedDate = this.config.noCalendar ? new Date(new Date().setHours(0, 0, 0, 0)) : new Date(new Date().getFullYear(), 0, 1, 0, 0, 0, 0);

				var matched = false;

				for (var i = 0, matchIndex = 0, regexStr = ""; i < format.length; i++) {
					var token = format[i];
					var isBackSlash = token === "\\";
					var escaped = format[i - 1] === "\\" || isBackSlash;
					if (this.tokenRegex[token] && !escaped) {
						regexStr += this.tokenRegex[token];
						var match = new RegExp(regexStr).exec(date);
						if (match && (matched = true)) this.revFormat[token](parsedDate, match[++matchIndex]);
					} else if (!isBackSlash) regexStr += "."; // don't really care
				}

				date = matched ? parsedDate : null;
			}
		} else if (date instanceof Date) date = new Date(date.getTime()); // create a copy

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

// IE9 classList polyfill
/* istanbul ignore next */
if (!window.document.documentElement.classList && Object.defineProperty && typeof HTMLElement !== "undefined") {
	Object.defineProperty(HTMLElement.prototype, "classList", {
		get: function get() {
			var self = this;
			function update(fn) {
				return function (value) {
					var classes = self.className.split(/\s+/),
					    index = classes.indexOf(value);

					fn(classes, index, value);
					self.className = classes.join(" ");
				};
			}

			var ret = {
				add: update(function (classes, index, value) {
					if (!~index) classes.push(value);
				}),

				remove: update(function (classes, index) {
					if (~index) classes.splice(index, 1);
				}),

				toggle: update(function (classes, index, value) {
					if (~index) classes.splice(index, 1);else classes.push(value);
				}),

				contains: function contains(value) {
					return !!~self.className.split(/\s+/).indexOf(value);
				},

				item: function item(i) {
					return self.className.split(/\s+/)[i] || null;
				}
			};

			Object.defineProperty(ret, "length", {
				get: function get() {
					return self.className.split(/\s+/).length;
				}
			});

			return ret;
		}
	});
}

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
  if (typeof options === 'function') {
    options = options.options
  }
  makeOptionsHot(id, options)
  var record = map[id]
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
    hotAPI.createRecord("data-v-079b469c", __vue__options__)
  } else {
    hotAPI.reload("data-v-079b469c", __vue__options__)
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
    hotAPI.createRecord("data-v-a17d29e6", __vue__options__)
  } else {
    hotAPI.reload("data-v-a17d29e6", __vue__options__)
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
            var requestParams = (0, _assign2.default)({ query: filter }, this.ajaxParams);
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
    hotAPI.createRecord("data-v-79d6f4b7", __vue__options__)
  } else {
    hotAPI.reload("data-v-79d6f4b7", __vue__options__)
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
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvYmFiZWwtcnVudGltZS9jb3JlLWpzL2pzb24vc3RyaW5naWZ5LmpzIiwibm9kZV9tb2R1bGVzL2JhYmVsLXJ1bnRpbWUvY29yZS1qcy9vYmplY3QvYXNzaWduLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYS1mdW5jdGlvbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYW4tb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19hcnJheS1pbmNsdWRlcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY29mLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jb3JlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jdHguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2RlZmluZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2Rlc2NyaXB0b3JzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kb20tY3JlYXRlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19lbnVtLWJ1Zy1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19leHBvcnQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2ZhaWxzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19nbG9iYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2hhcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faGlkZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faWU4LWRvbS1kZWZpbmUuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lvYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lzLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWFzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWRwLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtZ29wcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWtleXMtaW50ZXJuYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtcGllLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19wcm9wZXJ0eS1kZXNjLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQta2V5LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLWluZGV4LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pbnRlZ2VyLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1sZW5ndGguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8tcHJpbWl0aXZlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL191aWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvZmxhdHBpY2tyL2Rpc3QvZmxhdHBpY2tyLmpzIiwibm9kZV9tb2R1bGVzL3Z1ZS1ob3QtcmVsb2FkLWFwaS9pbmRleC5qcyIsInZ1ZVxcdnVlXFxEYXRlUGlja2VyLnZ1ZT81MGE1MDhmMyIsInZ1ZVxcdnVlXFxJbnRlcnZpZXdUYWJsZS52dWU/ZWE2NTJjNTYiLCJ2dWVcXHZ1ZVxcVHlwZWFoZWFkLnZ1ZT8xMjk0NTRiYyIsInZ1ZVxcdnVlXFx0cm91Ymxlc2hvb3RpbmcuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUNBQTs7QUNBQTs7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7O0FDREE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDcEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTs7QUNEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ25CQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzVEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1BBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaENBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2ZBOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDUEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNMQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDWEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDOStEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7QUMzSEE7Ozs7Ozs7QUFHQTtBQUNBO0FBQ0E7QUFEQTtBQUdBO0FBQ0E7QUFDQTtBQUZBO0FBSUE7QUFDQTtBQUNBO0FBQUE7QUFBQTtBQUZBO0FBSUE7QUFDQTtBQUNBO0FBRkE7QUFaQTtBQWlCQTtBQUNBO0FBQ0E7QUFEQTtBQUdBOztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBSEE7QUFLQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQU5BO0FBUUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQVBBOztBQVVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUVBO0FBQ0E7QUFHQTtBQUNBO0FBQ0E7QUFmQTtBQXhEQTs7Ozs7QUFmQTtBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7QUNXQTtBQUNBO0FBQ0E7QUFDQTtBQUFBO0FBQUE7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUFBO0FBQUE7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUFBO0FBQUE7QUFGQTtBQVRBO0FBY0E7QUFDQTtBQUNBO0FBREE7QUFHQTs7QUFDQTtBQUVBO0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBaEJBO0FBa0JBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBREE7QUFHQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFaQTs7QUFlQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUF2RUE7Ozs7O0FBVkE7QUFBQTs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQzZCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBSEE7QUFLQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBTkE7QUFRQTtBQUFBOztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBQ0E7QUFBQTs7QUFBQTs7QUFDQTtBQUNBO0FBQ0E7QUFFQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBaERBO0FBOUJBOzs7OztBQTdCQTtBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7QUNBRTs7OztBQUNGOzs7O0FBQ0E7Ozs7QUFDQTs7OztBQUNBOzs7O0FBQ0E7Ozs7OztBQUVBLGNBQUksR0FBSjtBQUNBLGNBQUksR0FBSjs7QUFFQSxjQUFJLFNBQUosQ0FBYyxXQUFkO0FBQ0EsY0FBSSxTQUFKLENBQWMsV0FBZDtBQUNBLGNBQUksU0FBSixDQUFjLGlCQUFkIiwiZmlsZSI6ImdlbmVyYXRlZC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzQ29udGVudCI6WyIoZnVuY3Rpb24gZSh0LG4scil7ZnVuY3Rpb24gcyhvLHUpe2lmKCFuW29dKXtpZighdFtvXSl7dmFyIGE9dHlwZW9mIHJlcXVpcmU9PVwiZnVuY3Rpb25cIiYmcmVxdWlyZTtpZighdSYmYSlyZXR1cm4gYShvLCEwKTtpZihpKXJldHVybiBpKG8sITApO3ZhciBmPW5ldyBFcnJvcihcIkNhbm5vdCBmaW5kIG1vZHVsZSAnXCIrbytcIidcIik7dGhyb3cgZi5jb2RlPVwiTU9EVUxFX05PVF9GT1VORFwiLGZ9dmFyIGw9bltvXT17ZXhwb3J0czp7fX07dFtvXVswXS5jYWxsKGwuZXhwb3J0cyxmdW5jdGlvbihlKXt2YXIgbj10W29dWzFdW2VdO3JldHVybiBzKG4/bjplKX0sbCxsLmV4cG9ydHMsZSx0LG4scil9cmV0dXJuIG5bb10uZXhwb3J0c312YXIgaT10eXBlb2YgcmVxdWlyZT09XCJmdW5jdGlvblwiJiZyZXF1aXJlO2Zvcih2YXIgbz0wO288ci5sZW5ndGg7bysrKXMocltvXSk7cmV0dXJuIHN9KSIsIm1vZHVsZS5leHBvcnRzID0geyBcImRlZmF1bHRcIjogcmVxdWlyZShcImNvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeVwiKSwgX19lc01vZHVsZTogdHJ1ZSB9OyIsIm1vZHVsZS5leHBvcnRzID0geyBcImRlZmF1bHRcIjogcmVxdWlyZShcImNvcmUtanMvbGlicmFyeS9mbi9vYmplY3QvYXNzaWduXCIpLCBfX2VzTW9kdWxlOiB0cnVlIH07IiwidmFyIGNvcmUgID0gcmVxdWlyZSgnLi4vLi4vbW9kdWxlcy9fY29yZScpXG4gICwgJEpTT04gPSBjb3JlLkpTT04gfHwgKGNvcmUuSlNPTiA9IHtzdHJpbmdpZnk6IEpTT04uc3RyaW5naWZ5fSk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uIHN0cmluZ2lmeShpdCl7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW51c2VkLXZhcnNcbiAgcmV0dXJuICRKU09OLnN0cmluZ2lmeS5hcHBseSgkSlNPTiwgYXJndW1lbnRzKTtcbn07IiwicmVxdWlyZSgnLi4vLi4vbW9kdWxlcy9lczYub2JqZWN0LmFzc2lnbicpO1xubW9kdWxlLmV4cG9ydHMgPSByZXF1aXJlKCcuLi8uLi9tb2R1bGVzL19jb3JlJykuT2JqZWN0LmFzc2lnbjsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgaWYodHlwZW9mIGl0ICE9ICdmdW5jdGlvbicpdGhyb3cgVHlwZUVycm9yKGl0ICsgJyBpcyBub3QgYSBmdW5jdGlvbiEnKTtcbiAgcmV0dXJuIGl0O1xufTsiLCJ2YXIgaXNPYmplY3QgPSByZXF1aXJlKCcuL19pcy1vYmplY3QnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICBpZighaXNPYmplY3QoaXQpKXRocm93IFR5cGVFcnJvcihpdCArICcgaXMgbm90IGFuIG9iamVjdCEnKTtcbiAgcmV0dXJuIGl0O1xufTsiLCIvLyBmYWxzZSAtPiBBcnJheSNpbmRleE9mXG4vLyB0cnVlICAtPiBBcnJheSNpbmNsdWRlc1xudmFyIHRvSU9iamVjdCA9IHJlcXVpcmUoJy4vX3RvLWlvYmplY3QnKVxuICAsIHRvTGVuZ3RoICA9IHJlcXVpcmUoJy4vX3RvLWxlbmd0aCcpXG4gICwgdG9JbmRleCAgID0gcmVxdWlyZSgnLi9fdG8taW5kZXgnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oSVNfSU5DTFVERVMpe1xuICByZXR1cm4gZnVuY3Rpb24oJHRoaXMsIGVsLCBmcm9tSW5kZXgpe1xuICAgIHZhciBPICAgICAgPSB0b0lPYmplY3QoJHRoaXMpXG4gICAgICAsIGxlbmd0aCA9IHRvTGVuZ3RoKE8ubGVuZ3RoKVxuICAgICAgLCBpbmRleCAgPSB0b0luZGV4KGZyb21JbmRleCwgbGVuZ3RoKVxuICAgICAgLCB2YWx1ZTtcbiAgICAvLyBBcnJheSNpbmNsdWRlcyB1c2VzIFNhbWVWYWx1ZVplcm8gZXF1YWxpdHkgYWxnb3JpdGhtXG4gICAgaWYoSVNfSU5DTFVERVMgJiYgZWwgIT0gZWwpd2hpbGUobGVuZ3RoID4gaW5kZXgpe1xuICAgICAgdmFsdWUgPSBPW2luZGV4KytdO1xuICAgICAgaWYodmFsdWUgIT0gdmFsdWUpcmV0dXJuIHRydWU7XG4gICAgLy8gQXJyYXkjdG9JbmRleCBpZ25vcmVzIGhvbGVzLCBBcnJheSNpbmNsdWRlcyAtIG5vdFxuICAgIH0gZWxzZSBmb3IoO2xlbmd0aCA+IGluZGV4OyBpbmRleCsrKWlmKElTX0lOQ0xVREVTIHx8IGluZGV4IGluIE8pe1xuICAgICAgaWYoT1tpbmRleF0gPT09IGVsKXJldHVybiBJU19JTkNMVURFUyB8fCBpbmRleCB8fCAwO1xuICAgIH0gcmV0dXJuICFJU19JTkNMVURFUyAmJiAtMTtcbiAgfTtcbn07IiwidmFyIHRvU3RyaW5nID0ge30udG9TdHJpbmc7XG5cbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gdG9TdHJpbmcuY2FsbChpdCkuc2xpY2UoOCwgLTEpO1xufTsiLCJ2YXIgY29yZSA9IG1vZHVsZS5leHBvcnRzID0ge3ZlcnNpb246ICcyLjQuMCd9O1xuaWYodHlwZW9mIF9fZSA9PSAnbnVtYmVyJylfX2UgPSBjb3JlOyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVuZGVmIiwiLy8gb3B0aW9uYWwgLyBzaW1wbGUgY29udGV4dCBiaW5kaW5nXG52YXIgYUZ1bmN0aW9uID0gcmVxdWlyZSgnLi9fYS1mdW5jdGlvbicpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihmbiwgdGhhdCwgbGVuZ3RoKXtcbiAgYUZ1bmN0aW9uKGZuKTtcbiAgaWYodGhhdCA9PT0gdW5kZWZpbmVkKXJldHVybiBmbjtcbiAgc3dpdGNoKGxlbmd0aCl7XG4gICAgY2FzZSAxOiByZXR1cm4gZnVuY3Rpb24oYSl7XG4gICAgICByZXR1cm4gZm4uY2FsbCh0aGF0LCBhKTtcbiAgICB9O1xuICAgIGNhc2UgMjogcmV0dXJuIGZ1bmN0aW9uKGEsIGIpe1xuICAgICAgcmV0dXJuIGZuLmNhbGwodGhhdCwgYSwgYik7XG4gICAgfTtcbiAgICBjYXNlIDM6IHJldHVybiBmdW5jdGlvbihhLCBiLCBjKXtcbiAgICAgIHJldHVybiBmbi5jYWxsKHRoYXQsIGEsIGIsIGMpO1xuICAgIH07XG4gIH1cbiAgcmV0dXJuIGZ1bmN0aW9uKC8qIC4uLmFyZ3MgKi8pe1xuICAgIHJldHVybiBmbi5hcHBseSh0aGF0LCBhcmd1bWVudHMpO1xuICB9O1xufTsiLCIvLyA3LjIuMSBSZXF1aXJlT2JqZWN0Q29lcmNpYmxlKGFyZ3VtZW50KVxubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIGlmKGl0ID09IHVuZGVmaW5lZCl0aHJvdyBUeXBlRXJyb3IoXCJDYW4ndCBjYWxsIG1ldGhvZCBvbiAgXCIgKyBpdCk7XG4gIHJldHVybiBpdDtcbn07IiwiLy8gVGhhbmsncyBJRTggZm9yIGhpcyBmdW5ueSBkZWZpbmVQcm9wZXJ0eVxubW9kdWxlLmV4cG9ydHMgPSAhcmVxdWlyZSgnLi9fZmFpbHMnKShmdW5jdGlvbigpe1xuICByZXR1cm4gT2JqZWN0LmRlZmluZVByb3BlcnR5KHt9LCAnYScsIHtnZXQ6IGZ1bmN0aW9uKCl7IHJldHVybiA3OyB9fSkuYSAhPSA3O1xufSk7IiwidmFyIGlzT2JqZWN0ID0gcmVxdWlyZSgnLi9faXMtb2JqZWN0JylcbiAgLCBkb2N1bWVudCA9IHJlcXVpcmUoJy4vX2dsb2JhbCcpLmRvY3VtZW50XG4gIC8vIGluIG9sZCBJRSB0eXBlb2YgZG9jdW1lbnQuY3JlYXRlRWxlbWVudCBpcyAnb2JqZWN0J1xuICAsIGlzID0gaXNPYmplY3QoZG9jdW1lbnQpICYmIGlzT2JqZWN0KGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBpcyA/IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoaXQpIDoge307XG59OyIsIi8vIElFIDgtIGRvbid0IGVudW0gYnVnIGtleXNcbm1vZHVsZS5leHBvcnRzID0gKFxuICAnY29uc3RydWN0b3IsaGFzT3duUHJvcGVydHksaXNQcm90b3R5cGVPZixwcm9wZXJ0eUlzRW51bWVyYWJsZSx0b0xvY2FsZVN0cmluZyx0b1N0cmluZyx2YWx1ZU9mJ1xuKS5zcGxpdCgnLCcpOyIsInZhciBnbG9iYWwgICAgPSByZXF1aXJlKCcuL19nbG9iYWwnKVxuICAsIGNvcmUgICAgICA9IHJlcXVpcmUoJy4vX2NvcmUnKVxuICAsIGN0eCAgICAgICA9IHJlcXVpcmUoJy4vX2N0eCcpXG4gICwgaGlkZSAgICAgID0gcmVxdWlyZSgnLi9faGlkZScpXG4gICwgUFJPVE9UWVBFID0gJ3Byb3RvdHlwZSc7XG5cbnZhciAkZXhwb3J0ID0gZnVuY3Rpb24odHlwZSwgbmFtZSwgc291cmNlKXtcbiAgdmFyIElTX0ZPUkNFRCA9IHR5cGUgJiAkZXhwb3J0LkZcbiAgICAsIElTX0dMT0JBTCA9IHR5cGUgJiAkZXhwb3J0LkdcbiAgICAsIElTX1NUQVRJQyA9IHR5cGUgJiAkZXhwb3J0LlNcbiAgICAsIElTX1BST1RPICA9IHR5cGUgJiAkZXhwb3J0LlBcbiAgICAsIElTX0JJTkQgICA9IHR5cGUgJiAkZXhwb3J0LkJcbiAgICAsIElTX1dSQVAgICA9IHR5cGUgJiAkZXhwb3J0LldcbiAgICAsIGV4cG9ydHMgICA9IElTX0dMT0JBTCA/IGNvcmUgOiBjb3JlW25hbWVdIHx8IChjb3JlW25hbWVdID0ge30pXG4gICAgLCBleHBQcm90byAgPSBleHBvcnRzW1BST1RPVFlQRV1cbiAgICAsIHRhcmdldCAgICA9IElTX0dMT0JBTCA/IGdsb2JhbCA6IElTX1NUQVRJQyA/IGdsb2JhbFtuYW1lXSA6IChnbG9iYWxbbmFtZV0gfHwge30pW1BST1RPVFlQRV1cbiAgICAsIGtleSwgb3duLCBvdXQ7XG4gIGlmKElTX0dMT0JBTClzb3VyY2UgPSBuYW1lO1xuICBmb3Ioa2V5IGluIHNvdXJjZSl7XG4gICAgLy8gY29udGFpbnMgaW4gbmF0aXZlXG4gICAgb3duID0gIUlTX0ZPUkNFRCAmJiB0YXJnZXQgJiYgdGFyZ2V0W2tleV0gIT09IHVuZGVmaW5lZDtcbiAgICBpZihvd24gJiYga2V5IGluIGV4cG9ydHMpY29udGludWU7XG4gICAgLy8gZXhwb3J0IG5hdGl2ZSBvciBwYXNzZWRcbiAgICBvdXQgPSBvd24gPyB0YXJnZXRba2V5XSA6IHNvdXJjZVtrZXldO1xuICAgIC8vIHByZXZlbnQgZ2xvYmFsIHBvbGx1dGlvbiBmb3IgbmFtZXNwYWNlc1xuICAgIGV4cG9ydHNba2V5XSA9IElTX0dMT0JBTCAmJiB0eXBlb2YgdGFyZ2V0W2tleV0gIT0gJ2Z1bmN0aW9uJyA/IHNvdXJjZVtrZXldXG4gICAgLy8gYmluZCB0aW1lcnMgdG8gZ2xvYmFsIGZvciBjYWxsIGZyb20gZXhwb3J0IGNvbnRleHRcbiAgICA6IElTX0JJTkQgJiYgb3duID8gY3R4KG91dCwgZ2xvYmFsKVxuICAgIC8vIHdyYXAgZ2xvYmFsIGNvbnN0cnVjdG9ycyBmb3IgcHJldmVudCBjaGFuZ2UgdGhlbSBpbiBsaWJyYXJ5XG4gICAgOiBJU19XUkFQICYmIHRhcmdldFtrZXldID09IG91dCA/IChmdW5jdGlvbihDKXtcbiAgICAgIHZhciBGID0gZnVuY3Rpb24oYSwgYiwgYyl7XG4gICAgICAgIGlmKHRoaXMgaW5zdGFuY2VvZiBDKXtcbiAgICAgICAgICBzd2l0Y2goYXJndW1lbnRzLmxlbmd0aCl7XG4gICAgICAgICAgICBjYXNlIDA6IHJldHVybiBuZXcgQztcbiAgICAgICAgICAgIGNhc2UgMTogcmV0dXJuIG5ldyBDKGEpO1xuICAgICAgICAgICAgY2FzZSAyOiByZXR1cm4gbmV3IEMoYSwgYik7XG4gICAgICAgICAgfSByZXR1cm4gbmV3IEMoYSwgYiwgYyk7XG4gICAgICAgIH0gcmV0dXJuIEMuYXBwbHkodGhpcywgYXJndW1lbnRzKTtcbiAgICAgIH07XG4gICAgICBGW1BST1RPVFlQRV0gPSBDW1BST1RPVFlQRV07XG4gICAgICByZXR1cm4gRjtcbiAgICAvLyBtYWtlIHN0YXRpYyB2ZXJzaW9ucyBmb3IgcHJvdG90eXBlIG1ldGhvZHNcbiAgICB9KShvdXQpIDogSVNfUFJPVE8gJiYgdHlwZW9mIG91dCA9PSAnZnVuY3Rpb24nID8gY3R4KEZ1bmN0aW9uLmNhbGwsIG91dCkgOiBvdXQ7XG4gICAgLy8gZXhwb3J0IHByb3RvIG1ldGhvZHMgdG8gY29yZS4lQ09OU1RSVUNUT1IlLm1ldGhvZHMuJU5BTUUlXG4gICAgaWYoSVNfUFJPVE8pe1xuICAgICAgKGV4cG9ydHMudmlydHVhbCB8fCAoZXhwb3J0cy52aXJ0dWFsID0ge30pKVtrZXldID0gb3V0O1xuICAgICAgLy8gZXhwb3J0IHByb3RvIG1ldGhvZHMgdG8gY29yZS4lQ09OU1RSVUNUT1IlLnByb3RvdHlwZS4lTkFNRSVcbiAgICAgIGlmKHR5cGUgJiAkZXhwb3J0LlIgJiYgZXhwUHJvdG8gJiYgIWV4cFByb3RvW2tleV0paGlkZShleHBQcm90bywga2V5LCBvdXQpO1xuICAgIH1cbiAgfVxufTtcbi8vIHR5cGUgYml0bWFwXG4kZXhwb3J0LkYgPSAxOyAgIC8vIGZvcmNlZFxuJGV4cG9ydC5HID0gMjsgICAvLyBnbG9iYWxcbiRleHBvcnQuUyA9IDQ7ICAgLy8gc3RhdGljXG4kZXhwb3J0LlAgPSA4OyAgIC8vIHByb3RvXG4kZXhwb3J0LkIgPSAxNjsgIC8vIGJpbmRcbiRleHBvcnQuVyA9IDMyOyAgLy8gd3JhcFxuJGV4cG9ydC5VID0gNjQ7ICAvLyBzYWZlXG4kZXhwb3J0LlIgPSAxMjg7IC8vIHJlYWwgcHJvdG8gbWV0aG9kIGZvciBgbGlicmFyeWAgXG5tb2R1bGUuZXhwb3J0cyA9ICRleHBvcnQ7IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihleGVjKXtcbiAgdHJ5IHtcbiAgICByZXR1cm4gISFleGVjKCk7XG4gIH0gY2F0Y2goZSl7XG4gICAgcmV0dXJuIHRydWU7XG4gIH1cbn07IiwiLy8gaHR0cHM6Ly9naXRodWIuY29tL3psb2lyb2NrL2NvcmUtanMvaXNzdWVzLzg2I2lzc3VlY29tbWVudC0xMTU3NTkwMjhcbnZhciBnbG9iYWwgPSBtb2R1bGUuZXhwb3J0cyA9IHR5cGVvZiB3aW5kb3cgIT0gJ3VuZGVmaW5lZCcgJiYgd2luZG93Lk1hdGggPT0gTWF0aFxuICA/IHdpbmRvdyA6IHR5cGVvZiBzZWxmICE9ICd1bmRlZmluZWQnICYmIHNlbGYuTWF0aCA9PSBNYXRoID8gc2VsZiA6IEZ1bmN0aW9uKCdyZXR1cm4gdGhpcycpKCk7XG5pZih0eXBlb2YgX19nID09ICdudW1iZXInKV9fZyA9IGdsb2JhbDsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bmRlZiIsInZhciBoYXNPd25Qcm9wZXJ0eSA9IHt9Lmhhc093blByb3BlcnR5O1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCwga2V5KXtcbiAgcmV0dXJuIGhhc093blByb3BlcnR5LmNhbGwoaXQsIGtleSk7XG59OyIsInZhciBkUCAgICAgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWRwJylcbiAgLCBjcmVhdGVEZXNjID0gcmVxdWlyZSgnLi9fcHJvcGVydHktZGVzYycpO1xubW9kdWxlLmV4cG9ydHMgPSByZXF1aXJlKCcuL19kZXNjcmlwdG9ycycpID8gZnVuY3Rpb24ob2JqZWN0LCBrZXksIHZhbHVlKXtcbiAgcmV0dXJuIGRQLmYob2JqZWN0LCBrZXksIGNyZWF0ZURlc2MoMSwgdmFsdWUpKTtcbn0gOiBmdW5jdGlvbihvYmplY3QsIGtleSwgdmFsdWUpe1xuICBvYmplY3Rba2V5XSA9IHZhbHVlO1xuICByZXR1cm4gb2JqZWN0O1xufTsiLCJtb2R1bGUuZXhwb3J0cyA9ICFyZXF1aXJlKCcuL19kZXNjcmlwdG9ycycpICYmICFyZXF1aXJlKCcuL19mYWlscycpKGZ1bmN0aW9uKCl7XG4gIHJldHVybiBPYmplY3QuZGVmaW5lUHJvcGVydHkocmVxdWlyZSgnLi9fZG9tLWNyZWF0ZScpKCdkaXYnKSwgJ2EnLCB7Z2V0OiBmdW5jdGlvbigpeyByZXR1cm4gNzsgfX0pLmEgIT0gNztcbn0pOyIsIi8vIGZhbGxiYWNrIGZvciBub24tYXJyYXktbGlrZSBFUzMgYW5kIG5vbi1lbnVtZXJhYmxlIG9sZCBWOCBzdHJpbmdzXG52YXIgY29mID0gcmVxdWlyZSgnLi9fY29mJyk7XG5tb2R1bGUuZXhwb3J0cyA9IE9iamVjdCgneicpLnByb3BlcnR5SXNFbnVtZXJhYmxlKDApID8gT2JqZWN0IDogZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gY29mKGl0KSA9PSAnU3RyaW5nJyA/IGl0LnNwbGl0KCcnKSA6IE9iamVjdChpdCk7XG59OyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gdHlwZW9mIGl0ID09PSAnb2JqZWN0JyA/IGl0ICE9PSBudWxsIDogdHlwZW9mIGl0ID09PSAnZnVuY3Rpb24nO1xufTsiLCIndXNlIHN0cmljdCc7XG4vLyAxOS4xLjIuMSBPYmplY3QuYXNzaWduKHRhcmdldCwgc291cmNlLCAuLi4pXG52YXIgZ2V0S2V5cyAgPSByZXF1aXJlKCcuL19vYmplY3Qta2V5cycpXG4gICwgZ09QUyAgICAgPSByZXF1aXJlKCcuL19vYmplY3QtZ29wcycpXG4gICwgcElFICAgICAgPSByZXF1aXJlKCcuL19vYmplY3QtcGllJylcbiAgLCB0b09iamVjdCA9IHJlcXVpcmUoJy4vX3RvLW9iamVjdCcpXG4gICwgSU9iamVjdCAgPSByZXF1aXJlKCcuL19pb2JqZWN0JylcbiAgLCAkYXNzaWduICA9IE9iamVjdC5hc3NpZ247XG5cbi8vIHNob3VsZCB3b3JrIHdpdGggc3ltYm9scyBhbmQgc2hvdWxkIGhhdmUgZGV0ZXJtaW5pc3RpYyBwcm9wZXJ0eSBvcmRlciAoVjggYnVnKVxubW9kdWxlLmV4cG9ydHMgPSAhJGFzc2lnbiB8fCByZXF1aXJlKCcuL19mYWlscycpKGZ1bmN0aW9uKCl7XG4gIHZhciBBID0ge31cbiAgICAsIEIgPSB7fVxuICAgICwgUyA9IFN5bWJvbCgpXG4gICAgLCBLID0gJ2FiY2RlZmdoaWprbG1ub3BxcnN0JztcbiAgQVtTXSA9IDc7XG4gIEsuc3BsaXQoJycpLmZvckVhY2goZnVuY3Rpb24oayl7IEJba10gPSBrOyB9KTtcbiAgcmV0dXJuICRhc3NpZ24oe30sIEEpW1NdICE9IDcgfHwgT2JqZWN0LmtleXMoJGFzc2lnbih7fSwgQikpLmpvaW4oJycpICE9IEs7XG59KSA/IGZ1bmN0aW9uIGFzc2lnbih0YXJnZXQsIHNvdXJjZSl7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW51c2VkLXZhcnNcbiAgdmFyIFQgICAgID0gdG9PYmplY3QodGFyZ2V0KVxuICAgICwgYUxlbiAgPSBhcmd1bWVudHMubGVuZ3RoXG4gICAgLCBpbmRleCA9IDFcbiAgICAsIGdldFN5bWJvbHMgPSBnT1BTLmZcbiAgICAsIGlzRW51bSAgICAgPSBwSUUuZjtcbiAgd2hpbGUoYUxlbiA+IGluZGV4KXtcbiAgICB2YXIgUyAgICAgID0gSU9iamVjdChhcmd1bWVudHNbaW5kZXgrK10pXG4gICAgICAsIGtleXMgICA9IGdldFN5bWJvbHMgPyBnZXRLZXlzKFMpLmNvbmNhdChnZXRTeW1ib2xzKFMpKSA6IGdldEtleXMoUylcbiAgICAgICwgbGVuZ3RoID0ga2V5cy5sZW5ndGhcbiAgICAgICwgaiAgICAgID0gMFxuICAgICAgLCBrZXk7XG4gICAgd2hpbGUobGVuZ3RoID4gailpZihpc0VudW0uY2FsbChTLCBrZXkgPSBrZXlzW2orK10pKVRba2V5XSA9IFNba2V5XTtcbiAgfSByZXR1cm4gVDtcbn0gOiAkYXNzaWduOyIsInZhciBhbk9iamVjdCAgICAgICA9IHJlcXVpcmUoJy4vX2FuLW9iamVjdCcpXG4gICwgSUU4X0RPTV9ERUZJTkUgPSByZXF1aXJlKCcuL19pZTgtZG9tLWRlZmluZScpXG4gICwgdG9QcmltaXRpdmUgICAgPSByZXF1aXJlKCcuL190by1wcmltaXRpdmUnKVxuICAsIGRQICAgICAgICAgICAgID0gT2JqZWN0LmRlZmluZVByb3BlcnR5O1xuXG5leHBvcnRzLmYgPSByZXF1aXJlKCcuL19kZXNjcmlwdG9ycycpID8gT2JqZWN0LmRlZmluZVByb3BlcnR5IDogZnVuY3Rpb24gZGVmaW5lUHJvcGVydHkoTywgUCwgQXR0cmlidXRlcyl7XG4gIGFuT2JqZWN0KE8pO1xuICBQID0gdG9QcmltaXRpdmUoUCwgdHJ1ZSk7XG4gIGFuT2JqZWN0KEF0dHJpYnV0ZXMpO1xuICBpZihJRThfRE9NX0RFRklORSl0cnkge1xuICAgIHJldHVybiBkUChPLCBQLCBBdHRyaWJ1dGVzKTtcbiAgfSBjYXRjaChlKXsgLyogZW1wdHkgKi8gfVxuICBpZignZ2V0JyBpbiBBdHRyaWJ1dGVzIHx8ICdzZXQnIGluIEF0dHJpYnV0ZXMpdGhyb3cgVHlwZUVycm9yKCdBY2Nlc3NvcnMgbm90IHN1cHBvcnRlZCEnKTtcbiAgaWYoJ3ZhbHVlJyBpbiBBdHRyaWJ1dGVzKU9bUF0gPSBBdHRyaWJ1dGVzLnZhbHVlO1xuICByZXR1cm4gTztcbn07IiwiZXhwb3J0cy5mID0gT2JqZWN0LmdldE93blByb3BlcnR5U3ltYm9sczsiLCJ2YXIgaGFzICAgICAgICAgID0gcmVxdWlyZSgnLi9faGFzJylcbiAgLCB0b0lPYmplY3QgICAgPSByZXF1aXJlKCcuL190by1pb2JqZWN0JylcbiAgLCBhcnJheUluZGV4T2YgPSByZXF1aXJlKCcuL19hcnJheS1pbmNsdWRlcycpKGZhbHNlKVxuICAsIElFX1BST1RPICAgICA9IHJlcXVpcmUoJy4vX3NoYXJlZC1rZXknKSgnSUVfUFJPVE8nKTtcblxubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihvYmplY3QsIG5hbWVzKXtcbiAgdmFyIE8gICAgICA9IHRvSU9iamVjdChvYmplY3QpXG4gICAgLCBpICAgICAgPSAwXG4gICAgLCByZXN1bHQgPSBbXVxuICAgICwga2V5O1xuICBmb3Ioa2V5IGluIE8paWYoa2V5ICE9IElFX1BST1RPKWhhcyhPLCBrZXkpICYmIHJlc3VsdC5wdXNoKGtleSk7XG4gIC8vIERvbid0IGVudW0gYnVnICYgaGlkZGVuIGtleXNcbiAgd2hpbGUobmFtZXMubGVuZ3RoID4gaSlpZihoYXMoTywga2V5ID0gbmFtZXNbaSsrXSkpe1xuICAgIH5hcnJheUluZGV4T2YocmVzdWx0LCBrZXkpIHx8IHJlc3VsdC5wdXNoKGtleSk7XG4gIH1cbiAgcmV0dXJuIHJlc3VsdDtcbn07IiwiLy8gMTkuMS4yLjE0IC8gMTUuMi4zLjE0IE9iamVjdC5rZXlzKE8pXG52YXIgJGtleXMgICAgICAgPSByZXF1aXJlKCcuL19vYmplY3Qta2V5cy1pbnRlcm5hbCcpXG4gICwgZW51bUJ1Z0tleXMgPSByZXF1aXJlKCcuL19lbnVtLWJ1Zy1rZXlzJyk7XG5cbm1vZHVsZS5leHBvcnRzID0gT2JqZWN0LmtleXMgfHwgZnVuY3Rpb24ga2V5cyhPKXtcbiAgcmV0dXJuICRrZXlzKE8sIGVudW1CdWdLZXlzKTtcbn07IiwiZXhwb3J0cy5mID0ge30ucHJvcGVydHlJc0VudW1lcmFibGU7IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihiaXRtYXAsIHZhbHVlKXtcbiAgcmV0dXJuIHtcbiAgICBlbnVtZXJhYmxlICA6ICEoYml0bWFwICYgMSksXG4gICAgY29uZmlndXJhYmxlOiAhKGJpdG1hcCAmIDIpLFxuICAgIHdyaXRhYmxlICAgIDogIShiaXRtYXAgJiA0KSxcbiAgICB2YWx1ZSAgICAgICA6IHZhbHVlXG4gIH07XG59OyIsInZhciBzaGFyZWQgPSByZXF1aXJlKCcuL19zaGFyZWQnKSgna2V5cycpXG4gICwgdWlkICAgID0gcmVxdWlyZSgnLi9fdWlkJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGtleSl7XG4gIHJldHVybiBzaGFyZWRba2V5XSB8fCAoc2hhcmVkW2tleV0gPSB1aWQoa2V5KSk7XG59OyIsInZhciBnbG9iYWwgPSByZXF1aXJlKCcuL19nbG9iYWwnKVxuICAsIFNIQVJFRCA9ICdfX2NvcmUtanNfc2hhcmVkX18nXG4gICwgc3RvcmUgID0gZ2xvYmFsW1NIQVJFRF0gfHwgKGdsb2JhbFtTSEFSRURdID0ge30pO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihrZXkpe1xuICByZXR1cm4gc3RvcmVba2V5XSB8fCAoc3RvcmVba2V5XSA9IHt9KTtcbn07IiwidmFyIHRvSW50ZWdlciA9IHJlcXVpcmUoJy4vX3RvLWludGVnZXInKVxuICAsIG1heCAgICAgICA9IE1hdGgubWF4XG4gICwgbWluICAgICAgID0gTWF0aC5taW47XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGluZGV4LCBsZW5ndGgpe1xuICBpbmRleCA9IHRvSW50ZWdlcihpbmRleCk7XG4gIHJldHVybiBpbmRleCA8IDAgPyBtYXgoaW5kZXggKyBsZW5ndGgsIDApIDogbWluKGluZGV4LCBsZW5ndGgpO1xufTsiLCIvLyA3LjEuNCBUb0ludGVnZXJcbnZhciBjZWlsICA9IE1hdGguY2VpbFxuICAsIGZsb29yID0gTWF0aC5mbG9vcjtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gaXNOYU4oaXQgPSAraXQpID8gMCA6IChpdCA+IDAgPyBmbG9vciA6IGNlaWwpKGl0KTtcbn07IiwiLy8gdG8gaW5kZXhlZCBvYmplY3QsIHRvT2JqZWN0IHdpdGggZmFsbGJhY2sgZm9yIG5vbi1hcnJheS1saWtlIEVTMyBzdHJpbmdzXG52YXIgSU9iamVjdCA9IHJlcXVpcmUoJy4vX2lvYmplY3QnKVxuICAsIGRlZmluZWQgPSByZXF1aXJlKCcuL19kZWZpbmVkJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIElPYmplY3QoZGVmaW5lZChpdCkpO1xufTsiLCIvLyA3LjEuMTUgVG9MZW5ndGhcbnZhciB0b0ludGVnZXIgPSByZXF1aXJlKCcuL190by1pbnRlZ2VyJylcbiAgLCBtaW4gICAgICAgPSBNYXRoLm1pbjtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gaXQgPiAwID8gbWluKHRvSW50ZWdlcihpdCksIDB4MWZmZmZmZmZmZmZmZmYpIDogMDsgLy8gcG93KDIsIDUzKSAtIDEgPT0gOTAwNzE5OTI1NDc0MDk5MVxufTsiLCIvLyA3LjEuMTMgVG9PYmplY3QoYXJndW1lbnQpXG52YXIgZGVmaW5lZCA9IHJlcXVpcmUoJy4vX2RlZmluZWQnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gT2JqZWN0KGRlZmluZWQoaXQpKTtcbn07IiwiLy8gNy4xLjEgVG9QcmltaXRpdmUoaW5wdXQgWywgUHJlZmVycmVkVHlwZV0pXG52YXIgaXNPYmplY3QgPSByZXF1aXJlKCcuL19pcy1vYmplY3QnKTtcbi8vIGluc3RlYWQgb2YgdGhlIEVTNiBzcGVjIHZlcnNpb24sIHdlIGRpZG4ndCBpbXBsZW1lbnQgQEB0b1ByaW1pdGl2ZSBjYXNlXG4vLyBhbmQgdGhlIHNlY29uZCBhcmd1bWVudCAtIGZsYWcgLSBwcmVmZXJyZWQgdHlwZSBpcyBhIHN0cmluZ1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCwgUyl7XG4gIGlmKCFpc09iamVjdChpdCkpcmV0dXJuIGl0O1xuICB2YXIgZm4sIHZhbDtcbiAgaWYoUyAmJiB0eXBlb2YgKGZuID0gaXQudG9TdHJpbmcpID09ICdmdW5jdGlvbicgJiYgIWlzT2JqZWN0KHZhbCA9IGZuLmNhbGwoaXQpKSlyZXR1cm4gdmFsO1xuICBpZih0eXBlb2YgKGZuID0gaXQudmFsdWVPZikgPT0gJ2Z1bmN0aW9uJyAmJiAhaXNPYmplY3QodmFsID0gZm4uY2FsbChpdCkpKXJldHVybiB2YWw7XG4gIGlmKCFTICYmIHR5cGVvZiAoZm4gPSBpdC50b1N0cmluZykgPT0gJ2Z1bmN0aW9uJyAmJiAhaXNPYmplY3QodmFsID0gZm4uY2FsbChpdCkpKXJldHVybiB2YWw7XG4gIHRocm93IFR5cGVFcnJvcihcIkNhbid0IGNvbnZlcnQgb2JqZWN0IHRvIHByaW1pdGl2ZSB2YWx1ZVwiKTtcbn07IiwidmFyIGlkID0gMFxuICAsIHB4ID0gTWF0aC5yYW5kb20oKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oa2V5KXtcbiAgcmV0dXJuICdTeW1ib2woJy5jb25jYXQoa2V5ID09PSB1bmRlZmluZWQgPyAnJyA6IGtleSwgJylfJywgKCsraWQgKyBweCkudG9TdHJpbmcoMzYpKTtcbn07IiwiLy8gMTkuMS4zLjEgT2JqZWN0LmFzc2lnbih0YXJnZXQsIHNvdXJjZSlcbnZhciAkZXhwb3J0ID0gcmVxdWlyZSgnLi9fZXhwb3J0Jyk7XG5cbiRleHBvcnQoJGV4cG9ydC5TICsgJGV4cG9ydC5GLCAnT2JqZWN0Jywge2Fzc2lnbjogcmVxdWlyZSgnLi9fb2JqZWN0LWFzc2lnbicpfSk7IiwidmFyIF9leHRlbmRzID0gT2JqZWN0LmFzc2lnbiB8fCBmdW5jdGlvbiAodGFyZ2V0KSB7IGZvciAodmFyIGkgPSAxOyBpIDwgYXJndW1lbnRzLmxlbmd0aDsgaSsrKSB7IHZhciBzb3VyY2UgPSBhcmd1bWVudHNbaV07IGZvciAodmFyIGtleSBpbiBzb3VyY2UpIHsgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChzb3VyY2UsIGtleSkpIHsgdGFyZ2V0W2tleV0gPSBzb3VyY2Vba2V5XTsgfSB9IH0gcmV0dXJuIHRhcmdldDsgfTtcblxudmFyIF90eXBlb2YgPSB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgdHlwZW9mIFN5bWJvbC5pdGVyYXRvciA9PT0gXCJzeW1ib2xcIiA/IGZ1bmN0aW9uIChvYmopIHsgcmV0dXJuIHR5cGVvZiBvYmo7IH0gOiBmdW5jdGlvbiAob2JqKSB7IHJldHVybiBvYmogJiYgdHlwZW9mIFN5bWJvbCA9PT0gXCJmdW5jdGlvblwiICYmIG9iai5jb25zdHJ1Y3RvciA9PT0gU3ltYm9sICYmIG9iaiAhPT0gU3ltYm9sLnByb3RvdHlwZSA/IFwic3ltYm9sXCIgOiB0eXBlb2Ygb2JqOyB9O1xuXG4vKiEgZmxhdHBpY2tyIHYyLjQuNywgQGxpY2Vuc2UgTUlUICovXG5mdW5jdGlvbiBGbGF0cGlja3IoZWxlbWVudCwgY29uZmlnKSB7XG5cdHZhciBzZWxmID0gdGhpcztcblxuXHRzZWxmLmNoYW5nZU1vbnRoID0gY2hhbmdlTW9udGg7XG5cdHNlbGYuY2hhbmdlWWVhciA9IGNoYW5nZVllYXI7XG5cdHNlbGYuY2xlYXIgPSBjbGVhcjtcblx0c2VsZi5jbG9zZSA9IGNsb3NlO1xuXHRzZWxmLl9jcmVhdGVFbGVtZW50ID0gY3JlYXRlRWxlbWVudDtcblx0c2VsZi5kZXN0cm95ID0gZGVzdHJveTtcblx0c2VsZi5mb3JtYXREYXRlID0gZm9ybWF0RGF0ZTtcblx0c2VsZi5pc0VuYWJsZWQgPSBpc0VuYWJsZWQ7XG5cdHNlbGYuanVtcFRvRGF0ZSA9IGp1bXBUb0RhdGU7XG5cdHNlbGYub3BlbiA9IG9wZW47XG5cdHNlbGYucmVkcmF3ID0gcmVkcmF3O1xuXHRzZWxmLnNldCA9IHNldDtcblx0c2VsZi5zZXREYXRlID0gc2V0RGF0ZTtcblx0c2VsZi50b2dnbGUgPSB0b2dnbGU7XG5cblx0ZnVuY3Rpb24gaW5pdCgpIHtcblx0XHRpZiAoZWxlbWVudC5fZmxhdHBpY2tyKSBkZXN0cm95KGVsZW1lbnQuX2ZsYXRwaWNrcik7XG5cblx0XHRlbGVtZW50Ll9mbGF0cGlja3IgPSBzZWxmO1xuXG5cdFx0c2VsZi5lbGVtZW50ID0gZWxlbWVudDtcblx0XHRzZWxmLmluc3RhbmNlQ29uZmlnID0gY29uZmlnIHx8IHt9O1xuXHRcdHNlbGYucGFyc2VEYXRlID0gRmxhdHBpY2tyLnByb3RvdHlwZS5wYXJzZURhdGUuYmluZChzZWxmKTtcblxuXHRcdHNldHVwRm9ybWF0cygpO1xuXHRcdHBhcnNlQ29uZmlnKCk7XG5cdFx0c2V0dXBMb2NhbGUoKTtcblx0XHRzZXR1cElucHV0cygpO1xuXHRcdHNldHVwRGF0ZXMoKTtcblx0XHRzZXR1cEhlbHBlckZ1bmN0aW9ucygpO1xuXG5cdFx0c2VsZi5pc09wZW4gPSBzZWxmLmNvbmZpZy5pbmxpbmU7XG5cblx0XHRzZWxmLmlzTW9iaWxlID0gIXNlbGYuY29uZmlnLmRpc2FibGVNb2JpbGUgJiYgIXNlbGYuY29uZmlnLmlubGluZSAmJiBzZWxmLmNvbmZpZy5tb2RlID09PSBcInNpbmdsZVwiICYmICFzZWxmLmNvbmZpZy5kaXNhYmxlLmxlbmd0aCAmJiAhc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCAmJiAhc2VsZi5jb25maWcud2Vla051bWJlcnMgJiYgL0FuZHJvaWR8d2ViT1N8aVBob25lfGlQYWR8aVBvZHxCbGFja0JlcnJ5fElFTW9iaWxlfE9wZXJhIE1pbmkvaS50ZXN0KG5hdmlnYXRvci51c2VyQWdlbnQpO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSBidWlsZCgpO1xuXG5cdFx0YmluZCgpO1xuXG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggfHwgc2VsZi5jb25maWcubm9DYWxlbmRhcikge1xuXHRcdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHNldEhvdXJzRnJvbURhdGUoc2VsZi5jb25maWcubm9DYWxlbmRhciA/IHNlbGYuY29uZmlnLm1pbkRhdGUgOiBudWxsKTtcblx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzKSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLndpZHRoID0gc2VsZi5kYXlzLmNsaWVudFdpZHRoICsgc2VsZi53ZWVrV3JhcHBlci5jbGllbnRXaWR0aCArIFwicHhcIjtcblx0XHR9XG5cblx0XHRzZWxmLnNob3dUaW1lSW5wdXQgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID4gMCB8fCBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdFx0dHJpZ2dlckV2ZW50KFwiUmVhZHlcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBiaW5kVG9JbnN0YW5jZShmbikge1xuXHRcdGlmIChmbiAmJiBmbi5iaW5kKSByZXR1cm4gZm4uYmluZChzZWxmKTtcblx0XHRyZXR1cm4gZm47XG5cdH1cblxuXHRmdW5jdGlvbiB1cGRhdGVUaW1lKGUpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhciAmJiAhc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aClcblx0XHRcdC8vIHBpY2tpbmcgdGltZSBvbmx5XG5cdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZi5ub3ddO1xuXG5cdFx0dGltZVdyYXBwZXIoZSk7XG5cblx0XHRpZiAoIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHJldHVybjtcblxuXHRcdGlmICghc2VsZi5taW5EYXRlSGFzVGltZSB8fCBlLnR5cGUgIT09IFwiaW5wdXRcIiB8fCBlLnRhcmdldC52YWx1ZS5sZW5ndGggPj0gMikge1xuXHRcdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdH0gZWxzZSB7XG5cdFx0XHRzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cdFx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0XHR9LCAxMDAwKTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBzZXRIb3Vyc0Zyb21JbnB1dHMoKSB7XG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSByZXR1cm47XG5cblx0XHR2YXIgaG91cnMgPSBwYXJzZUludChzZWxmLmhvdXJFbGVtZW50LnZhbHVlLCAxMCkgfHwgMCxcblx0XHQgICAgbWludXRlcyA9IHBhcnNlSW50KHNlbGYubWludXRlRWxlbWVudC52YWx1ZSwgMTApIHx8IDAsXG5cdFx0ICAgIHNlY29uZHMgPSBzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gcGFyc2VJbnQoc2VsZi5zZWNvbmRFbGVtZW50LnZhbHVlLCAxMCkgfHwgMCA6IDA7XG5cblx0XHRpZiAoc2VsZi5hbVBNKSBob3VycyA9IGhvdXJzICUgMTIgKyAxMiAqIChzZWxmLmFtUE0udGV4dENvbnRlbnQgPT09IFwiUE1cIik7XG5cblx0XHRpZiAoc2VsZi5taW5EYXRlSGFzVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmosIHNlbGYuY29uZmlnLm1pbkRhdGUpID09PSAwKSB7XG5cblx0XHRcdGhvdXJzID0gTWF0aC5tYXgoaG91cnMsIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSk7XG5cdFx0XHRpZiAoaG91cnMgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSkgbWludXRlcyA9IE1hdGgubWF4KG1pbnV0ZXMsIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TWludXRlcygpKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5tYXhEYXRlSGFzVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmosIHNlbGYuY29uZmlnLm1heERhdGUpID09PSAwKSB7XG5cdFx0XHRob3VycyA9IE1hdGgubWluKGhvdXJzLCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkpO1xuXHRcdFx0aWYgKGhvdXJzID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkpIG1pbnV0ZXMgPSBNYXRoLm1pbihtaW51dGVzLCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1pbnV0ZXMoKSk7XG5cdFx0fVxuXG5cdFx0c2V0SG91cnMoaG91cnMsIG1pbnV0ZXMsIHNlY29uZHMpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0SG91cnNGcm9tRGF0ZShkYXRlT2JqKSB7XG5cdFx0dmFyIGRhdGUgPSBkYXRlT2JqIHx8IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqO1xuXG5cdFx0aWYgKGRhdGUpIHNldEhvdXJzKGRhdGUuZ2V0SG91cnMoKSwgZGF0ZS5nZXRNaW51dGVzKCksIGRhdGUuZ2V0U2Vjb25kcygpKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldEhvdXJzKGhvdXJzLCBtaW51dGVzLCBzZWNvbmRzKSB7XG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHtcblx0XHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLnNldEhvdXJzKGhvdXJzICUgMjQsIG1pbnV0ZXMsIHNlY29uZHMgfHwgMCwgMCk7XG5cdFx0fVxuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGVUaW1lIHx8IHNlbGYuaXNNb2JpbGUpIHJldHVybjtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZCghc2VsZi5jb25maWcudGltZV8yNGhyID8gKDEyICsgaG91cnMpICUgMTIgKyAxMiAqIChob3VycyAlIDEyID09PSAwKSA6IGhvdXJzKTtcblxuXHRcdHNlbGYubWludXRlRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKG1pbnV0ZXMpO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy50aW1lXzI0aHIpIHNlbGYuYW1QTS50ZXh0Q29udGVudCA9IGhvdXJzID49IDEyID8gXCJQTVwiIDogXCJBTVwiO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMpIHNlbGYuc2Vjb25kRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKHNlY29uZHMpO1xuXHR9XG5cblx0ZnVuY3Rpb24gb25ZZWFySW5wdXQoZXZlbnQpIHtcblx0XHR2YXIgeWVhciA9IGV2ZW50LnRhcmdldC52YWx1ZTtcblx0XHRpZiAoZXZlbnQuZGVsdGEpIHllYXIgPSAocGFyc2VJbnQoeWVhcikgKyBldmVudC5kZWx0YSkudG9TdHJpbmcoKTtcblxuXHRcdGlmICh5ZWFyLmxlbmd0aCA9PT0gNCkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYmx1cigpO1xuXHRcdFx0aWYgKCEvW15cXGRdLy50ZXN0KHllYXIpKSBjaGFuZ2VZZWFyKHllYXIpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIG9uTW9udGhTY3JvbGwoZSkge1xuXHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRzZWxmLmNoYW5nZU1vbnRoKE1hdGgubWF4KC0xLCBNYXRoLm1pbigxLCBlLndoZWVsRGVsdGEgfHwgLWUuZGVsdGFZKSkpO1xuXHR9XG5cblx0ZnVuY3Rpb24gYmluZCgpIHtcblx0XHRpZiAoc2VsZi5jb25maWcud3JhcCkge1xuXHRcdFx0W1wib3BlblwiLCBcImNsb3NlXCIsIFwidG9nZ2xlXCIsIFwiY2xlYXJcIl0uZm9yRWFjaChmdW5jdGlvbiAoZWwpIHtcblx0XHRcdFx0dmFyIHRvZ2dsZXMgPSBzZWxmLmVsZW1lbnQucXVlcnlTZWxlY3RvckFsbChcIltkYXRhLVwiICsgZWwgKyBcIl1cIik7XG5cdFx0XHRcdGZvciAodmFyIGkgPSAwOyBpIDwgdG9nZ2xlcy5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRcdHRvZ2dsZXNbaV0uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIHNlbGZbZWxdKTtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdFx0aWYgKHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFdmVudCAhPT0gdW5kZWZpbmVkKSB7XG5cdFx0XHRzZWxmLmNoYW5nZUV2ZW50ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50KFwiSFRNTEV2ZW50c1wiKTtcblx0XHRcdHNlbGYuY2hhbmdlRXZlbnQuaW5pdEV2ZW50KFwiY2hhbmdlXCIsIGZhbHNlLCB0cnVlKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5pc01vYmlsZSkgcmV0dXJuIHNldHVwTW9iaWxlKCk7XG5cblx0XHRzZWxmLmRlYm91bmNlZFJlc2l6ZSA9IGRlYm91bmNlKG9uUmVzaXplLCA1MCk7XG5cdFx0c2VsZi50cmlnZ2VyQ2hhbmdlID0gZnVuY3Rpb24gKCkge1xuXHRcdFx0dHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHRcdH07XG5cdFx0c2VsZi5kZWJvdW5jZWRDaGFuZ2UgPSBkZWJvdW5jZShzZWxmLnRyaWdnZXJDaGFuZ2UsIDMwMCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiICYmIHNlbGYuZGF5cykgc2VsZi5kYXlzLmFkZEV2ZW50TGlzdGVuZXIoXCJtb3VzZW92ZXJcIiwgb25Nb3VzZU92ZXIpO1xuXG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwia2V5ZG93blwiLCBvbktleURvd24pO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5zdGF0aWMpIChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmFkZEV2ZW50TGlzdGVuZXIoXCJrZXlkb3duXCIsIG9uS2V5RG93bik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmlubGluZSAmJiAhc2VsZi5jb25maWcuc3RhdGljKSB3aW5kb3cuYWRkRXZlbnRMaXN0ZW5lcihcInJlc2l6ZVwiLCBzZWxmLmRlYm91bmNlZFJlc2l6ZSk7XG5cblx0XHRpZiAod2luZG93Lm9udG91Y2hzdGFydCkgd2luZG93LmRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJ0b3VjaHN0YXJ0XCIsIGRvY3VtZW50Q2xpY2spO1xuXG5cdFx0d2luZG93LmRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBkb2N1bWVudENsaWNrKTtcblx0XHQoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5hZGRFdmVudExpc3RlbmVyKFwiYmx1clwiLCBkb2N1bWVudENsaWNrKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5jbGlja09wZW5zKSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgb3Blbik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHtcblx0XHRcdHNlbGYucHJldk1vbnRoTmF2LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHJldHVybiBjaGFuZ2VNb250aCgtMSk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYubmV4dE1vbnRoTmF2LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHJldHVybiBjaGFuZ2VNb250aCgxKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIndoZWVsXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRcdHJldHVybiBkZWJvdW5jZShvbk1vbnRoU2Nyb2xsKGUpLCA1MCk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJ3aGVlbFwiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0XHRyZXR1cm4gZGVib3VuY2UoeWVhclNjcm9sbChlKSwgNTApO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5zZWxlY3QoKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiaW5wdXRcIiwgb25ZZWFySW5wdXQpO1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImluY3JlbWVudFwiLCBvblllYXJJbnB1dCk7XG5cblx0XHRcdHNlbGYuZGF5cy5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgc2VsZWN0RGF0ZSk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwidHJhbnNpdGlvbmVuZFwiLCBwb3NpdGlvbkNhbGVuZGFyKTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwid2hlZWxcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdFx0cmV0dXJuIGRlYm91bmNlKHVwZGF0ZVRpbWUoZSksIDUpO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImlucHV0XCIsIHVwZGF0ZVRpbWUpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJpbmNyZW1lbnRcIiwgdXBkYXRlVGltZSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImluY3JlbWVudFwiLCBzZWxmLmRlYm91bmNlZENoYW5nZSk7XG5cblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwid2hlZWxcIiwgc2VsZi5kZWJvdW5jZWRDaGFuZ2UpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJpbnB1dFwiLCBzZWxmLnRyaWdnZXJDaGFuZ2UpO1xuXG5cdFx0XHRzZWxmLmhvdXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNlbGYuaG91ckVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYubWludXRlRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHR9KTtcblxuXHRcdFx0aWYgKHNlbGYuc2Vjb25kRWxlbWVudCkge1xuXHRcdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5hbVBNKSB7XG5cdFx0XHRcdHNlbGYuYW1QTS5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdFx0XHR1cGRhdGVUaW1lKGUpO1xuXHRcdFx0XHRcdHNlbGYudHJpZ2dlckNoYW5nZShlKTtcblx0XHRcdFx0fSk7XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24ganVtcFRvRGF0ZShqdW1wRGF0ZSkge1xuXHRcdGp1bXBEYXRlID0ganVtcERhdGUgPyBzZWxmLnBhcnNlRGF0ZShqdW1wRGF0ZSkgOiBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiB8fCAoc2VsZi5jb25maWcubWluRGF0ZSA+IHNlbGYubm93ID8gc2VsZi5jb25maWcubWluRGF0ZSA6IHNlbGYuY29uZmlnLm1heERhdGUgJiYgc2VsZi5jb25maWcubWF4RGF0ZSA8IHNlbGYubm93ID8gc2VsZi5jb25maWcubWF4RGF0ZSA6IHNlbGYubm93KTtcblxuXHRcdHRyeSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyID0ganVtcERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0ganVtcERhdGUuZ2V0TW9udGgoKTtcblx0XHR9IGNhdGNoIChlKSB7XG5cdFx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdFx0Y29uc29sZS5lcnJvcihlLnN0YWNrKTtcblx0XHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0XHRjb25zb2xlLndhcm4oXCJJbnZhbGlkIGRhdGUgc3VwcGxpZWQ6IFwiICsganVtcERhdGUpO1xuXHRcdH1cblxuXHRcdHNlbGYucmVkcmF3KCk7XG5cdH1cblxuXHRmdW5jdGlvbiBpbmNyZW1lbnROdW1JbnB1dChlLCBkZWx0YSwgaW5wdXRFbGVtKSB7XG5cdFx0dmFyIGlucHV0ID0gaW5wdXRFbGVtIHx8IGUudGFyZ2V0LnBhcmVudE5vZGUuY2hpbGROb2Rlc1swXTtcblx0XHR2YXIgZXYgPSB2b2lkIDA7XG5cblx0XHR0cnkge1xuXHRcdFx0ZXYgPSBuZXcgRXZlbnQoXCJpbmNyZW1lbnRcIiwgeyBcImJ1YmJsZXNcIjogdHJ1ZSB9KTtcblx0XHR9IGNhdGNoIChlcnIpIHtcblx0XHRcdGV2ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50KFwiQ3VzdG9tRXZlbnRcIik7XG5cdFx0XHRldi5pbml0Q3VzdG9tRXZlbnQoXCJpbmNyZW1lbnRcIiwgdHJ1ZSwgdHJ1ZSwge30pO1xuXHRcdH1cblxuXHRcdGV2LmRlbHRhID0gZGVsdGE7XG5cdFx0aW5wdXQuZGlzcGF0Y2hFdmVudChldik7XG5cdH1cblxuXHRmdW5jdGlvbiBjcmVhdGVOdW1iZXJJbnB1dChpbnB1dENsYXNzTmFtZSkge1xuXHRcdHZhciB3cmFwcGVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcIm51bUlucHV0V3JhcHBlclwiKSxcblx0XHQgICAgbnVtSW5wdXQgPSBjcmVhdGVFbGVtZW50KFwiaW5wdXRcIiwgXCJudW1JbnB1dCBcIiArIGlucHV0Q2xhc3NOYW1lKSxcblx0XHQgICAgYXJyb3dVcCA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiYXJyb3dVcFwiKSxcblx0XHQgICAgYXJyb3dEb3duID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJhcnJvd0Rvd25cIik7XG5cblx0XHRudW1JbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0bnVtSW5wdXQucGF0dGVybiA9IFwiXFxcXGQqXCI7XG5cdFx0d3JhcHBlci5hcHBlbmRDaGlsZChudW1JbnB1dCk7XG5cdFx0d3JhcHBlci5hcHBlbmRDaGlsZChhcnJvd1VwKTtcblx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKGFycm93RG93bik7XG5cblx0XHRhcnJvd1VwLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0cmV0dXJuIGluY3JlbWVudE51bUlucHV0KGUsIDEpO1xuXHRcdH0pO1xuXHRcdGFycm93RG93bi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHJldHVybiBpbmNyZW1lbnROdW1JbnB1dChlLCAtMSk7XG5cdFx0fSk7XG5cdFx0cmV0dXJuIHdyYXBwZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZCgpIHtcblx0XHR2YXIgZnJhZ21lbnQgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRG9jdW1lbnRGcmFnbWVudCgpO1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLWNhbGVuZGFyXCIpO1xuXHRcdHNlbGYubnVtSW5wdXRUeXBlID0gbmF2aWdhdG9yLnVzZXJBZ2VudC5pbmRleE9mKFwiTVNJRSA5LjBcIikgPiAwID8gXCJ0ZXh0XCIgOiBcIm51bWJlclwiO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSB7XG5cdFx0XHRmcmFnbWVudC5hcHBlbmRDaGlsZChidWlsZE1vbnRoTmF2KCkpO1xuXHRcdFx0c2VsZi5pbm5lckNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItaW5uZXJDb250YWluZXJcIik7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycykgc2VsZi5pbm5lckNvbnRhaW5lci5hcHBlbmRDaGlsZChidWlsZFdlZWtzKCkpO1xuXG5cdFx0XHRzZWxmLnJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXJDb250YWluZXJcIik7XG5cdFx0XHRzZWxmLnJDb250YWluZXIuYXBwZW5kQ2hpbGQoYnVpbGRXZWVrZGF5cygpKTtcblxuXHRcdFx0aWYgKCFzZWxmLmRheXMpIHtcblx0XHRcdFx0c2VsZi5kYXlzID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1kYXlzXCIpO1xuXHRcdFx0XHRzZWxmLmRheXMudGFiSW5kZXggPSAtMTtcblx0XHRcdH1cblxuXHRcdFx0YnVpbGREYXlzKCk7XG5cdFx0XHRzZWxmLnJDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VsZi5kYXlzKTtcblxuXHRcdFx0c2VsZi5pbm5lckNvbnRhaW5lci5hcHBlbmRDaGlsZChzZWxmLnJDb250YWluZXIpO1xuXHRcdFx0ZnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5pbm5lckNvbnRhaW5lcik7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIGZyYWdtZW50LmFwcGVuZENoaWxkKGJ1aWxkVGltZSgpKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcInJhbmdlTW9kZVwiKTtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuYXBwZW5kQ2hpbGQoZnJhZ21lbnQpO1xuXG5cdFx0dmFyIGN1c3RvbUFwcGVuZCA9IHNlbGYuY29uZmlnLmFwcGVuZFRvICYmIHNlbGYuY29uZmlnLmFwcGVuZFRvLm5vZGVUeXBlO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmlubGluZSB8fCBzZWxmLmNvbmZpZy5zdGF0aWMpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChzZWxmLmNvbmZpZy5pbmxpbmUgPyBcImlubGluZVwiIDogXCJzdGF0aWNcIik7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUgJiYgIWN1c3RvbUFwcGVuZCkge1xuXHRcdFx0XHRyZXR1cm4gc2VsZi5lbGVtZW50LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLm5leHRTaWJsaW5nKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuY29uZmlnLnN0YXRpYykge1xuXHRcdFx0XHR2YXIgd3JhcHBlciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd3JhcHBlclwiKTtcblx0XHRcdFx0c2VsZi5lbGVtZW50LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHdyYXBwZXIsIHNlbGYuZWxlbWVudCk7XG5cdFx0XHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoc2VsZi5lbGVtZW50KTtcblxuXHRcdFx0XHRpZiAoc2VsZi5hbHRJbnB1dCkgd3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLmFsdElucHV0KTtcblxuXHRcdFx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKHNlbGYuY2FsZW5kYXJDb250YWluZXIpO1xuXHRcdFx0XHRyZXR1cm47XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0KGN1c3RvbUFwcGVuZCA/IHNlbGYuY29uZmlnLmFwcGVuZFRvIDogd2luZG93LmRvY3VtZW50LmJvZHkpLmFwcGVuZENoaWxkKHNlbGYuY2FsZW5kYXJDb250YWluZXIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY3JlYXRlRGF5KGNsYXNzTmFtZSwgZGF0ZSwgZGF5TnVtYmVyKSB7XG5cdFx0dmFyIGRhdGVJc0VuYWJsZWQgPSBpc0VuYWJsZWQoZGF0ZSwgdHJ1ZSksXG5cdFx0ICAgIGRheUVsZW1lbnQgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1kYXkgXCIgKyBjbGFzc05hbWUsIGRhdGUuZ2V0RGF0ZSgpKTtcblxuXHRcdGRheUVsZW1lbnQuZGF0ZU9iaiA9IGRhdGU7XG5cblx0XHR0b2dnbGVDbGFzcyhkYXlFbGVtZW50LCBcInRvZGF5XCIsIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLm5vdykgPT09IDApO1xuXG5cdFx0aWYgKGRhdGVJc0VuYWJsZWQpIHtcblxuXHRcdFx0aWYgKGlzRGF0ZVNlbGVjdGVkKGRhdGUpKSB7XG5cdFx0XHRcdGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcInNlbGVjdGVkXCIpO1xuXHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gPSBkYXlFbGVtZW50O1xuXHRcdFx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRcdFx0dG9nZ2xlQ2xhc3MoZGF5RWxlbWVudCwgXCJzdGFydFJhbmdlXCIsIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pID09PSAwKTtcblxuXHRcdFx0XHRcdHRvZ2dsZUNsYXNzKGRheUVsZW1lbnQsIFwiZW5kUmFuZ2VcIiwgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1sxXSkgPT09IDApO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fSBlbHNlIHtcblx0XHRcdGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcImRpc2FibGVkXCIpO1xuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlc1swXSAmJiBkYXRlID4gc2VsZi5taW5SYW5nZURhdGUgJiYgZGF0ZSA8IHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgc2VsZi5taW5SYW5nZURhdGUgPSBkYXRlO2Vsc2UgaWYgKHNlbGYuc2VsZWN0ZWREYXRlc1swXSAmJiBkYXRlIDwgc2VsZi5tYXhSYW5nZURhdGUgJiYgZGF0ZSA+IHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgc2VsZi5tYXhSYW5nZURhdGUgPSBkYXRlO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdGlmIChpc0RhdGVJblJhbmdlKGRhdGUpICYmICFpc0RhdGVTZWxlY3RlZChkYXRlKSkgZGF5RWxlbWVudC5jbGFzc0xpc3QuYWRkKFwiaW5SYW5nZVwiKTtcblxuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEgJiYgKGRhdGUgPCBzZWxmLm1pblJhbmdlRGF0ZSB8fCBkYXRlID4gc2VsZi5tYXhSYW5nZURhdGUpKSBkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJub3RBbGxvd2VkXCIpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycyAmJiBjbGFzc05hbWUgIT09IFwicHJldk1vbnRoRGF5XCIgJiYgZGF5TnVtYmVyICUgNyA9PT0gMSkge1xuXHRcdFx0c2VsZi53ZWVrTnVtYmVycy5pbnNlcnRBZGphY2VudEhUTUwoXCJiZWZvcmVlbmRcIiwgXCI8c3BhbiBjbGFzcz0nZGlzYWJsZWQgZmxhdHBpY2tyLWRheSc+XCIgKyBzZWxmLmNvbmZpZy5nZXRXZWVrKGRhdGUpICsgXCI8L3NwYW4+XCIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIkRheUNyZWF0ZVwiLCBkYXlFbGVtZW50KTtcblxuXHRcdHJldHVybiBkYXlFbGVtZW50O1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGREYXlzKHllYXIsIG1vbnRoKSB7XG5cdFx0dmFyIGZpcnN0T2ZNb250aCA9IChuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCwgMSkuZ2V0RGF5KCkgLSBzZWxmLmwxMG4uZmlyc3REYXlPZldlZWsgKyA3KSAlIDcsXG5cdFx0ICAgIGlzUmFuZ2VNb2RlID0gc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiO1xuXG5cdFx0c2VsZi5wcmV2TW9udGhEYXlzID0gc2VsZi51dGlscy5nZXREYXlzaW5Nb250aCgoc2VsZi5jdXJyZW50TW9udGggLSAxICsgMTIpICUgMTIpO1xuXG5cdFx0dmFyIGRheXNJbk1vbnRoID0gc2VsZi51dGlscy5nZXREYXlzaW5Nb250aCgpLFxuXHRcdCAgICBkYXlzID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZURvY3VtZW50RnJhZ21lbnQoKTtcblxuXHRcdHZhciBkYXlOdW1iZXIgPSBzZWxmLnByZXZNb250aERheXMgKyAxIC0gZmlyc3RPZk1vbnRoO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzICYmIHNlbGYud2Vla051bWJlcnMuZmlyc3RDaGlsZCkgc2VsZi53ZWVrTnVtYmVycy50ZXh0Q29udGVudCA9IFwiXCI7XG5cblx0XHRpZiAoaXNSYW5nZU1vZGUpIHtcblx0XHRcdC8vIGNvbnN0IGRhdGVMaW1pdHMgPSBzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoIHx8IHNlbGYuY29uZmlnLmRpc2FibGUubGVuZ3RoIHx8IHNlbGYuY29uZmlnLm1peERhdGUgfHwgc2VsZi5jb25maWcubWF4RGF0ZTtcblx0XHRcdHNlbGYubWluUmFuZ2VEYXRlID0gbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggLSAxLCBkYXlOdW1iZXIpO1xuXHRcdFx0c2VsZi5tYXhSYW5nZURhdGUgPSBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsICg0MiAtIGZpcnN0T2ZNb250aCkgJSBkYXlzSW5Nb250aCk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuZGF5cy5maXJzdENoaWxkKSBzZWxmLmRheXMudGV4dENvbnRlbnQgPSBcIlwiO1xuXG5cdFx0Ly8gcHJlcGVuZCBkYXlzIGZyb20gdGhlIGVuZGluZyBvZiBwcmV2aW91cyBtb250aFxuXHRcdGZvciAoOyBkYXlOdW1iZXIgPD0gc2VsZi5wcmV2TW9udGhEYXlzOyBkYXlOdW1iZXIrKykge1xuXHRcdFx0ZGF5cy5hcHBlbmRDaGlsZChjcmVhdGVEYXkoXCJwcmV2TW9udGhEYXlcIiwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggLSAxLCBkYXlOdW1iZXIpLCBkYXlOdW1iZXIpKTtcblx0XHR9XG5cblx0XHQvLyBTdGFydCBhdCAxIHNpbmNlIHRoZXJlIGlzIG5vIDB0aCBkYXlcblx0XHRmb3IgKGRheU51bWJlciA9IDE7IGRheU51bWJlciA8PSBkYXlzSW5Nb250aDsgZGF5TnVtYmVyKyspIHtcblx0XHRcdGRheXMuYXBwZW5kQ2hpbGQoY3JlYXRlRGF5KFwiXCIsIG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoLCBkYXlOdW1iZXIpLCBkYXlOdW1iZXIpKTtcblx0XHR9XG5cblx0XHQvLyBhcHBlbmQgZGF5cyBmcm9tIHRoZSBuZXh0IG1vbnRoXG5cdFx0Zm9yICh2YXIgZGF5TnVtID0gZGF5c0luTW9udGggKyAxOyBkYXlOdW0gPD0gNDIgLSBmaXJzdE9mTW9udGg7IGRheU51bSsrKSB7XG5cdFx0XHRkYXlzLmFwcGVuZENoaWxkKGNyZWF0ZURheShcIm5leHRNb250aERheVwiLCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsIGRheU51bSAlIGRheXNJbk1vbnRoKSwgZGF5TnVtKSk7XG5cdFx0fVxuXG5cdFx0aWYgKGlzUmFuZ2VNb2RlICYmIHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEgJiYgZGF5cy5jaGlsZE5vZGVzWzBdKSB7XG5cdFx0XHRzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgfHwgc2VsZi5taW5SYW5nZURhdGUgPiBkYXlzLmNoaWxkTm9kZXNbMF0uZGF0ZU9iajtcblxuXHRcdFx0c2VsZi5faGlkZU5leHRNb250aEFycm93ID0gc2VsZi5faGlkZU5leHRNb250aEFycm93IHx8IHNlbGYubWF4UmFuZ2VEYXRlIDwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggKyAxLCAxKTtcblx0XHR9IGVsc2UgdXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXG5cdFx0c2VsZi5kYXlzLmFwcGVuZENoaWxkKGRheXMpO1xuXHRcdHJldHVybiBzZWxmLmRheXM7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZE1vbnRoTmF2KCkge1xuXHRcdHZhciBtb250aE5hdkZyYWdtZW50ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZURvY3VtZW50RnJhZ21lbnQoKTtcblx0XHRzZWxmLm1vbnRoTmF2ID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1tb250aFwiKTtcblxuXHRcdHNlbGYucHJldk1vbnRoTmF2ID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItcHJldi1tb250aFwiKTtcblx0XHRzZWxmLnByZXZNb250aE5hdi5pbm5lckhUTUwgPSBzZWxmLmNvbmZpZy5wcmV2QXJyb3c7XG5cblx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImN1ci1tb250aFwiKTtcblx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQudGl0bGUgPSBzZWxmLmwxMG4uc2Nyb2xsVGl0bGU7XG5cblx0XHR2YXIgeWVhcklucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJjdXIteWVhclwiKTtcblx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudCA9IHllYXJJbnB1dC5jaGlsZE5vZGVzWzBdO1xuXHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LnRpdGxlID0gc2VsZi5sMTBuLnNjcm9sbFRpdGxlO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1pbkRhdGUpIHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1pbiA9IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5tYXggPSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCk7XG5cblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmRpc2FibGVkID0gc2VsZi5jb25maWcubWluRGF0ZSAmJiBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHR9XG5cblx0XHRzZWxmLm5leHRNb250aE5hdiA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLW5leHQtbW9udGhcIik7XG5cdFx0c2VsZi5uZXh0TW9udGhOYXYuaW5uZXJIVE1MID0gc2VsZi5jb25maWcubmV4dEFycm93O1xuXG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItY3VycmVudC1tb250aFwiKTtcblx0XHRzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGguYXBwZW5kQ2hpbGQoc2VsZi5jdXJyZW50TW9udGhFbGVtZW50KTtcblx0XHRzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGguYXBwZW5kQ2hpbGQoeWVhcklucHV0KTtcblxuXHRcdG1vbnRoTmF2RnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5wcmV2TW9udGhOYXYpO1xuXHRcdG1vbnRoTmF2RnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoKTtcblx0XHRtb250aE5hdkZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYubmV4dE1vbnRoTmF2KTtcblx0XHRzZWxmLm1vbnRoTmF2LmFwcGVuZENoaWxkKG1vbnRoTmF2RnJhZ21lbnQpO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwiX2hpZGVQcmV2TW9udGhBcnJvd1wiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX19oaWRlUHJldk1vbnRoQXJyb3c7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRpZiAodGhpcy5fX2hpZGVQcmV2TW9udGhBcnJvdyAhPT0gYm9vbCkgc2VsZi5wcmV2TW9udGhOYXYuc3R5bGUuZGlzcGxheSA9IGJvb2wgPyBcIm5vbmVcIiA6IFwiYmxvY2tcIjtcblx0XHRcdFx0dGhpcy5fX2hpZGVQcmV2TW9udGhBcnJvdyA9IGJvb2w7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZiwgXCJfaGlkZU5leHRNb250aEFycm93XCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5fX2hpZGVOZXh0TW9udGhBcnJvdztcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChib29sKSB7XG5cdFx0XHRcdGlmICh0aGlzLl9faGlkZU5leHRNb250aEFycm93ICE9PSBib29sKSBzZWxmLm5leHRNb250aE5hdi5zdHlsZS5kaXNwbGF5ID0gYm9vbCA/IFwibm9uZVwiIDogXCJibG9ja1wiO1xuXHRcdFx0XHR0aGlzLl9faGlkZU5leHRNb250aEFycm93ID0gYm9vbDtcblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblxuXHRcdHJldHVybiBzZWxmLm1vbnRoTmF2O1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGRUaW1lKCkge1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcImhhc1RpbWVcIik7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcIm5vQ2FsZW5kYXJcIik7XG5cdFx0c2VsZi50aW1lQ29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci10aW1lXCIpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lci50YWJJbmRleCA9IC0xO1xuXHRcdHZhciBzZXBhcmF0b3IgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci10aW1lLXNlcGFyYXRvclwiLCBcIjpcIik7XG5cblx0XHR2YXIgaG91cklucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJmbGF0cGlja3ItaG91clwiKTtcblx0XHRzZWxmLmhvdXJFbGVtZW50ID0gaG91cklucHV0LmNoaWxkTm9kZXNbMF07XG5cblx0XHR2YXIgbWludXRlSW5wdXQgPSBjcmVhdGVOdW1iZXJJbnB1dChcImZsYXRwaWNrci1taW51dGVcIik7XG5cdFx0c2VsZi5taW51dGVFbGVtZW50ID0gbWludXRlSW5wdXQuY2hpbGROb2Rlc1swXTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudGFiSW5kZXggPSBzZWxmLm1pbnV0ZUVsZW1lbnQudGFiSW5kZXggPSAtMTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZChzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA/IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLmdldEhvdXJzKCkgOiBzZWxmLmNvbmZpZy5kZWZhdWx0SG91cik7XG5cblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZChzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA/IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLmdldE1pbnV0ZXMoKSA6IHNlbGYuY29uZmlnLmRlZmF1bHRNaW51dGUpO1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC5zdGVwID0gc2VsZi5jb25maWcuaG91ckluY3JlbWVudDtcblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQuc3RlcCA9IHNlbGYuY29uZmlnLm1pbnV0ZUluY3JlbWVudDtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQubWluID0gc2VsZi5jb25maWcudGltZV8yNGhyID8gMCA6IDE7XG5cdFx0c2VsZi5ob3VyRWxlbWVudC5tYXggPSBzZWxmLmNvbmZpZy50aW1lXzI0aHIgPyAyMyA6IDEyO1xuXG5cdFx0c2VsZi5taW51dGVFbGVtZW50Lm1pbiA9IDA7XG5cdFx0c2VsZi5taW51dGVFbGVtZW50Lm1heCA9IDU5O1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC50aXRsZSA9IHNlbGYubWludXRlRWxlbWVudC50aXRsZSA9IHNlbGYubDEwbi5zY3JvbGxUaXRsZTtcblxuXHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChob3VySW5wdXQpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChzZXBhcmF0b3IpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChtaW51dGVJbnB1dCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcudGltZV8yNGhyKSBzZWxmLnRpbWVDb250YWluZXIuY2xhc3NMaXN0LmFkZChcInRpbWUyNGhyXCIpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMpIHtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwiaGFzU2Vjb25kc1wiKTtcblxuXHRcdFx0dmFyIHNlY29uZElucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJmbGF0cGlja3Itc2Vjb25kXCIpO1xuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50ID0gc2Vjb25kSW5wdXQuY2hpbGROb2Rlc1swXTtcblxuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50LnZhbHVlID0gc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPyBzZWxmLnBhZChzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRTZWNvbmRzKCkpIDogXCIwMFwiO1xuXG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQuc3RlcCA9IHNlbGYubWludXRlRWxlbWVudC5zdGVwO1xuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50Lm1pbiA9IHNlbGYubWludXRlRWxlbWVudC5taW47XG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQubWF4ID0gc2VsZi5taW51dGVFbGVtZW50Lm1heDtcblxuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXRpbWUtc2VwYXJhdG9yXCIsIFwiOlwiKSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoc2Vjb25kSW5wdXQpO1xuXHRcdH1cblxuXHRcdGlmICghc2VsZi5jb25maWcudGltZV8yNGhyKSB7XG5cdFx0XHQvLyBhZGQgc2VsZi5hbVBNIGlmIGFwcHJvcHJpYXRlXG5cdFx0XHRzZWxmLmFtUE0gPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1hbS1wbVwiLCBbXCJBTVwiLCBcIlBNXCJdW3NlbGYuaG91ckVsZW1lbnQudmFsdWUgPiAxMSB8IDBdKTtcblx0XHRcdHNlbGYuYW1QTS50aXRsZSA9IHNlbGYubDEwbi50b2dnbGVUaXRsZTtcblx0XHRcdHNlbGYuYW1QTS50YWJJbmRleCA9IC0xO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlbGYuYW1QTSk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHNlbGYudGltZUNvbnRhaW5lcjtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkV2Vla2RheXMoKSB7XG5cdFx0aWYgKCFzZWxmLndlZWtkYXlDb250YWluZXIpIHNlbGYud2Vla2RheUNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd2Vla2RheXNcIik7XG5cblx0XHR2YXIgZmlyc3REYXlPZldlZWsgPSBzZWxmLmwxMG4uZmlyc3REYXlPZldlZWs7XG5cdFx0dmFyIHdlZWtkYXlzID0gc2VsZi5sMTBuLndlZWtkYXlzLnNob3J0aGFuZC5zbGljZSgpO1xuXG5cdFx0aWYgKGZpcnN0RGF5T2ZXZWVrID4gMCAmJiBmaXJzdERheU9mV2VlayA8IHdlZWtkYXlzLmxlbmd0aCkge1xuXHRcdFx0d2Vla2RheXMgPSBbXS5jb25jYXQod2Vla2RheXMuc3BsaWNlKGZpcnN0RGF5T2ZXZWVrLCB3ZWVrZGF5cy5sZW5ndGgpLCB3ZWVrZGF5cy5zcGxpY2UoMCwgZmlyc3REYXlPZldlZWspKTtcblx0XHR9XG5cblx0XHRzZWxmLndlZWtkYXlDb250YWluZXIuaW5uZXJIVE1MID0gXCJcXG5cXHRcXHQ8c3BhbiBjbGFzcz1mbGF0cGlja3Itd2Vla2RheT5cXG5cXHRcXHRcXHRcIiArIHdlZWtkYXlzLmpvaW4oXCI8L3NwYW4+PHNwYW4gY2xhc3M9ZmxhdHBpY2tyLXdlZWtkYXk+XCIpICsgXCJcXG5cXHRcXHQ8L3NwYW4+XFxuXFx0XFx0XCI7XG5cblx0XHRyZXR1cm4gc2VsZi53ZWVrZGF5Q29udGFpbmVyO1xuXHR9XG5cblx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0ZnVuY3Rpb24gYnVpbGRXZWVrcygpIHtcblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJoYXNXZWVrc1wiKTtcblx0XHRzZWxmLndlZWtXcmFwcGVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci13ZWVrd3JhcHBlclwiKTtcblx0XHRzZWxmLndlZWtXcmFwcGVyLmFwcGVuZENoaWxkKGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXdlZWtkYXlcIiwgc2VsZi5sMTBuLndlZWtBYmJyZXZpYXRpb24pKTtcblx0XHRzZWxmLndlZWtOdW1iZXJzID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci13ZWVrc1wiKTtcblx0XHRzZWxmLndlZWtXcmFwcGVyLmFwcGVuZENoaWxkKHNlbGYud2Vla051bWJlcnMpO1xuXG5cdFx0cmV0dXJuIHNlbGYud2Vla1dyYXBwZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBjaGFuZ2VNb250aCh2YWx1ZSwgaXNfb2Zmc2V0KSB7XG5cdFx0aXNfb2Zmc2V0ID0gdHlwZW9mIGlzX29mZnNldCA9PT0gXCJ1bmRlZmluZWRcIiB8fCBpc19vZmZzZXQ7XG5cdFx0dmFyIGRlbHRhID0gaXNfb2Zmc2V0ID8gdmFsdWUgOiB2YWx1ZSAtIHNlbGYuY3VycmVudE1vbnRoO1xuXG5cdFx0aWYgKGRlbHRhIDwgMCAmJiBzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgfHwgZGVsdGEgPiAwICYmIHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdykgcmV0dXJuO1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGggKz0gZGVsdGE7XG5cblx0XHRpZiAoc2VsZi5jdXJyZW50TW9udGggPCAwIHx8IHNlbGYuY3VycmVudE1vbnRoID4gMTEpIHtcblx0XHRcdHNlbGYuY3VycmVudFllYXIgKz0gc2VsZi5jdXJyZW50TW9udGggPiAxMSA/IDEgOiAtMTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0gKHNlbGYuY3VycmVudE1vbnRoICsgMTIpICUgMTI7XG5cblx0XHRcdHRyaWdnZXJFdmVudChcIlllYXJDaGFuZ2VcIik7XG5cdFx0fVxuXG5cdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXHRcdGJ1aWxkRGF5cygpO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSBzZWxmLmRheXMuZm9jdXMoKTtcblxuXHRcdHRyaWdnZXJFdmVudChcIk1vbnRoQ2hhbmdlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2xlYXIodHJpZ2dlckNoYW5nZUV2ZW50KSB7XG5cdFx0c2VsZi5pbnB1dC52YWx1ZSA9IFwiXCI7XG5cblx0XHRpZiAoc2VsZi5hbHRJbnB1dCkgc2VsZi5hbHRJbnB1dC52YWx1ZSA9IFwiXCI7XG5cblx0XHRpZiAoc2VsZi5tb2JpbGVJbnB1dCkgc2VsZi5tb2JpbGVJbnB1dC52YWx1ZSA9IFwiXCI7XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbXTtcblx0XHRzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IG51bGw7XG5cdFx0c2VsZi5zaG93VGltZUlucHV0ID0gZmFsc2U7XG5cblx0XHRzZWxmLnJlZHJhdygpO1xuXG5cdFx0aWYgKHRyaWdnZXJDaGFuZ2VFdmVudCAhPT0gZmFsc2UpXG5cdFx0XHQvLyB0cmlnZ2VyQ2hhbmdlRXZlbnQgaXMgdHJ1ZSAoZGVmYXVsdCkgb3IgYW4gRXZlbnRcblx0XHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNsb3NlKCkge1xuXHRcdHNlbGYuaXNPcGVuID0gZmFsc2U7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LnJlbW92ZShcIm9wZW5cIik7XG5cdFx0XHQoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5jbGFzc0xpc3QucmVtb3ZlKFwiYWN0aXZlXCIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIkNsb3NlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gZGVzdHJveShpbnN0YW5jZSkge1xuXHRcdGluc3RhbmNlID0gaW5zdGFuY2UgfHwgc2VsZjtcblx0XHRpbnN0YW5jZS5jbGVhcihmYWxzZSk7XG5cblx0XHR3aW5kb3cucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInJlc2l6ZVwiLCBpbnN0YW5jZS5kZWJvdW5jZWRSZXNpemUpO1xuXG5cdFx0d2luZG93LmRvY3VtZW50LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBkb2N1bWVudENsaWNrKTtcblx0XHR3aW5kb3cuZG9jdW1lbnQucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInRvdWNoc3RhcnRcIiwgZG9jdW1lbnRDbGljayk7XG5cdFx0d2luZG93LmRvY3VtZW50LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJibHVyXCIsIGRvY3VtZW50Q2xpY2spO1xuXG5cdFx0aWYgKGluc3RhbmNlLnRpbWVDb250YWluZXIpIGluc3RhbmNlLnRpbWVDb250YWluZXIucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInRyYW5zaXRpb25lbmRcIiwgcG9zaXRpb25DYWxlbmRhcik7XG5cblx0XHRpZiAoaW5zdGFuY2UubW9iaWxlSW5wdXQpIHtcblx0XHRcdGlmIChpbnN0YW5jZS5tb2JpbGVJbnB1dC5wYXJlbnROb2RlKSBpbnN0YW5jZS5tb2JpbGVJbnB1dC5wYXJlbnROb2RlLnJlbW92ZUNoaWxkKGluc3RhbmNlLm1vYmlsZUlucHV0KTtcblx0XHRcdGRlbGV0ZSBpbnN0YW5jZS5tb2JpbGVJbnB1dDtcblx0XHR9IGVsc2UgaWYgKGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyICYmIGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyLnBhcmVudE5vZGUpIGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoaW5zdGFuY2UuY2FsZW5kYXJDb250YWluZXIpO1xuXG5cdFx0aWYgKGluc3RhbmNlLmFsdElucHV0KSB7XG5cdFx0XHRpbnN0YW5jZS5pbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0XHRpZiAoaW5zdGFuY2UuYWx0SW5wdXQucGFyZW50Tm9kZSkgaW5zdGFuY2UuYWx0SW5wdXQucGFyZW50Tm9kZS5yZW1vdmVDaGlsZChpbnN0YW5jZS5hbHRJbnB1dCk7XG5cdFx0XHRkZWxldGUgaW5zdGFuY2UuYWx0SW5wdXQ7XG5cdFx0fVxuXG5cdFx0aW5zdGFuY2UuaW5wdXQudHlwZSA9IGluc3RhbmNlLmlucHV0Ll90eXBlO1xuXHRcdGluc3RhbmNlLmlucHV0LmNsYXNzTGlzdC5yZW1vdmUoXCJmbGF0cGlja3ItaW5wdXRcIik7XG5cdFx0aW5zdGFuY2UuaW5wdXQucmVtb3ZlRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIG9wZW4pO1xuXHRcdGluc3RhbmNlLmlucHV0LnJlbW92ZUF0dHJpYnV0ZShcInJlYWRvbmx5XCIpO1xuXG5cdFx0ZGVsZXRlIGluc3RhbmNlLmlucHV0Ll9mbGF0cGlja3I7XG5cdH1cblxuXHRmdW5jdGlvbiBpc0NhbGVuZGFyRWxlbShlbGVtKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmFwcGVuZFRvICYmIHNlbGYuY29uZmlnLmFwcGVuZFRvLmNvbnRhaW5zKGVsZW0pKSByZXR1cm4gdHJ1ZTtcblxuXHRcdHZhciBlID0gZWxlbTtcblx0XHR3aGlsZSAoZSkge1xuXG5cdFx0XHRpZiAoZSA9PT0gc2VsZi5jYWxlbmRhckNvbnRhaW5lcikgcmV0dXJuIHRydWU7XG5cdFx0XHRlID0gZS5wYXJlbnROb2RlO1xuXHRcdH1cblxuXHRcdHJldHVybiBmYWxzZTtcblx0fVxuXG5cdGZ1bmN0aW9uIGRvY3VtZW50Q2xpY2soZSkge1xuXHRcdGlmIChzZWxmLmlzT3BlbiAmJiAhc2VsZi5jb25maWcuaW5saW5lKSB7XG5cdFx0XHR2YXIgaXNDYWxlbmRhckVsZW1lbnQgPSBpc0NhbGVuZGFyRWxlbShlLnRhcmdldCk7XG5cdFx0XHR2YXIgaXNJbnB1dCA9IGUudGFyZ2V0ID09PSBzZWxmLmlucHV0IHx8IGUudGFyZ2V0ID09PSBzZWxmLmFsdElucHV0IHx8IHNlbGYuZWxlbWVudC5jb250YWlucyhlLnRhcmdldCkgfHxcblx0XHRcdC8vIHdlYiBjb21wb25lbnRzXG5cdFx0XHRlLnBhdGggJiYgZS5wYXRoLmluZGV4T2YgJiYgKH5lLnBhdGguaW5kZXhPZihzZWxmLmlucHV0KSB8fCB+ZS5wYXRoLmluZGV4T2Yoc2VsZi5hbHRJbnB1dCkpO1xuXG5cdFx0XHR2YXIgbG9zdEZvY3VzID0gZS50eXBlID09PSBcImJsdXJcIiA/IGlzSW5wdXQgJiYgZS5yZWxhdGVkVGFyZ2V0ICYmICFpc0NhbGVuZGFyRWxlbShlLnJlbGF0ZWRUYXJnZXQpIDogIWlzSW5wdXQgJiYgIWlzQ2FsZW5kYXJFbGVtZW50O1xuXG5cdFx0XHRpZiAobG9zdEZvY3VzKSB7XG5cdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0c2VsZi5jbG9zZSgpO1xuXG5cdFx0XHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSkge1xuXHRcdFx0XHRcdHNlbGYuY2xlYXIoKTtcblx0XHRcdFx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gZm9ybWF0RGF0ZShmcm10LCBkYXRlT2JqKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmZvcm1hdERhdGUpIHJldHVybiBzZWxmLmNvbmZpZy5mb3JtYXREYXRlKGZybXQsIGRhdGVPYmopO1xuXG5cdFx0dmFyIGNoYXJzID0gZnJtdC5zcGxpdChcIlwiKTtcblx0XHRyZXR1cm4gY2hhcnMubWFwKGZ1bmN0aW9uIChjLCBpKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5mb3JtYXRzW2NdICYmIGNoYXJzW2kgLSAxXSAhPT0gXCJcXFxcXCIgPyBzZWxmLmZvcm1hdHNbY10oZGF0ZU9iaikgOiBjICE9PSBcIlxcXFxcIiA/IGMgOiBcIlwiO1xuXHRcdH0pLmpvaW4oXCJcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBjaGFuZ2VZZWFyKG5ld1llYXIpIHtcblx0XHRpZiAoIW5ld1llYXIgfHwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWluICYmIG5ld1llYXIgPCBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5taW4gfHwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWF4ICYmIG5ld1llYXIgPiBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5tYXgpIHJldHVybjtcblxuXHRcdHZhciBuZXdZZWFyTnVtID0gcGFyc2VJbnQobmV3WWVhciwgMTApLFxuXHRcdCAgICBpc05ld1llYXIgPSBzZWxmLmN1cnJlbnRZZWFyICE9PSBuZXdZZWFyTnVtO1xuXG5cdFx0c2VsZi5jdXJyZW50WWVhciA9IG5ld1llYXJOdW0gfHwgc2VsZi5jdXJyZW50WWVhcjtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlICYmIHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSkge1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSBNYXRoLm1pbihzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1vbnRoKCksIHNlbGYuY3VycmVudE1vbnRoKTtcblx0XHR9IGVsc2UgaWYgKHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IE1hdGgubWF4KHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TW9udGgoKSwgc2VsZi5jdXJyZW50TW9udGgpO1xuXHRcdH1cblxuXHRcdGlmIChpc05ld1llYXIpIHtcblx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJZZWFyQ2hhbmdlXCIpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGlzRW5hYmxlZChkYXRlLCB0aW1lbGVzcykge1xuXHRcdHZhciBsdG1pbiA9IGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLmNvbmZpZy5taW5EYXRlLCB0eXBlb2YgdGltZWxlc3MgIT09IFwidW5kZWZpbmVkXCIgPyB0aW1lbGVzcyA6ICFzZWxmLm1pbkRhdGVIYXNUaW1lKSA8IDA7XG5cdFx0dmFyIGd0bWF4ID0gY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuY29uZmlnLm1heERhdGUsIHR5cGVvZiB0aW1lbGVzcyAhPT0gXCJ1bmRlZmluZWRcIiA/IHRpbWVsZXNzIDogIXNlbGYubWF4RGF0ZUhhc1RpbWUpID4gMDtcblxuXHRcdGlmIChsdG1pbiB8fCBndG1heCkgcmV0dXJuIGZhbHNlO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoICYmICFzZWxmLmNvbmZpZy5kaXNhYmxlLmxlbmd0aCkgcmV0dXJuIHRydWU7XG5cblx0XHR2YXIgZGF0ZVRvQ2hlY2sgPSBzZWxmLnBhcnNlRGF0ZShkYXRlLCB0cnVlKTsgLy8gdGltZWxlc3NcblxuXHRcdHZhciBib29sID0gc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCA+IDAsXG5cdFx0ICAgIGFycmF5ID0gYm9vbCA/IHNlbGYuY29uZmlnLmVuYWJsZSA6IHNlbGYuY29uZmlnLmRpc2FibGU7XG5cblx0XHRmb3IgKHZhciBpID0gMCwgZDsgaSA8IGFycmF5Lmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRkID0gYXJyYXlbaV07XG5cblx0XHRcdGlmIChkIGluc3RhbmNlb2YgRnVuY3Rpb24gJiYgZChkYXRlVG9DaGVjaykpIC8vIGRpc2FibGVkIGJ5IGZ1bmN0aW9uXG5cdFx0XHRcdHJldHVybiBib29sO2Vsc2UgaWYgKGQgaW5zdGFuY2VvZiBEYXRlICYmIGQuZ2V0VGltZSgpID09PSBkYXRlVG9DaGVjay5nZXRUaW1lKCkpXG5cdFx0XHRcdC8vIGRpc2FibGVkIGJ5IGRhdGVcblx0XHRcdFx0cmV0dXJuIGJvb2w7ZWxzZSBpZiAodHlwZW9mIGQgPT09IFwic3RyaW5nXCIgJiYgc2VsZi5wYXJzZURhdGUoZCwgdHJ1ZSkuZ2V0VGltZSgpID09PSBkYXRlVG9DaGVjay5nZXRUaW1lKCkpXG5cdFx0XHRcdC8vIGRpc2FibGVkIGJ5IGRhdGUgc3RyaW5nXG5cdFx0XHRcdHJldHVybiBib29sO2Vsc2UgaWYgKCAvLyBkaXNhYmxlZCBieSByYW5nZVxuXHRcdFx0KHR5cGVvZiBkID09PSBcInVuZGVmaW5lZFwiID8gXCJ1bmRlZmluZWRcIiA6IF90eXBlb2YoZCkpID09PSBcIm9iamVjdFwiICYmIGQuZnJvbSAmJiBkLnRvICYmIGRhdGVUb0NoZWNrID49IGQuZnJvbSAmJiBkYXRlVG9DaGVjayA8PSBkLnRvKSByZXR1cm4gYm9vbDtcblx0XHR9XG5cblx0XHRyZXR1cm4gIWJvb2w7XG5cdH1cblxuXHRmdW5jdGlvbiBvbktleURvd24oZSkge1xuXG5cdFx0aWYgKGUudGFyZ2V0ID09PSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KSAmJiBlLndoaWNoID09PSAxMykgc2VsZWN0RGF0ZShlKTtlbHNlIGlmIChzZWxmLmlzT3BlbiB8fCBzZWxmLmNvbmZpZy5pbmxpbmUpIHtcblx0XHRcdHN3aXRjaCAoZS5rZXkpIHtcblx0XHRcdFx0Y2FzZSBcIkVudGVyXCI6XG5cdFx0XHRcdFx0aWYgKHNlbGYudGltZUNvbnRhaW5lciAmJiBzZWxmLnRpbWVDb250YWluZXIuY29udGFpbnMoZS50YXJnZXQpKSB1cGRhdGVWYWx1ZSgpO2Vsc2Ugc2VsZWN0RGF0ZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJFc2NhcGVcIjpcblx0XHRcdFx0XHQvLyBlc2NhcGVcblx0XHRcdFx0XHRzZWxmLmNsb3NlKCk7XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSBcIkFycm93TGVmdFwiOlxuXHRcdFx0XHRcdGlmIChlLnRhcmdldCAhPT0gc2VsZi5pbnB1dCAmIGUudGFyZ2V0ICE9PSBzZWxmLmFsdElucHV0KSB7XG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0XHRjaGFuZ2VNb250aCgtMSk7XG5cdFx0XHRcdFx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQuZm9jdXMoKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSBcIkFycm93VXBcIjpcblx0XHRcdFx0XHRpZiAoIXNlbGYudGltZUNvbnRhaW5lciB8fCAhc2VsZi50aW1lQ29udGFpbmVyLmNvbnRhaW5zKGUudGFyZ2V0KSkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50WWVhcisrO1xuXHRcdFx0XHRcdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRcdFx0XHR9IGVsc2UgdXBkYXRlVGltZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJBcnJvd1JpZ2h0XCI6XG5cdFx0XHRcdFx0aWYgKGUudGFyZ2V0ICE9PSBzZWxmLmlucHV0ICYgZS50YXJnZXQgIT09IHNlbGYuYWx0SW5wdXQpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdGNoYW5nZU1vbnRoKDEpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LmZvY3VzKCk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJBcnJvd0Rvd25cIjpcblx0XHRcdFx0XHRpZiAoIXNlbGYudGltZUNvbnRhaW5lciB8fCAhc2VsZi50aW1lQ29udGFpbmVyLmNvbnRhaW5zKGUudGFyZ2V0KSkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50WWVhci0tO1xuXHRcdFx0XHRcdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRcdFx0XHR9IGVsc2UgdXBkYXRlVGltZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJUYWJcIjpcblx0XHRcdFx0XHRpZiAoZS50YXJnZXQgPT09IHNlbGYuaG91ckVsZW1lbnQpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdHNlbGYubWludXRlRWxlbWVudC5zZWxlY3QoKTtcblx0XHRcdFx0XHR9IGVsc2UgaWYgKGUudGFyZ2V0ID09PSBzZWxmLm1pbnV0ZUVsZW1lbnQgJiYgc2VsZi5hbVBNKSB7XG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0XHRzZWxmLmFtUE0uZm9jdXMoKTtcblx0XHRcdFx0XHR9XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRkZWZhdWx0OlxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHR9XG5cblx0XHRcdHRyaWdnZXJFdmVudChcIktleURvd25cIiwgZSk7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gb25Nb3VzZU92ZXIoZSkge1xuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoICE9PSAxIHx8ICFlLnRhcmdldC5jbGFzc0xpc3QuY29udGFpbnMoXCJmbGF0cGlja3ItZGF5XCIpKSByZXR1cm47XG5cblx0XHR2YXIgaG92ZXJEYXRlID0gZS50YXJnZXQuZGF0ZU9iaixcblx0XHQgICAgaW5pdGlhbERhdGUgPSBzZWxmLnBhcnNlRGF0ZShzZWxmLnNlbGVjdGVkRGF0ZXNbMF0sIHRydWUpLFxuXHRcdCAgICByYW5nZVN0YXJ0RGF0ZSA9IE1hdGgubWluKGhvdmVyRGF0ZS5nZXRUaW1lKCksIHNlbGYuc2VsZWN0ZWREYXRlc1swXS5nZXRUaW1lKCkpLFxuXHRcdCAgICByYW5nZUVuZERhdGUgPSBNYXRoLm1heChob3ZlckRhdGUuZ2V0VGltZSgpLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0uZ2V0VGltZSgpKSxcblx0XHQgICAgY29udGFpbnNEaXNhYmxlZCA9IGZhbHNlO1xuXG5cdFx0Zm9yICh2YXIgdCA9IHJhbmdlU3RhcnREYXRlOyB0IDwgcmFuZ2VFbmREYXRlOyB0ICs9IHNlbGYudXRpbHMuZHVyYXRpb24uREFZKSB7XG5cdFx0XHRpZiAoIWlzRW5hYmxlZChuZXcgRGF0ZSh0KSkpIHtcblx0XHRcdFx0Y29udGFpbnNEaXNhYmxlZCA9IHRydWU7XG5cdFx0XHRcdGJyZWFrO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHZhciBfbG9vcCA9IGZ1bmN0aW9uIF9sb29wKHRpbWVzdGFtcCwgaSkge1xuXHRcdFx0dmFyIG91dE9mUmFuZ2UgPSB0aW1lc3RhbXAgPCBzZWxmLm1pblJhbmdlRGF0ZS5nZXRUaW1lKCkgfHwgdGltZXN0YW1wID4gc2VsZi5tYXhSYW5nZURhdGUuZ2V0VGltZSgpO1xuXG5cdFx0XHRpZiAob3V0T2ZSYW5nZSkge1xuXHRcdFx0XHRzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QuYWRkKFwibm90QWxsb3dlZFwiKTtcblx0XHRcdFx0W1wiaW5SYW5nZVwiLCBcInN0YXJ0UmFuZ2VcIiwgXCJlbmRSYW5nZVwiXS5mb3JFYWNoKGZ1bmN0aW9uIChjKSB7XG5cdFx0XHRcdFx0c2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LnJlbW92ZShjKTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdHJldHVybiBcImNvbnRpbnVlXCI7XG5cdFx0XHR9IGVsc2UgaWYgKGNvbnRhaW5zRGlzYWJsZWQgJiYgIW91dE9mUmFuZ2UpIHJldHVybiBcImNvbnRpbnVlXCI7XG5cblx0XHRcdFtcInN0YXJ0UmFuZ2VcIiwgXCJpblJhbmdlXCIsIFwiZW5kUmFuZ2VcIiwgXCJub3RBbGxvd2VkXCJdLmZvckVhY2goZnVuY3Rpb24gKGMpIHtcblx0XHRcdFx0c2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LnJlbW92ZShjKTtcblx0XHRcdH0pO1xuXG5cdFx0XHR2YXIgbWluUmFuZ2VEYXRlID0gTWF0aC5tYXgoc2VsZi5taW5SYW5nZURhdGUuZ2V0VGltZSgpLCByYW5nZVN0YXJ0RGF0ZSksXG5cdFx0XHQgICAgbWF4UmFuZ2VEYXRlID0gTWF0aC5taW4oc2VsZi5tYXhSYW5nZURhdGUuZ2V0VGltZSgpLCByYW5nZUVuZERhdGUpO1xuXG5cdFx0XHRlLnRhcmdldC5jbGFzc0xpc3QuYWRkKGhvdmVyRGF0ZSA8IHNlbGYuc2VsZWN0ZWREYXRlc1swXSA/IFwic3RhcnRSYW5nZVwiIDogXCJlbmRSYW5nZVwiKTtcblxuXHRcdFx0aWYgKGluaXRpYWxEYXRlID4gaG92ZXJEYXRlICYmIHRpbWVzdGFtcCA9PT0gaW5pdGlhbERhdGUuZ2V0VGltZSgpKSBzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QuYWRkKFwiZW5kUmFuZ2VcIik7ZWxzZSBpZiAoaW5pdGlhbERhdGUgPCBob3ZlckRhdGUgJiYgdGltZXN0YW1wID09PSBpbml0aWFsRGF0ZS5nZXRUaW1lKCkpIHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5hZGQoXCJzdGFydFJhbmdlXCIpO2Vsc2UgaWYgKHRpbWVzdGFtcCA+PSBtaW5SYW5nZURhdGUgJiYgdGltZXN0YW1wIDw9IG1heFJhbmdlRGF0ZSkgc2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LmFkZChcImluUmFuZ2VcIik7XG5cdFx0fTtcblxuXHRcdGZvciAodmFyIHRpbWVzdGFtcCA9IHNlbGYuZGF5cy5jaGlsZE5vZGVzWzBdLmRhdGVPYmouZ2V0VGltZSgpLCBpID0gMDsgaSA8IDQyOyBpKyssIHRpbWVzdGFtcCArPSBzZWxmLnV0aWxzLmR1cmF0aW9uLkRBWSkge1xuXHRcdFx0dmFyIF9yZXQgPSBfbG9vcCh0aW1lc3RhbXAsIGkpO1xuXG5cdFx0XHRpZiAoX3JldCA9PT0gXCJjb250aW51ZVwiKSBjb250aW51ZTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBvblJlc2l6ZSgpIHtcblx0XHRpZiAoc2VsZi5pc09wZW4gJiYgIXNlbGYuY29uZmlnLnN0YXRpYyAmJiAhc2VsZi5jb25maWcuaW5saW5lKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBvcGVuKGUpIHtcblx0XHRpZiAoc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0aWYgKGUpIHtcblx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRlLnRhcmdldC5ibHVyKCk7XG5cdFx0XHR9XG5cblx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLm1vYmlsZUlucHV0LmNsaWNrKCk7XG5cdFx0XHR9LCAwKTtcblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiT3BlblwiKTtcblx0XHRcdHJldHVybjtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5pc09wZW4gfHwgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuZGlzYWJsZWQgfHwgc2VsZi5jb25maWcuaW5saW5lKSByZXR1cm47XG5cblx0XHRzZWxmLmlzT3BlbiA9IHRydWU7XG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwib3BlblwiKTtcblx0XHRwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdFx0KHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuY2xhc3NMaXN0LmFkZChcImFjdGl2ZVwiKTtcblxuXHRcdHRyaWdnZXJFdmVudChcIk9wZW5cIik7XG5cdH1cblxuXHRmdW5jdGlvbiBtaW5NYXhEYXRlU2V0dGVyKHR5cGUpIHtcblx0XHRyZXR1cm4gZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdHZhciBkYXRlT2JqID0gc2VsZi5jb25maWdbXCJfXCIgKyB0eXBlICsgXCJEYXRlXCJdID0gc2VsZi5wYXJzZURhdGUoZGF0ZSk7XG5cblx0XHRcdHZhciBpbnZlcnNlRGF0ZU9iaiA9IHNlbGYuY29uZmlnW1wiX1wiICsgKHR5cGUgPT09IFwibWluXCIgPyBcIm1heFwiIDogXCJtaW5cIikgKyBcIkRhdGVcIl07XG5cdFx0XHR2YXIgaXNWYWxpZERhdGUgPSBkYXRlICYmIGRhdGVPYmogaW5zdGFuY2VvZiBEYXRlO1xuXG5cdFx0XHRpZiAoaXNWYWxpZERhdGUpIHtcblx0XHRcdFx0c2VsZlt0eXBlICsgXCJEYXRlSGFzVGltZVwiXSA9IGRhdGVPYmouZ2V0SG91cnMoKSB8fCBkYXRlT2JqLmdldE1pbnV0ZXMoKSB8fCBkYXRlT2JqLmdldFNlY29uZHMoKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcykge1xuXHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBzZWxmLnNlbGVjdGVkRGF0ZXMuZmlsdGVyKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRcdFx0cmV0dXJuIGlzRW5hYmxlZChkKTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdGlmICghc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCAmJiB0eXBlID09PSBcIm1pblwiKSBzZXRIb3Vyc0Zyb21EYXRlKGRhdGVPYmopO1xuXHRcdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5kYXlzKSB7XG5cdFx0XHRcdHJlZHJhdygpO1xuXG5cdFx0XHRcdGlmIChpc1ZhbGlkRGF0ZSkgc2VsZi5jdXJyZW50WWVhckVsZW1lbnRbdHlwZV0gPSBkYXRlT2JqLmdldEZ1bGxZZWFyKCk7ZWxzZSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5yZW1vdmVBdHRyaWJ1dGUodHlwZSk7XG5cblx0XHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuZGlzYWJsZWQgPSBpbnZlcnNlRGF0ZU9iaiAmJiBkYXRlT2JqICYmIGludmVyc2VEYXRlT2JqLmdldEZ1bGxZZWFyKCkgPT09IGRhdGVPYmouZ2V0RnVsbFllYXIoKTtcblx0XHRcdH1cblx0XHR9O1xuXHR9XG5cblx0ZnVuY3Rpb24gcGFyc2VDb25maWcoKSB7XG5cdFx0dmFyIGJvb2xPcHRzID0gW1widXRjXCIsIFwid3JhcFwiLCBcIndlZWtOdW1iZXJzXCIsIFwiYWxsb3dJbnB1dFwiLCBcImNsaWNrT3BlbnNcIiwgXCJ0aW1lXzI0aHJcIiwgXCJlbmFibGVUaW1lXCIsIFwibm9DYWxlbmRhclwiLCBcImFsdElucHV0XCIsIFwic2hvcnRoYW5kQ3VycmVudE1vbnRoXCIsIFwiaW5saW5lXCIsIFwic3RhdGljXCIsIFwiZW5hYmxlU2Vjb25kc1wiLCBcImRpc2FibGVNb2JpbGVcIl07XG5cblx0XHR2YXIgaG9va3MgPSBbXCJvbkNoYW5nZVwiLCBcIm9uQ2xvc2VcIiwgXCJvbkRheUNyZWF0ZVwiLCBcIm9uS2V5RG93blwiLCBcIm9uTW9udGhDaGFuZ2VcIiwgXCJvbk9wZW5cIiwgXCJvblBhcnNlQ29uZmlnXCIsIFwib25SZWFkeVwiLCBcIm9uVmFsdWVVcGRhdGVcIiwgXCJvblllYXJDaGFuZ2VcIl07XG5cblx0XHRzZWxmLmNvbmZpZyA9IE9iamVjdC5jcmVhdGUoRmxhdHBpY2tyLmRlZmF1bHRDb25maWcpO1xuXG5cdFx0dmFyIHVzZXJDb25maWcgPSBfZXh0ZW5kcyh7fSwgc2VsZi5pbnN0YW5jZUNvbmZpZywgSlNPTi5wYXJzZShKU09OLnN0cmluZ2lmeShzZWxmLmVsZW1lbnQuZGF0YXNldCB8fCB7fSkpKTtcblxuXHRcdHNlbGYuY29uZmlnLnBhcnNlRGF0ZSA9IHVzZXJDb25maWcucGFyc2VEYXRlO1xuXHRcdHNlbGYuY29uZmlnLmZvcm1hdERhdGUgPSB1c2VyQ29uZmlnLmZvcm1hdERhdGU7XG5cblx0XHRfZXh0ZW5kcyhzZWxmLmNvbmZpZywgdXNlckNvbmZpZyk7XG5cblx0XHRpZiAoIXVzZXJDb25maWcuZGF0ZUZvcm1hdCAmJiB1c2VyQ29uZmlnLmVuYWJsZVRpbWUpIHtcblx0XHRcdHNlbGYuY29uZmlnLmRhdGVGb3JtYXQgPSBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyID8gXCJIOmlcIiArIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gXCI6U1wiIDogXCJcIikgOiBGbGF0cGlja3IuZGVmYXVsdENvbmZpZy5kYXRlRm9ybWF0ICsgXCIgSDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpO1xuXHRcdH1cblxuXHRcdGlmICh1c2VyQ29uZmlnLmFsdElucHV0ICYmIHVzZXJDb25maWcuZW5hYmxlVGltZSAmJiAhdXNlckNvbmZpZy5hbHRGb3JtYXQpIHtcblx0XHRcdHNlbGYuY29uZmlnLmFsdEZvcm1hdCA9IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgPyBcImg6aVwiICsgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBcIjpTIEtcIiA6IFwiIEtcIikgOiBGbGF0cGlja3IuZGVmYXVsdENvbmZpZy5hbHRGb3JtYXQgKyAoXCIgaDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpICsgXCIgS1wiKTtcblx0XHR9XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZi5jb25maWcsIFwibWluRGF0ZVwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX21pbkRhdGU7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBtaW5NYXhEYXRlU2V0dGVyKFwibWluXCIpXG5cdFx0fSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZi5jb25maWcsIFwibWF4RGF0ZVwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX21heERhdGU7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBtaW5NYXhEYXRlU2V0dGVyKFwibWF4XCIpXG5cdFx0fSk7XG5cblx0XHRzZWxmLmNvbmZpZy5taW5EYXRlID0gdXNlckNvbmZpZy5taW5EYXRlO1xuXHRcdHNlbGYuY29uZmlnLm1heERhdGUgPSB1c2VyQ29uZmlnLm1heERhdGU7XG5cblx0XHRmb3IgKHZhciBpID0gMDsgaSA8IGJvb2xPcHRzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPSBzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPT09IHRydWUgfHwgc2VsZi5jb25maWdbYm9vbE9wdHNbaV1dID09PSBcInRydWVcIjtcblx0XHR9Zm9yICh2YXIgX2kgPSAwOyBfaSA8IGhvb2tzLmxlbmd0aDsgX2krKykge1xuXHRcdFx0c2VsZi5jb25maWdbaG9va3NbX2ldXSA9IGFycmF5aWZ5KHNlbGYuY29uZmlnW2hvb2tzW19pXV0gfHwgW10pLm1hcChiaW5kVG9JbnN0YW5jZSk7XG5cdFx0fWZvciAodmFyIF9pMiA9IDA7IF9pMiA8IHNlbGYuY29uZmlnLnBsdWdpbnMubGVuZ3RoOyBfaTIrKykge1xuXHRcdFx0dmFyIHBsdWdpbkNvbmYgPSBzZWxmLmNvbmZpZy5wbHVnaW5zW19pMl0oc2VsZikgfHwge307XG5cdFx0XHRmb3IgKHZhciBrZXkgaW4gcGx1Z2luQ29uZikge1xuXG5cdFx0XHRcdGlmIChBcnJheS5pc0FycmF5KHNlbGYuY29uZmlnW2tleV0pIHx8IH5ob29rcy5pbmRleE9mKGtleSkpIHNlbGYuY29uZmlnW2tleV0gPSBhcnJheWlmeShwbHVnaW5Db25mW2tleV0pLm1hcChiaW5kVG9JbnN0YW5jZSkuY29uY2F0KHNlbGYuY29uZmlnW2tleV0pO2Vsc2UgaWYgKHR5cGVvZiB1c2VyQ29uZmlnW2tleV0gPT09IFwidW5kZWZpbmVkXCIpIHNlbGYuY29uZmlnW2tleV0gPSBwbHVnaW5Db25mW2tleV07XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0dHJpZ2dlckV2ZW50KFwiUGFyc2VDb25maWdcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cExvY2FsZSgpIHtcblx0XHRpZiAoX3R5cGVvZihzZWxmLmNvbmZpZy5sb2NhbGUpICE9PSBcIm9iamVjdFwiICYmIHR5cGVvZiBGbGF0cGlja3IubDEwbnNbc2VsZi5jb25maWcubG9jYWxlXSA9PT0gXCJ1bmRlZmluZWRcIikgY29uc29sZS53YXJuKFwiZmxhdHBpY2tyOiBpbnZhbGlkIGxvY2FsZSBcIiArIHNlbGYuY29uZmlnLmxvY2FsZSk7XG5cblx0XHRzZWxmLmwxMG4gPSBfZXh0ZW5kcyhPYmplY3QuY3JlYXRlKEZsYXRwaWNrci5sMTBucy5kZWZhdWx0KSwgX3R5cGVvZihzZWxmLmNvbmZpZy5sb2NhbGUpID09PSBcIm9iamVjdFwiID8gc2VsZi5jb25maWcubG9jYWxlIDogc2VsZi5jb25maWcubG9jYWxlICE9PSBcImRlZmF1bHRcIiA/IEZsYXRwaWNrci5sMTBuc1tzZWxmLmNvbmZpZy5sb2NhbGVdIHx8IHt9IDoge30pO1xuXHR9XG5cblx0ZnVuY3Rpb24gcG9zaXRpb25DYWxlbmRhcihlKSB7XG5cdFx0aWYgKGUgJiYgZS50YXJnZXQgIT09IHNlbGYudGltZUNvbnRhaW5lcikgcmV0dXJuO1xuXG5cdFx0dmFyIGNhbGVuZGFySGVpZ2h0ID0gc2VsZi5jYWxlbmRhckNvbnRhaW5lci5vZmZzZXRIZWlnaHQsXG5cdFx0ICAgIGNhbGVuZGFyV2lkdGggPSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLm9mZnNldFdpZHRoLFxuXHRcdCAgICBjb25maWdQb3MgPSBzZWxmLmNvbmZpZy5wb3NpdGlvbixcblx0XHQgICAgaW5wdXQgPSBzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQsXG5cdFx0ICAgIGlucHV0Qm91bmRzID0gaW5wdXQuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCksXG5cdFx0ICAgIGRpc3RhbmNlRnJvbUJvdHRvbSA9IHdpbmRvdy5pbm5lckhlaWdodCAtIGlucHV0Qm91bmRzLmJvdHRvbSArIGlucHV0Lm9mZnNldEhlaWdodCxcblx0XHQgICAgc2hvd09uVG9wID0gY29uZmlnUG9zID09PSBcImFib3ZlXCIgfHwgY29uZmlnUG9zICE9PSBcImJlbG93XCIgJiYgZGlzdGFuY2VGcm9tQm90dG9tIDwgY2FsZW5kYXJIZWlnaHQgJiYgaW5wdXRCb3VuZHMudG9wID4gY2FsZW5kYXJIZWlnaHQ7XG5cblx0XHR2YXIgdG9wID0gd2luZG93LnBhZ2VZT2Zmc2V0ICsgaW5wdXRCb3VuZHMudG9wICsgKCFzaG93T25Ub3AgPyBpbnB1dC5vZmZzZXRIZWlnaHQgKyAyIDogLWNhbGVuZGFySGVpZ2h0IC0gMik7XG5cblx0XHR0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcImFycm93VG9wXCIsICFzaG93T25Ub3ApO1xuXHRcdHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwiYXJyb3dCb3R0b21cIiwgc2hvd09uVG9wKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUpIHJldHVybjtcblxuXHRcdHZhciBsZWZ0ID0gd2luZG93LnBhZ2VYT2Zmc2V0ICsgaW5wdXRCb3VuZHMubGVmdDtcblx0XHR2YXIgcmlnaHQgPSB3aW5kb3cuZG9jdW1lbnQuYm9keS5vZmZzZXRXaWR0aCAtIGlucHV0Qm91bmRzLnJpZ2h0O1xuXHRcdHZhciByaWdodE1vc3QgPSBsZWZ0ICsgY2FsZW5kYXJXaWR0aCA+IHdpbmRvdy5kb2N1bWVudC5ib2R5Lm9mZnNldFdpZHRoO1xuXG5cdFx0dG9nZ2xlQ2xhc3Moc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgXCJyaWdodE1vc3RcIiwgcmlnaHRNb3N0KTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5zdGF0aWMpIHJldHVybjtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUudG9wID0gdG9wICsgXCJweFwiO1xuXG5cdFx0aWYgKCFyaWdodE1vc3QpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUubGVmdCA9IGxlZnQgKyBcInB4XCI7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLnJpZ2h0ID0gXCJhdXRvXCI7XG5cdFx0fSBlbHNlIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUubGVmdCA9IFwiYXV0b1wiO1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS5yaWdodCA9IHJpZ2h0ICsgXCJweFwiO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHJlZHJhdygpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhciB8fCBzZWxmLmlzTW9iaWxlKSByZXR1cm47XG5cblx0XHRidWlsZFdlZWtkYXlzKCk7XG5cdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXHRcdGJ1aWxkRGF5cygpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2VsZWN0RGF0ZShlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdGUuc3RvcFByb3BhZ2F0aW9uKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuYWxsb3dJbnB1dCAmJiBlLmtleSA9PT0gXCJFbnRlclwiICYmIGUudGFyZ2V0ID09PSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KSkge1xuXHRcdFx0c2VsZi5zZXREYXRlKChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLnZhbHVlLCB0cnVlLCBlLnRhcmdldCA9PT0gc2VsZi5hbHRJbnB1dCA/IHNlbGYuY29uZmlnLmFsdEZvcm1hdCA6IHNlbGYuY29uZmlnLmRhdGVGb3JtYXQpO1xuXHRcdFx0cmV0dXJuIGUudGFyZ2V0LmJsdXIoKTtcblx0XHR9XG5cblx0XHRpZiAoIWUudGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcImZsYXRwaWNrci1kYXlcIikgfHwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwiZGlzYWJsZWRcIikgfHwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwibm90QWxsb3dlZFwiKSkgcmV0dXJuO1xuXG5cdFx0dmFyIHNlbGVjdGVkRGF0ZSA9IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gbmV3IERhdGUoZS50YXJnZXQuZGF0ZU9iai5nZXRUaW1lKCkpO1xuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVFbGVtID0gZS50YXJnZXQ7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJzaW5nbGVcIikgc2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGVjdGVkRGF0ZV07ZWxzZSBpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJtdWx0aXBsZVwiKSB7XG5cdFx0XHR2YXIgc2VsZWN0ZWRJbmRleCA9IGlzRGF0ZVNlbGVjdGVkKHNlbGVjdGVkRGF0ZSk7XG5cdFx0XHRpZiAoc2VsZWN0ZWRJbmRleCkgc2VsZi5zZWxlY3RlZERhdGVzLnNwbGljZShzZWxlY3RlZEluZGV4LCAxKTtlbHNlIHNlbGYuc2VsZWN0ZWREYXRlcy5wdXNoKHNlbGVjdGVkRGF0ZSk7XG5cdFx0fSBlbHNlIGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID09PSAyKSBzZWxmLmNsZWFyKCk7XG5cblx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcy5wdXNoKHNlbGVjdGVkRGF0ZSk7XG5cblx0XHRcdC8vIHVubGVzcyBzZWxlY3Rpbmcgc2FtZSBkYXRlIHR3aWNlLCBzb3J0IGFzY2VuZGluZ2x5XG5cdFx0XHRpZiAoY29tcGFyZURhdGVzKHNlbGVjdGVkRGF0ZSwgc2VsZi5zZWxlY3RlZERhdGVzWzBdLCB0cnVlKSAhPT0gMCkgc2VsZi5zZWxlY3RlZERhdGVzLnNvcnQoZnVuY3Rpb24gKGEsIGIpIHtcblx0XHRcdFx0cmV0dXJuIGEuZ2V0VGltZSgpIC0gYi5nZXRUaW1lKCk7XG5cdFx0XHR9KTtcblx0XHR9XG5cblx0XHRzZXRIb3Vyc0Zyb21JbnB1dHMoKTtcblxuXHRcdGlmIChzZWxlY3RlZERhdGUuZ2V0TW9udGgoKSAhPT0gc2VsZi5jdXJyZW50TW9udGggJiYgc2VsZi5jb25maWcubW9kZSAhPT0gXCJyYW5nZVwiKSB7XG5cdFx0XHR2YXIgaXNOZXdZZWFyID0gc2VsZi5jdXJyZW50WWVhciAhPT0gc2VsZWN0ZWREYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyID0gc2VsZWN0ZWREYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IHNlbGVjdGVkRGF0ZS5nZXRNb250aCgpO1xuXG5cdFx0XHRpZiAoaXNOZXdZZWFyKSB0cmlnZ2VyRXZlbnQoXCJZZWFyQ2hhbmdlXCIpO1xuXG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJNb250aENoYW5nZVwiKTtcblx0XHR9XG5cblx0XHRidWlsZERheXMoKTtcblxuXHRcdGlmIChzZWxmLm1pbkRhdGVIYXNUaW1lICYmIHNlbGYuY29uZmlnLmVuYWJsZVRpbWUgJiYgY29tcGFyZURhdGVzKHNlbGVjdGVkRGF0ZSwgc2VsZi5jb25maWcubWluRGF0ZSkgPT09IDApIHNldEhvdXJzRnJvbURhdGUoc2VsZi5jb25maWcubWluRGF0ZSk7XG5cblx0XHR1cGRhdGVWYWx1ZSgpO1xuXG5cdFx0c2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5zaG93VGltZUlucHV0ID0gdHJ1ZTtcblx0XHR9LCA1MCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSkge1xuXHRcdFx0XHRvbk1vdXNlT3ZlcihlKTtcblxuXHRcdFx0XHRzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgfHwgc2VsZi5taW5SYW5nZURhdGUgPiBzZWxmLmRheXMuY2hpbGROb2Rlc1swXS5kYXRlT2JqO1xuXG5cdFx0XHRcdHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyA9IHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyB8fCBzZWxmLm1heFJhbmdlRGF0ZSA8IG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoICsgMSwgMSk7XG5cdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cdFx0XHRcdHNlbGYuY2xvc2UoKTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlVGltZSkgc2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRzZWxmLmhvdXJFbGVtZW50LnNlbGVjdCgpO1xuXHRcdH0sIDQ1MSk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJzaW5nbGVcIiAmJiAhc2VsZi5jb25maWcuZW5hYmxlVGltZSkgc2VsZi5jbG9zZSgpO1xuXG5cdFx0dHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0KG9wdGlvbiwgdmFsdWUpIHtcblx0XHRzZWxmLmNvbmZpZ1tvcHRpb25dID0gdmFsdWU7XG5cdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRqdW1wVG9EYXRlKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXRTZWxlY3RlZERhdGUoaW5wdXREYXRlLCBmb3JtYXQpIHtcblx0XHRpZiAoQXJyYXkuaXNBcnJheShpbnB1dERhdGUpKSBzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUubWFwKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5wYXJzZURhdGUoZCwgZmFsc2UsIGZvcm1hdCk7XG5cdFx0fSk7ZWxzZSBpZiAoaW5wdXREYXRlIGluc3RhbmNlb2YgRGF0ZSB8fCAhaXNOYU4oaW5wdXREYXRlKSkgc2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGYucGFyc2VEYXRlKGlucHV0RGF0ZSldO2Vsc2UgaWYgKGlucHV0RGF0ZSAmJiBpbnB1dERhdGUuc3Vic3RyaW5nKSB7XG5cdFx0XHRzd2l0Y2ggKHNlbGYuY29uZmlnLm1vZGUpIHtcblx0XHRcdFx0Y2FzZSBcInNpbmdsZVwiOlxuXHRcdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtzZWxmLnBhcnNlRGF0ZShpbnB1dERhdGUsIGZhbHNlLCBmb3JtYXQpXTtcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwibXVsdGlwbGVcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUuc3BsaXQoXCI7IFwiKS5tYXAoZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdFx0XHRcdHJldHVybiBzZWxmLnBhcnNlRGF0ZShkYXRlLCBmYWxzZSwgZm9ybWF0KTtcblx0XHRcdFx0XHR9KTtcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwicmFuZ2VcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUuc3BsaXQoc2VsZi5sMTBuLnJhbmdlU2VwYXJhdG9yKS5tYXAoZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdFx0XHRcdHJldHVybiBzZWxmLnBhcnNlRGF0ZShkYXRlLCBmYWxzZSwgZm9ybWF0KTtcblx0XHRcdFx0XHR9KTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGRlZmF1bHQ6XG5cdFx0XHRcdFx0YnJlYWs7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gc2VsZi5zZWxlY3RlZERhdGVzLmZpbHRlcihmdW5jdGlvbiAoZCkge1xuXHRcdFx0cmV0dXJuIGQgaW5zdGFuY2VvZiBEYXRlICYmIGlzRW5hYmxlZChkLCBmYWxzZSk7XG5cdFx0fSk7XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMuc29ydChmdW5jdGlvbiAoYSwgYikge1xuXHRcdFx0cmV0dXJuIGEuZ2V0VGltZSgpIC0gYi5nZXRUaW1lKCk7XG5cdFx0fSk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXREYXRlKGRhdGUsIHRyaWdnZXJDaGFuZ2UsIGZvcm1hdCkge1xuXHRcdGlmICghZGF0ZSkgcmV0dXJuIHNlbGYuY2xlYXIoKTtcblxuXHRcdHNldFNlbGVjdGVkRGF0ZShkYXRlLCBmb3JtYXQpO1xuXG5cdFx0c2VsZi5zaG93VGltZUlucHV0ID0gc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA+IDA7XG5cdFx0c2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBzZWxmLnNlbGVjdGVkRGF0ZXNbMF07XG5cblx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdGp1bXBUb0RhdGUoKTtcblxuXHRcdHNldEhvdXJzRnJvbURhdGUoKTtcblx0XHR1cGRhdGVWYWx1ZSgpO1xuXG5cdFx0aWYgKHRyaWdnZXJDaGFuZ2UpIHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwRGF0ZXMoKSB7XG5cdFx0ZnVuY3Rpb24gcGFyc2VEYXRlUnVsZXMoYXJyKSB7XG5cdFx0XHRmb3IgKHZhciBpID0gYXJyLmxlbmd0aDsgaS0tOykge1xuXHRcdFx0XHRpZiAodHlwZW9mIGFycltpXSA9PT0gXCJzdHJpbmdcIiB8fCArYXJyW2ldKSBhcnJbaV0gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0sIHRydWUpO2Vsc2UgaWYgKGFycltpXSAmJiBhcnJbaV0uZnJvbSAmJiBhcnJbaV0udG8pIHtcblx0XHRcdFx0XHRhcnJbaV0uZnJvbSA9IHNlbGYucGFyc2VEYXRlKGFycltpXS5mcm9tKTtcblx0XHRcdFx0XHRhcnJbaV0udG8gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0udG8pO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cblx0XHRcdHJldHVybiBhcnIuZmlsdGVyKGZ1bmN0aW9uICh4KSB7XG5cdFx0XHRcdHJldHVybiB4O1xuXHRcdFx0fSk7IC8vIHJlbW92ZSBmYWxzeSB2YWx1ZXNcblx0XHR9XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbXTtcblx0XHRzZWxmLm5vdyA9IG5ldyBEYXRlKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGgpIHNlbGYuY29uZmlnLmRpc2FibGUgPSBwYXJzZURhdGVSdWxlcyhzZWxmLmNvbmZpZy5kaXNhYmxlKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoKSBzZWxmLmNvbmZpZy5lbmFibGUgPSBwYXJzZURhdGVSdWxlcyhzZWxmLmNvbmZpZy5lbmFibGUpO1xuXG5cdFx0c2V0U2VsZWN0ZWREYXRlKHNlbGYuY29uZmlnLmRlZmF1bHREYXRlIHx8IHNlbGYuaW5wdXQudmFsdWUpO1xuXG5cdFx0dmFyIGluaXRpYWxEYXRlID0gc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA/IHNlbGYuc2VsZWN0ZWREYXRlc1swXSA6IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jb25maWcubWluRGF0ZS5nZXRUaW1lKCkgPiBzZWxmLm5vdyA/IHNlbGYuY29uZmlnLm1pbkRhdGUgOiBzZWxmLmNvbmZpZy5tYXhEYXRlICYmIHNlbGYuY29uZmlnLm1heERhdGUuZ2V0VGltZSgpIDwgc2VsZi5ub3cgPyBzZWxmLmNvbmZpZy5tYXhEYXRlIDogc2VsZi5ub3c7XG5cblx0XHRzZWxmLmN1cnJlbnRZZWFyID0gaW5pdGlhbERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRzZWxmLmN1cnJlbnRNb250aCA9IGluaXRpYWxEYXRlLmdldE1vbnRoKCk7XG5cblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkgc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBzZWxmLnNlbGVjdGVkRGF0ZXNbMF07XG5cblx0XHRzZWxmLm1pbkRhdGVIYXNUaW1lID0gc2VsZi5jb25maWcubWluRGF0ZSAmJiAoc2VsZi5jb25maWcubWluRGF0ZS5nZXRIb3VycygpIHx8IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TWludXRlcygpIHx8IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0U2Vjb25kcygpKTtcblxuXHRcdHNlbGYubWF4RGF0ZUhhc1RpbWUgPSBzZWxmLmNvbmZpZy5tYXhEYXRlICYmIChzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkgfHwgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNaW51dGVzKCkgfHwgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRTZWNvbmRzKCkpO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwibGF0ZXN0U2VsZWN0ZWREYXRlT2JqXCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gc2VsZi5fc2VsZWN0ZWREYXRlT2JqIHx8IHNlbGYuc2VsZWN0ZWREYXRlc1tzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoIC0gMV0gfHwgbnVsbDtcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChkYXRlKSB7XG5cdFx0XHRcdHNlbGYuX3NlbGVjdGVkRGF0ZU9iaiA9IGRhdGU7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcInNob3dUaW1lSW5wdXRcIiwge1xuXHRcdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0XHRyZXR1cm4gc2VsZi5fc2hvd1RpbWVJbnB1dDtcblx0XHRcdFx0fSxcblx0XHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRcdHNlbGYuX3Nob3dUaW1lSW5wdXQgPSBib29sO1xuXHRcdFx0XHRcdGlmIChzZWxmLmNhbGVuZGFyQ29udGFpbmVyKSB0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcInNob3dUaW1lSW5wdXRcIiwgYm9vbCk7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwSGVscGVyRnVuY3Rpb25zKCkge1xuXHRcdHNlbGYudXRpbHMgPSB7XG5cdFx0XHRkdXJhdGlvbjoge1xuXHRcdFx0XHREQVk6IDg2NDAwMDAwXG5cdFx0XHR9LFxuXHRcdFx0Z2V0RGF5c2luTW9udGg6IGZ1bmN0aW9uIGdldERheXNpbk1vbnRoKG1vbnRoLCB5cikge1xuXHRcdFx0XHRtb250aCA9IHR5cGVvZiBtb250aCA9PT0gXCJ1bmRlZmluZWRcIiA/IHNlbGYuY3VycmVudE1vbnRoIDogbW9udGg7XG5cblx0XHRcdFx0eXIgPSB0eXBlb2YgeXIgPT09IFwidW5kZWZpbmVkXCIgPyBzZWxmLmN1cnJlbnRZZWFyIDogeXI7XG5cblx0XHRcdFx0aWYgKG1vbnRoID09PSAxICYmICh5ciAlIDQgPT09IDAgJiYgeXIgJSAxMDAgIT09IDAgfHwgeXIgJSA0MDAgPT09IDApKSByZXR1cm4gMjk7XG5cblx0XHRcdFx0cmV0dXJuIHNlbGYubDEwbi5kYXlzSW5Nb250aFttb250aF07XG5cdFx0XHR9LFxuXHRcdFx0bW9udGhUb1N0cjogZnVuY3Rpb24gbW9udGhUb1N0cihtb250aE51bWJlciwgc2hvcnRoYW5kKSB7XG5cdFx0XHRcdHNob3J0aGFuZCA9IHR5cGVvZiBzaG9ydGhhbmQgPT09IFwidW5kZWZpbmVkXCIgPyBzZWxmLmNvbmZpZy5zaG9ydGhhbmRDdXJyZW50TW9udGggOiBzaG9ydGhhbmQ7XG5cblx0XHRcdFx0cmV0dXJuIHNlbGYubDEwbi5tb250aHNbKHNob3J0aGFuZCA/IFwic2hvcnRcIiA6IFwibG9uZ1wiKSArIFwiaGFuZFwiXVttb250aE51bWJlcl07XG5cdFx0XHR9XG5cdFx0fTtcblx0fVxuXG5cdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdGZ1bmN0aW9uIHNldHVwRm9ybWF0cygpIHtcblx0XHRbXCJEXCIsIFwiRlwiLCBcIkpcIiwgXCJNXCIsIFwiV1wiLCBcImxcIl0uZm9yRWFjaChmdW5jdGlvbiAoZikge1xuXHRcdFx0c2VsZi5mb3JtYXRzW2ZdID0gRmxhdHBpY2tyLnByb3RvdHlwZS5mb3JtYXRzW2ZdLmJpbmQoc2VsZik7XG5cdFx0fSk7XG5cblx0XHRzZWxmLnJldkZvcm1hdC5GID0gRmxhdHBpY2tyLnByb3RvdHlwZS5yZXZGb3JtYXQuRi5iaW5kKHNlbGYpO1xuXHRcdHNlbGYucmV2Rm9ybWF0Lk0gPSBGbGF0cGlja3IucHJvdG90eXBlLnJldkZvcm1hdC5NLmJpbmQoc2VsZik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cElucHV0cygpIHtcblx0XHRzZWxmLmlucHV0ID0gc2VsZi5jb25maWcud3JhcCA/IHNlbGYuZWxlbWVudC5xdWVyeVNlbGVjdG9yKFwiW2RhdGEtaW5wdXRdXCIpIDogc2VsZi5lbGVtZW50O1xuXG5cdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRpZiAoIXNlbGYuaW5wdXQpIHJldHVybiBjb25zb2xlLndhcm4oXCJFcnJvcjogaW52YWxpZCBpbnB1dCBlbGVtZW50IHNwZWNpZmllZFwiLCBzZWxmLmlucHV0KTtcblxuXHRcdHNlbGYuaW5wdXQuX3R5cGUgPSBzZWxmLmlucHV0LnR5cGU7XG5cdFx0c2VsZi5pbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0c2VsZi5pbnB1dC5jbGFzc0xpc3QuYWRkKFwiZmxhdHBpY2tyLWlucHV0XCIpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsdElucHV0KSB7XG5cdFx0XHQvLyByZXBsaWNhdGUgc2VsZi5lbGVtZW50XG5cdFx0XHRzZWxmLmFsdElucHV0ID0gY3JlYXRlRWxlbWVudChzZWxmLmlucHV0Lm5vZGVOYW1lLCBzZWxmLmlucHV0LmNsYXNzTmFtZSArIFwiIFwiICsgc2VsZi5jb25maWcuYWx0SW5wdXRDbGFzcyk7XG5cdFx0XHRzZWxmLmFsdElucHV0LnBsYWNlaG9sZGVyID0gc2VsZi5pbnB1dC5wbGFjZWhvbGRlcjtcblx0XHRcdHNlbGYuYWx0SW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXHRcdFx0c2VsZi5pbnB1dC50eXBlID0gXCJoaWRkZW5cIjtcblxuXHRcdFx0aWYgKCFzZWxmLmNvbmZpZy5zdGF0aWMgJiYgc2VsZi5pbnB1dC5wYXJlbnROb2RlKSBzZWxmLmlucHV0LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYuYWx0SW5wdXQsIHNlbGYuaW5wdXQubmV4dFNpYmxpbmcpO1xuXHRcdH1cblxuXHRcdGlmICghc2VsZi5jb25maWcuYWxsb3dJbnB1dCkgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuc2V0QXR0cmlidXRlKFwicmVhZG9ubHlcIiwgXCJyZWFkb25seVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwTW9iaWxlKCkge1xuXHRcdHZhciBpbnB1dFR5cGUgPSBzZWxmLmNvbmZpZy5lbmFibGVUaW1lID8gc2VsZi5jb25maWcubm9DYWxlbmRhciA/IFwidGltZVwiIDogXCJkYXRldGltZS1sb2NhbFwiIDogXCJkYXRlXCI7XG5cblx0XHRzZWxmLm1vYmlsZUlucHV0ID0gY3JlYXRlRWxlbWVudChcImlucHV0XCIsIHNlbGYuaW5wdXQuY2xhc3NOYW1lICsgXCIgZmxhdHBpY2tyLW1vYmlsZVwiKTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnN0ZXAgPSBcImFueVwiO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQudGFiSW5kZXggPSAxO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQudHlwZSA9IGlucHV0VHlwZTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LmRpc2FibGVkID0gc2VsZi5pbnB1dC5kaXNhYmxlZDtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnBsYWNlaG9sZGVyID0gc2VsZi5pbnB1dC5wbGFjZWhvbGRlcjtcblxuXHRcdHNlbGYubW9iaWxlRm9ybWF0U3RyID0gaW5wdXRUeXBlID09PSBcImRhdGV0aW1lLWxvY2FsXCIgPyBcIlktbS1kXFxcXFRIOmk6U1wiIDogaW5wdXRUeXBlID09PSBcImRhdGVcIiA/IFwiWS1tLWRcIiA6IFwiSDppOlNcIjtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSB7XG5cdFx0XHRzZWxmLm1vYmlsZUlucHV0LmRlZmF1bHRWYWx1ZSA9IHNlbGYubW9iaWxlSW5wdXQudmFsdWUgPSBmb3JtYXREYXRlKHNlbGYubW9iaWxlRm9ybWF0U3RyLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5taW5EYXRlKSBzZWxmLm1vYmlsZUlucHV0Lm1pbiA9IGZvcm1hdERhdGUoXCJZLW0tZFwiLCBzZWxmLmNvbmZpZy5taW5EYXRlKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlKSBzZWxmLm1vYmlsZUlucHV0Lm1heCA9IGZvcm1hdERhdGUoXCJZLW0tZFwiLCBzZWxmLmNvbmZpZy5tYXhEYXRlKTtcblxuXHRcdHNlbGYuaW5wdXQudHlwZSA9IFwiaGlkZGVuXCI7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsdElucHV0KSBzZWxmLmFsdElucHV0LnR5cGUgPSBcImhpZGRlblwiO1xuXG5cdFx0dHJ5IHtcblx0XHRcdHNlbGYuaW5wdXQucGFyZW50Tm9kZS5pbnNlcnRCZWZvcmUoc2VsZi5tb2JpbGVJbnB1dCwgc2VsZi5pbnB1dC5uZXh0U2libGluZyk7XG5cdFx0fSBjYXRjaCAoZSkge1xuXHRcdFx0Ly9cblx0XHR9XG5cblx0XHRzZWxmLm1vYmlsZUlucHV0LmFkZEV2ZW50TGlzdGVuZXIoXCJjaGFuZ2VcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gc2VsZi5wYXJzZURhdGUoZS50YXJnZXQudmFsdWUpO1xuXHRcdFx0c2VsZi5zZXREYXRlKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNsb3NlXCIpO1xuXHRcdH0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gdG9nZ2xlKCkge1xuXHRcdGlmIChzZWxmLmlzT3Blbikgc2VsZi5jbG9zZSgpO2Vsc2Ugc2VsZi5vcGVuKCk7XG5cdH1cblxuXHRmdW5jdGlvbiB0cmlnZ2VyRXZlbnQoZXZlbnQsIGRhdGEpIHtcblx0XHR2YXIgaG9va3MgPSBzZWxmLmNvbmZpZ1tcIm9uXCIgKyBldmVudF07XG5cblx0XHRpZiAoaG9va3MpIHtcblx0XHRcdGZvciAodmFyIGkgPSAwOyBob29rc1tpXSAmJiBpIDwgaG9va3MubGVuZ3RoOyBpKyspIHtcblx0XHRcdFx0aG9va3NbaV0oc2VsZi5zZWxlY3RlZERhdGVzLCBzZWxmLmlucHV0ICYmIHNlbGYuaW5wdXQudmFsdWUsIHNlbGYsIGRhdGEpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdGlmIChldmVudCA9PT0gXCJDaGFuZ2VcIikge1xuXHRcdFx0aWYgKHR5cGVvZiBFdmVudCA9PT0gXCJmdW5jdGlvblwiICYmIEV2ZW50LmNvbnN0cnVjdG9yKSB7XG5cdFx0XHRcdHNlbGYuaW5wdXQuZGlzcGF0Y2hFdmVudChuZXcgRXZlbnQoXCJjaGFuZ2VcIiwgeyBcImJ1YmJsZXNcIjogdHJ1ZSB9KSk7XG5cblx0XHRcdFx0Ly8gbWFueSBmcm9udC1lbmQgZnJhbWV3b3JrcyBiaW5kIHRvIHRoZSBpbnB1dCBldmVudFxuXHRcdFx0XHRzZWxmLmlucHV0LmRpc3BhdGNoRXZlbnQobmV3IEV2ZW50KFwiaW5wdXRcIiwgeyBcImJ1YmJsZXNcIjogdHJ1ZSB9KSk7XG5cdFx0XHR9XG5cblx0XHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0XHRlbHNlIHtcblx0XHRcdFx0XHRpZiAod2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50ICE9PSB1bmRlZmluZWQpIHJldHVybiBzZWxmLmlucHV0LmRpc3BhdGNoRXZlbnQoc2VsZi5jaGFuZ2VFdmVudCk7XG5cblx0XHRcdFx0XHRzZWxmLmlucHV0LmZpcmVFdmVudChcIm9uY2hhbmdlXCIpO1xuXHRcdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gaXNEYXRlU2VsZWN0ZWQoZGF0ZSkge1xuXHRcdGZvciAodmFyIGkgPSAwOyBpIDwgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRpZiAoY29tcGFyZURhdGVzKHNlbGYuc2VsZWN0ZWREYXRlc1tpXSwgZGF0ZSkgPT09IDApIHJldHVybiBcIlwiICsgaTtcblx0XHR9XG5cblx0XHRyZXR1cm4gZmFsc2U7XG5cdH1cblxuXHRmdW5jdGlvbiBpc0RhdGVJblJhbmdlKGRhdGUpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSAhPT0gXCJyYW5nZVwiIHx8IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPCAyKSByZXR1cm4gZmFsc2U7XG5cdFx0cmV0dXJuIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pID49IDAgJiYgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1sxXSkgPD0gMDtcblx0fVxuXG5cdGZ1bmN0aW9uIHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgfHwgc2VsZi5pc01vYmlsZSB8fCAhc2VsZi5tb250aE5hdikgcmV0dXJuO1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LnRleHRDb250ZW50ID0gc2VsZi51dGlscy5tb250aFRvU3RyKHNlbGYuY3VycmVudE1vbnRoKSArIFwiIFwiO1xuXHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LnZhbHVlID0gc2VsZi5jdXJyZW50WWVhcjtcblxuXHRcdHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyA9IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgKHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKSA/IHNlbGYuY3VycmVudE1vbnRoIDw9IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TW9udGgoKSA6IHNlbGYuY3VycmVudFllYXIgPCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkpO1xuXG5cdFx0c2VsZi5faGlkZU5leHRNb250aEFycm93ID0gc2VsZi5jb25maWcubWF4RGF0ZSAmJiAoc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpID8gc2VsZi5jdXJyZW50TW9udGggKyAxID4gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNb250aCgpIDogc2VsZi5jdXJyZW50WWVhciA+IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSk7XG5cdH1cblxuXHRmdW5jdGlvbiB1cGRhdGVWYWx1ZSgpIHtcblx0XHRpZiAoIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHJldHVybiBzZWxmLmNsZWFyKCk7XG5cblx0XHRpZiAoc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0c2VsZi5tb2JpbGVJbnB1dC52YWx1ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPyBmb3JtYXREYXRlKHNlbGYubW9iaWxlRm9ybWF0U3RyLCBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaikgOiBcIlwiO1xuXHRcdH1cblxuXHRcdHZhciBqb2luQ2hhciA9IHNlbGYuY29uZmlnLm1vZGUgIT09IFwicmFuZ2VcIiA/IFwiOyBcIiA6IHNlbGYubDEwbi5yYW5nZVNlcGFyYXRvcjtcblxuXHRcdHNlbGYuaW5wdXQudmFsdWUgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubWFwKGZ1bmN0aW9uIChkT2JqKSB7XG5cdFx0XHRyZXR1cm4gZm9ybWF0RGF0ZShzZWxmLmNvbmZpZy5kYXRlRm9ybWF0LCBkT2JqKTtcblx0XHR9KS5qb2luKGpvaW5DaGFyKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5hbHRJbnB1dCkge1xuXHRcdFx0c2VsZi5hbHRJbnB1dC52YWx1ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5tYXAoZnVuY3Rpb24gKGRPYmopIHtcblx0XHRcdFx0cmV0dXJuIGZvcm1hdERhdGUoc2VsZi5jb25maWcuYWx0Rm9ybWF0LCBkT2JqKTtcblx0XHRcdH0pLmpvaW4oam9pbkNoYXIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIlZhbHVlVXBkYXRlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24geWVhclNjcm9sbChlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG5cdFx0dmFyIGRlbHRhID0gTWF0aC5tYXgoLTEsIE1hdGgubWluKDEsIGUud2hlZWxEZWx0YSB8fCAtZS5kZWx0YVkpKSxcblx0XHQgICAgbmV3WWVhciA9IHBhcnNlSW50KGUudGFyZ2V0LnZhbHVlLCAxMCkgKyBkZWx0YTtcblxuXHRcdGNoYW5nZVllYXIobmV3WWVhcik7XG5cdFx0ZS50YXJnZXQudmFsdWUgPSBzZWxmLmN1cnJlbnRZZWFyO1xuXHR9XG5cblx0ZnVuY3Rpb24gY3JlYXRlRWxlbWVudCh0YWcsIGNsYXNzTmFtZSwgY29udGVudCkge1xuXHRcdHZhciBlID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZUVsZW1lbnQodGFnKTtcblx0XHRjbGFzc05hbWUgPSBjbGFzc05hbWUgfHwgXCJcIjtcblx0XHRjb250ZW50ID0gY29udGVudCB8fCBcIlwiO1xuXG5cdFx0ZS5jbGFzc05hbWUgPSBjbGFzc05hbWU7XG5cblx0XHRpZiAoY29udGVudCkgZS50ZXh0Q29udGVudCA9IGNvbnRlbnQ7XG5cblx0XHRyZXR1cm4gZTtcblx0fVxuXG5cdGZ1bmN0aW9uIGFycmF5aWZ5KG9iaikge1xuXHRcdGlmIChBcnJheS5pc0FycmF5KG9iaikpIHJldHVybiBvYmo7XG5cdFx0cmV0dXJuIFtvYmpdO1xuXHR9XG5cblx0ZnVuY3Rpb24gdG9nZ2xlQ2xhc3MoZWxlbSwgY2xhc3NOYW1lLCBib29sKSB7XG5cdFx0aWYgKGJvb2wpIHJldHVybiBlbGVtLmNsYXNzTGlzdC5hZGQoY2xhc3NOYW1lKTtcblx0XHRlbGVtLmNsYXNzTGlzdC5yZW1vdmUoY2xhc3NOYW1lKTtcblx0fVxuXG5cdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdGZ1bmN0aW9uIGRlYm91bmNlKGZ1bmMsIHdhaXQsIGltbWVkaWF0ZSkge1xuXHRcdHZhciB0aW1lb3V0ID0gdm9pZCAwO1xuXHRcdHJldHVybiBmdW5jdGlvbiAoKSB7XG5cdFx0XHR2YXIgY29udGV4dCA9IHRoaXMsXG5cdFx0XHQgICAgYXJncyA9IGFyZ3VtZW50cztcblx0XHRcdGNsZWFyVGltZW91dCh0aW1lb3V0KTtcblx0XHRcdHRpbWVvdXQgPSBzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0dGltZW91dCA9IG51bGw7XG5cdFx0XHRcdGlmICghaW1tZWRpYXRlKSBmdW5jLmFwcGx5KGNvbnRleHQsIGFyZ3MpO1xuXHRcdFx0fSwgd2FpdCk7XG5cdFx0XHRpZiAoaW1tZWRpYXRlICYmICF0aW1lb3V0KSBmdW5jLmFwcGx5KGNvbnRleHQsIGFyZ3MpO1xuXHRcdH07XG5cdH1cblxuXHRmdW5jdGlvbiBjb21wYXJlRGF0ZXMoZGF0ZTEsIGRhdGUyLCB0aW1lbGVzcykge1xuXHRcdGlmICghKGRhdGUxIGluc3RhbmNlb2YgRGF0ZSkgfHwgIShkYXRlMiBpbnN0YW5jZW9mIERhdGUpKSByZXR1cm4gZmFsc2U7XG5cblx0XHRpZiAodGltZWxlc3MgIT09IGZhbHNlKSB7XG5cdFx0XHRyZXR1cm4gbmV3IERhdGUoZGF0ZTEuZ2V0VGltZSgpKS5zZXRIb3VycygwLCAwLCAwLCAwKSAtIG5ldyBEYXRlKGRhdGUyLmdldFRpbWUoKSkuc2V0SG91cnMoMCwgMCwgMCwgMCk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIGRhdGUxLmdldFRpbWUoKSAtIGRhdGUyLmdldFRpbWUoKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHRpbWVXcmFwcGVyKGUpIHtcblx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cblx0XHR2YXIgaXNLZXlEb3duID0gZS50eXBlID09PSBcImtleWRvd25cIixcblx0XHQgICAgaXNXaGVlbCA9IGUudHlwZSA9PT0gXCJ3aGVlbFwiLFxuXHRcdCAgICBpc0luY3JlbWVudCA9IGUudHlwZSA9PT0gXCJpbmNyZW1lbnRcIixcblx0XHQgICAgaW5wdXQgPSBlLnRhcmdldDtcblxuXHRcdGlmIChlLnR5cGUgIT09IFwiaW5wdXRcIiAmJiAhaXNLZXlEb3duICYmIChlLnRhcmdldC52YWx1ZSB8fCBlLnRhcmdldC50ZXh0Q29udGVudCkubGVuZ3RoID49IDIgLy8gdHlwZWQgdHdvIGRpZ2l0c1xuXHRcdCkge1xuXHRcdFx0XHRlLnRhcmdldC5mb2N1cygpO1xuXHRcdFx0XHRlLnRhcmdldC5ibHVyKCk7XG5cdFx0XHR9XG5cblx0XHRpZiAoc2VsZi5hbVBNICYmIGUudGFyZ2V0ID09PSBzZWxmLmFtUE0pIHJldHVybiBlLnRhcmdldC50ZXh0Q29udGVudCA9IFtcIkFNXCIsIFwiUE1cIl1bZS50YXJnZXQudGV4dENvbnRlbnQgPT09IFwiQU1cIiB8IDBdO1xuXG5cdFx0dmFyIG1pbiA9IE51bWJlcihpbnB1dC5taW4pLFxuXHRcdCAgICBtYXggPSBOdW1iZXIoaW5wdXQubWF4KSxcblx0XHQgICAgc3RlcCA9IE51bWJlcihpbnB1dC5zdGVwKSxcblx0XHQgICAgY3VyVmFsdWUgPSBwYXJzZUludChpbnB1dC52YWx1ZSwgMTApLFxuXHRcdCAgICBkZWx0YSA9IGUuZGVsdGEgfHwgKCFpc0tleURvd24gPyBNYXRoLm1heCgtMSwgTWF0aC5taW4oMSwgZS53aGVlbERlbHRhIHx8IC1lLmRlbHRhWSkpIHx8IDAgOiBlLndoaWNoID09PSAzOCA/IDEgOiAtMSk7XG5cblx0XHR2YXIgbmV3VmFsdWUgPSBjdXJWYWx1ZSArIHN0ZXAgKiBkZWx0YTtcblxuXHRcdGlmICh0eXBlb2YgaW5wdXQudmFsdWUgIT09IFwidW5kZWZpbmVkXCIgJiYgaW5wdXQudmFsdWUubGVuZ3RoID09PSAyKSB7XG5cdFx0XHR2YXIgaXNIb3VyRWxlbSA9IGlucHV0ID09PSBzZWxmLmhvdXJFbGVtZW50LFxuXHRcdFx0ICAgIGlzTWludXRlRWxlbSA9IGlucHV0ID09PSBzZWxmLm1pbnV0ZUVsZW1lbnQ7XG5cblx0XHRcdGlmIChuZXdWYWx1ZSA8IG1pbikge1xuXHRcdFx0XHRuZXdWYWx1ZSA9IG1heCArIG5ld1ZhbHVlICsgIWlzSG91ckVsZW0gKyAoaXNIb3VyRWxlbSAmJiAhc2VsZi5hbVBNKTtcblxuXHRcdFx0XHRpZiAoaXNNaW51dGVFbGVtKSBpbmNyZW1lbnROdW1JbnB1dChudWxsLCAtMSwgc2VsZi5ob3VyRWxlbWVudCk7XG5cdFx0XHR9IGVsc2UgaWYgKG5ld1ZhbHVlID4gbWF4KSB7XG5cdFx0XHRcdG5ld1ZhbHVlID0gaW5wdXQgPT09IHNlbGYuaG91ckVsZW1lbnQgPyBuZXdWYWx1ZSAtIG1heCAtICFzZWxmLmFtUE0gOiBtaW47XG5cblx0XHRcdFx0aWYgKGlzTWludXRlRWxlbSkgaW5jcmVtZW50TnVtSW5wdXQobnVsbCwgMSwgc2VsZi5ob3VyRWxlbWVudCk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLmFtUE0gJiYgaXNIb3VyRWxlbSAmJiAoc3RlcCA9PT0gMSA/IG5ld1ZhbHVlICsgY3VyVmFsdWUgPT09IDIzIDogTWF0aC5hYnMobmV3VmFsdWUgLSBjdXJWYWx1ZSkgPiBzdGVwKSkgc2VsZi5hbVBNLnRleHRDb250ZW50ID0gc2VsZi5hbVBNLnRleHRDb250ZW50ID09PSBcIlBNXCIgPyBcIkFNXCIgOiBcIlBNXCI7XG5cblx0XHRcdGlucHV0LnZhbHVlID0gc2VsZi5wYWQobmV3VmFsdWUpO1xuXHRcdH1cblx0fVxuXG5cdGluaXQoKTtcblx0cmV0dXJuIHNlbGY7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5GbGF0cGlja3IuZGVmYXVsdENvbmZpZyA9IHtcblxuXHRtb2RlOiBcInNpbmdsZVwiLFxuXG5cdHBvc2l0aW9uOiBcInRvcFwiLFxuXG5cdC8qIGlmIHRydWUsIGRhdGVzIHdpbGwgYmUgcGFyc2VkLCBmb3JtYXR0ZWQsIGFuZCBkaXNwbGF5ZWQgaW4gVVRDLlxuIHByZWxvYWRpbmcgZGF0ZSBzdHJpbmdzIHcvIHRpbWV6b25lcyBpcyByZWNvbW1lbmRlZCBidXQgbm90IG5lY2Vzc2FyeSAqL1xuXHR1dGM6IGZhbHNlLFxuXG5cdC8vIHdyYXA6IHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI3N0cmFwXG5cdHdyYXA6IGZhbHNlLFxuXG5cdC8vIGVuYWJsZXMgd2VlayBudW1iZXJzXG5cdHdlZWtOdW1iZXJzOiBmYWxzZSxcblxuXHQvLyBhbGxvdyBtYW51YWwgZGF0ZXRpbWUgaW5wdXRcblx0YWxsb3dJbnB1dDogZmFsc2UsXG5cblx0LypcbiBcdGNsaWNraW5nIG9uIGlucHV0IG9wZW5zIHRoZSBkYXRlKHRpbWUpcGlja2VyLlxuIFx0ZGlzYWJsZSBpZiB5b3Ugd2lzaCB0byBvcGVuIHRoZSBjYWxlbmRhciBtYW51YWxseSB3aXRoIC5vcGVuKClcbiAqL1xuXHRjbGlja09wZW5zOiB0cnVlLFxuXG5cdC8vIGRpc3BsYXkgdGltZSBwaWNrZXIgaW4gMjQgaG91ciBtb2RlXG5cdHRpbWVfMjRocjogZmFsc2UsXG5cblx0Ly8gZW5hYmxlcyB0aGUgdGltZSBwaWNrZXIgZnVuY3Rpb25hbGl0eVxuXHRlbmFibGVUaW1lOiBmYWxzZSxcblxuXHQvLyBub0NhbGVuZGFyOiB0cnVlIHdpbGwgaGlkZSB0aGUgY2FsZW5kYXIuIHVzZSBmb3IgYSB0aW1lIHBpY2tlciBhbG9uZyB3LyBlbmFibGVUaW1lXG5cdG5vQ2FsZW5kYXI6IGZhbHNlLFxuXG5cdC8vIG1vcmUgZGF0ZSBmb3JtYXQgY2hhcnMgYXQgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNkYXRlZm9ybWF0XG5cdGRhdGVGb3JtYXQ6IFwiWS1tLWRcIixcblxuXHQvLyBhbHRJbnB1dCAtIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2FsdGlucHV0XG5cdGFsdElucHV0OiBmYWxzZSxcblxuXHQvLyB0aGUgY3JlYXRlZCBhbHRJbnB1dCBlbGVtZW50IHdpbGwgaGF2ZSB0aGlzIGNsYXNzLlxuXHRhbHRJbnB1dENsYXNzOiBcImZsYXRwaWNrci1pbnB1dCBmb3JtLWNvbnRyb2wgaW5wdXRcIixcblxuXHQvLyBzYW1lIGFzIGRhdGVGb3JtYXQsIGJ1dCBmb3IgYWx0SW5wdXRcblx0YWx0Rm9ybWF0OiBcIkYgaiwgWVwiLCAvLyBkZWZhdWx0cyB0byBlLmcuIEp1bmUgMTAsIDIwMTZcblxuXHQvLyBkZWZhdWx0RGF0ZSAtIGVpdGhlciBhIGRhdGVzdHJpbmcgb3IgYSBkYXRlIG9iamVjdC4gdXNlZCBmb3IgZGF0ZXRpbWVwaWNrZXJcInMgaW5pdGlhbCB2YWx1ZVxuXHRkZWZhdWx0RGF0ZTogbnVsbCxcblxuXHQvLyB0aGUgbWluaW11bSBkYXRlIHRoYXQgdXNlciBjYW4gcGljayAoaW5jbHVzaXZlKVxuXHRtaW5EYXRlOiBudWxsLFxuXG5cdC8vIHRoZSBtYXhpbXVtIGRhdGUgdGhhdCB1c2VyIGNhbiBwaWNrIChpbmNsdXNpdmUpXG5cdG1heERhdGU6IG51bGwsXG5cblx0Ly8gZGF0ZXBhcnNlciB0aGF0IHRyYW5zZm9ybXMgYSBnaXZlbiBzdHJpbmcgdG8gYSBkYXRlIG9iamVjdFxuXHRwYXJzZURhdGU6IG51bGwsXG5cblx0Ly8gZGF0ZWZvcm1hdHRlciB0aGF0IHRyYW5zZm9ybXMgYSBnaXZlbiBkYXRlIG9iamVjdCB0byBhIHN0cmluZywgYWNjb3JkaW5nIHRvIHBhc3NlZCBmb3JtYXRcblx0Zm9ybWF0RGF0ZTogbnVsbCxcblxuXHRnZXRXZWVrOiBmdW5jdGlvbiBnZXRXZWVrKGdpdmVuRGF0ZSkge1xuXHRcdHZhciBkYXRlID0gbmV3IERhdGUoZ2l2ZW5EYXRlLmdldFRpbWUoKSk7XG5cdFx0ZGF0ZS5zZXRIb3VycygwLCAwLCAwLCAwKTtcblxuXHRcdC8vIFRodXJzZGF5IGluIGN1cnJlbnQgd2VlayBkZWNpZGVzIHRoZSB5ZWFyLlxuXHRcdGRhdGUuc2V0RGF0ZShkYXRlLmdldERhdGUoKSArIDMgLSAoZGF0ZS5nZXREYXkoKSArIDYpICUgNyk7XG5cdFx0Ly8gSmFudWFyeSA0IGlzIGFsd2F5cyBpbiB3ZWVrIDEuXG5cdFx0dmFyIHdlZWsxID0gbmV3IERhdGUoZGF0ZS5nZXRGdWxsWWVhcigpLCAwLCA0KTtcblx0XHQvLyBBZGp1c3QgdG8gVGh1cnNkYXkgaW4gd2VlayAxIGFuZCBjb3VudCBudW1iZXIgb2Ygd2Vla3MgZnJvbSBkYXRlIHRvIHdlZWsxLlxuXHRcdHJldHVybiAxICsgTWF0aC5yb3VuZCgoKGRhdGUuZ2V0VGltZSgpIC0gd2VlazEuZ2V0VGltZSgpKSAvIDg2NDAwMDAwIC0gMyArICh3ZWVrMS5nZXREYXkoKSArIDYpICUgNykgLyA3KTtcblx0fSxcblxuXHQvLyBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNkaXNhYmxlXG5cdGVuYWJsZTogW10sXG5cblx0Ly8gc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jZGlzYWJsZVxuXHRkaXNhYmxlOiBbXSxcblxuXHQvLyBkaXNwbGF5IHRoZSBzaG9ydCB2ZXJzaW9uIG9mIG1vbnRoIG5hbWVzIC0gZS5nLiBTZXAgaW5zdGVhZCBvZiBTZXB0ZW1iZXJcblx0c2hvcnRoYW5kQ3VycmVudE1vbnRoOiBmYWxzZSxcblxuXHQvLyBkaXNwbGF5cyBjYWxlbmRhciBpbmxpbmUuIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2lubGluZS1jYWxlbmRhclxuXHRpbmxpbmU6IGZhbHNlLFxuXG5cdC8vIHBvc2l0aW9uIGNhbGVuZGFyIGluc2lkZSB3cmFwcGVyIGFuZCBuZXh0IHRvIHRoZSBpbnB1dCBlbGVtZW50XG5cdC8vIGxlYXZlIGF0IGZhbHNlIHVubGVzcyB5b3Uga25vdyB3aGF0IHlvdVwicmUgZG9pbmdcblx0c3RhdGljOiBmYWxzZSxcblxuXHQvLyBET00gbm9kZSB0byBhcHBlbmQgdGhlIGNhbGVuZGFyIHRvIGluICpzdGF0aWMqIG1vZGVcblx0YXBwZW5kVG86IG51bGwsXG5cblx0Ly8gY29kZSBmb3IgcHJldmlvdXMvbmV4dCBpY29ucy4gdGhpcyBpcyB3aGVyZSB5b3UgcHV0IHlvdXIgY3VzdG9tIGljb24gY29kZSBlLmcuIGZvbnRhd2Vzb21lXG5cdHByZXZBcnJvdzogXCI8c3ZnIHZlcnNpb249JzEuMScgeG1sbnM9J2h0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnJyB4bWxuczp4bGluaz0naHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluaycgdmlld0JveD0nMCAwIDE3IDE3Jz48Zz48L2c+PHBhdGggZD0nTTUuMjA3IDguNDcxbDcuMTQ2IDcuMTQ3LTAuNzA3IDAuNzA3LTcuODUzLTcuODU0IDcuODU0LTcuODUzIDAuNzA3IDAuNzA3LTcuMTQ3IDcuMTQ2eicgLz48L3N2Zz5cIixcblx0bmV4dEFycm93OiBcIjxzdmcgdmVyc2lvbj0nMS4xJyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHhtbG5zOnhsaW5rPSdodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rJyB2aWV3Qm94PScwIDAgMTcgMTcnPjxnPjwvZz48cGF0aCBkPSdNMTMuMjA3IDguNDcybC03Ljg1NCA3Ljg1NC0wLjcwNy0wLjcwNyA3LjE0Ni03LjE0Ni03LjE0Ni03LjE0OCAwLjcwNy0wLjcwNyA3Ljg1NCA3Ljg1NHonIC8+PC9zdmc+XCIsXG5cblx0Ly8gZW5hYmxlcyBzZWNvbmRzIGluIHRoZSB0aW1lIHBpY2tlclxuXHRlbmFibGVTZWNvbmRzOiBmYWxzZSxcblxuXHQvLyBzdGVwIHNpemUgdXNlZCB3aGVuIHNjcm9sbGluZy9pbmNyZW1lbnRpbmcgdGhlIGhvdXIgZWxlbWVudFxuXHRob3VySW5jcmVtZW50OiAxLFxuXG5cdC8vIHN0ZXAgc2l6ZSB1c2VkIHdoZW4gc2Nyb2xsaW5nL2luY3JlbWVudGluZyB0aGUgbWludXRlIGVsZW1lbnRcblx0bWludXRlSW5jcmVtZW50OiA1LFxuXG5cdC8vIGluaXRpYWwgdmFsdWUgaW4gdGhlIGhvdXIgZWxlbWVudFxuXHRkZWZhdWx0SG91cjogMTIsXG5cblx0Ly8gaW5pdGlhbCB2YWx1ZSBpbiB0aGUgbWludXRlIGVsZW1lbnRcblx0ZGVmYXVsdE1pbnV0ZTogMCxcblxuXHQvLyBkaXNhYmxlIG5hdGl2ZSBtb2JpbGUgZGF0ZXRpbWUgaW5wdXQgc3VwcG9ydFxuXHRkaXNhYmxlTW9iaWxlOiBmYWxzZSxcblxuXHQvLyBkZWZhdWx0IGxvY2FsZVxuXHRsb2NhbGU6IFwiZGVmYXVsdFwiLFxuXG5cdHBsdWdpbnM6IFtdLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIGNhbGVuZGFyIGlzIGNsb3NlZFxuXHRvbkNsb3NlOiBbXSwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gb25DaGFuZ2UgY2FsbGJhY2sgd2hlbiB1c2VyIHNlbGVjdHMgYSBkYXRlIG9yIHRpbWVcblx0b25DaGFuZ2U6IFtdLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBjYWxsZWQgZm9yIGV2ZXJ5IGRheSBlbGVtZW50XG5cdG9uRGF5Q3JlYXRlOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSB0aGUgbW9udGggaXMgY2hhbmdlZFxuXHRvbk1vbnRoQ2hhbmdlOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSBjYWxlbmRhciBpcyBvcGVuZWRcblx0b25PcGVuOiBbXSwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gY2FsbGVkIGFmdGVyIHRoZSBjb25maWd1cmF0aW9uIGhhcyBiZWVuIHBhcnNlZFxuXHRvblBhcnNlQ29uZmlnOiBbXSxcblxuXHQvLyBjYWxsZWQgYWZ0ZXIgY2FsZW5kYXIgaXMgcmVhZHlcblx0b25SZWFkeTogW10sIC8vIGZ1bmN0aW9uIChkYXRlT2JqLCBkYXRlU3RyKSB7fVxuXG5cdC8vIGNhbGxlZCBhZnRlciBpbnB1dCB2YWx1ZSB1cGRhdGVkXG5cdG9uVmFsdWVVcGRhdGU6IFtdLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIHRoZSB5ZWFyIGlzIGNoYW5nZWRcblx0b25ZZWFyQ2hhbmdlOiBbXSxcblxuXHRvbktleURvd246IFtdXG59O1xuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuRmxhdHBpY2tyLmwxMG5zID0ge1xuXHRlbjoge1xuXHRcdHdlZWtkYXlzOiB7XG5cdFx0XHRzaG9ydGhhbmQ6IFtcIlN1blwiLCBcIk1vblwiLCBcIlR1ZVwiLCBcIldlZFwiLCBcIlRodVwiLCBcIkZyaVwiLCBcIlNhdFwiXSxcblx0XHRcdGxvbmdoYW5kOiBbXCJTdW5kYXlcIiwgXCJNb25kYXlcIiwgXCJUdWVzZGF5XCIsIFwiV2VkbmVzZGF5XCIsIFwiVGh1cnNkYXlcIiwgXCJGcmlkYXlcIiwgXCJTYXR1cmRheVwiXVxuXHRcdH0sXG5cdFx0bW9udGhzOiB7XG5cdFx0XHRzaG9ydGhhbmQ6IFtcIkphblwiLCBcIkZlYlwiLCBcIk1hclwiLCBcIkFwclwiLCBcIk1heVwiLCBcIkp1blwiLCBcIkp1bFwiLCBcIkF1Z1wiLCBcIlNlcFwiLCBcIk9jdFwiLCBcIk5vdlwiLCBcIkRlY1wiXSxcblx0XHRcdGxvbmdoYW5kOiBbXCJKYW51YXJ5XCIsIFwiRmVicnVhcnlcIiwgXCJNYXJjaFwiLCBcIkFwcmlsXCIsIFwiTWF5XCIsIFwiSnVuZVwiLCBcIkp1bHlcIiwgXCJBdWd1c3RcIiwgXCJTZXB0ZW1iZXJcIiwgXCJPY3RvYmVyXCIsIFwiTm92ZW1iZXJcIiwgXCJEZWNlbWJlclwiXVxuXHRcdH0sXG5cdFx0ZGF5c0luTW9udGg6IFszMSwgMjgsIDMxLCAzMCwgMzEsIDMwLCAzMSwgMzEsIDMwLCAzMSwgMzAsIDMxXSxcblx0XHRmaXJzdERheU9mV2VlazogMCxcblx0XHRvcmRpbmFsOiBmdW5jdGlvbiBvcmRpbmFsKG50aCkge1xuXHRcdFx0dmFyIHMgPSBudGggJSAxMDA7XG5cdFx0XHRpZiAocyA+IDMgJiYgcyA8IDIxKSByZXR1cm4gXCJ0aFwiO1xuXHRcdFx0c3dpdGNoIChzICUgMTApIHtcblx0XHRcdFx0Y2FzZSAxOlxuXHRcdFx0XHRcdHJldHVybiBcInN0XCI7XG5cdFx0XHRcdGNhc2UgMjpcblx0XHRcdFx0XHRyZXR1cm4gXCJuZFwiO1xuXHRcdFx0XHRjYXNlIDM6XG5cdFx0XHRcdFx0cmV0dXJuIFwicmRcIjtcblx0XHRcdFx0ZGVmYXVsdDpcblx0XHRcdFx0XHRyZXR1cm4gXCJ0aFwiO1xuXHRcdFx0fVxuXHRcdH0sXG5cdFx0cmFuZ2VTZXBhcmF0b3I6IFwiIHRvIFwiLFxuXHRcdHdlZWtBYmJyZXZpYXRpb246IFwiV2tcIixcblx0XHRzY3JvbGxUaXRsZTogXCJTY3JvbGwgdG8gaW5jcmVtZW50XCIsXG5cdFx0dG9nZ2xlVGl0bGU6IFwiQ2xpY2sgdG8gdG9nZ2xlXCJcblx0fVxufTtcblxuRmxhdHBpY2tyLmwxMG5zLmRlZmF1bHQgPSBPYmplY3QuY3JlYXRlKEZsYXRwaWNrci5sMTBucy5lbik7XG5GbGF0cGlja3IubG9jYWxpemUgPSBmdW5jdGlvbiAobDEwbikge1xuXHRyZXR1cm4gX2V4dGVuZHMoRmxhdHBpY2tyLmwxMG5zLmRlZmF1bHQsIGwxMG4gfHwge30pO1xufTtcbkZsYXRwaWNrci5zZXREZWZhdWx0cyA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0cmV0dXJuIF9leHRlbmRzKEZsYXRwaWNrci5kZWZhdWx0Q29uZmlnLCBjb25maWcgfHwge30pO1xufTtcblxuRmxhdHBpY2tyLnByb3RvdHlwZSA9IHtcblx0Zm9ybWF0czoge1xuXHRcdC8vIGdldCB0aGUgZGF0ZSBpbiBVVENcblx0XHRaOiBmdW5jdGlvbiBaKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLnRvSVNPU3RyaW5nKCk7XG5cdFx0fSxcblxuXHRcdC8vIHdlZWtkYXkgbmFtZSwgc2hvcnQsIGUuZy4gVGh1XG5cdFx0RDogZnVuY3Rpb24gRChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5sMTBuLndlZWtkYXlzLnNob3J0aGFuZFt0aGlzLmZvcm1hdHMudyhkYXRlKV07XG5cdFx0fSxcblxuXHRcdC8vIGZ1bGwgbW9udGggbmFtZSBlLmcuIEphbnVhcnlcblx0XHRGOiBmdW5jdGlvbiBGKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLnV0aWxzLm1vbnRoVG9TdHIodGhpcy5mb3JtYXRzLm4oZGF0ZSkgLSAxLCBmYWxzZSk7XG5cdFx0fSxcblxuXHRcdC8vIGhvdXJzIHdpdGggbGVhZGluZyB6ZXJvIGUuZy4gMDNcblx0XHRIOiBmdW5jdGlvbiBIKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldEhvdXJzKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBkYXkgKDEtMzApIHdpdGggb3JkaW5hbCBzdWZmaXggZS5nLiAxc3QsIDJuZFxuXHRcdEo6IGZ1bmN0aW9uIEooZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0RGF0ZSgpICsgdGhpcy5sMTBuLm9yZGluYWwoZGF0ZS5nZXREYXRlKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBBTS9QTVxuXHRcdEs6IGZ1bmN0aW9uIEsoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0SG91cnMoKSA+IDExID8gXCJQTVwiIDogXCJBTVwiO1xuXHRcdH0sXG5cblx0XHQvLyBzaG9ydGhhbmQgbW9udGggZS5nLiBKYW4sIFNlcCwgT2N0LCBldGNcblx0XHRNOiBmdW5jdGlvbiBNKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLnV0aWxzLm1vbnRoVG9TdHIoZGF0ZS5nZXRNb250aCgpLCB0cnVlKTtcblx0XHR9LFxuXG5cdFx0Ly8gc2Vjb25kcyAwMC01OVxuXHRcdFM6IGZ1bmN0aW9uIFMoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0U2Vjb25kcygpKTtcblx0XHR9LFxuXG5cdFx0Ly8gdW5peCB0aW1lc3RhbXBcblx0XHRVOiBmdW5jdGlvbiBVKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldFRpbWUoKSAvIDEwMDA7XG5cdFx0fSxcblxuXHRcdFc6IGZ1bmN0aW9uIFcoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMuY29uZmlnLmdldFdlZWsoZGF0ZSk7XG5cdFx0fSxcblxuXHRcdC8vIGZ1bGwgeWVhciBlLmcuIDIwMTZcblx0XHRZOiBmdW5jdGlvbiBZKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0fSxcblxuXHRcdC8vIGRheSBpbiBtb250aCwgcGFkZGVkICgwMS0zMClcblx0XHRkOiBmdW5jdGlvbiBkKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldERhdGUoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGhvdXIgZnJvbSAxLTEyIChhbS9wbSlcblx0XHRoOiBmdW5jdGlvbiBoKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldEhvdXJzKCkgJSAxMiA/IGRhdGUuZ2V0SG91cnMoKSAlIDEyIDogMTI7XG5cdFx0fSxcblxuXHRcdC8vIG1pbnV0ZXMsIHBhZGRlZCB3aXRoIGxlYWRpbmcgemVybyBlLmcuIDA5XG5cdFx0aTogZnVuY3Rpb24gaShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRNaW51dGVzKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBkYXkgaW4gbW9udGggKDEtMzApXG5cdFx0ajogZnVuY3Rpb24gaihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXREYXRlKCk7XG5cdFx0fSxcblxuXHRcdC8vIHdlZWtkYXkgbmFtZSwgZnVsbCwgZS5nLiBUaHVyc2RheVxuXHRcdGw6IGZ1bmN0aW9uIGwoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMubDEwbi53ZWVrZGF5cy5sb25naGFuZFtkYXRlLmdldERheSgpXTtcblx0XHR9LFxuXG5cdFx0Ly8gcGFkZGVkIG1vbnRoIG51bWJlciAoMDEtMTIpXG5cdFx0bTogZnVuY3Rpb24gbShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRNb250aCgpICsgMSk7XG5cdFx0fSxcblxuXHRcdC8vIHRoZSBtb250aCBudW1iZXIgKDEtMTIpXG5cdFx0bjogZnVuY3Rpb24gbihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRNb250aCgpICsgMTtcblx0XHR9LFxuXG5cdFx0Ly8gc2Vjb25kcyAwLTU5XG5cdFx0czogZnVuY3Rpb24gcyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRTZWNvbmRzKCk7XG5cdFx0fSxcblxuXHRcdC8vIG51bWJlciBvZiB0aGUgZGF5IG9mIHRoZSB3ZWVrXG5cdFx0dzogZnVuY3Rpb24gdyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXREYXkoKTtcblx0XHR9LFxuXG5cdFx0Ly8gbGFzdCB0d28gZGlnaXRzIG9mIHllYXIgZS5nLiAxNiBmb3IgMjAxNlxuXHRcdHk6IGZ1bmN0aW9uIHkoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIFN0cmluZyhkYXRlLmdldEZ1bGxZZWFyKCkpLnN1YnN0cmluZygyKTtcblx0XHR9XG5cdH0sXG5cblx0cmV2Rm9ybWF0OiB7XG5cdFx0RDogZnVuY3Rpb24gRCgpIHt9LFxuXHRcdEY6IGZ1bmN0aW9uIEYoZGF0ZU9iaiwgbW9udGhOYW1lKSB7XG5cdFx0XHRkYXRlT2JqLnNldE1vbnRoKHRoaXMubDEwbi5tb250aHMubG9uZ2hhbmQuaW5kZXhPZihtb250aE5hbWUpKTtcblx0XHR9LFxuXHRcdEg6IGZ1bmN0aW9uIEgoZGF0ZU9iaiwgaG91cikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0SG91cnMocGFyc2VGbG9hdChob3VyKSk7XG5cdFx0fSxcblx0XHRKOiBmdW5jdGlvbiBKKGRhdGVPYmosIGRheSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0SzogZnVuY3Rpb24gSyhkYXRlT2JqLCBhbVBNKSB7XG5cdFx0XHR2YXIgaG91cnMgPSBkYXRlT2JqLmdldEhvdXJzKCk7XG5cblx0XHRcdGlmIChob3VycyAhPT0gMTIpIGRhdGVPYmouc2V0SG91cnMoaG91cnMgJSAxMiArIDEyICogL3BtL2kudGVzdChhbVBNKSk7XG5cdFx0fSxcblx0XHRNOiBmdW5jdGlvbiBNKGRhdGVPYmosIHNob3J0TW9udGgpIHtcblx0XHRcdGRhdGVPYmouc2V0TW9udGgodGhpcy5sMTBuLm1vbnRocy5zaG9ydGhhbmQuaW5kZXhPZihzaG9ydE1vbnRoKSk7XG5cdFx0fSxcblx0XHRTOiBmdW5jdGlvbiBTKGRhdGVPYmosIHNlY29uZHMpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldFNlY29uZHMoc2Vjb25kcyk7XG5cdFx0fSxcblx0XHRXOiBmdW5jdGlvbiBXKCkge30sXG5cdFx0WTogZnVuY3Rpb24gWShkYXRlT2JqLCB5ZWFyKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRGdWxsWWVhcih5ZWFyKTtcblx0XHR9LFxuXHRcdFo6IGZ1bmN0aW9uIFooZGF0ZU9iaiwgSVNPRGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmogPSBuZXcgRGF0ZShJU09EYXRlKTtcblx0XHR9LFxuXG5cdFx0ZDogZnVuY3Rpb24gZChkYXRlT2JqLCBkYXkpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldERhdGUocGFyc2VGbG9hdChkYXkpKTtcblx0XHR9LFxuXHRcdGg6IGZ1bmN0aW9uIGgoZGF0ZU9iaiwgaG91cikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0SG91cnMocGFyc2VGbG9hdChob3VyKSk7XG5cdFx0fSxcblx0XHRpOiBmdW5jdGlvbiBpKGRhdGVPYmosIG1pbnV0ZXMpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldE1pbnV0ZXMocGFyc2VGbG9hdChtaW51dGVzKSk7XG5cdFx0fSxcblx0XHRqOiBmdW5jdGlvbiBqKGRhdGVPYmosIGRheSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0bDogZnVuY3Rpb24gbCgpIHt9LFxuXHRcdG06IGZ1bmN0aW9uIG0oZGF0ZU9iaiwgbW9udGgpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldE1vbnRoKHBhcnNlRmxvYXQobW9udGgpIC0gMSk7XG5cdFx0fSxcblx0XHRuOiBmdW5jdGlvbiBuKGRhdGVPYmosIG1vbnRoKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRNb250aChwYXJzZUZsb2F0KG1vbnRoKSAtIDEpO1xuXHRcdH0sXG5cdFx0czogZnVuY3Rpb24gcyhkYXRlT2JqLCBzZWNvbmRzKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRTZWNvbmRzKHBhcnNlRmxvYXQoc2Vjb25kcykpO1xuXHRcdH0sXG5cdFx0dzogZnVuY3Rpb24gdygpIHt9LFxuXHRcdHk6IGZ1bmN0aW9uIHkoZGF0ZU9iaiwgeWVhcikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RnVsbFllYXIoMjAwMCArIHBhcnNlRmxvYXQoeWVhcikpO1xuXHRcdH1cblx0fSxcblxuXHR0b2tlblJlZ2V4OiB7XG5cdFx0RDogXCIoXFxcXHcrKVwiLFxuXHRcdEY6IFwiKFxcXFx3KylcIixcblx0XHRIOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdEo6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXFxcXHcrXCIsXG5cdFx0SzogXCIoXFxcXHcrKVwiLFxuXHRcdE06IFwiKFxcXFx3KylcIixcblx0XHRTOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdFk6IFwiKFxcXFxkezR9KVwiLFxuXHRcdFo6IFwiKC4rKVwiLFxuXHRcdGQ6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0aDogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRpOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdGo6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0bDogXCIoXFxcXHcrKVwiLFxuXHRcdG06IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0bjogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRzOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdHc6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0eTogXCIoXFxcXGR7Mn0pXCJcblx0fSxcblxuXHRwYWQ6IGZ1bmN0aW9uIHBhZChudW1iZXIpIHtcblx0XHRyZXR1cm4gKFwiMFwiICsgbnVtYmVyKS5zbGljZSgtMik7XG5cdH0sXG5cblx0cGFyc2VEYXRlOiBmdW5jdGlvbiBwYXJzZURhdGUoZGF0ZSwgdGltZWxlc3MsIGdpdmVuRm9ybWF0KSB7XG5cdFx0aWYgKCFkYXRlKSByZXR1cm4gbnVsbDtcblxuXHRcdHZhciBkYXRlX29yaWcgPSBkYXRlO1xuXG5cdFx0aWYgKGRhdGUudG9GaXhlZCkgLy8gdGltZXN0YW1wXG5cdFx0XHRkYXRlID0gbmV3IERhdGUoZGF0ZSk7ZWxzZSBpZiAodHlwZW9mIGRhdGUgPT09IFwic3RyaW5nXCIpIHtcblx0XHRcdHZhciBmb3JtYXQgPSB0eXBlb2YgZ2l2ZW5Gb3JtYXQgPT09IFwic3RyaW5nXCIgPyBnaXZlbkZvcm1hdCA6IHRoaXMuY29uZmlnLmRhdGVGb3JtYXQ7XG5cdFx0XHRkYXRlID0gZGF0ZS50cmltKCk7XG5cblx0XHRcdGlmIChkYXRlID09PSBcInRvZGF5XCIpIHtcblx0XHRcdFx0ZGF0ZSA9IG5ldyBEYXRlKCk7XG5cdFx0XHRcdHRpbWVsZXNzID0gdHJ1ZTtcblx0XHRcdH0gZWxzZSBpZiAodGhpcy5jb25maWcgJiYgdGhpcy5jb25maWcucGFyc2VEYXRlKSBkYXRlID0gdGhpcy5jb25maWcucGFyc2VEYXRlKGRhdGUpO2Vsc2UgaWYgKC9aJC8udGVzdChkYXRlKSB8fCAvR01UJC8udGVzdChkYXRlKSkgLy8gZGF0ZXN0cmluZ3Mgdy8gdGltZXpvbmVcblx0XHRcdFx0ZGF0ZSA9IG5ldyBEYXRlKGRhdGUpO2Vsc2Uge1xuXHRcdFx0XHR2YXIgcGFyc2VkRGF0ZSA9IHRoaXMuY29uZmlnLm5vQ2FsZW5kYXIgPyBuZXcgRGF0ZShuZXcgRGF0ZSgpLnNldEhvdXJzKDAsIDAsIDAsIDApKSA6IG5ldyBEYXRlKG5ldyBEYXRlKCkuZ2V0RnVsbFllYXIoKSwgMCwgMSwgMCwgMCwgMCwgMCk7XG5cblx0XHRcdFx0dmFyIG1hdGNoZWQgPSBmYWxzZTtcblxuXHRcdFx0XHRmb3IgKHZhciBpID0gMCwgbWF0Y2hJbmRleCA9IDAsIHJlZ2V4U3RyID0gXCJcIjsgaSA8IGZvcm1hdC5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRcdHZhciB0b2tlbiA9IGZvcm1hdFtpXTtcblx0XHRcdFx0XHR2YXIgaXNCYWNrU2xhc2ggPSB0b2tlbiA9PT0gXCJcXFxcXCI7XG5cdFx0XHRcdFx0dmFyIGVzY2FwZWQgPSBmb3JtYXRbaSAtIDFdID09PSBcIlxcXFxcIiB8fCBpc0JhY2tTbGFzaDtcblx0XHRcdFx0XHRpZiAodGhpcy50b2tlblJlZ2V4W3Rva2VuXSAmJiAhZXNjYXBlZCkge1xuXHRcdFx0XHRcdFx0cmVnZXhTdHIgKz0gdGhpcy50b2tlblJlZ2V4W3Rva2VuXTtcblx0XHRcdFx0XHRcdHZhciBtYXRjaCA9IG5ldyBSZWdFeHAocmVnZXhTdHIpLmV4ZWMoZGF0ZSk7XG5cdFx0XHRcdFx0XHRpZiAobWF0Y2ggJiYgKG1hdGNoZWQgPSB0cnVlKSkgdGhpcy5yZXZGb3JtYXRbdG9rZW5dKHBhcnNlZERhdGUsIG1hdGNoWysrbWF0Y2hJbmRleF0pO1xuXHRcdFx0XHRcdH0gZWxzZSBpZiAoIWlzQmFja1NsYXNoKSByZWdleFN0ciArPSBcIi5cIjsgLy8gZG9uJ3QgcmVhbGx5IGNhcmVcblx0XHRcdFx0fVxuXG5cdFx0XHRcdGRhdGUgPSBtYXRjaGVkID8gcGFyc2VkRGF0ZSA6IG51bGw7XG5cdFx0XHR9XG5cdFx0fSBlbHNlIGlmIChkYXRlIGluc3RhbmNlb2YgRGF0ZSkgZGF0ZSA9IG5ldyBEYXRlKGRhdGUuZ2V0VGltZSgpKTsgLy8gY3JlYXRlIGEgY29weVxuXG5cdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRpZiAoIShkYXRlIGluc3RhbmNlb2YgRGF0ZSkpIHtcblx0XHRcdGNvbnNvbGUud2FybihcImZsYXRwaWNrcjogaW52YWxpZCBkYXRlIFwiICsgZGF0ZV9vcmlnKTtcblx0XHRcdGNvbnNvbGUuaW5mbyh0aGlzLmVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuIG51bGw7XG5cdFx0fVxuXG5cdFx0aWYgKHRoaXMuY29uZmlnICYmIHRoaXMuY29uZmlnLnV0YyAmJiAhZGF0ZS5mcF9pc1VUQykgZGF0ZSA9IGRhdGUuZnBfdG9VVEMoKTtcblxuXHRcdGlmICh0aW1lbGVzcyA9PT0gdHJ1ZSkgZGF0ZS5zZXRIb3VycygwLCAwLCAwLCAwKTtcblxuXHRcdHJldHVybiBkYXRlO1xuXHR9XG59O1xuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuZnVuY3Rpb24gX2ZsYXRwaWNrcihub2RlTGlzdCwgY29uZmlnKSB7XG5cdHZhciBub2RlcyA9IEFycmF5LnByb3RvdHlwZS5zbGljZS5jYWxsKG5vZGVMaXN0KTsgLy8gc3RhdGljIGxpc3Rcblx0dmFyIGluc3RhbmNlcyA9IFtdO1xuXHRmb3IgKHZhciBpID0gMDsgaSA8IG5vZGVzLmxlbmd0aDsgaSsrKSB7XG5cdFx0dHJ5IHtcblx0XHRcdG5vZGVzW2ldLl9mbGF0cGlja3IgPSBuZXcgRmxhdHBpY2tyKG5vZGVzW2ldLCBjb25maWcgfHwge30pO1xuXHRcdFx0aW5zdGFuY2VzLnB1c2gobm9kZXNbaV0uX2ZsYXRwaWNrcik7XG5cdFx0fSBjYXRjaCAoZSkge1xuXHRcdFx0Y29uc29sZS53YXJuKGUsIGUuc3RhY2spO1xuXHRcdH1cblx0fVxuXG5cdHJldHVybiBpbnN0YW5jZXMubGVuZ3RoID09PSAxID8gaW5zdGFuY2VzWzBdIDogaW5zdGFuY2VzO1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuaWYgKHR5cGVvZiBIVE1MRWxlbWVudCAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHQvLyBicm93c2VyIGVudlxuXHRIVE1MQ29sbGVjdGlvbi5wcm90b3R5cGUuZmxhdHBpY2tyID0gTm9kZUxpc3QucHJvdG90eXBlLmZsYXRwaWNrciA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0XHRyZXR1cm4gX2ZsYXRwaWNrcih0aGlzLCBjb25maWcpO1xuXHR9O1xuXG5cdEhUTUxFbGVtZW50LnByb3RvdHlwZS5mbGF0cGlja3IgPSBmdW5jdGlvbiAoY29uZmlnKSB7XG5cdFx0cmV0dXJuIF9mbGF0cGlja3IoW3RoaXNdLCBjb25maWcpO1xuXHR9O1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuZnVuY3Rpb24gZmxhdHBpY2tyKHNlbGVjdG9yLCBjb25maWcpIHtcblx0cmV0dXJuIF9mbGF0cGlja3Iod2luZG93LmRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoc2VsZWN0b3IpLCBjb25maWcpO1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuaWYgKHR5cGVvZiBqUXVlcnkgIT09IFwidW5kZWZpbmVkXCIpIHtcblx0alF1ZXJ5LmZuLmZsYXRwaWNrciA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0XHRyZXR1cm4gX2ZsYXRwaWNrcih0aGlzLCBjb25maWcpO1xuXHR9O1xufVxuXG5EYXRlLnByb3RvdHlwZS5mcF9pbmNyID0gZnVuY3Rpb24gKGRheXMpIHtcblx0cmV0dXJuIG5ldyBEYXRlKHRoaXMuZ2V0RnVsbFllYXIoKSwgdGhpcy5nZXRNb250aCgpLCB0aGlzLmdldERhdGUoKSArIHBhcnNlSW50KGRheXMsIDEwKSk7XG59O1xuXG5EYXRlLnByb3RvdHlwZS5mcF9pc1VUQyA9IGZhbHNlO1xuRGF0ZS5wcm90b3R5cGUuZnBfdG9VVEMgPSBmdW5jdGlvbiAoKSB7XG5cdHZhciBuZXdEYXRlID0gbmV3IERhdGUodGhpcy5nZXRVVENGdWxsWWVhcigpLCB0aGlzLmdldFVUQ01vbnRoKCksIHRoaXMuZ2V0VVRDRGF0ZSgpLCB0aGlzLmdldFVUQ0hvdXJzKCksIHRoaXMuZ2V0VVRDTWludXRlcygpLCB0aGlzLmdldFVUQ1NlY29uZHMoKSk7XG5cblx0bmV3RGF0ZS5mcF9pc1VUQyA9IHRydWU7XG5cdHJldHVybiBuZXdEYXRlO1xufTtcblxuLy8gSUU5IGNsYXNzTGlzdCBwb2x5ZmlsbFxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbmlmICghd2luZG93LmRvY3VtZW50LmRvY3VtZW50RWxlbWVudC5jbGFzc0xpc3QgJiYgT2JqZWN0LmRlZmluZVByb3BlcnR5ICYmIHR5cGVvZiBIVE1MRWxlbWVudCAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHRPYmplY3QuZGVmaW5lUHJvcGVydHkoSFRNTEVsZW1lbnQucHJvdG90eXBlLCBcImNsYXNzTGlzdFwiLCB7XG5cdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHR2YXIgc2VsZiA9IHRoaXM7XG5cdFx0XHRmdW5jdGlvbiB1cGRhdGUoZm4pIHtcblx0XHRcdFx0cmV0dXJuIGZ1bmN0aW9uICh2YWx1ZSkge1xuXHRcdFx0XHRcdHZhciBjbGFzc2VzID0gc2VsZi5jbGFzc05hbWUuc3BsaXQoL1xccysvKSxcblx0XHRcdFx0XHQgICAgaW5kZXggPSBjbGFzc2VzLmluZGV4T2YodmFsdWUpO1xuXG5cdFx0XHRcdFx0Zm4oY2xhc3NlcywgaW5kZXgsIHZhbHVlKTtcblx0XHRcdFx0XHRzZWxmLmNsYXNzTmFtZSA9IGNsYXNzZXMuam9pbihcIiBcIik7XG5cdFx0XHRcdH07XG5cdFx0XHR9XG5cblx0XHRcdHZhciByZXQgPSB7XG5cdFx0XHRcdGFkZDogdXBkYXRlKGZ1bmN0aW9uIChjbGFzc2VzLCBpbmRleCwgdmFsdWUpIHtcblx0XHRcdFx0XHRpZiAoIX5pbmRleCkgY2xhc3Nlcy5wdXNoKHZhbHVlKTtcblx0XHRcdFx0fSksXG5cblx0XHRcdFx0cmVtb3ZlOiB1cGRhdGUoZnVuY3Rpb24gKGNsYXNzZXMsIGluZGV4KSB7XG5cdFx0XHRcdFx0aWYgKH5pbmRleCkgY2xhc3Nlcy5zcGxpY2UoaW5kZXgsIDEpO1xuXHRcdFx0XHR9KSxcblxuXHRcdFx0XHR0b2dnbGU6IHVwZGF0ZShmdW5jdGlvbiAoY2xhc3NlcywgaW5kZXgsIHZhbHVlKSB7XG5cdFx0XHRcdFx0aWYgKH5pbmRleCkgY2xhc3Nlcy5zcGxpY2UoaW5kZXgsIDEpO2Vsc2UgY2xhc3Nlcy5wdXNoKHZhbHVlKTtcblx0XHRcdFx0fSksXG5cblx0XHRcdFx0Y29udGFpbnM6IGZ1bmN0aW9uIGNvbnRhaW5zKHZhbHVlKSB7XG5cdFx0XHRcdFx0cmV0dXJuICEhfnNlbGYuY2xhc3NOYW1lLnNwbGl0KC9cXHMrLykuaW5kZXhPZih2YWx1ZSk7XG5cdFx0XHRcdH0sXG5cblx0XHRcdFx0aXRlbTogZnVuY3Rpb24gaXRlbShpKSB7XG5cdFx0XHRcdFx0cmV0dXJuIHNlbGYuY2xhc3NOYW1lLnNwbGl0KC9cXHMrLylbaV0gfHwgbnVsbDtcblx0XHRcdFx0fVxuXHRcdFx0fTtcblxuXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHJldCwgXCJsZW5ndGhcIiwge1xuXHRcdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0XHRyZXR1cm4gc2VsZi5jbGFzc05hbWUuc3BsaXQoL1xccysvKS5sZW5ndGg7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXG5cdFx0XHRyZXR1cm4gcmV0O1xuXHRcdH1cblx0fSk7XG59XG5cbmlmICh0eXBlb2YgbW9kdWxlICE9PSBcInVuZGVmaW5lZFwiKSBtb2R1bGUuZXhwb3J0cyA9IEZsYXRwaWNrcjtcbiIsInZhciBWdWUgLy8gbGF0ZSBiaW5kXG52YXIgdmVyc2lvblxudmFyIG1hcCA9IHdpbmRvdy5fX1ZVRV9IT1RfTUFQX18gPSBPYmplY3QuY3JlYXRlKG51bGwpXG52YXIgaW5zdGFsbGVkID0gZmFsc2VcbnZhciBpc0Jyb3dzZXJpZnkgPSBmYWxzZVxudmFyIGluaXRIb29rTmFtZSA9ICdiZWZvcmVDcmVhdGUnXG5cbmV4cG9ydHMuaW5zdGFsbCA9IGZ1bmN0aW9uICh2dWUsIGJyb3dzZXJpZnkpIHtcbiAgaWYgKGluc3RhbGxlZCkgcmV0dXJuXG4gIGluc3RhbGxlZCA9IHRydWVcblxuICBWdWUgPSB2dWUuX19lc01vZHVsZSA/IHZ1ZS5kZWZhdWx0IDogdnVlXG4gIHZlcnNpb24gPSBWdWUudmVyc2lvbi5zcGxpdCgnLicpLm1hcChOdW1iZXIpXG4gIGlzQnJvd3NlcmlmeSA9IGJyb3dzZXJpZnlcblxuICAvLyBjb21wYXQgd2l0aCA8IDIuMC4wLWFscGhhLjdcbiAgaWYgKFZ1ZS5jb25maWcuX2xpZmVjeWNsZUhvb2tzLmluZGV4T2YoJ2luaXQnKSA+IC0xKSB7XG4gICAgaW5pdEhvb2tOYW1lID0gJ2luaXQnXG4gIH1cblxuICBleHBvcnRzLmNvbXBhdGlibGUgPSB2ZXJzaW9uWzBdID49IDJcbiAgaWYgKCFleHBvcnRzLmNvbXBhdGlibGUpIHtcbiAgICBjb25zb2xlLndhcm4oXG4gICAgICAnW0hNUl0gWW91IGFyZSB1c2luZyBhIHZlcnNpb24gb2YgdnVlLWhvdC1yZWxvYWQtYXBpIHRoYXQgaXMgJyArXG4gICAgICAnb25seSBjb21wYXRpYmxlIHdpdGggVnVlLmpzIGNvcmUgXjIuMC4wLidcbiAgICApXG4gICAgcmV0dXJuXG4gIH1cbn1cblxuLyoqXG4gKiBDcmVhdGUgYSByZWNvcmQgZm9yIGEgaG90IG1vZHVsZSwgd2hpY2gga2VlcHMgdHJhY2sgb2YgaXRzIGNvbnN0cnVjdG9yXG4gKiBhbmQgaW5zdGFuY2VzXG4gKlxuICogQHBhcmFtIHtTdHJpbmd9IGlkXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICovXG5cbmV4cG9ydHMuY3JlYXRlUmVjb3JkID0gZnVuY3Rpb24gKGlkLCBvcHRpb25zKSB7XG4gIHZhciBDdG9yID0gbnVsbFxuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBDdG9yID0gb3B0aW9uc1xuICAgIG9wdGlvbnMgPSBDdG9yLm9wdGlvbnNcbiAgfVxuICBtYWtlT3B0aW9uc0hvdChpZCwgb3B0aW9ucylcbiAgbWFwW2lkXSA9IHtcbiAgICBDdG9yOiBWdWUuZXh0ZW5kKG9wdGlvbnMpLFxuICAgIGluc3RhbmNlczogW11cbiAgfVxufVxuXG4vKipcbiAqIE1ha2UgYSBDb21wb25lbnQgb3B0aW9ucyBvYmplY3QgaG90LlxuICpcbiAqIEBwYXJhbSB7U3RyaW5nfSBpZFxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqL1xuXG5mdW5jdGlvbiBtYWtlT3B0aW9uc0hvdCAoaWQsIG9wdGlvbnMpIHtcbiAgaW5qZWN0SG9vayhvcHRpb25zLCBpbml0SG9va05hbWUsIGZ1bmN0aW9uICgpIHtcbiAgICBtYXBbaWRdLmluc3RhbmNlcy5wdXNoKHRoaXMpXG4gIH0pXG4gIGluamVjdEhvb2sob3B0aW9ucywgJ2JlZm9yZURlc3Ryb3knLCBmdW5jdGlvbiAoKSB7XG4gICAgdmFyIGluc3RhbmNlcyA9IG1hcFtpZF0uaW5zdGFuY2VzXG4gICAgaW5zdGFuY2VzLnNwbGljZShpbnN0YW5jZXMuaW5kZXhPZih0aGlzKSwgMSlcbiAgfSlcbn1cblxuLyoqXG4gKiBJbmplY3QgYSBob29rIHRvIGEgaG90IHJlbG9hZGFibGUgY29tcG9uZW50IHNvIHRoYXRcbiAqIHdlIGNhbiBrZWVwIHRyYWNrIG9mIGl0LlxuICpcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKiBAcGFyYW0ge1N0cmluZ30gbmFtZVxuICogQHBhcmFtIHtGdW5jdGlvbn0gaG9va1xuICovXG5cbmZ1bmN0aW9uIGluamVjdEhvb2sgKG9wdGlvbnMsIG5hbWUsIGhvb2spIHtcbiAgdmFyIGV4aXN0aW5nID0gb3B0aW9uc1tuYW1lXVxuICBvcHRpb25zW25hbWVdID0gZXhpc3RpbmdcbiAgICA/IEFycmF5LmlzQXJyYXkoZXhpc3RpbmcpXG4gICAgICA/IGV4aXN0aW5nLmNvbmNhdChob29rKVxuICAgICAgOiBbZXhpc3RpbmcsIGhvb2tdXG4gICAgOiBbaG9va11cbn1cblxuZnVuY3Rpb24gdHJ5V3JhcCAoZm4pIHtcbiAgcmV0dXJuIGZ1bmN0aW9uIChpZCwgYXJnKSB7XG4gICAgdHJ5IHsgZm4oaWQsIGFyZykgfSBjYXRjaCAoZSkge1xuICAgICAgY29uc29sZS5lcnJvcihlKVxuICAgICAgY29uc29sZS53YXJuKCdTb21ldGhpbmcgd2VudCB3cm9uZyBkdXJpbmcgVnVlIGNvbXBvbmVudCBob3QtcmVsb2FkLiBGdWxsIHJlbG9hZCByZXF1aXJlZC4nKVxuICAgIH1cbiAgfVxufVxuXG5leHBvcnRzLnJlcmVuZGVyID0gdHJ5V3JhcChmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgdmFyIHJlY29yZCA9IG1hcFtpZF1cbiAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgb3B0aW9ucyA9IG9wdGlvbnMub3B0aW9uc1xuICB9XG4gIHJlY29yZC5DdG9yLm9wdGlvbnMucmVuZGVyID0gb3B0aW9ucy5yZW5kZXJcbiAgcmVjb3JkLkN0b3Iub3B0aW9ucy5zdGF0aWNSZW5kZXJGbnMgPSBvcHRpb25zLnN0YXRpY1JlbmRlckZuc1xuICByZWNvcmQuaW5zdGFuY2VzLnNsaWNlKCkuZm9yRWFjaChmdW5jdGlvbiAoaW5zdGFuY2UpIHtcbiAgICBpbnN0YW5jZS4kb3B0aW9ucy5yZW5kZXIgPSBvcHRpb25zLnJlbmRlclxuICAgIGluc3RhbmNlLiRvcHRpb25zLnN0YXRpY1JlbmRlckZucyA9IG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zXG4gICAgaW5zdGFuY2UuX3N0YXRpY1RyZWVzID0gW10gLy8gcmVzZXQgc3RhdGljIHRyZWVzXG4gICAgaW5zdGFuY2UuJGZvcmNlVXBkYXRlKClcbiAgfSlcbn0pXG5cbmV4cG9ydHMucmVsb2FkID0gdHJ5V3JhcChmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgb3B0aW9ucyA9IG9wdGlvbnMub3B0aW9uc1xuICB9XG4gIG1ha2VPcHRpb25zSG90KGlkLCBvcHRpb25zKVxuICB2YXIgcmVjb3JkID0gbWFwW2lkXVxuICBpZiAodmVyc2lvblsxXSA8IDIpIHtcbiAgICAvLyBwcmVzZXJ2ZSBwcmUgMi4yIGJlaGF2aW9yIGZvciBnbG9iYWwgbWl4aW4gaGFuZGxpbmdcbiAgICByZWNvcmQuQ3Rvci5leHRlbmRPcHRpb25zID0gb3B0aW9uc1xuICB9XG4gIHZhciBuZXdDdG9yID0gcmVjb3JkLkN0b3Iuc3VwZXIuZXh0ZW5kKG9wdGlvbnMpXG4gIHJlY29yZC5DdG9yLm9wdGlvbnMgPSBuZXdDdG9yLm9wdGlvbnNcbiAgcmVjb3JkLkN0b3IuY2lkID0gbmV3Q3Rvci5jaWRcbiAgcmVjb3JkLkN0b3IucHJvdG90eXBlID0gbmV3Q3Rvci5wcm90b3R5cGVcbiAgaWYgKG5ld0N0b3IucmVsZWFzZSkge1xuICAgIC8vIHRlbXBvcmFyeSBnbG9iYWwgbWl4aW4gc3RyYXRlZ3kgdXNlZCBpbiA8IDIuMC4wLWFscGhhLjZcbiAgICBuZXdDdG9yLnJlbGVhc2UoKVxuICB9XG4gIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgIGlmIChpbnN0YW5jZS4kdm5vZGUgJiYgaW5zdGFuY2UuJHZub2RlLmNvbnRleHQpIHtcbiAgICAgIGluc3RhbmNlLiR2bm9kZS5jb250ZXh0LiRmb3JjZVVwZGF0ZSgpXG4gICAgfSBlbHNlIHtcbiAgICAgIGNvbnNvbGUud2FybignUm9vdCBvciBtYW51YWxseSBtb3VudGVkIGluc3RhbmNlIG1vZGlmaWVkLiBGdWxsIHJlbG9hZCByZXF1aXJlZC4nKVxuICAgIH1cbiAgfSlcbn0pXG4iLCLvu788dGVtcGxhdGU+XHJcbiAgICA8ZGl2IGNsYXNzPVwiZm9ybS1kYXRlIGlucHV0LWdyb3VwXCI+XHJcbiAgICAgICAgPGlucHV0IHR5cGU9XCJ0ZXh0XCIgOmNsYXNzPVwiaW5wdXRDbGFzc1wiICA6cGxhY2Vob2xkZXI9XCJwbGFjZWhvbGRlclwiIDp2YWx1ZT1cInZhbHVlXCIgQGlucHV0PVwib25JbnB1dFwiIGRhdGEtaW5wdXQgLz5cclxuICAgICAgICA8YnV0dG9uIHR5cGU9XCJzdWJtaXRcIiBjbGFzcz1cImJ0biBidG4tbGluayBidG4tY2xlYXJcIiBkYXRhLWNsZWFyPlxyXG4gICAgICAgICAgICA8c3Bhbj48L3NwYW4+XHJcbiAgICAgICAgPC9idXR0b24+XHJcbiAgICAgICAgPHNwYW4gY2xhc3M9XCJpbnB1dC1ncm91cC1hZGRvblwiIGRhdGEtdG9nZ2xlPlxyXG4gICAgICAgICAgICA8c3BhbiBjbGFzcz1cImNhbGVuZGFyXCI+PC9zcGFuPlxyXG4gICAgICAgIDwvc3Bhbj5cclxuICAgIDwvZGl2PlxyXG48L3RlbXBsYXRlPlxyXG5cclxuPHNjcmlwdD5cclxuaW1wb3J0IEZsYXRwaWNrciBmcm9tICdmbGF0cGlja3InXHJcblxyXG5leHBvcnQgZGVmYXVsdCB7XHJcbiAgICBwcm9wczoge1xyXG4gICAgICAgIGlucHV0Q2xhc3M6IHtcclxuICAgICAgICAgICAgdHlwZTogU3RyaW5nXHJcbiAgICAgICAgfSxcclxuICAgICAgICBwbGFjZWhvbGRlcjoge1xyXG4gICAgICAgICAgICB0eXBlOiBTdHJpbmcsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICcnXHJcbiAgICAgICAgfSxcclxuICAgICAgICBvcHRpb25zOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IE9iamVjdCxcclxuICAgICAgICAgICAgZGVmYXVsdDogKCkgPT4geyByZXR1cm4ge30gfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgdmFsdWU6IHtcclxuICAgICAgICAgICAgdHlwZTogU3RyaW5nLFxyXG4gICAgICAgICAgICBkZWZhdWx0OiAnJ1xyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgZGF0YSAoKSB7XHJcbiAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICBmcDogbnVsbFxyXG4gICAgICB9XHJcbiAgfSxcclxuICAgIGNvbXB1dGVkOiB7XHJcbiAgICAgICAgZnBPcHRpb25zICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIEpTT04uc3RyaW5naWZ5KHRoaXMub3B0aW9ucylcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgd2F0Y2g6IHtcclxuICAgICAgICBmcE9wdGlvbnMgKG5ld09wdCkge1xyXG4gICAgICAgICAgICBjb25zdCBvcHRpb24gPSBKU09OLnBhcnNlKG5ld09wdClcclxuICAgICAgICAgICAgZm9yIChsZXQgbyBpbiBvcHRpb24pIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZnAuc2V0KG8sIG9wdGlvbltvXSlcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgbW91bnRlZCAoKSB7XHJcbiAgICAgIGNvbnN0IHNlbGYgPSB0aGlzXHJcbiAgICAgIGNvbnN0IG9yaWdPblZhbFVwZGF0ZSA9IHRoaXMub3B0aW9ucy5vblZhbHVlVXBkYXRlXHJcbiAgICAgIGNvbnN0IG1lcmdlZE9wdGlvbnMgPSBPYmplY3QuYXNzaWduKHRoaXMub3B0aW9ucywge1xyXG4gICAgICAgICAgd3JhcDogdHJ1ZSxcclxuICAgICAgICAgIG9uVmFsdWVVcGRhdGUgKCkge1xyXG4gICAgICAgICAgICAgIHNlbGYub25JbnB1dChzZWxmLiRlbC5xdWVyeVNlbGVjdG9yKCdpbnB1dCcpLnZhbHVlKVxyXG4gICAgICAgICAgICAgIGlmICh0eXBlb2Ygb3JpZ09uVmFsVXBkYXRlID09PSAnZnVuY3Rpb24nKSB7XHJcbiAgICAgICAgICAgICAgICAgIG9yaWdPblZhbFVwZGF0ZSgpXHJcbiAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgfVxyXG4gICAgICB9KVxyXG5cclxuICAgICAgdGhpcy5mcCA9IG5ldyBGbGF0cGlja3IodGhpcy4kZWwsIG1lcmdlZE9wdGlvbnMpXHJcbiAgICAgIHRoaXMuJGVtaXQoJ0ZsYXRwaWNrclJlZicsIHRoaXMuZnApXHJcbiAgfSxcclxuICBkZXN0cm95ZWQgKCkge1xyXG4gICAgICB0aGlzLmZwLmRlc3Ryb3koKVxyXG4gICAgICB0aGlzLmZwID0gbnVsbFxyXG4gIH0sXHJcbiAgICBtZXRob2RzOiB7XHJcbiAgICAgICAgb25JbnB1dCAoZSkge1xyXG4gICAgICAgICAgICBjb25zdCBzZWxlY3RlZERhdGVzID0gdGhpcy5mcC5zZWxlY3RlZERhdGVzIHx8IFtdO1xyXG4gICAgICAgICAgICBjb25zdCBsZWZ0ID0gc2VsZWN0ZWREYXRlcy5sZW5ndGggPiAwID8gc2VsZWN0ZWREYXRlc1swXSA6IG51bGw7XHJcbiAgICAgICAgICAgIGNvbnN0IHJpZ2h0ID0gc2VsZWN0ZWREYXRlcy5sZW5ndGggPiAxID8gc2VsZWN0ZWREYXRlc1sxXSA6IG51bGw7XHJcbiAgICAgICAgICAgIHRoaXMuJGVtaXQoJ2lucHV0JywgKHR5cGVvZiBlID09PSAnc3RyaW5nJyA/IGUgOiBlLnRhcmdldC52YWx1ZSksIGxlZnQsIHJpZ2h0KVxyXG5cclxuICAgICAgICAgICAgaWYgKHJpZ2h0ID09IG51bGwpXHJcbiAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGVsLmNsYXNzTGlzdC5yZW1vdmUoXCJhbnN3ZXJlZFwiKVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kZWwuY2xhc3NMaXN0LmFkZChcImFuc3dlcmVkXCIpXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuPC9zY3JpcHQ+Iiwi77u/PHRlbXBsYXRlPlxyXG4gICAgPHRhYmxlIGNsYXNzPVwidGFibGUgdGFibGUtc3RyaXBlZCB0YWJsZS1vcmRlcmVkIHRhYmxlLWJvcmRlcmVkIHRhYmxlLWhvdmVyIHRhYmxlLXdpdGgtY2hlY2tib3hlcyB0YWJsZS13aXRoLXByZWZpbGxlZC1jb2x1bW4gdGFibGUtaW50ZXJ2aWV3c1wiPlxyXG4gICAgICAgIDx0aGVhZD5cclxuICAgICAgICAgICAgXHJcbiAgICAgICAgPC90aGVhZD5cclxuICAgICAgICA8dGJvZHk+PC90Ym9keT5cclxuICAgIDwvdGFibGU+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG5leHBvcnQgZGVmYXVsdCB7XHJcbiAgICBwcm9wczoge1xyXG4gICAgICAgIGFkZFBhcmFtc1RvUmVxdWVzdDoge1xyXG4gICAgICAgICAgICB0eXBlOiBGdW5jdGlvbixcclxuICAgICAgICAgICAgZGVmYXVsdDogKGQpID0+IHsgcmV0dXJuIGQgfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcmVzcG9uc2VQcm9jZXNzb3I6IHtcclxuICAgICAgICAgICAgdHlwZTogRnVuY3Rpb24sXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6IChyKSA9PiB7IHJldHVybiByIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIHRhYmxlT3B0aW9uczoge1xyXG4gICAgICAgICAgICB0eXBlOiBPYmplY3QsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICgpID0+IHsgcmV0dXJuIHt9IH1cclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgZGF0YSAoKSB7XHJcbiAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgdGFibGU6IG51bGxcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgY29tcHV0ZWQ6IHtcclxuICAgIH0sXHJcbiAgICB3YXRjaDoge1xyXG4gICAgfSxcclxuICAgIG1ldGhvZHM6IHtcclxuICAgICAgICByZWxvYWQ6IGZ1bmN0aW9uKGRhdGEpe1xyXG4gICAgICAgICAgICB0aGlzLnRhYmxlLmFqYXguZGF0YSA9IGRhdGE7XHJcbiAgICAgICAgICAgIHRoaXMudGFibGUuYWpheC5yZWxvYWQoKVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgb25UYWJsZUluaXRDb21wbGV0ZTogZnVuY3Rpb24oKXtcclxuICAgICAgICAgICAgJCh0aGlzLiRlbCkucGFyZW50KCcuZGF0YVRhYmxlc193cmFwcGVyJykuZmluZCgnLmRhdGFUYWJsZXNfZmlsdGVyIGxhYmVsJykub24oJ2NsaWNrJywgZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgIGlmIChlLnRhcmdldCAhPT0gdGhpcylcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgICAgICBpZiAoJCh0aGlzKS5oYXNDbGFzcyhcImFjdGl2ZVwiKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICQodGhpcykucmVtb3ZlQ2xhc3MoXCJhY3RpdmVcIik7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICAkKHRoaXMpLmFkZENsYXNzKFwiYWN0aXZlXCIpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgbW91bnRlZCAoKSB7XHJcbiAgICAgICAgY29uc3Qgc2VsZiA9IHRoaXNcclxuICAgICAgICB2YXIgb3B0aW9ucyA9IE9iamVjdC5hc3NpZ24oe1xyXG4gICAgICAgICAgICBwcm9jZXNzaW5nOiB0cnVlLFxyXG4gICAgICAgICAgICBzZXJ2ZXJTaWRlOiB0cnVlLFxyXG4gICAgICAgICAgICBsYW5ndWFnZTpcclxuICAgICAgICAgICAge1xyXG4gICAgICAgICAgICAgICAgXCJ1cmxcIjogd2luZG93LmlucHV0LnNldHRpbmdzLmNvbmZpZy5kYXRhVGFibGVUcmFuc2xhdGlvbnNVcmwsXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHNlYXJjaEhpZ2hsaWdodDogdHJ1ZSxcclxuICAgICAgICAgICAgcGFnaW5nVHlwZTogXCJmdWxsX251bWJlcnNcIixcclxuICAgICAgICAgICAgbGVuZ3RoQ2hhbmdlOiBmYWxzZSwgLy8gZG8gbm90IHNob3cgcGFnZSBzaXplIHNlbGVjdG9yXHJcbiAgICAgICAgICAgIHBhZ2VMZW5ndGg6IDEwLCAvLyBwYWdlIHNpemVcclxuICAgICAgICAgICAgZG9tOiBcImZydHBcIixcclxuICAgICAgICAgICAgY29uZGl0aW9uYWxQYWdpbmc6IHRydWVcclxuICAgICAgICB9LCB0aGlzLnRhYmxlT3B0aW9ucylcclxuXHJcbiAgICAgICAgb3B0aW9ucy5hamF4LmRhdGEgPSBmdW5jdGlvbihkKXtcclxuICAgICAgICAgICAgc2VsZi5hZGRQYXJhbXNUb1JlcXVlc3QoZCk7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgb3B0aW9ucy5hamF4LmNvbXBsZXRlID0gZnVuY3Rpb24ocmVzcG9uc2Upe1xyXG4gICAgICAgICAgICBzZWxmLnJlc3BvbnNlUHJvY2Vzc29yKHJlc3BvbnNlLnJlc3BvbnNlSlNPTik7XHJcbiAgICAgICAgfTtcclxuXHJcbiAgICAgICAgdGhpcy50YWJsZSA9ICQodGhpcy4kZWwpLkRhdGFUYWJsZShvcHRpb25zKVxyXG4gICAgICAgIHRoaXMudGFibGUub24oJ2luaXQuZHQnLCB0aGlzLm9uVGFibGVJbml0Q29tcGxldGUpO1xyXG4gICAgICAgIHRoaXMuJGVtaXQoJ0RhdGFUYWJsZVJlZicsIHRoaXMudGFibGUpXHJcbiAgICB9LFxyXG4gICAgZGVzdHJveWVkICgpIHtcclxuICAgIH1cclxufVxyXG48L3NjcmlwdD4iLCLvu788dGVtcGxhdGU+XHJcbiAgICA8ZGl2IGNsYXNzPVwiY29tYm8tYm94XCI+XHJcbiAgICAgICAgPGRpdiBjbGFzcz1cImJ0bi1ncm91cCBidG4taW5wdXQgY2xlYXJmaXhcIj5cclxuICAgICAgICAgICAgPGJ1dHRvbiB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4gZHJvcGRvd24tdG9nZ2xlXCIgZGF0YS10b2dnbGU9XCJkcm9wZG93blwiPlxyXG4gICAgICAgICAgICAgICAgPHNwYW4gZGF0YS1iaW5kPVwibGFiZWxcIiB2LWlmPVwidmFsdWUgPT09IG51bGxcIiBjbGFzcz1cImdyYXktdGV4dFwiPnt7cGxhY2Vob2xkZXJUZXh0fX08L3NwYW4+XHJcbiAgICAgICAgICAgICAgICA8c3BhbiBkYXRhLWJpbmQ9XCJsYWJlbFwiIHYtZWxzZT57e3ZhbHVlLnZhbHVlfX08L3NwYW4+XHJcbiAgICAgICAgICAgIDwvYnV0dG9uPlxyXG4gICAgICAgICAgICA8dWwgcmVmPVwiZHJvcGRvd25NZW51XCIgY2xhc3M9XCJkcm9wZG93bi1tZW51XCIgcm9sZT1cIm1lbnVcIj5cclxuICAgICAgICAgICAgICAgIDxsaT5cclxuICAgICAgICAgICAgICAgICAgICA8aW5wdXQgdHlwZT1cInRleHRcIiByZWY9XCJzZWFyY2hCb3hcIiA6aWQ9XCJpbnB1dElkXCIgcGxhY2Vob2xkZXI9XCJTZWFyY2hcIiBAaW5wdXQ9XCJ1cGRhdGVPcHRpb25zTGlzdFwiIHYtb246a2V5dXAuZG93bj1cIm9uU2VhcmNoQm94RG93bktleVwiIHYtbW9kZWw9XCJzZWFyY2hUZXJtXCIgLz5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1mb3I9XCJvcHRpb24gaW4gb3B0aW9uc1wiIDprZXk9XCJvcHRpb24ua2V5XCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGEgaHJlZj1cImphdmFzY3JpcHQ6dm9pZCgwKTtcIiB2LW9uOmNsaWNrPVwic2VsZWN0T3B0aW9uKG9wdGlvbilcIiB2LWh0bWw9XCJoaWdobGlnaHQob3B0aW9uLnZhbHVlLCBzZWFyY2hUZXJtKVwiIHYtb246a2V5ZG93bi51cD1cIm9uT3B0aW9uVXBLZXlcIj48L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtaWY9XCJpc0xvYWRpbmdcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YT5Mb2FkaW5nLi4uPC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWlmPVwiIWlzTG9hZGluZyAmJiBvcHRpb25zLmxlbmd0aCA9PT0gMFwiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhPk5vIHJlc3VsdHMgZm91bmQ8L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICA8L3VsPlxyXG4gICAgICAgIDwvZGl2PlxyXG4gICAgICAgIDxidXR0b24gdi1pZj1cInZhbHVlICE9PSBudWxsXCIgY2xhc3M9XCJidG4gYnRuLWxpbmsgYnRuLWNsZWFyXCIgQGNsaWNrPVwiY2xlYXJcIj5cclxuICAgICAgICAgICAgPHNwYW4+PC9zcGFuPlxyXG4gICAgICAgIDwvYnV0dG9uPlxyXG4gICAgPC9kaXY+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG4gICAgbW9kdWxlLmV4cG9ydHMgPSB7XHJcbiAgICAgICAgbmFtZTogJ3VzZXItc2VsZWN0b3InLFxyXG4gICAgICAgIHByb3BzOiBbJ2ZldGNoVXJsJywgJ2NvbnRyb2xJZCcsICd2YWx1ZScsICdwbGFjZWhvbGRlcicsICdhamF4UGFyYW1zJ10sXHJcbiAgICAgICAgZGF0YTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9uczogW10sXHJcbiAgICAgICAgICAgICAgICBpc0xvYWRpbmc6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgc2VhcmNoVGVybTogJydcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGNvbXB1dGVkOiB7XHJcbiAgICAgICAgICAgIGlucHV0SWQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBgc2JfJHt0aGlzLmNvbnRyb2xJZH1gO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBwbGFjZWhvbGRlclRleHQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLnBsYWNlaG9sZGVyIHx8IFwiU2VsZWN0XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIG1vdW50ZWQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBjb25zdCBqcUVsID0gJCh0aGlzLiRlbClcclxuICAgICAgICAgICAgY29uc3QgZm9jdXNUbyA9IGpxRWwuZmluZChgIyR7dGhpcy5pbnB1dElkfWApXHJcbiAgICAgICAgICAgIGpxRWwub24oJ3Nob3duLmJzLmRyb3Bkb3duJywgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgZm9jdXNUby5mb2N1cygpXHJcbiAgICAgICAgICAgICAgICB0aGlzLmZldGNoT3B0aW9ucyh0aGlzLnNlYXJjaFRlcm0pXHJcbiAgICAgICAgICAgIH0pXHJcblxyXG4gICAgICAgICAgICBqcUVsLm9uKCdoaWRkZW4uYnMuZHJvcGRvd24nLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnNlYXJjaFRlcm0gPSBcIlwiXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgfSxcclxuICAgICAgICBtZXRob2RzOiB7XHJcbiAgICAgICAgICAgIG9uU2VhcmNoQm94RG93bktleShldmVudCkge1xyXG4gICAgICAgICAgICAgICAgdmFyICRmaXJzdE9wdGlvbkFuY2hvciA9ICQodGhpcy4kcmVmcy5kcm9wZG93bk1lbnUpLmZpbmQoJ2EnKS5maXJzdCgpO1xyXG4gICAgICAgICAgICAgICAgJGZpcnN0T3B0aW9uQW5jaG9yLmZvY3VzKCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIG9uT3B0aW9uVXBLZXkoZXZlbnQpIHtcclxuICAgICAgICAgICAgICAgIHZhciBpc0ZpcnN0T3B0aW9uID0gJChldmVudC50YXJnZXQpLnBhcmVudCgpLmluZGV4KCkgPT09IDE7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKGlzRmlyc3RPcHRpb24pIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLiRyZWZzLnNlYXJjaEJveC5mb2N1cygpO1xyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50LnN0b3BQcm9wYWdhdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBmZXRjaE9wdGlvbnM6IGZ1bmN0aW9uIChmaWx0ZXIgPSBcIlwiKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICB2YXIgcmVxdWVzdFBhcmFtcyA9IE9iamVjdC5hc3NpZ24oeyBxdWVyeTogZmlsdGVyIH0sIHRoaXMuYWpheFBhcmFtcyk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRodHRwLmdldCh0aGlzLmZldGNoVXJsLCB7cGFyYW1zOiByZXF1ZXN0UGFyYW1zfSlcclxuICAgICAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMub3B0aW9ucyA9IHJlc3BvbnNlLmJvZHkub3B0aW9ucyB8fCBbXTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9LCByZXNwb25zZSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIFxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgY2xlYXI6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGVtaXQoJ3NlbGVjdGVkJywgbnVsbCwgdGhpcy5jb250cm9sSWQpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zZWFyY2hUZXJtID0gXCJcIjtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgc2VsZWN0T3B0aW9uOiBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGVtaXQoJ3NlbGVjdGVkJywgdmFsdWUsIHRoaXMuY29udHJvbElkKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgdXBkYXRlT3B0aW9uc0xpc3QoZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5mZXRjaE9wdGlvbnMoZS50YXJnZXQudmFsdWUpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBoaWdobGlnaHQ6IGZ1bmN0aW9uICh0aXRsZSwgc2VhcmNoVGVybSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGVuY29kZWRUaXRsZSA9IF8uZXNjYXBlKHRpdGxlKTtcclxuICAgICAgICAgICAgICAgIGlmIChzZWFyY2hUZXJtKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHNhZmVTZWFyY2hUZXJtID0gXy5lc2NhcGUoXy5lc2NhcGVSZWdFeHAoc2VhcmNoVGVybSkpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICB2YXIgaVF1ZXJ5ID0gbmV3IFJlZ0V4cChzYWZlU2VhcmNoVGVybSwgXCJpZ1wiKTtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZW5jb2RlZFRpdGxlLnJlcGxhY2UoaVF1ZXJ5LCAobWF0Y2hlZFR4dCwgYSwgYikgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gYDxzdHJvbmc+JHttYXRjaGVkVHh0fTwvc3Ryb25nPmA7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGVuY29kZWRUaXRsZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgIH07XHJcbjwvc2NyaXB0PiIsIu+7v++7v2ltcG9ydCBWdWUgZnJvbSAndnVlJ1xyXG5pbXBvcnQgVnVlUmVzb3VyY2UgZnJvbSAndnVlLXJlc291cmNlJ1xyXG5pbXBvcnQgVHlwZWFoZWFkIGZyb20gJy4vVHlwZWFoZWFkLnZ1ZSdcclxuaW1wb3J0IERhdGVQaWNrZXIgZnJvbSAnLi9EYXRlUGlja2VyLnZ1ZSdcclxuaW1wb3J0IEludGVydmlld1RhYmxlIGZyb20gJy4vSW50ZXJ2aWV3VGFibGUudnVlJ1xyXG5pbXBvcnQgVmVlVmFsaWRhdGUgZnJvbSAndmVlLXZhbGlkYXRlJztcclxuXHJcblZ1ZS51c2UoVmVlVmFsaWRhdGUpO1xyXG5WdWUudXNlKFZ1ZVJlc291cmNlKTtcclxuXHJcblZ1ZS5jb21wb25lbnQoJ0ZsYXRwaWNrcicsIERhdGVQaWNrZXIpO1xyXG5WdWUuY29tcG9uZW50KFwidHlwZWFoZWFkXCIsIFR5cGVhaGVhZCk7XHJcblZ1ZS5jb21wb25lbnQoXCJpbnRlcnZpZXctdGFibGVcIiwgSW50ZXJ2aWV3VGFibGUpOyJdfQ==
