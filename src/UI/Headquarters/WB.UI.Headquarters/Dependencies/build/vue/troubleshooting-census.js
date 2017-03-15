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

/*! flatpickr v2.4.4, @license MIT */
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

		if (self.selectedDates.length) {
			if (self.config.enableTime) setHoursFromDate();
			updateValue();
		}

		if (self.config.weekNumbers) {
			self.calendarContainer.style.width = self.days.clientWidth + self.weekWrapper.clientWidth + "px";
		}

		self.showTimeInput = self.selectedDates.length || self.config.noCalendar;

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

		if (!self.config.time_24hr && self.selectedDates.length) self.amPM.textContent = self.latestSelectedDateObj.getHours() >= 12 ? "PM" : "AM";

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

		if (!self.config.static && self.config.allowInput) (self.altInput || self.input).addEventListener("keydown", onKeyDown);

		if (!self.config.inline && !self.config.static) window.addEventListener("resize", self.debouncedResize);

		if (window.ontouchstart) window.document.addEventListener("touchstart", documentClick);

		window.document.addEventListener("click", documentClick);
		window.document.addEventListener("blur", documentClick);

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

		if (typeof Event !== "undefined") {
			var ev = new Event("increment", { "bubbles": true });
			ev.delta = delta;
			input.dispatchEvent(ev);
		} else {
			var _ev = window.document.createEvent("CustomEvent");
			_ev.initCustomEvent("increment", true, true, {});
			_ev.delta = delta;
			input.dispatchEvent(_ev);
		}
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
			positionCalendar();

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
			dayElement.tabIndex = 0;

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

		self.hourElement.tabIndex = self.minuteElement.tabIndex = 0;

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
			self.amPM.tabIndex = 0;
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

		return self.calendarContainer.contains(elem);
	}

	function documentClick(e) {
		var isInput = self.element.contains(e.target) || e.target === self.input || e.target === self.altInput ||
		// web components
		e.path && e.path.indexOf && (~e.path.indexOf(self.input) || ~e.path.indexOf(self.altInput));

		if (self.isOpen && !self.config.inline && !isCalendarElem(e.target) && !isInput) {
			e.preventDefault();
			self.close();

			if (self.config.mode === "range" && self.selectedDates.length === 1) {
				self.clear();
				self.redraw();
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
			switch (e.which) {
				case 13:
					if (self.timeContainer && self.timeContainer.contains(e.target)) updateValue();else selectDate(e);

					break;

				case 27:
					// escape
					self.close();
					break;

				case 37:
					if (e.target !== self.input & e.target !== self.altInput) {
						e.preventDefault();
						changeMonth(-1);
						self.currentMonthElement.focus();
					}
					break;

				case 38:
					if (!self.timeContainer || !self.timeContainer.contains(e.target)) {
						e.preventDefault();
						self.currentYear++;
						self.redraw();
					} else updateTime(e);

					break;

				case 39:
					if (e.target !== self.input & e.target !== self.altInput) {
						e.preventDefault();
						changeMonth(1);
						self.currentMonthElement.focus();
					}
					break;

				case 40:
					if (!self.timeContainer || !self.timeContainer.contains(e.target)) {
						e.preventDefault();
						self.currentYear--;
						self.redraw();
					} else updateTime(e);

					break;

				default:
					break;

			}
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
		} else if (self.isOpen || (self.altInput || self.input).disabled || self.config.inline) return;

		self.calendarContainer.classList.add("open");

		if (!self.config.static && !self.config.inline) positionCalendar();

		self.isOpen = true;

		if (!self.config.allowInput) {
			(self.altInput || self.input).blur();
			(self.config.noCalendar ? self.timeContainer : self.selectedDateElem ? self.selectedDateElem : self.days).focus();
		}

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

		var hooks = ["onChange", "onClose", "onDayCreate", "onMonthChange", "onOpen", "onParseConfig", "onReady", "onValueUpdate", "onYearChange"];

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
				if (Array.isArray(self.config[key])) self.config[key] = arrayify(pluginConf[key]).map(bindToInstance).concat(self.config[key]);else if (typeof userConfig[key] === "undefined") self.config[key] = pluginConf[key];
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
		    showOnTop = configPos === "above" || configPos !== "below" && distanceFromBottom < calendarHeight + 60;

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

		if (self.config.allowInput && e.which === 13 && e.target === (self.altInput || self.input)) {
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

		if (e.which === 13 && self.config.enableTime) setTimeout(function () {
			return self.hourElement.focus();
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
			return d instanceof Date && d.getTime() && isEnabled(d, false);
		});

		self.selectedDates.sort(function (a, b) {
			return a.getTime() - b.getTime();
		});
	}

	function setDate(date, triggerChange, format) {
		if (!date) return self.clear();

		setSelectedDate(date, format);

		if (self.selectedDates.length > 0) {
			self.showTimeInput = true;
			self.latestSelectedDateObj = self.selectedDates[0];
		} else self.latestSelectedDateObj = null;

		self.redraw();
		jumpToDate();

		setHoursFromDate();
		updateValue();

		if (triggerChange !== false) triggerEvent("Change");
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
				set: function set(bool) {
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
			for (var i = 0; i < hooks.length; i++) {
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
			for (var _len = arguments.length, args = Array(_len), _key = 0; _key < _len; _key++) {
				args[_key] = arguments[_key];
			}

			var context = this;
			var later = function later() {
				timeout = null;
				if (!immediate) func.apply(context, args);
			};

			clearTimeout(timeout);
			timeout = setTimeout(later, wait);
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

		if (input.value.length === 2) {
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
	onYearChange: []
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
        this.fp = new _flatpickr2.default(this.$el.querySelector('input'), (0, _assign2.default)(this.options, {
            onValueUpdate: function onValueUpdate() {
                self.onInput(self.$el.querySelector('input').value);
                if (typeof origOnValUpdate === 'function') {
                    origOnValUpdate();
                }
            }
        }));
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
        }
    }
};
})()
if (module.exports.__esModule) module.exports = module.exports.default
var __vue__options__ = (typeof module.exports === "function"? module.exports.options: module.exports)
if (__vue__options__.functional) {console.error("[vueify] functional components are not supported and should be defined in plain js files using render functions.")}
__vue__options__.render = function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('div',{staticClass:"form-date input-group"},[_c('input',{class:_vm.inputClass,attrs:{"type":"text","placeholder":_vm.placeholder},domProps:{"value":_vm.value},on:{"input":_vm.onInput}}),_vm._v(" "),_vm._m(0)])}
__vue__options__.staticRenderFns = [function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('span',{staticClass:"input-group-addon"},[_c('span',{staticClass:"calendar"})])}]
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-717d849a", __vue__options__)
  } else {
    hotAPI.rerender("data-v-717d849a", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/json/stringify":1,"babel-runtime/core-js/object/assign":2,"flatpickr":40,"vue-hot-reload-api":41}],43:[function(require,module,exports){
(function (global){
;(function(){
"use strict";

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _stringify = require("babel-runtime/core-js/json/stringify");

var _stringify2 = _interopRequireDefault(_stringify);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

exports.default = {
    props: {
        filter: {
            type: Object,
            default: function _default() {
                return {};
            }
        },
        fetchUrl: {
            type: String
        }
    },
    data: function data() {
        return {
            table: null
        };
    },

    computed: {
        interviewFilters: function interviewFilters() {
            return (0, _stringify2.default)(this.filter);
        }
    },
    watch: {
        interviewFilters: function interviewFilters(newFilters) {
            this.table.ajax.reload();
        }
    },
    mounted: function mounted() {
        var self = this;
        this.table = $(this.$el).DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: this.fetchUrl,
                type: "POST",
                data: self.filter
            },
            searchHighlight: true,
            rowId: 'id',
            pagingType: "full_numbers",
            lengthChange: false,
            pageLength: 50,
            "order": [[2, 'desc']],
            dom: "frtp",
            conditionalPaging: true
        });
        this.$emit('DataTableRef', this.table);
    },
    destroyed: function destroyed() {}
};
})()
if (module.exports.__esModule) module.exports = module.exports.default
var __vue__options__ = (typeof module.exports === "function"? module.exports.options: module.exports)
if (__vue__options__.functional) {console.error("[vueify] functional components are not supported and should be defined in plain js files using render functions.")}
__vue__options__.render = function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _vm._m(0)}
__vue__options__.staticRenderFns = [function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('table',{staticClass:"table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews"},[_c('thead',[_c('tr',[_c('th',{staticClass:" interview-id title-row"},[_vm._v("\n                Interview id\n            ")]),_vm._v(" "),_c('th',{staticClass:"has-comments"},[_c('span',{staticClass:"comment-icon responded"})]),_vm._v(" "),_c('th',{staticClass:"uploaded-to-hq"},[_vm._v("Uploaded to HQ")]),_vm._v(" "),_c('th',{staticClass:"interview-conducted"},[_vm._v("interview conducted")]),_vm._v(" "),_c('th',{staticClass:"flags"},[_vm._v("Flags")]),_vm._v(" "),_c('th',{staticClass:"status"},[_vm._v("current Status")]),_vm._v(" "),_c('th',{staticClass:"answered-questions"},[_vm._v("answered questions")]),_vm._v(" "),_c('th',{staticClass:"left-empty"},[_vm._v("left empty")]),_vm._v(" "),_c('th',{staticClass:"errors"},[_vm._v("errors")]),_vm._v(" "),_c('th',{staticClass:"date last-update"},[_vm._v("last update")]),_vm._v(" "),_c('th',{staticClass:"download-on-device"},[_vm._v("downloaded on device")])])]),_vm._v(" "),_c('tbody')])}]
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-a35002b8", __vue__options__)
  } else {
    hotAPI.reload("data-v-a35002b8", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/json/stringify":1,"vue-hot-reload-api":41}],44:[function(require,module,exports){
(function (global){
;(function(){
'use strict';

module.exports = {
    name: 'user-selector',
    props: ['fetchUrl', 'controlId', 'value', 'placeholder'],
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

            console.log('filter: {filter}');
            this.isLoading = true;
            this.$http.get(this.fetchUrl, { params: { query: filter } }).then(function (response) {
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
    hotAPI.createRecord("data-v-11c7a1e1", __vue__options__)
  } else {
    hotAPI.rerender("data-v-11c7a1e1", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"vue-hot-reload-api":41}],45:[function(require,module,exports){
(function (global){
'use strict';

var _vue = (typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null);

var _vue2 = _interopRequireDefault(_vue);

var _vueResource = (typeof window !== "undefined" ? window['VueResource'] : typeof global !== "undefined" ? global['VueResource'] : null);

var _vueResource2 = _interopRequireDefault(_vueResource);

var _UserSelector = require('./UserSelector.vue');

var _UserSelector2 = _interopRequireDefault(_UserSelector);

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
_vue2.default.component("user-selector", _UserSelector2.default);
_vue2.default.component("interview-table", _InterviewTable2.default);

var app = new _vue2.default({
    data: {
        interviewerId: null,
        questionnaireId: null,
        changedFrom: null,
        changedTo: null,
        dateRangePickerOptions: {
            mode: "range",
            maxDate: "today",
            minDate: new Date().fp_incr(-30)
        },
        tableFilters: {}
    },
    computed: {},
    methods: {
        userSelected: function userSelected(newValue) {
            this.interviewerId = newValue;
        },
        questionnaireSelected: function questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },
        rangeSelected: function rangeSelected(textValue, from, to) {
            this.changedFrom = from;
            this.changedTo = to;
        },
        validateForm: function validateForm() {
            var _this = this;

            this.$validator.validateAll().then(function (result) {
                if (result) {
                    _this.findInterviews();
                }
            });
        },
        findInterviews: function findInterviews() {
            this.tableFilters = {
                interviewerId: this.interviewerId,
                questionnaireId: this.questionnaireId,
                changedFrom: this.changedFrom,
                changedTo: this.changedTo
            };
            document.querySelector("main").classList.remove("search-wasnt-started");
        }
    },
    mounted: function mounted() {
        document.querySelector("main").classList.remove("hold-transition");
    }
});

window.onload = function () {
    _vue2.default.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
};

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"./DatePicker.vue":42,"./InterviewTable.vue":43,"./UserSelector.vue":44}]},{},[45])
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvYmFiZWwtcnVudGltZS9jb3JlLWpzL2pzb24vc3RyaW5naWZ5LmpzIiwibm9kZV9tb2R1bGVzL2JhYmVsLXJ1bnRpbWUvY29yZS1qcy9vYmplY3QvYXNzaWduLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYS1mdW5jdGlvbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYW4tb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19hcnJheS1pbmNsdWRlcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY29mLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jb3JlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jdHguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2RlZmluZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2Rlc2NyaXB0b3JzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kb20tY3JlYXRlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19lbnVtLWJ1Zy1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19leHBvcnQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2ZhaWxzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19nbG9iYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2hhcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faGlkZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faWU4LWRvbS1kZWZpbmUuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lvYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lzLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWFzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWRwLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtZ29wcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWtleXMtaW50ZXJuYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtcGllLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19wcm9wZXJ0eS1kZXNjLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQta2V5LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLWluZGV4LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pbnRlZ2VyLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1sZW5ndGguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8tcHJpbWl0aXZlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL191aWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvZmxhdHBpY2tyL2Rpc3QvZmxhdHBpY2tyLmpzIiwibm9kZV9tb2R1bGVzL3Z1ZS1ob3QtcmVsb2FkLWFwaS9pbmRleC5qcyIsInZ1ZVxcdnVlXFxEYXRlUGlja2VyLnZ1ZT8zOTcyMDY0YSIsInZ1ZVxcdnVlXFxJbnRlcnZpZXdUYWJsZS52dWU/NzI5MTBlZWUiLCJ2dWVcXHZ1ZVxcVXNlclNlbGVjdG9yLnZ1ZT9mNWU2Y2E2NiIsInZ1ZVxcdnVlXFx0cm91Ymxlc2hvb3RpbmctY2Vuc3VzLmpzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0FDQUE7O0FDQUE7O0FDQUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBOztBQ0RBO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ3BCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7O0FDREE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNuQkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUM1REE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTkE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNQQTtBQUNBO0FBQ0E7O0FDRkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7O0FDRkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2hDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNmQTs7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2hCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTs7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1BBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNMQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUMzOURBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQzlIQTs7Ozs7OztBQUdBO0FBQ0E7QUFDQTtBQURBO0FBR0E7QUFDQTtBQUNBO0FBRkE7QUFJQTtBQUNBO0FBQ0E7QUFBQTtBQUFBO0FBRkE7QUFJQTtBQUNBO0FBQ0E7QUFGQTtBQVpBO0FBaUJBO0FBQ0E7QUFDQTtBQURBO0FBR0E7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFIQTtBQUtBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBTkE7QUFRQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQU5BO0FBUUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBTkE7QUFyREE7Ozs7O0FBWkE7QUFBQTs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDeUJBO0FBQ0E7QUFDQTtBQUNBO0FBQUE7QUFBQTtBQUZBO0FBSUE7QUFDQTtBQURBO0FBTEE7QUFTQTtBQUNBO0FBQ0E7QUFEQTtBQUdBOztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBSEE7QUFLQTtBQUNBO0FBQ0E7QUFDQTtBQUhBO0FBS0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBSEE7QUFLQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBZkE7QUFpQkE7QUFDQTtBQUNBO0FBOUNBOzs7OztBQXhCQTtBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDNkJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFIQTtBQUtBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQUE7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUFBOztBQUFBOztBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFoREE7QUE5QkE7Ozs7O0FBN0JBO0FBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ0FFOzs7O0FBQ0Y7Ozs7QUFDQTs7OztBQUNBOzs7O0FBQ0E7Ozs7QUFDQTs7Ozs7O0FBRUEsY0FBSSxHQUFKO0FBQ0EsY0FBSSxHQUFKOztBQUVBLGNBQUksU0FBSixDQUFjLFdBQWQ7QUFDQSxjQUFJLFNBQUosQ0FBYyxlQUFkO0FBQ0EsY0FBSSxTQUFKLENBQWMsaUJBQWQ7O0FBRUEsSUFBSSxNQUFNLGtCQUFRO0FBQ2QsVUFBTTtBQUNGLHVCQUFlLElBRGI7QUFFRix5QkFBaUIsSUFGZjtBQUdGLHFCQUFhLElBSFg7QUFJRixtQkFBVyxJQUpUO0FBS0YsZ0NBQXdCO0FBQ3BCLGtCQUFNLE9BRGM7QUFFcEIscUJBQVMsT0FGVztBQUdwQixxQkFBUyxJQUFJLElBQUosR0FBVyxPQUFYLENBQW1CLENBQUMsRUFBcEI7QUFIVyxTQUx0QjtBQVVGLHNCQUFjO0FBVlosS0FEUTtBQWFkLGNBQVUsRUFiSTtBQWVkLGFBQVM7QUFDTCxvQkFESyx3QkFDUSxRQURSLEVBQ2tCO0FBQ25CLGlCQUFLLGFBQUwsR0FBcUIsUUFBckI7QUFDSCxTQUhJO0FBSUwsNkJBSkssaUNBSWlCLFFBSmpCLEVBSTJCO0FBQzVCLGlCQUFLLGVBQUwsR0FBdUIsUUFBdkI7QUFDSCxTQU5JO0FBT0wscUJBUEsseUJBT1MsU0FQVCxFQU9vQixJQVBwQixFQU8wQixFQVAxQixFQU84QjtBQUMvQixpQkFBSyxXQUFMLEdBQW1CLElBQW5CO0FBQ0EsaUJBQUssU0FBTCxHQUFpQixFQUFqQjtBQUNILFNBVkk7QUFXTCxvQkFYSywwQkFXVTtBQUFBOztBQUNYLGlCQUFLLFVBQUwsQ0FBZ0IsV0FBaEIsR0FBOEIsSUFBOUIsQ0FBbUMsa0JBQVU7QUFDekMsb0JBQUksTUFBSixFQUFZO0FBQ1IsMEJBQUssY0FBTDtBQUNIO0FBQ0osYUFKRDtBQUtILFNBakJJO0FBa0JMLHNCQWxCSyw0QkFrQlk7QUFDYixpQkFBSyxZQUFMLEdBQW9CO0FBQ2hCLCtCQUFlLEtBQUssYUFESjtBQUVoQixpQ0FBaUIsS0FBSyxlQUZOO0FBR2hCLDZCQUFhLEtBQUssV0FIRjtBQUloQiwyQkFBVyxLQUFLO0FBSkEsYUFBcEI7QUFNQSxxQkFBUyxhQUFULENBQXVCLE1BQXZCLEVBQStCLFNBQS9CLENBQXlDLE1BQXpDLENBQWdELHNCQUFoRDtBQUNIO0FBMUJJLEtBZks7QUEyQ2QsYUFBUyxtQkFBVztBQUNoQixpQkFBUyxhQUFULENBQXVCLE1BQXZCLEVBQStCLFNBQS9CLENBQXlDLE1BQXpDLENBQWdELGlCQUFoRDtBQUNIO0FBN0NhLENBQVIsQ0FBVjs7QUFnREEsT0FBTyxNQUFQLEdBQWdCLFlBQVk7QUFDeEIsa0JBQUksSUFBSixDQUFTLE9BQVQsQ0FBaUIsTUFBakIsQ0FBd0IsZUFBeEIsSUFBMkMsTUFBTSxRQUFOLENBQWUsS0FBZixDQUFxQixLQUFoRTs7QUFFQSxRQUFJLE1BQUosQ0FBVyxNQUFYO0FBQ0gsQ0FKRCIsImZpbGUiOiJnZW5lcmF0ZWQuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlc0NvbnRlbnQiOlsiKGZ1bmN0aW9uIGUodCxuLHIpe2Z1bmN0aW9uIHMobyx1KXtpZighbltvXSl7aWYoIXRbb10pe3ZhciBhPXR5cGVvZiByZXF1aXJlPT1cImZ1bmN0aW9uXCImJnJlcXVpcmU7aWYoIXUmJmEpcmV0dXJuIGEobywhMCk7aWYoaSlyZXR1cm4gaShvLCEwKTt2YXIgZj1uZXcgRXJyb3IoXCJDYW5ub3QgZmluZCBtb2R1bGUgJ1wiK28rXCInXCIpO3Rocm93IGYuY29kZT1cIk1PRFVMRV9OT1RfRk9VTkRcIixmfXZhciBsPW5bb109e2V4cG9ydHM6e319O3Rbb11bMF0uY2FsbChsLmV4cG9ydHMsZnVuY3Rpb24oZSl7dmFyIG49dFtvXVsxXVtlXTtyZXR1cm4gcyhuP246ZSl9LGwsbC5leHBvcnRzLGUsdCxuLHIpfXJldHVybiBuW29dLmV4cG9ydHN9dmFyIGk9dHlwZW9mIHJlcXVpcmU9PVwiZnVuY3Rpb25cIiYmcmVxdWlyZTtmb3IodmFyIG89MDtvPHIubGVuZ3RoO28rKylzKHJbb10pO3JldHVybiBzfSkiLCJtb2R1bGUuZXhwb3J0cyA9IHsgXCJkZWZhdWx0XCI6IHJlcXVpcmUoXCJjb3JlLWpzL2xpYnJhcnkvZm4vanNvbi9zdHJpbmdpZnlcIiksIF9fZXNNb2R1bGU6IHRydWUgfTsiLCJtb2R1bGUuZXhwb3J0cyA9IHsgXCJkZWZhdWx0XCI6IHJlcXVpcmUoXCJjb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnblwiKSwgX19lc01vZHVsZTogdHJ1ZSB9OyIsInZhciBjb3JlICA9IHJlcXVpcmUoJy4uLy4uL21vZHVsZXMvX2NvcmUnKVxuICAsICRKU09OID0gY29yZS5KU09OIHx8IChjb3JlLkpTT04gPSB7c3RyaW5naWZ5OiBKU09OLnN0cmluZ2lmeX0pO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbiBzdHJpbmdpZnkoaXQpeyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVudXNlZC12YXJzXG4gIHJldHVybiAkSlNPTi5zdHJpbmdpZnkuYXBwbHkoJEpTT04sIGFyZ3VtZW50cyk7XG59OyIsInJlcXVpcmUoJy4uLy4uL21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24nKTtcbm1vZHVsZS5leHBvcnRzID0gcmVxdWlyZSgnLi4vLi4vbW9kdWxlcy9fY29yZScpLk9iamVjdC5hc3NpZ247IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIGlmKHR5cGVvZiBpdCAhPSAnZnVuY3Rpb24nKXRocm93IFR5cGVFcnJvcihpdCArICcgaXMgbm90IGEgZnVuY3Rpb24hJyk7XG4gIHJldHVybiBpdDtcbn07IiwidmFyIGlzT2JqZWN0ID0gcmVxdWlyZSgnLi9faXMtb2JqZWN0Jyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgaWYoIWlzT2JqZWN0KGl0KSl0aHJvdyBUeXBlRXJyb3IoaXQgKyAnIGlzIG5vdCBhbiBvYmplY3QhJyk7XG4gIHJldHVybiBpdDtcbn07IiwiLy8gZmFsc2UgLT4gQXJyYXkjaW5kZXhPZlxuLy8gdHJ1ZSAgLT4gQXJyYXkjaW5jbHVkZXNcbnZhciB0b0lPYmplY3QgPSByZXF1aXJlKCcuL190by1pb2JqZWN0JylcbiAgLCB0b0xlbmd0aCAgPSByZXF1aXJlKCcuL190by1sZW5ndGgnKVxuICAsIHRvSW5kZXggICA9IHJlcXVpcmUoJy4vX3RvLWluZGV4Jyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKElTX0lOQ0xVREVTKXtcbiAgcmV0dXJuIGZ1bmN0aW9uKCR0aGlzLCBlbCwgZnJvbUluZGV4KXtcbiAgICB2YXIgTyAgICAgID0gdG9JT2JqZWN0KCR0aGlzKVxuICAgICAgLCBsZW5ndGggPSB0b0xlbmd0aChPLmxlbmd0aClcbiAgICAgICwgaW5kZXggID0gdG9JbmRleChmcm9tSW5kZXgsIGxlbmd0aClcbiAgICAgICwgdmFsdWU7XG4gICAgLy8gQXJyYXkjaW5jbHVkZXMgdXNlcyBTYW1lVmFsdWVaZXJvIGVxdWFsaXR5IGFsZ29yaXRobVxuICAgIGlmKElTX0lOQ0xVREVTICYmIGVsICE9IGVsKXdoaWxlKGxlbmd0aCA+IGluZGV4KXtcbiAgICAgIHZhbHVlID0gT1tpbmRleCsrXTtcbiAgICAgIGlmKHZhbHVlICE9IHZhbHVlKXJldHVybiB0cnVlO1xuICAgIC8vIEFycmF5I3RvSW5kZXggaWdub3JlcyBob2xlcywgQXJyYXkjaW5jbHVkZXMgLSBub3RcbiAgICB9IGVsc2UgZm9yKDtsZW5ndGggPiBpbmRleDsgaW5kZXgrKylpZihJU19JTkNMVURFUyB8fCBpbmRleCBpbiBPKXtcbiAgICAgIGlmKE9baW5kZXhdID09PSBlbClyZXR1cm4gSVNfSU5DTFVERVMgfHwgaW5kZXggfHwgMDtcbiAgICB9IHJldHVybiAhSVNfSU5DTFVERVMgJiYgLTE7XG4gIH07XG59OyIsInZhciB0b1N0cmluZyA9IHt9LnRvU3RyaW5nO1xuXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIHRvU3RyaW5nLmNhbGwoaXQpLnNsaWNlKDgsIC0xKTtcbn07IiwidmFyIGNvcmUgPSBtb2R1bGUuZXhwb3J0cyA9IHt2ZXJzaW9uOiAnMi40LjAnfTtcbmlmKHR5cGVvZiBfX2UgPT0gJ251bWJlcicpX19lID0gY29yZTsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bmRlZiIsIi8vIG9wdGlvbmFsIC8gc2ltcGxlIGNvbnRleHQgYmluZGluZ1xudmFyIGFGdW5jdGlvbiA9IHJlcXVpcmUoJy4vX2EtZnVuY3Rpb24nKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oZm4sIHRoYXQsIGxlbmd0aCl7XG4gIGFGdW5jdGlvbihmbik7XG4gIGlmKHRoYXQgPT09IHVuZGVmaW5lZClyZXR1cm4gZm47XG4gIHN3aXRjaChsZW5ndGgpe1xuICAgIGNhc2UgMTogcmV0dXJuIGZ1bmN0aW9uKGEpe1xuICAgICAgcmV0dXJuIGZuLmNhbGwodGhhdCwgYSk7XG4gICAgfTtcbiAgICBjYXNlIDI6IHJldHVybiBmdW5jdGlvbihhLCBiKXtcbiAgICAgIHJldHVybiBmbi5jYWxsKHRoYXQsIGEsIGIpO1xuICAgIH07XG4gICAgY2FzZSAzOiByZXR1cm4gZnVuY3Rpb24oYSwgYiwgYyl7XG4gICAgICByZXR1cm4gZm4uY2FsbCh0aGF0LCBhLCBiLCBjKTtcbiAgICB9O1xuICB9XG4gIHJldHVybiBmdW5jdGlvbigvKiAuLi5hcmdzICovKXtcbiAgICByZXR1cm4gZm4uYXBwbHkodGhhdCwgYXJndW1lbnRzKTtcbiAgfTtcbn07IiwiLy8gNy4yLjEgUmVxdWlyZU9iamVjdENvZXJjaWJsZShhcmd1bWVudClcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICBpZihpdCA9PSB1bmRlZmluZWQpdGhyb3cgVHlwZUVycm9yKFwiQ2FuJ3QgY2FsbCBtZXRob2Qgb24gIFwiICsgaXQpO1xuICByZXR1cm4gaXQ7XG59OyIsIi8vIFRoYW5rJ3MgSUU4IGZvciBoaXMgZnVubnkgZGVmaW5lUHJvcGVydHlcbm1vZHVsZS5leHBvcnRzID0gIXJlcXVpcmUoJy4vX2ZhaWxzJykoZnVuY3Rpb24oKXtcbiAgcmV0dXJuIE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh7fSwgJ2EnLCB7Z2V0OiBmdW5jdGlvbigpeyByZXR1cm4gNzsgfX0pLmEgIT0gNztcbn0pOyIsInZhciBpc09iamVjdCA9IHJlcXVpcmUoJy4vX2lzLW9iamVjdCcpXG4gICwgZG9jdW1lbnQgPSByZXF1aXJlKCcuL19nbG9iYWwnKS5kb2N1bWVudFxuICAvLyBpbiBvbGQgSUUgdHlwZW9mIGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQgaXMgJ29iamVjdCdcbiAgLCBpcyA9IGlzT2JqZWN0KGRvY3VtZW50KSAmJiBpc09iamVjdChkb2N1bWVudC5jcmVhdGVFbGVtZW50KTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gaXMgPyBkb2N1bWVudC5jcmVhdGVFbGVtZW50KGl0KSA6IHt9O1xufTsiLCIvLyBJRSA4LSBkb24ndCBlbnVtIGJ1ZyBrZXlzXG5tb2R1bGUuZXhwb3J0cyA9IChcbiAgJ2NvbnN0cnVjdG9yLGhhc093blByb3BlcnR5LGlzUHJvdG90eXBlT2YscHJvcGVydHlJc0VudW1lcmFibGUsdG9Mb2NhbGVTdHJpbmcsdG9TdHJpbmcsdmFsdWVPZidcbikuc3BsaXQoJywnKTsiLCJ2YXIgZ2xvYmFsICAgID0gcmVxdWlyZSgnLi9fZ2xvYmFsJylcbiAgLCBjb3JlICAgICAgPSByZXF1aXJlKCcuL19jb3JlJylcbiAgLCBjdHggICAgICAgPSByZXF1aXJlKCcuL19jdHgnKVxuICAsIGhpZGUgICAgICA9IHJlcXVpcmUoJy4vX2hpZGUnKVxuICAsIFBST1RPVFlQRSA9ICdwcm90b3R5cGUnO1xuXG52YXIgJGV4cG9ydCA9IGZ1bmN0aW9uKHR5cGUsIG5hbWUsIHNvdXJjZSl7XG4gIHZhciBJU19GT1JDRUQgPSB0eXBlICYgJGV4cG9ydC5GXG4gICAgLCBJU19HTE9CQUwgPSB0eXBlICYgJGV4cG9ydC5HXG4gICAgLCBJU19TVEFUSUMgPSB0eXBlICYgJGV4cG9ydC5TXG4gICAgLCBJU19QUk9UTyAgPSB0eXBlICYgJGV4cG9ydC5QXG4gICAgLCBJU19CSU5EICAgPSB0eXBlICYgJGV4cG9ydC5CXG4gICAgLCBJU19XUkFQICAgPSB0eXBlICYgJGV4cG9ydC5XXG4gICAgLCBleHBvcnRzICAgPSBJU19HTE9CQUwgPyBjb3JlIDogY29yZVtuYW1lXSB8fCAoY29yZVtuYW1lXSA9IHt9KVxuICAgICwgZXhwUHJvdG8gID0gZXhwb3J0c1tQUk9UT1RZUEVdXG4gICAgLCB0YXJnZXQgICAgPSBJU19HTE9CQUwgPyBnbG9iYWwgOiBJU19TVEFUSUMgPyBnbG9iYWxbbmFtZV0gOiAoZ2xvYmFsW25hbWVdIHx8IHt9KVtQUk9UT1RZUEVdXG4gICAgLCBrZXksIG93biwgb3V0O1xuICBpZihJU19HTE9CQUwpc291cmNlID0gbmFtZTtcbiAgZm9yKGtleSBpbiBzb3VyY2Upe1xuICAgIC8vIGNvbnRhaW5zIGluIG5hdGl2ZVxuICAgIG93biA9ICFJU19GT1JDRUQgJiYgdGFyZ2V0ICYmIHRhcmdldFtrZXldICE9PSB1bmRlZmluZWQ7XG4gICAgaWYob3duICYmIGtleSBpbiBleHBvcnRzKWNvbnRpbnVlO1xuICAgIC8vIGV4cG9ydCBuYXRpdmUgb3IgcGFzc2VkXG4gICAgb3V0ID0gb3duID8gdGFyZ2V0W2tleV0gOiBzb3VyY2Vba2V5XTtcbiAgICAvLyBwcmV2ZW50IGdsb2JhbCBwb2xsdXRpb24gZm9yIG5hbWVzcGFjZXNcbiAgICBleHBvcnRzW2tleV0gPSBJU19HTE9CQUwgJiYgdHlwZW9mIHRhcmdldFtrZXldICE9ICdmdW5jdGlvbicgPyBzb3VyY2Vba2V5XVxuICAgIC8vIGJpbmQgdGltZXJzIHRvIGdsb2JhbCBmb3IgY2FsbCBmcm9tIGV4cG9ydCBjb250ZXh0XG4gICAgOiBJU19CSU5EICYmIG93biA/IGN0eChvdXQsIGdsb2JhbClcbiAgICAvLyB3cmFwIGdsb2JhbCBjb25zdHJ1Y3RvcnMgZm9yIHByZXZlbnQgY2hhbmdlIHRoZW0gaW4gbGlicmFyeVxuICAgIDogSVNfV1JBUCAmJiB0YXJnZXRba2V5XSA9PSBvdXQgPyAoZnVuY3Rpb24oQyl7XG4gICAgICB2YXIgRiA9IGZ1bmN0aW9uKGEsIGIsIGMpe1xuICAgICAgICBpZih0aGlzIGluc3RhbmNlb2YgQyl7XG4gICAgICAgICAgc3dpdGNoKGFyZ3VtZW50cy5sZW5ndGgpe1xuICAgICAgICAgICAgY2FzZSAwOiByZXR1cm4gbmV3IEM7XG4gICAgICAgICAgICBjYXNlIDE6IHJldHVybiBuZXcgQyhhKTtcbiAgICAgICAgICAgIGNhc2UgMjogcmV0dXJuIG5ldyBDKGEsIGIpO1xuICAgICAgICAgIH0gcmV0dXJuIG5ldyBDKGEsIGIsIGMpO1xuICAgICAgICB9IHJldHVybiBDLmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XG4gICAgICB9O1xuICAgICAgRltQUk9UT1RZUEVdID0gQ1tQUk9UT1RZUEVdO1xuICAgICAgcmV0dXJuIEY7XG4gICAgLy8gbWFrZSBzdGF0aWMgdmVyc2lvbnMgZm9yIHByb3RvdHlwZSBtZXRob2RzXG4gICAgfSkob3V0KSA6IElTX1BST1RPICYmIHR5cGVvZiBvdXQgPT0gJ2Z1bmN0aW9uJyA/IGN0eChGdW5jdGlvbi5jYWxsLCBvdXQpIDogb3V0O1xuICAgIC8vIGV4cG9ydCBwcm90byBtZXRob2RzIHRvIGNvcmUuJUNPTlNUUlVDVE9SJS5tZXRob2RzLiVOQU1FJVxuICAgIGlmKElTX1BST1RPKXtcbiAgICAgIChleHBvcnRzLnZpcnR1YWwgfHwgKGV4cG9ydHMudmlydHVhbCA9IHt9KSlba2V5XSA9IG91dDtcbiAgICAgIC8vIGV4cG9ydCBwcm90byBtZXRob2RzIHRvIGNvcmUuJUNPTlNUUlVDVE9SJS5wcm90b3R5cGUuJU5BTUUlXG4gICAgICBpZih0eXBlICYgJGV4cG9ydC5SICYmIGV4cFByb3RvICYmICFleHBQcm90b1trZXldKWhpZGUoZXhwUHJvdG8sIGtleSwgb3V0KTtcbiAgICB9XG4gIH1cbn07XG4vLyB0eXBlIGJpdG1hcFxuJGV4cG9ydC5GID0gMTsgICAvLyBmb3JjZWRcbiRleHBvcnQuRyA9IDI7ICAgLy8gZ2xvYmFsXG4kZXhwb3J0LlMgPSA0OyAgIC8vIHN0YXRpY1xuJGV4cG9ydC5QID0gODsgICAvLyBwcm90b1xuJGV4cG9ydC5CID0gMTY7ICAvLyBiaW5kXG4kZXhwb3J0LlcgPSAzMjsgIC8vIHdyYXBcbiRleHBvcnQuVSA9IDY0OyAgLy8gc2FmZVxuJGV4cG9ydC5SID0gMTI4OyAvLyByZWFsIHByb3RvIG1ldGhvZCBmb3IgYGxpYnJhcnlgIFxubW9kdWxlLmV4cG9ydHMgPSAkZXhwb3J0OyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oZXhlYyl7XG4gIHRyeSB7XG4gICAgcmV0dXJuICEhZXhlYygpO1xuICB9IGNhdGNoKGUpe1xuICAgIHJldHVybiB0cnVlO1xuICB9XG59OyIsIi8vIGh0dHBzOi8vZ2l0aHViLmNvbS96bG9pcm9jay9jb3JlLWpzL2lzc3Vlcy84NiNpc3N1ZWNvbW1lbnQtMTE1NzU5MDI4XG52YXIgZ2xvYmFsID0gbW9kdWxlLmV4cG9ydHMgPSB0eXBlb2Ygd2luZG93ICE9ICd1bmRlZmluZWQnICYmIHdpbmRvdy5NYXRoID09IE1hdGhcbiAgPyB3aW5kb3cgOiB0eXBlb2Ygc2VsZiAhPSAndW5kZWZpbmVkJyAmJiBzZWxmLk1hdGggPT0gTWF0aCA/IHNlbGYgOiBGdW5jdGlvbigncmV0dXJuIHRoaXMnKSgpO1xuaWYodHlwZW9mIF9fZyA9PSAnbnVtYmVyJylfX2cgPSBnbG9iYWw7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW5kZWYiLCJ2YXIgaGFzT3duUHJvcGVydHkgPSB7fS5oYXNPd25Qcm9wZXJ0eTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQsIGtleSl7XG4gIHJldHVybiBoYXNPd25Qcm9wZXJ0eS5jYWxsKGl0LCBrZXkpO1xufTsiLCJ2YXIgZFAgICAgICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1kcCcpXG4gICwgY3JlYXRlRGVzYyA9IHJlcXVpcmUoJy4vX3Byb3BlcnR5LWRlc2MnKTtcbm1vZHVsZS5leHBvcnRzID0gcmVxdWlyZSgnLi9fZGVzY3JpcHRvcnMnKSA/IGZ1bmN0aW9uKG9iamVjdCwga2V5LCB2YWx1ZSl7XG4gIHJldHVybiBkUC5mKG9iamVjdCwga2V5LCBjcmVhdGVEZXNjKDEsIHZhbHVlKSk7XG59IDogZnVuY3Rpb24ob2JqZWN0LCBrZXksIHZhbHVlKXtcbiAgb2JqZWN0W2tleV0gPSB2YWx1ZTtcbiAgcmV0dXJuIG9iamVjdDtcbn07IiwibW9kdWxlLmV4cG9ydHMgPSAhcmVxdWlyZSgnLi9fZGVzY3JpcHRvcnMnKSAmJiAhcmVxdWlyZSgnLi9fZmFpbHMnKShmdW5jdGlvbigpe1xuICByZXR1cm4gT2JqZWN0LmRlZmluZVByb3BlcnR5KHJlcXVpcmUoJy4vX2RvbS1jcmVhdGUnKSgnZGl2JyksICdhJywge2dldDogZnVuY3Rpb24oKXsgcmV0dXJuIDc7IH19KS5hICE9IDc7XG59KTsiLCIvLyBmYWxsYmFjayBmb3Igbm9uLWFycmF5LWxpa2UgRVMzIGFuZCBub24tZW51bWVyYWJsZSBvbGQgVjggc3RyaW5nc1xudmFyIGNvZiA9IHJlcXVpcmUoJy4vX2NvZicpO1xubW9kdWxlLmV4cG9ydHMgPSBPYmplY3QoJ3onKS5wcm9wZXJ0eUlzRW51bWVyYWJsZSgwKSA/IE9iamVjdCA6IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGNvZihpdCkgPT0gJ1N0cmluZycgPyBpdC5zcGxpdCgnJykgOiBPYmplY3QoaXQpO1xufTsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIHR5cGVvZiBpdCA9PT0gJ29iamVjdCcgPyBpdCAhPT0gbnVsbCA6IHR5cGVvZiBpdCA9PT0gJ2Z1bmN0aW9uJztcbn07IiwiJ3VzZSBzdHJpY3QnO1xuLy8gMTkuMS4yLjEgT2JqZWN0LmFzc2lnbih0YXJnZXQsIHNvdXJjZSwgLi4uKVxudmFyIGdldEtleXMgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWtleXMnKVxuICAsIGdPUFMgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWdvcHMnKVxuICAsIHBJRSAgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LXBpZScpXG4gICwgdG9PYmplY3QgPSByZXF1aXJlKCcuL190by1vYmplY3QnKVxuICAsIElPYmplY3QgID0gcmVxdWlyZSgnLi9faW9iamVjdCcpXG4gICwgJGFzc2lnbiAgPSBPYmplY3QuYXNzaWduO1xuXG4vLyBzaG91bGQgd29yayB3aXRoIHN5bWJvbHMgYW5kIHNob3VsZCBoYXZlIGRldGVybWluaXN0aWMgcHJvcGVydHkgb3JkZXIgKFY4IGJ1Zylcbm1vZHVsZS5leHBvcnRzID0gISRhc3NpZ24gfHwgcmVxdWlyZSgnLi9fZmFpbHMnKShmdW5jdGlvbigpe1xuICB2YXIgQSA9IHt9XG4gICAgLCBCID0ge31cbiAgICAsIFMgPSBTeW1ib2woKVxuICAgICwgSyA9ICdhYmNkZWZnaGlqa2xtbm9wcXJzdCc7XG4gIEFbU10gPSA3O1xuICBLLnNwbGl0KCcnKS5mb3JFYWNoKGZ1bmN0aW9uKGspeyBCW2tdID0gazsgfSk7XG4gIHJldHVybiAkYXNzaWduKHt9LCBBKVtTXSAhPSA3IHx8IE9iamVjdC5rZXlzKCRhc3NpZ24oe30sIEIpKS5qb2luKCcnKSAhPSBLO1xufSkgPyBmdW5jdGlvbiBhc3NpZ24odGFyZ2V0LCBzb3VyY2UpeyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVudXNlZC12YXJzXG4gIHZhciBUICAgICA9IHRvT2JqZWN0KHRhcmdldClcbiAgICAsIGFMZW4gID0gYXJndW1lbnRzLmxlbmd0aFxuICAgICwgaW5kZXggPSAxXG4gICAgLCBnZXRTeW1ib2xzID0gZ09QUy5mXG4gICAgLCBpc0VudW0gICAgID0gcElFLmY7XG4gIHdoaWxlKGFMZW4gPiBpbmRleCl7XG4gICAgdmFyIFMgICAgICA9IElPYmplY3QoYXJndW1lbnRzW2luZGV4KytdKVxuICAgICAgLCBrZXlzICAgPSBnZXRTeW1ib2xzID8gZ2V0S2V5cyhTKS5jb25jYXQoZ2V0U3ltYm9scyhTKSkgOiBnZXRLZXlzKFMpXG4gICAgICAsIGxlbmd0aCA9IGtleXMubGVuZ3RoXG4gICAgICAsIGogICAgICA9IDBcbiAgICAgICwga2V5O1xuICAgIHdoaWxlKGxlbmd0aCA+IGopaWYoaXNFbnVtLmNhbGwoUywga2V5ID0ga2V5c1tqKytdKSlUW2tleV0gPSBTW2tleV07XG4gIH0gcmV0dXJuIFQ7XG59IDogJGFzc2lnbjsiLCJ2YXIgYW5PYmplY3QgICAgICAgPSByZXF1aXJlKCcuL19hbi1vYmplY3QnKVxuICAsIElFOF9ET01fREVGSU5FID0gcmVxdWlyZSgnLi9faWU4LWRvbS1kZWZpbmUnKVxuICAsIHRvUHJpbWl0aXZlICAgID0gcmVxdWlyZSgnLi9fdG8tcHJpbWl0aXZlJylcbiAgLCBkUCAgICAgICAgICAgICA9IE9iamVjdC5kZWZpbmVQcm9wZXJ0eTtcblxuZXhwb3J0cy5mID0gcmVxdWlyZSgnLi9fZGVzY3JpcHRvcnMnKSA/IE9iamVjdC5kZWZpbmVQcm9wZXJ0eSA6IGZ1bmN0aW9uIGRlZmluZVByb3BlcnR5KE8sIFAsIEF0dHJpYnV0ZXMpe1xuICBhbk9iamVjdChPKTtcbiAgUCA9IHRvUHJpbWl0aXZlKFAsIHRydWUpO1xuICBhbk9iamVjdChBdHRyaWJ1dGVzKTtcbiAgaWYoSUU4X0RPTV9ERUZJTkUpdHJ5IHtcbiAgICByZXR1cm4gZFAoTywgUCwgQXR0cmlidXRlcyk7XG4gIH0gY2F0Y2goZSl7IC8qIGVtcHR5ICovIH1cbiAgaWYoJ2dldCcgaW4gQXR0cmlidXRlcyB8fCAnc2V0JyBpbiBBdHRyaWJ1dGVzKXRocm93IFR5cGVFcnJvcignQWNjZXNzb3JzIG5vdCBzdXBwb3J0ZWQhJyk7XG4gIGlmKCd2YWx1ZScgaW4gQXR0cmlidXRlcylPW1BdID0gQXR0cmlidXRlcy52YWx1ZTtcbiAgcmV0dXJuIE87XG59OyIsImV4cG9ydHMuZiA9IE9iamVjdC5nZXRPd25Qcm9wZXJ0eVN5bWJvbHM7IiwidmFyIGhhcyAgICAgICAgICA9IHJlcXVpcmUoJy4vX2hhcycpXG4gICwgdG9JT2JqZWN0ICAgID0gcmVxdWlyZSgnLi9fdG8taW9iamVjdCcpXG4gICwgYXJyYXlJbmRleE9mID0gcmVxdWlyZSgnLi9fYXJyYXktaW5jbHVkZXMnKShmYWxzZSlcbiAgLCBJRV9QUk9UTyAgICAgPSByZXF1aXJlKCcuL19zaGFyZWQta2V5JykoJ0lFX1BST1RPJyk7XG5cbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24ob2JqZWN0LCBuYW1lcyl7XG4gIHZhciBPICAgICAgPSB0b0lPYmplY3Qob2JqZWN0KVxuICAgICwgaSAgICAgID0gMFxuICAgICwgcmVzdWx0ID0gW11cbiAgICAsIGtleTtcbiAgZm9yKGtleSBpbiBPKWlmKGtleSAhPSBJRV9QUk9UTyloYXMoTywga2V5KSAmJiByZXN1bHQucHVzaChrZXkpO1xuICAvLyBEb24ndCBlbnVtIGJ1ZyAmIGhpZGRlbiBrZXlzXG4gIHdoaWxlKG5hbWVzLmxlbmd0aCA+IGkpaWYoaGFzKE8sIGtleSA9IG5hbWVzW2krK10pKXtcbiAgICB+YXJyYXlJbmRleE9mKHJlc3VsdCwga2V5KSB8fCByZXN1bHQucHVzaChrZXkpO1xuICB9XG4gIHJldHVybiByZXN1bHQ7XG59OyIsIi8vIDE5LjEuMi4xNCAvIDE1LjIuMy4xNCBPYmplY3Qua2V5cyhPKVxudmFyICRrZXlzICAgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWtleXMtaW50ZXJuYWwnKVxuICAsIGVudW1CdWdLZXlzID0gcmVxdWlyZSgnLi9fZW51bS1idWcta2V5cycpO1xuXG5tb2R1bGUuZXhwb3J0cyA9IE9iamVjdC5rZXlzIHx8IGZ1bmN0aW9uIGtleXMoTyl7XG4gIHJldHVybiAka2V5cyhPLCBlbnVtQnVnS2V5cyk7XG59OyIsImV4cG9ydHMuZiA9IHt9LnByb3BlcnR5SXNFbnVtZXJhYmxlOyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oYml0bWFwLCB2YWx1ZSl7XG4gIHJldHVybiB7XG4gICAgZW51bWVyYWJsZSAgOiAhKGJpdG1hcCAmIDEpLFxuICAgIGNvbmZpZ3VyYWJsZTogIShiaXRtYXAgJiAyKSxcbiAgICB3cml0YWJsZSAgICA6ICEoYml0bWFwICYgNCksXG4gICAgdmFsdWUgICAgICAgOiB2YWx1ZVxuICB9O1xufTsiLCJ2YXIgc2hhcmVkID0gcmVxdWlyZSgnLi9fc2hhcmVkJykoJ2tleXMnKVxuICAsIHVpZCAgICA9IHJlcXVpcmUoJy4vX3VpZCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihrZXkpe1xuICByZXR1cm4gc2hhcmVkW2tleV0gfHwgKHNoYXJlZFtrZXldID0gdWlkKGtleSkpO1xufTsiLCJ2YXIgZ2xvYmFsID0gcmVxdWlyZSgnLi9fZ2xvYmFsJylcbiAgLCBTSEFSRUQgPSAnX19jb3JlLWpzX3NoYXJlZF9fJ1xuICAsIHN0b3JlICA9IGdsb2JhbFtTSEFSRURdIHx8IChnbG9iYWxbU0hBUkVEXSA9IHt9KTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oa2V5KXtcbiAgcmV0dXJuIHN0b3JlW2tleV0gfHwgKHN0b3JlW2tleV0gPSB7fSk7XG59OyIsInZhciB0b0ludGVnZXIgPSByZXF1aXJlKCcuL190by1pbnRlZ2VyJylcbiAgLCBtYXggICAgICAgPSBNYXRoLm1heFxuICAsIG1pbiAgICAgICA9IE1hdGgubWluO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpbmRleCwgbGVuZ3RoKXtcbiAgaW5kZXggPSB0b0ludGVnZXIoaW5kZXgpO1xuICByZXR1cm4gaW5kZXggPCAwID8gbWF4KGluZGV4ICsgbGVuZ3RoLCAwKSA6IG1pbihpbmRleCwgbGVuZ3RoKTtcbn07IiwiLy8gNy4xLjQgVG9JbnRlZ2VyXG52YXIgY2VpbCAgPSBNYXRoLmNlaWxcbiAgLCBmbG9vciA9IE1hdGguZmxvb3I7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGlzTmFOKGl0ID0gK2l0KSA/IDAgOiAoaXQgPiAwID8gZmxvb3IgOiBjZWlsKShpdCk7XG59OyIsIi8vIHRvIGluZGV4ZWQgb2JqZWN0LCB0b09iamVjdCB3aXRoIGZhbGxiYWNrIGZvciBub24tYXJyYXktbGlrZSBFUzMgc3RyaW5nc1xudmFyIElPYmplY3QgPSByZXF1aXJlKCcuL19pb2JqZWN0JylcbiAgLCBkZWZpbmVkID0gcmVxdWlyZSgnLi9fZGVmaW5lZCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBJT2JqZWN0KGRlZmluZWQoaXQpKTtcbn07IiwiLy8gNy4xLjE1IFRvTGVuZ3RoXG52YXIgdG9JbnRlZ2VyID0gcmVxdWlyZSgnLi9fdG8taW50ZWdlcicpXG4gICwgbWluICAgICAgID0gTWF0aC5taW47XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGl0ID4gMCA/IG1pbih0b0ludGVnZXIoaXQpLCAweDFmZmZmZmZmZmZmZmZmKSA6IDA7IC8vIHBvdygyLCA1MykgLSAxID09IDkwMDcxOTkyNTQ3NDA5OTFcbn07IiwiLy8gNy4xLjEzIFRvT2JqZWN0KGFyZ3VtZW50KVxudmFyIGRlZmluZWQgPSByZXF1aXJlKCcuL19kZWZpbmVkJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIE9iamVjdChkZWZpbmVkKGl0KSk7XG59OyIsIi8vIDcuMS4xIFRvUHJpbWl0aXZlKGlucHV0IFssIFByZWZlcnJlZFR5cGVdKVxudmFyIGlzT2JqZWN0ID0gcmVxdWlyZSgnLi9faXMtb2JqZWN0Jyk7XG4vLyBpbnN0ZWFkIG9mIHRoZSBFUzYgc3BlYyB2ZXJzaW9uLCB3ZSBkaWRuJ3QgaW1wbGVtZW50IEBAdG9QcmltaXRpdmUgY2FzZVxuLy8gYW5kIHRoZSBzZWNvbmQgYXJndW1lbnQgLSBmbGFnIC0gcHJlZmVycmVkIHR5cGUgaXMgYSBzdHJpbmdcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQsIFMpe1xuICBpZighaXNPYmplY3QoaXQpKXJldHVybiBpdDtcbiAgdmFyIGZuLCB2YWw7XG4gIGlmKFMgJiYgdHlwZW9mIChmbiA9IGl0LnRvU3RyaW5nKSA9PSAnZnVuY3Rpb24nICYmICFpc09iamVjdCh2YWwgPSBmbi5jYWxsKGl0KSkpcmV0dXJuIHZhbDtcbiAgaWYodHlwZW9mIChmbiA9IGl0LnZhbHVlT2YpID09ICdmdW5jdGlvbicgJiYgIWlzT2JqZWN0KHZhbCA9IGZuLmNhbGwoaXQpKSlyZXR1cm4gdmFsO1xuICBpZighUyAmJiB0eXBlb2YgKGZuID0gaXQudG9TdHJpbmcpID09ICdmdW5jdGlvbicgJiYgIWlzT2JqZWN0KHZhbCA9IGZuLmNhbGwoaXQpKSlyZXR1cm4gdmFsO1xuICB0aHJvdyBUeXBlRXJyb3IoXCJDYW4ndCBjb252ZXJ0IG9iamVjdCB0byBwcmltaXRpdmUgdmFsdWVcIik7XG59OyIsInZhciBpZCA9IDBcbiAgLCBweCA9IE1hdGgucmFuZG9tKCk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGtleSl7XG4gIHJldHVybiAnU3ltYm9sKCcuY29uY2F0KGtleSA9PT0gdW5kZWZpbmVkID8gJycgOiBrZXksICcpXycsICgrK2lkICsgcHgpLnRvU3RyaW5nKDM2KSk7XG59OyIsIi8vIDE5LjEuMy4xIE9iamVjdC5hc3NpZ24odGFyZ2V0LCBzb3VyY2UpXG52YXIgJGV4cG9ydCA9IHJlcXVpcmUoJy4vX2V4cG9ydCcpO1xuXG4kZXhwb3J0KCRleHBvcnQuUyArICRleHBvcnQuRiwgJ09iamVjdCcsIHthc3NpZ246IHJlcXVpcmUoJy4vX29iamVjdC1hc3NpZ24nKX0pOyIsInZhciBfZXh0ZW5kcyA9IE9iamVjdC5hc3NpZ24gfHwgZnVuY3Rpb24gKHRhcmdldCkgeyBmb3IgKHZhciBpID0gMTsgaSA8IGFyZ3VtZW50cy5sZW5ndGg7IGkrKykgeyB2YXIgc291cmNlID0gYXJndW1lbnRzW2ldOyBmb3IgKHZhciBrZXkgaW4gc291cmNlKSB7IGlmIChPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwoc291cmNlLCBrZXkpKSB7IHRhcmdldFtrZXldID0gc291cmNlW2tleV07IH0gfSB9IHJldHVybiB0YXJnZXQ7IH07XG5cbnZhciBfdHlwZW9mID0gdHlwZW9mIFN5bWJvbCA9PT0gXCJmdW5jdGlvblwiICYmIHR5cGVvZiBTeW1ib2wuaXRlcmF0b3IgPT09IFwic3ltYm9sXCIgPyBmdW5jdGlvbiAob2JqKSB7IHJldHVybiB0eXBlb2Ygb2JqOyB9IDogZnVuY3Rpb24gKG9iaikgeyByZXR1cm4gb2JqICYmIHR5cGVvZiBTeW1ib2wgPT09IFwiZnVuY3Rpb25cIiAmJiBvYmouY29uc3RydWN0b3IgPT09IFN5bWJvbCAmJiBvYmogIT09IFN5bWJvbC5wcm90b3R5cGUgPyBcInN5bWJvbFwiIDogdHlwZW9mIG9iajsgfTtcblxuLyohIGZsYXRwaWNrciB2Mi40LjQsIEBsaWNlbnNlIE1JVCAqL1xuZnVuY3Rpb24gRmxhdHBpY2tyKGVsZW1lbnQsIGNvbmZpZykge1xuXHR2YXIgc2VsZiA9IHRoaXM7XG5cblx0c2VsZi5jaGFuZ2VNb250aCA9IGNoYW5nZU1vbnRoO1xuXHRzZWxmLmNoYW5nZVllYXIgPSBjaGFuZ2VZZWFyO1xuXHRzZWxmLmNsZWFyID0gY2xlYXI7XG5cdHNlbGYuY2xvc2UgPSBjbG9zZTtcblx0c2VsZi5fY3JlYXRlRWxlbWVudCA9IGNyZWF0ZUVsZW1lbnQ7XG5cdHNlbGYuZGVzdHJveSA9IGRlc3Ryb3k7XG5cdHNlbGYuZm9ybWF0RGF0ZSA9IGZvcm1hdERhdGU7XG5cdHNlbGYuaXNFbmFibGVkID0gaXNFbmFibGVkO1xuXHRzZWxmLmp1bXBUb0RhdGUgPSBqdW1wVG9EYXRlO1xuXHRzZWxmLm9wZW4gPSBvcGVuO1xuXHRzZWxmLnJlZHJhdyA9IHJlZHJhdztcblx0c2VsZi5zZXQgPSBzZXQ7XG5cdHNlbGYuc2V0RGF0ZSA9IHNldERhdGU7XG5cdHNlbGYudG9nZ2xlID0gdG9nZ2xlO1xuXG5cdGZ1bmN0aW9uIGluaXQoKSB7XG5cdFx0aWYgKGVsZW1lbnQuX2ZsYXRwaWNrcikgZGVzdHJveShlbGVtZW50Ll9mbGF0cGlja3IpO1xuXG5cdFx0ZWxlbWVudC5fZmxhdHBpY2tyID0gc2VsZjtcblxuXHRcdHNlbGYuZWxlbWVudCA9IGVsZW1lbnQ7XG5cdFx0c2VsZi5pbnN0YW5jZUNvbmZpZyA9IGNvbmZpZyB8fCB7fTtcblx0XHRzZWxmLnBhcnNlRGF0ZSA9IEZsYXRwaWNrci5wcm90b3R5cGUucGFyc2VEYXRlLmJpbmQoc2VsZik7XG5cblx0XHRzZXR1cEZvcm1hdHMoKTtcblx0XHRwYXJzZUNvbmZpZygpO1xuXHRcdHNldHVwTG9jYWxlKCk7XG5cdFx0c2V0dXBJbnB1dHMoKTtcblx0XHRzZXR1cERhdGVzKCk7XG5cdFx0c2V0dXBIZWxwZXJGdW5jdGlvbnMoKTtcblxuXHRcdHNlbGYuaXNPcGVuID0gc2VsZi5jb25maWcuaW5saW5lO1xuXG5cdFx0c2VsZi5pc01vYmlsZSA9ICFzZWxmLmNvbmZpZy5kaXNhYmxlTW9iaWxlICYmICFzZWxmLmNvbmZpZy5pbmxpbmUgJiYgc2VsZi5jb25maWcubW9kZSA9PT0gXCJzaW5nbGVcIiAmJiAhc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGggJiYgIXNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggJiYgIXNlbGYuY29uZmlnLndlZWtOdW1iZXJzICYmIC9BbmRyb2lkfHdlYk9TfGlQaG9uZXxpUGFkfGlQb2R8QmxhY2tCZXJyeXxJRU1vYmlsZXxPcGVyYSBNaW5pL2kudGVzdChuYXZpZ2F0b3IudXNlckFnZW50KTtcblxuXHRcdGlmICghc2VsZi5pc01vYmlsZSkgYnVpbGQoKTtcblxuXHRcdGJpbmQoKTtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSB7XG5cdFx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlVGltZSkgc2V0SG91cnNGcm9tRGF0ZSgpO1xuXHRcdFx0dXBkYXRlVmFsdWUoKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcud2Vla051bWJlcnMpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUud2lkdGggPSBzZWxmLmRheXMuY2xpZW50V2lkdGggKyBzZWxmLndlZWtXcmFwcGVyLmNsaWVudFdpZHRoICsgXCJweFwiO1xuXHRcdH1cblxuXHRcdHNlbGYuc2hvd1RpbWVJbnB1dCA9IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggfHwgc2VsZi5jb25maWcubm9DYWxlbmRhcjtcblxuXHRcdGlmICghc2VsZi5pc01vYmlsZSkgcG9zaXRpb25DYWxlbmRhcigpO1xuXHRcdHRyaWdnZXJFdmVudChcIlJlYWR5XCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gYmluZFRvSW5zdGFuY2UoZm4pIHtcblx0XHRpZiAoZm4gJiYgZm4uYmluZCkgcmV0dXJuIGZuLmJpbmQoc2VsZik7XG5cdFx0cmV0dXJuIGZuO1xuXHR9XG5cblx0ZnVuY3Rpb24gdXBkYXRlVGltZShlKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgJiYgIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpXG5cdFx0XHQvLyBwaWNraW5nIHRpbWUgb25seVxuXHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGYubm93XTtcblxuXHRcdHRpbWVXcmFwcGVyKGUpO1xuXG5cdFx0aWYgKCFzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSByZXR1cm47XG5cblx0XHRpZiAoIXNlbGYubWluRGF0ZUhhc1RpbWUgfHwgZS50eXBlICE9PSBcImlucHV0XCIgfHwgZS50YXJnZXQudmFsdWUubGVuZ3RoID49IDIpIHtcblx0XHRcdHNldEhvdXJzRnJvbUlucHV0cygpO1xuXHRcdFx0dXBkYXRlVmFsdWUoKTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0c2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNldEhvdXJzRnJvbUlucHV0cygpO1xuXHRcdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdFx0fSwgMTAwMCk7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gc2V0SG91cnNGcm9tSW5wdXRzKCkge1xuXHRcdGlmICghc2VsZi5jb25maWcuZW5hYmxlVGltZSkgcmV0dXJuO1xuXG5cdFx0dmFyIGhvdXJzID0gcGFyc2VJbnQoc2VsZi5ob3VyRWxlbWVudC52YWx1ZSwgMTApIHx8IDAsXG5cdFx0ICAgIG1pbnV0ZXMgPSBwYXJzZUludChzZWxmLm1pbnV0ZUVsZW1lbnQudmFsdWUsIDEwKSB8fCAwLFxuXHRcdCAgICBzZWNvbmRzID0gc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IHBhcnNlSW50KHNlbGYuc2Vjb25kRWxlbWVudC52YWx1ZSwgMTApIHx8IDAgOiAwO1xuXG5cdFx0aWYgKHNlbGYuYW1QTSkgaG91cnMgPSBob3VycyAlIDEyICsgMTIgKiAoc2VsZi5hbVBNLnRleHRDb250ZW50ID09PSBcIlBNXCIpO1xuXG5cdFx0aWYgKHNlbGYubWluRGF0ZUhhc1RpbWUgJiYgY29tcGFyZURhdGVzKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLCBzZWxmLmNvbmZpZy5taW5EYXRlKSA9PT0gMCkge1xuXG5cdFx0XHRob3VycyA9IE1hdGgubWF4KGhvdXJzLCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEhvdXJzKCkpO1xuXHRcdFx0aWYgKGhvdXJzID09PSBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEhvdXJzKCkpIG1pbnV0ZXMgPSBNYXRoLm1heChtaW51dGVzLCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldE1pbnV0ZXMoKSk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYubWF4RGF0ZUhhc1RpbWUgJiYgY29tcGFyZURhdGVzKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLCBzZWxmLmNvbmZpZy5tYXhEYXRlKSA9PT0gMCkge1xuXHRcdFx0aG91cnMgPSBNYXRoLm1pbihob3Vycywgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRIb3VycygpKTtcblx0XHRcdGlmIChob3VycyA9PT0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRIb3VycygpKSBtaW51dGVzID0gTWF0aC5taW4obWludXRlcywgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNaW51dGVzKCkpO1xuXHRcdH1cblxuXHRcdHNldEhvdXJzKGhvdXJzLCBtaW51dGVzLCBzZWNvbmRzKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldEhvdXJzRnJvbURhdGUoZGF0ZU9iaikge1xuXHRcdHZhciBkYXRlID0gZGF0ZU9iaiB8fCBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iajtcblxuXHRcdGlmIChkYXRlKSBzZXRIb3VycyhkYXRlLmdldEhvdXJzKCksIGRhdGUuZ2V0TWludXRlcygpLCBkYXRlLmdldFNlY29uZHMoKSk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXRIb3Vycyhob3VycywgbWludXRlcywgc2Vjb25kcykge1xuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSB7XG5cdFx0XHRzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5zZXRIb3Vycyhob3VycyAlIDI0LCBtaW51dGVzLCBzZWNvbmRzIHx8IDAsIDApO1xuXHRcdH1cblxuXHRcdGlmICghc2VsZi5jb25maWcuZW5hYmxlVGltZSB8fCBzZWxmLmlzTW9iaWxlKSByZXR1cm47XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoIXNlbGYuY29uZmlnLnRpbWVfMjRociA/ICgxMiArIGhvdXJzKSAlIDEyICsgMTIgKiAoaG91cnMgJSAxMiA9PT0gMCkgOiBob3Vycyk7XG5cblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZChtaW51dGVzKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcudGltZV8yNGhyICYmIHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHNlbGYuYW1QTS50ZXh0Q29udGVudCA9IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLmdldEhvdXJzKCkgPj0gMTIgPyBcIlBNXCIgOiBcIkFNXCI7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcykgc2VsZi5zZWNvbmRFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoc2Vjb25kcyk7XG5cdH1cblxuXHRmdW5jdGlvbiBvblllYXJJbnB1dChldmVudCkge1xuXHRcdHZhciB5ZWFyID0gZXZlbnQudGFyZ2V0LnZhbHVlO1xuXHRcdGlmIChldmVudC5kZWx0YSkgeWVhciA9IChwYXJzZUludCh5ZWFyKSArIGV2ZW50LmRlbHRhKS50b1N0cmluZygpO1xuXG5cdFx0aWYgKHllYXIubGVuZ3RoID09PSA0KSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5ibHVyKCk7XG5cdFx0XHRpZiAoIS9bXlxcZF0vLnRlc3QoeWVhcikpIGNoYW5nZVllYXIoeWVhcik7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gb25Nb250aFNjcm9sbChlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdHNlbGYuY2hhbmdlTW9udGgoTWF0aC5tYXgoLTEsIE1hdGgubWluKDEsIGUud2hlZWxEZWx0YSB8fCAtZS5kZWx0YVkpKSk7XG5cdH1cblxuXHRmdW5jdGlvbiBiaW5kKCkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy53cmFwKSB7XG5cdFx0XHRbXCJvcGVuXCIsIFwiY2xvc2VcIiwgXCJ0b2dnbGVcIiwgXCJjbGVhclwiXS5mb3JFYWNoKGZ1bmN0aW9uIChlbCkge1xuXHRcdFx0XHR2YXIgdG9nZ2xlcyA9IHNlbGYuZWxlbWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiW2RhdGEtXCIgKyBlbCArIFwiXVwiKTtcblx0XHRcdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCB0b2dnbGVzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRcdFx0dG9nZ2xlc1tpXS5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgc2VsZltlbF0pO1xuXHRcdFx0XHR9XG5cdFx0XHR9KTtcblx0XHR9XG5cblx0XHRpZiAod2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50ICE9PSB1bmRlZmluZWQpIHtcblx0XHRcdHNlbGYuY2hhbmdlRXZlbnQgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRXZlbnQoXCJIVE1MRXZlbnRzXCIpO1xuXHRcdFx0c2VsZi5jaGFuZ2VFdmVudC5pbml0RXZlbnQoXCJjaGFuZ2VcIiwgZmFsc2UsIHRydWUpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmlzTW9iaWxlKSByZXR1cm4gc2V0dXBNb2JpbGUoKTtcblxuXHRcdHNlbGYuZGVib3VuY2VkUmVzaXplID0gZGVib3VuY2Uob25SZXNpemUsIDUwKTtcblx0XHRzZWxmLnRyaWdnZXJDaGFuZ2UgPSBmdW5jdGlvbiAoKSB7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdFx0fTtcblx0XHRzZWxmLmRlYm91bmNlZENoYW5nZSA9IGRlYm91bmNlKHNlbGYudHJpZ2dlckNoYW5nZSwgMzAwKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIgJiYgc2VsZi5kYXlzKSBzZWxmLmRheXMuYWRkRXZlbnRMaXN0ZW5lcihcIm1vdXNlb3ZlclwiLCBvbk1vdXNlT3Zlcik7XG5cblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJrZXlkb3duXCIsIG9uS2V5RG93bik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLnN0YXRpYyAmJiBzZWxmLmNvbmZpZy5hbGxvd0lucHV0KSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5hZGRFdmVudExpc3RlbmVyKFwia2V5ZG93blwiLCBvbktleURvd24pO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5pbmxpbmUgJiYgIXNlbGYuY29uZmlnLnN0YXRpYykgd2luZG93LmFkZEV2ZW50TGlzdGVuZXIoXCJyZXNpemVcIiwgc2VsZi5kZWJvdW5jZWRSZXNpemUpO1xuXG5cdFx0aWYgKHdpbmRvdy5vbnRvdWNoc3RhcnQpIHdpbmRvdy5kb2N1bWVudC5hZGRFdmVudExpc3RlbmVyKFwidG91Y2hzdGFydFwiLCBkb2N1bWVudENsaWNrKTtcblxuXHRcdHdpbmRvdy5kb2N1bWVudC5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZG9jdW1lbnRDbGljayk7XG5cdFx0d2luZG93LmRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJibHVyXCIsIGRvY3VtZW50Q2xpY2spO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmNsaWNrT3BlbnMpIChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmFkZEV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBvcGVuKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcubm9DYWxlbmRhcikge1xuXHRcdFx0c2VsZi5wcmV2TW9udGhOYXYuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0cmV0dXJuIGNoYW5nZU1vbnRoKC0xKTtcblx0XHRcdH0pO1xuXHRcdFx0c2VsZi5uZXh0TW9udGhOYXYuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0cmV0dXJuIGNoYW5nZU1vbnRoKDEpO1xuXHRcdFx0fSk7XG5cblx0XHRcdHNlbGYuY3VycmVudE1vbnRoRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwid2hlZWxcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdFx0cmV0dXJuIGRlYm91bmNlKG9uTW9udGhTY3JvbGwoZSksIDUwKTtcblx0XHRcdH0pO1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIndoZWVsXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRcdHJldHVybiBkZWJvdW5jZSh5ZWFyU2Nyb2xsKGUpLCA1MCk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LnNlbGVjdCgpO1xuXHRcdFx0fSk7XG5cblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJpbnB1dFwiLCBvblllYXJJbnB1dCk7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiaW5jcmVtZW50XCIsIG9uWWVhcklucHV0KTtcblxuXHRcdFx0c2VsZi5kYXlzLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBzZWxlY3REYXRlKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlVGltZSkge1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJ0cmFuc2l0aW9uZW5kXCIsIHBvc2l0aW9uQ2FsZW5kYXIpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJ3aGVlbFwiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0XHRyZXR1cm4gZGVib3VuY2UodXBkYXRlVGltZShlKSwgNSk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwiaW5wdXRcIiwgdXBkYXRlVGltZSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImluY3JlbWVudFwiLCB1cGRhdGVUaW1lKTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwiaW5jcmVtZW50XCIsIHNlbGYuZGVib3VuY2VkQ2hhbmdlKTtcblxuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJ3aGVlbFwiLCBzZWxmLmRlYm91bmNlZENoYW5nZSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImlucHV0XCIsIHNlbGYudHJpZ2dlckNoYW5nZSk7XG5cblx0XHRcdHNlbGYuaG91ckVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0c2VsZi5ob3VyRWxlbWVudC5zZWxlY3QoKTtcblx0XHRcdH0pO1xuXHRcdFx0c2VsZi5taW51dGVFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNlbGYubWludXRlRWxlbWVudC5zZWxlY3QoKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRpZiAoc2VsZi5zZWNvbmRFbGVtZW50KSB7XG5cdFx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC5zZWxlY3QoKTtcblx0XHRcdFx0fSk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLmFtUE0pIHtcblx0XHRcdFx0c2VsZi5hbVBNLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0XHRcdHVwZGF0ZVRpbWUoZSk7XG5cdFx0XHRcdFx0c2VsZi50cmlnZ2VyQ2hhbmdlKGUpO1xuXHRcdFx0XHR9KTtcblx0XHRcdH1cblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBqdW1wVG9EYXRlKGp1bXBEYXRlKSB7XG5cdFx0anVtcERhdGUgPSBqdW1wRGF0ZSA/IHNlbGYucGFyc2VEYXRlKGp1bXBEYXRlKSA6IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqIHx8IChzZWxmLmNvbmZpZy5taW5EYXRlID4gc2VsZi5ub3cgPyBzZWxmLmNvbmZpZy5taW5EYXRlIDogc2VsZi5jb25maWcubWF4RGF0ZSAmJiBzZWxmLmNvbmZpZy5tYXhEYXRlIDwgc2VsZi5ub3cgPyBzZWxmLmNvbmZpZy5tYXhEYXRlIDogc2VsZi5ub3cpO1xuXG5cdFx0dHJ5IHtcblx0XHRcdHNlbGYuY3VycmVudFllYXIgPSBqdW1wRGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSBqdW1wRGF0ZS5nZXRNb250aCgpO1xuXHRcdH0gY2F0Y2ggKGUpIHtcblx0XHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0XHRjb25zb2xlLmVycm9yKGUuc3RhY2spO1xuXHRcdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRcdGNvbnNvbGUud2FybihcIkludmFsaWQgZGF0ZSBzdXBwbGllZDogXCIgKyBqdW1wRGF0ZSk7XG5cdFx0fVxuXG5cdFx0c2VsZi5yZWRyYXcoKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGluY3JlbWVudE51bUlucHV0KGUsIGRlbHRhLCBpbnB1dEVsZW0pIHtcblx0XHR2YXIgaW5wdXQgPSBpbnB1dEVsZW0gfHwgZS50YXJnZXQucGFyZW50Tm9kZS5jaGlsZE5vZGVzWzBdO1xuXG5cdFx0aWYgKHR5cGVvZiBFdmVudCAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHRcdFx0dmFyIGV2ID0gbmV3IEV2ZW50KFwiaW5jcmVtZW50XCIsIHsgXCJidWJibGVzXCI6IHRydWUgfSk7XG5cdFx0XHRldi5kZWx0YSA9IGRlbHRhO1xuXHRcdFx0aW5wdXQuZGlzcGF0Y2hFdmVudChldik7XG5cdFx0fSBlbHNlIHtcblx0XHRcdHZhciBfZXYgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRXZlbnQoXCJDdXN0b21FdmVudFwiKTtcblx0XHRcdF9ldi5pbml0Q3VzdG9tRXZlbnQoXCJpbmNyZW1lbnRcIiwgdHJ1ZSwgdHJ1ZSwge30pO1xuXHRcdFx0X2V2LmRlbHRhID0gZGVsdGE7XG5cdFx0XHRpbnB1dC5kaXNwYXRjaEV2ZW50KF9ldik7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gY3JlYXRlTnVtYmVySW5wdXQoaW5wdXRDbGFzc05hbWUpIHtcblx0XHR2YXIgd3JhcHBlciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJudW1JbnB1dFdyYXBwZXJcIiksXG5cdFx0ICAgIG51bUlucHV0ID0gY3JlYXRlRWxlbWVudChcImlucHV0XCIsIFwibnVtSW5wdXQgXCIgKyBpbnB1dENsYXNzTmFtZSksXG5cdFx0ICAgIGFycm93VXAgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImFycm93VXBcIiksXG5cdFx0ICAgIGFycm93RG93biA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiYXJyb3dEb3duXCIpO1xuXG5cdFx0bnVtSW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXHRcdG51bUlucHV0LnBhdHRlcm4gPSBcIlxcXFxkKlwiO1xuXHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQobnVtSW5wdXQpO1xuXHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoYXJyb3dVcCk7XG5cdFx0d3JhcHBlci5hcHBlbmRDaGlsZChhcnJvd0Rvd24pO1xuXG5cdFx0YXJyb3dVcC5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHJldHVybiBpbmNyZW1lbnROdW1JbnB1dChlLCAxKTtcblx0XHR9KTtcblx0XHRhcnJvd0Rvd24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRyZXR1cm4gaW5jcmVtZW50TnVtSW5wdXQoZSwgLTEpO1xuXHRcdH0pO1xuXHRcdHJldHVybiB3cmFwcGVyO1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGQoKSB7XG5cdFx0dmFyIGZyYWdtZW50ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZURvY3VtZW50RnJhZ21lbnQoKTtcblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1jYWxlbmRhclwiKTtcblx0XHRzZWxmLm51bUlucHV0VHlwZSA9IG5hdmlnYXRvci51c2VyQWdlbnQuaW5kZXhPZihcIk1TSUUgOS4wXCIpID4gMCA/IFwidGV4dFwiIDogXCJudW1iZXJcIjtcblxuXHRcdGlmICghc2VsZi5jb25maWcubm9DYWxlbmRhcikge1xuXHRcdFx0ZnJhZ21lbnQuYXBwZW5kQ2hpbGQoYnVpbGRNb250aE5hdigpKTtcblx0XHRcdHNlbGYuaW5uZXJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLWlubmVyQ29udGFpbmVyXCIpO1xuXG5cdFx0XHRpZiAoc2VsZi5jb25maWcud2Vla051bWJlcnMpIHNlbGYuaW5uZXJDb250YWluZXIuYXBwZW5kQ2hpbGQoYnVpbGRXZWVrcygpKTtcblxuXHRcdFx0c2VsZi5yQ29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1yQ29udGFpbmVyXCIpO1xuXHRcdFx0c2VsZi5yQ29udGFpbmVyLmFwcGVuZENoaWxkKGJ1aWxkV2Vla2RheXMoKSk7XG5cblx0XHRcdGlmICghc2VsZi5kYXlzKSB7XG5cdFx0XHRcdHNlbGYuZGF5cyA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItZGF5c1wiKTtcblx0XHRcdFx0c2VsZi5kYXlzLnRhYkluZGV4ID0gLTE7XG5cdFx0XHR9XG5cblx0XHRcdGJ1aWxkRGF5cygpO1xuXHRcdFx0c2VsZi5yQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlbGYuZGF5cyk7XG5cblx0XHRcdHNlbGYuaW5uZXJDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VsZi5yQ29udGFpbmVyKTtcblx0XHRcdGZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYuaW5uZXJDb250YWluZXIpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSBmcmFnbWVudC5hcHBlbmRDaGlsZChidWlsZFRpbWUoKSk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJyYW5nZU1vZGVcIik7XG5cblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmFwcGVuZENoaWxkKGZyYWdtZW50KTtcblxuXHRcdHZhciBjdXN0b21BcHBlbmQgPSBzZWxmLmNvbmZpZy5hcHBlbmRUbyAmJiBzZWxmLmNvbmZpZy5hcHBlbmRUby5ub2RlVHlwZTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUgfHwgc2VsZi5jb25maWcuc3RhdGljKSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoc2VsZi5jb25maWcuaW5saW5lID8gXCJpbmxpbmVcIiA6IFwic3RhdGljXCIpO1xuXHRcdFx0cG9zaXRpb25DYWxlbmRhcigpO1xuXG5cdFx0XHRpZiAoc2VsZi5jb25maWcuaW5saW5lICYmICFjdXN0b21BcHBlbmQpIHtcblx0XHRcdFx0cmV0dXJuIHNlbGYuZWxlbWVudC5wYXJlbnROb2RlLmluc2VydEJlZm9yZShzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5uZXh0U2libGluZyk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy5zdGF0aWMpIHtcblx0XHRcdFx0dmFyIHdyYXBwZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXdyYXBwZXJcIik7XG5cdFx0XHRcdHNlbGYuZWxlbWVudC5wYXJlbnROb2RlLmluc2VydEJlZm9yZSh3cmFwcGVyLCBzZWxmLmVsZW1lbnQpO1xuXHRcdFx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKHNlbGYuZWxlbWVudCk7XG5cblx0XHRcdFx0aWYgKHNlbGYuYWx0SW5wdXQpIHdyYXBwZXIuYXBwZW5kQ2hpbGQoc2VsZi5hbHRJbnB1dCk7XG5cblx0XHRcdFx0d3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLmNhbGVuZGFyQ29udGFpbmVyKTtcblx0XHRcdFx0cmV0dXJuO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdChjdXN0b21BcHBlbmQgPyBzZWxmLmNvbmZpZy5hcHBlbmRUbyA6IHdpbmRvdy5kb2N1bWVudC5ib2R5KS5hcHBlbmRDaGlsZChzZWxmLmNhbGVuZGFyQ29udGFpbmVyKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNyZWF0ZURheShjbGFzc05hbWUsIGRhdGUsIGRheU51bWJlcikge1xuXHRcdHZhciBkYXRlSXNFbmFibGVkID0gaXNFbmFibGVkKGRhdGUsIHRydWUpLFxuXHRcdCAgICBkYXlFbGVtZW50ID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItZGF5IFwiICsgY2xhc3NOYW1lLCBkYXRlLmdldERhdGUoKSk7XG5cblx0XHRkYXlFbGVtZW50LmRhdGVPYmogPSBkYXRlO1xuXG5cdFx0dG9nZ2xlQ2xhc3MoZGF5RWxlbWVudCwgXCJ0b2RheVwiLCBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5ub3cpID09PSAwKTtcblxuXHRcdGlmIChkYXRlSXNFbmFibGVkKSB7XG5cdFx0XHRkYXlFbGVtZW50LnRhYkluZGV4ID0gMDtcblxuXHRcdFx0aWYgKGlzRGF0ZVNlbGVjdGVkKGRhdGUpKSB7XG5cdFx0XHRcdGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcInNlbGVjdGVkXCIpO1xuXHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gPSBkYXlFbGVtZW50O1xuXHRcdFx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRcdFx0dG9nZ2xlQ2xhc3MoZGF5RWxlbWVudCwgXCJzdGFydFJhbmdlXCIsIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pID09PSAwKTtcblxuXHRcdFx0XHRcdHRvZ2dsZUNsYXNzKGRheUVsZW1lbnQsIFwiZW5kUmFuZ2VcIiwgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1sxXSkgPT09IDApO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cdFx0fSBlbHNlIHtcblx0XHRcdGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcImRpc2FibGVkXCIpO1xuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlc1swXSAmJiBkYXRlID4gc2VsZi5taW5SYW5nZURhdGUgJiYgZGF0ZSA8IHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgc2VsZi5taW5SYW5nZURhdGUgPSBkYXRlO2Vsc2UgaWYgKHNlbGYuc2VsZWN0ZWREYXRlc1swXSAmJiBkYXRlIDwgc2VsZi5tYXhSYW5nZURhdGUgJiYgZGF0ZSA+IHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgc2VsZi5tYXhSYW5nZURhdGUgPSBkYXRlO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdGlmIChpc0RhdGVJblJhbmdlKGRhdGUpICYmICFpc0RhdGVTZWxlY3RlZChkYXRlKSkgZGF5RWxlbWVudC5jbGFzc0xpc3QuYWRkKFwiaW5SYW5nZVwiKTtcblxuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEgJiYgKGRhdGUgPCBzZWxmLm1pblJhbmdlRGF0ZSB8fCBkYXRlID4gc2VsZi5tYXhSYW5nZURhdGUpKSBkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJub3RBbGxvd2VkXCIpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycyAmJiBjbGFzc05hbWUgIT09IFwicHJldk1vbnRoRGF5XCIgJiYgZGF5TnVtYmVyICUgNyA9PT0gMSkge1xuXHRcdFx0c2VsZi53ZWVrTnVtYmVycy5pbnNlcnRBZGphY2VudEhUTUwoXCJiZWZvcmVlbmRcIiwgXCI8c3BhbiBjbGFzcz0nZGlzYWJsZWQgZmxhdHBpY2tyLWRheSc+XCIgKyBzZWxmLmNvbmZpZy5nZXRXZWVrKGRhdGUpICsgXCI8L3NwYW4+XCIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIkRheUNyZWF0ZVwiLCBkYXlFbGVtZW50KTtcblxuXHRcdHJldHVybiBkYXlFbGVtZW50O1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGREYXlzKHllYXIsIG1vbnRoKSB7XG5cdFx0dmFyIGZpcnN0T2ZNb250aCA9IChuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCwgMSkuZ2V0RGF5KCkgLSBzZWxmLmwxMG4uZmlyc3REYXlPZldlZWsgKyA3KSAlIDcsXG5cdFx0ICAgIGlzUmFuZ2VNb2RlID0gc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiO1xuXG5cdFx0c2VsZi5wcmV2TW9udGhEYXlzID0gc2VsZi51dGlscy5nZXREYXlzaW5Nb250aCgoc2VsZi5jdXJyZW50TW9udGggLSAxICsgMTIpICUgMTIpO1xuXG5cdFx0dmFyIGRheXNJbk1vbnRoID0gc2VsZi51dGlscy5nZXREYXlzaW5Nb250aCgpLFxuXHRcdCAgICBkYXlzID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZURvY3VtZW50RnJhZ21lbnQoKTtcblxuXHRcdHZhciBkYXlOdW1iZXIgPSBzZWxmLnByZXZNb250aERheXMgKyAxIC0gZmlyc3RPZk1vbnRoO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzICYmIHNlbGYud2Vla051bWJlcnMuZmlyc3RDaGlsZCkgc2VsZi53ZWVrTnVtYmVycy50ZXh0Q29udGVudCA9IFwiXCI7XG5cblx0XHRpZiAoaXNSYW5nZU1vZGUpIHtcblx0XHRcdC8vIGNvbnN0IGRhdGVMaW1pdHMgPSBzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoIHx8IHNlbGYuY29uZmlnLmRpc2FibGUubGVuZ3RoIHx8IHNlbGYuY29uZmlnLm1peERhdGUgfHwgc2VsZi5jb25maWcubWF4RGF0ZTtcblx0XHRcdHNlbGYubWluUmFuZ2VEYXRlID0gbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggLSAxLCBkYXlOdW1iZXIpO1xuXHRcdFx0c2VsZi5tYXhSYW5nZURhdGUgPSBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsICg0MiAtIGZpcnN0T2ZNb250aCkgJSBkYXlzSW5Nb250aCk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuZGF5cy5maXJzdENoaWxkKSBzZWxmLmRheXMudGV4dENvbnRlbnQgPSBcIlwiO1xuXG5cdFx0Ly8gcHJlcGVuZCBkYXlzIGZyb20gdGhlIGVuZGluZyBvZiBwcmV2aW91cyBtb250aFxuXHRcdGZvciAoOyBkYXlOdW1iZXIgPD0gc2VsZi5wcmV2TW9udGhEYXlzOyBkYXlOdW1iZXIrKykge1xuXHRcdFx0ZGF5cy5hcHBlbmRDaGlsZChjcmVhdGVEYXkoXCJwcmV2TW9udGhEYXlcIiwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggLSAxLCBkYXlOdW1iZXIpLCBkYXlOdW1iZXIpKTtcblx0XHR9XG5cblx0XHQvLyBTdGFydCBhdCAxIHNpbmNlIHRoZXJlIGlzIG5vIDB0aCBkYXlcblx0XHRmb3IgKGRheU51bWJlciA9IDE7IGRheU51bWJlciA8PSBkYXlzSW5Nb250aDsgZGF5TnVtYmVyKyspIHtcblx0XHRcdGRheXMuYXBwZW5kQ2hpbGQoY3JlYXRlRGF5KFwiXCIsIG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoLCBkYXlOdW1iZXIpLCBkYXlOdW1iZXIpKTtcblx0XHR9XG5cblx0XHQvLyBhcHBlbmQgZGF5cyBmcm9tIHRoZSBuZXh0IG1vbnRoXG5cdFx0Zm9yICh2YXIgZGF5TnVtID0gZGF5c0luTW9udGggKyAxOyBkYXlOdW0gPD0gNDIgLSBmaXJzdE9mTW9udGg7IGRheU51bSsrKSB7XG5cdFx0XHRkYXlzLmFwcGVuZENoaWxkKGNyZWF0ZURheShcIm5leHRNb250aERheVwiLCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsIGRheU51bSAlIGRheXNJbk1vbnRoKSwgZGF5TnVtKSk7XG5cdFx0fVxuXG5cdFx0aWYgKGlzUmFuZ2VNb2RlICYmIHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEgJiYgZGF5cy5jaGlsZE5vZGVzWzBdKSB7XG5cdFx0XHRzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgfHwgc2VsZi5taW5SYW5nZURhdGUgPiBkYXlzLmNoaWxkTm9kZXNbMF0uZGF0ZU9iajtcblxuXHRcdFx0c2VsZi5faGlkZU5leHRNb250aEFycm93ID0gc2VsZi5faGlkZU5leHRNb250aEFycm93IHx8IHNlbGYubWF4UmFuZ2VEYXRlIDwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggKyAxLCAxKTtcblx0XHR9IGVsc2UgdXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXG5cdFx0c2VsZi5kYXlzLmFwcGVuZENoaWxkKGRheXMpO1xuXHRcdHJldHVybiBzZWxmLmRheXM7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZE1vbnRoTmF2KCkge1xuXHRcdHZhciBtb250aE5hdkZyYWdtZW50ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZURvY3VtZW50RnJhZ21lbnQoKTtcblx0XHRzZWxmLm1vbnRoTmF2ID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1tb250aFwiKTtcblxuXHRcdHNlbGYucHJldk1vbnRoTmF2ID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItcHJldi1tb250aFwiKTtcblx0XHRzZWxmLnByZXZNb250aE5hdi5pbm5lckhUTUwgPSBzZWxmLmNvbmZpZy5wcmV2QXJyb3c7XG5cblx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImN1ci1tb250aFwiKTtcblx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQudGl0bGUgPSBzZWxmLmwxMG4uc2Nyb2xsVGl0bGU7XG5cblx0XHR2YXIgeWVhcklucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJjdXIteWVhclwiKTtcblx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudCA9IHllYXJJbnB1dC5jaGlsZE5vZGVzWzBdO1xuXHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LnRpdGxlID0gc2VsZi5sMTBuLnNjcm9sbFRpdGxlO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1pbkRhdGUpIHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1pbiA9IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5tYXggPSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCk7XG5cblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmRpc2FibGVkID0gc2VsZi5jb25maWcubWluRGF0ZSAmJiBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHR9XG5cblx0XHRzZWxmLm5leHRNb250aE5hdiA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLW5leHQtbW9udGhcIik7XG5cdFx0c2VsZi5uZXh0TW9udGhOYXYuaW5uZXJIVE1MID0gc2VsZi5jb25maWcubmV4dEFycm93O1xuXG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItY3VycmVudC1tb250aFwiKTtcblx0XHRzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGguYXBwZW5kQ2hpbGQoc2VsZi5jdXJyZW50TW9udGhFbGVtZW50KTtcblx0XHRzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGguYXBwZW5kQ2hpbGQoeWVhcklucHV0KTtcblxuXHRcdG1vbnRoTmF2RnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5wcmV2TW9udGhOYXYpO1xuXHRcdG1vbnRoTmF2RnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoKTtcblx0XHRtb250aE5hdkZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYubmV4dE1vbnRoTmF2KTtcblx0XHRzZWxmLm1vbnRoTmF2LmFwcGVuZENoaWxkKG1vbnRoTmF2RnJhZ21lbnQpO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwiX2hpZGVQcmV2TW9udGhBcnJvd1wiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX19oaWRlUHJldk1vbnRoQXJyb3c7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRpZiAodGhpcy5fX2hpZGVQcmV2TW9udGhBcnJvdyAhPT0gYm9vbCkgc2VsZi5wcmV2TW9udGhOYXYuc3R5bGUuZGlzcGxheSA9IGJvb2wgPyBcIm5vbmVcIiA6IFwiYmxvY2tcIjtcblx0XHRcdFx0dGhpcy5fX2hpZGVQcmV2TW9udGhBcnJvdyA9IGJvb2w7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZiwgXCJfaGlkZU5leHRNb250aEFycm93XCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5fX2hpZGVOZXh0TW9udGhBcnJvdztcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChib29sKSB7XG5cdFx0XHRcdGlmICh0aGlzLl9faGlkZU5leHRNb250aEFycm93ICE9PSBib29sKSBzZWxmLm5leHRNb250aE5hdi5zdHlsZS5kaXNwbGF5ID0gYm9vbCA/IFwibm9uZVwiIDogXCJibG9ja1wiO1xuXHRcdFx0XHR0aGlzLl9faGlkZU5leHRNb250aEFycm93ID0gYm9vbDtcblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblxuXHRcdHJldHVybiBzZWxmLm1vbnRoTmF2O1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGRUaW1lKCkge1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcImhhc1RpbWVcIik7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcIm5vQ2FsZW5kYXJcIik7XG5cdFx0c2VsZi50aW1lQ29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci10aW1lXCIpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lci50YWJJbmRleCA9IC0xO1xuXHRcdHZhciBzZXBhcmF0b3IgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci10aW1lLXNlcGFyYXRvclwiLCBcIjpcIik7XG5cblx0XHR2YXIgaG91cklucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJmbGF0cGlja3ItaG91clwiKTtcblx0XHRzZWxmLmhvdXJFbGVtZW50ID0gaG91cklucHV0LmNoaWxkTm9kZXNbMF07XG5cblx0XHR2YXIgbWludXRlSW5wdXQgPSBjcmVhdGVOdW1iZXJJbnB1dChcImZsYXRwaWNrci1taW51dGVcIik7XG5cdFx0c2VsZi5taW51dGVFbGVtZW50ID0gbWludXRlSW5wdXQuY2hpbGROb2Rlc1swXTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudGFiSW5kZXggPSBzZWxmLm1pbnV0ZUVsZW1lbnQudGFiSW5kZXggPSAwO1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID8gc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouZ2V0SG91cnMoKSA6IHNlbGYuY29uZmlnLmRlZmF1bHRIb3VyKTtcblxuXHRcdHNlbGYubWludXRlRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID8gc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouZ2V0TWludXRlcygpIDogc2VsZi5jb25maWcuZGVmYXVsdE1pbnV0ZSk7XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnN0ZXAgPSBzZWxmLmNvbmZpZy5ob3VySW5jcmVtZW50O1xuXHRcdHNlbGYubWludXRlRWxlbWVudC5zdGVwID0gc2VsZi5jb25maWcubWludXRlSW5jcmVtZW50O1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC5taW4gPSBzZWxmLmNvbmZpZy50aW1lXzI0aHIgPyAwIDogMTtcblx0XHRzZWxmLmhvdXJFbGVtZW50Lm1heCA9IHNlbGYuY29uZmlnLnRpbWVfMjRociA/IDIzIDogMTI7XG5cblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQubWluID0gMDtcblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQubWF4ID0gNTk7XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnRpdGxlID0gc2VsZi5taW51dGVFbGVtZW50LnRpdGxlID0gc2VsZi5sMTBuLnNjcm9sbFRpdGxlO1xuXG5cdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKGhvdXJJbnB1dCk7XG5cdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlcGFyYXRvcik7XG5cdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKG1pbnV0ZUlucHV0KTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy50aW1lXzI0aHIpIHNlbGYudGltZUNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwidGltZTI0aHJcIik7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcykge1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJoYXNTZWNvbmRzXCIpO1xuXG5cdFx0XHR2YXIgc2Vjb25kSW5wdXQgPSBjcmVhdGVOdW1iZXJJbnB1dChcImZsYXRwaWNrci1zZWNvbmRcIik7XG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQgPSBzZWNvbmRJbnB1dC5jaGlsZE5vZGVzWzBdO1xuXG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQudmFsdWUgPSBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA/IHNlbGYucGFkKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLmdldFNlY29uZHMoKSkgOiBcIjAwXCI7XG5cblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC5zdGVwID0gc2VsZi5taW51dGVFbGVtZW50LnN0ZXA7XG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQubWluID0gc2VsZi5taW51dGVFbGVtZW50Lm1pbjtcblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC5tYXggPSBzZWxmLm1pbnV0ZUVsZW1lbnQubWF4O1xuXG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItdGltZS1zZXBhcmF0b3JcIiwgXCI6XCIpKTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChzZWNvbmRJbnB1dCk7XG5cdFx0fVxuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy50aW1lXzI0aHIpIHtcblx0XHRcdC8vIGFkZCBzZWxmLmFtUE0gaWYgYXBwcm9wcmlhdGVcblx0XHRcdHNlbGYuYW1QTSA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLWFtLXBtXCIsIFtcIkFNXCIsIFwiUE1cIl1bc2VsZi5ob3VyRWxlbWVudC52YWx1ZSA+IDExIHwgMF0pO1xuXHRcdFx0c2VsZi5hbVBNLnRpdGxlID0gc2VsZi5sMTBuLnRvZ2dsZVRpdGxlO1xuXHRcdFx0c2VsZi5hbVBNLnRhYkluZGV4ID0gMDtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChzZWxmLmFtUE0pO1xuXHRcdH1cblxuXHRcdHJldHVybiBzZWxmLnRpbWVDb250YWluZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZFdlZWtkYXlzKCkge1xuXHRcdGlmICghc2VsZi53ZWVrZGF5Q29udGFpbmVyKSBzZWxmLndlZWtkYXlDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXdlZWtkYXlzXCIpO1xuXG5cdFx0dmFyIGZpcnN0RGF5T2ZXZWVrID0gc2VsZi5sMTBuLmZpcnN0RGF5T2ZXZWVrO1xuXHRcdHZhciB3ZWVrZGF5cyA9IHNlbGYubDEwbi53ZWVrZGF5cy5zaG9ydGhhbmQuc2xpY2UoKTtcblxuXHRcdGlmIChmaXJzdERheU9mV2VlayA+IDAgJiYgZmlyc3REYXlPZldlZWsgPCB3ZWVrZGF5cy5sZW5ndGgpIHtcblx0XHRcdHdlZWtkYXlzID0gW10uY29uY2F0KHdlZWtkYXlzLnNwbGljZShmaXJzdERheU9mV2Vlaywgd2Vla2RheXMubGVuZ3RoKSwgd2Vla2RheXMuc3BsaWNlKDAsIGZpcnN0RGF5T2ZXZWVrKSk7XG5cdFx0fVxuXG5cdFx0c2VsZi53ZWVrZGF5Q29udGFpbmVyLmlubmVySFRNTCA9IFwiXFxuXFx0XFx0PHNwYW4gY2xhc3M9ZmxhdHBpY2tyLXdlZWtkYXk+XFxuXFx0XFx0XFx0XCIgKyB3ZWVrZGF5cy5qb2luKFwiPC9zcGFuPjxzcGFuIGNsYXNzPWZsYXRwaWNrci13ZWVrZGF5PlwiKSArIFwiXFxuXFx0XFx0PC9zcGFuPlxcblxcdFxcdFwiO1xuXG5cdFx0cmV0dXJuIHNlbGYud2Vla2RheUNvbnRhaW5lcjtcblx0fVxuXG5cdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdGZ1bmN0aW9uIGJ1aWxkV2Vla3MoKSB7XG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwiaGFzV2Vla3NcIik7XG5cdFx0c2VsZi53ZWVrV3JhcHBlciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd2Vla3dyYXBwZXJcIik7XG5cdFx0c2VsZi53ZWVrV3JhcHBlci5hcHBlbmRDaGlsZChjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci13ZWVrZGF5XCIsIHNlbGYubDEwbi53ZWVrQWJicmV2aWF0aW9uKSk7XG5cdFx0c2VsZi53ZWVrTnVtYmVycyA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd2Vla3NcIik7XG5cdFx0c2VsZi53ZWVrV3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLndlZWtOdW1iZXJzKTtcblxuXHRcdHJldHVybiBzZWxmLndlZWtXcmFwcGVyO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2hhbmdlTW9udGgodmFsdWUsIGlzX29mZnNldCkge1xuXHRcdGlzX29mZnNldCA9IHR5cGVvZiBpc19vZmZzZXQgPT09IFwidW5kZWZpbmVkXCIgfHwgaXNfb2Zmc2V0O1xuXHRcdHZhciBkZWx0YSA9IGlzX29mZnNldCA/IHZhbHVlIDogdmFsdWUgLSBzZWxmLmN1cnJlbnRNb250aDtcblxuXHRcdGlmIChkZWx0YSA8IDAgJiYgc2VsZi5faGlkZVByZXZNb250aEFycm93IHx8IGRlbHRhID4gMCAmJiBzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cpIHJldHVybjtcblxuXHRcdHNlbGYuY3VycmVudE1vbnRoICs9IGRlbHRhO1xuXG5cdFx0aWYgKHNlbGYuY3VycmVudE1vbnRoIDwgMCB8fCBzZWxmLmN1cnJlbnRNb250aCA+IDExKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyICs9IHNlbGYuY3VycmVudE1vbnRoID4gMTEgPyAxIDogLTE7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IChzZWxmLmN1cnJlbnRNb250aCArIDEyKSAlIDEyO1xuXG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJZZWFyQ2hhbmdlXCIpO1xuXHRcdH1cblxuXHRcdHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblx0XHRidWlsZERheXMoKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcubm9DYWxlbmRhcikgc2VsZi5kYXlzLmZvY3VzKCk7XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJNb250aENoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNsZWFyKHRyaWdnZXJDaGFuZ2VFdmVudCkge1xuXHRcdHNlbGYuaW5wdXQudmFsdWUgPSBcIlwiO1xuXG5cdFx0aWYgKHNlbGYuYWx0SW5wdXQpIHNlbGYuYWx0SW5wdXQudmFsdWUgPSBcIlwiO1xuXG5cdFx0aWYgKHNlbGYubW9iaWxlSW5wdXQpIHNlbGYubW9iaWxlSW5wdXQudmFsdWUgPSBcIlwiO1xuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gW107XG5cdFx0c2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBudWxsO1xuXHRcdHNlbGYuc2hvd1RpbWVJbnB1dCA9IGZhbHNlO1xuXG5cdFx0c2VsZi5yZWRyYXcoKTtcblxuXHRcdGlmICh0cmlnZ2VyQ2hhbmdlRXZlbnQgIT09IGZhbHNlKVxuXHRcdFx0Ly8gdHJpZ2dlckNoYW5nZUV2ZW50IGlzIHRydWUgKGRlZmF1bHQpIG9yIGFuIEV2ZW50XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBjbG9zZSgpIHtcblx0XHRzZWxmLmlzT3BlbiA9IGZhbHNlO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5yZW1vdmUoXCJvcGVuXCIpO1xuXHRcdFx0KHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuY2xhc3NMaXN0LnJlbW92ZShcImFjdGl2ZVwiKTtcblx0XHR9XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJDbG9zZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGRlc3Ryb3koaW5zdGFuY2UpIHtcblx0XHRpbnN0YW5jZSA9IGluc3RhbmNlIHx8IHNlbGY7XG5cdFx0aW5zdGFuY2UuY2xlYXIoZmFsc2UpO1xuXG5cdFx0d2luZG93LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJyZXNpemVcIiwgaW5zdGFuY2UuZGVib3VuY2VkUmVzaXplKTtcblxuXHRcdHdpbmRvdy5kb2N1bWVudC5yZW1vdmVFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZG9jdW1lbnRDbGljayk7XG5cdFx0d2luZG93LmRvY3VtZW50LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJ0b3VjaHN0YXJ0XCIsIGRvY3VtZW50Q2xpY2spO1xuXHRcdHdpbmRvdy5kb2N1bWVudC5yZW1vdmVFdmVudExpc3RlbmVyKFwiYmx1clwiLCBkb2N1bWVudENsaWNrKTtcblxuXHRcdGlmIChpbnN0YW5jZS50aW1lQ29udGFpbmVyKSBpbnN0YW5jZS50aW1lQ29udGFpbmVyLnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJ0cmFuc2l0aW9uZW5kXCIsIHBvc2l0aW9uQ2FsZW5kYXIpO1xuXG5cdFx0aWYgKGluc3RhbmNlLm1vYmlsZUlucHV0KSB7XG5cdFx0XHRpZiAoaW5zdGFuY2UubW9iaWxlSW5wdXQucGFyZW50Tm9kZSkgaW5zdGFuY2UubW9iaWxlSW5wdXQucGFyZW50Tm9kZS5yZW1vdmVDaGlsZChpbnN0YW5jZS5tb2JpbGVJbnB1dCk7XG5cdFx0XHRkZWxldGUgaW5zdGFuY2UubW9iaWxlSW5wdXQ7XG5cdFx0fSBlbHNlIGlmIChpbnN0YW5jZS5jYWxlbmRhckNvbnRhaW5lciAmJiBpbnN0YW5jZS5jYWxlbmRhckNvbnRhaW5lci5wYXJlbnROb2RlKSBpbnN0YW5jZS5jYWxlbmRhckNvbnRhaW5lci5wYXJlbnROb2RlLnJlbW92ZUNoaWxkKGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyKTtcblxuXHRcdGlmIChpbnN0YW5jZS5hbHRJbnB1dCkge1xuXHRcdFx0aW5zdGFuY2UuaW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXHRcdFx0aWYgKGluc3RhbmNlLmFsdElucHV0LnBhcmVudE5vZGUpIGluc3RhbmNlLmFsdElucHV0LnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoaW5zdGFuY2UuYWx0SW5wdXQpO1xuXHRcdFx0ZGVsZXRlIGluc3RhbmNlLmFsdElucHV0O1xuXHRcdH1cblxuXHRcdGluc3RhbmNlLmlucHV0LnR5cGUgPSBpbnN0YW5jZS5pbnB1dC5fdHlwZTtcblx0XHRpbnN0YW5jZS5pbnB1dC5jbGFzc0xpc3QucmVtb3ZlKFwiZmxhdHBpY2tyLWlucHV0XCIpO1xuXHRcdGluc3RhbmNlLmlucHV0LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBvcGVuKTtcblx0XHRpbnN0YW5jZS5pbnB1dC5yZW1vdmVBdHRyaWJ1dGUoXCJyZWFkb25seVwiKTtcblxuXHRcdGRlbGV0ZSBpbnN0YW5jZS5pbnB1dC5fZmxhdHBpY2tyO1xuXHR9XG5cblx0ZnVuY3Rpb24gaXNDYWxlbmRhckVsZW0oZWxlbSkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5hcHBlbmRUbyAmJiBzZWxmLmNvbmZpZy5hcHBlbmRUby5jb250YWlucyhlbGVtKSkgcmV0dXJuIHRydWU7XG5cblx0XHRyZXR1cm4gc2VsZi5jYWxlbmRhckNvbnRhaW5lci5jb250YWlucyhlbGVtKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGRvY3VtZW50Q2xpY2soZSkge1xuXHRcdHZhciBpc0lucHV0ID0gc2VsZi5lbGVtZW50LmNvbnRhaW5zKGUudGFyZ2V0KSB8fCBlLnRhcmdldCA9PT0gc2VsZi5pbnB1dCB8fCBlLnRhcmdldCA9PT0gc2VsZi5hbHRJbnB1dCB8fFxuXHRcdC8vIHdlYiBjb21wb25lbnRzXG5cdFx0ZS5wYXRoICYmIGUucGF0aC5pbmRleE9mICYmICh+ZS5wYXRoLmluZGV4T2Yoc2VsZi5pbnB1dCkgfHwgfmUucGF0aC5pbmRleE9mKHNlbGYuYWx0SW5wdXQpKTtcblxuXHRcdGlmIChzZWxmLmlzT3BlbiAmJiAhc2VsZi5jb25maWcuaW5saW5lICYmICFpc0NhbGVuZGFyRWxlbShlLnRhcmdldCkgJiYgIWlzSW5wdXQpIHtcblx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdHNlbGYuY2xvc2UoKTtcblxuXHRcdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIiAmJiBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID09PSAxKSB7XG5cdFx0XHRcdHNlbGYuY2xlYXIoKTtcblx0XHRcdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRcdH1cblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBmb3JtYXREYXRlKGZybXQsIGRhdGVPYmopIHtcblx0XHRpZiAoc2VsZi5jb25maWcuZm9ybWF0RGF0ZSkgcmV0dXJuIHNlbGYuY29uZmlnLmZvcm1hdERhdGUoZnJtdCwgZGF0ZU9iaik7XG5cblx0XHR2YXIgY2hhcnMgPSBmcm10LnNwbGl0KFwiXCIpO1xuXHRcdHJldHVybiBjaGFycy5tYXAoZnVuY3Rpb24gKGMsIGkpIHtcblx0XHRcdHJldHVybiBzZWxmLmZvcm1hdHNbY10gJiYgY2hhcnNbaSAtIDFdICE9PSBcIlxcXFxcIiA/IHNlbGYuZm9ybWF0c1tjXShkYXRlT2JqKSA6IGMgIT09IFwiXFxcXFwiID8gYyA6IFwiXCI7XG5cdFx0fSkuam9pbihcIlwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNoYW5nZVllYXIobmV3WWVhcikge1xuXHRcdGlmICghbmV3WWVhciB8fCBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5taW4gJiYgbmV3WWVhciA8IHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1pbiB8fCBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5tYXggJiYgbmV3WWVhciA+IHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1heCkgcmV0dXJuO1xuXG5cdFx0dmFyIG5ld1llYXJOdW0gPSBwYXJzZUludChuZXdZZWFyLCAxMCksXG5cdFx0ICAgIGlzTmV3WWVhciA9IHNlbGYuY3VycmVudFllYXIgIT09IG5ld1llYXJOdW07XG5cblx0XHRzZWxmLmN1cnJlbnRZZWFyID0gbmV3WWVhck51bSB8fCBzZWxmLmN1cnJlbnRZZWFyO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1heERhdGUgJiYgc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IE1hdGgubWluKHNlbGYuY29uZmlnLm1heERhdGUuZ2V0TW9udGgoKSwgc2VsZi5jdXJyZW50TW9udGgpO1xuXHRcdH0gZWxzZSBpZiAoc2VsZi5jb25maWcubWluRGF0ZSAmJiBzZWxmLmN1cnJlbnRZZWFyID09PSBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkpIHtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0gTWF0aC5tYXgoc2VsZi5jb25maWcubWluRGF0ZS5nZXRNb250aCgpLCBzZWxmLmN1cnJlbnRNb250aCk7XG5cdFx0fVxuXG5cdFx0aWYgKGlzTmV3WWVhcikge1xuXHRcdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIlllYXJDaGFuZ2VcIik7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gaXNFbmFibGVkKGRhdGUsIHRpbWVsZXNzKSB7XG5cdFx0dmFyIGx0bWluID0gY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuY29uZmlnLm1pbkRhdGUsIHR5cGVvZiB0aW1lbGVzcyAhPT0gXCJ1bmRlZmluZWRcIiA/IHRpbWVsZXNzIDogIXNlbGYubWluRGF0ZUhhc1RpbWUpIDwgMDtcblx0XHR2YXIgZ3RtYXggPSBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5jb25maWcubWF4RGF0ZSwgdHlwZW9mIHRpbWVsZXNzICE9PSBcInVuZGVmaW5lZFwiID8gdGltZWxlc3MgOiAhc2VsZi5tYXhEYXRlSGFzVGltZSkgPiAwO1xuXG5cdFx0aWYgKGx0bWluIHx8IGd0bWF4KSByZXR1cm4gZmFsc2U7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggJiYgIXNlbGYuY29uZmlnLmRpc2FibGUubGVuZ3RoKSByZXR1cm4gdHJ1ZTtcblxuXHRcdHZhciBkYXRlVG9DaGVjayA9IHNlbGYucGFyc2VEYXRlKGRhdGUsIHRydWUpOyAvLyB0aW1lbGVzc1xuXG5cdFx0dmFyIGJvb2wgPSBzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoID4gMCxcblx0XHQgICAgYXJyYXkgPSBib29sID8gc2VsZi5jb25maWcuZW5hYmxlIDogc2VsZi5jb25maWcuZGlzYWJsZTtcblxuXHRcdGZvciAodmFyIGkgPSAwLCBkOyBpIDwgYXJyYXkubGVuZ3RoOyBpKyspIHtcblx0XHRcdGQgPSBhcnJheVtpXTtcblxuXHRcdFx0aWYgKGQgaW5zdGFuY2VvZiBGdW5jdGlvbiAmJiBkKGRhdGVUb0NoZWNrKSkgLy8gZGlzYWJsZWQgYnkgZnVuY3Rpb25cblx0XHRcdFx0cmV0dXJuIGJvb2w7ZWxzZSBpZiAoZCBpbnN0YW5jZW9mIERhdGUgJiYgZC5nZXRUaW1lKCkgPT09IGRhdGVUb0NoZWNrLmdldFRpbWUoKSlcblx0XHRcdFx0Ly8gZGlzYWJsZWQgYnkgZGF0ZVxuXHRcdFx0XHRyZXR1cm4gYm9vbDtlbHNlIGlmICh0eXBlb2YgZCA9PT0gXCJzdHJpbmdcIiAmJiBzZWxmLnBhcnNlRGF0ZShkLCB0cnVlKS5nZXRUaW1lKCkgPT09IGRhdGVUb0NoZWNrLmdldFRpbWUoKSlcblx0XHRcdFx0Ly8gZGlzYWJsZWQgYnkgZGF0ZSBzdHJpbmdcblx0XHRcdFx0cmV0dXJuIGJvb2w7ZWxzZSBpZiAoIC8vIGRpc2FibGVkIGJ5IHJhbmdlXG5cdFx0XHQodHlwZW9mIGQgPT09IFwidW5kZWZpbmVkXCIgPyBcInVuZGVmaW5lZFwiIDogX3R5cGVvZihkKSkgPT09IFwib2JqZWN0XCIgJiYgZC5mcm9tICYmIGQudG8gJiYgZGF0ZVRvQ2hlY2sgPj0gZC5mcm9tICYmIGRhdGVUb0NoZWNrIDw9IGQudG8pIHJldHVybiBib29sO1xuXHRcdH1cblxuXHRcdHJldHVybiAhYm9vbDtcblx0fVxuXG5cdGZ1bmN0aW9uIG9uS2V5RG93bihlKSB7XG5cdFx0aWYgKGUudGFyZ2V0ID09PSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KSAmJiBlLndoaWNoID09PSAxMykgc2VsZWN0RGF0ZShlKTtlbHNlIGlmIChzZWxmLmlzT3BlbiB8fCBzZWxmLmNvbmZpZy5pbmxpbmUpIHtcblx0XHRcdHN3aXRjaCAoZS53aGljaCkge1xuXHRcdFx0XHRjYXNlIDEzOlxuXHRcdFx0XHRcdGlmIChzZWxmLnRpbWVDb250YWluZXIgJiYgc2VsZi50aW1lQ29udGFpbmVyLmNvbnRhaW5zKGUudGFyZ2V0KSkgdXBkYXRlVmFsdWUoKTtlbHNlIHNlbGVjdERhdGUoZSk7XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIDI3OlxuXHRcdFx0XHRcdC8vIGVzY2FwZVxuXHRcdFx0XHRcdHNlbGYuY2xvc2UoKTtcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIDM3OlxuXHRcdFx0XHRcdGlmIChlLnRhcmdldCAhPT0gc2VsZi5pbnB1dCAmIGUudGFyZ2V0ICE9PSBzZWxmLmFsdElucHV0KSB7XG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0XHRjaGFuZ2VNb250aCgtMSk7XG5cdFx0XHRcdFx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQuZm9jdXMoKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSAzODpcblx0XHRcdFx0XHRpZiAoIXNlbGYudGltZUNvbnRhaW5lciB8fCAhc2VsZi50aW1lQ29udGFpbmVyLmNvbnRhaW5zKGUudGFyZ2V0KSkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50WWVhcisrO1xuXHRcdFx0XHRcdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRcdFx0XHR9IGVsc2UgdXBkYXRlVGltZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgMzk6XG5cdFx0XHRcdFx0aWYgKGUudGFyZ2V0ICE9PSBzZWxmLmlucHV0ICYgZS50YXJnZXQgIT09IHNlbGYuYWx0SW5wdXQpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdGNoYW5nZU1vbnRoKDEpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LmZvY3VzKCk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgNDA6XG5cdFx0XHRcdFx0aWYgKCFzZWxmLnRpbWVDb250YWluZXIgfHwgIXNlbGYudGltZUNvbnRhaW5lci5jb250YWlucyhlLnRhcmdldCkpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdHNlbGYuY3VycmVudFllYXItLTtcblx0XHRcdFx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHRcdFx0fSBlbHNlIHVwZGF0ZVRpbWUoZSk7XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRkZWZhdWx0OlxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gb25Nb3VzZU92ZXIoZSkge1xuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoICE9PSAxIHx8ICFlLnRhcmdldC5jbGFzc0xpc3QuY29udGFpbnMoXCJmbGF0cGlja3ItZGF5XCIpKSByZXR1cm47XG5cblx0XHR2YXIgaG92ZXJEYXRlID0gZS50YXJnZXQuZGF0ZU9iaixcblx0XHQgICAgaW5pdGlhbERhdGUgPSBzZWxmLnBhcnNlRGF0ZShzZWxmLnNlbGVjdGVkRGF0ZXNbMF0sIHRydWUpLFxuXHRcdCAgICByYW5nZVN0YXJ0RGF0ZSA9IE1hdGgubWluKGhvdmVyRGF0ZS5nZXRUaW1lKCksIHNlbGYuc2VsZWN0ZWREYXRlc1swXS5nZXRUaW1lKCkpLFxuXHRcdCAgICByYW5nZUVuZERhdGUgPSBNYXRoLm1heChob3ZlckRhdGUuZ2V0VGltZSgpLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0uZ2V0VGltZSgpKSxcblx0XHQgICAgY29udGFpbnNEaXNhYmxlZCA9IGZhbHNlO1xuXG5cdFx0Zm9yICh2YXIgdCA9IHJhbmdlU3RhcnREYXRlOyB0IDwgcmFuZ2VFbmREYXRlOyB0ICs9IHNlbGYudXRpbHMuZHVyYXRpb24uREFZKSB7XG5cdFx0XHRpZiAoIWlzRW5hYmxlZChuZXcgRGF0ZSh0KSkpIHtcblx0XHRcdFx0Y29udGFpbnNEaXNhYmxlZCA9IHRydWU7XG5cdFx0XHRcdGJyZWFrO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHZhciBfbG9vcCA9IGZ1bmN0aW9uIF9sb29wKHRpbWVzdGFtcCwgaSkge1xuXHRcdFx0dmFyIG91dE9mUmFuZ2UgPSB0aW1lc3RhbXAgPCBzZWxmLm1pblJhbmdlRGF0ZS5nZXRUaW1lKCkgfHwgdGltZXN0YW1wID4gc2VsZi5tYXhSYW5nZURhdGUuZ2V0VGltZSgpO1xuXG5cdFx0XHRpZiAob3V0T2ZSYW5nZSkge1xuXHRcdFx0XHRzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QuYWRkKFwibm90QWxsb3dlZFwiKTtcblx0XHRcdFx0W1wiaW5SYW5nZVwiLCBcInN0YXJ0UmFuZ2VcIiwgXCJlbmRSYW5nZVwiXS5mb3JFYWNoKGZ1bmN0aW9uIChjKSB7XG5cdFx0XHRcdFx0c2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LnJlbW92ZShjKTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdHJldHVybiBcImNvbnRpbnVlXCI7XG5cdFx0XHR9IGVsc2UgaWYgKGNvbnRhaW5zRGlzYWJsZWQgJiYgIW91dE9mUmFuZ2UpIHJldHVybiBcImNvbnRpbnVlXCI7XG5cblx0XHRcdFtcInN0YXJ0UmFuZ2VcIiwgXCJpblJhbmdlXCIsIFwiZW5kUmFuZ2VcIiwgXCJub3RBbGxvd2VkXCJdLmZvckVhY2goZnVuY3Rpb24gKGMpIHtcblx0XHRcdFx0c2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LnJlbW92ZShjKTtcblx0XHRcdH0pO1xuXG5cdFx0XHR2YXIgbWluUmFuZ2VEYXRlID0gTWF0aC5tYXgoc2VsZi5taW5SYW5nZURhdGUuZ2V0VGltZSgpLCByYW5nZVN0YXJ0RGF0ZSksXG5cdFx0XHQgICAgbWF4UmFuZ2VEYXRlID0gTWF0aC5taW4oc2VsZi5tYXhSYW5nZURhdGUuZ2V0VGltZSgpLCByYW5nZUVuZERhdGUpO1xuXG5cdFx0XHRlLnRhcmdldC5jbGFzc0xpc3QuYWRkKGhvdmVyRGF0ZSA8IHNlbGYuc2VsZWN0ZWREYXRlc1swXSA/IFwic3RhcnRSYW5nZVwiIDogXCJlbmRSYW5nZVwiKTtcblxuXHRcdFx0aWYgKGluaXRpYWxEYXRlID4gaG92ZXJEYXRlICYmIHRpbWVzdGFtcCA9PT0gaW5pdGlhbERhdGUuZ2V0VGltZSgpKSBzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QuYWRkKFwiZW5kUmFuZ2VcIik7ZWxzZSBpZiAoaW5pdGlhbERhdGUgPCBob3ZlckRhdGUgJiYgdGltZXN0YW1wID09PSBpbml0aWFsRGF0ZS5nZXRUaW1lKCkpIHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5hZGQoXCJzdGFydFJhbmdlXCIpO2Vsc2UgaWYgKHRpbWVzdGFtcCA+PSBtaW5SYW5nZURhdGUgJiYgdGltZXN0YW1wIDw9IG1heFJhbmdlRGF0ZSkgc2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LmFkZChcImluUmFuZ2VcIik7XG5cdFx0fTtcblxuXHRcdGZvciAodmFyIHRpbWVzdGFtcCA9IHNlbGYuZGF5cy5jaGlsZE5vZGVzWzBdLmRhdGVPYmouZ2V0VGltZSgpLCBpID0gMDsgaSA8IDQyOyBpKyssIHRpbWVzdGFtcCArPSBzZWxmLnV0aWxzLmR1cmF0aW9uLkRBWSkge1xuXHRcdFx0dmFyIF9yZXQgPSBfbG9vcCh0aW1lc3RhbXAsIGkpO1xuXG5cdFx0XHRpZiAoX3JldCA9PT0gXCJjb250aW51ZVwiKSBjb250aW51ZTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBvblJlc2l6ZSgpIHtcblx0XHRpZiAoc2VsZi5pc09wZW4gJiYgIXNlbGYuY29uZmlnLnN0YXRpYyAmJiAhc2VsZi5jb25maWcuaW5saW5lKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBvcGVuKGUpIHtcblx0XHRpZiAoc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0aWYgKGUpIHtcblx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRlLnRhcmdldC5ibHVyKCk7XG5cdFx0XHR9XG5cblx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLm1vYmlsZUlucHV0LmNsaWNrKCk7XG5cdFx0XHR9LCAwKTtcblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiT3BlblwiKTtcblx0XHRcdHJldHVybjtcblx0XHR9IGVsc2UgaWYgKHNlbGYuaXNPcGVuIHx8IChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmRpc2FibGVkIHx8IHNlbGYuY29uZmlnLmlubGluZSkgcmV0dXJuO1xuXG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwib3BlblwiKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuc3RhdGljICYmICFzZWxmLmNvbmZpZy5pbmxpbmUpIHBvc2l0aW9uQ2FsZW5kYXIoKTtcblxuXHRcdHNlbGYuaXNPcGVuID0gdHJ1ZTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuYWxsb3dJbnB1dCkge1xuXHRcdFx0KHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuYmx1cigpO1xuXHRcdFx0KHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgPyBzZWxmLnRpbWVDb250YWluZXIgOiBzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gPyBzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gOiBzZWxmLmRheXMpLmZvY3VzKCk7XG5cdFx0fVxuXG5cdFx0KHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuY2xhc3NMaXN0LmFkZChcImFjdGl2ZVwiKTtcblx0XHR0cmlnZ2VyRXZlbnQoXCJPcGVuXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gbWluTWF4RGF0ZVNldHRlcih0eXBlKSB7XG5cdFx0cmV0dXJuIGZ1bmN0aW9uIChkYXRlKSB7XG5cdFx0XHR2YXIgZGF0ZU9iaiA9IHNlbGYuY29uZmlnW1wiX1wiICsgdHlwZSArIFwiRGF0ZVwiXSA9IHNlbGYucGFyc2VEYXRlKGRhdGUpO1xuXG5cdFx0XHR2YXIgaW52ZXJzZURhdGVPYmogPSBzZWxmLmNvbmZpZ1tcIl9cIiArICh0eXBlID09PSBcIm1pblwiID8gXCJtYXhcIiA6IFwibWluXCIpICsgXCJEYXRlXCJdO1xuXHRcdFx0dmFyIGlzVmFsaWREYXRlID0gZGF0ZSAmJiBkYXRlT2JqIGluc3RhbmNlb2YgRGF0ZTtcblxuXHRcdFx0aWYgKGlzVmFsaWREYXRlKSB7XG5cdFx0XHRcdHNlbGZbdHlwZSArIFwiRGF0ZUhhc1RpbWVcIl0gPSBkYXRlT2JqLmdldEhvdXJzKCkgfHwgZGF0ZU9iai5nZXRNaW51dGVzKCkgfHwgZGF0ZU9iai5nZXRTZWNvbmRzKCk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMpIHtcblx0XHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gc2VsZi5zZWxlY3RlZERhdGVzLmZpbHRlcihmdW5jdGlvbiAoZCkge1xuXHRcdFx0XHRcdHJldHVybiBpc0VuYWJsZWQoZCk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0XHRpZiAoIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggJiYgdHlwZSA9PT0gXCJtaW5cIikgc2V0SG91cnNGcm9tRGF0ZShkYXRlT2JqKTtcblx0XHRcdFx0dXBkYXRlVmFsdWUoKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuZGF5cykge1xuXHRcdFx0XHRyZWRyYXcoKTtcblxuXHRcdFx0XHRpZiAoaXNWYWxpZERhdGUpIHNlbGYuY3VycmVudFllYXJFbGVtZW50W3R5cGVdID0gZGF0ZU9iai5nZXRGdWxsWWVhcigpO2Vsc2Ugc2VsZi5jdXJyZW50WWVhckVsZW1lbnQucmVtb3ZlQXR0cmlidXRlKHR5cGUpO1xuXG5cdFx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmRpc2FibGVkID0gaW52ZXJzZURhdGVPYmogJiYgZGF0ZU9iaiAmJiBpbnZlcnNlRGF0ZU9iai5nZXRGdWxsWWVhcigpID09PSBkYXRlT2JqLmdldEZ1bGxZZWFyKCk7XG5cdFx0XHR9XG5cdFx0fTtcblx0fVxuXG5cdGZ1bmN0aW9uIHBhcnNlQ29uZmlnKCkge1xuXHRcdHZhciBib29sT3B0cyA9IFtcInV0Y1wiLCBcIndyYXBcIiwgXCJ3ZWVrTnVtYmVyc1wiLCBcImFsbG93SW5wdXRcIiwgXCJjbGlja09wZW5zXCIsIFwidGltZV8yNGhyXCIsIFwiZW5hYmxlVGltZVwiLCBcIm5vQ2FsZW5kYXJcIiwgXCJhbHRJbnB1dFwiLCBcInNob3J0aGFuZEN1cnJlbnRNb250aFwiLCBcImlubGluZVwiLCBcInN0YXRpY1wiLCBcImVuYWJsZVNlY29uZHNcIiwgXCJkaXNhYmxlTW9iaWxlXCJdO1xuXG5cdFx0dmFyIGhvb2tzID0gW1wib25DaGFuZ2VcIiwgXCJvbkNsb3NlXCIsIFwib25EYXlDcmVhdGVcIiwgXCJvbk1vbnRoQ2hhbmdlXCIsIFwib25PcGVuXCIsIFwib25QYXJzZUNvbmZpZ1wiLCBcIm9uUmVhZHlcIiwgXCJvblZhbHVlVXBkYXRlXCIsIFwib25ZZWFyQ2hhbmdlXCJdO1xuXG5cdFx0c2VsZi5jb25maWcgPSBPYmplY3QuY3JlYXRlKEZsYXRwaWNrci5kZWZhdWx0Q29uZmlnKTtcblxuXHRcdHZhciB1c2VyQ29uZmlnID0gX2V4dGVuZHMoe30sIHNlbGYuaW5zdGFuY2VDb25maWcsIEpTT04ucGFyc2UoSlNPTi5zdHJpbmdpZnkoc2VsZi5lbGVtZW50LmRhdGFzZXQgfHwge30pKSk7XG5cblx0XHRzZWxmLmNvbmZpZy5wYXJzZURhdGUgPSB1c2VyQ29uZmlnLnBhcnNlRGF0ZTtcblx0XHRzZWxmLmNvbmZpZy5mb3JtYXREYXRlID0gdXNlckNvbmZpZy5mb3JtYXREYXRlO1xuXG5cdFx0X2V4dGVuZHMoc2VsZi5jb25maWcsIHVzZXJDb25maWcpO1xuXG5cdFx0aWYgKCF1c2VyQ29uZmlnLmRhdGVGb3JtYXQgJiYgdXNlckNvbmZpZy5lbmFibGVUaW1lKSB7XG5cdFx0XHRzZWxmLmNvbmZpZy5kYXRlRm9ybWF0ID0gc2VsZi5jb25maWcubm9DYWxlbmRhciA/IFwiSDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpIDogRmxhdHBpY2tyLmRlZmF1bHRDb25maWcuZGF0ZUZvcm1hdCArIFwiIEg6aVwiICsgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBcIjpTXCIgOiBcIlwiKTtcblx0XHR9XG5cblx0XHRpZiAodXNlckNvbmZpZy5hbHRJbnB1dCAmJiB1c2VyQ29uZmlnLmVuYWJsZVRpbWUgJiYgIXVzZXJDb25maWcuYWx0Rm9ybWF0KSB7XG5cdFx0XHRzZWxmLmNvbmZpZy5hbHRGb3JtYXQgPSBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyID8gXCJoOmlcIiArIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gXCI6UyBLXCIgOiBcIiBLXCIpIDogRmxhdHBpY2tyLmRlZmF1bHRDb25maWcuYWx0Rm9ybWF0ICsgKFwiIGg6aVwiICsgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBcIjpTXCIgOiBcIlwiKSArIFwiIEtcIik7XG5cdFx0fVxuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYuY29uZmlnLCBcIm1pbkRhdGVcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiB0aGlzLl9taW5EYXRlO1xuXHRcdFx0fSxcblx0XHRcdHNldDogbWluTWF4RGF0ZVNldHRlcihcIm1pblwiKVxuXHRcdH0pO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYuY29uZmlnLCBcIm1heERhdGVcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiB0aGlzLl9tYXhEYXRlO1xuXHRcdFx0fSxcblx0XHRcdHNldDogbWluTWF4RGF0ZVNldHRlcihcIm1heFwiKVxuXHRcdH0pO1xuXG5cdFx0c2VsZi5jb25maWcubWluRGF0ZSA9IHVzZXJDb25maWcubWluRGF0ZTtcblx0XHRzZWxmLmNvbmZpZy5tYXhEYXRlID0gdXNlckNvbmZpZy5tYXhEYXRlO1xuXG5cdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCBib29sT3B0cy5sZW5ndGg7IGkrKykge1xuXHRcdFx0c2VsZi5jb25maWdbYm9vbE9wdHNbaV1dID0gc2VsZi5jb25maWdbYm9vbE9wdHNbaV1dID09PSB0cnVlIHx8IHNlbGYuY29uZmlnW2Jvb2xPcHRzW2ldXSA9PT0gXCJ0cnVlXCI7XG5cdFx0fWZvciAodmFyIF9pID0gMDsgX2kgPCBob29rcy5sZW5ndGg7IF9pKyspIHtcblx0XHRcdHNlbGYuY29uZmlnW2hvb2tzW19pXV0gPSBhcnJheWlmeShzZWxmLmNvbmZpZ1tob29rc1tfaV1dIHx8IFtdKS5tYXAoYmluZFRvSW5zdGFuY2UpO1xuXHRcdH1mb3IgKHZhciBfaTIgPSAwOyBfaTIgPCBzZWxmLmNvbmZpZy5wbHVnaW5zLmxlbmd0aDsgX2kyKyspIHtcblx0XHRcdHZhciBwbHVnaW5Db25mID0gc2VsZi5jb25maWcucGx1Z2luc1tfaTJdKHNlbGYpIHx8IHt9O1xuXHRcdFx0Zm9yICh2YXIga2V5IGluIHBsdWdpbkNvbmYpIHtcblx0XHRcdFx0aWYgKEFycmF5LmlzQXJyYXkoc2VsZi5jb25maWdba2V5XSkpIHNlbGYuY29uZmlnW2tleV0gPSBhcnJheWlmeShwbHVnaW5Db25mW2tleV0pLm1hcChiaW5kVG9JbnN0YW5jZSkuY29uY2F0KHNlbGYuY29uZmlnW2tleV0pO2Vsc2UgaWYgKHR5cGVvZiB1c2VyQ29uZmlnW2tleV0gPT09IFwidW5kZWZpbmVkXCIpIHNlbGYuY29uZmlnW2tleV0gPSBwbHVnaW5Db25mW2tleV07XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0dHJpZ2dlckV2ZW50KFwiUGFyc2VDb25maWdcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cExvY2FsZSgpIHtcblx0XHRpZiAoX3R5cGVvZihzZWxmLmNvbmZpZy5sb2NhbGUpICE9PSBcIm9iamVjdFwiICYmIHR5cGVvZiBGbGF0cGlja3IubDEwbnNbc2VsZi5jb25maWcubG9jYWxlXSA9PT0gXCJ1bmRlZmluZWRcIikgY29uc29sZS53YXJuKFwiZmxhdHBpY2tyOiBpbnZhbGlkIGxvY2FsZSBcIiArIHNlbGYuY29uZmlnLmxvY2FsZSk7XG5cblx0XHRzZWxmLmwxMG4gPSBfZXh0ZW5kcyhPYmplY3QuY3JlYXRlKEZsYXRwaWNrci5sMTBucy5kZWZhdWx0KSwgX3R5cGVvZihzZWxmLmNvbmZpZy5sb2NhbGUpID09PSBcIm9iamVjdFwiID8gc2VsZi5jb25maWcubG9jYWxlIDogc2VsZi5jb25maWcubG9jYWxlICE9PSBcImRlZmF1bHRcIiA/IEZsYXRwaWNrci5sMTBuc1tzZWxmLmNvbmZpZy5sb2NhbGVdIHx8IHt9IDoge30pO1xuXHR9XG5cblx0ZnVuY3Rpb24gcG9zaXRpb25DYWxlbmRhcihlKSB7XG5cdFx0aWYgKGUgJiYgZS50YXJnZXQgIT09IHNlbGYudGltZUNvbnRhaW5lcikgcmV0dXJuO1xuXG5cdFx0dmFyIGNhbGVuZGFySGVpZ2h0ID0gc2VsZi5jYWxlbmRhckNvbnRhaW5lci5vZmZzZXRIZWlnaHQsXG5cdFx0ICAgIGNhbGVuZGFyV2lkdGggPSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLm9mZnNldFdpZHRoLFxuXHRcdCAgICBjb25maWdQb3MgPSBzZWxmLmNvbmZpZy5wb3NpdGlvbixcblx0XHQgICAgaW5wdXQgPSBzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQsXG5cdFx0ICAgIGlucHV0Qm91bmRzID0gaW5wdXQuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCksXG5cdFx0ICAgIGRpc3RhbmNlRnJvbUJvdHRvbSA9IHdpbmRvdy5pbm5lckhlaWdodCAtIGlucHV0Qm91bmRzLmJvdHRvbSArIGlucHV0Lm9mZnNldEhlaWdodCxcblx0XHQgICAgc2hvd09uVG9wID0gY29uZmlnUG9zID09PSBcImFib3ZlXCIgfHwgY29uZmlnUG9zICE9PSBcImJlbG93XCIgJiYgZGlzdGFuY2VGcm9tQm90dG9tIDwgY2FsZW5kYXJIZWlnaHQgKyA2MDtcblxuXHRcdHZhciB0b3AgPSB3aW5kb3cucGFnZVlPZmZzZXQgKyBpbnB1dEJvdW5kcy50b3AgKyAoIXNob3dPblRvcCA/IGlucHV0Lm9mZnNldEhlaWdodCArIDIgOiAtY2FsZW5kYXJIZWlnaHQgLSAyKTtcblxuXHRcdHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwiYXJyb3dUb3BcIiwgIXNob3dPblRvcCk7XG5cdFx0dG9nZ2xlQ2xhc3Moc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgXCJhcnJvd0JvdHRvbVwiLCBzaG93T25Ub3ApO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmlubGluZSkgcmV0dXJuO1xuXG5cdFx0dmFyIGxlZnQgPSB3aW5kb3cucGFnZVhPZmZzZXQgKyBpbnB1dEJvdW5kcy5sZWZ0O1xuXHRcdHZhciByaWdodCA9IHdpbmRvdy5kb2N1bWVudC5ib2R5Lm9mZnNldFdpZHRoIC0gaW5wdXRCb3VuZHMucmlnaHQ7XG5cdFx0dmFyIHJpZ2h0TW9zdCA9IGxlZnQgKyBjYWxlbmRhcldpZHRoID4gd2luZG93LmRvY3VtZW50LmJvZHkub2Zmc2V0V2lkdGg7XG5cblx0XHR0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcInJpZ2h0TW9zdFwiLCByaWdodE1vc3QpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLnN0YXRpYykgcmV0dXJuO1xuXG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS50b3AgPSB0b3AgKyBcInB4XCI7XG5cblx0XHRpZiAoIXJpZ2h0TW9zdCkge1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS5sZWZ0ID0gbGVmdCArIFwicHhcIjtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUucmlnaHQgPSBcImF1dG9cIjtcblx0XHR9IGVsc2Uge1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS5sZWZ0ID0gXCJhdXRvXCI7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLnJpZ2h0ID0gcmlnaHQgKyBcInB4XCI7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gcmVkcmF3KCkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5ub0NhbGVuZGFyIHx8IHNlbGYuaXNNb2JpbGUpIHJldHVybjtcblxuXHRcdGJ1aWxkV2Vla2RheXMoKTtcblx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cdFx0YnVpbGREYXlzKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZWxlY3REYXRlKGUpIHtcblx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0ZS5zdG9wUHJvcGFnYXRpb24oKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5hbGxvd0lucHV0ICYmIGUud2hpY2ggPT09IDEzICYmIGUudGFyZ2V0ID09PSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KSkge1xuXHRcdFx0c2VsZi5zZXREYXRlKChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLnZhbHVlLCB0cnVlLCBlLnRhcmdldCA9PT0gc2VsZi5hbHRJbnB1dCA/IHNlbGYuY29uZmlnLmFsdEZvcm1hdCA6IHNlbGYuY29uZmlnLmRhdGVGb3JtYXQpO1xuXHRcdFx0cmV0dXJuIGUudGFyZ2V0LmJsdXIoKTtcblx0XHR9XG5cblx0XHRpZiAoIWUudGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcImZsYXRwaWNrci1kYXlcIikgfHwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwiZGlzYWJsZWRcIikgfHwgZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwibm90QWxsb3dlZFwiKSkgcmV0dXJuO1xuXG5cdFx0dmFyIHNlbGVjdGVkRGF0ZSA9IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gbmV3IERhdGUoZS50YXJnZXQuZGF0ZU9iai5nZXRUaW1lKCkpO1xuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVFbGVtID0gZS50YXJnZXQ7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJzaW5nbGVcIikgc2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGVjdGVkRGF0ZV07ZWxzZSBpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJtdWx0aXBsZVwiKSB7XG5cdFx0XHR2YXIgc2VsZWN0ZWRJbmRleCA9IGlzRGF0ZVNlbGVjdGVkKHNlbGVjdGVkRGF0ZSk7XG5cdFx0XHRpZiAoc2VsZWN0ZWRJbmRleCkgc2VsZi5zZWxlY3RlZERhdGVzLnNwbGljZShzZWxlY3RlZEluZGV4LCAxKTtlbHNlIHNlbGYuc2VsZWN0ZWREYXRlcy5wdXNoKHNlbGVjdGVkRGF0ZSk7XG5cdFx0fSBlbHNlIGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID09PSAyKSBzZWxmLmNsZWFyKCk7XG5cblx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcy5wdXNoKHNlbGVjdGVkRGF0ZSk7XG5cblx0XHRcdC8vIHVubGVzcyBzZWxlY3Rpbmcgc2FtZSBkYXRlIHR3aWNlLCBzb3J0IGFzY2VuZGluZ2x5XG5cdFx0XHRpZiAoY29tcGFyZURhdGVzKHNlbGVjdGVkRGF0ZSwgc2VsZi5zZWxlY3RlZERhdGVzWzBdLCB0cnVlKSAhPT0gMCkgc2VsZi5zZWxlY3RlZERhdGVzLnNvcnQoZnVuY3Rpb24gKGEsIGIpIHtcblx0XHRcdFx0cmV0dXJuIGEuZ2V0VGltZSgpIC0gYi5nZXRUaW1lKCk7XG5cdFx0XHR9KTtcblx0XHR9XG5cblx0XHRzZXRIb3Vyc0Zyb21JbnB1dHMoKTtcblxuXHRcdGlmIChzZWxlY3RlZERhdGUuZ2V0TW9udGgoKSAhPT0gc2VsZi5jdXJyZW50TW9udGggJiYgc2VsZi5jb25maWcubW9kZSAhPT0gXCJyYW5nZVwiKSB7XG5cdFx0XHR2YXIgaXNOZXdZZWFyID0gc2VsZi5jdXJyZW50WWVhciAhPT0gc2VsZWN0ZWREYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyID0gc2VsZWN0ZWREYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IHNlbGVjdGVkRGF0ZS5nZXRNb250aCgpO1xuXG5cdFx0XHRpZiAoaXNOZXdZZWFyKSB0cmlnZ2VyRXZlbnQoXCJZZWFyQ2hhbmdlXCIpO1xuXG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJNb250aENoYW5nZVwiKTtcblx0XHR9XG5cblx0XHRidWlsZERheXMoKTtcblxuXHRcdGlmIChzZWxmLm1pbkRhdGVIYXNUaW1lICYmIHNlbGYuY29uZmlnLmVuYWJsZVRpbWUgJiYgY29tcGFyZURhdGVzKHNlbGVjdGVkRGF0ZSwgc2VsZi5jb25maWcubWluRGF0ZSkgPT09IDApIHNldEhvdXJzRnJvbURhdGUoc2VsZi5jb25maWcubWluRGF0ZSk7XG5cblx0XHR1cGRhdGVWYWx1ZSgpO1xuXG5cdFx0c2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5zaG93VGltZUlucHV0ID0gdHJ1ZTtcblx0XHR9LCA1MCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSkge1xuXHRcdFx0XHRvbk1vdXNlT3ZlcihlKTtcblxuXHRcdFx0XHRzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgfHwgc2VsZi5taW5SYW5nZURhdGUgPiBzZWxmLmRheXMuY2hpbGROb2Rlc1swXS5kYXRlT2JqO1xuXG5cdFx0XHRcdHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyA9IHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyB8fCBzZWxmLm1heFJhbmdlRGF0ZSA8IG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoICsgMSwgMSk7XG5cdFx0XHR9IGVsc2Uge1xuXHRcdFx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cdFx0XHRcdHNlbGYuY2xvc2UoKTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRpZiAoZS53aGljaCA9PT0gMTMgJiYgc2VsZi5jb25maWcuZW5hYmxlVGltZSkgc2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5ob3VyRWxlbWVudC5mb2N1cygpO1xuXHRcdH0sIDQ1MSk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJzaW5nbGVcIiAmJiAhc2VsZi5jb25maWcuZW5hYmxlVGltZSkgc2VsZi5jbG9zZSgpO1xuXG5cdFx0dHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0KG9wdGlvbiwgdmFsdWUpIHtcblx0XHRzZWxmLmNvbmZpZ1tvcHRpb25dID0gdmFsdWU7XG5cdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRqdW1wVG9EYXRlKCk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXRTZWxlY3RlZERhdGUoaW5wdXREYXRlLCBmb3JtYXQpIHtcblx0XHRpZiAoQXJyYXkuaXNBcnJheShpbnB1dERhdGUpKSBzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUubWFwKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5wYXJzZURhdGUoZCwgZmFsc2UsIGZvcm1hdCk7XG5cdFx0fSk7ZWxzZSBpZiAoaW5wdXREYXRlIGluc3RhbmNlb2YgRGF0ZSB8fCAhaXNOYU4oaW5wdXREYXRlKSkgc2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGYucGFyc2VEYXRlKGlucHV0RGF0ZSldO2Vsc2UgaWYgKGlucHV0RGF0ZSAmJiBpbnB1dERhdGUuc3Vic3RyaW5nKSB7XG5cdFx0XHRzd2l0Y2ggKHNlbGYuY29uZmlnLm1vZGUpIHtcblx0XHRcdFx0Y2FzZSBcInNpbmdsZVwiOlxuXHRcdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtzZWxmLnBhcnNlRGF0ZShpbnB1dERhdGUsIGZhbHNlLCBmb3JtYXQpXTtcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwibXVsdGlwbGVcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUuc3BsaXQoXCI7IFwiKS5tYXAoZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdFx0XHRcdHJldHVybiBzZWxmLnBhcnNlRGF0ZShkYXRlLCBmYWxzZSwgZm9ybWF0KTtcblx0XHRcdFx0XHR9KTtcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIFwicmFuZ2VcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBpbnB1dERhdGUuc3BsaXQoc2VsZi5sMTBuLnJhbmdlU2VwYXJhdG9yKS5tYXAoZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdFx0XHRcdHJldHVybiBzZWxmLnBhcnNlRGF0ZShkYXRlLCBmYWxzZSwgZm9ybWF0KTtcblx0XHRcdFx0XHR9KTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGRlZmF1bHQ6XG5cdFx0XHRcdFx0YnJlYWs7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gc2VsZi5zZWxlY3RlZERhdGVzLmZpbHRlcihmdW5jdGlvbiAoZCkge1xuXHRcdFx0cmV0dXJuIGQgaW5zdGFuY2VvZiBEYXRlICYmIGQuZ2V0VGltZSgpICYmIGlzRW5hYmxlZChkLCBmYWxzZSk7XG5cdFx0fSk7XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMuc29ydChmdW5jdGlvbiAoYSwgYikge1xuXHRcdFx0cmV0dXJuIGEuZ2V0VGltZSgpIC0gYi5nZXRUaW1lKCk7XG5cdFx0fSk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXREYXRlKGRhdGUsIHRyaWdnZXJDaGFuZ2UsIGZvcm1hdCkge1xuXHRcdGlmICghZGF0ZSkgcmV0dXJuIHNlbGYuY2xlYXIoKTtcblxuXHRcdHNldFNlbGVjdGVkRGF0ZShkYXRlLCBmb3JtYXQpO1xuXG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPiAwKSB7XG5cdFx0XHRzZWxmLnNob3dUaW1lSW5wdXQgPSB0cnVlO1xuXHRcdFx0c2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBzZWxmLnNlbGVjdGVkRGF0ZXNbMF07XG5cdFx0fSBlbHNlIHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gbnVsbDtcblxuXHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0anVtcFRvRGF0ZSgpO1xuXG5cdFx0c2V0SG91cnNGcm9tRGF0ZSgpO1xuXHRcdHVwZGF0ZVZhbHVlKCk7XG5cblx0XHRpZiAodHJpZ2dlckNoYW5nZSAhPT0gZmFsc2UpIHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwRGF0ZXMoKSB7XG5cdFx0ZnVuY3Rpb24gcGFyc2VEYXRlUnVsZXMoYXJyKSB7XG5cdFx0XHRmb3IgKHZhciBpID0gYXJyLmxlbmd0aDsgaS0tOykge1xuXHRcdFx0XHRpZiAodHlwZW9mIGFycltpXSA9PT0gXCJzdHJpbmdcIiB8fCArYXJyW2ldKSBhcnJbaV0gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0sIHRydWUpO2Vsc2UgaWYgKGFycltpXSAmJiBhcnJbaV0uZnJvbSAmJiBhcnJbaV0udG8pIHtcblx0XHRcdFx0XHRhcnJbaV0uZnJvbSA9IHNlbGYucGFyc2VEYXRlKGFycltpXS5mcm9tKTtcblx0XHRcdFx0XHRhcnJbaV0udG8gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0udG8pO1xuXHRcdFx0XHR9XG5cdFx0XHR9XG5cblx0XHRcdHJldHVybiBhcnIuZmlsdGVyKGZ1bmN0aW9uICh4KSB7XG5cdFx0XHRcdHJldHVybiB4O1xuXHRcdFx0fSk7IC8vIHJlbW92ZSBmYWxzeSB2YWx1ZXNcblx0XHR9XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbXTtcblx0XHRzZWxmLm5vdyA9IG5ldyBEYXRlKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGgpIHNlbGYuY29uZmlnLmRpc2FibGUgPSBwYXJzZURhdGVSdWxlcyhzZWxmLmNvbmZpZy5kaXNhYmxlKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoKSBzZWxmLmNvbmZpZy5lbmFibGUgPSBwYXJzZURhdGVSdWxlcyhzZWxmLmNvbmZpZy5lbmFibGUpO1xuXG5cdFx0c2V0U2VsZWN0ZWREYXRlKHNlbGYuY29uZmlnLmRlZmF1bHREYXRlIHx8IHNlbGYuaW5wdXQudmFsdWUpO1xuXG5cdFx0dmFyIGluaXRpYWxEYXRlID0gc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA/IHNlbGYuc2VsZWN0ZWREYXRlc1swXSA6IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jb25maWcubWluRGF0ZS5nZXRUaW1lKCkgPiBzZWxmLm5vdyA/IHNlbGYuY29uZmlnLm1pbkRhdGUgOiBzZWxmLmNvbmZpZy5tYXhEYXRlICYmIHNlbGYuY29uZmlnLm1heERhdGUuZ2V0VGltZSgpIDwgc2VsZi5ub3cgPyBzZWxmLmNvbmZpZy5tYXhEYXRlIDogc2VsZi5ub3c7XG5cblx0XHRzZWxmLmN1cnJlbnRZZWFyID0gaW5pdGlhbERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRzZWxmLmN1cnJlbnRNb250aCA9IGluaXRpYWxEYXRlLmdldE1vbnRoKCk7XG5cblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkgc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBzZWxmLnNlbGVjdGVkRGF0ZXNbMF07XG5cblx0XHRzZWxmLm1pbkRhdGVIYXNUaW1lID0gc2VsZi5jb25maWcubWluRGF0ZSAmJiAoc2VsZi5jb25maWcubWluRGF0ZS5nZXRIb3VycygpIHx8IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TWludXRlcygpIHx8IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0U2Vjb25kcygpKTtcblxuXHRcdHNlbGYubWF4RGF0ZUhhc1RpbWUgPSBzZWxmLmNvbmZpZy5tYXhEYXRlICYmIChzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkgfHwgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNaW51dGVzKCkgfHwgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRTZWNvbmRzKCkpO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwibGF0ZXN0U2VsZWN0ZWREYXRlT2JqXCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gc2VsZi5fc2VsZWN0ZWREYXRlT2JqIHx8IHNlbGYuc2VsZWN0ZWREYXRlc1tzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoIC0gMV0gfHwgbnVsbDtcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChkYXRlKSB7XG5cdFx0XHRcdHNlbGYuX3NlbGVjdGVkRGF0ZU9iaiA9IGRhdGU7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcInNob3dUaW1lSW5wdXRcIiwge1xuXHRcdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChib29sKSB7XG5cdFx0XHRcdFx0aWYgKHNlbGYuY2FsZW5kYXJDb250YWluZXIpIHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwic2hvd1RpbWVJbnB1dFwiLCBib29sKTtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gc2V0dXBIZWxwZXJGdW5jdGlvbnMoKSB7XG5cdFx0c2VsZi51dGlscyA9IHtcblx0XHRcdGR1cmF0aW9uOiB7XG5cdFx0XHRcdERBWTogODY0MDAwMDBcblx0XHRcdH0sXG5cdFx0XHRnZXREYXlzaW5Nb250aDogZnVuY3Rpb24gZ2V0RGF5c2luTW9udGgobW9udGgsIHlyKSB7XG5cdFx0XHRcdG1vbnRoID0gdHlwZW9mIG1vbnRoID09PSBcInVuZGVmaW5lZFwiID8gc2VsZi5jdXJyZW50TW9udGggOiBtb250aDtcblxuXHRcdFx0XHR5ciA9IHR5cGVvZiB5ciA9PT0gXCJ1bmRlZmluZWRcIiA/IHNlbGYuY3VycmVudFllYXIgOiB5cjtcblxuXHRcdFx0XHRpZiAobW9udGggPT09IDEgJiYgKHlyICUgNCA9PT0gMCAmJiB5ciAlIDEwMCAhPT0gMCB8fCB5ciAlIDQwMCA9PT0gMCkpIHJldHVybiAyOTtcblxuXHRcdFx0XHRyZXR1cm4gc2VsZi5sMTBuLmRheXNJbk1vbnRoW21vbnRoXTtcblx0XHRcdH0sXG5cdFx0XHRtb250aFRvU3RyOiBmdW5jdGlvbiBtb250aFRvU3RyKG1vbnRoTnVtYmVyLCBzaG9ydGhhbmQpIHtcblx0XHRcdFx0c2hvcnRoYW5kID0gdHlwZW9mIHNob3J0aGFuZCA9PT0gXCJ1bmRlZmluZWRcIiA/IHNlbGYuY29uZmlnLnNob3J0aGFuZEN1cnJlbnRNb250aCA6IHNob3J0aGFuZDtcblxuXHRcdFx0XHRyZXR1cm4gc2VsZi5sMTBuLm1vbnRoc1soc2hvcnRoYW5kID8gXCJzaG9ydFwiIDogXCJsb25nXCIpICsgXCJoYW5kXCJdW21vbnRoTnVtYmVyXTtcblx0XHRcdH1cblx0XHR9O1xuXHR9XG5cblx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0ZnVuY3Rpb24gc2V0dXBGb3JtYXRzKCkge1xuXHRcdFtcIkRcIiwgXCJGXCIsIFwiSlwiLCBcIk1cIiwgXCJXXCIsIFwibFwiXS5mb3JFYWNoKGZ1bmN0aW9uIChmKSB7XG5cdFx0XHRzZWxmLmZvcm1hdHNbZl0gPSBGbGF0cGlja3IucHJvdG90eXBlLmZvcm1hdHNbZl0uYmluZChzZWxmKTtcblx0XHR9KTtcblxuXHRcdHNlbGYucmV2Rm9ybWF0LkYgPSBGbGF0cGlja3IucHJvdG90eXBlLnJldkZvcm1hdC5GLmJpbmQoc2VsZik7XG5cdFx0c2VsZi5yZXZGb3JtYXQuTSA9IEZsYXRwaWNrci5wcm90b3R5cGUucmV2Rm9ybWF0Lk0uYmluZChzZWxmKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwSW5wdXRzKCkge1xuXHRcdHNlbGYuaW5wdXQgPSBzZWxmLmNvbmZpZy53cmFwID8gc2VsZi5lbGVtZW50LnF1ZXJ5U2VsZWN0b3IoXCJbZGF0YS1pbnB1dF1cIikgOiBzZWxmLmVsZW1lbnQ7XG5cblx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdGlmICghc2VsZi5pbnB1dCkgcmV0dXJuIGNvbnNvbGUud2FybihcIkVycm9yOiBpbnZhbGlkIGlucHV0IGVsZW1lbnQgc3BlY2lmaWVkXCIsIHNlbGYuaW5wdXQpO1xuXG5cdFx0c2VsZi5pbnB1dC5fdHlwZSA9IHNlbGYuaW5wdXQudHlwZTtcblx0XHRzZWxmLmlucHV0LnR5cGUgPSBcInRleHRcIjtcblx0XHRzZWxmLmlucHV0LmNsYXNzTGlzdC5hZGQoXCJmbGF0cGlja3ItaW5wdXRcIik7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuYWx0SW5wdXQpIHtcblx0XHRcdC8vIHJlcGxpY2F0ZSBzZWxmLmVsZW1lbnRcblx0XHRcdHNlbGYuYWx0SW5wdXQgPSBjcmVhdGVFbGVtZW50KHNlbGYuaW5wdXQubm9kZU5hbWUsIHNlbGYuaW5wdXQuY2xhc3NOYW1lICsgXCIgXCIgKyBzZWxmLmNvbmZpZy5hbHRJbnB1dENsYXNzKTtcblx0XHRcdHNlbGYuYWx0SW5wdXQucGxhY2Vob2xkZXIgPSBzZWxmLmlucHV0LnBsYWNlaG9sZGVyO1xuXHRcdFx0c2VsZi5hbHRJbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0XHRzZWxmLmlucHV0LnR5cGUgPSBcImhpZGRlblwiO1xuXG5cdFx0XHRpZiAoIXNlbGYuY29uZmlnLnN0YXRpYyAmJiBzZWxmLmlucHV0LnBhcmVudE5vZGUpIHNlbGYuaW5wdXQucGFyZW50Tm9kZS5pbnNlcnRCZWZvcmUoc2VsZi5hbHRJbnB1dCwgc2VsZi5pbnB1dC5uZXh0U2libGluZyk7XG5cdFx0fVxuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5hbGxvd0lucHV0KSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5zZXRBdHRyaWJ1dGUoXCJyZWFkb25seVwiLCBcInJlYWRvbmx5XCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0dXBNb2JpbGUoKSB7XG5cdFx0dmFyIGlucHV0VHlwZSA9IHNlbGYuY29uZmlnLmVuYWJsZVRpbWUgPyBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyID8gXCJ0aW1lXCIgOiBcImRhdGV0aW1lLWxvY2FsXCIgOiBcImRhdGVcIjtcblxuXHRcdHNlbGYubW9iaWxlSW5wdXQgPSBjcmVhdGVFbGVtZW50KFwiaW5wdXRcIiwgc2VsZi5pbnB1dC5jbGFzc05hbWUgKyBcIiBmbGF0cGlja3ItbW9iaWxlXCIpO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQuc3RlcCA9IFwiYW55XCI7XG5cdFx0c2VsZi5tb2JpbGVJbnB1dC50YWJJbmRleCA9IDE7XG5cdFx0c2VsZi5tb2JpbGVJbnB1dC50eXBlID0gaW5wdXRUeXBlO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQuZGlzYWJsZWQgPSBzZWxmLmlucHV0LmRpc2FibGVkO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQucGxhY2Vob2xkZXIgPSBzZWxmLmlucHV0LnBsYWNlaG9sZGVyO1xuXG5cdFx0c2VsZi5tb2JpbGVGb3JtYXRTdHIgPSBpbnB1dFR5cGUgPT09IFwiZGF0ZXRpbWUtbG9jYWxcIiA/IFwiWS1tLWRcXFxcVEg6aTpTXCIgOiBpbnB1dFR5cGUgPT09IFwiZGF0ZVwiID8gXCJZLW0tZFwiIDogXCJIOmk6U1wiO1xuXG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHtcblx0XHRcdHNlbGYubW9iaWxlSW5wdXQuZGVmYXVsdFZhbHVlID0gc2VsZi5tb2JpbGVJbnB1dC52YWx1ZSA9IGZvcm1hdERhdGUoc2VsZi5tb2JpbGVGb3JtYXRTdHIsIHNlbGYuc2VsZWN0ZWREYXRlc1swXSk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1pbkRhdGUpIHNlbGYubW9iaWxlSW5wdXQubWluID0gZm9ybWF0RGF0ZShcIlktbS1kXCIsIHNlbGYuY29uZmlnLm1pbkRhdGUpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1heERhdGUpIHNlbGYubW9iaWxlSW5wdXQubWF4ID0gZm9ybWF0RGF0ZShcIlktbS1kXCIsIHNlbGYuY29uZmlnLm1heERhdGUpO1xuXG5cdFx0c2VsZi5pbnB1dC50eXBlID0gXCJoaWRkZW5cIjtcblx0XHRpZiAoc2VsZi5jb25maWcuYWx0SW5wdXQpIHNlbGYuYWx0SW5wdXQudHlwZSA9IFwiaGlkZGVuXCI7XG5cblx0XHR0cnkge1xuXHRcdFx0c2VsZi5pbnB1dC5wYXJlbnROb2RlLmluc2VydEJlZm9yZShzZWxmLm1vYmlsZUlucHV0LCBzZWxmLmlucHV0Lm5leHRTaWJsaW5nKTtcblx0XHR9IGNhdGNoIChlKSB7XG5cdFx0XHQvL1xuXHRcdH1cblxuXHRcdHNlbGYubW9iaWxlSW5wdXQuYWRkRXZlbnRMaXN0ZW5lcihcImNoYW5nZVwiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0c2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBzZWxmLnBhcnNlRGF0ZShlLnRhcmdldC52YWx1ZSk7XG5cdFx0XHRzZWxmLnNldERhdGUoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmopO1xuXHRcdFx0dHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHRcdFx0dHJpZ2dlckV2ZW50KFwiQ2xvc2VcIik7XG5cdFx0fSk7XG5cdH1cblxuXHRmdW5jdGlvbiB0b2dnbGUoKSB7XG5cdFx0aWYgKHNlbGYuaXNPcGVuKSBzZWxmLmNsb3NlKCk7ZWxzZSBzZWxmLm9wZW4oKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHRyaWdnZXJFdmVudChldmVudCwgZGF0YSkge1xuXHRcdHZhciBob29rcyA9IHNlbGYuY29uZmlnW1wib25cIiArIGV2ZW50XTtcblxuXHRcdGlmIChob29rcykge1xuXHRcdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCBob29rcy5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRob29rc1tpXShzZWxmLnNlbGVjdGVkRGF0ZXMsIHNlbGYuaW5wdXQgJiYgc2VsZi5pbnB1dC52YWx1ZSwgc2VsZiwgZGF0YSk7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0aWYgKGV2ZW50ID09PSBcIkNoYW5nZVwiKSB7XG5cdFx0XHRpZiAodHlwZW9mIEV2ZW50ID09PSBcImZ1bmN0aW9uXCIgJiYgRXZlbnQuY29uc3RydWN0b3IpIHtcblx0XHRcdFx0c2VsZi5pbnB1dC5kaXNwYXRjaEV2ZW50KG5ldyBFdmVudChcImNoYW5nZVwiLCB7IFwiYnViYmxlc1wiOiB0cnVlIH0pKTtcblxuXHRcdFx0XHQvLyBtYW55IGZyb250LWVuZCBmcmFtZXdvcmtzIGJpbmQgdG8gdGhlIGlucHV0IGV2ZW50XG5cdFx0XHRcdHNlbGYuaW5wdXQuZGlzcGF0Y2hFdmVudChuZXcgRXZlbnQoXCJpbnB1dFwiLCB7IFwiYnViYmxlc1wiOiB0cnVlIH0pKTtcblx0XHRcdH1cblxuXHRcdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRcdGVsc2Uge1xuXHRcdFx0XHRcdGlmICh3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRXZlbnQgIT09IHVuZGVmaW5lZCkgcmV0dXJuIHNlbGYuaW5wdXQuZGlzcGF0Y2hFdmVudChzZWxmLmNoYW5nZUV2ZW50KTtcblxuXHRcdFx0XHRcdHNlbGYuaW5wdXQuZmlyZUV2ZW50KFwib25jaGFuZ2VcIik7XG5cdFx0XHRcdH1cblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBpc0RhdGVTZWxlY3RlZChkYXRlKSB7XG5cdFx0Zm9yICh2YXIgaSA9IDA7IGkgPCBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoOyBpKyspIHtcblx0XHRcdGlmIChjb21wYXJlRGF0ZXMoc2VsZi5zZWxlY3RlZERhdGVzW2ldLCBkYXRlKSA9PT0gMCkgcmV0dXJuIFwiXCIgKyBpO1xuXHRcdH1cblxuXHRcdHJldHVybiBmYWxzZTtcblx0fVxuXG5cdGZ1bmN0aW9uIGlzRGF0ZUluUmFuZ2UoZGF0ZSkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlICE9PSBcInJhbmdlXCIgfHwgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA8IDIpIHJldHVybiBmYWxzZTtcblx0XHRyZXR1cm4gY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgPj0gMCAmJiBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5zZWxlY3RlZERhdGVzWzFdKSA8PSAwO1xuXHR9XG5cblx0ZnVuY3Rpb24gdXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhciB8fCBzZWxmLmlzTW9iaWxlIHx8ICFzZWxmLm1vbnRoTmF2KSByZXR1cm47XG5cblx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQudGV4dENvbnRlbnQgPSBzZWxmLnV0aWxzLm1vbnRoVG9TdHIoc2VsZi5jdXJyZW50TW9udGgpICsgXCIgXCI7XG5cdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQudmFsdWUgPSBzZWxmLmN1cnJlbnRZZWFyO1xuXG5cdFx0c2VsZi5faGlkZVByZXZNb250aEFycm93ID0gc2VsZi5jb25maWcubWluRGF0ZSAmJiAoc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpID8gc2VsZi5jdXJyZW50TW9udGggPD0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRNb250aCgpIDogc2VsZi5jdXJyZW50WWVhciA8IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKSk7XG5cblx0XHRzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgPSBzZWxmLmNvbmZpZy5tYXhEYXRlICYmIChzZWxmLmN1cnJlbnRZZWFyID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCkgPyBzZWxmLmN1cnJlbnRNb250aCArIDEgPiBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1vbnRoKCkgOiBzZWxmLmN1cnJlbnRZZWFyID4gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHVwZGF0ZVZhbHVlKCkge1xuXHRcdGlmICghc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkgcmV0dXJuIHNlbGYuY2xlYXIoKTtcblxuXHRcdGlmIChzZWxmLmlzTW9iaWxlKSB7XG5cdFx0XHRzZWxmLm1vYmlsZUlucHV0LnZhbHVlID0gc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA/IGZvcm1hdERhdGUoc2VsZi5tb2JpbGVGb3JtYXRTdHIsIHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqKSA6IFwiXCI7XG5cdFx0fVxuXG5cdFx0dmFyIGpvaW5DaGFyID0gc2VsZi5jb25maWcubW9kZSAhPT0gXCJyYW5nZVwiID8gXCI7IFwiIDogc2VsZi5sMTBuLnJhbmdlU2VwYXJhdG9yO1xuXG5cdFx0c2VsZi5pbnB1dC52YWx1ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5tYXAoZnVuY3Rpb24gKGRPYmopIHtcblx0XHRcdHJldHVybiBmb3JtYXREYXRlKHNlbGYuY29uZmlnLmRhdGVGb3JtYXQsIGRPYmopO1xuXHRcdH0pLmpvaW4oam9pbkNoYXIpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsdElucHV0KSB7XG5cdFx0XHRzZWxmLmFsdElucHV0LnZhbHVlID0gc2VsZi5zZWxlY3RlZERhdGVzLm1hcChmdW5jdGlvbiAoZE9iaikge1xuXHRcdFx0XHRyZXR1cm4gZm9ybWF0RGF0ZShzZWxmLmNvbmZpZy5hbHRGb3JtYXQsIGRPYmopO1xuXHRcdFx0fSkuam9pbihqb2luQ2hhcik7XG5cdFx0fVxuXG5cdFx0dHJpZ2dlckV2ZW50KFwiVmFsdWVVcGRhdGVcIik7XG5cdH1cblxuXHRmdW5jdGlvbiB5ZWFyU2Nyb2xsKGUpIHtcblx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cblx0XHR2YXIgZGVsdGEgPSBNYXRoLm1heCgtMSwgTWF0aC5taW4oMSwgZS53aGVlbERlbHRhIHx8IC1lLmRlbHRhWSkpLFxuXHRcdCAgICBuZXdZZWFyID0gcGFyc2VJbnQoZS50YXJnZXQudmFsdWUsIDEwKSArIGRlbHRhO1xuXG5cdFx0Y2hhbmdlWWVhcihuZXdZZWFyKTtcblx0XHRlLnRhcmdldC52YWx1ZSA9IHNlbGYuY3VycmVudFllYXI7XG5cdH1cblxuXHRmdW5jdGlvbiBjcmVhdGVFbGVtZW50KHRhZywgY2xhc3NOYW1lLCBjb250ZW50KSB7XG5cdFx0dmFyIGUgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRWxlbWVudCh0YWcpO1xuXHRcdGNsYXNzTmFtZSA9IGNsYXNzTmFtZSB8fCBcIlwiO1xuXHRcdGNvbnRlbnQgPSBjb250ZW50IHx8IFwiXCI7XG5cblx0XHRlLmNsYXNzTmFtZSA9IGNsYXNzTmFtZTtcblxuXHRcdGlmIChjb250ZW50KSBlLnRleHRDb250ZW50ID0gY29udGVudDtcblxuXHRcdHJldHVybiBlO1xuXHR9XG5cblx0ZnVuY3Rpb24gYXJyYXlpZnkob2JqKSB7XG5cdFx0aWYgKEFycmF5LmlzQXJyYXkob2JqKSkgcmV0dXJuIG9iajtcblx0XHRyZXR1cm4gW29ial07XG5cdH1cblxuXHRmdW5jdGlvbiB0b2dnbGVDbGFzcyhlbGVtLCBjbGFzc05hbWUsIGJvb2wpIHtcblx0XHRpZiAoYm9vbCkgcmV0dXJuIGVsZW0uY2xhc3NMaXN0LmFkZChjbGFzc05hbWUpO1xuXHRcdGVsZW0uY2xhc3NMaXN0LnJlbW92ZShjbGFzc05hbWUpO1xuXHR9XG5cblx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0ZnVuY3Rpb24gZGVib3VuY2UoZnVuYywgd2FpdCwgaW1tZWRpYXRlKSB7XG5cdFx0dmFyIHRpbWVvdXQgPSB2b2lkIDA7XG5cdFx0cmV0dXJuIGZ1bmN0aW9uICgpIHtcblx0XHRcdGZvciAodmFyIF9sZW4gPSBhcmd1bWVudHMubGVuZ3RoLCBhcmdzID0gQXJyYXkoX2xlbiksIF9rZXkgPSAwOyBfa2V5IDwgX2xlbjsgX2tleSsrKSB7XG5cdFx0XHRcdGFyZ3NbX2tleV0gPSBhcmd1bWVudHNbX2tleV07XG5cdFx0XHR9XG5cblx0XHRcdHZhciBjb250ZXh0ID0gdGhpcztcblx0XHRcdHZhciBsYXRlciA9IGZ1bmN0aW9uIGxhdGVyKCkge1xuXHRcdFx0XHR0aW1lb3V0ID0gbnVsbDtcblx0XHRcdFx0aWYgKCFpbW1lZGlhdGUpIGZ1bmMuYXBwbHkoY29udGV4dCwgYXJncyk7XG5cdFx0XHR9O1xuXG5cdFx0XHRjbGVhclRpbWVvdXQodGltZW91dCk7XG5cdFx0XHR0aW1lb3V0ID0gc2V0VGltZW91dChsYXRlciwgd2FpdCk7XG5cdFx0XHRpZiAoaW1tZWRpYXRlICYmICF0aW1lb3V0KSBmdW5jLmFwcGx5KGNvbnRleHQsIGFyZ3MpO1xuXHRcdH07XG5cdH1cblxuXHRmdW5jdGlvbiBjb21wYXJlRGF0ZXMoZGF0ZTEsIGRhdGUyLCB0aW1lbGVzcykge1xuXHRcdGlmICghKGRhdGUxIGluc3RhbmNlb2YgRGF0ZSkgfHwgIShkYXRlMiBpbnN0YW5jZW9mIERhdGUpKSByZXR1cm4gZmFsc2U7XG5cblx0XHRpZiAodGltZWxlc3MgIT09IGZhbHNlKSB7XG5cdFx0XHRyZXR1cm4gbmV3IERhdGUoZGF0ZTEuZ2V0VGltZSgpKS5zZXRIb3VycygwLCAwLCAwLCAwKSAtIG5ldyBEYXRlKGRhdGUyLmdldFRpbWUoKSkuc2V0SG91cnMoMCwgMCwgMCwgMCk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIGRhdGUxLmdldFRpbWUoKSAtIGRhdGUyLmdldFRpbWUoKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHRpbWVXcmFwcGVyKGUpIHtcblx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cblx0XHR2YXIgaXNLZXlEb3duID0gZS50eXBlID09PSBcImtleWRvd25cIixcblx0XHQgICAgaXNXaGVlbCA9IGUudHlwZSA9PT0gXCJ3aGVlbFwiLFxuXHRcdCAgICBpc0luY3JlbWVudCA9IGUudHlwZSA9PT0gXCJpbmNyZW1lbnRcIixcblx0XHQgICAgaW5wdXQgPSBlLnRhcmdldDtcblxuXHRcdGlmIChlLnR5cGUgIT09IFwiaW5wdXRcIiAmJiAhaXNLZXlEb3duICYmIChlLnRhcmdldC52YWx1ZSB8fCBlLnRhcmdldC50ZXh0Q29udGVudCkubGVuZ3RoID49IDIgLy8gdHlwZWQgdHdvIGRpZ2l0c1xuXHRcdCkge1xuXHRcdFx0XHRlLnRhcmdldC5mb2N1cygpO1xuXHRcdFx0XHRlLnRhcmdldC5ibHVyKCk7XG5cdFx0XHR9XG5cblx0XHRpZiAoc2VsZi5hbVBNICYmIGUudGFyZ2V0ID09PSBzZWxmLmFtUE0pIHJldHVybiBlLnRhcmdldC50ZXh0Q29udGVudCA9IFtcIkFNXCIsIFwiUE1cIl1bZS50YXJnZXQudGV4dENvbnRlbnQgPT09IFwiQU1cIiB8IDBdO1xuXG5cdFx0dmFyIG1pbiA9IE51bWJlcihpbnB1dC5taW4pLFxuXHRcdCAgICBtYXggPSBOdW1iZXIoaW5wdXQubWF4KSxcblx0XHQgICAgc3RlcCA9IE51bWJlcihpbnB1dC5zdGVwKSxcblx0XHQgICAgY3VyVmFsdWUgPSBwYXJzZUludChpbnB1dC52YWx1ZSwgMTApLFxuXHRcdCAgICBkZWx0YSA9IGUuZGVsdGEgfHwgKCFpc0tleURvd24gPyBNYXRoLm1heCgtMSwgTWF0aC5taW4oMSwgZS53aGVlbERlbHRhIHx8IC1lLmRlbHRhWSkpIHx8IDAgOiBlLndoaWNoID09PSAzOCA/IDEgOiAtMSk7XG5cblx0XHR2YXIgbmV3VmFsdWUgPSBjdXJWYWx1ZSArIHN0ZXAgKiBkZWx0YTtcblxuXHRcdGlmIChpbnB1dC52YWx1ZS5sZW5ndGggPT09IDIpIHtcblx0XHRcdHZhciBpc0hvdXJFbGVtID0gaW5wdXQgPT09IHNlbGYuaG91ckVsZW1lbnQsXG5cdFx0XHQgICAgaXNNaW51dGVFbGVtID0gaW5wdXQgPT09IHNlbGYubWludXRlRWxlbWVudDtcblxuXHRcdFx0aWYgKG5ld1ZhbHVlIDwgbWluKSB7XG5cdFx0XHRcdG5ld1ZhbHVlID0gbWF4ICsgbmV3VmFsdWUgKyAhaXNIb3VyRWxlbSArIChpc0hvdXJFbGVtICYmICFzZWxmLmFtUE0pO1xuXG5cdFx0XHRcdGlmIChpc01pbnV0ZUVsZW0pIGluY3JlbWVudE51bUlucHV0KG51bGwsIC0xLCBzZWxmLmhvdXJFbGVtZW50KTtcblx0XHRcdH0gZWxzZSBpZiAobmV3VmFsdWUgPiBtYXgpIHtcblx0XHRcdFx0bmV3VmFsdWUgPSBpbnB1dCA9PT0gc2VsZi5ob3VyRWxlbWVudCA/IG5ld1ZhbHVlIC0gbWF4IC0gIXNlbGYuYW1QTSA6IG1pbjtcblxuXHRcdFx0XHRpZiAoaXNNaW51dGVFbGVtKSBpbmNyZW1lbnROdW1JbnB1dChudWxsLCAxLCBzZWxmLmhvdXJFbGVtZW50KTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuYW1QTSAmJiBpc0hvdXJFbGVtICYmIChzdGVwID09PSAxID8gbmV3VmFsdWUgKyBjdXJWYWx1ZSA9PT0gMjMgOiBNYXRoLmFicyhuZXdWYWx1ZSAtIGN1clZhbHVlKSA+IHN0ZXApKSBzZWxmLmFtUE0udGV4dENvbnRlbnQgPSBzZWxmLmFtUE0udGV4dENvbnRlbnQgPT09IFwiUE1cIiA/IFwiQU1cIiA6IFwiUE1cIjtcblxuXHRcdFx0aW5wdXQudmFsdWUgPSBzZWxmLnBhZChuZXdWYWx1ZSk7XG5cdFx0fVxuXHR9XG5cblx0aW5pdCgpO1xuXHRyZXR1cm4gc2VsZjtcbn1cblxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbkZsYXRwaWNrci5kZWZhdWx0Q29uZmlnID0ge1xuXG5cdG1vZGU6IFwic2luZ2xlXCIsXG5cblx0cG9zaXRpb246IFwidG9wXCIsXG5cblx0LyogaWYgdHJ1ZSwgZGF0ZXMgd2lsbCBiZSBwYXJzZWQsIGZvcm1hdHRlZCwgYW5kIGRpc3BsYXllZCBpbiBVVEMuXG4gcHJlbG9hZGluZyBkYXRlIHN0cmluZ3Mgdy8gdGltZXpvbmVzIGlzIHJlY29tbWVuZGVkIGJ1dCBub3QgbmVjZXNzYXJ5ICovXG5cdHV0YzogZmFsc2UsXG5cblx0Ly8gd3JhcDogc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jc3RyYXBcblx0d3JhcDogZmFsc2UsXG5cblx0Ly8gZW5hYmxlcyB3ZWVrIG51bWJlcnNcblx0d2Vla051bWJlcnM6IGZhbHNlLFxuXG5cdC8vIGFsbG93IG1hbnVhbCBkYXRldGltZSBpbnB1dFxuXHRhbGxvd0lucHV0OiBmYWxzZSxcblxuXHQvKlxuIFx0Y2xpY2tpbmcgb24gaW5wdXQgb3BlbnMgdGhlIGRhdGUodGltZSlwaWNrZXIuXG4gXHRkaXNhYmxlIGlmIHlvdSB3aXNoIHRvIG9wZW4gdGhlIGNhbGVuZGFyIG1hbnVhbGx5IHdpdGggLm9wZW4oKVxuICovXG5cdGNsaWNrT3BlbnM6IHRydWUsXG5cblx0Ly8gZGlzcGxheSB0aW1lIHBpY2tlciBpbiAyNCBob3VyIG1vZGVcblx0dGltZV8yNGhyOiBmYWxzZSxcblxuXHQvLyBlbmFibGVzIHRoZSB0aW1lIHBpY2tlciBmdW5jdGlvbmFsaXR5XG5cdGVuYWJsZVRpbWU6IGZhbHNlLFxuXG5cdC8vIG5vQ2FsZW5kYXI6IHRydWUgd2lsbCBoaWRlIHRoZSBjYWxlbmRhci4gdXNlIGZvciBhIHRpbWUgcGlja2VyIGFsb25nIHcvIGVuYWJsZVRpbWVcblx0bm9DYWxlbmRhcjogZmFsc2UsXG5cblx0Ly8gbW9yZSBkYXRlIGZvcm1hdCBjaGFycyBhdCBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2RhdGVmb3JtYXRcblx0ZGF0ZUZvcm1hdDogXCJZLW0tZFwiLFxuXG5cdC8vIGFsdElucHV0IC0gc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jYWx0aW5wdXRcblx0YWx0SW5wdXQ6IGZhbHNlLFxuXG5cdC8vIHRoZSBjcmVhdGVkIGFsdElucHV0IGVsZW1lbnQgd2lsbCBoYXZlIHRoaXMgY2xhc3MuXG5cdGFsdElucHV0Q2xhc3M6IFwiZmxhdHBpY2tyLWlucHV0IGZvcm0tY29udHJvbCBpbnB1dFwiLFxuXG5cdC8vIHNhbWUgYXMgZGF0ZUZvcm1hdCwgYnV0IGZvciBhbHRJbnB1dFxuXHRhbHRGb3JtYXQ6IFwiRiBqLCBZXCIsIC8vIGRlZmF1bHRzIHRvIGUuZy4gSnVuZSAxMCwgMjAxNlxuXG5cdC8vIGRlZmF1bHREYXRlIC0gZWl0aGVyIGEgZGF0ZXN0cmluZyBvciBhIGRhdGUgb2JqZWN0LiB1c2VkIGZvciBkYXRldGltZXBpY2tlclwicyBpbml0aWFsIHZhbHVlXG5cdGRlZmF1bHREYXRlOiBudWxsLFxuXG5cdC8vIHRoZSBtaW5pbXVtIGRhdGUgdGhhdCB1c2VyIGNhbiBwaWNrIChpbmNsdXNpdmUpXG5cdG1pbkRhdGU6IG51bGwsXG5cblx0Ly8gdGhlIG1heGltdW0gZGF0ZSB0aGF0IHVzZXIgY2FuIHBpY2sgKGluY2x1c2l2ZSlcblx0bWF4RGF0ZTogbnVsbCxcblxuXHQvLyBkYXRlcGFyc2VyIHRoYXQgdHJhbnNmb3JtcyBhIGdpdmVuIHN0cmluZyB0byBhIGRhdGUgb2JqZWN0XG5cdHBhcnNlRGF0ZTogbnVsbCxcblxuXHQvLyBkYXRlZm9ybWF0dGVyIHRoYXQgdHJhbnNmb3JtcyBhIGdpdmVuIGRhdGUgb2JqZWN0IHRvIGEgc3RyaW5nLCBhY2NvcmRpbmcgdG8gcGFzc2VkIGZvcm1hdFxuXHRmb3JtYXREYXRlOiBudWxsLFxuXG5cdGdldFdlZWs6IGZ1bmN0aW9uIGdldFdlZWsoZ2l2ZW5EYXRlKSB7XG5cdFx0dmFyIGRhdGUgPSBuZXcgRGF0ZShnaXZlbkRhdGUuZ2V0VGltZSgpKTtcblx0XHRkYXRlLnNldEhvdXJzKDAsIDAsIDAsIDApO1xuXG5cdFx0Ly8gVGh1cnNkYXkgaW4gY3VycmVudCB3ZWVrIGRlY2lkZXMgdGhlIHllYXIuXG5cdFx0ZGF0ZS5zZXREYXRlKGRhdGUuZ2V0RGF0ZSgpICsgMyAtIChkYXRlLmdldERheSgpICsgNikgJSA3KTtcblx0XHQvLyBKYW51YXJ5IDQgaXMgYWx3YXlzIGluIHdlZWsgMS5cblx0XHR2YXIgd2VlazEgPSBuZXcgRGF0ZShkYXRlLmdldEZ1bGxZZWFyKCksIDAsIDQpO1xuXHRcdC8vIEFkanVzdCB0byBUaHVyc2RheSBpbiB3ZWVrIDEgYW5kIGNvdW50IG51bWJlciBvZiB3ZWVrcyBmcm9tIGRhdGUgdG8gd2VlazEuXG5cdFx0cmV0dXJuIDEgKyBNYXRoLnJvdW5kKCgoZGF0ZS5nZXRUaW1lKCkgLSB3ZWVrMS5nZXRUaW1lKCkpIC8gODY0MDAwMDAgLSAzICsgKHdlZWsxLmdldERheSgpICsgNikgJSA3KSAvIDcpO1xuXHR9LFxuXG5cdC8vIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2Rpc2FibGVcblx0ZW5hYmxlOiBbXSxcblxuXHQvLyBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNkaXNhYmxlXG5cdGRpc2FibGU6IFtdLFxuXG5cdC8vIGRpc3BsYXkgdGhlIHNob3J0IHZlcnNpb24gb2YgbW9udGggbmFtZXMgLSBlLmcuIFNlcCBpbnN0ZWFkIG9mIFNlcHRlbWJlclxuXHRzaG9ydGhhbmRDdXJyZW50TW9udGg6IGZhbHNlLFxuXG5cdC8vIGRpc3BsYXlzIGNhbGVuZGFyIGlubGluZS4gc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jaW5saW5lLWNhbGVuZGFyXG5cdGlubGluZTogZmFsc2UsXG5cblx0Ly8gcG9zaXRpb24gY2FsZW5kYXIgaW5zaWRlIHdyYXBwZXIgYW5kIG5leHQgdG8gdGhlIGlucHV0IGVsZW1lbnRcblx0Ly8gbGVhdmUgYXQgZmFsc2UgdW5sZXNzIHlvdSBrbm93IHdoYXQgeW91XCJyZSBkb2luZ1xuXHRzdGF0aWM6IGZhbHNlLFxuXG5cdC8vIERPTSBub2RlIHRvIGFwcGVuZCB0aGUgY2FsZW5kYXIgdG8gaW4gKnN0YXRpYyogbW9kZVxuXHRhcHBlbmRUbzogbnVsbCxcblxuXHQvLyBjb2RlIGZvciBwcmV2aW91cy9uZXh0IGljb25zLiB0aGlzIGlzIHdoZXJlIHlvdSBwdXQgeW91ciBjdXN0b20gaWNvbiBjb2RlIGUuZy4gZm9udGF3ZXNvbWVcblx0cHJldkFycm93OiBcIjxzdmcgdmVyc2lvbj0nMS4xJyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHhtbG5zOnhsaW5rPSdodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rJyB2aWV3Qm94PScwIDAgMTcgMTcnPjxnPjwvZz48cGF0aCBkPSdNNS4yMDcgOC40NzFsNy4xNDYgNy4xNDctMC43MDcgMC43MDctNy44NTMtNy44NTQgNy44NTQtNy44NTMgMC43MDcgMC43MDctNy4xNDcgNy4xNDZ6JyAvPjwvc3ZnPlwiLFxuXHRuZXh0QXJyb3c6IFwiPHN2ZyB2ZXJzaW9uPScxLjEnIHhtbG5zPSdodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZycgeG1sbnM6eGxpbms9J2h0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsnIHZpZXdCb3g9JzAgMCAxNyAxNyc+PGc+PC9nPjxwYXRoIGQ9J00xMy4yMDcgOC40NzJsLTcuODU0IDcuODU0LTAuNzA3LTAuNzA3IDcuMTQ2LTcuMTQ2LTcuMTQ2LTcuMTQ4IDAuNzA3LTAuNzA3IDcuODU0IDcuODU0eicgLz48L3N2Zz5cIixcblxuXHQvLyBlbmFibGVzIHNlY29uZHMgaW4gdGhlIHRpbWUgcGlja2VyXG5cdGVuYWJsZVNlY29uZHM6IGZhbHNlLFxuXG5cdC8vIHN0ZXAgc2l6ZSB1c2VkIHdoZW4gc2Nyb2xsaW5nL2luY3JlbWVudGluZyB0aGUgaG91ciBlbGVtZW50XG5cdGhvdXJJbmNyZW1lbnQ6IDEsXG5cblx0Ly8gc3RlcCBzaXplIHVzZWQgd2hlbiBzY3JvbGxpbmcvaW5jcmVtZW50aW5nIHRoZSBtaW51dGUgZWxlbWVudFxuXHRtaW51dGVJbmNyZW1lbnQ6IDUsXG5cblx0Ly8gaW5pdGlhbCB2YWx1ZSBpbiB0aGUgaG91ciBlbGVtZW50XG5cdGRlZmF1bHRIb3VyOiAxMixcblxuXHQvLyBpbml0aWFsIHZhbHVlIGluIHRoZSBtaW51dGUgZWxlbWVudFxuXHRkZWZhdWx0TWludXRlOiAwLFxuXG5cdC8vIGRpc2FibGUgbmF0aXZlIG1vYmlsZSBkYXRldGltZSBpbnB1dCBzdXBwb3J0XG5cdGRpc2FibGVNb2JpbGU6IGZhbHNlLFxuXG5cdC8vIGRlZmF1bHQgbG9jYWxlXG5cdGxvY2FsZTogXCJkZWZhdWx0XCIsXG5cblx0cGx1Z2luczogW10sXG5cblx0Ly8gY2FsbGVkIGV2ZXJ5IHRpbWUgY2FsZW5kYXIgaXMgY2xvc2VkXG5cdG9uQ2xvc2U6IFtdLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBvbkNoYW5nZSBjYWxsYmFjayB3aGVuIHVzZXIgc2VsZWN0cyBhIGRhdGUgb3IgdGltZVxuXHRvbkNoYW5nZTogW10sIC8vIGZ1bmN0aW9uIChkYXRlT2JqLCBkYXRlU3RyKSB7fVxuXG5cdC8vIGNhbGxlZCBmb3IgZXZlcnkgZGF5IGVsZW1lbnRcblx0b25EYXlDcmVhdGU6IFtdLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIHRoZSBtb250aCBpcyBjaGFuZ2VkXG5cdG9uTW9udGhDaGFuZ2U6IFtdLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIGNhbGVuZGFyIGlzIG9wZW5lZFxuXHRvbk9wZW46IFtdLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBjYWxsZWQgYWZ0ZXIgdGhlIGNvbmZpZ3VyYXRpb24gaGFzIGJlZW4gcGFyc2VkXG5cdG9uUGFyc2VDb25maWc6IFtdLFxuXG5cdC8vIGNhbGxlZCBhZnRlciBjYWxlbmRhciBpcyByZWFkeVxuXHRvblJlYWR5OiBbXSwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gY2FsbGVkIGFmdGVyIGlucHV0IHZhbHVlIHVwZGF0ZWRcblx0b25WYWx1ZVVwZGF0ZTogW10sXG5cblx0Ly8gY2FsbGVkIGV2ZXJ5IHRpbWUgdGhlIHllYXIgaXMgY2hhbmdlZFxuXHRvblllYXJDaGFuZ2U6IFtdXG59O1xuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuRmxhdHBpY2tyLmwxMG5zID0ge1xuXHRlbjoge1xuXHRcdHdlZWtkYXlzOiB7XG5cdFx0XHRzaG9ydGhhbmQ6IFtcIlN1blwiLCBcIk1vblwiLCBcIlR1ZVwiLCBcIldlZFwiLCBcIlRodVwiLCBcIkZyaVwiLCBcIlNhdFwiXSxcblx0XHRcdGxvbmdoYW5kOiBbXCJTdW5kYXlcIiwgXCJNb25kYXlcIiwgXCJUdWVzZGF5XCIsIFwiV2VkbmVzZGF5XCIsIFwiVGh1cnNkYXlcIiwgXCJGcmlkYXlcIiwgXCJTYXR1cmRheVwiXVxuXHRcdH0sXG5cdFx0bW9udGhzOiB7XG5cdFx0XHRzaG9ydGhhbmQ6IFtcIkphblwiLCBcIkZlYlwiLCBcIk1hclwiLCBcIkFwclwiLCBcIk1heVwiLCBcIkp1blwiLCBcIkp1bFwiLCBcIkF1Z1wiLCBcIlNlcFwiLCBcIk9jdFwiLCBcIk5vdlwiLCBcIkRlY1wiXSxcblx0XHRcdGxvbmdoYW5kOiBbXCJKYW51YXJ5XCIsIFwiRmVicnVhcnlcIiwgXCJNYXJjaFwiLCBcIkFwcmlsXCIsIFwiTWF5XCIsIFwiSnVuZVwiLCBcIkp1bHlcIiwgXCJBdWd1c3RcIiwgXCJTZXB0ZW1iZXJcIiwgXCJPY3RvYmVyXCIsIFwiTm92ZW1iZXJcIiwgXCJEZWNlbWJlclwiXVxuXHRcdH0sXG5cdFx0ZGF5c0luTW9udGg6IFszMSwgMjgsIDMxLCAzMCwgMzEsIDMwLCAzMSwgMzEsIDMwLCAzMSwgMzAsIDMxXSxcblx0XHRmaXJzdERheU9mV2VlazogMCxcblx0XHRvcmRpbmFsOiBmdW5jdGlvbiBvcmRpbmFsKG50aCkge1xuXHRcdFx0dmFyIHMgPSBudGggJSAxMDA7XG5cdFx0XHRpZiAocyA+IDMgJiYgcyA8IDIxKSByZXR1cm4gXCJ0aFwiO1xuXHRcdFx0c3dpdGNoIChzICUgMTApIHtcblx0XHRcdFx0Y2FzZSAxOlxuXHRcdFx0XHRcdHJldHVybiBcInN0XCI7XG5cdFx0XHRcdGNhc2UgMjpcblx0XHRcdFx0XHRyZXR1cm4gXCJuZFwiO1xuXHRcdFx0XHRjYXNlIDM6XG5cdFx0XHRcdFx0cmV0dXJuIFwicmRcIjtcblx0XHRcdFx0ZGVmYXVsdDpcblx0XHRcdFx0XHRyZXR1cm4gXCJ0aFwiO1xuXHRcdFx0fVxuXHRcdH0sXG5cdFx0cmFuZ2VTZXBhcmF0b3I6IFwiIHRvIFwiLFxuXHRcdHdlZWtBYmJyZXZpYXRpb246IFwiV2tcIixcblx0XHRzY3JvbGxUaXRsZTogXCJTY3JvbGwgdG8gaW5jcmVtZW50XCIsXG5cdFx0dG9nZ2xlVGl0bGU6IFwiQ2xpY2sgdG8gdG9nZ2xlXCJcblx0fVxufTtcblxuRmxhdHBpY2tyLmwxMG5zLmRlZmF1bHQgPSBPYmplY3QuY3JlYXRlKEZsYXRwaWNrci5sMTBucy5lbik7XG5GbGF0cGlja3IubG9jYWxpemUgPSBmdW5jdGlvbiAobDEwbikge1xuXHRyZXR1cm4gX2V4dGVuZHMoRmxhdHBpY2tyLmwxMG5zLmRlZmF1bHQsIGwxMG4gfHwge30pO1xufTtcbkZsYXRwaWNrci5zZXREZWZhdWx0cyA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0cmV0dXJuIF9leHRlbmRzKEZsYXRwaWNrci5kZWZhdWx0Q29uZmlnLCBjb25maWcgfHwge30pO1xufTtcblxuRmxhdHBpY2tyLnByb3RvdHlwZSA9IHtcblx0Zm9ybWF0czoge1xuXHRcdC8vIGdldCB0aGUgZGF0ZSBpbiBVVENcblx0XHRaOiBmdW5jdGlvbiBaKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLnRvSVNPU3RyaW5nKCk7XG5cdFx0fSxcblxuXHRcdC8vIHdlZWtkYXkgbmFtZSwgc2hvcnQsIGUuZy4gVGh1XG5cdFx0RDogZnVuY3Rpb24gRChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5sMTBuLndlZWtkYXlzLnNob3J0aGFuZFt0aGlzLmZvcm1hdHMudyhkYXRlKV07XG5cdFx0fSxcblxuXHRcdC8vIGZ1bGwgbW9udGggbmFtZSBlLmcuIEphbnVhcnlcblx0XHRGOiBmdW5jdGlvbiBGKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLnV0aWxzLm1vbnRoVG9TdHIodGhpcy5mb3JtYXRzLm4oZGF0ZSkgLSAxLCBmYWxzZSk7XG5cdFx0fSxcblxuXHRcdC8vIGhvdXJzIHdpdGggbGVhZGluZyB6ZXJvIGUuZy4gMDNcblx0XHRIOiBmdW5jdGlvbiBIKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldEhvdXJzKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBkYXkgKDEtMzApIHdpdGggb3JkaW5hbCBzdWZmaXggZS5nLiAxc3QsIDJuZFxuXHRcdEo6IGZ1bmN0aW9uIEooZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0RGF0ZSgpICsgdGhpcy5sMTBuLm9yZGluYWwoZGF0ZS5nZXREYXRlKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBBTS9QTVxuXHRcdEs6IGZ1bmN0aW9uIEsoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0SG91cnMoKSA+IDExID8gXCJQTVwiIDogXCJBTVwiO1xuXHRcdH0sXG5cblx0XHQvLyBzaG9ydGhhbmQgbW9udGggZS5nLiBKYW4sIFNlcCwgT2N0LCBldGNcblx0XHRNOiBmdW5jdGlvbiBNKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLnV0aWxzLm1vbnRoVG9TdHIoZGF0ZS5nZXRNb250aCgpLCB0cnVlKTtcblx0XHR9LFxuXG5cdFx0Ly8gc2Vjb25kcyAwMC01OVxuXHRcdFM6IGZ1bmN0aW9uIFMoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0U2Vjb25kcygpKTtcblx0XHR9LFxuXG5cdFx0Ly8gdW5peCB0aW1lc3RhbXBcblx0XHRVOiBmdW5jdGlvbiBVKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldFRpbWUoKSAvIDEwMDA7XG5cdFx0fSxcblxuXHRcdFc6IGZ1bmN0aW9uIFcoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMuY29uZmlnLmdldFdlZWsoZGF0ZSk7XG5cdFx0fSxcblxuXHRcdC8vIGZ1bGwgeWVhciBlLmcuIDIwMTZcblx0XHRZOiBmdW5jdGlvbiBZKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0fSxcblxuXHRcdC8vIGRheSBpbiBtb250aCwgcGFkZGVkICgwMS0zMClcblx0XHRkOiBmdW5jdGlvbiBkKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldERhdGUoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGhvdXIgZnJvbSAxLTEyIChhbS9wbSlcblx0XHRoOiBmdW5jdGlvbiBoKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldEhvdXJzKCkgJSAxMiA/IGRhdGUuZ2V0SG91cnMoKSAlIDEyIDogMTI7XG5cdFx0fSxcblxuXHRcdC8vIG1pbnV0ZXMsIHBhZGRlZCB3aXRoIGxlYWRpbmcgemVybyBlLmcuIDA5XG5cdFx0aTogZnVuY3Rpb24gaShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRNaW51dGVzKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBkYXkgaW4gbW9udGggKDEtMzApXG5cdFx0ajogZnVuY3Rpb24gaihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXREYXRlKCk7XG5cdFx0fSxcblxuXHRcdC8vIHdlZWtkYXkgbmFtZSwgZnVsbCwgZS5nLiBUaHVyc2RheVxuXHRcdGw6IGZ1bmN0aW9uIGwoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMubDEwbi53ZWVrZGF5cy5sb25naGFuZFtkYXRlLmdldERheSgpXTtcblx0XHR9LFxuXG5cdFx0Ly8gcGFkZGVkIG1vbnRoIG51bWJlciAoMDEtMTIpXG5cdFx0bTogZnVuY3Rpb24gbShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRNb250aCgpICsgMSk7XG5cdFx0fSxcblxuXHRcdC8vIHRoZSBtb250aCBudW1iZXIgKDEtMTIpXG5cdFx0bjogZnVuY3Rpb24gbihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRNb250aCgpICsgMTtcblx0XHR9LFxuXG5cdFx0Ly8gc2Vjb25kcyAwLTU5XG5cdFx0czogZnVuY3Rpb24gcyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRTZWNvbmRzKCk7XG5cdFx0fSxcblxuXHRcdC8vIG51bWJlciBvZiB0aGUgZGF5IG9mIHRoZSB3ZWVrXG5cdFx0dzogZnVuY3Rpb24gdyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXREYXkoKTtcblx0XHR9LFxuXG5cdFx0Ly8gbGFzdCB0d28gZGlnaXRzIG9mIHllYXIgZS5nLiAxNiBmb3IgMjAxNlxuXHRcdHk6IGZ1bmN0aW9uIHkoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIFN0cmluZyhkYXRlLmdldEZ1bGxZZWFyKCkpLnN1YnN0cmluZygyKTtcblx0XHR9XG5cdH0sXG5cblx0cmV2Rm9ybWF0OiB7XG5cdFx0RDogZnVuY3Rpb24gRCgpIHt9LFxuXHRcdEY6IGZ1bmN0aW9uIEYoZGF0ZU9iaiwgbW9udGhOYW1lKSB7XG5cdFx0XHRkYXRlT2JqLnNldE1vbnRoKHRoaXMubDEwbi5tb250aHMubG9uZ2hhbmQuaW5kZXhPZihtb250aE5hbWUpKTtcblx0XHR9LFxuXHRcdEg6IGZ1bmN0aW9uIEgoZGF0ZU9iaiwgaG91cikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0SG91cnMocGFyc2VGbG9hdChob3VyKSk7XG5cdFx0fSxcblx0XHRKOiBmdW5jdGlvbiBKKGRhdGVPYmosIGRheSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0SzogZnVuY3Rpb24gSyhkYXRlT2JqLCBhbVBNKSB7XG5cdFx0XHR2YXIgaG91cnMgPSBkYXRlT2JqLmdldEhvdXJzKCk7XG5cblx0XHRcdGlmIChob3VycyAhPT0gMTIpIGRhdGVPYmouc2V0SG91cnMoaG91cnMgJSAxMiArIDEyICogL3BtL2kudGVzdChhbVBNKSk7XG5cdFx0fSxcblx0XHRNOiBmdW5jdGlvbiBNKGRhdGVPYmosIHNob3J0TW9udGgpIHtcblx0XHRcdGRhdGVPYmouc2V0TW9udGgodGhpcy5sMTBuLm1vbnRocy5zaG9ydGhhbmQuaW5kZXhPZihzaG9ydE1vbnRoKSk7XG5cdFx0fSxcblx0XHRTOiBmdW5jdGlvbiBTKGRhdGVPYmosIHNlY29uZHMpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldFNlY29uZHMoc2Vjb25kcyk7XG5cdFx0fSxcblx0XHRXOiBmdW5jdGlvbiBXKCkge30sXG5cdFx0WTogZnVuY3Rpb24gWShkYXRlT2JqLCB5ZWFyKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRGdWxsWWVhcih5ZWFyKTtcblx0XHR9LFxuXHRcdFo6IGZ1bmN0aW9uIFooZGF0ZU9iaiwgSVNPRGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmogPSBuZXcgRGF0ZShJU09EYXRlKTtcblx0XHR9LFxuXG5cdFx0ZDogZnVuY3Rpb24gZChkYXRlT2JqLCBkYXkpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldERhdGUocGFyc2VGbG9hdChkYXkpKTtcblx0XHR9LFxuXHRcdGg6IGZ1bmN0aW9uIGgoZGF0ZU9iaiwgaG91cikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0SG91cnMocGFyc2VGbG9hdChob3VyKSk7XG5cdFx0fSxcblx0XHRpOiBmdW5jdGlvbiBpKGRhdGVPYmosIG1pbnV0ZXMpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldE1pbnV0ZXMocGFyc2VGbG9hdChtaW51dGVzKSk7XG5cdFx0fSxcblx0XHRqOiBmdW5jdGlvbiBqKGRhdGVPYmosIGRheSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0bDogZnVuY3Rpb24gbCgpIHt9LFxuXHRcdG06IGZ1bmN0aW9uIG0oZGF0ZU9iaiwgbW9udGgpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldE1vbnRoKHBhcnNlRmxvYXQobW9udGgpIC0gMSk7XG5cdFx0fSxcblx0XHRuOiBmdW5jdGlvbiBuKGRhdGVPYmosIG1vbnRoKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRNb250aChwYXJzZUZsb2F0KG1vbnRoKSAtIDEpO1xuXHRcdH0sXG5cdFx0czogZnVuY3Rpb24gcyhkYXRlT2JqLCBzZWNvbmRzKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRTZWNvbmRzKHBhcnNlRmxvYXQoc2Vjb25kcykpO1xuXHRcdH0sXG5cdFx0dzogZnVuY3Rpb24gdygpIHt9LFxuXHRcdHk6IGZ1bmN0aW9uIHkoZGF0ZU9iaiwgeWVhcikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RnVsbFllYXIoMjAwMCArIHBhcnNlRmxvYXQoeWVhcikpO1xuXHRcdH1cblx0fSxcblxuXHR0b2tlblJlZ2V4OiB7XG5cdFx0RDogXCIoXFxcXHcrKVwiLFxuXHRcdEY6IFwiKFxcXFx3KylcIixcblx0XHRIOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdEo6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXFxcXHcrXCIsXG5cdFx0SzogXCIoXFxcXHcrKVwiLFxuXHRcdE06IFwiKFxcXFx3KylcIixcblx0XHRTOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdFk6IFwiKFxcXFxkezR9KVwiLFxuXHRcdFo6IFwiKC4rKVwiLFxuXHRcdGQ6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0aDogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRpOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdGo6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0bDogXCIoXFxcXHcrKVwiLFxuXHRcdG06IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0bjogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRzOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdHc6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0eTogXCIoXFxcXGR7Mn0pXCJcblx0fSxcblxuXHRwYWQ6IGZ1bmN0aW9uIHBhZChudW1iZXIpIHtcblx0XHRyZXR1cm4gKFwiMFwiICsgbnVtYmVyKS5zbGljZSgtMik7XG5cdH0sXG5cblx0cGFyc2VEYXRlOiBmdW5jdGlvbiBwYXJzZURhdGUoZGF0ZSwgdGltZWxlc3MsIGdpdmVuRm9ybWF0KSB7XG5cdFx0aWYgKCFkYXRlKSByZXR1cm4gbnVsbDtcblxuXHRcdHZhciBkYXRlX29yaWcgPSBkYXRlO1xuXG5cdFx0aWYgKGRhdGUudG9GaXhlZCkgLy8gdGltZXN0YW1wXG5cdFx0XHRkYXRlID0gbmV3IERhdGUoZGF0ZSk7ZWxzZSBpZiAodHlwZW9mIGRhdGUgPT09IFwic3RyaW5nXCIpIHtcblx0XHRcdHZhciBmb3JtYXQgPSB0eXBlb2YgZ2l2ZW5Gb3JtYXQgPT09IFwic3RyaW5nXCIgPyBnaXZlbkZvcm1hdCA6IHRoaXMuY29uZmlnLmRhdGVGb3JtYXQ7XG5cdFx0XHRkYXRlID0gZGF0ZS50cmltKCk7XG5cblx0XHRcdGlmIChkYXRlID09PSBcInRvZGF5XCIpIHtcblx0XHRcdFx0ZGF0ZSA9IG5ldyBEYXRlKCk7XG5cdFx0XHRcdHRpbWVsZXNzID0gdHJ1ZTtcblx0XHRcdH0gZWxzZSBpZiAodGhpcy5jb25maWcgJiYgdGhpcy5jb25maWcucGFyc2VEYXRlKSBkYXRlID0gdGhpcy5jb25maWcucGFyc2VEYXRlKGRhdGUpO2Vsc2UgaWYgKC9aJC8udGVzdChkYXRlKSB8fCAvR01UJC8udGVzdChkYXRlKSkgLy8gZGF0ZXN0cmluZ3Mgdy8gdGltZXpvbmVcblx0XHRcdFx0ZGF0ZSA9IG5ldyBEYXRlKGRhdGUpO2Vsc2Uge1xuXHRcdFx0XHR2YXIgcGFyc2VkRGF0ZSA9IHRoaXMuY29uZmlnLm5vQ2FsZW5kYXIgPyBuZXcgRGF0ZShuZXcgRGF0ZSgpLnNldEhvdXJzKDAsIDAsIDAsIDApKSA6IG5ldyBEYXRlKG5ldyBEYXRlKCkuZ2V0RnVsbFllYXIoKSwgMCwgMSwgMCwgMCwgMCwgMCk7XG5cblx0XHRcdFx0dmFyIG1hdGNoZWQgPSBmYWxzZTtcblxuXHRcdFx0XHRmb3IgKHZhciBpID0gMCwgbWF0Y2hJbmRleCA9IDAsIHJlZ2V4U3RyID0gXCJcIjsgaSA8IGZvcm1hdC5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRcdHZhciB0b2tlbiA9IGZvcm1hdFtpXTtcblx0XHRcdFx0XHR2YXIgaXNCYWNrU2xhc2ggPSB0b2tlbiA9PT0gXCJcXFxcXCI7XG5cdFx0XHRcdFx0dmFyIGVzY2FwZWQgPSBmb3JtYXRbaSAtIDFdID09PSBcIlxcXFxcIiB8fCBpc0JhY2tTbGFzaDtcblx0XHRcdFx0XHRpZiAodGhpcy50b2tlblJlZ2V4W3Rva2VuXSAmJiAhZXNjYXBlZCkge1xuXHRcdFx0XHRcdFx0cmVnZXhTdHIgKz0gdGhpcy50b2tlblJlZ2V4W3Rva2VuXTtcblx0XHRcdFx0XHRcdHZhciBtYXRjaCA9IG5ldyBSZWdFeHAocmVnZXhTdHIpLmV4ZWMoZGF0ZSk7XG5cdFx0XHRcdFx0XHRpZiAobWF0Y2ggJiYgKG1hdGNoZWQgPSB0cnVlKSkgdGhpcy5yZXZGb3JtYXRbdG9rZW5dKHBhcnNlZERhdGUsIG1hdGNoWysrbWF0Y2hJbmRleF0pO1xuXHRcdFx0XHRcdH0gZWxzZSBpZiAoIWlzQmFja1NsYXNoKSByZWdleFN0ciArPSBcIi5cIjsgLy8gZG9uJ3QgcmVhbGx5IGNhcmVcblx0XHRcdFx0fVxuXG5cdFx0XHRcdGRhdGUgPSBtYXRjaGVkID8gcGFyc2VkRGF0ZSA6IG51bGw7XG5cdFx0XHR9XG5cdFx0fSBlbHNlIGlmIChkYXRlIGluc3RhbmNlb2YgRGF0ZSkgZGF0ZSA9IG5ldyBEYXRlKGRhdGUuZ2V0VGltZSgpKTsgLy8gY3JlYXRlIGEgY29weVxuXG5cdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRpZiAoIShkYXRlIGluc3RhbmNlb2YgRGF0ZSkpIHtcblx0XHRcdGNvbnNvbGUud2FybihcImZsYXRwaWNrcjogaW52YWxpZCBkYXRlIFwiICsgZGF0ZV9vcmlnKTtcblx0XHRcdGNvbnNvbGUuaW5mbyh0aGlzLmVsZW1lbnQpO1xuXHRcdFx0cmV0dXJuIG51bGw7XG5cdFx0fVxuXG5cdFx0aWYgKHRoaXMuY29uZmlnICYmIHRoaXMuY29uZmlnLnV0YyAmJiAhZGF0ZS5mcF9pc1VUQykgZGF0ZSA9IGRhdGUuZnBfdG9VVEMoKTtcblxuXHRcdGlmICh0aW1lbGVzcyA9PT0gdHJ1ZSkgZGF0ZS5zZXRIb3VycygwLCAwLCAwLCAwKTtcblxuXHRcdHJldHVybiBkYXRlO1xuXHR9XG59O1xuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuZnVuY3Rpb24gX2ZsYXRwaWNrcihub2RlTGlzdCwgY29uZmlnKSB7XG5cdHZhciBub2RlcyA9IEFycmF5LnByb3RvdHlwZS5zbGljZS5jYWxsKG5vZGVMaXN0KTsgLy8gc3RhdGljIGxpc3Rcblx0dmFyIGluc3RhbmNlcyA9IFtdO1xuXHRmb3IgKHZhciBpID0gMDsgaSA8IG5vZGVzLmxlbmd0aDsgaSsrKSB7XG5cdFx0dHJ5IHtcblx0XHRcdG5vZGVzW2ldLl9mbGF0cGlja3IgPSBuZXcgRmxhdHBpY2tyKG5vZGVzW2ldLCBjb25maWcgfHwge30pO1xuXHRcdFx0aW5zdGFuY2VzLnB1c2gobm9kZXNbaV0uX2ZsYXRwaWNrcik7XG5cdFx0fSBjYXRjaCAoZSkge1xuXHRcdFx0Y29uc29sZS53YXJuKGUsIGUuc3RhY2spO1xuXHRcdH1cblx0fVxuXG5cdHJldHVybiBpbnN0YW5jZXMubGVuZ3RoID09PSAxID8gaW5zdGFuY2VzWzBdIDogaW5zdGFuY2VzO1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuaWYgKHR5cGVvZiBIVE1MRWxlbWVudCAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHQvLyBicm93c2VyIGVudlxuXHRIVE1MQ29sbGVjdGlvbi5wcm90b3R5cGUuZmxhdHBpY2tyID0gTm9kZUxpc3QucHJvdG90eXBlLmZsYXRwaWNrciA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0XHRyZXR1cm4gX2ZsYXRwaWNrcih0aGlzLCBjb25maWcpO1xuXHR9O1xuXG5cdEhUTUxFbGVtZW50LnByb3RvdHlwZS5mbGF0cGlja3IgPSBmdW5jdGlvbiAoY29uZmlnKSB7XG5cdFx0cmV0dXJuIF9mbGF0cGlja3IoW3RoaXNdLCBjb25maWcpO1xuXHR9O1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuZnVuY3Rpb24gZmxhdHBpY2tyKHNlbGVjdG9yLCBjb25maWcpIHtcblx0cmV0dXJuIF9mbGF0cGlja3Iod2luZG93LmRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoc2VsZWN0b3IpLCBjb25maWcpO1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuaWYgKHR5cGVvZiBqUXVlcnkgIT09IFwidW5kZWZpbmVkXCIpIHtcblx0alF1ZXJ5LmZuLmZsYXRwaWNrciA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0XHRyZXR1cm4gX2ZsYXRwaWNrcih0aGlzLCBjb25maWcpO1xuXHR9O1xufVxuXG5EYXRlLnByb3RvdHlwZS5mcF9pbmNyID0gZnVuY3Rpb24gKGRheXMpIHtcblx0cmV0dXJuIG5ldyBEYXRlKHRoaXMuZ2V0RnVsbFllYXIoKSwgdGhpcy5nZXRNb250aCgpLCB0aGlzLmdldERhdGUoKSArIHBhcnNlSW50KGRheXMsIDEwKSk7XG59O1xuXG5EYXRlLnByb3RvdHlwZS5mcF9pc1VUQyA9IGZhbHNlO1xuRGF0ZS5wcm90b3R5cGUuZnBfdG9VVEMgPSBmdW5jdGlvbiAoKSB7XG5cdHZhciBuZXdEYXRlID0gbmV3IERhdGUodGhpcy5nZXRVVENGdWxsWWVhcigpLCB0aGlzLmdldFVUQ01vbnRoKCksIHRoaXMuZ2V0VVRDRGF0ZSgpLCB0aGlzLmdldFVUQ0hvdXJzKCksIHRoaXMuZ2V0VVRDTWludXRlcygpLCB0aGlzLmdldFVUQ1NlY29uZHMoKSk7XG5cblx0bmV3RGF0ZS5mcF9pc1VUQyA9IHRydWU7XG5cdHJldHVybiBuZXdEYXRlO1xufTtcblxuLy8gSUU5IGNsYXNzTGlzdCBwb2x5ZmlsbFxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbmlmICghd2luZG93LmRvY3VtZW50LmRvY3VtZW50RWxlbWVudC5jbGFzc0xpc3QgJiYgT2JqZWN0LmRlZmluZVByb3BlcnR5ICYmIHR5cGVvZiBIVE1MRWxlbWVudCAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHRPYmplY3QuZGVmaW5lUHJvcGVydHkoSFRNTEVsZW1lbnQucHJvdG90eXBlLCBcImNsYXNzTGlzdFwiLCB7XG5cdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHR2YXIgc2VsZiA9IHRoaXM7XG5cdFx0XHRmdW5jdGlvbiB1cGRhdGUoZm4pIHtcblx0XHRcdFx0cmV0dXJuIGZ1bmN0aW9uICh2YWx1ZSkge1xuXHRcdFx0XHRcdHZhciBjbGFzc2VzID0gc2VsZi5jbGFzc05hbWUuc3BsaXQoL1xccysvKSxcblx0XHRcdFx0XHQgICAgaW5kZXggPSBjbGFzc2VzLmluZGV4T2YodmFsdWUpO1xuXG5cdFx0XHRcdFx0Zm4oY2xhc3NlcywgaW5kZXgsIHZhbHVlKTtcblx0XHRcdFx0XHRzZWxmLmNsYXNzTmFtZSA9IGNsYXNzZXMuam9pbihcIiBcIik7XG5cdFx0XHRcdH07XG5cdFx0XHR9XG5cblx0XHRcdHZhciByZXQgPSB7XG5cdFx0XHRcdGFkZDogdXBkYXRlKGZ1bmN0aW9uIChjbGFzc2VzLCBpbmRleCwgdmFsdWUpIHtcblx0XHRcdFx0XHRpZiAoIX5pbmRleCkgY2xhc3Nlcy5wdXNoKHZhbHVlKTtcblx0XHRcdFx0fSksXG5cblx0XHRcdFx0cmVtb3ZlOiB1cGRhdGUoZnVuY3Rpb24gKGNsYXNzZXMsIGluZGV4KSB7XG5cdFx0XHRcdFx0aWYgKH5pbmRleCkgY2xhc3Nlcy5zcGxpY2UoaW5kZXgsIDEpO1xuXHRcdFx0XHR9KSxcblxuXHRcdFx0XHR0b2dnbGU6IHVwZGF0ZShmdW5jdGlvbiAoY2xhc3NlcywgaW5kZXgsIHZhbHVlKSB7XG5cdFx0XHRcdFx0aWYgKH5pbmRleCkgY2xhc3Nlcy5zcGxpY2UoaW5kZXgsIDEpO2Vsc2UgY2xhc3Nlcy5wdXNoKHZhbHVlKTtcblx0XHRcdFx0fSksXG5cblx0XHRcdFx0Y29udGFpbnM6IGZ1bmN0aW9uIGNvbnRhaW5zKHZhbHVlKSB7XG5cdFx0XHRcdFx0cmV0dXJuICEhfnNlbGYuY2xhc3NOYW1lLnNwbGl0KC9cXHMrLykuaW5kZXhPZih2YWx1ZSk7XG5cdFx0XHRcdH0sXG5cblx0XHRcdFx0aXRlbTogZnVuY3Rpb24gaXRlbShpKSB7XG5cdFx0XHRcdFx0cmV0dXJuIHNlbGYuY2xhc3NOYW1lLnNwbGl0KC9cXHMrLylbaV0gfHwgbnVsbDtcblx0XHRcdFx0fVxuXHRcdFx0fTtcblxuXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHJldCwgXCJsZW5ndGhcIiwge1xuXHRcdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0XHRyZXR1cm4gc2VsZi5jbGFzc05hbWUuc3BsaXQoL1xccysvKS5sZW5ndGg7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXG5cdFx0XHRyZXR1cm4gcmV0O1xuXHRcdH1cblx0fSk7XG59XG5cbmlmICh0eXBlb2YgbW9kdWxlICE9PSBcInVuZGVmaW5lZFwiKSBtb2R1bGUuZXhwb3J0cyA9IEZsYXRwaWNrcjtcbiIsInZhciBWdWUgLy8gbGF0ZSBiaW5kXG52YXIgdmVyc2lvblxudmFyIG1hcCA9IHdpbmRvdy5fX1ZVRV9IT1RfTUFQX18gPSBPYmplY3QuY3JlYXRlKG51bGwpXG52YXIgaW5zdGFsbGVkID0gZmFsc2VcbnZhciBpc0Jyb3dzZXJpZnkgPSBmYWxzZVxudmFyIGluaXRIb29rTmFtZSA9ICdiZWZvcmVDcmVhdGUnXG5cbmV4cG9ydHMuaW5zdGFsbCA9IGZ1bmN0aW9uICh2dWUsIGJyb3dzZXJpZnkpIHtcbiAgaWYgKGluc3RhbGxlZCkgcmV0dXJuXG4gIGluc3RhbGxlZCA9IHRydWVcblxuICBWdWUgPSB2dWUuX19lc01vZHVsZSA/IHZ1ZS5kZWZhdWx0IDogdnVlXG4gIHZlcnNpb24gPSBWdWUudmVyc2lvbi5zcGxpdCgnLicpLm1hcChOdW1iZXIpXG4gIGlzQnJvd3NlcmlmeSA9IGJyb3dzZXJpZnlcblxuICAvLyBjb21wYXQgd2l0aCA8IDIuMC4wLWFscGhhLjdcbiAgaWYgKFZ1ZS5jb25maWcuX2xpZmVjeWNsZUhvb2tzLmluZGV4T2YoJ2luaXQnKSA+IC0xKSB7XG4gICAgaW5pdEhvb2tOYW1lID0gJ2luaXQnXG4gIH1cblxuICBleHBvcnRzLmNvbXBhdGlibGUgPSB2ZXJzaW9uWzBdID49IDJcbiAgaWYgKCFleHBvcnRzLmNvbXBhdGlibGUpIHtcbiAgICBjb25zb2xlLndhcm4oXG4gICAgICAnW0hNUl0gWW91IGFyZSB1c2luZyBhIHZlcnNpb24gb2YgdnVlLWhvdC1yZWxvYWQtYXBpIHRoYXQgaXMgJyArXG4gICAgICAnb25seSBjb21wYXRpYmxlIHdpdGggVnVlLmpzIGNvcmUgXjIuMC4wLidcbiAgICApXG4gICAgcmV0dXJuXG4gIH1cbn1cblxuLyoqXG4gKiBDcmVhdGUgYSByZWNvcmQgZm9yIGEgaG90IG1vZHVsZSwgd2hpY2gga2VlcHMgdHJhY2sgb2YgaXRzIGNvbnN0cnVjdG9yXG4gKiBhbmQgaW5zdGFuY2VzXG4gKlxuICogQHBhcmFtIHtTdHJpbmd9IGlkXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICovXG5cbmV4cG9ydHMuY3JlYXRlUmVjb3JkID0gZnVuY3Rpb24gKGlkLCBvcHRpb25zKSB7XG4gIHZhciBDdG9yID0gbnVsbFxuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBDdG9yID0gb3B0aW9uc1xuICAgIG9wdGlvbnMgPSBDdG9yLm9wdGlvbnNcbiAgfVxuICBtYWtlT3B0aW9uc0hvdChpZCwgb3B0aW9ucylcbiAgbWFwW2lkXSA9IHtcbiAgICBDdG9yOiBWdWUuZXh0ZW5kKG9wdGlvbnMpLFxuICAgIGluc3RhbmNlczogW11cbiAgfVxufVxuXG4vKipcbiAqIE1ha2UgYSBDb21wb25lbnQgb3B0aW9ucyBvYmplY3QgaG90LlxuICpcbiAqIEBwYXJhbSB7U3RyaW5nfSBpZFxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqL1xuXG5mdW5jdGlvbiBtYWtlT3B0aW9uc0hvdCAoaWQsIG9wdGlvbnMpIHtcbiAgaW5qZWN0SG9vayhvcHRpb25zLCBpbml0SG9va05hbWUsIGZ1bmN0aW9uICgpIHtcbiAgICBtYXBbaWRdLmluc3RhbmNlcy5wdXNoKHRoaXMpXG4gIH0pXG4gIGluamVjdEhvb2sob3B0aW9ucywgJ2JlZm9yZURlc3Ryb3knLCBmdW5jdGlvbiAoKSB7XG4gICAgdmFyIGluc3RhbmNlcyA9IG1hcFtpZF0uaW5zdGFuY2VzXG4gICAgaW5zdGFuY2VzLnNwbGljZShpbnN0YW5jZXMuaW5kZXhPZih0aGlzKSwgMSlcbiAgfSlcbn1cblxuLyoqXG4gKiBJbmplY3QgYSBob29rIHRvIGEgaG90IHJlbG9hZGFibGUgY29tcG9uZW50IHNvIHRoYXRcbiAqIHdlIGNhbiBrZWVwIHRyYWNrIG9mIGl0LlxuICpcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKiBAcGFyYW0ge1N0cmluZ30gbmFtZVxuICogQHBhcmFtIHtGdW5jdGlvbn0gaG9va1xuICovXG5cbmZ1bmN0aW9uIGluamVjdEhvb2sgKG9wdGlvbnMsIG5hbWUsIGhvb2spIHtcbiAgdmFyIGV4aXN0aW5nID0gb3B0aW9uc1tuYW1lXVxuICBvcHRpb25zW25hbWVdID0gZXhpc3RpbmdcbiAgICA/IEFycmF5LmlzQXJyYXkoZXhpc3RpbmcpXG4gICAgICA/IGV4aXN0aW5nLmNvbmNhdChob29rKVxuICAgICAgOiBbZXhpc3RpbmcsIGhvb2tdXG4gICAgOiBbaG9va11cbn1cblxuZnVuY3Rpb24gdHJ5V3JhcCAoZm4pIHtcbiAgcmV0dXJuIGZ1bmN0aW9uIChpZCwgYXJnKSB7XG4gICAgdHJ5IHsgZm4oaWQsIGFyZykgfSBjYXRjaCAoZSkge1xuICAgICAgY29uc29sZS5lcnJvcihlKVxuICAgICAgY29uc29sZS53YXJuKCdTb21ldGhpbmcgd2VudCB3cm9uZyBkdXJpbmcgVnVlIGNvbXBvbmVudCBob3QtcmVsb2FkLiBGdWxsIHJlbG9hZCByZXF1aXJlZC4nKVxuICAgIH1cbiAgfVxufVxuXG5leHBvcnRzLnJlcmVuZGVyID0gdHJ5V3JhcChmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgdmFyIHJlY29yZCA9IG1hcFtpZF1cbiAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgb3B0aW9ucyA9IG9wdGlvbnMub3B0aW9uc1xuICB9XG4gIHJlY29yZC5DdG9yLm9wdGlvbnMucmVuZGVyID0gb3B0aW9ucy5yZW5kZXJcbiAgcmVjb3JkLkN0b3Iub3B0aW9ucy5zdGF0aWNSZW5kZXJGbnMgPSBvcHRpb25zLnN0YXRpY1JlbmRlckZuc1xuICByZWNvcmQuaW5zdGFuY2VzLnNsaWNlKCkuZm9yRWFjaChmdW5jdGlvbiAoaW5zdGFuY2UpIHtcbiAgICBpbnN0YW5jZS4kb3B0aW9ucy5yZW5kZXIgPSBvcHRpb25zLnJlbmRlclxuICAgIGluc3RhbmNlLiRvcHRpb25zLnN0YXRpY1JlbmRlckZucyA9IG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zXG4gICAgaW5zdGFuY2UuX3N0YXRpY1RyZWVzID0gW10gLy8gcmVzZXQgc3RhdGljIHRyZWVzXG4gICAgaW5zdGFuY2UuJGZvcmNlVXBkYXRlKClcbiAgfSlcbn0pXG5cbmV4cG9ydHMucmVsb2FkID0gdHJ5V3JhcChmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgb3B0aW9ucyA9IG9wdGlvbnMub3B0aW9uc1xuICB9XG4gIG1ha2VPcHRpb25zSG90KGlkLCBvcHRpb25zKVxuICB2YXIgcmVjb3JkID0gbWFwW2lkXVxuICBpZiAodmVyc2lvblsxXSA8IDIpIHtcbiAgICAvLyBwcmVzZXJ2ZSBwcmUgMi4yIGJlaGF2aW9yIGZvciBnbG9iYWwgbWl4aW4gaGFuZGxpbmdcbiAgICByZWNvcmQuQ3Rvci5leHRlbmRPcHRpb25zID0gb3B0aW9uc1xuICB9XG4gIHZhciBuZXdDdG9yID0gcmVjb3JkLkN0b3Iuc3VwZXIuZXh0ZW5kKG9wdGlvbnMpXG4gIHJlY29yZC5DdG9yLm9wdGlvbnMgPSBuZXdDdG9yLm9wdGlvbnNcbiAgcmVjb3JkLkN0b3IuY2lkID0gbmV3Q3Rvci5jaWRcbiAgcmVjb3JkLkN0b3IucHJvdG90eXBlID0gbmV3Q3Rvci5wcm90b3R5cGVcbiAgaWYgKG5ld0N0b3IucmVsZWFzZSkge1xuICAgIC8vIHRlbXBvcmFyeSBnbG9iYWwgbWl4aW4gc3RyYXRlZ3kgdXNlZCBpbiA8IDIuMC4wLWFscGhhLjZcbiAgICBuZXdDdG9yLnJlbGVhc2UoKVxuICB9XG4gIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgIGlmIChpbnN0YW5jZS4kdm5vZGUgJiYgaW5zdGFuY2UuJHZub2RlLmNvbnRleHQpIHtcbiAgICAgIGluc3RhbmNlLiR2bm9kZS5jb250ZXh0LiRmb3JjZVVwZGF0ZSgpXG4gICAgfSBlbHNlIHtcbiAgICAgIGNvbnNvbGUud2FybignUm9vdCBvciBtYW51YWxseSBtb3VudGVkIGluc3RhbmNlIG1vZGlmaWVkLiBGdWxsIHJlbG9hZCByZXF1aXJlZC4nKVxuICAgIH1cbiAgfSlcbn0pXG4iLCLvu788dGVtcGxhdGU+XHJcbiAgICA8ZGl2IGNsYXNzPVwiZm9ybS1kYXRlIGlucHV0LWdyb3VwXCI+XHJcbiAgICAgICAgPGlucHV0IHR5cGU9XCJ0ZXh0XCIgOmNsYXNzPVwiaW5wdXRDbGFzc1wiIDpwbGFjZWhvbGRlcj1cInBsYWNlaG9sZGVyXCIgOnZhbHVlPVwidmFsdWVcIiBAaW5wdXQ9XCJvbklucHV0XCIgLz5cclxuICAgICAgICA8c3BhbiBjbGFzcz1cImlucHV0LWdyb3VwLWFkZG9uXCI+XHJcbiAgICAgICAgICAgIDxzcGFuIGNsYXNzPVwiY2FsZW5kYXJcIj48L3NwYW4+XHJcbiAgICAgICAgPC9zcGFuPlxyXG4gICAgPC9kaXY+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG5pbXBvcnQgRmxhdHBpY2tyIGZyb20gJ2ZsYXRwaWNrcidcclxuXHJcbmV4cG9ydCBkZWZhdWx0IHtcclxuICAgIHByb3BzOiB7XHJcbiAgICAgICAgaW5wdXRDbGFzczoge1xyXG4gICAgICAgICAgICB0eXBlOiBTdHJpbmdcclxuICAgICAgICB9LFxyXG4gICAgICAgIHBsYWNlaG9sZGVyOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IFN0cmluZyxcclxuICAgICAgICAgICAgZGVmYXVsdDogJydcclxuICAgICAgICB9LFxyXG4gICAgICAgIG9wdGlvbnM6IHtcclxuICAgICAgICAgICAgdHlwZTogT2JqZWN0LFxyXG4gICAgICAgICAgICBkZWZhdWx0OiAoKSA9PiB7IHJldHVybiB7fSB9XHJcbiAgICAgICAgfSxcclxuICAgICAgICB2YWx1ZToge1xyXG4gICAgICAgICAgICB0eXBlOiBTdHJpbmcsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICcnXHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICBkYXRhICgpIHtcclxuICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgIGZwOiBudWxsXHJcbiAgICAgIH1cclxuICB9LFxyXG4gICAgY29tcHV0ZWQ6IHtcclxuICAgICAgICBmcE9wdGlvbnMgKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gSlNPTi5zdHJpbmdpZnkodGhpcy5vcHRpb25zKVxyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgICB3YXRjaDoge1xyXG4gICAgICAgIGZwT3B0aW9ucyAobmV3T3B0KSB7XHJcbiAgICAgICAgICAgIGNvbnN0IG9wdGlvbiA9IEpTT04ucGFyc2UobmV3T3B0KVxyXG4gICAgICAgICAgICBmb3IgKGxldCBvIGluIG9wdGlvbikge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5mcC5zZXQobywgb3B0aW9uW29dKVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICBtb3VudGVkICgpIHtcclxuICAgICAgY29uc3Qgc2VsZiA9IHRoaXNcclxuICAgICAgY29uc3Qgb3JpZ09uVmFsVXBkYXRlID0gdGhpcy5vcHRpb25zLm9uVmFsdWVVcGRhdGVcclxuICAgICAgdGhpcy5mcCA9IG5ldyBGbGF0cGlja3IodGhpcy4kZWwucXVlcnlTZWxlY3RvcignaW5wdXQnKSwgT2JqZWN0LmFzc2lnbih0aGlzLm9wdGlvbnMsIHtcclxuICAgICAgICAgIG9uVmFsdWVVcGRhdGUgKCkge1xyXG4gICAgICAgICAgICAgIHNlbGYub25JbnB1dChzZWxmLiRlbC5xdWVyeVNlbGVjdG9yKCdpbnB1dCcpLnZhbHVlKVxyXG4gICAgICAgICAgICAgIGlmICh0eXBlb2Ygb3JpZ09uVmFsVXBkYXRlID09PSAnZnVuY3Rpb24nKSB7XHJcbiAgICAgICAgICAgICAgICAgIG9yaWdPblZhbFVwZGF0ZSgpXHJcbiAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgfVxyXG4gICAgICB9KSlcclxuICAgICAgdGhpcy4kZW1pdCgnRmxhdHBpY2tyUmVmJywgdGhpcy5mcClcclxuICB9LFxyXG4gIGRlc3Ryb3llZCAoKSB7XHJcbiAgICAgIHRoaXMuZnAuZGVzdHJveSgpXHJcbiAgICAgIHRoaXMuZnAgPSBudWxsXHJcbiAgfSxcclxuICAgIG1ldGhvZHM6IHtcclxuICAgICAgICBvbklucHV0IChlKSB7XHJcbiAgICAgICAgICAgIGNvbnN0IHNlbGVjdGVkRGF0ZXMgPSB0aGlzLmZwLnNlbGVjdGVkRGF0ZXMgfHwgW107XHJcbiAgICAgICAgICAgIGNvbnN0IGxlZnQgPSBzZWxlY3RlZERhdGVzLmxlbmd0aCA+IDAgPyBzZWxlY3RlZERhdGVzWzBdIDogbnVsbDtcclxuICAgICAgICAgICAgY29uc3QgcmlnaHQgPSBzZWxlY3RlZERhdGVzLmxlbmd0aCA+IDEgPyBzZWxlY3RlZERhdGVzWzFdIDogbnVsbDtcclxuICAgICAgICAgICAgdGhpcy4kZW1pdCgnaW5wdXQnLCAodHlwZW9mIGUgPT09ICdzdHJpbmcnID8gZSA6IGUudGFyZ2V0LnZhbHVlKSwgbGVmdCwgcmlnaHQpXHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG59XHJcbjwvc2NyaXB0PiIsIu+7vzx0ZW1wbGF0ZT5cclxuICAgIDx0YWJsZSBjbGFzcz1cInRhYmxlIHRhYmxlLXN0cmlwZWQgdGFibGUtb3JkZXJlZCB0YWJsZS1ib3JkZXJlZCB0YWJsZS1ob3ZlciB0YWJsZS13aXRoLWNoZWNrYm94ZXMgdGFibGUtd2l0aC1wcmVmaWxsZWQtY29sdW1uIHRhYmxlLWludGVydmlld3NcIj5cclxuICAgICAgICA8dGhlYWQ+XHJcbiAgICAgICAgICAgIDx0cj5cclxuICAgICAgICAgICAgICAgIDx0aCBjbGFzcz1cIiBpbnRlcnZpZXctaWQgdGl0bGUtcm93XCI+XHJcbiAgICAgICAgICAgICAgICAgICAgSW50ZXJ2aWV3IGlkXHJcbiAgICAgICAgICAgICAgICA8L3RoPlxyXG4gICAgICAgICAgICAgICAgPHRoIGNsYXNzPVwiaGFzLWNvbW1lbnRzXCI+PHNwYW4gY2xhc3M9XCJjb21tZW50LWljb24gcmVzcG9uZGVkXCI+PC9zcGFuPjwvdGg+XHJcbiAgICAgICAgICAgICAgICA8dGggY2xhc3M9XCJ1cGxvYWRlZC10by1ocVwiPlVwbG9hZGVkIHRvIEhRPC90aD5cclxuICAgICAgICAgICAgICAgIDx0aCBjbGFzcz1cImludGVydmlldy1jb25kdWN0ZWRcIj5pbnRlcnZpZXcgY29uZHVjdGVkPC90aD5cclxuICAgICAgICAgICAgICAgIDx0aCBjbGFzcz1cImZsYWdzXCI+RmxhZ3M8L3RoPlxyXG4gICAgICAgICAgICAgICAgPHRoIGNsYXNzPVwic3RhdHVzXCI+Y3VycmVudCBTdGF0dXM8L3RoPlxyXG4gICAgICAgICAgICAgICAgPHRoIGNsYXNzPVwiYW5zd2VyZWQtcXVlc3Rpb25zXCI+YW5zd2VyZWQgcXVlc3Rpb25zPC90aD5cclxuICAgICAgICAgICAgICAgIDx0aCBjbGFzcz1cImxlZnQtZW1wdHlcIj5sZWZ0IGVtcHR5PC90aD5cclxuICAgICAgICAgICAgICAgIDx0aCBjbGFzcz1cImVycm9yc1wiPmVycm9yczwvdGg+XHJcbiAgICAgICAgICAgICAgICA8dGggY2xhc3M9XCJkYXRlIGxhc3QtdXBkYXRlXCI+bGFzdCB1cGRhdGU8L3RoPlxyXG4gICAgICAgICAgICAgICAgPHRoIGNsYXNzPVwiZG93bmxvYWQtb24tZGV2aWNlXCI+ZG93bmxvYWRlZCBvbiBkZXZpY2U8L3RoPlxyXG4gICAgICAgICAgICA8L3RyPlxyXG4gICAgICAgIDwvdGhlYWQ+XHJcbiAgICAgICAgPHRib2R5PjwvdGJvZHk+XHJcbiAgICA8L3RhYmxlPlxyXG48L3RlbXBsYXRlPlxyXG5cclxuPHNjcmlwdD5cclxuZXhwb3J0IGRlZmF1bHQge1xyXG4gICAgcHJvcHM6IHtcclxuICAgICAgICBmaWx0ZXI6IHtcclxuICAgICAgICAgICAgdHlwZTogT2JqZWN0LFxyXG4gICAgICAgICAgICBkZWZhdWx0OiAoKSA9PiB7IHJldHVybiB7fSB9XHJcbiAgICAgICAgfSxcclxuICAgICAgIGZldGNoVXJsOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IFN0cmluZyxcclxuICAgICAgIH1cclxuICAgIH0sXHJcbiAgICBkYXRhICgpIHtcclxuICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICB0YWJsZTogbnVsbFxyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgICBjb21wdXRlZDoge1xyXG4gICAgICAgIGludGVydmlld0ZpbHRlcnMgKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gSlNPTi5zdHJpbmdpZnkodGhpcy5maWx0ZXIpXHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICAgIHdhdGNoOiB7XHJcbiAgICAgICAgaW50ZXJ2aWV3RmlsdGVycyAobmV3RmlsdGVycykge1xyXG4gICAgICAgICAgICB0aGlzLnRhYmxlLmFqYXgucmVsb2FkKCk7XHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICAgIG1vdW50ZWQgKCkge1xyXG4gICAgICAgIGNvbnN0IHNlbGYgPSB0aGlzXHJcbiAgICAgICAgdGhpcy50YWJsZSA9ICQodGhpcy4kZWwpLkRhdGFUYWJsZSh7XHJcbiAgICAgICAgICAgIHByb2Nlc3Npbmc6IHRydWUsXHJcbiAgICAgICAgICAgIHNlcnZlclNpZGU6IHRydWUsXHJcbiAgICAgICAgICAgIGFqYXg6IHtcclxuICAgICAgICAgICAgICAgIHVybDogdGhpcy5mZXRjaFVybCxcclxuICAgICAgICAgICAgICAgIHR5cGU6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICAgICAgZGF0YTogc2VsZi5maWx0ZXJcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgc2VhcmNoSGlnaGxpZ2h0OiB0cnVlLFxyXG4gICAgICAgICAgICByb3dJZDogJ2lkJyxcclxuICAgICAgICAgICAgcGFnaW5nVHlwZTogXCJmdWxsX251bWJlcnNcIixcclxuICAgICAgICAgICAgbGVuZ3RoQ2hhbmdlOiBmYWxzZSwgLy8gZG8gbm90IHNob3cgcGFnZSBzaXplIHNlbGVjdG9yXHJcbiAgICAgICAgICAgIHBhZ2VMZW5ndGg6IDUwLCAvLyBwYWdlIHNpemVcclxuICAgICAgICAgICAgXCJvcmRlclwiOiBbWzIsICdkZXNjJ11dLFxyXG4gICAgICAgICAgICBkb206IFwiZnJ0cFwiLFxyXG4gICAgICAgICAgICBjb25kaXRpb25hbFBhZ2luZzogdHJ1ZVxyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHRoaXMuJGVtaXQoJ0RhdGFUYWJsZVJlZicsIHRoaXMudGFibGUpXHJcbiAgICB9LFxyXG4gICAgZGVzdHJveWVkICgpIHtcclxuICAgIH1cclxufVxyXG48L3NjcmlwdD4iLCLvu788dGVtcGxhdGU+XHJcbiAgICA8ZGl2IGNsYXNzPVwiY29tYm8tYm94XCI+XHJcbiAgICAgICAgPGRpdiBjbGFzcz1cImJ0bi1ncm91cCBidG4taW5wdXQgY2xlYXJmaXhcIj5cclxuICAgICAgICAgICAgPGJ1dHRvbiB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4gZHJvcGRvd24tdG9nZ2xlXCIgZGF0YS10b2dnbGU9XCJkcm9wZG93blwiPlxyXG4gICAgICAgICAgICAgICAgPHNwYW4gZGF0YS1iaW5kPVwibGFiZWxcIiB2LWlmPVwidmFsdWUgPT09IG51bGxcIiBjbGFzcz1cImdyYXktdGV4dFwiPnt7cGxhY2Vob2xkZXJUZXh0fX08L3NwYW4+XHJcbiAgICAgICAgICAgICAgICA8c3BhbiBkYXRhLWJpbmQ9XCJsYWJlbFwiIHYtZWxzZT57e3ZhbHVlLnZhbHVlfX08L3NwYW4+XHJcbiAgICAgICAgICAgIDwvYnV0dG9uPlxyXG4gICAgICAgICAgICA8dWwgcmVmPVwiZHJvcGRvd25NZW51XCIgY2xhc3M9XCJkcm9wZG93bi1tZW51XCIgcm9sZT1cIm1lbnVcIj5cclxuICAgICAgICAgICAgICAgIDxsaT5cclxuICAgICAgICAgICAgICAgICAgICA8aW5wdXQgdHlwZT1cInRleHRcIiByZWY9XCJzZWFyY2hCb3hcIiA6aWQ9XCJpbnB1dElkXCIgcGxhY2Vob2xkZXI9XCJTZWFyY2hcIiBAaW5wdXQ9XCJ1cGRhdGVPcHRpb25zTGlzdFwiIHYtb246a2V5dXAuZG93bj1cIm9uU2VhcmNoQm94RG93bktleVwiIHYtbW9kZWw9XCJzZWFyY2hUZXJtXCIgLz5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1mb3I9XCJvcHRpb24gaW4gb3B0aW9uc1wiIDprZXk9XCJvcHRpb24ua2V5XCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGEgaHJlZj1cImphdmFzY3JpcHQ6dm9pZCgwKTtcIiB2LW9uOmNsaWNrPVwic2VsZWN0T3B0aW9uKG9wdGlvbilcIiB2LWh0bWw9XCJoaWdobGlnaHQob3B0aW9uLnZhbHVlLCBzZWFyY2hUZXJtKVwiIHYtb246a2V5ZG93bi51cD1cIm9uT3B0aW9uVXBLZXlcIj48L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtaWY9XCJpc0xvYWRpbmdcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YT5Mb2FkaW5nLi4uPC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWlmPVwiIWlzTG9hZGluZyAmJiBvcHRpb25zLmxlbmd0aCA9PT0gMFwiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhPk5vIHJlc3VsdHMgZm91bmQ8L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICA8L3VsPlxyXG4gICAgICAgIDwvZGl2PlxyXG4gICAgICAgIDxidXR0b24gdi1pZj1cInZhbHVlICE9PSBudWxsXCIgY2xhc3M9XCJidG4gYnRuLWxpbmsgYnRuLWNsZWFyXCIgQGNsaWNrPVwiY2xlYXJcIj5cclxuICAgICAgICAgICAgPHNwYW4+PC9zcGFuPlxyXG4gICAgICAgIDwvYnV0dG9uPlxyXG4gICAgPC9kaXY+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG4gICAgbW9kdWxlLmV4cG9ydHMgPSB7XHJcbiAgICAgICAgbmFtZTogJ3VzZXItc2VsZWN0b3InLFxyXG4gICAgICAgIHByb3BzOiBbJ2ZldGNoVXJsJywgJ2NvbnRyb2xJZCcsICd2YWx1ZScsICdwbGFjZWhvbGRlciddLFxyXG4gICAgICAgIGRhdGE6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgICAgICAgIG9wdGlvbnM6IFtdLFxyXG4gICAgICAgICAgICAgICAgaXNMb2FkaW5nOiBmYWxzZSxcclxuICAgICAgICAgICAgICAgIHNlYXJjaFRlcm06ICcnXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfSxcclxuICAgICAgICBjb21wdXRlZDoge1xyXG4gICAgICAgICAgICBpbnB1dElkOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gYHNiXyR7dGhpcy5jb250cm9sSWR9YDtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgcGxhY2Vob2xkZXJUZXh0OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gdGhpcy5wbGFjZWhvbGRlciB8fCBcIlNlbGVjdFwiO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuICAgICAgICBtb3VudGVkOiBmdW5jdGlvbigpIHtcclxuICAgICAgICAgICAgY29uc3QganFFbCA9ICQodGhpcy4kZWwpXHJcbiAgICAgICAgICAgIGNvbnN0IGZvY3VzVG8gPSBqcUVsLmZpbmQoYCMke3RoaXMuaW5wdXRJZH1gKVxyXG4gICAgICAgICAgICBqcUVsLm9uKCdzaG93bi5icy5kcm9wZG93bicsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIGZvY3VzVG8uZm9jdXMoKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5mZXRjaE9wdGlvbnModGhpcy5zZWFyY2hUZXJtKVxyXG4gICAgICAgICAgICB9KVxyXG5cclxuICAgICAgICAgICAganFFbC5vbignaGlkZGVuLmJzLmRyb3Bkb3duJywgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zZWFyY2hUZXJtID0gXCJcIlxyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgbWV0aG9kczoge1xyXG4gICAgICAgICAgICBvblNlYXJjaEJveERvd25LZXkoZXZlbnQpIHtcclxuICAgICAgICAgICAgICAgIHZhciAkZmlyc3RPcHRpb25BbmNob3IgPSAkKHRoaXMuJHJlZnMuZHJvcGRvd25NZW51KS5maW5kKCdhJykuZmlyc3QoKTtcclxuICAgICAgICAgICAgICAgICRmaXJzdE9wdGlvbkFuY2hvci5mb2N1cygpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBvbk9wdGlvblVwS2V5KGV2ZW50KSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgaXNGaXJzdE9wdGlvbiA9ICQoZXZlbnQudGFyZ2V0KS5wYXJlbnQoKS5pbmRleCgpID09PSAxO1xyXG5cclxuICAgICAgICAgICAgICAgIGlmIChpc0ZpcnN0T3B0aW9uKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy4kcmVmcy5zZWFyY2hCb3guZm9jdXMoKTtcclxuICAgICAgICAgICAgICAgICAgICBldmVudC5zdG9wUHJvcGFnYXRpb24oKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgZmV0Y2hPcHRpb25zOiBmdW5jdGlvbiAoZmlsdGVyID0gXCJcIikge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZS5sb2coYGZpbHRlcjoge2ZpbHRlcn1gKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGh0dHAuZ2V0KHRoaXMuZmV0Y2hVcmwsIHtwYXJhbXM6IHsgcXVlcnk6IGZpbHRlciB9fSlcclxuICAgICAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMub3B0aW9ucyA9IHJlc3BvbnNlLmJvZHkub3B0aW9ucyB8fCBbXTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9LCByZXNwb25zZSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIFxyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgY2xlYXI6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGVtaXQoJ3NlbGVjdGVkJywgbnVsbCwgdGhpcy5jb250cm9sSWQpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zZWFyY2hUZXJtID0gXCJcIjtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgc2VsZWN0T3B0aW9uOiBmdW5jdGlvbiAodmFsdWUpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuJGVtaXQoJ3NlbGVjdGVkJywgdmFsdWUsIHRoaXMuY29udHJvbElkKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgdXBkYXRlT3B0aW9uc0xpc3QoZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5mZXRjaE9wdGlvbnMoZS50YXJnZXQudmFsdWUpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBoaWdobGlnaHQ6IGZ1bmN0aW9uICh0aXRsZSwgc2VhcmNoVGVybSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGVuY29kZWRUaXRsZSA9IF8uZXNjYXBlKHRpdGxlKTtcclxuICAgICAgICAgICAgICAgIGlmIChzZWFyY2hUZXJtKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHNhZmVTZWFyY2hUZXJtID0gXy5lc2NhcGUoXy5lc2NhcGVSZWdFeHAoc2VhcmNoVGVybSkpO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICB2YXIgaVF1ZXJ5ID0gbmV3IFJlZ0V4cChzYWZlU2VhcmNoVGVybSwgXCJpZ1wiKTtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gZW5jb2RlZFRpdGxlLnJlcGxhY2UoaVF1ZXJ5LCAobWF0Y2hlZFR4dCwgYSwgYikgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gYDxzdHJvbmc+JHttYXRjaGVkVHh0fTwvc3Ryb25nPmA7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGVuY29kZWRUaXRsZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgIH07XHJcbjwvc2NyaXB0PiIsIu+7v++7v2ltcG9ydCBWdWUgZnJvbSAndnVlJ1xyXG5pbXBvcnQgVnVlUmVzb3VyY2UgZnJvbSAndnVlLXJlc291cmNlJ1xyXG5pbXBvcnQgVXNlclNlbGVjdG9yIGZyb20gJy4vVXNlclNlbGVjdG9yLnZ1ZSdcclxuaW1wb3J0IERhdGVQaWNrZXIgZnJvbSAnLi9EYXRlUGlja2VyLnZ1ZSdcclxuaW1wb3J0IEludGVydmlld1RhYmxlIGZyb20gJy4vSW50ZXJ2aWV3VGFibGUudnVlJ1xyXG5pbXBvcnQgVmVlVmFsaWRhdGUgZnJvbSAndmVlLXZhbGlkYXRlJztcclxuXHJcblZ1ZS51c2UoVmVlVmFsaWRhdGUpO1xyXG5WdWUudXNlKFZ1ZVJlc291cmNlKTtcclxuXHJcblZ1ZS5jb21wb25lbnQoJ0ZsYXRwaWNrcicsIERhdGVQaWNrZXIpO1xyXG5WdWUuY29tcG9uZW50KFwidXNlci1zZWxlY3RvclwiLCBVc2VyU2VsZWN0b3IpO1xyXG5WdWUuY29tcG9uZW50KFwiaW50ZXJ2aWV3LXRhYmxlXCIsIEludGVydmlld1RhYmxlKTtcclxuXHJcbnZhciBhcHAgPSBuZXcgVnVlKHtcclxuICAgIGRhdGE6IHtcclxuICAgICAgICBpbnRlcnZpZXdlcklkOiBudWxsLFxyXG4gICAgICAgIHF1ZXN0aW9ubmFpcmVJZDogbnVsbCxcclxuICAgICAgICBjaGFuZ2VkRnJvbTogbnVsbCxcclxuICAgICAgICBjaGFuZ2VkVG86IG51bGwsXHJcbiAgICAgICAgZGF0ZVJhbmdlUGlja2VyT3B0aW9uczoge1xyXG4gICAgICAgICAgICBtb2RlOiBcInJhbmdlXCIsXHJcbiAgICAgICAgICAgIG1heERhdGU6IFwidG9kYXlcIixcclxuICAgICAgICAgICAgbWluRGF0ZTogbmV3IERhdGUoKS5mcF9pbmNyKC0zMClcclxuICAgICAgICB9LFxyXG4gICAgICAgIHRhYmxlRmlsdGVyczoge31cclxuICAgIH0sXHJcbiAgICBjb21wdXRlZDoge1xyXG4gICAgfSxcclxuICAgIG1ldGhvZHM6IHtcclxuICAgICAgICB1c2VyU2VsZWN0ZWQobmV3VmFsdWUpIHtcclxuICAgICAgICAgICAgdGhpcy5pbnRlcnZpZXdlcklkID0gbmV3VmFsdWU7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBxdWVzdGlvbm5haXJlU2VsZWN0ZWQobmV3VmFsdWUpIHtcclxuICAgICAgICAgICAgdGhpcy5xdWVzdGlvbm5haXJlSWQgPSBuZXdWYWx1ZTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIHJhbmdlU2VsZWN0ZWQodGV4dFZhbHVlLCBmcm9tLCB0bykge1xyXG4gICAgICAgICAgICB0aGlzLmNoYW5nZWRGcm9tID0gZnJvbTtcclxuICAgICAgICAgICAgdGhpcy5jaGFuZ2VkVG8gPSB0bztcclxuICAgICAgICB9LFxyXG4gICAgICAgIHZhbGlkYXRlRm9ybSgpIHtcclxuICAgICAgICAgICAgdGhpcy4kdmFsaWRhdG9yLnZhbGlkYXRlQWxsKCkudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgaWYgKHJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuZmluZEludGVydmlld3MoKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBmaW5kSW50ZXJ2aWV3cygpIHtcclxuICAgICAgICAgICAgdGhpcy50YWJsZUZpbHRlcnMgPSB7XHJcbiAgICAgICAgICAgICAgICBpbnRlcnZpZXdlcklkOiB0aGlzLmludGVydmlld2VySWQsXHJcbiAgICAgICAgICAgICAgICBxdWVzdGlvbm5haXJlSWQ6IHRoaXMucXVlc3Rpb25uYWlyZUlkLFxyXG4gICAgICAgICAgICAgICAgY2hhbmdlZEZyb206IHRoaXMuY2hhbmdlZEZyb20sXHJcbiAgICAgICAgICAgICAgICBjaGFuZ2VkVG86IHRoaXMuY2hhbmdlZFRvXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCJtYWluXCIpLmNsYXNzTGlzdC5yZW1vdmUoXCJzZWFyY2gtd2FzbnQtc3RhcnRlZFwiKTtcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gICAgbW91bnRlZDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIm1haW5cIikuY2xhc3NMaXN0LnJlbW92ZShcImhvbGQtdHJhbnNpdGlvblwiKTtcclxuICAgIH1cclxufSk7XHJcblxyXG53aW5kb3cub25sb2FkID0gZnVuY3Rpb24gKCkge1xyXG4gICAgVnVlLmh0dHAuaGVhZGVycy5jb21tb25bJ0F1dGhvcml6YXRpb24nXSA9IGlucHV0LnNldHRpbmdzLmFjc3JmLnRva2VuO1xyXG5cclxuICAgIGFwcC4kbW91bnQoJyNhcHAnKTtcclxufSJdfQ==
