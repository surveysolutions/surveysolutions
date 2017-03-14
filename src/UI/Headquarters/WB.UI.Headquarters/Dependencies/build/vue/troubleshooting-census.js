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
        this.fp = new _flatpickr2.default(this.$el, (0, _assign2.default)(this.options, {
            onValueUpdate: function onValueUpdate() {
                self.onInput(self.$el.value, self.fp.selectedDates);
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
        onInput: function onInput(e, selectedDates) {
            typeof e === 'string' ? this.$emit('input', e) : this.$emit('input', e.target.value);
        }
    }
};
})()
if (module.exports.__esModule) module.exports = module.exports.default
var __vue__options__ = (typeof module.exports === "function"? module.exports.options: module.exports)
if (__vue__options__.functional) {console.error("[vueify] functional components are not supported and should be defined in plain js files using render functions.")}
__vue__options__.render = function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('input',{class:_vm.inputClass,attrs:{"type":"text","placeholder":_vm.placeholder},domProps:{"value":_vm.value},on:{"input":_vm.onInput}})}
__vue__options__.staticRenderFns = []
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-224cef12", __vue__options__)
  } else {
    hotAPI.reload("data-v-224cef12", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/json/stringify":1,"babel-runtime/core-js/object/assign":2,"flatpickr":40,"vue-hot-reload-api":41}],43:[function(require,module,exports){
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
    hotAPI.createRecord("data-v-4ea71100", __vue__options__)
  } else {
    hotAPI.reload("data-v-4ea71100", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"vue-hot-reload-api":41}],44:[function(require,module,exports){
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

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

_vue2.default.component('Flatpickr', _DatePicker2.default);
_vue2.default.use(_vueResource2.default);

_vue2.default.component("user-selector", _UserSelector2.default);

var app = new _vue2.default({
    data: {
        interviewerId: null,
        questionnaireId: null,
        dateStr: null,
        dateRangePickerOptions: {
            mode: "range",
            maxDate: "today"
        }
    },
    methods: {
        userSelected: function userSelected(newValue, id) {
            this.interviewerId = newValue;
        },
        questionnaireSelected: function questionnaireSelected(newValue, id) {
            this.questionnaireId = newValue;
        },
        rangeSelected: function rangeSelected(newValue, id) {
            console.log(newValue);
            console.log(id);
        }
    }
});

window.onload = function () {
    _vue2.default.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
};

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"./DatePicker.vue":42,"./UserSelector.vue":43}]},{},[44])
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvYmFiZWwtcnVudGltZS9jb3JlLWpzL2pzb24vc3RyaW5naWZ5LmpzIiwibm9kZV9tb2R1bGVzL2JhYmVsLXJ1bnRpbWUvY29yZS1qcy9vYmplY3QvYXNzaWduLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYS1mdW5jdGlvbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYW4tb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19hcnJheS1pbmNsdWRlcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY29mLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jb3JlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jdHguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2RlZmluZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2Rlc2NyaXB0b3JzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kb20tY3JlYXRlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19lbnVtLWJ1Zy1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19leHBvcnQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2ZhaWxzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19nbG9iYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2hhcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faGlkZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faWU4LWRvbS1kZWZpbmUuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lvYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lzLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWFzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWRwLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtZ29wcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWtleXMtaW50ZXJuYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtcGllLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19wcm9wZXJ0eS1kZXNjLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQta2V5LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLWluZGV4LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pbnRlZ2VyLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1sZW5ndGguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8tcHJpbWl0aXZlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL191aWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvZmxhdHBpY2tyL2Rpc3QvZmxhdHBpY2tyLmpzIiwibm9kZV9tb2R1bGVzL3Z1ZS1ob3QtcmVsb2FkLWFwaS9pbmRleC5qcyIsInZ1ZVxcdnVlXFxEYXRlUGlja2VyLnZ1ZT8yNWY4M2MwOSIsInZ1ZVxcdnVlXFxVc2VyU2VsZWN0b3IudnVlPzRmZjA1NGNlIiwidnVlXFx2dWVcXHRyb3VibGVzaG9vdGluZy1jZW5zdXMuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUNBQTs7QUNBQTs7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7O0FDREE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDcEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTs7QUNEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ25CQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzVEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1BBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaENBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2ZBOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDUEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNMQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDWEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzM5REE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDbklBOzs7Ozs7O0FBR0E7QUFDQTtBQUNBO0FBREE7QUFHQTtBQUNBO0FBQ0E7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUFBO0FBQUE7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUZBO0FBWkE7QUFpQkE7QUFDQTtBQUNBO0FBREE7QUFHQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUhBO0FBS0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBTkE7QUFRQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFIQTtBQXJEQTs7Ozs7QUFQQTtBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDNkJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFIQTtBQUtBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQUE7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUFBOztBQUFBOztBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFoREE7QUE5QkE7Ozs7O0FBN0JBO0FBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ0FDOzs7O0FBQ0Q7Ozs7QUFDQTs7OztBQUNBOzs7Ozs7QUFFQSxjQUFJLFNBQUosQ0FBYyxXQUFkO0FBQ0EsY0FBSSxHQUFKOztBQUdBLGNBQUksU0FBSixDQUFjLGVBQWQ7O0FBRUEsSUFBSSxNQUFNLGtCQUFRO0FBQ2QsVUFBTTtBQUNGLHVCQUFlLElBRGI7QUFFRix5QkFBaUIsSUFGZjtBQUdGLGlCQUFTLElBSFA7QUFJRixnQ0FBd0I7QUFDcEIsa0JBQU0sT0FEYztBQUVwQixxQkFBUztBQUZXO0FBSnRCLEtBRFE7QUFVZCxhQUFTO0FBQ0wsb0JBREssd0JBQ1EsUUFEUixFQUNrQixFQURsQixFQUNzQjtBQUN2QixpQkFBSyxhQUFMLEdBQXFCLFFBQXJCO0FBQ0gsU0FISTtBQUlMLDZCQUpLLGlDQUlpQixRQUpqQixFQUkyQixFQUozQixFQUkrQjtBQUNoQyxpQkFBSyxlQUFMLEdBQXVCLFFBQXZCO0FBQ0gsU0FOSTtBQU9MLHFCQVBLLHlCQU9TLFFBUFQsRUFPbUIsRUFQbkIsRUFPdUI7QUFDeEIsb0JBQVEsR0FBUixDQUFZLFFBQVo7QUFDQSxvQkFBUSxHQUFSLENBQVksRUFBWjtBQUNIO0FBVkk7QUFWSyxDQUFSLENBQVY7O0FBd0JBLE9BQU8sTUFBUCxHQUFnQixZQUFZO0FBQ3hCLGtCQUFJLElBQUosQ0FBUyxPQUFULENBQWlCLE1BQWpCLENBQXdCLGVBQXhCLElBQTJDLE1BQU0sUUFBTixDQUFlLEtBQWYsQ0FBcUIsS0FBaEU7O0FBRUEsUUFBSSxNQUFKLENBQVcsTUFBWDtBQUNILENBSkQiLCJmaWxlIjoiZ2VuZXJhdGVkLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXNDb250ZW50IjpbIihmdW5jdGlvbiBlKHQsbixyKXtmdW5jdGlvbiBzKG8sdSl7aWYoIW5bb10pe2lmKCF0W29dKXt2YXIgYT10eXBlb2YgcmVxdWlyZT09XCJmdW5jdGlvblwiJiZyZXF1aXJlO2lmKCF1JiZhKXJldHVybiBhKG8sITApO2lmKGkpcmV0dXJuIGkobywhMCk7dmFyIGY9bmV3IEVycm9yKFwiQ2Fubm90IGZpbmQgbW9kdWxlICdcIitvK1wiJ1wiKTt0aHJvdyBmLmNvZGU9XCJNT0RVTEVfTk9UX0ZPVU5EXCIsZn12YXIgbD1uW29dPXtleHBvcnRzOnt9fTt0W29dWzBdLmNhbGwobC5leHBvcnRzLGZ1bmN0aW9uKGUpe3ZhciBuPXRbb11bMV1bZV07cmV0dXJuIHMobj9uOmUpfSxsLGwuZXhwb3J0cyxlLHQsbixyKX1yZXR1cm4gbltvXS5leHBvcnRzfXZhciBpPXR5cGVvZiByZXF1aXJlPT1cImZ1bmN0aW9uXCImJnJlcXVpcmU7Zm9yKHZhciBvPTA7bzxyLmxlbmd0aDtvKyspcyhyW29dKTtyZXR1cm4gc30pIiwibW9kdWxlLmV4cG9ydHMgPSB7IFwiZGVmYXVsdFwiOiByZXF1aXJlKFwiY29yZS1qcy9saWJyYXJ5L2ZuL2pzb24vc3RyaW5naWZ5XCIpLCBfX2VzTW9kdWxlOiB0cnVlIH07IiwibW9kdWxlLmV4cG9ydHMgPSB7IFwiZGVmYXVsdFwiOiByZXF1aXJlKFwiY29yZS1qcy9saWJyYXJ5L2ZuL29iamVjdC9hc3NpZ25cIiksIF9fZXNNb2R1bGU6IHRydWUgfTsiLCJ2YXIgY29yZSAgPSByZXF1aXJlKCcuLi8uLi9tb2R1bGVzL19jb3JlJylcbiAgLCAkSlNPTiA9IGNvcmUuSlNPTiB8fCAoY29yZS5KU09OID0ge3N0cmluZ2lmeTogSlNPTi5zdHJpbmdpZnl9KTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24gc3RyaW5naWZ5KGl0KXsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bnVzZWQtdmFyc1xuICByZXR1cm4gJEpTT04uc3RyaW5naWZ5LmFwcGx5KCRKU09OLCBhcmd1bWVudHMpO1xufTsiLCJyZXF1aXJlKCcuLi8uLi9tb2R1bGVzL2VzNi5vYmplY3QuYXNzaWduJyk7XG5tb2R1bGUuZXhwb3J0cyA9IHJlcXVpcmUoJy4uLy4uL21vZHVsZXMvX2NvcmUnKS5PYmplY3QuYXNzaWduOyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICBpZih0eXBlb2YgaXQgIT0gJ2Z1bmN0aW9uJyl0aHJvdyBUeXBlRXJyb3IoaXQgKyAnIGlzIG5vdCBhIGZ1bmN0aW9uIScpO1xuICByZXR1cm4gaXQ7XG59OyIsInZhciBpc09iamVjdCA9IHJlcXVpcmUoJy4vX2lzLW9iamVjdCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIGlmKCFpc09iamVjdChpdCkpdGhyb3cgVHlwZUVycm9yKGl0ICsgJyBpcyBub3QgYW4gb2JqZWN0IScpO1xuICByZXR1cm4gaXQ7XG59OyIsIi8vIGZhbHNlIC0+IEFycmF5I2luZGV4T2Zcbi8vIHRydWUgIC0+IEFycmF5I2luY2x1ZGVzXG52YXIgdG9JT2JqZWN0ID0gcmVxdWlyZSgnLi9fdG8taW9iamVjdCcpXG4gICwgdG9MZW5ndGggID0gcmVxdWlyZSgnLi9fdG8tbGVuZ3RoJylcbiAgLCB0b0luZGV4ICAgPSByZXF1aXJlKCcuL190by1pbmRleCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihJU19JTkNMVURFUyl7XG4gIHJldHVybiBmdW5jdGlvbigkdGhpcywgZWwsIGZyb21JbmRleCl7XG4gICAgdmFyIE8gICAgICA9IHRvSU9iamVjdCgkdGhpcylcbiAgICAgICwgbGVuZ3RoID0gdG9MZW5ndGgoTy5sZW5ndGgpXG4gICAgICAsIGluZGV4ICA9IHRvSW5kZXgoZnJvbUluZGV4LCBsZW5ndGgpXG4gICAgICAsIHZhbHVlO1xuICAgIC8vIEFycmF5I2luY2x1ZGVzIHVzZXMgU2FtZVZhbHVlWmVybyBlcXVhbGl0eSBhbGdvcml0aG1cbiAgICBpZihJU19JTkNMVURFUyAmJiBlbCAhPSBlbCl3aGlsZShsZW5ndGggPiBpbmRleCl7XG4gICAgICB2YWx1ZSA9IE9baW5kZXgrK107XG4gICAgICBpZih2YWx1ZSAhPSB2YWx1ZSlyZXR1cm4gdHJ1ZTtcbiAgICAvLyBBcnJheSN0b0luZGV4IGlnbm9yZXMgaG9sZXMsIEFycmF5I2luY2x1ZGVzIC0gbm90XG4gICAgfSBlbHNlIGZvcig7bGVuZ3RoID4gaW5kZXg7IGluZGV4KyspaWYoSVNfSU5DTFVERVMgfHwgaW5kZXggaW4gTyl7XG4gICAgICBpZihPW2luZGV4XSA9PT0gZWwpcmV0dXJuIElTX0lOQ0xVREVTIHx8IGluZGV4IHx8IDA7XG4gICAgfSByZXR1cm4gIUlTX0lOQ0xVREVTICYmIC0xO1xuICB9O1xufTsiLCJ2YXIgdG9TdHJpbmcgPSB7fS50b1N0cmluZztcblxubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiB0b1N0cmluZy5jYWxsKGl0KS5zbGljZSg4LCAtMSk7XG59OyIsInZhciBjb3JlID0gbW9kdWxlLmV4cG9ydHMgPSB7dmVyc2lvbjogJzIuNC4wJ307XG5pZih0eXBlb2YgX19lID09ICdudW1iZXInKV9fZSA9IGNvcmU7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW5kZWYiLCIvLyBvcHRpb25hbCAvIHNpbXBsZSBjb250ZXh0IGJpbmRpbmdcbnZhciBhRnVuY3Rpb24gPSByZXF1aXJlKCcuL19hLWZ1bmN0aW9uJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGZuLCB0aGF0LCBsZW5ndGgpe1xuICBhRnVuY3Rpb24oZm4pO1xuICBpZih0aGF0ID09PSB1bmRlZmluZWQpcmV0dXJuIGZuO1xuICBzd2l0Y2gobGVuZ3RoKXtcbiAgICBjYXNlIDE6IHJldHVybiBmdW5jdGlvbihhKXtcbiAgICAgIHJldHVybiBmbi5jYWxsKHRoYXQsIGEpO1xuICAgIH07XG4gICAgY2FzZSAyOiByZXR1cm4gZnVuY3Rpb24oYSwgYil7XG4gICAgICByZXR1cm4gZm4uY2FsbCh0aGF0LCBhLCBiKTtcbiAgICB9O1xuICAgIGNhc2UgMzogcmV0dXJuIGZ1bmN0aW9uKGEsIGIsIGMpe1xuICAgICAgcmV0dXJuIGZuLmNhbGwodGhhdCwgYSwgYiwgYyk7XG4gICAgfTtcbiAgfVxuICByZXR1cm4gZnVuY3Rpb24oLyogLi4uYXJncyAqLyl7XG4gICAgcmV0dXJuIGZuLmFwcGx5KHRoYXQsIGFyZ3VtZW50cyk7XG4gIH07XG59OyIsIi8vIDcuMi4xIFJlcXVpcmVPYmplY3RDb2VyY2libGUoYXJndW1lbnQpXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgaWYoaXQgPT0gdW5kZWZpbmVkKXRocm93IFR5cGVFcnJvcihcIkNhbid0IGNhbGwgbWV0aG9kIG9uICBcIiArIGl0KTtcbiAgcmV0dXJuIGl0O1xufTsiLCIvLyBUaGFuaydzIElFOCBmb3IgaGlzIGZ1bm55IGRlZmluZVByb3BlcnR5XG5tb2R1bGUuZXhwb3J0cyA9ICFyZXF1aXJlKCcuL19mYWlscycpKGZ1bmN0aW9uKCl7XG4gIHJldHVybiBPYmplY3QuZGVmaW5lUHJvcGVydHkoe30sICdhJywge2dldDogZnVuY3Rpb24oKXsgcmV0dXJuIDc7IH19KS5hICE9IDc7XG59KTsiLCJ2YXIgaXNPYmplY3QgPSByZXF1aXJlKCcuL19pcy1vYmplY3QnKVxuICAsIGRvY3VtZW50ID0gcmVxdWlyZSgnLi9fZ2xvYmFsJykuZG9jdW1lbnRcbiAgLy8gaW4gb2xkIElFIHR5cGVvZiBkb2N1bWVudC5jcmVhdGVFbGVtZW50IGlzICdvYmplY3QnXG4gICwgaXMgPSBpc09iamVjdChkb2N1bWVudCkgJiYgaXNPYmplY3QoZG9jdW1lbnQuY3JlYXRlRWxlbWVudCk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGlzID8gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChpdCkgOiB7fTtcbn07IiwiLy8gSUUgOC0gZG9uJ3QgZW51bSBidWcga2V5c1xubW9kdWxlLmV4cG9ydHMgPSAoXG4gICdjb25zdHJ1Y3RvcixoYXNPd25Qcm9wZXJ0eSxpc1Byb3RvdHlwZU9mLHByb3BlcnR5SXNFbnVtZXJhYmxlLHRvTG9jYWxlU3RyaW5nLHRvU3RyaW5nLHZhbHVlT2YnXG4pLnNwbGl0KCcsJyk7IiwidmFyIGdsb2JhbCAgICA9IHJlcXVpcmUoJy4vX2dsb2JhbCcpXG4gICwgY29yZSAgICAgID0gcmVxdWlyZSgnLi9fY29yZScpXG4gICwgY3R4ICAgICAgID0gcmVxdWlyZSgnLi9fY3R4JylcbiAgLCBoaWRlICAgICAgPSByZXF1aXJlKCcuL19oaWRlJylcbiAgLCBQUk9UT1RZUEUgPSAncHJvdG90eXBlJztcblxudmFyICRleHBvcnQgPSBmdW5jdGlvbih0eXBlLCBuYW1lLCBzb3VyY2Upe1xuICB2YXIgSVNfRk9SQ0VEID0gdHlwZSAmICRleHBvcnQuRlxuICAgICwgSVNfR0xPQkFMID0gdHlwZSAmICRleHBvcnQuR1xuICAgICwgSVNfU1RBVElDID0gdHlwZSAmICRleHBvcnQuU1xuICAgICwgSVNfUFJPVE8gID0gdHlwZSAmICRleHBvcnQuUFxuICAgICwgSVNfQklORCAgID0gdHlwZSAmICRleHBvcnQuQlxuICAgICwgSVNfV1JBUCAgID0gdHlwZSAmICRleHBvcnQuV1xuICAgICwgZXhwb3J0cyAgID0gSVNfR0xPQkFMID8gY29yZSA6IGNvcmVbbmFtZV0gfHwgKGNvcmVbbmFtZV0gPSB7fSlcbiAgICAsIGV4cFByb3RvICA9IGV4cG9ydHNbUFJPVE9UWVBFXVxuICAgICwgdGFyZ2V0ICAgID0gSVNfR0xPQkFMID8gZ2xvYmFsIDogSVNfU1RBVElDID8gZ2xvYmFsW25hbWVdIDogKGdsb2JhbFtuYW1lXSB8fCB7fSlbUFJPVE9UWVBFXVxuICAgICwga2V5LCBvd24sIG91dDtcbiAgaWYoSVNfR0xPQkFMKXNvdXJjZSA9IG5hbWU7XG4gIGZvcihrZXkgaW4gc291cmNlKXtcbiAgICAvLyBjb250YWlucyBpbiBuYXRpdmVcbiAgICBvd24gPSAhSVNfRk9SQ0VEICYmIHRhcmdldCAmJiB0YXJnZXRba2V5XSAhPT0gdW5kZWZpbmVkO1xuICAgIGlmKG93biAmJiBrZXkgaW4gZXhwb3J0cyljb250aW51ZTtcbiAgICAvLyBleHBvcnQgbmF0aXZlIG9yIHBhc3NlZFxuICAgIG91dCA9IG93biA/IHRhcmdldFtrZXldIDogc291cmNlW2tleV07XG4gICAgLy8gcHJldmVudCBnbG9iYWwgcG9sbHV0aW9uIGZvciBuYW1lc3BhY2VzXG4gICAgZXhwb3J0c1trZXldID0gSVNfR0xPQkFMICYmIHR5cGVvZiB0YXJnZXRba2V5XSAhPSAnZnVuY3Rpb24nID8gc291cmNlW2tleV1cbiAgICAvLyBiaW5kIHRpbWVycyB0byBnbG9iYWwgZm9yIGNhbGwgZnJvbSBleHBvcnQgY29udGV4dFxuICAgIDogSVNfQklORCAmJiBvd24gPyBjdHgob3V0LCBnbG9iYWwpXG4gICAgLy8gd3JhcCBnbG9iYWwgY29uc3RydWN0b3JzIGZvciBwcmV2ZW50IGNoYW5nZSB0aGVtIGluIGxpYnJhcnlcbiAgICA6IElTX1dSQVAgJiYgdGFyZ2V0W2tleV0gPT0gb3V0ID8gKGZ1bmN0aW9uKEMpe1xuICAgICAgdmFyIEYgPSBmdW5jdGlvbihhLCBiLCBjKXtcbiAgICAgICAgaWYodGhpcyBpbnN0YW5jZW9mIEMpe1xuICAgICAgICAgIHN3aXRjaChhcmd1bWVudHMubGVuZ3RoKXtcbiAgICAgICAgICAgIGNhc2UgMDogcmV0dXJuIG5ldyBDO1xuICAgICAgICAgICAgY2FzZSAxOiByZXR1cm4gbmV3IEMoYSk7XG4gICAgICAgICAgICBjYXNlIDI6IHJldHVybiBuZXcgQyhhLCBiKTtcbiAgICAgICAgICB9IHJldHVybiBuZXcgQyhhLCBiLCBjKTtcbiAgICAgICAgfSByZXR1cm4gQy5hcHBseSh0aGlzLCBhcmd1bWVudHMpO1xuICAgICAgfTtcbiAgICAgIEZbUFJPVE9UWVBFXSA9IENbUFJPVE9UWVBFXTtcbiAgICAgIHJldHVybiBGO1xuICAgIC8vIG1ha2Ugc3RhdGljIHZlcnNpb25zIGZvciBwcm90b3R5cGUgbWV0aG9kc1xuICAgIH0pKG91dCkgOiBJU19QUk9UTyAmJiB0eXBlb2Ygb3V0ID09ICdmdW5jdGlvbicgPyBjdHgoRnVuY3Rpb24uY2FsbCwgb3V0KSA6IG91dDtcbiAgICAvLyBleHBvcnQgcHJvdG8gbWV0aG9kcyB0byBjb3JlLiVDT05TVFJVQ1RPUiUubWV0aG9kcy4lTkFNRSVcbiAgICBpZihJU19QUk9UTyl7XG4gICAgICAoZXhwb3J0cy52aXJ0dWFsIHx8IChleHBvcnRzLnZpcnR1YWwgPSB7fSkpW2tleV0gPSBvdXQ7XG4gICAgICAvLyBleHBvcnQgcHJvdG8gbWV0aG9kcyB0byBjb3JlLiVDT05TVFJVQ1RPUiUucHJvdG90eXBlLiVOQU1FJVxuICAgICAgaWYodHlwZSAmICRleHBvcnQuUiAmJiBleHBQcm90byAmJiAhZXhwUHJvdG9ba2V5XSloaWRlKGV4cFByb3RvLCBrZXksIG91dCk7XG4gICAgfVxuICB9XG59O1xuLy8gdHlwZSBiaXRtYXBcbiRleHBvcnQuRiA9IDE7ICAgLy8gZm9yY2VkXG4kZXhwb3J0LkcgPSAyOyAgIC8vIGdsb2JhbFxuJGV4cG9ydC5TID0gNDsgICAvLyBzdGF0aWNcbiRleHBvcnQuUCA9IDg7ICAgLy8gcHJvdG9cbiRleHBvcnQuQiA9IDE2OyAgLy8gYmluZFxuJGV4cG9ydC5XID0gMzI7ICAvLyB3cmFwXG4kZXhwb3J0LlUgPSA2NDsgIC8vIHNhZmVcbiRleHBvcnQuUiA9IDEyODsgLy8gcmVhbCBwcm90byBtZXRob2QgZm9yIGBsaWJyYXJ5YCBcbm1vZHVsZS5leHBvcnRzID0gJGV4cG9ydDsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGV4ZWMpe1xuICB0cnkge1xuICAgIHJldHVybiAhIWV4ZWMoKTtcbiAgfSBjYXRjaChlKXtcbiAgICByZXR1cm4gdHJ1ZTtcbiAgfVxufTsiLCIvLyBodHRwczovL2dpdGh1Yi5jb20vemxvaXJvY2svY29yZS1qcy9pc3N1ZXMvODYjaXNzdWVjb21tZW50LTExNTc1OTAyOFxudmFyIGdsb2JhbCA9IG1vZHVsZS5leHBvcnRzID0gdHlwZW9mIHdpbmRvdyAhPSAndW5kZWZpbmVkJyAmJiB3aW5kb3cuTWF0aCA9PSBNYXRoXG4gID8gd2luZG93IDogdHlwZW9mIHNlbGYgIT0gJ3VuZGVmaW5lZCcgJiYgc2VsZi5NYXRoID09IE1hdGggPyBzZWxmIDogRnVuY3Rpb24oJ3JldHVybiB0aGlzJykoKTtcbmlmKHR5cGVvZiBfX2cgPT0gJ251bWJlcicpX19nID0gZ2xvYmFsOyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVuZGVmIiwidmFyIGhhc093blByb3BlcnR5ID0ge30uaGFzT3duUHJvcGVydHk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0LCBrZXkpe1xuICByZXR1cm4gaGFzT3duUHJvcGVydHkuY2FsbChpdCwga2V5KTtcbn07IiwidmFyIGRQICAgICAgICAgPSByZXF1aXJlKCcuL19vYmplY3QtZHAnKVxuICAsIGNyZWF0ZURlc2MgPSByZXF1aXJlKCcuL19wcm9wZXJ0eS1kZXNjJyk7XG5tb2R1bGUuZXhwb3J0cyA9IHJlcXVpcmUoJy4vX2Rlc2NyaXB0b3JzJykgPyBmdW5jdGlvbihvYmplY3QsIGtleSwgdmFsdWUpe1xuICByZXR1cm4gZFAuZihvYmplY3QsIGtleSwgY3JlYXRlRGVzYygxLCB2YWx1ZSkpO1xufSA6IGZ1bmN0aW9uKG9iamVjdCwga2V5LCB2YWx1ZSl7XG4gIG9iamVjdFtrZXldID0gdmFsdWU7XG4gIHJldHVybiBvYmplY3Q7XG59OyIsIm1vZHVsZS5leHBvcnRzID0gIXJlcXVpcmUoJy4vX2Rlc2NyaXB0b3JzJykgJiYgIXJlcXVpcmUoJy4vX2ZhaWxzJykoZnVuY3Rpb24oKXtcbiAgcmV0dXJuIE9iamVjdC5kZWZpbmVQcm9wZXJ0eShyZXF1aXJlKCcuL19kb20tY3JlYXRlJykoJ2RpdicpLCAnYScsIHtnZXQ6IGZ1bmN0aW9uKCl7IHJldHVybiA3OyB9fSkuYSAhPSA3O1xufSk7IiwiLy8gZmFsbGJhY2sgZm9yIG5vbi1hcnJheS1saWtlIEVTMyBhbmQgbm9uLWVudW1lcmFibGUgb2xkIFY4IHN0cmluZ3NcbnZhciBjb2YgPSByZXF1aXJlKCcuL19jb2YnKTtcbm1vZHVsZS5leHBvcnRzID0gT2JqZWN0KCd6JykucHJvcGVydHlJc0VudW1lcmFibGUoMCkgPyBPYmplY3QgOiBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBjb2YoaXQpID09ICdTdHJpbmcnID8gaXQuc3BsaXQoJycpIDogT2JqZWN0KGl0KTtcbn07IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiB0eXBlb2YgaXQgPT09ICdvYmplY3QnID8gaXQgIT09IG51bGwgOiB0eXBlb2YgaXQgPT09ICdmdW5jdGlvbic7XG59OyIsIid1c2Ugc3RyaWN0Jztcbi8vIDE5LjEuMi4xIE9iamVjdC5hc3NpZ24odGFyZ2V0LCBzb3VyY2UsIC4uLilcbnZhciBnZXRLZXlzICA9IHJlcXVpcmUoJy4vX29iamVjdC1rZXlzJylcbiAgLCBnT1BTICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1nb3BzJylcbiAgLCBwSUUgICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1waWUnKVxuICAsIHRvT2JqZWN0ID0gcmVxdWlyZSgnLi9fdG8tb2JqZWN0JylcbiAgLCBJT2JqZWN0ICA9IHJlcXVpcmUoJy4vX2lvYmplY3QnKVxuICAsICRhc3NpZ24gID0gT2JqZWN0LmFzc2lnbjtcblxuLy8gc2hvdWxkIHdvcmsgd2l0aCBzeW1ib2xzIGFuZCBzaG91bGQgaGF2ZSBkZXRlcm1pbmlzdGljIHByb3BlcnR5IG9yZGVyIChWOCBidWcpXG5tb2R1bGUuZXhwb3J0cyA9ICEkYXNzaWduIHx8IHJlcXVpcmUoJy4vX2ZhaWxzJykoZnVuY3Rpb24oKXtcbiAgdmFyIEEgPSB7fVxuICAgICwgQiA9IHt9XG4gICAgLCBTID0gU3ltYm9sKClcbiAgICAsIEsgPSAnYWJjZGVmZ2hpamtsbW5vcHFyc3QnO1xuICBBW1NdID0gNztcbiAgSy5zcGxpdCgnJykuZm9yRWFjaChmdW5jdGlvbihrKXsgQltrXSA9IGs7IH0pO1xuICByZXR1cm4gJGFzc2lnbih7fSwgQSlbU10gIT0gNyB8fCBPYmplY3Qua2V5cygkYXNzaWduKHt9LCBCKSkuam9pbignJykgIT0gSztcbn0pID8gZnVuY3Rpb24gYXNzaWduKHRhcmdldCwgc291cmNlKXsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bnVzZWQtdmFyc1xuICB2YXIgVCAgICAgPSB0b09iamVjdCh0YXJnZXQpXG4gICAgLCBhTGVuICA9IGFyZ3VtZW50cy5sZW5ndGhcbiAgICAsIGluZGV4ID0gMVxuICAgICwgZ2V0U3ltYm9scyA9IGdPUFMuZlxuICAgICwgaXNFbnVtICAgICA9IHBJRS5mO1xuICB3aGlsZShhTGVuID4gaW5kZXgpe1xuICAgIHZhciBTICAgICAgPSBJT2JqZWN0KGFyZ3VtZW50c1tpbmRleCsrXSlcbiAgICAgICwga2V5cyAgID0gZ2V0U3ltYm9scyA/IGdldEtleXMoUykuY29uY2F0KGdldFN5bWJvbHMoUykpIDogZ2V0S2V5cyhTKVxuICAgICAgLCBsZW5ndGggPSBrZXlzLmxlbmd0aFxuICAgICAgLCBqICAgICAgPSAwXG4gICAgICAsIGtleTtcbiAgICB3aGlsZShsZW5ndGggPiBqKWlmKGlzRW51bS5jYWxsKFMsIGtleSA9IGtleXNbaisrXSkpVFtrZXldID0gU1trZXldO1xuICB9IHJldHVybiBUO1xufSA6ICRhc3NpZ247IiwidmFyIGFuT2JqZWN0ICAgICAgID0gcmVxdWlyZSgnLi9fYW4tb2JqZWN0JylcbiAgLCBJRThfRE9NX0RFRklORSA9IHJlcXVpcmUoJy4vX2llOC1kb20tZGVmaW5lJylcbiAgLCB0b1ByaW1pdGl2ZSAgICA9IHJlcXVpcmUoJy4vX3RvLXByaW1pdGl2ZScpXG4gICwgZFAgICAgICAgICAgICAgPSBPYmplY3QuZGVmaW5lUHJvcGVydHk7XG5cbmV4cG9ydHMuZiA9IHJlcXVpcmUoJy4vX2Rlc2NyaXB0b3JzJykgPyBPYmplY3QuZGVmaW5lUHJvcGVydHkgOiBmdW5jdGlvbiBkZWZpbmVQcm9wZXJ0eShPLCBQLCBBdHRyaWJ1dGVzKXtcbiAgYW5PYmplY3QoTyk7XG4gIFAgPSB0b1ByaW1pdGl2ZShQLCB0cnVlKTtcbiAgYW5PYmplY3QoQXR0cmlidXRlcyk7XG4gIGlmKElFOF9ET01fREVGSU5FKXRyeSB7XG4gICAgcmV0dXJuIGRQKE8sIFAsIEF0dHJpYnV0ZXMpO1xuICB9IGNhdGNoKGUpeyAvKiBlbXB0eSAqLyB9XG4gIGlmKCdnZXQnIGluIEF0dHJpYnV0ZXMgfHwgJ3NldCcgaW4gQXR0cmlidXRlcyl0aHJvdyBUeXBlRXJyb3IoJ0FjY2Vzc29ycyBub3Qgc3VwcG9ydGVkIScpO1xuICBpZigndmFsdWUnIGluIEF0dHJpYnV0ZXMpT1tQXSA9IEF0dHJpYnV0ZXMudmFsdWU7XG4gIHJldHVybiBPO1xufTsiLCJleHBvcnRzLmYgPSBPYmplY3QuZ2V0T3duUHJvcGVydHlTeW1ib2xzOyIsInZhciBoYXMgICAgICAgICAgPSByZXF1aXJlKCcuL19oYXMnKVxuICAsIHRvSU9iamVjdCAgICA9IHJlcXVpcmUoJy4vX3RvLWlvYmplY3QnKVxuICAsIGFycmF5SW5kZXhPZiA9IHJlcXVpcmUoJy4vX2FycmF5LWluY2x1ZGVzJykoZmFsc2UpXG4gICwgSUVfUFJPVE8gICAgID0gcmVxdWlyZSgnLi9fc2hhcmVkLWtleScpKCdJRV9QUk9UTycpO1xuXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKG9iamVjdCwgbmFtZXMpe1xuICB2YXIgTyAgICAgID0gdG9JT2JqZWN0KG9iamVjdClcbiAgICAsIGkgICAgICA9IDBcbiAgICAsIHJlc3VsdCA9IFtdXG4gICAgLCBrZXk7XG4gIGZvcihrZXkgaW4gTylpZihrZXkgIT0gSUVfUFJPVE8paGFzKE8sIGtleSkgJiYgcmVzdWx0LnB1c2goa2V5KTtcbiAgLy8gRG9uJ3QgZW51bSBidWcgJiBoaWRkZW4ga2V5c1xuICB3aGlsZShuYW1lcy5sZW5ndGggPiBpKWlmKGhhcyhPLCBrZXkgPSBuYW1lc1tpKytdKSl7XG4gICAgfmFycmF5SW5kZXhPZihyZXN1bHQsIGtleSkgfHwgcmVzdWx0LnB1c2goa2V5KTtcbiAgfVxuICByZXR1cm4gcmVzdWx0O1xufTsiLCIvLyAxOS4xLjIuMTQgLyAxNS4yLjMuMTQgT2JqZWN0LmtleXMoTylcbnZhciAka2V5cyAgICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1rZXlzLWludGVybmFsJylcbiAgLCBlbnVtQnVnS2V5cyA9IHJlcXVpcmUoJy4vX2VudW0tYnVnLWtleXMnKTtcblxubW9kdWxlLmV4cG9ydHMgPSBPYmplY3Qua2V5cyB8fCBmdW5jdGlvbiBrZXlzKE8pe1xuICByZXR1cm4gJGtleXMoTywgZW51bUJ1Z0tleXMpO1xufTsiLCJleHBvcnRzLmYgPSB7fS5wcm9wZXJ0eUlzRW51bWVyYWJsZTsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGJpdG1hcCwgdmFsdWUpe1xuICByZXR1cm4ge1xuICAgIGVudW1lcmFibGUgIDogIShiaXRtYXAgJiAxKSxcbiAgICBjb25maWd1cmFibGU6ICEoYml0bWFwICYgMiksXG4gICAgd3JpdGFibGUgICAgOiAhKGJpdG1hcCAmIDQpLFxuICAgIHZhbHVlICAgICAgIDogdmFsdWVcbiAgfTtcbn07IiwidmFyIHNoYXJlZCA9IHJlcXVpcmUoJy4vX3NoYXJlZCcpKCdrZXlzJylcbiAgLCB1aWQgICAgPSByZXF1aXJlKCcuL191aWQnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oa2V5KXtcbiAgcmV0dXJuIHNoYXJlZFtrZXldIHx8IChzaGFyZWRba2V5XSA9IHVpZChrZXkpKTtcbn07IiwidmFyIGdsb2JhbCA9IHJlcXVpcmUoJy4vX2dsb2JhbCcpXG4gICwgU0hBUkVEID0gJ19fY29yZS1qc19zaGFyZWRfXydcbiAgLCBzdG9yZSAgPSBnbG9iYWxbU0hBUkVEXSB8fCAoZ2xvYmFsW1NIQVJFRF0gPSB7fSk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGtleSl7XG4gIHJldHVybiBzdG9yZVtrZXldIHx8IChzdG9yZVtrZXldID0ge30pO1xufTsiLCJ2YXIgdG9JbnRlZ2VyID0gcmVxdWlyZSgnLi9fdG8taW50ZWdlcicpXG4gICwgbWF4ICAgICAgID0gTWF0aC5tYXhcbiAgLCBtaW4gICAgICAgPSBNYXRoLm1pbjtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaW5kZXgsIGxlbmd0aCl7XG4gIGluZGV4ID0gdG9JbnRlZ2VyKGluZGV4KTtcbiAgcmV0dXJuIGluZGV4IDwgMCA/IG1heChpbmRleCArIGxlbmd0aCwgMCkgOiBtaW4oaW5kZXgsIGxlbmd0aCk7XG59OyIsIi8vIDcuMS40IFRvSW50ZWdlclxudmFyIGNlaWwgID0gTWF0aC5jZWlsXG4gICwgZmxvb3IgPSBNYXRoLmZsb29yO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBpc05hTihpdCA9ICtpdCkgPyAwIDogKGl0ID4gMCA/IGZsb29yIDogY2VpbCkoaXQpO1xufTsiLCIvLyB0byBpbmRleGVkIG9iamVjdCwgdG9PYmplY3Qgd2l0aCBmYWxsYmFjayBmb3Igbm9uLWFycmF5LWxpa2UgRVMzIHN0cmluZ3NcbnZhciBJT2JqZWN0ID0gcmVxdWlyZSgnLi9faW9iamVjdCcpXG4gICwgZGVmaW5lZCA9IHJlcXVpcmUoJy4vX2RlZmluZWQnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gSU9iamVjdChkZWZpbmVkKGl0KSk7XG59OyIsIi8vIDcuMS4xNSBUb0xlbmd0aFxudmFyIHRvSW50ZWdlciA9IHJlcXVpcmUoJy4vX3RvLWludGVnZXInKVxuICAsIG1pbiAgICAgICA9IE1hdGgubWluO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBpdCA+IDAgPyBtaW4odG9JbnRlZ2VyKGl0KSwgMHgxZmZmZmZmZmZmZmZmZikgOiAwOyAvLyBwb3coMiwgNTMpIC0gMSA9PSA5MDA3MTk5MjU0NzQwOTkxXG59OyIsIi8vIDcuMS4xMyBUb09iamVjdChhcmd1bWVudClcbnZhciBkZWZpbmVkID0gcmVxdWlyZSgnLi9fZGVmaW5lZCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBPYmplY3QoZGVmaW5lZChpdCkpO1xufTsiLCIvLyA3LjEuMSBUb1ByaW1pdGl2ZShpbnB1dCBbLCBQcmVmZXJyZWRUeXBlXSlcbnZhciBpc09iamVjdCA9IHJlcXVpcmUoJy4vX2lzLW9iamVjdCcpO1xuLy8gaW5zdGVhZCBvZiB0aGUgRVM2IHNwZWMgdmVyc2lvbiwgd2UgZGlkbid0IGltcGxlbWVudCBAQHRvUHJpbWl0aXZlIGNhc2Vcbi8vIGFuZCB0aGUgc2Vjb25kIGFyZ3VtZW50IC0gZmxhZyAtIHByZWZlcnJlZCB0eXBlIGlzIGEgc3RyaW5nXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0LCBTKXtcbiAgaWYoIWlzT2JqZWN0KGl0KSlyZXR1cm4gaXQ7XG4gIHZhciBmbiwgdmFsO1xuICBpZihTICYmIHR5cGVvZiAoZm4gPSBpdC50b1N0cmluZykgPT0gJ2Z1bmN0aW9uJyAmJiAhaXNPYmplY3QodmFsID0gZm4uY2FsbChpdCkpKXJldHVybiB2YWw7XG4gIGlmKHR5cGVvZiAoZm4gPSBpdC52YWx1ZU9mKSA9PSAnZnVuY3Rpb24nICYmICFpc09iamVjdCh2YWwgPSBmbi5jYWxsKGl0KSkpcmV0dXJuIHZhbDtcbiAgaWYoIVMgJiYgdHlwZW9mIChmbiA9IGl0LnRvU3RyaW5nKSA9PSAnZnVuY3Rpb24nICYmICFpc09iamVjdCh2YWwgPSBmbi5jYWxsKGl0KSkpcmV0dXJuIHZhbDtcbiAgdGhyb3cgVHlwZUVycm9yKFwiQ2FuJ3QgY29udmVydCBvYmplY3QgdG8gcHJpbWl0aXZlIHZhbHVlXCIpO1xufTsiLCJ2YXIgaWQgPSAwXG4gICwgcHggPSBNYXRoLnJhbmRvbSgpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihrZXkpe1xuICByZXR1cm4gJ1N5bWJvbCgnLmNvbmNhdChrZXkgPT09IHVuZGVmaW5lZCA/ICcnIDoga2V5LCAnKV8nLCAoKytpZCArIHB4KS50b1N0cmluZygzNikpO1xufTsiLCIvLyAxOS4xLjMuMSBPYmplY3QuYXNzaWduKHRhcmdldCwgc291cmNlKVxudmFyICRleHBvcnQgPSByZXF1aXJlKCcuL19leHBvcnQnKTtcblxuJGV4cG9ydCgkZXhwb3J0LlMgKyAkZXhwb3J0LkYsICdPYmplY3QnLCB7YXNzaWduOiByZXF1aXJlKCcuL19vYmplY3QtYXNzaWduJyl9KTsiLCJ2YXIgX2V4dGVuZHMgPSBPYmplY3QuYXNzaWduIHx8IGZ1bmN0aW9uICh0YXJnZXQpIHsgZm9yICh2YXIgaSA9IDE7IGkgPCBhcmd1bWVudHMubGVuZ3RoOyBpKyspIHsgdmFyIHNvdXJjZSA9IGFyZ3VtZW50c1tpXTsgZm9yICh2YXIga2V5IGluIHNvdXJjZSkgeyBpZiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKHNvdXJjZSwga2V5KSkgeyB0YXJnZXRba2V5XSA9IHNvdXJjZVtrZXldOyB9IH0gfSByZXR1cm4gdGFyZ2V0OyB9O1xuXG52YXIgX3R5cGVvZiA9IHR5cGVvZiBTeW1ib2wgPT09IFwiZnVuY3Rpb25cIiAmJiB0eXBlb2YgU3ltYm9sLml0ZXJhdG9yID09PSBcInN5bWJvbFwiID8gZnVuY3Rpb24gKG9iaikgeyByZXR1cm4gdHlwZW9mIG9iajsgfSA6IGZ1bmN0aW9uIChvYmopIHsgcmV0dXJuIG9iaiAmJiB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgb2JqLmNvbnN0cnVjdG9yID09PSBTeW1ib2wgJiYgb2JqICE9PSBTeW1ib2wucHJvdG90eXBlID8gXCJzeW1ib2xcIiA6IHR5cGVvZiBvYmo7IH07XG5cbi8qISBmbGF0cGlja3IgdjIuNC40LCBAbGljZW5zZSBNSVQgKi9cbmZ1bmN0aW9uIEZsYXRwaWNrcihlbGVtZW50LCBjb25maWcpIHtcblx0dmFyIHNlbGYgPSB0aGlzO1xuXG5cdHNlbGYuY2hhbmdlTW9udGggPSBjaGFuZ2VNb250aDtcblx0c2VsZi5jaGFuZ2VZZWFyID0gY2hhbmdlWWVhcjtcblx0c2VsZi5jbGVhciA9IGNsZWFyO1xuXHRzZWxmLmNsb3NlID0gY2xvc2U7XG5cdHNlbGYuX2NyZWF0ZUVsZW1lbnQgPSBjcmVhdGVFbGVtZW50O1xuXHRzZWxmLmRlc3Ryb3kgPSBkZXN0cm95O1xuXHRzZWxmLmZvcm1hdERhdGUgPSBmb3JtYXREYXRlO1xuXHRzZWxmLmlzRW5hYmxlZCA9IGlzRW5hYmxlZDtcblx0c2VsZi5qdW1wVG9EYXRlID0ganVtcFRvRGF0ZTtcblx0c2VsZi5vcGVuID0gb3Blbjtcblx0c2VsZi5yZWRyYXcgPSByZWRyYXc7XG5cdHNlbGYuc2V0ID0gc2V0O1xuXHRzZWxmLnNldERhdGUgPSBzZXREYXRlO1xuXHRzZWxmLnRvZ2dsZSA9IHRvZ2dsZTtcblxuXHRmdW5jdGlvbiBpbml0KCkge1xuXHRcdGlmIChlbGVtZW50Ll9mbGF0cGlja3IpIGRlc3Ryb3koZWxlbWVudC5fZmxhdHBpY2tyKTtcblxuXHRcdGVsZW1lbnQuX2ZsYXRwaWNrciA9IHNlbGY7XG5cblx0XHRzZWxmLmVsZW1lbnQgPSBlbGVtZW50O1xuXHRcdHNlbGYuaW5zdGFuY2VDb25maWcgPSBjb25maWcgfHwge307XG5cdFx0c2VsZi5wYXJzZURhdGUgPSBGbGF0cGlja3IucHJvdG90eXBlLnBhcnNlRGF0ZS5iaW5kKHNlbGYpO1xuXG5cdFx0c2V0dXBGb3JtYXRzKCk7XG5cdFx0cGFyc2VDb25maWcoKTtcblx0XHRzZXR1cExvY2FsZSgpO1xuXHRcdHNldHVwSW5wdXRzKCk7XG5cdFx0c2V0dXBEYXRlcygpO1xuXHRcdHNldHVwSGVscGVyRnVuY3Rpb25zKCk7XG5cblx0XHRzZWxmLmlzT3BlbiA9IHNlbGYuY29uZmlnLmlubGluZTtcblxuXHRcdHNlbGYuaXNNb2JpbGUgPSAhc2VsZi5jb25maWcuZGlzYWJsZU1vYmlsZSAmJiAhc2VsZi5jb25maWcuaW5saW5lICYmIHNlbGYuY29uZmlnLm1vZGUgPT09IFwic2luZ2xlXCIgJiYgIXNlbGYuY29uZmlnLmRpc2FibGUubGVuZ3RoICYmICFzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoICYmICFzZWxmLmNvbmZpZy53ZWVrTnVtYmVycyAmJiAvQW5kcm9pZHx3ZWJPU3xpUGhvbmV8aVBhZHxpUG9kfEJsYWNrQmVycnl8SUVNb2JpbGV8T3BlcmEgTWluaS9pLnRlc3QobmF2aWdhdG9yLnVzZXJBZ2VudCk7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIGJ1aWxkKCk7XG5cblx0XHRiaW5kKCk7XG5cblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkge1xuXHRcdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHNldEhvdXJzRnJvbURhdGUoKTtcblx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzKSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLndpZHRoID0gc2VsZi5kYXlzLmNsaWVudFdpZHRoICsgc2VsZi53ZWVrV3JhcHBlci5jbGllbnRXaWR0aCArIFwicHhcIjtcblx0XHR9XG5cblx0XHRzZWxmLnNob3dUaW1lSW5wdXQgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoIHx8IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXI7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIHBvc2l0aW9uQ2FsZW5kYXIoKTtcblx0XHR0cmlnZ2VyRXZlbnQoXCJSZWFkeVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGJpbmRUb0luc3RhbmNlKGZuKSB7XG5cdFx0aWYgKGZuICYmIGZuLmJpbmQpIHJldHVybiBmbi5iaW5kKHNlbGYpO1xuXHRcdHJldHVybiBmbjtcblx0fVxuXG5cdGZ1bmN0aW9uIHVwZGF0ZVRpbWUoZSkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5ub0NhbGVuZGFyICYmICFzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKVxuXHRcdFx0Ly8gcGlja2luZyB0aW1lIG9ubHlcblx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtzZWxmLm5vd107XG5cblx0XHR0aW1lV3JhcHBlcihlKTtcblxuXHRcdGlmICghc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkgcmV0dXJuO1xuXG5cdFx0aWYgKCFzZWxmLm1pbkRhdGVIYXNUaW1lIHx8IGUudHlwZSAhPT0gXCJpbnB1dFwiIHx8IGUudGFyZ2V0LnZhbHVlLmxlbmd0aCA+PSAyKSB7XG5cdFx0XHRzZXRIb3Vyc0Zyb21JbnB1dHMoKTtcblx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0fSBlbHNlIHtcblx0XHRcdHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZXRIb3Vyc0Zyb21JbnB1dHMoKTtcblx0XHRcdFx0dXBkYXRlVmFsdWUoKTtcblx0XHRcdH0sIDEwMDApO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHNldEhvdXJzRnJvbUlucHV0cygpIHtcblx0XHRpZiAoIXNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHJldHVybjtcblxuXHRcdHZhciBob3VycyA9IHBhcnNlSW50KHNlbGYuaG91ckVsZW1lbnQudmFsdWUsIDEwKSB8fCAwLFxuXHRcdCAgICBtaW51dGVzID0gcGFyc2VJbnQoc2VsZi5taW51dGVFbGVtZW50LnZhbHVlLCAxMCkgfHwgMCxcblx0XHQgICAgc2Vjb25kcyA9IHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBwYXJzZUludChzZWxmLnNlY29uZEVsZW1lbnQudmFsdWUsIDEwKSB8fCAwIDogMDtcblxuXHRcdGlmIChzZWxmLmFtUE0pIGhvdXJzID0gaG91cnMgJSAxMiArIDEyICogKHNlbGYuYW1QTS50ZXh0Q29udGVudCA9PT0gXCJQTVwiKTtcblxuXHRcdGlmIChzZWxmLm1pbkRhdGVIYXNUaW1lICYmIGNvbXBhcmVEYXRlcyhzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiwgc2VsZi5jb25maWcubWluRGF0ZSkgPT09IDApIHtcblxuXHRcdFx0aG91cnMgPSBNYXRoLm1heChob3Vycywgc2VsZi5jb25maWcubWluRGF0ZS5nZXRIb3VycygpKTtcblx0XHRcdGlmIChob3VycyA9PT0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRIb3VycygpKSBtaW51dGVzID0gTWF0aC5tYXgobWludXRlcywgc2VsZi5jb25maWcubWluRGF0ZS5nZXRNaW51dGVzKCkpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLm1heERhdGVIYXNUaW1lICYmIGNvbXBhcmVEYXRlcyhzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiwgc2VsZi5jb25maWcubWF4RGF0ZSkgPT09IDApIHtcblx0XHRcdGhvdXJzID0gTWF0aC5taW4oaG91cnMsIHNlbGYuY29uZmlnLm1heERhdGUuZ2V0SG91cnMoKSk7XG5cdFx0XHRpZiAoaG91cnMgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0SG91cnMoKSkgbWludXRlcyA9IE1hdGgubWluKG1pbnV0ZXMsIHNlbGYuY29uZmlnLm1heERhdGUuZ2V0TWludXRlcygpKTtcblx0XHR9XG5cblx0XHRzZXRIb3Vycyhob3VycywgbWludXRlcywgc2Vjb25kcyk7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXRIb3Vyc0Zyb21EYXRlKGRhdGVPYmopIHtcblx0XHR2YXIgZGF0ZSA9IGRhdGVPYmogfHwgc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmo7XG5cblx0XHRpZiAoZGF0ZSkgc2V0SG91cnMoZGF0ZS5nZXRIb3VycygpLCBkYXRlLmdldE1pbnV0ZXMoKSwgZGF0ZS5nZXRTZWNvbmRzKCkpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0SG91cnMoaG91cnMsIG1pbnV0ZXMsIHNlY29uZHMpIHtcblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkge1xuXHRcdFx0c2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouc2V0SG91cnMoaG91cnMgJSAyNCwgbWludXRlcywgc2Vjb25kcyB8fCAwLCAwKTtcblx0XHR9XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmVuYWJsZVRpbWUgfHwgc2VsZi5pc01vYmlsZSkgcmV0dXJuO1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKCFzZWxmLmNvbmZpZy50aW1lXzI0aHIgPyAoMTIgKyBob3VycykgJSAxMiArIDEyICogKGhvdXJzICUgMTIgPT09IDApIDogaG91cnMpO1xuXG5cdFx0c2VsZi5taW51dGVFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQobWludXRlcyk7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLnRpbWVfMjRociAmJiBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSBzZWxmLmFtUE0udGV4dENvbnRlbnQgPSBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRIb3VycygpID49IDEyID8gXCJQTVwiIDogXCJBTVwiO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMpIHNlbGYuc2Vjb25kRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKHNlY29uZHMpO1xuXHR9XG5cblx0ZnVuY3Rpb24gb25ZZWFySW5wdXQoZXZlbnQpIHtcblx0XHR2YXIgeWVhciA9IGV2ZW50LnRhcmdldC52YWx1ZTtcblx0XHRpZiAoZXZlbnQuZGVsdGEpIHllYXIgPSAocGFyc2VJbnQoeWVhcikgKyBldmVudC5kZWx0YSkudG9TdHJpbmcoKTtcblxuXHRcdGlmICh5ZWFyLmxlbmd0aCA9PT0gNCkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYmx1cigpO1xuXHRcdFx0aWYgKCEvW15cXGRdLy50ZXN0KHllYXIpKSBjaGFuZ2VZZWFyKHllYXIpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIG9uTW9udGhTY3JvbGwoZSkge1xuXHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRzZWxmLmNoYW5nZU1vbnRoKE1hdGgubWF4KC0xLCBNYXRoLm1pbigxLCBlLndoZWVsRGVsdGEgfHwgLWUuZGVsdGFZKSkpO1xuXHR9XG5cblx0ZnVuY3Rpb24gYmluZCgpIHtcblx0XHRpZiAoc2VsZi5jb25maWcud3JhcCkge1xuXHRcdFx0W1wib3BlblwiLCBcImNsb3NlXCIsIFwidG9nZ2xlXCIsIFwiY2xlYXJcIl0uZm9yRWFjaChmdW5jdGlvbiAoZWwpIHtcblx0XHRcdFx0dmFyIHRvZ2dsZXMgPSBzZWxmLmVsZW1lbnQucXVlcnlTZWxlY3RvckFsbChcIltkYXRhLVwiICsgZWwgKyBcIl1cIik7XG5cdFx0XHRcdGZvciAodmFyIGkgPSAwOyBpIDwgdG9nZ2xlcy5sZW5ndGg7IGkrKykge1xuXHRcdFx0XHRcdHRvZ2dsZXNbaV0uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIHNlbGZbZWxdKTtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdFx0aWYgKHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFdmVudCAhPT0gdW5kZWZpbmVkKSB7XG5cdFx0XHRzZWxmLmNoYW5nZUV2ZW50ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50KFwiSFRNTEV2ZW50c1wiKTtcblx0XHRcdHNlbGYuY2hhbmdlRXZlbnQuaW5pdEV2ZW50KFwiY2hhbmdlXCIsIGZhbHNlLCB0cnVlKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5pc01vYmlsZSkgcmV0dXJuIHNldHVwTW9iaWxlKCk7XG5cblx0XHRzZWxmLmRlYm91bmNlZFJlc2l6ZSA9IGRlYm91bmNlKG9uUmVzaXplLCA1MCk7XG5cdFx0c2VsZi50cmlnZ2VyQ2hhbmdlID0gZnVuY3Rpb24gKCkge1xuXHRcdFx0dHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHRcdH07XG5cdFx0c2VsZi5kZWJvdW5jZWRDaGFuZ2UgPSBkZWJvdW5jZShzZWxmLnRyaWdnZXJDaGFuZ2UsIDMwMCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiICYmIHNlbGYuZGF5cykgc2VsZi5kYXlzLmFkZEV2ZW50TGlzdGVuZXIoXCJtb3VzZW92ZXJcIiwgb25Nb3VzZU92ZXIpO1xuXG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwia2V5ZG93blwiLCBvbktleURvd24pO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5zdGF0aWMgJiYgc2VsZi5jb25maWcuYWxsb3dJbnB1dCkgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuYWRkRXZlbnRMaXN0ZW5lcihcImtleWRvd25cIiwgb25LZXlEb3duKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuaW5saW5lICYmICFzZWxmLmNvbmZpZy5zdGF0aWMpIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKFwicmVzaXplXCIsIHNlbGYuZGVib3VuY2VkUmVzaXplKTtcblxuXHRcdGlmICh3aW5kb3cub250b3VjaHN0YXJ0KSB3aW5kb3cuZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcInRvdWNoc3RhcnRcIiwgZG9jdW1lbnRDbGljayk7XG5cblx0XHR3aW5kb3cuZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGRvY3VtZW50Q2xpY2spO1xuXHRcdHdpbmRvdy5kb2N1bWVudC5hZGRFdmVudExpc3RlbmVyKFwiYmx1clwiLCBkb2N1bWVudENsaWNrKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5jbGlja09wZW5zKSAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgb3Blbik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHtcblx0XHRcdHNlbGYucHJldk1vbnRoTmF2LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHJldHVybiBjaGFuZ2VNb250aCgtMSk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYubmV4dE1vbnRoTmF2LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHJldHVybiBjaGFuZ2VNb250aCgxKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIndoZWVsXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRcdHJldHVybiBkZWJvdW5jZShvbk1vbnRoU2Nyb2xsKGUpLCA1MCk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJ3aGVlbFwiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0XHRyZXR1cm4gZGVib3VuY2UoeWVhclNjcm9sbChlKSwgNTApO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5zZWxlY3QoKTtcblx0XHRcdH0pO1xuXG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiaW5wdXRcIiwgb25ZZWFySW5wdXQpO1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImluY3JlbWVudFwiLCBvblllYXJJbnB1dCk7XG5cblx0XHRcdHNlbGYuZGF5cy5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgc2VsZWN0RGF0ZSk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwidHJhbnNpdGlvbmVuZFwiLCBwb3NpdGlvbkNhbGVuZGFyKTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwid2hlZWxcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdFx0cmV0dXJuIGRlYm91bmNlKHVwZGF0ZVRpbWUoZSksIDUpO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImlucHV0XCIsIHVwZGF0ZVRpbWUpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJpbmNyZW1lbnRcIiwgdXBkYXRlVGltZSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImluY3JlbWVudFwiLCBzZWxmLmRlYm91bmNlZENoYW5nZSk7XG5cblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwid2hlZWxcIiwgc2VsZi5kZWJvdW5jZWRDaGFuZ2UpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJpbnB1dFwiLCBzZWxmLnRyaWdnZXJDaGFuZ2UpO1xuXG5cdFx0XHRzZWxmLmhvdXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNlbGYuaG91ckVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHR9KTtcblx0XHRcdHNlbGYubWludXRlRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHR9KTtcblxuXHRcdFx0aWYgKHNlbGYuc2Vjb25kRWxlbWVudCkge1xuXHRcdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5hbVBNKSB7XG5cdFx0XHRcdHNlbGYuYW1QTS5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdFx0XHR1cGRhdGVUaW1lKGUpO1xuXHRcdFx0XHRcdHNlbGYudHJpZ2dlckNoYW5nZShlKTtcblx0XHRcdFx0fSk7XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24ganVtcFRvRGF0ZShqdW1wRGF0ZSkge1xuXHRcdGp1bXBEYXRlID0ganVtcERhdGUgPyBzZWxmLnBhcnNlRGF0ZShqdW1wRGF0ZSkgOiBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiB8fCAoc2VsZi5jb25maWcubWluRGF0ZSA+IHNlbGYubm93ID8gc2VsZi5jb25maWcubWluRGF0ZSA6IHNlbGYuY29uZmlnLm1heERhdGUgJiYgc2VsZi5jb25maWcubWF4RGF0ZSA8IHNlbGYubm93ID8gc2VsZi5jb25maWcubWF4RGF0ZSA6IHNlbGYubm93KTtcblxuXHRcdHRyeSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyID0ganVtcERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0ganVtcERhdGUuZ2V0TW9udGgoKTtcblx0XHR9IGNhdGNoIChlKSB7XG5cdFx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdFx0Y29uc29sZS5lcnJvcihlLnN0YWNrKTtcblx0XHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0XHRjb25zb2xlLndhcm4oXCJJbnZhbGlkIGRhdGUgc3VwcGxpZWQ6IFwiICsganVtcERhdGUpO1xuXHRcdH1cblxuXHRcdHNlbGYucmVkcmF3KCk7XG5cdH1cblxuXHRmdW5jdGlvbiBpbmNyZW1lbnROdW1JbnB1dChlLCBkZWx0YSwgaW5wdXRFbGVtKSB7XG5cdFx0dmFyIGlucHV0ID0gaW5wdXRFbGVtIHx8IGUudGFyZ2V0LnBhcmVudE5vZGUuY2hpbGROb2Rlc1swXTtcblxuXHRcdGlmICh0eXBlb2YgRXZlbnQgIT09IFwidW5kZWZpbmVkXCIpIHtcblx0XHRcdHZhciBldiA9IG5ldyBFdmVudChcImluY3JlbWVudFwiLCB7IFwiYnViYmxlc1wiOiB0cnVlIH0pO1xuXHRcdFx0ZXYuZGVsdGEgPSBkZWx0YTtcblx0XHRcdGlucHV0LmRpc3BhdGNoRXZlbnQoZXYpO1xuXHRcdH0gZWxzZSB7XG5cdFx0XHR2YXIgX2V2ID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50KFwiQ3VzdG9tRXZlbnRcIik7XG5cdFx0XHRfZXYuaW5pdEN1c3RvbUV2ZW50KFwiaW5jcmVtZW50XCIsIHRydWUsIHRydWUsIHt9KTtcblx0XHRcdF9ldi5kZWx0YSA9IGRlbHRhO1xuXHRcdFx0aW5wdXQuZGlzcGF0Y2hFdmVudChfZXYpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGNyZWF0ZU51bWJlcklucHV0KGlucHV0Q2xhc3NOYW1lKSB7XG5cdFx0dmFyIHdyYXBwZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwibnVtSW5wdXRXcmFwcGVyXCIpLFxuXHRcdCAgICBudW1JbnB1dCA9IGNyZWF0ZUVsZW1lbnQoXCJpbnB1dFwiLCBcIm51bUlucHV0IFwiICsgaW5wdXRDbGFzc05hbWUpLFxuXHRcdCAgICBhcnJvd1VwID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJhcnJvd1VwXCIpLFxuXHRcdCAgICBhcnJvd0Rvd24gPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImFycm93RG93blwiKTtcblxuXHRcdG51bUlucHV0LnR5cGUgPSBcInRleHRcIjtcblx0XHRudW1JbnB1dC5wYXR0ZXJuID0gXCJcXFxcZCpcIjtcblx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKG51bUlucHV0KTtcblx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKGFycm93VXApO1xuXHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoYXJyb3dEb3duKTtcblxuXHRcdGFycm93VXAuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRyZXR1cm4gaW5jcmVtZW50TnVtSW5wdXQoZSwgMSk7XG5cdFx0fSk7XG5cdFx0YXJyb3dEb3duLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0cmV0dXJuIGluY3JlbWVudE51bUlucHV0KGUsIC0xKTtcblx0XHR9KTtcblx0XHRyZXR1cm4gd3JhcHBlcjtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkKCkge1xuXHRcdHZhciBmcmFnbWVudCA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVEb2N1bWVudEZyYWdtZW50KCk7XG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItY2FsZW5kYXJcIik7XG5cdFx0c2VsZi5udW1JbnB1dFR5cGUgPSBuYXZpZ2F0b3IudXNlckFnZW50LmluZGV4T2YoXCJNU0lFIDkuMFwiKSA+IDAgPyBcInRleHRcIiA6IFwibnVtYmVyXCI7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHtcblx0XHRcdGZyYWdtZW50LmFwcGVuZENoaWxkKGJ1aWxkTW9udGhOYXYoKSk7XG5cdFx0XHRzZWxmLmlubmVyQ29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1pbm5lckNvbnRhaW5lclwiKTtcblxuXHRcdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzKSBzZWxmLmlubmVyQ29udGFpbmVyLmFwcGVuZENoaWxkKGJ1aWxkV2Vla3MoKSk7XG5cblx0XHRcdHNlbGYuckNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItckNvbnRhaW5lclwiKTtcblx0XHRcdHNlbGYuckNvbnRhaW5lci5hcHBlbmRDaGlsZChidWlsZFdlZWtkYXlzKCkpO1xuXG5cdFx0XHRpZiAoIXNlbGYuZGF5cykge1xuXHRcdFx0XHRzZWxmLmRheXMgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLWRheXNcIik7XG5cdFx0XHRcdHNlbGYuZGF5cy50YWJJbmRleCA9IC0xO1xuXHRcdFx0fVxuXG5cdFx0XHRidWlsZERheXMoKTtcblx0XHRcdHNlbGYuckNvbnRhaW5lci5hcHBlbmRDaGlsZChzZWxmLmRheXMpO1xuXG5cdFx0XHRzZWxmLmlubmVyQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlbGYuckNvbnRhaW5lcik7XG5cdFx0XHRmcmFnbWVudC5hcHBlbmRDaGlsZChzZWxmLmlubmVyQ29udGFpbmVyKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlVGltZSkgZnJhZ21lbnQuYXBwZW5kQ2hpbGQoYnVpbGRUaW1lKCkpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIikgc2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwicmFuZ2VNb2RlXCIpO1xuXG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5hcHBlbmRDaGlsZChmcmFnbWVudCk7XG5cblx0XHR2YXIgY3VzdG9tQXBwZW5kID0gc2VsZi5jb25maWcuYXBwZW5kVG8gJiYgc2VsZi5jb25maWcuYXBwZW5kVG8ubm9kZVR5cGU7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuaW5saW5lIHx8IHNlbGYuY29uZmlnLnN0YXRpYykge1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKHNlbGYuY29uZmlnLmlubGluZSA/IFwiaW5saW5lXCIgOiBcInN0YXRpY1wiKTtcblx0XHRcdHBvc2l0aW9uQ2FsZW5kYXIoKTtcblxuXHRcdFx0aWYgKHNlbGYuY29uZmlnLmlubGluZSAmJiAhY3VzdG9tQXBwZW5kKSB7XG5cdFx0XHRcdHJldHVybiBzZWxmLmVsZW1lbnQucGFyZW50Tm9kZS5pbnNlcnRCZWZvcmUoc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkubmV4dFNpYmxpbmcpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5jb25maWcuc3RhdGljKSB7XG5cdFx0XHRcdHZhciB3cmFwcGVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci13cmFwcGVyXCIpO1xuXHRcdFx0XHRzZWxmLmVsZW1lbnQucGFyZW50Tm9kZS5pbnNlcnRCZWZvcmUod3JhcHBlciwgc2VsZi5lbGVtZW50KTtcblx0XHRcdFx0d3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLmVsZW1lbnQpO1xuXG5cdFx0XHRcdGlmIChzZWxmLmFsdElucHV0KSB3cmFwcGVyLmFwcGVuZENoaWxkKHNlbGYuYWx0SW5wdXQpO1xuXG5cdFx0XHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoc2VsZi5jYWxlbmRhckNvbnRhaW5lcik7XG5cdFx0XHRcdHJldHVybjtcblx0XHRcdH1cblx0XHR9XG5cblx0XHQoY3VzdG9tQXBwZW5kID8gc2VsZi5jb25maWcuYXBwZW5kVG8gOiB3aW5kb3cuZG9jdW1lbnQuYm9keSkuYXBwZW5kQ2hpbGQoc2VsZi5jYWxlbmRhckNvbnRhaW5lcik7XG5cdH1cblxuXHRmdW5jdGlvbiBjcmVhdGVEYXkoY2xhc3NOYW1lLCBkYXRlLCBkYXlOdW1iZXIpIHtcblx0XHR2YXIgZGF0ZUlzRW5hYmxlZCA9IGlzRW5hYmxlZChkYXRlLCB0cnVlKSxcblx0XHQgICAgZGF5RWxlbWVudCA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLWRheSBcIiArIGNsYXNzTmFtZSwgZGF0ZS5nZXREYXRlKCkpO1xuXG5cdFx0ZGF5RWxlbWVudC5kYXRlT2JqID0gZGF0ZTtcblxuXHRcdHRvZ2dsZUNsYXNzKGRheUVsZW1lbnQsIFwidG9kYXlcIiwgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYubm93KSA9PT0gMCk7XG5cblx0XHRpZiAoZGF0ZUlzRW5hYmxlZCkge1xuXHRcdFx0ZGF5RWxlbWVudC50YWJJbmRleCA9IDA7XG5cblx0XHRcdGlmIChpc0RhdGVTZWxlY3RlZChkYXRlKSkge1xuXHRcdFx0XHRkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJzZWxlY3RlZFwiKTtcblx0XHRcdFx0c2VsZi5zZWxlY3RlZERhdGVFbGVtID0gZGF5RWxlbWVudDtcblx0XHRcdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIikge1xuXHRcdFx0XHRcdHRvZ2dsZUNsYXNzKGRheUVsZW1lbnQsIFwic3RhcnRSYW5nZVwiLCBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5zZWxlY3RlZERhdGVzWzBdKSA9PT0gMCk7XG5cblx0XHRcdFx0XHR0b2dnbGVDbGFzcyhkYXlFbGVtZW50LCBcImVuZFJhbmdlXCIsIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMV0pID09PSAwKTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXHRcdH0gZWxzZSB7XG5cdFx0XHRkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJkaXNhYmxlZFwiKTtcblx0XHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXNbMF0gJiYgZGF0ZSA+IHNlbGYubWluUmFuZ2VEYXRlICYmIGRhdGUgPCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pIHNlbGYubWluUmFuZ2VEYXRlID0gZGF0ZTtlbHNlIGlmIChzZWxmLnNlbGVjdGVkRGF0ZXNbMF0gJiYgZGF0ZSA8IHNlbGYubWF4UmFuZ2VEYXRlICYmIGRhdGUgPiBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pIHNlbGYubWF4UmFuZ2VEYXRlID0gZGF0ZTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRpZiAoaXNEYXRlSW5SYW5nZShkYXRlKSAmJiAhaXNEYXRlU2VsZWN0ZWQoZGF0ZSkpIGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcImluUmFuZ2VcIik7XG5cblx0XHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID09PSAxICYmIChkYXRlIDwgc2VsZi5taW5SYW5nZURhdGUgfHwgZGF0ZSA+IHNlbGYubWF4UmFuZ2VEYXRlKSkgZGF5RWxlbWVudC5jbGFzc0xpc3QuYWRkKFwibm90QWxsb3dlZFwiKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcud2Vla051bWJlcnMgJiYgY2xhc3NOYW1lICE9PSBcInByZXZNb250aERheVwiICYmIGRheU51bWJlciAlIDcgPT09IDEpIHtcblx0XHRcdHNlbGYud2Vla051bWJlcnMuaW5zZXJ0QWRqYWNlbnRIVE1MKFwiYmVmb3JlZW5kXCIsIFwiPHNwYW4gY2xhc3M9J2Rpc2FibGVkIGZsYXRwaWNrci1kYXknPlwiICsgc2VsZi5jb25maWcuZ2V0V2VlayhkYXRlKSArIFwiPC9zcGFuPlwiKTtcblx0XHR9XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJEYXlDcmVhdGVcIiwgZGF5RWxlbWVudCk7XG5cblx0XHRyZXR1cm4gZGF5RWxlbWVudDtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkRGF5cyh5ZWFyLCBtb250aCkge1xuXHRcdHZhciBmaXJzdE9mTW9udGggPSAobmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGgsIDEpLmdldERheSgpIC0gc2VsZi5sMTBuLmZpcnN0RGF5T2ZXZWVrICsgNykgJSA3LFxuXHRcdCAgICBpc1JhbmdlTW9kZSA9IHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIjtcblxuXHRcdHNlbGYucHJldk1vbnRoRGF5cyA9IHNlbGYudXRpbHMuZ2V0RGF5c2luTW9udGgoKHNlbGYuY3VycmVudE1vbnRoIC0gMSArIDEyKSAlIDEyKTtcblxuXHRcdHZhciBkYXlzSW5Nb250aCA9IHNlbGYudXRpbHMuZ2V0RGF5c2luTW9udGgoKSxcblx0XHQgICAgZGF5cyA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVEb2N1bWVudEZyYWdtZW50KCk7XG5cblx0XHR2YXIgZGF5TnVtYmVyID0gc2VsZi5wcmV2TW9udGhEYXlzICsgMSAtIGZpcnN0T2ZNb250aDtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycyAmJiBzZWxmLndlZWtOdW1iZXJzLmZpcnN0Q2hpbGQpIHNlbGYud2Vla051bWJlcnMudGV4dENvbnRlbnQgPSBcIlwiO1xuXG5cdFx0aWYgKGlzUmFuZ2VNb2RlKSB7XG5cdFx0XHQvLyBjb25zdCBkYXRlTGltaXRzID0gc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCB8fCBzZWxmLmNvbmZpZy5kaXNhYmxlLmxlbmd0aCB8fCBzZWxmLmNvbmZpZy5taXhEYXRlIHx8IHNlbGYuY29uZmlnLm1heERhdGU7XG5cdFx0XHRzZWxmLm1pblJhbmdlRGF0ZSA9IG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoIC0gMSwgZGF5TnVtYmVyKTtcblx0XHRcdHNlbGYubWF4UmFuZ2VEYXRlID0gbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggKyAxLCAoNDIgLSBmaXJzdE9mTW9udGgpICUgZGF5c0luTW9udGgpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmRheXMuZmlyc3RDaGlsZCkgc2VsZi5kYXlzLnRleHRDb250ZW50ID0gXCJcIjtcblxuXHRcdC8vIHByZXBlbmQgZGF5cyBmcm9tIHRoZSBlbmRpbmcgb2YgcHJldmlvdXMgbW9udGhcblx0XHRmb3IgKDsgZGF5TnVtYmVyIDw9IHNlbGYucHJldk1vbnRoRGF5czsgZGF5TnVtYmVyKyspIHtcblx0XHRcdGRheXMuYXBwZW5kQ2hpbGQoY3JlYXRlRGF5KFwicHJldk1vbnRoRGF5XCIsIG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoIC0gMSwgZGF5TnVtYmVyKSwgZGF5TnVtYmVyKSk7XG5cdFx0fVxuXG5cdFx0Ly8gU3RhcnQgYXQgMSBzaW5jZSB0aGVyZSBpcyBubyAwdGggZGF5XG5cdFx0Zm9yIChkYXlOdW1iZXIgPSAxOyBkYXlOdW1iZXIgPD0gZGF5c0luTW9udGg7IGRheU51bWJlcisrKSB7XG5cdFx0XHRkYXlzLmFwcGVuZENoaWxkKGNyZWF0ZURheShcIlwiLCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCwgZGF5TnVtYmVyKSwgZGF5TnVtYmVyKSk7XG5cdFx0fVxuXG5cdFx0Ly8gYXBwZW5kIGRheXMgZnJvbSB0aGUgbmV4dCBtb250aFxuXHRcdGZvciAodmFyIGRheU51bSA9IGRheXNJbk1vbnRoICsgMTsgZGF5TnVtIDw9IDQyIC0gZmlyc3RPZk1vbnRoOyBkYXlOdW0rKykge1xuXHRcdFx0ZGF5cy5hcHBlbmRDaGlsZChjcmVhdGVEYXkoXCJuZXh0TW9udGhEYXlcIiwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggKyAxLCBkYXlOdW0gJSBkYXlzSW5Nb250aCksIGRheU51bSkpO1xuXHRcdH1cblxuXHRcdGlmIChpc1JhbmdlTW9kZSAmJiBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID09PSAxICYmIGRheXMuY2hpbGROb2Rlc1swXSkge1xuXHRcdFx0c2VsZi5faGlkZVByZXZNb250aEFycm93ID0gc2VsZi5faGlkZVByZXZNb250aEFycm93IHx8IHNlbGYubWluUmFuZ2VEYXRlID4gZGF5cy5jaGlsZE5vZGVzWzBdLmRhdGVPYmo7XG5cblx0XHRcdHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyA9IHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyB8fCBzZWxmLm1heFJhbmdlRGF0ZSA8IG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoICsgMSwgMSk7XG5cdFx0fSBlbHNlIHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblxuXHRcdHNlbGYuZGF5cy5hcHBlbmRDaGlsZChkYXlzKTtcblx0XHRyZXR1cm4gc2VsZi5kYXlzO1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGRNb250aE5hdigpIHtcblx0XHR2YXIgbW9udGhOYXZGcmFnbWVudCA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVEb2N1bWVudEZyYWdtZW50KCk7XG5cdFx0c2VsZi5tb250aE5hdiA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItbW9udGhcIik7XG5cblx0XHRzZWxmLnByZXZNb250aE5hdiA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXByZXYtbW9udGhcIik7XG5cdFx0c2VsZi5wcmV2TW9udGhOYXYuaW5uZXJIVE1MID0gc2VsZi5jb25maWcucHJldkFycm93O1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50ID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJjdXItbW9udGhcIik7XG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LnRpdGxlID0gc2VsZi5sMTBuLnNjcm9sbFRpdGxlO1xuXG5cdFx0dmFyIHllYXJJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiY3VyLXllYXJcIik7XG5cdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQgPSB5ZWFySW5wdXQuY2hpbGROb2Rlc1swXTtcblx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC50aXRsZSA9IHNlbGYubDEwbi5zY3JvbGxUaXRsZTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5taW5EYXRlKSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5taW4gPSBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWF4RGF0ZSkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWF4ID0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpO1xuXG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5kaXNhYmxlZCA9IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0fVxuXG5cdFx0c2VsZi5uZXh0TW9udGhOYXYgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1uZXh0LW1vbnRoXCIpO1xuXHRcdHNlbGYubmV4dE1vbnRoTmF2LmlubmVySFRNTCA9IHNlbGYuY29uZmlnLm5leHRBcnJvdztcblxuXHRcdHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aCA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLWN1cnJlbnQtbW9udGhcIik7XG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoLmFwcGVuZENoaWxkKHNlbGYuY3VycmVudE1vbnRoRWxlbWVudCk7XG5cdFx0c2VsZi5uYXZpZ2F0aW9uQ3VycmVudE1vbnRoLmFwcGVuZENoaWxkKHllYXJJbnB1dCk7XG5cblx0XHRtb250aE5hdkZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYucHJldk1vbnRoTmF2KTtcblx0XHRtb250aE5hdkZyYWdtZW50LmFwcGVuZENoaWxkKHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aCk7XG5cdFx0bW9udGhOYXZGcmFnbWVudC5hcHBlbmRDaGlsZChzZWxmLm5leHRNb250aE5hdik7XG5cdFx0c2VsZi5tb250aE5hdi5hcHBlbmRDaGlsZChtb250aE5hdkZyYWdtZW50KTtcblxuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcIl9oaWRlUHJldk1vbnRoQXJyb3dcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiB0aGlzLl9faGlkZVByZXZNb250aEFycm93O1xuXHRcdFx0fSxcblx0XHRcdHNldDogZnVuY3Rpb24gc2V0KGJvb2wpIHtcblx0XHRcdFx0aWYgKHRoaXMuX19oaWRlUHJldk1vbnRoQXJyb3cgIT09IGJvb2wpIHNlbGYucHJldk1vbnRoTmF2LnN0eWxlLmRpc3BsYXkgPSBib29sID8gXCJub25lXCIgOiBcImJsb2NrXCI7XG5cdFx0XHRcdHRoaXMuX19oaWRlUHJldk1vbnRoQXJyb3cgPSBib29sO1xuXHRcdFx0fVxuXHRcdH0pO1xuXG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwiX2hpZGVOZXh0TW9udGhBcnJvd1wiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX19oaWRlTmV4dE1vbnRoQXJyb3c7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRpZiAodGhpcy5fX2hpZGVOZXh0TW9udGhBcnJvdyAhPT0gYm9vbCkgc2VsZi5uZXh0TW9udGhOYXYuc3R5bGUuZGlzcGxheSA9IGJvb2wgPyBcIm5vbmVcIiA6IFwiYmxvY2tcIjtcblx0XHRcdFx0dGhpcy5fX2hpZGVOZXh0TW9udGhBcnJvdyA9IGJvb2w7XG5cdFx0XHR9XG5cdFx0fSk7XG5cblx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cblx0XHRyZXR1cm4gc2VsZi5tb250aE5hdjtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkVGltZSgpIHtcblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJoYXNUaW1lXCIpO1xuXHRcdGlmIChzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJub0NhbGVuZGFyXCIpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItdGltZVwiKTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIudGFiSW5kZXggPSAtMTtcblx0XHR2YXIgc2VwYXJhdG9yID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItdGltZS1zZXBhcmF0b3JcIiwgXCI6XCIpO1xuXG5cdFx0dmFyIGhvdXJJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiZmxhdHBpY2tyLWhvdXJcIik7XG5cdFx0c2VsZi5ob3VyRWxlbWVudCA9IGhvdXJJbnB1dC5jaGlsZE5vZGVzWzBdO1xuXG5cdFx0dmFyIG1pbnV0ZUlucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJmbGF0cGlja3ItbWludXRlXCIpO1xuXHRcdHNlbGYubWludXRlRWxlbWVudCA9IG1pbnV0ZUlucHV0LmNoaWxkTm9kZXNbMF07XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnRhYkluZGV4ID0gc2VsZi5taW51dGVFbGVtZW50LnRhYkluZGV4ID0gMDtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZChzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA/IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLmdldEhvdXJzKCkgOiBzZWxmLmNvbmZpZy5kZWZhdWx0SG91cik7XG5cblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZChzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA/IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLmdldE1pbnV0ZXMoKSA6IHNlbGYuY29uZmlnLmRlZmF1bHRNaW51dGUpO1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC5zdGVwID0gc2VsZi5jb25maWcuaG91ckluY3JlbWVudDtcblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQuc3RlcCA9IHNlbGYuY29uZmlnLm1pbnV0ZUluY3JlbWVudDtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQubWluID0gc2VsZi5jb25maWcudGltZV8yNGhyID8gMCA6IDE7XG5cdFx0c2VsZi5ob3VyRWxlbWVudC5tYXggPSBzZWxmLmNvbmZpZy50aW1lXzI0aHIgPyAyMyA6IDEyO1xuXG5cdFx0c2VsZi5taW51dGVFbGVtZW50Lm1pbiA9IDA7XG5cdFx0c2VsZi5taW51dGVFbGVtZW50Lm1heCA9IDU5O1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC50aXRsZSA9IHNlbGYubWludXRlRWxlbWVudC50aXRsZSA9IHNlbGYubDEwbi5zY3JvbGxUaXRsZTtcblxuXHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChob3VySW5wdXQpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChzZXBhcmF0b3IpO1xuXHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChtaW51dGVJbnB1dCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcudGltZV8yNGhyKSBzZWxmLnRpbWVDb250YWluZXIuY2xhc3NMaXN0LmFkZChcInRpbWUyNGhyXCIpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMpIHtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwiaGFzU2Vjb25kc1wiKTtcblxuXHRcdFx0dmFyIHNlY29uZElucHV0ID0gY3JlYXRlTnVtYmVySW5wdXQoXCJmbGF0cGlja3Itc2Vjb25kXCIpO1xuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50ID0gc2Vjb25kSW5wdXQuY2hpbGROb2Rlc1swXTtcblxuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50LnZhbHVlID0gc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPyBzZWxmLnBhZChzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRTZWNvbmRzKCkpIDogXCIwMFwiO1xuXG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQuc3RlcCA9IHNlbGYubWludXRlRWxlbWVudC5zdGVwO1xuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50Lm1pbiA9IHNlbGYubWludXRlRWxlbWVudC5taW47XG5cdFx0XHRzZWxmLnNlY29uZEVsZW1lbnQubWF4ID0gc2VsZi5taW51dGVFbGVtZW50Lm1heDtcblxuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXRpbWUtc2VwYXJhdG9yXCIsIFwiOlwiKSk7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoc2Vjb25kSW5wdXQpO1xuXHRcdH1cblxuXHRcdGlmICghc2VsZi5jb25maWcudGltZV8yNGhyKSB7XG5cdFx0XHQvLyBhZGQgc2VsZi5hbVBNIGlmIGFwcHJvcHJpYXRlXG5cdFx0XHRzZWxmLmFtUE0gPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1hbS1wbVwiLCBbXCJBTVwiLCBcIlBNXCJdW3NlbGYuaG91ckVsZW1lbnQudmFsdWUgPiAxMSB8IDBdKTtcblx0XHRcdHNlbGYuYW1QTS50aXRsZSA9IHNlbGYubDEwbi50b2dnbGVUaXRsZTtcblx0XHRcdHNlbGYuYW1QTS50YWJJbmRleCA9IDA7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VsZi5hbVBNKTtcblx0XHR9XG5cblx0XHRyZXR1cm4gc2VsZi50aW1lQ29udGFpbmVyO1xuXHR9XG5cblx0ZnVuY3Rpb24gYnVpbGRXZWVrZGF5cygpIHtcblx0XHRpZiAoIXNlbGYud2Vla2RheUNvbnRhaW5lcikgc2VsZi53ZWVrZGF5Q29udGFpbmVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci13ZWVrZGF5c1wiKTtcblxuXHRcdHZhciBmaXJzdERheU9mV2VlayA9IHNlbGYubDEwbi5maXJzdERheU9mV2Vlaztcblx0XHR2YXIgd2Vla2RheXMgPSBzZWxmLmwxMG4ud2Vla2RheXMuc2hvcnRoYW5kLnNsaWNlKCk7XG5cblx0XHRpZiAoZmlyc3REYXlPZldlZWsgPiAwICYmIGZpcnN0RGF5T2ZXZWVrIDwgd2Vla2RheXMubGVuZ3RoKSB7XG5cdFx0XHR3ZWVrZGF5cyA9IFtdLmNvbmNhdCh3ZWVrZGF5cy5zcGxpY2UoZmlyc3REYXlPZldlZWssIHdlZWtkYXlzLmxlbmd0aCksIHdlZWtkYXlzLnNwbGljZSgwLCBmaXJzdERheU9mV2VlaykpO1xuXHRcdH1cblxuXHRcdHNlbGYud2Vla2RheUNvbnRhaW5lci5pbm5lckhUTUwgPSBcIlxcblxcdFxcdDxzcGFuIGNsYXNzPWZsYXRwaWNrci13ZWVrZGF5PlxcblxcdFxcdFxcdFwiICsgd2Vla2RheXMuam9pbihcIjwvc3Bhbj48c3BhbiBjbGFzcz1mbGF0cGlja3Itd2Vla2RheT5cIikgKyBcIlxcblxcdFxcdDwvc3Bhbj5cXG5cXHRcXHRcIjtcblxuXHRcdHJldHVybiBzZWxmLndlZWtkYXlDb250YWluZXI7XG5cdH1cblxuXHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRmdW5jdGlvbiBidWlsZFdlZWtzKCkge1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcImhhc1dlZWtzXCIpO1xuXHRcdHNlbGYud2Vla1dyYXBwZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXdlZWt3cmFwcGVyXCIpO1xuXHRcdHNlbGYud2Vla1dyYXBwZXIuYXBwZW5kQ2hpbGQoY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3Itd2Vla2RheVwiLCBzZWxmLmwxMG4ud2Vla0FiYnJldmlhdGlvbikpO1xuXHRcdHNlbGYud2Vla051bWJlcnMgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXdlZWtzXCIpO1xuXHRcdHNlbGYud2Vla1dyYXBwZXIuYXBwZW5kQ2hpbGQoc2VsZi53ZWVrTnVtYmVycyk7XG5cblx0XHRyZXR1cm4gc2VsZi53ZWVrV3JhcHBlcjtcblx0fVxuXG5cdGZ1bmN0aW9uIGNoYW5nZU1vbnRoKHZhbHVlLCBpc19vZmZzZXQpIHtcblx0XHRpc19vZmZzZXQgPSB0eXBlb2YgaXNfb2Zmc2V0ID09PSBcInVuZGVmaW5lZFwiIHx8IGlzX29mZnNldDtcblx0XHR2YXIgZGVsdGEgPSBpc19vZmZzZXQgPyB2YWx1ZSA6IHZhbHVlIC0gc2VsZi5jdXJyZW50TW9udGg7XG5cblx0XHRpZiAoZGVsdGEgPCAwICYmIHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyB8fCBkZWx0YSA+IDAgJiYgc2VsZi5faGlkZU5leHRNb250aEFycm93KSByZXR1cm47XG5cblx0XHRzZWxmLmN1cnJlbnRNb250aCArPSBkZWx0YTtcblxuXHRcdGlmIChzZWxmLmN1cnJlbnRNb250aCA8IDAgfHwgc2VsZi5jdXJyZW50TW9udGggPiAxMSkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhciArPSBzZWxmLmN1cnJlbnRNb250aCA+IDExID8gMSA6IC0xO1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSAoc2VsZi5jdXJyZW50TW9udGggKyAxMikgJSAxMjtcblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiWWVhckNoYW5nZVwiKTtcblx0XHR9XG5cblx0XHR1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cdFx0YnVpbGREYXlzKCk7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLm5vQ2FsZW5kYXIpIHNlbGYuZGF5cy5mb2N1cygpO1xuXG5cdFx0dHJpZ2dlckV2ZW50KFwiTW9udGhDaGFuZ2VcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBjbGVhcih0cmlnZ2VyQ2hhbmdlRXZlbnQpIHtcblx0XHRzZWxmLmlucHV0LnZhbHVlID0gXCJcIjtcblxuXHRcdGlmIChzZWxmLmFsdElucHV0KSBzZWxmLmFsdElucHV0LnZhbHVlID0gXCJcIjtcblxuXHRcdGlmIChzZWxmLm1vYmlsZUlucHV0KSBzZWxmLm1vYmlsZUlucHV0LnZhbHVlID0gXCJcIjtcblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtdO1xuXHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gbnVsbDtcblx0XHRzZWxmLnNob3dUaW1lSW5wdXQgPSBmYWxzZTtcblxuXHRcdHNlbGYucmVkcmF3KCk7XG5cblx0XHRpZiAodHJpZ2dlckNoYW5nZUV2ZW50ICE9PSBmYWxzZSlcblx0XHRcdC8vIHRyaWdnZXJDaGFuZ2VFdmVudCBpcyB0cnVlIChkZWZhdWx0KSBvciBhbiBFdmVudFxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2xvc2UoKSB7XG5cdFx0c2VsZi5pc09wZW4gPSBmYWxzZTtcblxuXHRcdGlmICghc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QucmVtb3ZlKFwib3BlblwiKTtcblx0XHRcdChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmNsYXNzTGlzdC5yZW1vdmUoXCJhY3RpdmVcIik7XG5cdFx0fVxuXG5cdFx0dHJpZ2dlckV2ZW50KFwiQ2xvc2VcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBkZXN0cm95KGluc3RhbmNlKSB7XG5cdFx0aW5zdGFuY2UgPSBpbnN0YW5jZSB8fCBzZWxmO1xuXHRcdGluc3RhbmNlLmNsZWFyKGZhbHNlKTtcblxuXHRcdHdpbmRvdy5yZW1vdmVFdmVudExpc3RlbmVyKFwicmVzaXplXCIsIGluc3RhbmNlLmRlYm91bmNlZFJlc2l6ZSk7XG5cblx0XHR3aW5kb3cuZG9jdW1lbnQucmVtb3ZlRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGRvY3VtZW50Q2xpY2spO1xuXHRcdHdpbmRvdy5kb2N1bWVudC5yZW1vdmVFdmVudExpc3RlbmVyKFwidG91Y2hzdGFydFwiLCBkb2N1bWVudENsaWNrKTtcblx0XHR3aW5kb3cuZG9jdW1lbnQucmVtb3ZlRXZlbnRMaXN0ZW5lcihcImJsdXJcIiwgZG9jdW1lbnRDbGljayk7XG5cblx0XHRpZiAoaW5zdGFuY2UudGltZUNvbnRhaW5lcikgaW5zdGFuY2UudGltZUNvbnRhaW5lci5yZW1vdmVFdmVudExpc3RlbmVyKFwidHJhbnNpdGlvbmVuZFwiLCBwb3NpdGlvbkNhbGVuZGFyKTtcblxuXHRcdGlmIChpbnN0YW5jZS5tb2JpbGVJbnB1dCkge1xuXHRcdFx0aWYgKGluc3RhbmNlLm1vYmlsZUlucHV0LnBhcmVudE5vZGUpIGluc3RhbmNlLm1vYmlsZUlucHV0LnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoaW5zdGFuY2UubW9iaWxlSW5wdXQpO1xuXHRcdFx0ZGVsZXRlIGluc3RhbmNlLm1vYmlsZUlucHV0O1xuXHRcdH0gZWxzZSBpZiAoaW5zdGFuY2UuY2FsZW5kYXJDb250YWluZXIgJiYgaW5zdGFuY2UuY2FsZW5kYXJDb250YWluZXIucGFyZW50Tm9kZSkgaW5zdGFuY2UuY2FsZW5kYXJDb250YWluZXIucGFyZW50Tm9kZS5yZW1vdmVDaGlsZChpbnN0YW5jZS5jYWxlbmRhckNvbnRhaW5lcik7XG5cblx0XHRpZiAoaW5zdGFuY2UuYWx0SW5wdXQpIHtcblx0XHRcdGluc3RhbmNlLmlucHV0LnR5cGUgPSBcInRleHRcIjtcblx0XHRcdGlmIChpbnN0YW5jZS5hbHRJbnB1dC5wYXJlbnROb2RlKSBpbnN0YW5jZS5hbHRJbnB1dC5wYXJlbnROb2RlLnJlbW92ZUNoaWxkKGluc3RhbmNlLmFsdElucHV0KTtcblx0XHRcdGRlbGV0ZSBpbnN0YW5jZS5hbHRJbnB1dDtcblx0XHR9XG5cblx0XHRpbnN0YW5jZS5pbnB1dC50eXBlID0gaW5zdGFuY2UuaW5wdXQuX3R5cGU7XG5cdFx0aW5zdGFuY2UuaW5wdXQuY2xhc3NMaXN0LnJlbW92ZShcImZsYXRwaWNrci1pbnB1dFwiKTtcblx0XHRpbnN0YW5jZS5pbnB1dC5yZW1vdmVFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgb3Blbik7XG5cdFx0aW5zdGFuY2UuaW5wdXQucmVtb3ZlQXR0cmlidXRlKFwicmVhZG9ubHlcIik7XG5cblx0XHRkZWxldGUgaW5zdGFuY2UuaW5wdXQuX2ZsYXRwaWNrcjtcblx0fVxuXG5cdGZ1bmN0aW9uIGlzQ2FsZW5kYXJFbGVtKGVsZW0pIHtcblx0XHRpZiAoc2VsZi5jb25maWcuYXBwZW5kVG8gJiYgc2VsZi5jb25maWcuYXBwZW5kVG8uY29udGFpbnMoZWxlbSkpIHJldHVybiB0cnVlO1xuXG5cdFx0cmV0dXJuIHNlbGYuY2FsZW5kYXJDb250YWluZXIuY29udGFpbnMoZWxlbSk7XG5cdH1cblxuXHRmdW5jdGlvbiBkb2N1bWVudENsaWNrKGUpIHtcblx0XHR2YXIgaXNJbnB1dCA9IHNlbGYuZWxlbWVudC5jb250YWlucyhlLnRhcmdldCkgfHwgZS50YXJnZXQgPT09IHNlbGYuaW5wdXQgfHwgZS50YXJnZXQgPT09IHNlbGYuYWx0SW5wdXQgfHxcblx0XHQvLyB3ZWIgY29tcG9uZW50c1xuXHRcdGUucGF0aCAmJiBlLnBhdGguaW5kZXhPZiAmJiAofmUucGF0aC5pbmRleE9mKHNlbGYuaW5wdXQpIHx8IH5lLnBhdGguaW5kZXhPZihzZWxmLmFsdElucHV0KSk7XG5cblx0XHRpZiAoc2VsZi5pc09wZW4gJiYgIXNlbGYuY29uZmlnLmlubGluZSAmJiAhaXNDYWxlbmRhckVsZW0oZS50YXJnZXQpICYmICFpc0lucHV0KSB7XG5cdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRzZWxmLmNsb3NlKCk7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSkge1xuXHRcdFx0XHRzZWxmLmNsZWFyKCk7XG5cdFx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gZm9ybWF0RGF0ZShmcm10LCBkYXRlT2JqKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmZvcm1hdERhdGUpIHJldHVybiBzZWxmLmNvbmZpZy5mb3JtYXREYXRlKGZybXQsIGRhdGVPYmopO1xuXG5cdFx0dmFyIGNoYXJzID0gZnJtdC5zcGxpdChcIlwiKTtcblx0XHRyZXR1cm4gY2hhcnMubWFwKGZ1bmN0aW9uIChjLCBpKSB7XG5cdFx0XHRyZXR1cm4gc2VsZi5mb3JtYXRzW2NdICYmIGNoYXJzW2kgLSAxXSAhPT0gXCJcXFxcXCIgPyBzZWxmLmZvcm1hdHNbY10oZGF0ZU9iaikgOiBjICE9PSBcIlxcXFxcIiA/IGMgOiBcIlwiO1xuXHRcdH0pLmpvaW4oXCJcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBjaGFuZ2VZZWFyKG5ld1llYXIpIHtcblx0XHRpZiAoIW5ld1llYXIgfHwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWluICYmIG5ld1llYXIgPCBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5taW4gfHwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWF4ICYmIG5ld1llYXIgPiBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5tYXgpIHJldHVybjtcblxuXHRcdHZhciBuZXdZZWFyTnVtID0gcGFyc2VJbnQobmV3WWVhciwgMTApLFxuXHRcdCAgICBpc05ld1llYXIgPSBzZWxmLmN1cnJlbnRZZWFyICE9PSBuZXdZZWFyTnVtO1xuXG5cdFx0c2VsZi5jdXJyZW50WWVhciA9IG5ld1llYXJOdW0gfHwgc2VsZi5jdXJyZW50WWVhcjtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlICYmIHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSkge1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSBNYXRoLm1pbihzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1vbnRoKCksIHNlbGYuY3VycmVudE1vbnRoKTtcblx0XHR9IGVsc2UgaWYgKHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpKSB7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IE1hdGgubWF4KHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TW9udGgoKSwgc2VsZi5jdXJyZW50TW9udGgpO1xuXHRcdH1cblxuXHRcdGlmIChpc05ld1llYXIpIHtcblx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJZZWFyQ2hhbmdlXCIpO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGlzRW5hYmxlZChkYXRlLCB0aW1lbGVzcykge1xuXHRcdHZhciBsdG1pbiA9IGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLmNvbmZpZy5taW5EYXRlLCB0eXBlb2YgdGltZWxlc3MgIT09IFwidW5kZWZpbmVkXCIgPyB0aW1lbGVzcyA6ICFzZWxmLm1pbkRhdGVIYXNUaW1lKSA8IDA7XG5cdFx0dmFyIGd0bWF4ID0gY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuY29uZmlnLm1heERhdGUsIHR5cGVvZiB0aW1lbGVzcyAhPT0gXCJ1bmRlZmluZWRcIiA/IHRpbWVsZXNzIDogIXNlbGYubWF4RGF0ZUhhc1RpbWUpID4gMDtcblxuXHRcdGlmIChsdG1pbiB8fCBndG1heCkgcmV0dXJuIGZhbHNlO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGUubGVuZ3RoICYmICFzZWxmLmNvbmZpZy5kaXNhYmxlLmxlbmd0aCkgcmV0dXJuIHRydWU7XG5cblx0XHR2YXIgZGF0ZVRvQ2hlY2sgPSBzZWxmLnBhcnNlRGF0ZShkYXRlLCB0cnVlKTsgLy8gdGltZWxlc3NcblxuXHRcdHZhciBib29sID0gc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCA+IDAsXG5cdFx0ICAgIGFycmF5ID0gYm9vbCA/IHNlbGYuY29uZmlnLmVuYWJsZSA6IHNlbGYuY29uZmlnLmRpc2FibGU7XG5cblx0XHRmb3IgKHZhciBpID0gMCwgZDsgaSA8IGFycmF5Lmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRkID0gYXJyYXlbaV07XG5cblx0XHRcdGlmIChkIGluc3RhbmNlb2YgRnVuY3Rpb24gJiYgZChkYXRlVG9DaGVjaykpIC8vIGRpc2FibGVkIGJ5IGZ1bmN0aW9uXG5cdFx0XHRcdHJldHVybiBib29sO2Vsc2UgaWYgKGQgaW5zdGFuY2VvZiBEYXRlICYmIGQuZ2V0VGltZSgpID09PSBkYXRlVG9DaGVjay5nZXRUaW1lKCkpXG5cdFx0XHRcdC8vIGRpc2FibGVkIGJ5IGRhdGVcblx0XHRcdFx0cmV0dXJuIGJvb2w7ZWxzZSBpZiAodHlwZW9mIGQgPT09IFwic3RyaW5nXCIgJiYgc2VsZi5wYXJzZURhdGUoZCwgdHJ1ZSkuZ2V0VGltZSgpID09PSBkYXRlVG9DaGVjay5nZXRUaW1lKCkpXG5cdFx0XHRcdC8vIGRpc2FibGVkIGJ5IGRhdGUgc3RyaW5nXG5cdFx0XHRcdHJldHVybiBib29sO2Vsc2UgaWYgKCAvLyBkaXNhYmxlZCBieSByYW5nZVxuXHRcdFx0KHR5cGVvZiBkID09PSBcInVuZGVmaW5lZFwiID8gXCJ1bmRlZmluZWRcIiA6IF90eXBlb2YoZCkpID09PSBcIm9iamVjdFwiICYmIGQuZnJvbSAmJiBkLnRvICYmIGRhdGVUb0NoZWNrID49IGQuZnJvbSAmJiBkYXRlVG9DaGVjayA8PSBkLnRvKSByZXR1cm4gYm9vbDtcblx0XHR9XG5cblx0XHRyZXR1cm4gIWJvb2w7XG5cdH1cblxuXHRmdW5jdGlvbiBvbktleURvd24oZSkge1xuXHRcdGlmIChlLnRhcmdldCA9PT0gKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkgJiYgZS53aGljaCA9PT0gMTMpIHNlbGVjdERhdGUoZSk7ZWxzZSBpZiAoc2VsZi5pc09wZW4gfHwgc2VsZi5jb25maWcuaW5saW5lKSB7XG5cdFx0XHRzd2l0Y2ggKGUud2hpY2gpIHtcblx0XHRcdFx0Y2FzZSAxMzpcblx0XHRcdFx0XHRpZiAoc2VsZi50aW1lQ29udGFpbmVyICYmIHNlbGYudGltZUNvbnRhaW5lci5jb250YWlucyhlLnRhcmdldCkpIHVwZGF0ZVZhbHVlKCk7ZWxzZSBzZWxlY3REYXRlKGUpO1xuXG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSAyNzpcblx0XHRcdFx0XHQvLyBlc2NhcGVcblx0XHRcdFx0XHRzZWxmLmNsb3NlKCk7XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSAzNzpcblx0XHRcdFx0XHRpZiAoZS50YXJnZXQgIT09IHNlbGYuaW5wdXQgJiBlLnRhcmdldCAhPT0gc2VsZi5hbHRJbnB1dCkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0Y2hhbmdlTW9udGgoLTEpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LmZvY3VzKCk7XG5cdFx0XHRcdFx0fVxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgMzg6XG5cdFx0XHRcdFx0aWYgKCFzZWxmLnRpbWVDb250YWluZXIgfHwgIXNlbGYudGltZUNvbnRhaW5lci5jb250YWlucyhlLnRhcmdldCkpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdHNlbGYuY3VycmVudFllYXIrKztcblx0XHRcdFx0XHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0XHRcdFx0fSBlbHNlIHVwZGF0ZVRpbWUoZSk7XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIDM5OlxuXHRcdFx0XHRcdGlmIChlLnRhcmdldCAhPT0gc2VsZi5pbnB1dCAmIGUudGFyZ2V0ICE9PSBzZWxmLmFsdElucHV0KSB7XG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0XHRjaGFuZ2VNb250aCgxKTtcblx0XHRcdFx0XHRcdHNlbGYuY3VycmVudE1vbnRoRWxlbWVudC5mb2N1cygpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIDQwOlxuXHRcdFx0XHRcdGlmICghc2VsZi50aW1lQ29udGFpbmVyIHx8ICFzZWxmLnRpbWVDb250YWluZXIuY29udGFpbnMoZS50YXJnZXQpKSB7XG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0XHRzZWxmLmN1cnJlbnRZZWFyLS07XG5cdFx0XHRcdFx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdFx0XHRcdH0gZWxzZSB1cGRhdGVUaW1lKGUpO1xuXG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0ZGVmYXVsdDpcblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0fVxuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIG9uTW91c2VPdmVyKGUpIHtcblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCAhPT0gMSB8fCAhZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwiZmxhdHBpY2tyLWRheVwiKSkgcmV0dXJuO1xuXG5cdFx0dmFyIGhvdmVyRGF0ZSA9IGUudGFyZ2V0LmRhdGVPYmosXG5cdFx0ICAgIGluaXRpYWxEYXRlID0gc2VsZi5wYXJzZURhdGUoc2VsZi5zZWxlY3RlZERhdGVzWzBdLCB0cnVlKSxcblx0XHQgICAgcmFuZ2VTdGFydERhdGUgPSBNYXRoLm1pbihob3ZlckRhdGUuZ2V0VGltZSgpLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0uZ2V0VGltZSgpKSxcblx0XHQgICAgcmFuZ2VFbmREYXRlID0gTWF0aC5tYXgoaG92ZXJEYXRlLmdldFRpbWUoKSwgc2VsZi5zZWxlY3RlZERhdGVzWzBdLmdldFRpbWUoKSksXG5cdFx0ICAgIGNvbnRhaW5zRGlzYWJsZWQgPSBmYWxzZTtcblxuXHRcdGZvciAodmFyIHQgPSByYW5nZVN0YXJ0RGF0ZTsgdCA8IHJhbmdlRW5kRGF0ZTsgdCArPSBzZWxmLnV0aWxzLmR1cmF0aW9uLkRBWSkge1xuXHRcdFx0aWYgKCFpc0VuYWJsZWQobmV3IERhdGUodCkpKSB7XG5cdFx0XHRcdGNvbnRhaW5zRGlzYWJsZWQgPSB0cnVlO1xuXHRcdFx0XHRicmVhaztcblx0XHRcdH1cblx0XHR9XG5cblx0XHR2YXIgX2xvb3AgPSBmdW5jdGlvbiBfbG9vcCh0aW1lc3RhbXAsIGkpIHtcblx0XHRcdHZhciBvdXRPZlJhbmdlID0gdGltZXN0YW1wIDwgc2VsZi5taW5SYW5nZURhdGUuZ2V0VGltZSgpIHx8IHRpbWVzdGFtcCA+IHNlbGYubWF4UmFuZ2VEYXRlLmdldFRpbWUoKTtcblxuXHRcdFx0aWYgKG91dE9mUmFuZ2UpIHtcblx0XHRcdFx0c2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LmFkZChcIm5vdEFsbG93ZWRcIik7XG5cdFx0XHRcdFtcImluUmFuZ2VcIiwgXCJzdGFydFJhbmdlXCIsIFwiZW5kUmFuZ2VcIl0uZm9yRWFjaChmdW5jdGlvbiAoYykge1xuXHRcdFx0XHRcdHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5yZW1vdmUoYyk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0XHRyZXR1cm4gXCJjb250aW51ZVwiO1xuXHRcdFx0fSBlbHNlIGlmIChjb250YWluc0Rpc2FibGVkICYmICFvdXRPZlJhbmdlKSByZXR1cm4gXCJjb250aW51ZVwiO1xuXG5cdFx0XHRbXCJzdGFydFJhbmdlXCIsIFwiaW5SYW5nZVwiLCBcImVuZFJhbmdlXCIsIFwibm90QWxsb3dlZFwiXS5mb3JFYWNoKGZ1bmN0aW9uIChjKSB7XG5cdFx0XHRcdHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5yZW1vdmUoYyk7XG5cdFx0XHR9KTtcblxuXHRcdFx0dmFyIG1pblJhbmdlRGF0ZSA9IE1hdGgubWF4KHNlbGYubWluUmFuZ2VEYXRlLmdldFRpbWUoKSwgcmFuZ2VTdGFydERhdGUpLFxuXHRcdFx0ICAgIG1heFJhbmdlRGF0ZSA9IE1hdGgubWluKHNlbGYubWF4UmFuZ2VEYXRlLmdldFRpbWUoKSwgcmFuZ2VFbmREYXRlKTtcblxuXHRcdFx0ZS50YXJnZXQuY2xhc3NMaXN0LmFkZChob3ZlckRhdGUgPCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0gPyBcInN0YXJ0UmFuZ2VcIiA6IFwiZW5kUmFuZ2VcIik7XG5cblx0XHRcdGlmIChpbml0aWFsRGF0ZSA+IGhvdmVyRGF0ZSAmJiB0aW1lc3RhbXAgPT09IGluaXRpYWxEYXRlLmdldFRpbWUoKSkgc2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LmFkZChcImVuZFJhbmdlXCIpO2Vsc2UgaWYgKGluaXRpYWxEYXRlIDwgaG92ZXJEYXRlICYmIHRpbWVzdGFtcCA9PT0gaW5pdGlhbERhdGUuZ2V0VGltZSgpKSBzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QuYWRkKFwic3RhcnRSYW5nZVwiKTtlbHNlIGlmICh0aW1lc3RhbXAgPj0gbWluUmFuZ2VEYXRlICYmIHRpbWVzdGFtcCA8PSBtYXhSYW5nZURhdGUpIHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5hZGQoXCJpblJhbmdlXCIpO1xuXHRcdH07XG5cblx0XHRmb3IgKHZhciB0aW1lc3RhbXAgPSBzZWxmLmRheXMuY2hpbGROb2Rlc1swXS5kYXRlT2JqLmdldFRpbWUoKSwgaSA9IDA7IGkgPCA0MjsgaSsrLCB0aW1lc3RhbXAgKz0gc2VsZi51dGlscy5kdXJhdGlvbi5EQVkpIHtcblx0XHRcdHZhciBfcmV0ID0gX2xvb3AodGltZXN0YW1wLCBpKTtcblxuXHRcdFx0aWYgKF9yZXQgPT09IFwiY29udGludWVcIikgY29udGludWU7XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gb25SZXNpemUoKSB7XG5cdFx0aWYgKHNlbGYuaXNPcGVuICYmICFzZWxmLmNvbmZpZy5zdGF0aWMgJiYgIXNlbGYuY29uZmlnLmlubGluZSkgcG9zaXRpb25DYWxlbmRhcigpO1xuXHR9XG5cblx0ZnVuY3Rpb24gb3BlbihlKSB7XG5cdFx0aWYgKHNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdGlmIChlKSB7XG5cdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0ZS50YXJnZXQuYmx1cigpO1xuXHRcdFx0fVxuXG5cdFx0XHRzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0c2VsZi5tb2JpbGVJbnB1dC5jbGljaygpO1xuXHRcdFx0fSwgMCk7XG5cblx0XHRcdHRyaWdnZXJFdmVudChcIk9wZW5cIik7XG5cdFx0XHRyZXR1cm47XG5cdFx0fSBlbHNlIGlmIChzZWxmLmlzT3BlbiB8fCAoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5kaXNhYmxlZCB8fCBzZWxmLmNvbmZpZy5pbmxpbmUpIHJldHVybjtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcIm9wZW5cIik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLnN0YXRpYyAmJiAhc2VsZi5jb25maWcuaW5saW5lKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cblx0XHRzZWxmLmlzT3BlbiA9IHRydWU7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmFsbG93SW5wdXQpIHtcblx0XHRcdChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmJsdXIoKTtcblx0XHRcdChzZWxmLmNvbmZpZy5ub0NhbGVuZGFyID8gc2VsZi50aW1lQ29udGFpbmVyIDogc2VsZi5zZWxlY3RlZERhdGVFbGVtID8gc2VsZi5zZWxlY3RlZERhdGVFbGVtIDogc2VsZi5kYXlzKS5mb2N1cygpO1xuXHRcdH1cblxuXHRcdChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmNsYXNzTGlzdC5hZGQoXCJhY3RpdmVcIik7XG5cdFx0dHJpZ2dlckV2ZW50KFwiT3BlblwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIG1pbk1heERhdGVTZXR0ZXIodHlwZSkge1xuXHRcdHJldHVybiBmdW5jdGlvbiAoZGF0ZSkge1xuXHRcdFx0dmFyIGRhdGVPYmogPSBzZWxmLmNvbmZpZ1tcIl9cIiArIHR5cGUgKyBcIkRhdGVcIl0gPSBzZWxmLnBhcnNlRGF0ZShkYXRlKTtcblxuXHRcdFx0dmFyIGludmVyc2VEYXRlT2JqID0gc2VsZi5jb25maWdbXCJfXCIgKyAodHlwZSA9PT0gXCJtaW5cIiA/IFwibWF4XCIgOiBcIm1pblwiKSArIFwiRGF0ZVwiXTtcblx0XHRcdHZhciBpc1ZhbGlkRGF0ZSA9IGRhdGUgJiYgZGF0ZU9iaiBpbnN0YW5jZW9mIERhdGU7XG5cblx0XHRcdGlmIChpc1ZhbGlkRGF0ZSkge1xuXHRcdFx0XHRzZWxmW3R5cGUgKyBcIkRhdGVIYXNUaW1lXCJdID0gZGF0ZU9iai5nZXRIb3VycygpIHx8IGRhdGVPYmouZ2V0TWludXRlcygpIHx8IGRhdGVPYmouZ2V0U2Vjb25kcygpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzKSB7XG5cdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IHNlbGYuc2VsZWN0ZWREYXRlcy5maWx0ZXIoZnVuY3Rpb24gKGQpIHtcblx0XHRcdFx0XHRyZXR1cm4gaXNFbmFibGVkKGQpO1xuXHRcdFx0XHR9KTtcblx0XHRcdFx0aWYgKCFzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoICYmIHR5cGUgPT09IFwibWluXCIpIHNldEhvdXJzRnJvbURhdGUoZGF0ZU9iaik7XG5cdFx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLmRheXMpIHtcblx0XHRcdFx0cmVkcmF3KCk7XG5cblx0XHRcdFx0aWYgKGlzVmFsaWREYXRlKSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudFt0eXBlXSA9IGRhdGVPYmouZ2V0RnVsbFllYXIoKTtlbHNlIHNlbGYuY3VycmVudFllYXJFbGVtZW50LnJlbW92ZUF0dHJpYnV0ZSh0eXBlKTtcblxuXHRcdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5kaXNhYmxlZCA9IGludmVyc2VEYXRlT2JqICYmIGRhdGVPYmogJiYgaW52ZXJzZURhdGVPYmouZ2V0RnVsbFllYXIoKSA9PT0gZGF0ZU9iai5nZXRGdWxsWWVhcigpO1xuXHRcdFx0fVxuXHRcdH07XG5cdH1cblxuXHRmdW5jdGlvbiBwYXJzZUNvbmZpZygpIHtcblx0XHR2YXIgYm9vbE9wdHMgPSBbXCJ1dGNcIiwgXCJ3cmFwXCIsIFwid2Vla051bWJlcnNcIiwgXCJhbGxvd0lucHV0XCIsIFwiY2xpY2tPcGVuc1wiLCBcInRpbWVfMjRoclwiLCBcImVuYWJsZVRpbWVcIiwgXCJub0NhbGVuZGFyXCIsIFwiYWx0SW5wdXRcIiwgXCJzaG9ydGhhbmRDdXJyZW50TW9udGhcIiwgXCJpbmxpbmVcIiwgXCJzdGF0aWNcIiwgXCJlbmFibGVTZWNvbmRzXCIsIFwiZGlzYWJsZU1vYmlsZVwiXTtcblxuXHRcdHZhciBob29rcyA9IFtcIm9uQ2hhbmdlXCIsIFwib25DbG9zZVwiLCBcIm9uRGF5Q3JlYXRlXCIsIFwib25Nb250aENoYW5nZVwiLCBcIm9uT3BlblwiLCBcIm9uUGFyc2VDb25maWdcIiwgXCJvblJlYWR5XCIsIFwib25WYWx1ZVVwZGF0ZVwiLCBcIm9uWWVhckNoYW5nZVwiXTtcblxuXHRcdHNlbGYuY29uZmlnID0gT2JqZWN0LmNyZWF0ZShGbGF0cGlja3IuZGVmYXVsdENvbmZpZyk7XG5cblx0XHR2YXIgdXNlckNvbmZpZyA9IF9leHRlbmRzKHt9LCBzZWxmLmluc3RhbmNlQ29uZmlnLCBKU09OLnBhcnNlKEpTT04uc3RyaW5naWZ5KHNlbGYuZWxlbWVudC5kYXRhc2V0IHx8IHt9KSkpO1xuXG5cdFx0c2VsZi5jb25maWcucGFyc2VEYXRlID0gdXNlckNvbmZpZy5wYXJzZURhdGU7XG5cdFx0c2VsZi5jb25maWcuZm9ybWF0RGF0ZSA9IHVzZXJDb25maWcuZm9ybWF0RGF0ZTtcblxuXHRcdF9leHRlbmRzKHNlbGYuY29uZmlnLCB1c2VyQ29uZmlnKTtcblxuXHRcdGlmICghdXNlckNvbmZpZy5kYXRlRm9ybWF0ICYmIHVzZXJDb25maWcuZW5hYmxlVGltZSkge1xuXHRcdFx0c2VsZi5jb25maWcuZGF0ZUZvcm1hdCA9IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgPyBcIkg6aVwiICsgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBcIjpTXCIgOiBcIlwiKSA6IEZsYXRwaWNrci5kZWZhdWx0Q29uZmlnLmRhdGVGb3JtYXQgKyBcIiBIOmlcIiArIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gXCI6U1wiIDogXCJcIik7XG5cdFx0fVxuXG5cdFx0aWYgKHVzZXJDb25maWcuYWx0SW5wdXQgJiYgdXNlckNvbmZpZy5lbmFibGVUaW1lICYmICF1c2VyQ29uZmlnLmFsdEZvcm1hdCkge1xuXHRcdFx0c2VsZi5jb25maWcuYWx0Rm9ybWF0ID0gc2VsZi5jb25maWcubm9DYWxlbmRhciA/IFwiaDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlMgS1wiIDogXCIgS1wiKSA6IEZsYXRwaWNrci5kZWZhdWx0Q29uZmlnLmFsdEZvcm1hdCArIChcIiBoOmlcIiArIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gXCI6U1wiIDogXCJcIikgKyBcIiBLXCIpO1xuXHRcdH1cblxuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLmNvbmZpZywgXCJtaW5EYXRlXCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5fbWluRGF0ZTtcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IG1pbk1heERhdGVTZXR0ZXIoXCJtaW5cIilcblx0XHR9KTtcblxuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLmNvbmZpZywgXCJtYXhEYXRlXCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5fbWF4RGF0ZTtcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IG1pbk1heERhdGVTZXR0ZXIoXCJtYXhcIilcblx0XHR9KTtcblxuXHRcdHNlbGYuY29uZmlnLm1pbkRhdGUgPSB1c2VyQ29uZmlnLm1pbkRhdGU7XG5cdFx0c2VsZi5jb25maWcubWF4RGF0ZSA9IHVzZXJDb25maWcubWF4RGF0ZTtcblxuXHRcdGZvciAodmFyIGkgPSAwOyBpIDwgYm9vbE9wdHMubGVuZ3RoOyBpKyspIHtcblx0XHRcdHNlbGYuY29uZmlnW2Jvb2xPcHRzW2ldXSA9IHNlbGYuY29uZmlnW2Jvb2xPcHRzW2ldXSA9PT0gdHJ1ZSB8fCBzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPT09IFwidHJ1ZVwiO1xuXHRcdH1mb3IgKHZhciBfaSA9IDA7IF9pIDwgaG9va3MubGVuZ3RoOyBfaSsrKSB7XG5cdFx0XHRzZWxmLmNvbmZpZ1tob29rc1tfaV1dID0gYXJyYXlpZnkoc2VsZi5jb25maWdbaG9va3NbX2ldXSB8fCBbXSkubWFwKGJpbmRUb0luc3RhbmNlKTtcblx0XHR9Zm9yICh2YXIgX2kyID0gMDsgX2kyIDwgc2VsZi5jb25maWcucGx1Z2lucy5sZW5ndGg7IF9pMisrKSB7XG5cdFx0XHR2YXIgcGx1Z2luQ29uZiA9IHNlbGYuY29uZmlnLnBsdWdpbnNbX2kyXShzZWxmKSB8fCB7fTtcblx0XHRcdGZvciAodmFyIGtleSBpbiBwbHVnaW5Db25mKSB7XG5cdFx0XHRcdGlmIChBcnJheS5pc0FycmF5KHNlbGYuY29uZmlnW2tleV0pKSBzZWxmLmNvbmZpZ1trZXldID0gYXJyYXlpZnkocGx1Z2luQ29uZltrZXldKS5tYXAoYmluZFRvSW5zdGFuY2UpLmNvbmNhdChzZWxmLmNvbmZpZ1trZXldKTtlbHNlIGlmICh0eXBlb2YgdXNlckNvbmZpZ1trZXldID09PSBcInVuZGVmaW5lZFwiKSBzZWxmLmNvbmZpZ1trZXldID0gcGx1Z2luQ29uZltrZXldO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIlBhcnNlQ29uZmlnXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0dXBMb2NhbGUoKSB7XG5cdFx0aWYgKF90eXBlb2Yoc2VsZi5jb25maWcubG9jYWxlKSAhPT0gXCJvYmplY3RcIiAmJiB0eXBlb2YgRmxhdHBpY2tyLmwxMG5zW3NlbGYuY29uZmlnLmxvY2FsZV0gPT09IFwidW5kZWZpbmVkXCIpIGNvbnNvbGUud2FybihcImZsYXRwaWNrcjogaW52YWxpZCBsb2NhbGUgXCIgKyBzZWxmLmNvbmZpZy5sb2NhbGUpO1xuXG5cdFx0c2VsZi5sMTBuID0gX2V4dGVuZHMoT2JqZWN0LmNyZWF0ZShGbGF0cGlja3IubDEwbnMuZGVmYXVsdCksIF90eXBlb2Yoc2VsZi5jb25maWcubG9jYWxlKSA9PT0gXCJvYmplY3RcIiA/IHNlbGYuY29uZmlnLmxvY2FsZSA6IHNlbGYuY29uZmlnLmxvY2FsZSAhPT0gXCJkZWZhdWx0XCIgPyBGbGF0cGlja3IubDEwbnNbc2VsZi5jb25maWcubG9jYWxlXSB8fCB7fSA6IHt9KTtcblx0fVxuXG5cdGZ1bmN0aW9uIHBvc2l0aW9uQ2FsZW5kYXIoZSkge1xuXHRcdGlmIChlICYmIGUudGFyZ2V0ICE9PSBzZWxmLnRpbWVDb250YWluZXIpIHJldHVybjtcblxuXHRcdHZhciBjYWxlbmRhckhlaWdodCA9IHNlbGYuY2FsZW5kYXJDb250YWluZXIub2Zmc2V0SGVpZ2h0LFxuXHRcdCAgICBjYWxlbmRhcldpZHRoID0gc2VsZi5jYWxlbmRhckNvbnRhaW5lci5vZmZzZXRXaWR0aCxcblx0XHQgICAgY29uZmlnUG9zID0gc2VsZi5jb25maWcucG9zaXRpb24sXG5cdFx0ICAgIGlucHV0ID0gc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0LFxuXHRcdCAgICBpbnB1dEJvdW5kcyA9IGlucHV0LmdldEJvdW5kaW5nQ2xpZW50UmVjdCgpLFxuXHRcdCAgICBkaXN0YW5jZUZyb21Cb3R0b20gPSB3aW5kb3cuaW5uZXJIZWlnaHQgLSBpbnB1dEJvdW5kcy5ib3R0b20gKyBpbnB1dC5vZmZzZXRIZWlnaHQsXG5cdFx0ICAgIHNob3dPblRvcCA9IGNvbmZpZ1BvcyA9PT0gXCJhYm92ZVwiIHx8IGNvbmZpZ1BvcyAhPT0gXCJiZWxvd1wiICYmIGRpc3RhbmNlRnJvbUJvdHRvbSA8IGNhbGVuZGFySGVpZ2h0ICsgNjA7XG5cblx0XHR2YXIgdG9wID0gd2luZG93LnBhZ2VZT2Zmc2V0ICsgaW5wdXRCb3VuZHMudG9wICsgKCFzaG93T25Ub3AgPyBpbnB1dC5vZmZzZXRIZWlnaHQgKyAyIDogLWNhbGVuZGFySGVpZ2h0IC0gMik7XG5cblx0XHR0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcImFycm93VG9wXCIsICFzaG93T25Ub3ApO1xuXHRcdHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwiYXJyb3dCb3R0b21cIiwgc2hvd09uVG9wKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUpIHJldHVybjtcblxuXHRcdHZhciBsZWZ0ID0gd2luZG93LnBhZ2VYT2Zmc2V0ICsgaW5wdXRCb3VuZHMubGVmdDtcblx0XHR2YXIgcmlnaHQgPSB3aW5kb3cuZG9jdW1lbnQuYm9keS5vZmZzZXRXaWR0aCAtIGlucHV0Qm91bmRzLnJpZ2h0O1xuXHRcdHZhciByaWdodE1vc3QgPSBsZWZ0ICsgY2FsZW5kYXJXaWR0aCA+IHdpbmRvdy5kb2N1bWVudC5ib2R5Lm9mZnNldFdpZHRoO1xuXG5cdFx0dG9nZ2xlQ2xhc3Moc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgXCJyaWdodE1vc3RcIiwgcmlnaHRNb3N0KTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5zdGF0aWMpIHJldHVybjtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUudG9wID0gdG9wICsgXCJweFwiO1xuXG5cdFx0aWYgKCFyaWdodE1vc3QpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUubGVmdCA9IGxlZnQgKyBcInB4XCI7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLnJpZ2h0ID0gXCJhdXRvXCI7XG5cdFx0fSBlbHNlIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUubGVmdCA9IFwiYXV0b1wiO1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS5yaWdodCA9IHJpZ2h0ICsgXCJweFwiO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHJlZHJhdygpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhciB8fCBzZWxmLmlzTW9iaWxlKSByZXR1cm47XG5cblx0XHRidWlsZFdlZWtkYXlzKCk7XG5cdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXHRcdGJ1aWxkRGF5cygpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2VsZWN0RGF0ZShlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdGUuc3RvcFByb3BhZ2F0aW9uKCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuYWxsb3dJbnB1dCAmJiBlLndoaWNoID09PSAxMyAmJiBlLnRhcmdldCA9PT0gKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkpIHtcblx0XHRcdHNlbGYuc2V0RGF0ZSgoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS52YWx1ZSwgdHJ1ZSwgZS50YXJnZXQgPT09IHNlbGYuYWx0SW5wdXQgPyBzZWxmLmNvbmZpZy5hbHRGb3JtYXQgOiBzZWxmLmNvbmZpZy5kYXRlRm9ybWF0KTtcblx0XHRcdHJldHVybiBlLnRhcmdldC5ibHVyKCk7XG5cdFx0fVxuXG5cdFx0aWYgKCFlLnRhcmdldC5jbGFzc0xpc3QuY29udGFpbnMoXCJmbGF0cGlja3ItZGF5XCIpIHx8IGUudGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcImRpc2FibGVkXCIpIHx8IGUudGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcIm5vdEFsbG93ZWRcIikpIHJldHVybjtcblxuXHRcdHZhciBzZWxlY3RlZERhdGUgPSBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IG5ldyBEYXRlKGUudGFyZ2V0LmRhdGVPYmouZ2V0VGltZSgpKTtcblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlRWxlbSA9IGUudGFyZ2V0O1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwic2luZ2xlXCIpIHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtzZWxlY3RlZERhdGVdO2Vsc2UgaWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwibXVsdGlwbGVcIikge1xuXHRcdFx0dmFyIHNlbGVjdGVkSW5kZXggPSBpc0RhdGVTZWxlY3RlZChzZWxlY3RlZERhdGUpO1xuXHRcdFx0aWYgKHNlbGVjdGVkSW5kZXgpIHNlbGYuc2VsZWN0ZWREYXRlcy5zcGxpY2Uoc2VsZWN0ZWRJbmRleCwgMSk7ZWxzZSBzZWxmLnNlbGVjdGVkRGF0ZXMucHVzaChzZWxlY3RlZERhdGUpO1xuXHRcdH0gZWxzZSBpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiKSB7XG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMikgc2VsZi5jbGVhcigpO1xuXG5cdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMucHVzaChzZWxlY3RlZERhdGUpO1xuXG5cdFx0XHQvLyB1bmxlc3Mgc2VsZWN0aW5nIHNhbWUgZGF0ZSB0d2ljZSwgc29ydCBhc2NlbmRpbmdseVxuXHRcdFx0aWYgKGNvbXBhcmVEYXRlcyhzZWxlY3RlZERhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1swXSwgdHJ1ZSkgIT09IDApIHNlbGYuc2VsZWN0ZWREYXRlcy5zb3J0KGZ1bmN0aW9uIChhLCBiKSB7XG5cdFx0XHRcdHJldHVybiBhLmdldFRpbWUoKSAtIGIuZ2V0VGltZSgpO1xuXHRcdFx0fSk7XG5cdFx0fVxuXG5cdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cblx0XHRpZiAoc2VsZWN0ZWREYXRlLmdldE1vbnRoKCkgIT09IHNlbGYuY3VycmVudE1vbnRoICYmIHNlbGYuY29uZmlnLm1vZGUgIT09IFwicmFuZ2VcIikge1xuXHRcdFx0dmFyIGlzTmV3WWVhciA9IHNlbGYuY3VycmVudFllYXIgIT09IHNlbGVjdGVkRGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhciA9IHNlbGVjdGVkRGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSBzZWxlY3RlZERhdGUuZ2V0TW9udGgoKTtcblxuXHRcdFx0aWYgKGlzTmV3WWVhcikgdHJpZ2dlckV2ZW50KFwiWWVhckNoYW5nZVwiKTtcblxuXHRcdFx0dHJpZ2dlckV2ZW50KFwiTW9udGhDaGFuZ2VcIik7XG5cdFx0fVxuXG5cdFx0YnVpbGREYXlzKCk7XG5cblx0XHRpZiAoc2VsZi5taW5EYXRlSGFzVGltZSAmJiBzZWxmLmNvbmZpZy5lbmFibGVUaW1lICYmIGNvbXBhcmVEYXRlcyhzZWxlY3RlZERhdGUsIHNlbGYuY29uZmlnLm1pbkRhdGUpID09PSAwKSBzZXRIb3Vyc0Zyb21EYXRlKHNlbGYuY29uZmlnLm1pbkRhdGUpO1xuXG5cdFx0dXBkYXRlVmFsdWUoKTtcblxuXHRcdHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0cmV0dXJuIHNlbGYuc2hvd1RpbWVJbnB1dCA9IHRydWU7XG5cdFx0fSwgNTApO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIikge1xuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEpIHtcblx0XHRcdFx0b25Nb3VzZU92ZXIoZSk7XG5cblx0XHRcdFx0c2VsZi5faGlkZVByZXZNb250aEFycm93ID0gc2VsZi5faGlkZVByZXZNb250aEFycm93IHx8IHNlbGYubWluUmFuZ2VEYXRlID4gc2VsZi5kYXlzLmNoaWxkTm9kZXNbMF0uZGF0ZU9iajtcblxuXHRcdFx0XHRzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgfHwgc2VsZi5tYXhSYW5nZURhdGUgPCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsIDEpO1xuXHRcdFx0fSBlbHNlIHtcblx0XHRcdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXHRcdFx0XHRzZWxmLmNsb3NlKCk7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0aWYgKGUud2hpY2ggPT09IDEzICYmIHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xuXHRcdFx0cmV0dXJuIHNlbGYuaG91ckVsZW1lbnQuZm9jdXMoKTtcblx0XHR9LCA0NTEpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwic2luZ2xlXCIgJiYgIXNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIHNlbGYuY2xvc2UoKTtcblxuXHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldChvcHRpb24sIHZhbHVlKSB7XG5cdFx0c2VsZi5jb25maWdbb3B0aW9uXSA9IHZhbHVlO1xuXHRcdHNlbGYucmVkcmF3KCk7XG5cdFx0anVtcFRvRGF0ZSgpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0U2VsZWN0ZWREYXRlKGlucHV0RGF0ZSwgZm9ybWF0KSB7XG5cdFx0aWYgKEFycmF5LmlzQXJyYXkoaW5wdXREYXRlKSkgc2VsZi5zZWxlY3RlZERhdGVzID0gaW5wdXREYXRlLm1hcChmdW5jdGlvbiAoZCkge1xuXHRcdFx0cmV0dXJuIHNlbGYucGFyc2VEYXRlKGQsIGZhbHNlLCBmb3JtYXQpO1xuXHRcdH0pO2Vsc2UgaWYgKGlucHV0RGF0ZSBpbnN0YW5jZW9mIERhdGUgfHwgIWlzTmFOKGlucHV0RGF0ZSkpIHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtzZWxmLnBhcnNlRGF0ZShpbnB1dERhdGUpXTtlbHNlIGlmIChpbnB1dERhdGUgJiYgaW5wdXREYXRlLnN1YnN0cmluZykge1xuXHRcdFx0c3dpdGNoIChzZWxmLmNvbmZpZy5tb2RlKSB7XG5cdFx0XHRcdGNhc2UgXCJzaW5nbGVcIjpcblx0XHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZi5wYXJzZURhdGUoaW5wdXREYXRlLCBmYWxzZSwgZm9ybWF0KV07XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSBcIm11bHRpcGxlXCI6XG5cdFx0XHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gaW5wdXREYXRlLnNwbGl0KFwiOyBcIikubWFwKGZ1bmN0aW9uIChkYXRlKSB7XG5cdFx0XHRcdFx0XHRyZXR1cm4gc2VsZi5wYXJzZURhdGUoZGF0ZSwgZmFsc2UsIGZvcm1hdCk7XG5cdFx0XHRcdFx0fSk7XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSBcInJhbmdlXCI6XG5cdFx0XHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gaW5wdXREYXRlLnNwbGl0KHNlbGYubDEwbi5yYW5nZVNlcGFyYXRvcikubWFwKGZ1bmN0aW9uIChkYXRlKSB7XG5cdFx0XHRcdFx0XHRyZXR1cm4gc2VsZi5wYXJzZURhdGUoZGF0ZSwgZmFsc2UsIGZvcm1hdCk7XG5cdFx0XHRcdFx0fSk7XG5cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRkZWZhdWx0OlxuXHRcdFx0XHRcdGJyZWFrO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IHNlbGYuc2VsZWN0ZWREYXRlcy5maWx0ZXIoZnVuY3Rpb24gKGQpIHtcblx0XHRcdHJldHVybiBkIGluc3RhbmNlb2YgRGF0ZSAmJiBkLmdldFRpbWUoKSAmJiBpc0VuYWJsZWQoZCwgZmFsc2UpO1xuXHRcdH0pO1xuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVzLnNvcnQoZnVuY3Rpb24gKGEsIGIpIHtcblx0XHRcdHJldHVybiBhLmdldFRpbWUoKSAtIGIuZ2V0VGltZSgpO1xuXHRcdH0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0RGF0ZShkYXRlLCB0cmlnZ2VyQ2hhbmdlLCBmb3JtYXQpIHtcblx0XHRpZiAoIWRhdGUpIHJldHVybiBzZWxmLmNsZWFyKCk7XG5cblx0XHRzZXRTZWxlY3RlZERhdGUoZGF0ZSwgZm9ybWF0KTtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID4gMCkge1xuXHRcdFx0c2VsZi5zaG93VGltZUlucHV0ID0gdHJ1ZTtcblx0XHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gc2VsZi5zZWxlY3RlZERhdGVzWzBdO1xuXHRcdH0gZWxzZSBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IG51bGw7XG5cblx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdGp1bXBUb0RhdGUoKTtcblxuXHRcdHNldEhvdXJzRnJvbURhdGUoKTtcblx0XHR1cGRhdGVWYWx1ZSgpO1xuXG5cdFx0aWYgKHRyaWdnZXJDaGFuZ2UgIT09IGZhbHNlKSB0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cERhdGVzKCkge1xuXHRcdGZ1bmN0aW9uIHBhcnNlRGF0ZVJ1bGVzKGFycikge1xuXHRcdFx0Zm9yICh2YXIgaSA9IGFyci5sZW5ndGg7IGktLTspIHtcblx0XHRcdFx0aWYgKHR5cGVvZiBhcnJbaV0gPT09IFwic3RyaW5nXCIgfHwgK2FycltpXSkgYXJyW2ldID0gc2VsZi5wYXJzZURhdGUoYXJyW2ldLCB0cnVlKTtlbHNlIGlmIChhcnJbaV0gJiYgYXJyW2ldLmZyb20gJiYgYXJyW2ldLnRvKSB7XG5cdFx0XHRcdFx0YXJyW2ldLmZyb20gPSBzZWxmLnBhcnNlRGF0ZShhcnJbaV0uZnJvbSk7XG5cdFx0XHRcdFx0YXJyW2ldLnRvID0gc2VsZi5wYXJzZURhdGUoYXJyW2ldLnRvKTtcblx0XHRcdFx0fVxuXHRcdFx0fVxuXG5cdFx0XHRyZXR1cm4gYXJyLmZpbHRlcihmdW5jdGlvbiAoeCkge1xuXHRcdFx0XHRyZXR1cm4geDtcblx0XHRcdH0pOyAvLyByZW1vdmUgZmFsc3kgdmFsdWVzXG5cdFx0fVxuXG5cdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gW107XG5cdFx0c2VsZi5ub3cgPSBuZXcgRGF0ZSgpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmRpc2FibGUubGVuZ3RoKSBzZWxmLmNvbmZpZy5kaXNhYmxlID0gcGFyc2VEYXRlUnVsZXMoc2VsZi5jb25maWcuZGlzYWJsZSk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCkgc2VsZi5jb25maWcuZW5hYmxlID0gcGFyc2VEYXRlUnVsZXMoc2VsZi5jb25maWcuZW5hYmxlKTtcblxuXHRcdHNldFNlbGVjdGVkRGF0ZShzZWxmLmNvbmZpZy5kZWZhdWx0RGF0ZSB8fCBzZWxmLmlucHV0LnZhbHVlKTtcblxuXHRcdHZhciBpbml0aWFsRGF0ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPyBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0gOiBzZWxmLmNvbmZpZy5taW5EYXRlICYmIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0VGltZSgpID4gc2VsZi5ub3cgPyBzZWxmLmNvbmZpZy5taW5EYXRlIDogc2VsZi5jb25maWcubWF4RGF0ZSAmJiBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldFRpbWUoKSA8IHNlbGYubm93ID8gc2VsZi5jb25maWcubWF4RGF0ZSA6IHNlbGYubm93O1xuXG5cdFx0c2VsZi5jdXJyZW50WWVhciA9IGluaXRpYWxEYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0c2VsZi5jdXJyZW50TW9udGggPSBpbml0aWFsRGF0ZS5nZXRNb250aCgpO1xuXG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gc2VsZi5zZWxlY3RlZERhdGVzWzBdO1xuXG5cdFx0c2VsZi5taW5EYXRlSGFzVGltZSA9IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgKHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSB8fCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldE1pbnV0ZXMoKSB8fCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldFNlY29uZHMoKSk7XG5cblx0XHRzZWxmLm1heERhdGVIYXNUaW1lID0gc2VsZi5jb25maWcubWF4RGF0ZSAmJiAoc2VsZi5jb25maWcubWF4RGF0ZS5nZXRIb3VycygpIHx8IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0TWludXRlcygpIHx8IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0U2Vjb25kcygpKTtcblxuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcImxhdGVzdFNlbGVjdGVkRGF0ZU9ialwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHNlbGYuX3NlbGVjdGVkRGF0ZU9iaiB8fCBzZWxmLnNlbGVjdGVkRGF0ZXNbc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCAtIDFdIHx8IG51bGw7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoZGF0ZSkge1xuXHRcdFx0XHRzZWxmLl9zZWxlY3RlZERhdGVPYmogPSBkYXRlO1xuXHRcdFx0fVxuXHRcdH0pO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSB7XG5cdFx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZiwgXCJzaG93VGltZUlucHV0XCIsIHtcblx0XHRcdFx0c2V0OiBmdW5jdGlvbiBzZXQoYm9vbCkge1xuXHRcdFx0XHRcdGlmIChzZWxmLmNhbGVuZGFyQ29udGFpbmVyKSB0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcInNob3dUaW1lSW5wdXRcIiwgYm9vbCk7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwSGVscGVyRnVuY3Rpb25zKCkge1xuXHRcdHNlbGYudXRpbHMgPSB7XG5cdFx0XHRkdXJhdGlvbjoge1xuXHRcdFx0XHREQVk6IDg2NDAwMDAwXG5cdFx0XHR9LFxuXHRcdFx0Z2V0RGF5c2luTW9udGg6IGZ1bmN0aW9uIGdldERheXNpbk1vbnRoKG1vbnRoLCB5cikge1xuXHRcdFx0XHRtb250aCA9IHR5cGVvZiBtb250aCA9PT0gXCJ1bmRlZmluZWRcIiA/IHNlbGYuY3VycmVudE1vbnRoIDogbW9udGg7XG5cblx0XHRcdFx0eXIgPSB0eXBlb2YgeXIgPT09IFwidW5kZWZpbmVkXCIgPyBzZWxmLmN1cnJlbnRZZWFyIDogeXI7XG5cblx0XHRcdFx0aWYgKG1vbnRoID09PSAxICYmICh5ciAlIDQgPT09IDAgJiYgeXIgJSAxMDAgIT09IDAgfHwgeXIgJSA0MDAgPT09IDApKSByZXR1cm4gMjk7XG5cblx0XHRcdFx0cmV0dXJuIHNlbGYubDEwbi5kYXlzSW5Nb250aFttb250aF07XG5cdFx0XHR9LFxuXHRcdFx0bW9udGhUb1N0cjogZnVuY3Rpb24gbW9udGhUb1N0cihtb250aE51bWJlciwgc2hvcnRoYW5kKSB7XG5cdFx0XHRcdHNob3J0aGFuZCA9IHR5cGVvZiBzaG9ydGhhbmQgPT09IFwidW5kZWZpbmVkXCIgPyBzZWxmLmNvbmZpZy5zaG9ydGhhbmRDdXJyZW50TW9udGggOiBzaG9ydGhhbmQ7XG5cblx0XHRcdFx0cmV0dXJuIHNlbGYubDEwbi5tb250aHNbKHNob3J0aGFuZCA/IFwic2hvcnRcIiA6IFwibG9uZ1wiKSArIFwiaGFuZFwiXVttb250aE51bWJlcl07XG5cdFx0XHR9XG5cdFx0fTtcblx0fVxuXG5cdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdGZ1bmN0aW9uIHNldHVwRm9ybWF0cygpIHtcblx0XHRbXCJEXCIsIFwiRlwiLCBcIkpcIiwgXCJNXCIsIFwiV1wiLCBcImxcIl0uZm9yRWFjaChmdW5jdGlvbiAoZikge1xuXHRcdFx0c2VsZi5mb3JtYXRzW2ZdID0gRmxhdHBpY2tyLnByb3RvdHlwZS5mb3JtYXRzW2ZdLmJpbmQoc2VsZik7XG5cdFx0fSk7XG5cblx0XHRzZWxmLnJldkZvcm1hdC5GID0gRmxhdHBpY2tyLnByb3RvdHlwZS5yZXZGb3JtYXQuRi5iaW5kKHNlbGYpO1xuXHRcdHNlbGYucmV2Rm9ybWF0Lk0gPSBGbGF0cGlja3IucHJvdG90eXBlLnJldkZvcm1hdC5NLmJpbmQoc2VsZik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cElucHV0cygpIHtcblx0XHRzZWxmLmlucHV0ID0gc2VsZi5jb25maWcud3JhcCA/IHNlbGYuZWxlbWVudC5xdWVyeVNlbGVjdG9yKFwiW2RhdGEtaW5wdXRdXCIpIDogc2VsZi5lbGVtZW50O1xuXG5cdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRpZiAoIXNlbGYuaW5wdXQpIHJldHVybiBjb25zb2xlLndhcm4oXCJFcnJvcjogaW52YWxpZCBpbnB1dCBlbGVtZW50IHNwZWNpZmllZFwiLCBzZWxmLmlucHV0KTtcblxuXHRcdHNlbGYuaW5wdXQuX3R5cGUgPSBzZWxmLmlucHV0LnR5cGU7XG5cdFx0c2VsZi5pbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0c2VsZi5pbnB1dC5jbGFzc0xpc3QuYWRkKFwiZmxhdHBpY2tyLWlucHV0XCIpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsdElucHV0KSB7XG5cdFx0XHQvLyByZXBsaWNhdGUgc2VsZi5lbGVtZW50XG5cdFx0XHRzZWxmLmFsdElucHV0ID0gY3JlYXRlRWxlbWVudChzZWxmLmlucHV0Lm5vZGVOYW1lLCBzZWxmLmlucHV0LmNsYXNzTmFtZSArIFwiIFwiICsgc2VsZi5jb25maWcuYWx0SW5wdXRDbGFzcyk7XG5cdFx0XHRzZWxmLmFsdElucHV0LnBsYWNlaG9sZGVyID0gc2VsZi5pbnB1dC5wbGFjZWhvbGRlcjtcblx0XHRcdHNlbGYuYWx0SW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXHRcdFx0c2VsZi5pbnB1dC50eXBlID0gXCJoaWRkZW5cIjtcblxuXHRcdFx0aWYgKCFzZWxmLmNvbmZpZy5zdGF0aWMgJiYgc2VsZi5pbnB1dC5wYXJlbnROb2RlKSBzZWxmLmlucHV0LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYuYWx0SW5wdXQsIHNlbGYuaW5wdXQubmV4dFNpYmxpbmcpO1xuXHRcdH1cblxuXHRcdGlmICghc2VsZi5jb25maWcuYWxsb3dJbnB1dCkgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuc2V0QXR0cmlidXRlKFwicmVhZG9ubHlcIiwgXCJyZWFkb25seVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwTW9iaWxlKCkge1xuXHRcdHZhciBpbnB1dFR5cGUgPSBzZWxmLmNvbmZpZy5lbmFibGVUaW1lID8gc2VsZi5jb25maWcubm9DYWxlbmRhciA/IFwidGltZVwiIDogXCJkYXRldGltZS1sb2NhbFwiIDogXCJkYXRlXCI7XG5cblx0XHRzZWxmLm1vYmlsZUlucHV0ID0gY3JlYXRlRWxlbWVudChcImlucHV0XCIsIHNlbGYuaW5wdXQuY2xhc3NOYW1lICsgXCIgZmxhdHBpY2tyLW1vYmlsZVwiKTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnN0ZXAgPSBcImFueVwiO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQudGFiSW5kZXggPSAxO1xuXHRcdHNlbGYubW9iaWxlSW5wdXQudHlwZSA9IGlucHV0VHlwZTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LmRpc2FibGVkID0gc2VsZi5pbnB1dC5kaXNhYmxlZDtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnBsYWNlaG9sZGVyID0gc2VsZi5pbnB1dC5wbGFjZWhvbGRlcjtcblxuXHRcdHNlbGYubW9iaWxlRm9ybWF0U3RyID0gaW5wdXRUeXBlID09PSBcImRhdGV0aW1lLWxvY2FsXCIgPyBcIlktbS1kXFxcXFRIOmk6U1wiIDogaW5wdXRUeXBlID09PSBcImRhdGVcIiA/IFwiWS1tLWRcIiA6IFwiSDppOlNcIjtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSB7XG5cdFx0XHRzZWxmLm1vYmlsZUlucHV0LmRlZmF1bHRWYWx1ZSA9IHNlbGYubW9iaWxlSW5wdXQudmFsdWUgPSBmb3JtYXREYXRlKHNlbGYubW9iaWxlRm9ybWF0U3RyLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5taW5EYXRlKSBzZWxmLm1vYmlsZUlucHV0Lm1pbiA9IGZvcm1hdERhdGUoXCJZLW0tZFwiLCBzZWxmLmNvbmZpZy5taW5EYXRlKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tYXhEYXRlKSBzZWxmLm1vYmlsZUlucHV0Lm1heCA9IGZvcm1hdERhdGUoXCJZLW0tZFwiLCBzZWxmLmNvbmZpZy5tYXhEYXRlKTtcblxuXHRcdHNlbGYuaW5wdXQudHlwZSA9IFwiaGlkZGVuXCI7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsdElucHV0KSBzZWxmLmFsdElucHV0LnR5cGUgPSBcImhpZGRlblwiO1xuXG5cdFx0dHJ5IHtcblx0XHRcdHNlbGYuaW5wdXQucGFyZW50Tm9kZS5pbnNlcnRCZWZvcmUoc2VsZi5tb2JpbGVJbnB1dCwgc2VsZi5pbnB1dC5uZXh0U2libGluZyk7XG5cdFx0fSBjYXRjaCAoZSkge1xuXHRcdFx0Ly9cblx0XHR9XG5cblx0XHRzZWxmLm1vYmlsZUlucHV0LmFkZEV2ZW50TGlzdGVuZXIoXCJjaGFuZ2VcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID0gc2VsZi5wYXJzZURhdGUoZS50YXJnZXQudmFsdWUpO1xuXHRcdFx0c2VsZi5zZXREYXRlKHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNsb3NlXCIpO1xuXHRcdH0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gdG9nZ2xlKCkge1xuXHRcdGlmIChzZWxmLmlzT3Blbikgc2VsZi5jbG9zZSgpO2Vsc2Ugc2VsZi5vcGVuKCk7XG5cdH1cblxuXHRmdW5jdGlvbiB0cmlnZ2VyRXZlbnQoZXZlbnQsIGRhdGEpIHtcblx0XHR2YXIgaG9va3MgPSBzZWxmLmNvbmZpZ1tcIm9uXCIgKyBldmVudF07XG5cblx0XHRpZiAoaG9va3MpIHtcblx0XHRcdGZvciAodmFyIGkgPSAwOyBpIDwgaG9va3MubGVuZ3RoOyBpKyspIHtcblx0XHRcdFx0aG9va3NbaV0oc2VsZi5zZWxlY3RlZERhdGVzLCBzZWxmLmlucHV0ICYmIHNlbGYuaW5wdXQudmFsdWUsIHNlbGYsIGRhdGEpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdGlmIChldmVudCA9PT0gXCJDaGFuZ2VcIikge1xuXHRcdFx0aWYgKHR5cGVvZiBFdmVudCA9PT0gXCJmdW5jdGlvblwiICYmIEV2ZW50LmNvbnN0cnVjdG9yKSB7XG5cdFx0XHRcdHNlbGYuaW5wdXQuZGlzcGF0Y2hFdmVudChuZXcgRXZlbnQoXCJjaGFuZ2VcIiwgeyBcImJ1YmJsZXNcIjogdHJ1ZSB9KSk7XG5cblx0XHRcdFx0Ly8gbWFueSBmcm9udC1lbmQgZnJhbWV3b3JrcyBiaW5kIHRvIHRoZSBpbnB1dCBldmVudFxuXHRcdFx0XHRzZWxmLmlucHV0LmRpc3BhdGNoRXZlbnQobmV3IEV2ZW50KFwiaW5wdXRcIiwgeyBcImJ1YmJsZXNcIjogdHJ1ZSB9KSk7XG5cdFx0XHR9XG5cblx0XHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0XHRlbHNlIHtcblx0XHRcdFx0XHRpZiAod2luZG93LmRvY3VtZW50LmNyZWF0ZUV2ZW50ICE9PSB1bmRlZmluZWQpIHJldHVybiBzZWxmLmlucHV0LmRpc3BhdGNoRXZlbnQoc2VsZi5jaGFuZ2VFdmVudCk7XG5cblx0XHRcdFx0XHRzZWxmLmlucHV0LmZpcmVFdmVudChcIm9uY2hhbmdlXCIpO1xuXHRcdFx0XHR9XG5cdFx0fVxuXHR9XG5cblx0ZnVuY3Rpb24gaXNEYXRlU2VsZWN0ZWQoZGF0ZSkge1xuXHRcdGZvciAodmFyIGkgPSAwOyBpIDwgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRpZiAoY29tcGFyZURhdGVzKHNlbGYuc2VsZWN0ZWREYXRlc1tpXSwgZGF0ZSkgPT09IDApIHJldHVybiBcIlwiICsgaTtcblx0XHR9XG5cblx0XHRyZXR1cm4gZmFsc2U7XG5cdH1cblxuXHRmdW5jdGlvbiBpc0RhdGVJblJhbmdlKGRhdGUpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubW9kZSAhPT0gXCJyYW5nZVwiIHx8IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPCAyKSByZXR1cm4gZmFsc2U7XG5cdFx0cmV0dXJuIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0pID49IDAgJiYgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1sxXSkgPD0gMDtcblx0fVxuXG5cdGZ1bmN0aW9uIHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgfHwgc2VsZi5pc01vYmlsZSB8fCAhc2VsZi5tb250aE5hdikgcmV0dXJuO1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LnRleHRDb250ZW50ID0gc2VsZi51dGlscy5tb250aFRvU3RyKHNlbGYuY3VycmVudE1vbnRoKSArIFwiIFwiO1xuXHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LnZhbHVlID0gc2VsZi5jdXJyZW50WWVhcjtcblxuXHRcdHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyA9IHNlbGYuY29uZmlnLm1pbkRhdGUgJiYgKHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKSA/IHNlbGYuY3VycmVudE1vbnRoIDw9IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TW9udGgoKSA6IHNlbGYuY3VycmVudFllYXIgPCBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkpO1xuXG5cdFx0c2VsZi5faGlkZU5leHRNb250aEFycm93ID0gc2VsZi5jb25maWcubWF4RGF0ZSAmJiAoc2VsZi5jdXJyZW50WWVhciA9PT0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpID8gc2VsZi5jdXJyZW50TW9udGggKyAxID4gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNb250aCgpIDogc2VsZi5jdXJyZW50WWVhciA+IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSk7XG5cdH1cblxuXHRmdW5jdGlvbiB1cGRhdGVWYWx1ZSgpIHtcblx0XHRpZiAoIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHJldHVybiBzZWxmLmNsZWFyKCk7XG5cblx0XHRpZiAoc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0c2VsZi5tb2JpbGVJbnB1dC52YWx1ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPyBmb3JtYXREYXRlKHNlbGYubW9iaWxlRm9ybWF0U3RyLCBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaikgOiBcIlwiO1xuXHRcdH1cblxuXHRcdHZhciBqb2luQ2hhciA9IHNlbGYuY29uZmlnLm1vZGUgIT09IFwicmFuZ2VcIiA/IFwiOyBcIiA6IHNlbGYubDEwbi5yYW5nZVNlcGFyYXRvcjtcblxuXHRcdHNlbGYuaW5wdXQudmFsdWUgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubWFwKGZ1bmN0aW9uIChkT2JqKSB7XG5cdFx0XHRyZXR1cm4gZm9ybWF0RGF0ZShzZWxmLmNvbmZpZy5kYXRlRm9ybWF0LCBkT2JqKTtcblx0XHR9KS5qb2luKGpvaW5DaGFyKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5hbHRJbnB1dCkge1xuXHRcdFx0c2VsZi5hbHRJbnB1dC52YWx1ZSA9IHNlbGYuc2VsZWN0ZWREYXRlcy5tYXAoZnVuY3Rpb24gKGRPYmopIHtcblx0XHRcdFx0cmV0dXJuIGZvcm1hdERhdGUoc2VsZi5jb25maWcuYWx0Rm9ybWF0LCBkT2JqKTtcblx0XHRcdH0pLmpvaW4oam9pbkNoYXIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIlZhbHVlVXBkYXRlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24geWVhclNjcm9sbChlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG5cdFx0dmFyIGRlbHRhID0gTWF0aC5tYXgoLTEsIE1hdGgubWluKDEsIGUud2hlZWxEZWx0YSB8fCAtZS5kZWx0YVkpKSxcblx0XHQgICAgbmV3WWVhciA9IHBhcnNlSW50KGUudGFyZ2V0LnZhbHVlLCAxMCkgKyBkZWx0YTtcblxuXHRcdGNoYW5nZVllYXIobmV3WWVhcik7XG5cdFx0ZS50YXJnZXQudmFsdWUgPSBzZWxmLmN1cnJlbnRZZWFyO1xuXHR9XG5cblx0ZnVuY3Rpb24gY3JlYXRlRWxlbWVudCh0YWcsIGNsYXNzTmFtZSwgY29udGVudCkge1xuXHRcdHZhciBlID0gd2luZG93LmRvY3VtZW50LmNyZWF0ZUVsZW1lbnQodGFnKTtcblx0XHRjbGFzc05hbWUgPSBjbGFzc05hbWUgfHwgXCJcIjtcblx0XHRjb250ZW50ID0gY29udGVudCB8fCBcIlwiO1xuXG5cdFx0ZS5jbGFzc05hbWUgPSBjbGFzc05hbWU7XG5cblx0XHRpZiAoY29udGVudCkgZS50ZXh0Q29udGVudCA9IGNvbnRlbnQ7XG5cblx0XHRyZXR1cm4gZTtcblx0fVxuXG5cdGZ1bmN0aW9uIGFycmF5aWZ5KG9iaikge1xuXHRcdGlmIChBcnJheS5pc0FycmF5KG9iaikpIHJldHVybiBvYmo7XG5cdFx0cmV0dXJuIFtvYmpdO1xuXHR9XG5cblx0ZnVuY3Rpb24gdG9nZ2xlQ2xhc3MoZWxlbSwgY2xhc3NOYW1lLCBib29sKSB7XG5cdFx0aWYgKGJvb2wpIHJldHVybiBlbGVtLmNsYXNzTGlzdC5hZGQoY2xhc3NOYW1lKTtcblx0XHRlbGVtLmNsYXNzTGlzdC5yZW1vdmUoY2xhc3NOYW1lKTtcblx0fVxuXG5cdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdGZ1bmN0aW9uIGRlYm91bmNlKGZ1bmMsIHdhaXQsIGltbWVkaWF0ZSkge1xuXHRcdHZhciB0aW1lb3V0ID0gdm9pZCAwO1xuXHRcdHJldHVybiBmdW5jdGlvbiAoKSB7XG5cdFx0XHRmb3IgKHZhciBfbGVuID0gYXJndW1lbnRzLmxlbmd0aCwgYXJncyA9IEFycmF5KF9sZW4pLCBfa2V5ID0gMDsgX2tleSA8IF9sZW47IF9rZXkrKykge1xuXHRcdFx0XHRhcmdzW19rZXldID0gYXJndW1lbnRzW19rZXldO1xuXHRcdFx0fVxuXG5cdFx0XHR2YXIgY29udGV4dCA9IHRoaXM7XG5cdFx0XHR2YXIgbGF0ZXIgPSBmdW5jdGlvbiBsYXRlcigpIHtcblx0XHRcdFx0dGltZW91dCA9IG51bGw7XG5cdFx0XHRcdGlmICghaW1tZWRpYXRlKSBmdW5jLmFwcGx5KGNvbnRleHQsIGFyZ3MpO1xuXHRcdFx0fTtcblxuXHRcdFx0Y2xlYXJUaW1lb3V0KHRpbWVvdXQpO1xuXHRcdFx0dGltZW91dCA9IHNldFRpbWVvdXQobGF0ZXIsIHdhaXQpO1xuXHRcdFx0aWYgKGltbWVkaWF0ZSAmJiAhdGltZW91dCkgZnVuYy5hcHBseShjb250ZXh0LCBhcmdzKTtcblx0XHR9O1xuXHR9XG5cblx0ZnVuY3Rpb24gY29tcGFyZURhdGVzKGRhdGUxLCBkYXRlMiwgdGltZWxlc3MpIHtcblx0XHRpZiAoIShkYXRlMSBpbnN0YW5jZW9mIERhdGUpIHx8ICEoZGF0ZTIgaW5zdGFuY2VvZiBEYXRlKSkgcmV0dXJuIGZhbHNlO1xuXG5cdFx0aWYgKHRpbWVsZXNzICE9PSBmYWxzZSkge1xuXHRcdFx0cmV0dXJuIG5ldyBEYXRlKGRhdGUxLmdldFRpbWUoKSkuc2V0SG91cnMoMCwgMCwgMCwgMCkgLSBuZXcgRGF0ZShkYXRlMi5nZXRUaW1lKCkpLnNldEhvdXJzKDAsIDAsIDAsIDApO1xuXHRcdH1cblxuXHRcdHJldHVybiBkYXRlMS5nZXRUaW1lKCkgLSBkYXRlMi5nZXRUaW1lKCk7XG5cdH1cblxuXHRmdW5jdGlvbiB0aW1lV3JhcHBlcihlKSB7XG5cdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXG5cdFx0dmFyIGlzS2V5RG93biA9IGUudHlwZSA9PT0gXCJrZXlkb3duXCIsXG5cdFx0ICAgIGlzV2hlZWwgPSBlLnR5cGUgPT09IFwid2hlZWxcIixcblx0XHQgICAgaXNJbmNyZW1lbnQgPSBlLnR5cGUgPT09IFwiaW5jcmVtZW50XCIsXG5cdFx0ICAgIGlucHV0ID0gZS50YXJnZXQ7XG5cblx0XHRpZiAoZS50eXBlICE9PSBcImlucHV0XCIgJiYgIWlzS2V5RG93biAmJiAoZS50YXJnZXQudmFsdWUgfHwgZS50YXJnZXQudGV4dENvbnRlbnQpLmxlbmd0aCA+PSAyIC8vIHR5cGVkIHR3byBkaWdpdHNcblx0XHQpIHtcblx0XHRcdFx0ZS50YXJnZXQuZm9jdXMoKTtcblx0XHRcdFx0ZS50YXJnZXQuYmx1cigpO1xuXHRcdFx0fVxuXG5cdFx0aWYgKHNlbGYuYW1QTSAmJiBlLnRhcmdldCA9PT0gc2VsZi5hbVBNKSByZXR1cm4gZS50YXJnZXQudGV4dENvbnRlbnQgPSBbXCJBTVwiLCBcIlBNXCJdW2UudGFyZ2V0LnRleHRDb250ZW50ID09PSBcIkFNXCIgfCAwXTtcblxuXHRcdHZhciBtaW4gPSBOdW1iZXIoaW5wdXQubWluKSxcblx0XHQgICAgbWF4ID0gTnVtYmVyKGlucHV0Lm1heCksXG5cdFx0ICAgIHN0ZXAgPSBOdW1iZXIoaW5wdXQuc3RlcCksXG5cdFx0ICAgIGN1clZhbHVlID0gcGFyc2VJbnQoaW5wdXQudmFsdWUsIDEwKSxcblx0XHQgICAgZGVsdGEgPSBlLmRlbHRhIHx8ICghaXNLZXlEb3duID8gTWF0aC5tYXgoLTEsIE1hdGgubWluKDEsIGUud2hlZWxEZWx0YSB8fCAtZS5kZWx0YVkpKSB8fCAwIDogZS53aGljaCA9PT0gMzggPyAxIDogLTEpO1xuXG5cdFx0dmFyIG5ld1ZhbHVlID0gY3VyVmFsdWUgKyBzdGVwICogZGVsdGE7XG5cblx0XHRpZiAoaW5wdXQudmFsdWUubGVuZ3RoID09PSAyKSB7XG5cdFx0XHR2YXIgaXNIb3VyRWxlbSA9IGlucHV0ID09PSBzZWxmLmhvdXJFbGVtZW50LFxuXHRcdFx0ICAgIGlzTWludXRlRWxlbSA9IGlucHV0ID09PSBzZWxmLm1pbnV0ZUVsZW1lbnQ7XG5cblx0XHRcdGlmIChuZXdWYWx1ZSA8IG1pbikge1xuXHRcdFx0XHRuZXdWYWx1ZSA9IG1heCArIG5ld1ZhbHVlICsgIWlzSG91ckVsZW0gKyAoaXNIb3VyRWxlbSAmJiAhc2VsZi5hbVBNKTtcblxuXHRcdFx0XHRpZiAoaXNNaW51dGVFbGVtKSBpbmNyZW1lbnROdW1JbnB1dChudWxsLCAtMSwgc2VsZi5ob3VyRWxlbWVudCk7XG5cdFx0XHR9IGVsc2UgaWYgKG5ld1ZhbHVlID4gbWF4KSB7XG5cdFx0XHRcdG5ld1ZhbHVlID0gaW5wdXQgPT09IHNlbGYuaG91ckVsZW1lbnQgPyBuZXdWYWx1ZSAtIG1heCAtICFzZWxmLmFtUE0gOiBtaW47XG5cblx0XHRcdFx0aWYgKGlzTWludXRlRWxlbSkgaW5jcmVtZW50TnVtSW5wdXQobnVsbCwgMSwgc2VsZi5ob3VyRWxlbWVudCk7XG5cdFx0XHR9XG5cblx0XHRcdGlmIChzZWxmLmFtUE0gJiYgaXNIb3VyRWxlbSAmJiAoc3RlcCA9PT0gMSA/IG5ld1ZhbHVlICsgY3VyVmFsdWUgPT09IDIzIDogTWF0aC5hYnMobmV3VmFsdWUgLSBjdXJWYWx1ZSkgPiBzdGVwKSkgc2VsZi5hbVBNLnRleHRDb250ZW50ID0gc2VsZi5hbVBNLnRleHRDb250ZW50ID09PSBcIlBNXCIgPyBcIkFNXCIgOiBcIlBNXCI7XG5cblx0XHRcdGlucHV0LnZhbHVlID0gc2VsZi5wYWQobmV3VmFsdWUpO1xuXHRcdH1cblx0fVxuXG5cdGluaXQoKTtcblx0cmV0dXJuIHNlbGY7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5GbGF0cGlja3IuZGVmYXVsdENvbmZpZyA9IHtcblxuXHRtb2RlOiBcInNpbmdsZVwiLFxuXG5cdHBvc2l0aW9uOiBcInRvcFwiLFxuXG5cdC8qIGlmIHRydWUsIGRhdGVzIHdpbGwgYmUgcGFyc2VkLCBmb3JtYXR0ZWQsIGFuZCBkaXNwbGF5ZWQgaW4gVVRDLlxuIHByZWxvYWRpbmcgZGF0ZSBzdHJpbmdzIHcvIHRpbWV6b25lcyBpcyByZWNvbW1lbmRlZCBidXQgbm90IG5lY2Vzc2FyeSAqL1xuXHR1dGM6IGZhbHNlLFxuXG5cdC8vIHdyYXA6IHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI3N0cmFwXG5cdHdyYXA6IGZhbHNlLFxuXG5cdC8vIGVuYWJsZXMgd2VlayBudW1iZXJzXG5cdHdlZWtOdW1iZXJzOiBmYWxzZSxcblxuXHQvLyBhbGxvdyBtYW51YWwgZGF0ZXRpbWUgaW5wdXRcblx0YWxsb3dJbnB1dDogZmFsc2UsXG5cblx0LypcbiBcdGNsaWNraW5nIG9uIGlucHV0IG9wZW5zIHRoZSBkYXRlKHRpbWUpcGlja2VyLlxuIFx0ZGlzYWJsZSBpZiB5b3Ugd2lzaCB0byBvcGVuIHRoZSBjYWxlbmRhciBtYW51YWxseSB3aXRoIC5vcGVuKClcbiAqL1xuXHRjbGlja09wZW5zOiB0cnVlLFxuXG5cdC8vIGRpc3BsYXkgdGltZSBwaWNrZXIgaW4gMjQgaG91ciBtb2RlXG5cdHRpbWVfMjRocjogZmFsc2UsXG5cblx0Ly8gZW5hYmxlcyB0aGUgdGltZSBwaWNrZXIgZnVuY3Rpb25hbGl0eVxuXHRlbmFibGVUaW1lOiBmYWxzZSxcblxuXHQvLyBub0NhbGVuZGFyOiB0cnVlIHdpbGwgaGlkZSB0aGUgY2FsZW5kYXIuIHVzZSBmb3IgYSB0aW1lIHBpY2tlciBhbG9uZyB3LyBlbmFibGVUaW1lXG5cdG5vQ2FsZW5kYXI6IGZhbHNlLFxuXG5cdC8vIG1vcmUgZGF0ZSBmb3JtYXQgY2hhcnMgYXQgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNkYXRlZm9ybWF0XG5cdGRhdGVGb3JtYXQ6IFwiWS1tLWRcIixcblxuXHQvLyBhbHRJbnB1dCAtIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2FsdGlucHV0XG5cdGFsdElucHV0OiBmYWxzZSxcblxuXHQvLyB0aGUgY3JlYXRlZCBhbHRJbnB1dCBlbGVtZW50IHdpbGwgaGF2ZSB0aGlzIGNsYXNzLlxuXHRhbHRJbnB1dENsYXNzOiBcImZsYXRwaWNrci1pbnB1dCBmb3JtLWNvbnRyb2wgaW5wdXRcIixcblxuXHQvLyBzYW1lIGFzIGRhdGVGb3JtYXQsIGJ1dCBmb3IgYWx0SW5wdXRcblx0YWx0Rm9ybWF0OiBcIkYgaiwgWVwiLCAvLyBkZWZhdWx0cyB0byBlLmcuIEp1bmUgMTAsIDIwMTZcblxuXHQvLyBkZWZhdWx0RGF0ZSAtIGVpdGhlciBhIGRhdGVzdHJpbmcgb3IgYSBkYXRlIG9iamVjdC4gdXNlZCBmb3IgZGF0ZXRpbWVwaWNrZXJcInMgaW5pdGlhbCB2YWx1ZVxuXHRkZWZhdWx0RGF0ZTogbnVsbCxcblxuXHQvLyB0aGUgbWluaW11bSBkYXRlIHRoYXQgdXNlciBjYW4gcGljayAoaW5jbHVzaXZlKVxuXHRtaW5EYXRlOiBudWxsLFxuXG5cdC8vIHRoZSBtYXhpbXVtIGRhdGUgdGhhdCB1c2VyIGNhbiBwaWNrIChpbmNsdXNpdmUpXG5cdG1heERhdGU6IG51bGwsXG5cblx0Ly8gZGF0ZXBhcnNlciB0aGF0IHRyYW5zZm9ybXMgYSBnaXZlbiBzdHJpbmcgdG8gYSBkYXRlIG9iamVjdFxuXHRwYXJzZURhdGU6IG51bGwsXG5cblx0Ly8gZGF0ZWZvcm1hdHRlciB0aGF0IHRyYW5zZm9ybXMgYSBnaXZlbiBkYXRlIG9iamVjdCB0byBhIHN0cmluZywgYWNjb3JkaW5nIHRvIHBhc3NlZCBmb3JtYXRcblx0Zm9ybWF0RGF0ZTogbnVsbCxcblxuXHRnZXRXZWVrOiBmdW5jdGlvbiBnZXRXZWVrKGdpdmVuRGF0ZSkge1xuXHRcdHZhciBkYXRlID0gbmV3IERhdGUoZ2l2ZW5EYXRlLmdldFRpbWUoKSk7XG5cdFx0ZGF0ZS5zZXRIb3VycygwLCAwLCAwLCAwKTtcblxuXHRcdC8vIFRodXJzZGF5IGluIGN1cnJlbnQgd2VlayBkZWNpZGVzIHRoZSB5ZWFyLlxuXHRcdGRhdGUuc2V0RGF0ZShkYXRlLmdldERhdGUoKSArIDMgLSAoZGF0ZS5nZXREYXkoKSArIDYpICUgNyk7XG5cdFx0Ly8gSmFudWFyeSA0IGlzIGFsd2F5cyBpbiB3ZWVrIDEuXG5cdFx0dmFyIHdlZWsxID0gbmV3IERhdGUoZGF0ZS5nZXRGdWxsWWVhcigpLCAwLCA0KTtcblx0XHQvLyBBZGp1c3QgdG8gVGh1cnNkYXkgaW4gd2VlayAxIGFuZCBjb3VudCBudW1iZXIgb2Ygd2Vla3MgZnJvbSBkYXRlIHRvIHdlZWsxLlxuXHRcdHJldHVybiAxICsgTWF0aC5yb3VuZCgoKGRhdGUuZ2V0VGltZSgpIC0gd2VlazEuZ2V0VGltZSgpKSAvIDg2NDAwMDAwIC0gMyArICh3ZWVrMS5nZXREYXkoKSArIDYpICUgNykgLyA3KTtcblx0fSxcblxuXHQvLyBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNkaXNhYmxlXG5cdGVuYWJsZTogW10sXG5cblx0Ly8gc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jZGlzYWJsZVxuXHRkaXNhYmxlOiBbXSxcblxuXHQvLyBkaXNwbGF5IHRoZSBzaG9ydCB2ZXJzaW9uIG9mIG1vbnRoIG5hbWVzIC0gZS5nLiBTZXAgaW5zdGVhZCBvZiBTZXB0ZW1iZXJcblx0c2hvcnRoYW5kQ3VycmVudE1vbnRoOiBmYWxzZSxcblxuXHQvLyBkaXNwbGF5cyBjYWxlbmRhciBpbmxpbmUuIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2lubGluZS1jYWxlbmRhclxuXHRpbmxpbmU6IGZhbHNlLFxuXG5cdC8vIHBvc2l0aW9uIGNhbGVuZGFyIGluc2lkZSB3cmFwcGVyIGFuZCBuZXh0IHRvIHRoZSBpbnB1dCBlbGVtZW50XG5cdC8vIGxlYXZlIGF0IGZhbHNlIHVubGVzcyB5b3Uga25vdyB3aGF0IHlvdVwicmUgZG9pbmdcblx0c3RhdGljOiBmYWxzZSxcblxuXHQvLyBET00gbm9kZSB0byBhcHBlbmQgdGhlIGNhbGVuZGFyIHRvIGluICpzdGF0aWMqIG1vZGVcblx0YXBwZW5kVG86IG51bGwsXG5cblx0Ly8gY29kZSBmb3IgcHJldmlvdXMvbmV4dCBpY29ucy4gdGhpcyBpcyB3aGVyZSB5b3UgcHV0IHlvdXIgY3VzdG9tIGljb24gY29kZSBlLmcuIGZvbnRhd2Vzb21lXG5cdHByZXZBcnJvdzogXCI8c3ZnIHZlcnNpb249JzEuMScgeG1sbnM9J2h0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnJyB4bWxuczp4bGluaz0naHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluaycgdmlld0JveD0nMCAwIDE3IDE3Jz48Zz48L2c+PHBhdGggZD0nTTUuMjA3IDguNDcxbDcuMTQ2IDcuMTQ3LTAuNzA3IDAuNzA3LTcuODUzLTcuODU0IDcuODU0LTcuODUzIDAuNzA3IDAuNzA3LTcuMTQ3IDcuMTQ2eicgLz48L3N2Zz5cIixcblx0bmV4dEFycm93OiBcIjxzdmcgdmVyc2lvbj0nMS4xJyB4bWxucz0naHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmcnIHhtbG5zOnhsaW5rPSdodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rJyB2aWV3Qm94PScwIDAgMTcgMTcnPjxnPjwvZz48cGF0aCBkPSdNMTMuMjA3IDguNDcybC03Ljg1NCA3Ljg1NC0wLjcwNy0wLjcwNyA3LjE0Ni03LjE0Ni03LjE0Ni03LjE0OCAwLjcwNy0wLjcwNyA3Ljg1NCA3Ljg1NHonIC8+PC9zdmc+XCIsXG5cblx0Ly8gZW5hYmxlcyBzZWNvbmRzIGluIHRoZSB0aW1lIHBpY2tlclxuXHRlbmFibGVTZWNvbmRzOiBmYWxzZSxcblxuXHQvLyBzdGVwIHNpemUgdXNlZCB3aGVuIHNjcm9sbGluZy9pbmNyZW1lbnRpbmcgdGhlIGhvdXIgZWxlbWVudFxuXHRob3VySW5jcmVtZW50OiAxLFxuXG5cdC8vIHN0ZXAgc2l6ZSB1c2VkIHdoZW4gc2Nyb2xsaW5nL2luY3JlbWVudGluZyB0aGUgbWludXRlIGVsZW1lbnRcblx0bWludXRlSW5jcmVtZW50OiA1LFxuXG5cdC8vIGluaXRpYWwgdmFsdWUgaW4gdGhlIGhvdXIgZWxlbWVudFxuXHRkZWZhdWx0SG91cjogMTIsXG5cblx0Ly8gaW5pdGlhbCB2YWx1ZSBpbiB0aGUgbWludXRlIGVsZW1lbnRcblx0ZGVmYXVsdE1pbnV0ZTogMCxcblxuXHQvLyBkaXNhYmxlIG5hdGl2ZSBtb2JpbGUgZGF0ZXRpbWUgaW5wdXQgc3VwcG9ydFxuXHRkaXNhYmxlTW9iaWxlOiBmYWxzZSxcblxuXHQvLyBkZWZhdWx0IGxvY2FsZVxuXHRsb2NhbGU6IFwiZGVmYXVsdFwiLFxuXG5cdHBsdWdpbnM6IFtdLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIGNhbGVuZGFyIGlzIGNsb3NlZFxuXHRvbkNsb3NlOiBbXSwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gb25DaGFuZ2UgY2FsbGJhY2sgd2hlbiB1c2VyIHNlbGVjdHMgYSBkYXRlIG9yIHRpbWVcblx0b25DaGFuZ2U6IFtdLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBjYWxsZWQgZm9yIGV2ZXJ5IGRheSBlbGVtZW50XG5cdG9uRGF5Q3JlYXRlOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSB0aGUgbW9udGggaXMgY2hhbmdlZFxuXHRvbk1vbnRoQ2hhbmdlOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSBjYWxlbmRhciBpcyBvcGVuZWRcblx0b25PcGVuOiBbXSwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gY2FsbGVkIGFmdGVyIHRoZSBjb25maWd1cmF0aW9uIGhhcyBiZWVuIHBhcnNlZFxuXHRvblBhcnNlQ29uZmlnOiBbXSxcblxuXHQvLyBjYWxsZWQgYWZ0ZXIgY2FsZW5kYXIgaXMgcmVhZHlcblx0b25SZWFkeTogW10sIC8vIGZ1bmN0aW9uIChkYXRlT2JqLCBkYXRlU3RyKSB7fVxuXG5cdC8vIGNhbGxlZCBhZnRlciBpbnB1dCB2YWx1ZSB1cGRhdGVkXG5cdG9uVmFsdWVVcGRhdGU6IFtdLFxuXG5cdC8vIGNhbGxlZCBldmVyeSB0aW1lIHRoZSB5ZWFyIGlzIGNoYW5nZWRcblx0b25ZZWFyQ2hhbmdlOiBbXVxufTtcblxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbkZsYXRwaWNrci5sMTBucyA9IHtcblx0ZW46IHtcblx0XHR3ZWVrZGF5czoge1xuXHRcdFx0c2hvcnRoYW5kOiBbXCJTdW5cIiwgXCJNb25cIiwgXCJUdWVcIiwgXCJXZWRcIiwgXCJUaHVcIiwgXCJGcmlcIiwgXCJTYXRcIl0sXG5cdFx0XHRsb25naGFuZDogW1wiU3VuZGF5XCIsIFwiTW9uZGF5XCIsIFwiVHVlc2RheVwiLCBcIldlZG5lc2RheVwiLCBcIlRodXJzZGF5XCIsIFwiRnJpZGF5XCIsIFwiU2F0dXJkYXlcIl1cblx0XHR9LFxuXHRcdG1vbnRoczoge1xuXHRcdFx0c2hvcnRoYW5kOiBbXCJKYW5cIiwgXCJGZWJcIiwgXCJNYXJcIiwgXCJBcHJcIiwgXCJNYXlcIiwgXCJKdW5cIiwgXCJKdWxcIiwgXCJBdWdcIiwgXCJTZXBcIiwgXCJPY3RcIiwgXCJOb3ZcIiwgXCJEZWNcIl0sXG5cdFx0XHRsb25naGFuZDogW1wiSmFudWFyeVwiLCBcIkZlYnJ1YXJ5XCIsIFwiTWFyY2hcIiwgXCJBcHJpbFwiLCBcIk1heVwiLCBcIkp1bmVcIiwgXCJKdWx5XCIsIFwiQXVndXN0XCIsIFwiU2VwdGVtYmVyXCIsIFwiT2N0b2JlclwiLCBcIk5vdmVtYmVyXCIsIFwiRGVjZW1iZXJcIl1cblx0XHR9LFxuXHRcdGRheXNJbk1vbnRoOiBbMzEsIDI4LCAzMSwgMzAsIDMxLCAzMCwgMzEsIDMxLCAzMCwgMzEsIDMwLCAzMV0sXG5cdFx0Zmlyc3REYXlPZldlZWs6IDAsXG5cdFx0b3JkaW5hbDogZnVuY3Rpb24gb3JkaW5hbChudGgpIHtcblx0XHRcdHZhciBzID0gbnRoICUgMTAwO1xuXHRcdFx0aWYgKHMgPiAzICYmIHMgPCAyMSkgcmV0dXJuIFwidGhcIjtcblx0XHRcdHN3aXRjaCAocyAlIDEwKSB7XG5cdFx0XHRcdGNhc2UgMTpcblx0XHRcdFx0XHRyZXR1cm4gXCJzdFwiO1xuXHRcdFx0XHRjYXNlIDI6XG5cdFx0XHRcdFx0cmV0dXJuIFwibmRcIjtcblx0XHRcdFx0Y2FzZSAzOlxuXHRcdFx0XHRcdHJldHVybiBcInJkXCI7XG5cdFx0XHRcdGRlZmF1bHQ6XG5cdFx0XHRcdFx0cmV0dXJuIFwidGhcIjtcblx0XHRcdH1cblx0XHR9LFxuXHRcdHJhbmdlU2VwYXJhdG9yOiBcIiB0byBcIixcblx0XHR3ZWVrQWJicmV2aWF0aW9uOiBcIldrXCIsXG5cdFx0c2Nyb2xsVGl0bGU6IFwiU2Nyb2xsIHRvIGluY3JlbWVudFwiLFxuXHRcdHRvZ2dsZVRpdGxlOiBcIkNsaWNrIHRvIHRvZ2dsZVwiXG5cdH1cbn07XG5cbkZsYXRwaWNrci5sMTBucy5kZWZhdWx0ID0gT2JqZWN0LmNyZWF0ZShGbGF0cGlja3IubDEwbnMuZW4pO1xuRmxhdHBpY2tyLmxvY2FsaXplID0gZnVuY3Rpb24gKGwxMG4pIHtcblx0cmV0dXJuIF9leHRlbmRzKEZsYXRwaWNrci5sMTBucy5kZWZhdWx0LCBsMTBuIHx8IHt9KTtcbn07XG5GbGF0cGlja3Iuc2V0RGVmYXVsdHMgPSBmdW5jdGlvbiAoY29uZmlnKSB7XG5cdHJldHVybiBfZXh0ZW5kcyhGbGF0cGlja3IuZGVmYXVsdENvbmZpZywgY29uZmlnIHx8IHt9KTtcbn07XG5cbkZsYXRwaWNrci5wcm90b3R5cGUgPSB7XG5cdGZvcm1hdHM6IHtcblx0XHQvLyBnZXQgdGhlIGRhdGUgaW4gVVRDXG5cdFx0WjogZnVuY3Rpb24gWihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS50b0lTT1N0cmluZygpO1xuXHRcdH0sXG5cblx0XHQvLyB3ZWVrZGF5IG5hbWUsIHNob3J0LCBlLmcuIFRodVxuXHRcdEQ6IGZ1bmN0aW9uIEQoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMubDEwbi53ZWVrZGF5cy5zaG9ydGhhbmRbdGhpcy5mb3JtYXRzLncoZGF0ZSldO1xuXHRcdH0sXG5cblx0XHQvLyBmdWxsIG1vbnRoIG5hbWUgZS5nLiBKYW51YXJ5XG5cdFx0RjogZnVuY3Rpb24gRihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy51dGlscy5tb250aFRvU3RyKHRoaXMuZm9ybWF0cy5uKGRhdGUpIC0gMSwgZmFsc2UpO1xuXHRcdH0sXG5cblx0XHQvLyBob3VycyB3aXRoIGxlYWRpbmcgemVybyBlLmcuIDAzXG5cdFx0SDogZnVuY3Rpb24gSChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRIb3VycygpKTtcblx0XHR9LFxuXG5cdFx0Ly8gZGF5ICgxLTMwKSB3aXRoIG9yZGluYWwgc3VmZml4IGUuZy4gMXN0LCAybmRcblx0XHRKOiBmdW5jdGlvbiBKKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldERhdGUoKSArIHRoaXMubDEwbi5vcmRpbmFsKGRhdGUuZ2V0RGF0ZSgpKTtcblx0XHR9LFxuXG5cdFx0Ly8gQU0vUE1cblx0XHRLOiBmdW5jdGlvbiBLKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldEhvdXJzKCkgPiAxMSA/IFwiUE1cIiA6IFwiQU1cIjtcblx0XHR9LFxuXG5cdFx0Ly8gc2hvcnRoYW5kIG1vbnRoIGUuZy4gSmFuLCBTZXAsIE9jdCwgZXRjXG5cdFx0TTogZnVuY3Rpb24gTShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy51dGlscy5tb250aFRvU3RyKGRhdGUuZ2V0TW9udGgoKSwgdHJ1ZSk7XG5cdFx0fSxcblxuXHRcdC8vIHNlY29uZHMgMDAtNTlcblx0XHRTOiBmdW5jdGlvbiBTKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldFNlY29uZHMoKSk7XG5cdFx0fSxcblxuXHRcdC8vIHVuaXggdGltZXN0YW1wXG5cdFx0VTogZnVuY3Rpb24gVShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRUaW1lKCkgLyAxMDAwO1xuXHRcdH0sXG5cblx0XHRXOiBmdW5jdGlvbiBXKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLmNvbmZpZy5nZXRXZWVrKGRhdGUpO1xuXHRcdH0sXG5cblx0XHQvLyBmdWxsIHllYXIgZS5nLiAyMDE2XG5cdFx0WTogZnVuY3Rpb24gWShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdH0sXG5cblx0XHQvLyBkYXkgaW4gbW9udGgsIHBhZGRlZCAoMDEtMzApXG5cdFx0ZDogZnVuY3Rpb24gZChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXREYXRlKCkpO1xuXHRcdH0sXG5cblx0XHQvLyBob3VyIGZyb20gMS0xMiAoYW0vcG0pXG5cdFx0aDogZnVuY3Rpb24gaChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRIb3VycygpICUgMTIgPyBkYXRlLmdldEhvdXJzKCkgJSAxMiA6IDEyO1xuXHRcdH0sXG5cblx0XHQvLyBtaW51dGVzLCBwYWRkZWQgd2l0aCBsZWFkaW5nIHplcm8gZS5nLiAwOVxuXHRcdGk6IGZ1bmN0aW9uIGkoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0TWludXRlcygpKTtcblx0XHR9LFxuXG5cdFx0Ly8gZGF5IGluIG1vbnRoICgxLTMwKVxuXHRcdGo6IGZ1bmN0aW9uIGooZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0RGF0ZSgpO1xuXHRcdH0sXG5cblx0XHQvLyB3ZWVrZGF5IG5hbWUsIGZ1bGwsIGUuZy4gVGh1cnNkYXlcblx0XHRsOiBmdW5jdGlvbiBsKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLmwxMG4ud2Vla2RheXMubG9uZ2hhbmRbZGF0ZS5nZXREYXkoKV07XG5cdFx0fSxcblxuXHRcdC8vIHBhZGRlZCBtb250aCBudW1iZXIgKDAxLTEyKVxuXHRcdG06IGZ1bmN0aW9uIG0oZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0TW9udGgoKSArIDEpO1xuXHRcdH0sXG5cblx0XHQvLyB0aGUgbW9udGggbnVtYmVyICgxLTEyKVxuXHRcdG46IGZ1bmN0aW9uIG4oZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0TW9udGgoKSArIDE7XG5cdFx0fSxcblxuXHRcdC8vIHNlY29uZHMgMC01OVxuXHRcdHM6IGZ1bmN0aW9uIHMoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0U2Vjb25kcygpO1xuXHRcdH0sXG5cblx0XHQvLyBudW1iZXIgb2YgdGhlIGRheSBvZiB0aGUgd2Vla1xuXHRcdHc6IGZ1bmN0aW9uIHcoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0RGF5KCk7XG5cdFx0fSxcblxuXHRcdC8vIGxhc3QgdHdvIGRpZ2l0cyBvZiB5ZWFyIGUuZy4gMTYgZm9yIDIwMTZcblx0XHR5OiBmdW5jdGlvbiB5KGRhdGUpIHtcblx0XHRcdHJldHVybiBTdHJpbmcoZGF0ZS5nZXRGdWxsWWVhcigpKS5zdWJzdHJpbmcoMik7XG5cdFx0fVxuXHR9LFxuXG5cdHJldkZvcm1hdDoge1xuXHRcdEQ6IGZ1bmN0aW9uIEQoKSB7fSxcblx0XHRGOiBmdW5jdGlvbiBGKGRhdGVPYmosIG1vbnRoTmFtZSkge1xuXHRcdFx0ZGF0ZU9iai5zZXRNb250aCh0aGlzLmwxMG4ubW9udGhzLmxvbmdoYW5kLmluZGV4T2YobW9udGhOYW1lKSk7XG5cdFx0fSxcblx0XHRIOiBmdW5jdGlvbiBIKGRhdGVPYmosIGhvdXIpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldEhvdXJzKHBhcnNlRmxvYXQoaG91cikpO1xuXHRcdH0sXG5cdFx0SjogZnVuY3Rpb24gSihkYXRlT2JqLCBkYXkpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldERhdGUocGFyc2VGbG9hdChkYXkpKTtcblx0XHR9LFxuXHRcdEs6IGZ1bmN0aW9uIEsoZGF0ZU9iaiwgYW1QTSkge1xuXHRcdFx0dmFyIGhvdXJzID0gZGF0ZU9iai5nZXRIb3VycygpO1xuXG5cdFx0XHRpZiAoaG91cnMgIT09IDEyKSBkYXRlT2JqLnNldEhvdXJzKGhvdXJzICUgMTIgKyAxMiAqIC9wbS9pLnRlc3QoYW1QTSkpO1xuXHRcdH0sXG5cdFx0TTogZnVuY3Rpb24gTShkYXRlT2JqLCBzaG9ydE1vbnRoKSB7XG5cdFx0XHRkYXRlT2JqLnNldE1vbnRoKHRoaXMubDEwbi5tb250aHMuc2hvcnRoYW5kLmluZGV4T2Yoc2hvcnRNb250aCkpO1xuXHRcdH0sXG5cdFx0UzogZnVuY3Rpb24gUyhkYXRlT2JqLCBzZWNvbmRzKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRTZWNvbmRzKHNlY29uZHMpO1xuXHRcdH0sXG5cdFx0VzogZnVuY3Rpb24gVygpIHt9LFxuXHRcdFk6IGZ1bmN0aW9uIFkoZGF0ZU9iaiwgeWVhcikge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RnVsbFllYXIoeWVhcik7XG5cdFx0fSxcblx0XHRaOiBmdW5jdGlvbiBaKGRhdGVPYmosIElTT0RhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqID0gbmV3IERhdGUoSVNPRGF0ZSk7XG5cdFx0fSxcblxuXHRcdGQ6IGZ1bmN0aW9uIGQoZGF0ZU9iaiwgZGF5KSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXREYXRlKHBhcnNlRmxvYXQoZGF5KSk7XG5cdFx0fSxcblx0XHRoOiBmdW5jdGlvbiBoKGRhdGVPYmosIGhvdXIpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldEhvdXJzKHBhcnNlRmxvYXQoaG91cikpO1xuXHRcdH0sXG5cdFx0aTogZnVuY3Rpb24gaShkYXRlT2JqLCBtaW51dGVzKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRNaW51dGVzKHBhcnNlRmxvYXQobWludXRlcykpO1xuXHRcdH0sXG5cdFx0ajogZnVuY3Rpb24gaihkYXRlT2JqLCBkYXkpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldERhdGUocGFyc2VGbG9hdChkYXkpKTtcblx0XHR9LFxuXHRcdGw6IGZ1bmN0aW9uIGwoKSB7fSxcblx0XHRtOiBmdW5jdGlvbiBtKGRhdGVPYmosIG1vbnRoKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRNb250aChwYXJzZUZsb2F0KG1vbnRoKSAtIDEpO1xuXHRcdH0sXG5cdFx0bjogZnVuY3Rpb24gbihkYXRlT2JqLCBtb250aCkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0TW9udGgocGFyc2VGbG9hdChtb250aCkgLSAxKTtcblx0XHR9LFxuXHRcdHM6IGZ1bmN0aW9uIHMoZGF0ZU9iaiwgc2Vjb25kcykge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0U2Vjb25kcyhwYXJzZUZsb2F0KHNlY29uZHMpKTtcblx0XHR9LFxuXHRcdHc6IGZ1bmN0aW9uIHcoKSB7fSxcblx0XHR5OiBmdW5jdGlvbiB5KGRhdGVPYmosIHllYXIpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldEZ1bGxZZWFyKDIwMDAgKyBwYXJzZUZsb2F0KHllYXIpKTtcblx0XHR9XG5cdH0sXG5cblx0dG9rZW5SZWdleDoge1xuXHRcdEQ6IFwiKFxcXFx3KylcIixcblx0XHRGOiBcIihcXFxcdyspXCIsXG5cdFx0SDogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRKOiBcIihcXFxcZFxcXFxkfFxcXFxkKVxcXFx3K1wiLFxuXHRcdEs6IFwiKFxcXFx3KylcIixcblx0XHRNOiBcIihcXFxcdyspXCIsXG5cdFx0UzogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRZOiBcIihcXFxcZHs0fSlcIixcblx0XHRaOiBcIiguKylcIixcblx0XHRkOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdGg6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0aTogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRqOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdGw6IFwiKFxcXFx3KylcIixcblx0XHRtOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdG46IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0czogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHR3OiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdHk6IFwiKFxcXFxkezJ9KVwiXG5cdH0sXG5cblx0cGFkOiBmdW5jdGlvbiBwYWQobnVtYmVyKSB7XG5cdFx0cmV0dXJuIChcIjBcIiArIG51bWJlcikuc2xpY2UoLTIpO1xuXHR9LFxuXG5cdHBhcnNlRGF0ZTogZnVuY3Rpb24gcGFyc2VEYXRlKGRhdGUsIHRpbWVsZXNzLCBnaXZlbkZvcm1hdCkge1xuXHRcdGlmICghZGF0ZSkgcmV0dXJuIG51bGw7XG5cblx0XHR2YXIgZGF0ZV9vcmlnID0gZGF0ZTtcblxuXHRcdGlmIChkYXRlLnRvRml4ZWQpIC8vIHRpbWVzdGFtcFxuXHRcdFx0ZGF0ZSA9IG5ldyBEYXRlKGRhdGUpO2Vsc2UgaWYgKHR5cGVvZiBkYXRlID09PSBcInN0cmluZ1wiKSB7XG5cdFx0XHR2YXIgZm9ybWF0ID0gdHlwZW9mIGdpdmVuRm9ybWF0ID09PSBcInN0cmluZ1wiID8gZ2l2ZW5Gb3JtYXQgOiB0aGlzLmNvbmZpZy5kYXRlRm9ybWF0O1xuXHRcdFx0ZGF0ZSA9IGRhdGUudHJpbSgpO1xuXG5cdFx0XHRpZiAoZGF0ZSA9PT0gXCJ0b2RheVwiKSB7XG5cdFx0XHRcdGRhdGUgPSBuZXcgRGF0ZSgpO1xuXHRcdFx0XHR0aW1lbGVzcyA9IHRydWU7XG5cdFx0XHR9IGVsc2UgaWYgKHRoaXMuY29uZmlnICYmIHRoaXMuY29uZmlnLnBhcnNlRGF0ZSkgZGF0ZSA9IHRoaXMuY29uZmlnLnBhcnNlRGF0ZShkYXRlKTtlbHNlIGlmICgvWiQvLnRlc3QoZGF0ZSkgfHwgL0dNVCQvLnRlc3QoZGF0ZSkpIC8vIGRhdGVzdHJpbmdzIHcvIHRpbWV6b25lXG5cdFx0XHRcdGRhdGUgPSBuZXcgRGF0ZShkYXRlKTtlbHNlIHtcblx0XHRcdFx0dmFyIHBhcnNlZERhdGUgPSB0aGlzLmNvbmZpZy5ub0NhbGVuZGFyID8gbmV3IERhdGUobmV3IERhdGUoKS5zZXRIb3VycygwLCAwLCAwLCAwKSkgOiBuZXcgRGF0ZShuZXcgRGF0ZSgpLmdldEZ1bGxZZWFyKCksIDAsIDEsIDAsIDAsIDAsIDApO1xuXG5cdFx0XHRcdHZhciBtYXRjaGVkID0gZmFsc2U7XG5cblx0XHRcdFx0Zm9yICh2YXIgaSA9IDAsIG1hdGNoSW5kZXggPSAwLCByZWdleFN0ciA9IFwiXCI7IGkgPCBmb3JtYXQubGVuZ3RoOyBpKyspIHtcblx0XHRcdFx0XHR2YXIgdG9rZW4gPSBmb3JtYXRbaV07XG5cdFx0XHRcdFx0dmFyIGlzQmFja1NsYXNoID0gdG9rZW4gPT09IFwiXFxcXFwiO1xuXHRcdFx0XHRcdHZhciBlc2NhcGVkID0gZm9ybWF0W2kgLSAxXSA9PT0gXCJcXFxcXCIgfHwgaXNCYWNrU2xhc2g7XG5cdFx0XHRcdFx0aWYgKHRoaXMudG9rZW5SZWdleFt0b2tlbl0gJiYgIWVzY2FwZWQpIHtcblx0XHRcdFx0XHRcdHJlZ2V4U3RyICs9IHRoaXMudG9rZW5SZWdleFt0b2tlbl07XG5cdFx0XHRcdFx0XHR2YXIgbWF0Y2ggPSBuZXcgUmVnRXhwKHJlZ2V4U3RyKS5leGVjKGRhdGUpO1xuXHRcdFx0XHRcdFx0aWYgKG1hdGNoICYmIChtYXRjaGVkID0gdHJ1ZSkpIHRoaXMucmV2Rm9ybWF0W3Rva2VuXShwYXJzZWREYXRlLCBtYXRjaFsrK21hdGNoSW5kZXhdKTtcblx0XHRcdFx0XHR9IGVsc2UgaWYgKCFpc0JhY2tTbGFzaCkgcmVnZXhTdHIgKz0gXCIuXCI7IC8vIGRvbid0IHJlYWxseSBjYXJlXG5cdFx0XHRcdH1cblxuXHRcdFx0XHRkYXRlID0gbWF0Y2hlZCA/IHBhcnNlZERhdGUgOiBudWxsO1xuXHRcdFx0fVxuXHRcdH0gZWxzZSBpZiAoZGF0ZSBpbnN0YW5jZW9mIERhdGUpIGRhdGUgPSBuZXcgRGF0ZShkYXRlLmdldFRpbWUoKSk7IC8vIGNyZWF0ZSBhIGNvcHlcblxuXHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0aWYgKCEoZGF0ZSBpbnN0YW5jZW9mIERhdGUpKSB7XG5cdFx0XHRjb25zb2xlLndhcm4oXCJmbGF0cGlja3I6IGludmFsaWQgZGF0ZSBcIiArIGRhdGVfb3JpZyk7XG5cdFx0XHRjb25zb2xlLmluZm8odGhpcy5lbGVtZW50KTtcblx0XHRcdHJldHVybiBudWxsO1xuXHRcdH1cblxuXHRcdGlmICh0aGlzLmNvbmZpZyAmJiB0aGlzLmNvbmZpZy51dGMgJiYgIWRhdGUuZnBfaXNVVEMpIGRhdGUgPSBkYXRlLmZwX3RvVVRDKCk7XG5cblx0XHRpZiAodGltZWxlc3MgPT09IHRydWUpIGRhdGUuc2V0SG91cnMoMCwgMCwgMCwgMCk7XG5cblx0XHRyZXR1cm4gZGF0ZTtcblx0fVxufTtcblxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbmZ1bmN0aW9uIF9mbGF0cGlja3Iobm9kZUxpc3QsIGNvbmZpZykge1xuXHR2YXIgbm9kZXMgPSBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChub2RlTGlzdCk7IC8vIHN0YXRpYyBsaXN0XG5cdHZhciBpbnN0YW5jZXMgPSBbXTtcblx0Zm9yICh2YXIgaSA9IDA7IGkgPCBub2Rlcy5sZW5ndGg7IGkrKykge1xuXHRcdHRyeSB7XG5cdFx0XHRub2Rlc1tpXS5fZmxhdHBpY2tyID0gbmV3IEZsYXRwaWNrcihub2Rlc1tpXSwgY29uZmlnIHx8IHt9KTtcblx0XHRcdGluc3RhbmNlcy5wdXNoKG5vZGVzW2ldLl9mbGF0cGlja3IpO1xuXHRcdH0gY2F0Y2ggKGUpIHtcblx0XHRcdGNvbnNvbGUud2FybihlLCBlLnN0YWNrKTtcblx0XHR9XG5cdH1cblxuXHRyZXR1cm4gaW5zdGFuY2VzLmxlbmd0aCA9PT0gMSA/IGluc3RhbmNlc1swXSA6IGluc3RhbmNlcztcbn1cblxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbmlmICh0eXBlb2YgSFRNTEVsZW1lbnQgIT09IFwidW5kZWZpbmVkXCIpIHtcblx0Ly8gYnJvd3NlciBlbnZcblx0SFRNTENvbGxlY3Rpb24ucHJvdG90eXBlLmZsYXRwaWNrciA9IE5vZGVMaXN0LnByb3RvdHlwZS5mbGF0cGlja3IgPSBmdW5jdGlvbiAoY29uZmlnKSB7XG5cdFx0cmV0dXJuIF9mbGF0cGlja3IodGhpcywgY29uZmlnKTtcblx0fTtcblxuXHRIVE1MRWxlbWVudC5wcm90b3R5cGUuZmxhdHBpY2tyID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRcdHJldHVybiBfZmxhdHBpY2tyKFt0aGlzXSwgY29uZmlnKTtcblx0fTtcbn1cblxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbmZ1bmN0aW9uIGZsYXRwaWNrcihzZWxlY3RvciwgY29uZmlnKSB7XG5cdHJldHVybiBfZmxhdHBpY2tyKHdpbmRvdy5kb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKHNlbGVjdG9yKSwgY29uZmlnKTtcbn1cblxuLyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cbmlmICh0eXBlb2YgalF1ZXJ5ICE9PSBcInVuZGVmaW5lZFwiKSB7XG5cdGpRdWVyeS5mbi5mbGF0cGlja3IgPSBmdW5jdGlvbiAoY29uZmlnKSB7XG5cdFx0cmV0dXJuIF9mbGF0cGlja3IodGhpcywgY29uZmlnKTtcblx0fTtcbn1cblxuRGF0ZS5wcm90b3R5cGUuZnBfaW5jciA9IGZ1bmN0aW9uIChkYXlzKSB7XG5cdHJldHVybiBuZXcgRGF0ZSh0aGlzLmdldEZ1bGxZZWFyKCksIHRoaXMuZ2V0TW9udGgoKSwgdGhpcy5nZXREYXRlKCkgKyBwYXJzZUludChkYXlzLCAxMCkpO1xufTtcblxuRGF0ZS5wcm90b3R5cGUuZnBfaXNVVEMgPSBmYWxzZTtcbkRhdGUucHJvdG90eXBlLmZwX3RvVVRDID0gZnVuY3Rpb24gKCkge1xuXHR2YXIgbmV3RGF0ZSA9IG5ldyBEYXRlKHRoaXMuZ2V0VVRDRnVsbFllYXIoKSwgdGhpcy5nZXRVVENNb250aCgpLCB0aGlzLmdldFVUQ0RhdGUoKSwgdGhpcy5nZXRVVENIb3VycygpLCB0aGlzLmdldFVUQ01pbnV0ZXMoKSwgdGhpcy5nZXRVVENTZWNvbmRzKCkpO1xuXG5cdG5ld0RhdGUuZnBfaXNVVEMgPSB0cnVlO1xuXHRyZXR1cm4gbmV3RGF0ZTtcbn07XG5cbi8vIElFOSBjbGFzc0xpc3QgcG9seWZpbGxcbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5pZiAoIXdpbmRvdy5kb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuY2xhc3NMaXN0ICYmIE9iamVjdC5kZWZpbmVQcm9wZXJ0eSAmJiB0eXBlb2YgSFRNTEVsZW1lbnQgIT09IFwidW5kZWZpbmVkXCIpIHtcblx0T2JqZWN0LmRlZmluZVByb3BlcnR5KEhUTUxFbGVtZW50LnByb3RvdHlwZSwgXCJjbGFzc0xpc3RcIiwge1xuXHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0dmFyIHNlbGYgPSB0aGlzO1xuXHRcdFx0ZnVuY3Rpb24gdXBkYXRlKGZuKSB7XG5cdFx0XHRcdHJldHVybiBmdW5jdGlvbiAodmFsdWUpIHtcblx0XHRcdFx0XHR2YXIgY2xhc3NlcyA9IHNlbGYuY2xhc3NOYW1lLnNwbGl0KC9cXHMrLyksXG5cdFx0XHRcdFx0ICAgIGluZGV4ID0gY2xhc3Nlcy5pbmRleE9mKHZhbHVlKTtcblxuXHRcdFx0XHRcdGZuKGNsYXNzZXMsIGluZGV4LCB2YWx1ZSk7XG5cdFx0XHRcdFx0c2VsZi5jbGFzc05hbWUgPSBjbGFzc2VzLmpvaW4oXCIgXCIpO1xuXHRcdFx0XHR9O1xuXHRcdFx0fVxuXG5cdFx0XHR2YXIgcmV0ID0ge1xuXHRcdFx0XHRhZGQ6IHVwZGF0ZShmdW5jdGlvbiAoY2xhc3NlcywgaW5kZXgsIHZhbHVlKSB7XG5cdFx0XHRcdFx0aWYgKCF+aW5kZXgpIGNsYXNzZXMucHVzaCh2YWx1ZSk7XG5cdFx0XHRcdH0pLFxuXG5cdFx0XHRcdHJlbW92ZTogdXBkYXRlKGZ1bmN0aW9uIChjbGFzc2VzLCBpbmRleCkge1xuXHRcdFx0XHRcdGlmICh+aW5kZXgpIGNsYXNzZXMuc3BsaWNlKGluZGV4LCAxKTtcblx0XHRcdFx0fSksXG5cblx0XHRcdFx0dG9nZ2xlOiB1cGRhdGUoZnVuY3Rpb24gKGNsYXNzZXMsIGluZGV4LCB2YWx1ZSkge1xuXHRcdFx0XHRcdGlmICh+aW5kZXgpIGNsYXNzZXMuc3BsaWNlKGluZGV4LCAxKTtlbHNlIGNsYXNzZXMucHVzaCh2YWx1ZSk7XG5cdFx0XHRcdH0pLFxuXG5cdFx0XHRcdGNvbnRhaW5zOiBmdW5jdGlvbiBjb250YWlucyh2YWx1ZSkge1xuXHRcdFx0XHRcdHJldHVybiAhIX5zZWxmLmNsYXNzTmFtZS5zcGxpdCgvXFxzKy8pLmluZGV4T2YodmFsdWUpO1xuXHRcdFx0XHR9LFxuXG5cdFx0XHRcdGl0ZW06IGZ1bmN0aW9uIGl0ZW0oaSkge1xuXHRcdFx0XHRcdHJldHVybiBzZWxmLmNsYXNzTmFtZS5zcGxpdCgvXFxzKy8pW2ldIHx8IG51bGw7XG5cdFx0XHRcdH1cblx0XHRcdH07XG5cblx0XHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShyZXQsIFwibGVuZ3RoXCIsIHtcblx0XHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdFx0cmV0dXJuIHNlbGYuY2xhc3NOYW1lLnNwbGl0KC9cXHMrLykubGVuZ3RoO1xuXHRcdFx0XHR9XG5cdFx0XHR9KTtcblxuXHRcdFx0cmV0dXJuIHJldDtcblx0XHR9XG5cdH0pO1xufVxuXG5pZiAodHlwZW9mIG1vZHVsZSAhPT0gXCJ1bmRlZmluZWRcIikgbW9kdWxlLmV4cG9ydHMgPSBGbGF0cGlja3I7XG4iLCJ2YXIgVnVlIC8vIGxhdGUgYmluZFxudmFyIHZlcnNpb25cbnZhciBtYXAgPSB3aW5kb3cuX19WVUVfSE9UX01BUF9fID0gT2JqZWN0LmNyZWF0ZShudWxsKVxudmFyIGluc3RhbGxlZCA9IGZhbHNlXG52YXIgaXNCcm93c2VyaWZ5ID0gZmFsc2VcbnZhciBpbml0SG9va05hbWUgPSAnYmVmb3JlQ3JlYXRlJ1xuXG5leHBvcnRzLmluc3RhbGwgPSBmdW5jdGlvbiAodnVlLCBicm93c2VyaWZ5KSB7XG4gIGlmIChpbnN0YWxsZWQpIHJldHVyblxuICBpbnN0YWxsZWQgPSB0cnVlXG5cbiAgVnVlID0gdnVlLl9fZXNNb2R1bGUgPyB2dWUuZGVmYXVsdCA6IHZ1ZVxuICB2ZXJzaW9uID0gVnVlLnZlcnNpb24uc3BsaXQoJy4nKS5tYXAoTnVtYmVyKVxuICBpc0Jyb3dzZXJpZnkgPSBicm93c2VyaWZ5XG5cbiAgLy8gY29tcGF0IHdpdGggPCAyLjAuMC1hbHBoYS43XG4gIGlmIChWdWUuY29uZmlnLl9saWZlY3ljbGVIb29rcy5pbmRleE9mKCdpbml0JykgPiAtMSkge1xuICAgIGluaXRIb29rTmFtZSA9ICdpbml0J1xuICB9XG5cbiAgZXhwb3J0cy5jb21wYXRpYmxlID0gdmVyc2lvblswXSA+PSAyXG4gIGlmICghZXhwb3J0cy5jb21wYXRpYmxlKSB7XG4gICAgY29uc29sZS53YXJuKFxuICAgICAgJ1tITVJdIFlvdSBhcmUgdXNpbmcgYSB2ZXJzaW9uIG9mIHZ1ZS1ob3QtcmVsb2FkLWFwaSB0aGF0IGlzICcgK1xuICAgICAgJ29ubHkgY29tcGF0aWJsZSB3aXRoIFZ1ZS5qcyBjb3JlIF4yLjAuMC4nXG4gICAgKVxuICAgIHJldHVyblxuICB9XG59XG5cbi8qKlxuICogQ3JlYXRlIGEgcmVjb3JkIGZvciBhIGhvdCBtb2R1bGUsIHdoaWNoIGtlZXBzIHRyYWNrIG9mIGl0cyBjb25zdHJ1Y3RvclxuICogYW5kIGluc3RhbmNlc1xuICpcbiAqIEBwYXJhbSB7U3RyaW5nfSBpZFxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqL1xuXG5leHBvcnRzLmNyZWF0ZVJlY29yZCA9IGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICB2YXIgQ3RvciA9IG51bGxcbiAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgQ3RvciA9IG9wdGlvbnNcbiAgICBvcHRpb25zID0gQ3Rvci5vcHRpb25zXG4gIH1cbiAgbWFrZU9wdGlvbnNIb3QoaWQsIG9wdGlvbnMpXG4gIG1hcFtpZF0gPSB7XG4gICAgQ3RvcjogVnVlLmV4dGVuZChvcHRpb25zKSxcbiAgICBpbnN0YW5jZXM6IFtdXG4gIH1cbn1cblxuLyoqXG4gKiBNYWtlIGEgQ29tcG9uZW50IG9wdGlvbnMgb2JqZWN0IGhvdC5cbiAqXG4gKiBAcGFyYW0ge1N0cmluZ30gaWRcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKi9cblxuZnVuY3Rpb24gbWFrZU9wdGlvbnNIb3QgKGlkLCBvcHRpb25zKSB7XG4gIGluamVjdEhvb2sob3B0aW9ucywgaW5pdEhvb2tOYW1lLCBmdW5jdGlvbiAoKSB7XG4gICAgbWFwW2lkXS5pbnN0YW5jZXMucHVzaCh0aGlzKVxuICB9KVxuICBpbmplY3RIb29rKG9wdGlvbnMsICdiZWZvcmVEZXN0cm95JywgZnVuY3Rpb24gKCkge1xuICAgIHZhciBpbnN0YW5jZXMgPSBtYXBbaWRdLmluc3RhbmNlc1xuICAgIGluc3RhbmNlcy5zcGxpY2UoaW5zdGFuY2VzLmluZGV4T2YodGhpcyksIDEpXG4gIH0pXG59XG5cbi8qKlxuICogSW5qZWN0IGEgaG9vayB0byBhIGhvdCByZWxvYWRhYmxlIGNvbXBvbmVudCBzbyB0aGF0XG4gKiB3ZSBjYW4ga2VlcCB0cmFjayBvZiBpdC5cbiAqXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICogQHBhcmFtIHtTdHJpbmd9IG5hbWVcbiAqIEBwYXJhbSB7RnVuY3Rpb259IGhvb2tcbiAqL1xuXG5mdW5jdGlvbiBpbmplY3RIb29rIChvcHRpb25zLCBuYW1lLCBob29rKSB7XG4gIHZhciBleGlzdGluZyA9IG9wdGlvbnNbbmFtZV1cbiAgb3B0aW9uc1tuYW1lXSA9IGV4aXN0aW5nXG4gICAgPyBBcnJheS5pc0FycmF5KGV4aXN0aW5nKVxuICAgICAgPyBleGlzdGluZy5jb25jYXQoaG9vaylcbiAgICAgIDogW2V4aXN0aW5nLCBob29rXVxuICAgIDogW2hvb2tdXG59XG5cbmZ1bmN0aW9uIHRyeVdyYXAgKGZuKSB7XG4gIHJldHVybiBmdW5jdGlvbiAoaWQsIGFyZykge1xuICAgIHRyeSB7IGZuKGlkLCBhcmcpIH0gY2F0Y2ggKGUpIHtcbiAgICAgIGNvbnNvbGUuZXJyb3IoZSlcbiAgICAgIGNvbnNvbGUud2FybignU29tZXRoaW5nIHdlbnQgd3JvbmcgZHVyaW5nIFZ1ZSBjb21wb25lbnQgaG90LXJlbG9hZC4gRnVsbCByZWxvYWQgcmVxdWlyZWQuJylcbiAgICB9XG4gIH1cbn1cblxuZXhwb3J0cy5yZXJlbmRlciA9IHRyeVdyYXAoZnVuY3Rpb24gKGlkLCBvcHRpb25zKSB7XG4gIHZhciByZWNvcmQgPSBtYXBbaWRdXG4gIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIG9wdGlvbnMgPSBvcHRpb25zLm9wdGlvbnNcbiAgfVxuICByZWNvcmQuQ3Rvci5vcHRpb25zLnJlbmRlciA9IG9wdGlvbnMucmVuZGVyXG4gIHJlY29yZC5DdG9yLm9wdGlvbnMuc3RhdGljUmVuZGVyRm5zID0gb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnNcbiAgcmVjb3JkLmluc3RhbmNlcy5zbGljZSgpLmZvckVhY2goZnVuY3Rpb24gKGluc3RhbmNlKSB7XG4gICAgaW5zdGFuY2UuJG9wdGlvbnMucmVuZGVyID0gb3B0aW9ucy5yZW5kZXJcbiAgICBpbnN0YW5jZS4kb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnMgPSBvcHRpb25zLnN0YXRpY1JlbmRlckZuc1xuICAgIGluc3RhbmNlLl9zdGF0aWNUcmVlcyA9IFtdIC8vIHJlc2V0IHN0YXRpYyB0cmVlc1xuICAgIGluc3RhbmNlLiRmb3JjZVVwZGF0ZSgpXG4gIH0pXG59KVxuXG5leHBvcnRzLnJlbG9hZCA9IHRyeVdyYXAoZnVuY3Rpb24gKGlkLCBvcHRpb25zKSB7XG4gIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIG9wdGlvbnMgPSBvcHRpb25zLm9wdGlvbnNcbiAgfVxuICBtYWtlT3B0aW9uc0hvdChpZCwgb3B0aW9ucylcbiAgdmFyIHJlY29yZCA9IG1hcFtpZF1cbiAgaWYgKHZlcnNpb25bMV0gPCAyKSB7XG4gICAgLy8gcHJlc2VydmUgcHJlIDIuMiBiZWhhdmlvciBmb3IgZ2xvYmFsIG1peGluIGhhbmRsaW5nXG4gICAgcmVjb3JkLkN0b3IuZXh0ZW5kT3B0aW9ucyA9IG9wdGlvbnNcbiAgfVxuICB2YXIgbmV3Q3RvciA9IHJlY29yZC5DdG9yLnN1cGVyLmV4dGVuZChvcHRpb25zKVxuICByZWNvcmQuQ3Rvci5vcHRpb25zID0gbmV3Q3Rvci5vcHRpb25zXG4gIHJlY29yZC5DdG9yLmNpZCA9IG5ld0N0b3IuY2lkXG4gIHJlY29yZC5DdG9yLnByb3RvdHlwZSA9IG5ld0N0b3IucHJvdG90eXBlXG4gIGlmIChuZXdDdG9yLnJlbGVhc2UpIHtcbiAgICAvLyB0ZW1wb3JhcnkgZ2xvYmFsIG1peGluIHN0cmF0ZWd5IHVzZWQgaW4gPCAyLjAuMC1hbHBoYS42XG4gICAgbmV3Q3Rvci5yZWxlYXNlKClcbiAgfVxuICByZWNvcmQuaW5zdGFuY2VzLnNsaWNlKCkuZm9yRWFjaChmdW5jdGlvbiAoaW5zdGFuY2UpIHtcbiAgICBpZiAoaW5zdGFuY2UuJHZub2RlICYmIGluc3RhbmNlLiR2bm9kZS5jb250ZXh0KSB7XG4gICAgICBpbnN0YW5jZS4kdm5vZGUuY29udGV4dC4kZm9yY2VVcGRhdGUoKVxuICAgIH0gZWxzZSB7XG4gICAgICBjb25zb2xlLndhcm4oJ1Jvb3Qgb3IgbWFudWFsbHkgbW91bnRlZCBpbnN0YW5jZSBtb2RpZmllZC4gRnVsbCByZWxvYWQgcmVxdWlyZWQuJylcbiAgICB9XG4gIH0pXG59KVxuIiwi77u/PHRlbXBsYXRlPlxyXG4gICAgPGlucHV0IHR5cGU9XCJ0ZXh0XCIgOmNsYXNzPVwiaW5wdXRDbGFzc1wiIDpwbGFjZWhvbGRlcj1cInBsYWNlaG9sZGVyXCIgOnZhbHVlPVwidmFsdWVcIiBAaW5wdXQ9XCJvbklucHV0XCI+XHJcbjwvdGVtcGxhdGU+XHJcblxyXG48c2NyaXB0PlxyXG5pbXBvcnQgRmxhdHBpY2tyIGZyb20gJ2ZsYXRwaWNrcidcclxuXHJcbmV4cG9ydCBkZWZhdWx0IHtcclxuICAgIHByb3BzOiB7XHJcbiAgICAgICAgaW5wdXRDbGFzczoge1xyXG4gICAgICAgICAgICB0eXBlOiBTdHJpbmdcclxuICAgICAgICB9LFxyXG4gICAgICAgIHBsYWNlaG9sZGVyOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IFN0cmluZyxcclxuICAgICAgICAgICAgZGVmYXVsdDogJydcclxuICAgICAgICB9LFxyXG4gICAgICAgIG9wdGlvbnM6IHtcclxuICAgICAgICAgICAgdHlwZTogT2JqZWN0LFxyXG4gICAgICAgICAgICBkZWZhdWx0OiAoKSA9PiB7IHJldHVybiB7fSB9XHJcbiAgICAgICAgfSxcclxuICAgICAgICB2YWx1ZToge1xyXG4gICAgICAgICAgICB0eXBlOiBTdHJpbmcsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICcnXHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICBkYXRhICgpIHtcclxuICAgICAgcmV0dXJuIHtcclxuICAgICAgICAgIGZwOiBudWxsXHJcbiAgICAgIH1cclxuICB9LFxyXG4gICAgY29tcHV0ZWQ6IHtcclxuICAgICAgICBmcE9wdGlvbnMgKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gSlNPTi5zdHJpbmdpZnkodGhpcy5vcHRpb25zKVxyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgICB3YXRjaDoge1xyXG4gICAgICAgIGZwT3B0aW9ucyAobmV3T3B0KSB7XHJcbiAgICAgICAgICAgIGNvbnN0IG9wdGlvbiA9IEpTT04ucGFyc2UobmV3T3B0KVxyXG4gICAgICAgICAgICBmb3IgKGxldCBvIGluIG9wdGlvbikge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5mcC5zZXQobywgb3B0aW9uW29dKVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICBtb3VudGVkICgpIHtcclxuICAgICAgY29uc3Qgc2VsZiA9IHRoaXNcclxuICAgICAgY29uc3Qgb3JpZ09uVmFsVXBkYXRlID0gdGhpcy5vcHRpb25zLm9uVmFsdWVVcGRhdGVcclxuICAgICAgdGhpcy5mcCA9IG5ldyBGbGF0cGlja3IodGhpcy4kZWwsIE9iamVjdC5hc3NpZ24odGhpcy5vcHRpb25zLCB7XHJcbiAgICAgICAgICBvblZhbHVlVXBkYXRlICgpIHtcclxuICAgICAgICAgICAgICBzZWxmLm9uSW5wdXQoc2VsZi4kZWwudmFsdWUsIHNlbGYuZnAuc2VsZWN0ZWREYXRlcylcclxuICAgICAgICAgICAgICBpZiAodHlwZW9mIG9yaWdPblZhbFVwZGF0ZSA9PT0gJ2Z1bmN0aW9uJykge1xyXG4gICAgICAgICAgICAgICAgICBvcmlnT25WYWxVcGRhdGUoKVxyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgIH1cclxuICAgICAgfSkpXHJcbiAgICAgIHRoaXMuJGVtaXQoJ0ZsYXRwaWNrclJlZicsIHRoaXMuZnApXHJcbiAgfSxcclxuICBkZXN0cm95ZWQgKCkge1xyXG4gICAgICB0aGlzLmZwLmRlc3Ryb3koKVxyXG4gICAgICB0aGlzLmZwID0gbnVsbFxyXG4gIH0sXHJcbiAgICBtZXRob2RzOiB7XHJcbiAgICAgICAgb25JbnB1dCAoZSwgc2VsZWN0ZWREYXRlcykge1xyXG4gICAgICAgICAgICB0eXBlb2YgZSA9PT0gJ3N0cmluZycgPyB0aGlzLiRlbWl0KCdpbnB1dCcsIGUpIDogdGhpcy4kZW1pdCgnaW5wdXQnLCBlLnRhcmdldC52YWx1ZSlcclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuPC9zY3JpcHQ+Iiwi77u/PHRlbXBsYXRlPlxyXG4gICAgPGRpdiBjbGFzcz1cImNvbWJvLWJveFwiPlxyXG4gICAgICAgIDxkaXYgY2xhc3M9XCJidG4tZ3JvdXAgYnRuLWlucHV0IGNsZWFyZml4XCI+XHJcbiAgICAgICAgICAgIDxidXR0b24gdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuIGRyb3Bkb3duLXRvZ2dsZVwiIGRhdGEtdG9nZ2xlPVwiZHJvcGRvd25cIj5cclxuICAgICAgICAgICAgICAgIDxzcGFuIGRhdGEtYmluZD1cImxhYmVsXCIgdi1pZj1cInZhbHVlID09PSBudWxsXCIgY2xhc3M9XCJncmF5LXRleHRcIj57e3BsYWNlaG9sZGVyVGV4dH19PC9zcGFuPlxyXG4gICAgICAgICAgICAgICAgPHNwYW4gZGF0YS1iaW5kPVwibGFiZWxcIiB2LWVsc2U+e3t2YWx1ZS52YWx1ZX19PC9zcGFuPlxyXG4gICAgICAgICAgICA8L2J1dHRvbj5cclxuICAgICAgICAgICAgPHVsIHJlZj1cImRyb3Bkb3duTWVudVwiIGNsYXNzPVwiZHJvcGRvd24tbWVudVwiIHJvbGU9XCJtZW51XCI+XHJcbiAgICAgICAgICAgICAgICA8bGk+XHJcbiAgICAgICAgICAgICAgICAgICAgPGlucHV0IHR5cGU9XCJ0ZXh0XCIgcmVmPVwic2VhcmNoQm94XCIgOmlkPVwiaW5wdXRJZFwiIHBsYWNlaG9sZGVyPVwiU2VhcmNoXCIgQGlucHV0PVwidXBkYXRlT3B0aW9uc0xpc3RcIiB2LW9uOmtleXVwLmRvd249XCJvblNlYXJjaEJveERvd25LZXlcIiB2LW1vZGVsPVwic2VhcmNoVGVybVwiIC8+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtZm9yPVwib3B0aW9uIGluIG9wdGlvbnNcIiA6a2V5PVwib3B0aW9uLmtleVwiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhIGhyZWY9XCJqYXZhc2NyaXB0OnZvaWQoMCk7XCIgdi1vbjpjbGljaz1cInNlbGVjdE9wdGlvbihvcHRpb24pXCIgdi1odG1sPVwiaGlnaGxpZ2h0KG9wdGlvbi52YWx1ZSwgc2VhcmNoVGVybSlcIiB2LW9uOmtleWRvd24udXA9XCJvbk9wdGlvblVwS2V5XCI+PC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWlmPVwiaXNMb2FkaW5nXCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGE+TG9hZGluZy4uLjwvYT5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1pZj1cIiFpc0xvYWRpbmcgJiYgb3B0aW9ucy5sZW5ndGggPT09IDBcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YT5ObyByZXN1bHRzIGZvdW5kPC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgPC91bD5cclxuICAgICAgICA8L2Rpdj5cclxuICAgICAgICA8YnV0dG9uIHYtaWY9XCJ2YWx1ZSAhPT0gbnVsbFwiIGNsYXNzPVwiYnRuIGJ0bi1saW5rIGJ0bi1jbGVhclwiIEBjbGljaz1cImNsZWFyXCI+XHJcbiAgICAgICAgICAgIDxzcGFuPjwvc3Bhbj5cclxuICAgICAgICA8L2J1dHRvbj5cclxuICAgIDwvZGl2PlxyXG48L3RlbXBsYXRlPlxyXG5cclxuPHNjcmlwdD5cclxuICAgIG1vZHVsZS5leHBvcnRzID0ge1xyXG4gICAgICAgIG5hbWU6ICd1c2VyLXNlbGVjdG9yJyxcclxuICAgICAgICBwcm9wczogWydmZXRjaFVybCcsICdjb250cm9sSWQnLCAndmFsdWUnLCAncGxhY2Vob2xkZXInXSxcclxuICAgICAgICBkYXRhOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zOiBbXSxcclxuICAgICAgICAgICAgICAgIGlzTG9hZGluZzogZmFsc2UsXHJcbiAgICAgICAgICAgICAgICBzZWFyY2hUZXJtOiAnJ1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgY29tcHV0ZWQ6IHtcclxuICAgICAgICAgICAgaW5wdXRJZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGBzYl8ke3RoaXMuY29udHJvbElkfWA7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHBsYWNlaG9sZGVyVGV4dDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMucGxhY2Vob2xkZXIgfHwgXCJTZWxlY3RcIjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgbW91bnRlZDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGNvbnN0IGpxRWwgPSAkKHRoaXMuJGVsKVxyXG4gICAgICAgICAgICBjb25zdCBmb2N1c1RvID0ganFFbC5maW5kKGAjJHt0aGlzLmlucHV0SWR9YClcclxuICAgICAgICAgICAganFFbC5vbignc2hvd24uYnMuZHJvcGRvd24nLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICBmb2N1c1RvLmZvY3VzKClcclxuICAgICAgICAgICAgICAgIHRoaXMuZmV0Y2hPcHRpb25zKHRoaXMuc2VhcmNoVGVybSlcclxuICAgICAgICAgICAgfSlcclxuXHJcbiAgICAgICAgICAgIGpxRWwub24oJ2hpZGRlbi5icy5kcm9wZG93bicsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VhcmNoVGVybSA9IFwiXCJcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICB9LFxyXG4gICAgICAgIG1ldGhvZHM6IHtcclxuICAgICAgICAgICAgb25TZWFyY2hCb3hEb3duS2V5KGV2ZW50KSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgJGZpcnN0T3B0aW9uQW5jaG9yID0gJCh0aGlzLiRyZWZzLmRyb3Bkb3duTWVudSkuZmluZCgnYScpLmZpcnN0KCk7XHJcbiAgICAgICAgICAgICAgICAkZmlyc3RPcHRpb25BbmNob3IuZm9jdXMoKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgb25PcHRpb25VcEtleShldmVudCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGlzRmlyc3RPcHRpb24gPSAkKGV2ZW50LnRhcmdldCkucGFyZW50KCkuaW5kZXgoKSA9PT0gMTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoaXNGaXJzdE9wdGlvbikge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuJHJlZnMuc2VhcmNoQm94LmZvY3VzKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnQuc3RvcFByb3BhZ2F0aW9uKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGZldGNoT3B0aW9uczogZnVuY3Rpb24gKGZpbHRlciA9IFwiXCIpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUubG9nKGBmaWx0ZXI6IHtmaWx0ZXJ9YCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRodHRwLmdldCh0aGlzLmZldGNoVXJsLCB7cGFyYW1zOiB7IHF1ZXJ5OiBmaWx0ZXIgfX0pXHJcbiAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLm9wdGlvbnMgPSByZXNwb25zZS5ib2R5Lm9wdGlvbnMgfHwgW107XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfSwgcmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGNsZWFyOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRlbWl0KCdzZWxlY3RlZCcsIG51bGwsIHRoaXMuY29udHJvbElkKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VhcmNoVGVybSA9IFwiXCI7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHNlbGVjdE9wdGlvbjogZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRlbWl0KCdzZWxlY3RlZCcsIHZhbHVlLCB0aGlzLmNvbnRyb2xJZCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHVwZGF0ZU9wdGlvbnNMaXN0KGUpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZmV0Y2hPcHRpb25zKGUudGFyZ2V0LnZhbHVlKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgaGlnaGxpZ2h0OiBmdW5jdGlvbiAodGl0bGUsIHNlYXJjaFRlcm0pIHtcclxuICAgICAgICAgICAgICAgIHZhciBlbmNvZGVkVGl0bGUgPSBfLmVzY2FwZSh0aXRsZSk7XHJcbiAgICAgICAgICAgICAgICBpZiAoc2VhcmNoVGVybSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBzYWZlU2VhcmNoVGVybSA9IF8uZXNjYXBlKF8uZXNjYXBlUmVnRXhwKHNlYXJjaFRlcm0pKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGlRdWVyeSA9IG5ldyBSZWdFeHAoc2FmZVNlYXJjaFRlcm0sIFwiaWdcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGVuY29kZWRUaXRsZS5yZXBsYWNlKGlRdWVyeSwgKG1hdGNoZWRUeHQsIGEsIGIpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGA8c3Ryb25nPiR7bWF0Y2hlZFR4dH08L3N0cm9uZz5gO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiBlbmNvZGVkVGl0bGU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9O1xyXG48L3NjcmlwdD4iLCLvu79pbXBvcnQgVnVlIGZyb20gJ3Z1ZSdcclxuaW1wb3J0IFZ1ZVJlc291cmNlIGZyb20gJ3Z1ZS1yZXNvdXJjZSdcclxuaW1wb3J0IFVzZXJTZWxlY3RvciBmcm9tICcuL1VzZXJTZWxlY3Rvci52dWUnXHJcbmltcG9ydCBEYXRlUGlja2VyIGZyb20gJy4vRGF0ZVBpY2tlci52dWUnXHJcblxyXG5WdWUuY29tcG9uZW50KCdGbGF0cGlja3InLCBEYXRlUGlja2VyKTtcclxuVnVlLnVzZShWdWVSZXNvdXJjZSk7XHJcblxyXG5cclxuVnVlLmNvbXBvbmVudChcInVzZXItc2VsZWN0b3JcIiwgVXNlclNlbGVjdG9yKTtcclxuXHJcbnZhciBhcHAgPSBuZXcgVnVlKHtcclxuICAgIGRhdGE6IHtcclxuICAgICAgICBpbnRlcnZpZXdlcklkOiBudWxsLFxyXG4gICAgICAgIHF1ZXN0aW9ubmFpcmVJZDogbnVsbCxcclxuICAgICAgICBkYXRlU3RyOiBudWxsLFxyXG4gICAgICAgIGRhdGVSYW5nZVBpY2tlck9wdGlvbnM6IHtcclxuICAgICAgICAgICAgbW9kZTogXCJyYW5nZVwiLFxyXG4gICAgICAgICAgICBtYXhEYXRlOiBcInRvZGF5XCIsXHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICAgIG1ldGhvZHM6IHtcclxuICAgICAgICB1c2VyU2VsZWN0ZWQobmV3VmFsdWUsIGlkKSB7XHJcbiAgICAgICAgICAgIHRoaXMuaW50ZXJ2aWV3ZXJJZCA9IG5ld1ZhbHVlO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcXVlc3Rpb25uYWlyZVNlbGVjdGVkKG5ld1ZhbHVlLCBpZCkge1xyXG4gICAgICAgICAgICB0aGlzLnF1ZXN0aW9ubmFpcmVJZCA9IG5ld1ZhbHVlO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcmFuZ2VTZWxlY3RlZChuZXdWYWx1ZSwgaWQpIHtcclxuICAgICAgICAgICAgY29uc29sZS5sb2cobmV3VmFsdWUpO1xyXG4gICAgICAgICAgICBjb25zb2xlLmxvZyhpZCk7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG59KTtcclxuXHJcbndpbmRvdy5vbmxvYWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICBWdWUuaHR0cC5oZWFkZXJzLmNvbW1vblsnQXV0aG9yaXphdGlvbiddID0gaW5wdXQuc2V0dGluZ3MuYWNzcmYudG9rZW47XHJcblxyXG4gICAgYXBwLiRtb3VudCgnI2FwcCcpO1xyXG59Il19
