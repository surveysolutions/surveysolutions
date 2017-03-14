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

var _veeValidate = (typeof window !== "undefined" ? window['VeeValidate'] : typeof global !== "undefined" ? global['VeeValidate'] : null);

var _veeValidate2 = _interopRequireDefault(_veeValidate);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

_vue2.default.use(_veeValidate2.default);
_vue2.default.use(_vueResource2.default);

_vue2.default.component('Flatpickr', _DatePicker2.default);
_vue2.default.component("user-selector", _UserSelector2.default);

var app = new _vue2.default({
    data: {
        interviewerId: null,
        questionnaireId: null,
        dateFrom: null,
        dateTo: null,
        dateRangePickerOptions: {
            mode: "range",
            maxDate: "today",
            minDate: new Date().fp_incr(-30)
        }
    },
    methods: {
        userSelected: function userSelected(newValue) {
            this.interviewerId = newValue;
        },
        questionnaireSelected: function questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },
        rangeSelected: function rangeSelected(textValue, from, to) {
            this.dateFrom = from;
            this.dateTo = to;
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

},{"./DatePicker.vue":42,"./UserSelector.vue":43}]},{},[44])
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvYmFiZWwtcnVudGltZS9jb3JlLWpzL2pzb24vc3RyaW5naWZ5LmpzIiwibm9kZV9tb2R1bGVzL2JhYmVsLXJ1bnRpbWUvY29yZS1qcy9vYmplY3QvYXNzaWduLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvZm4vb2JqZWN0L2Fzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYS1mdW5jdGlvbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYW4tb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19hcnJheS1pbmNsdWRlcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY29mLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jb3JlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19jdHguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2RlZmluZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2Rlc2NyaXB0b3JzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kb20tY3JlYXRlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19lbnVtLWJ1Zy1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19leHBvcnQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2ZhaWxzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19nbG9iYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2hhcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faGlkZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9faWU4LWRvbS1kZWZpbmUuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lvYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2lzLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWFzc2lnbi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWRwLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtZ29wcy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWtleXMtaW50ZXJuYWwuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1rZXlzLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3QtcGllLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19wcm9wZXJ0eS1kZXNjLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQta2V5LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19zaGFyZWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLWluZGV4LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pbnRlZ2VyLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1sZW5ndGguanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8tcHJpbWl0aXZlLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL191aWQuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvZXM2Lm9iamVjdC5hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvZmxhdHBpY2tyL2Rpc3QvZmxhdHBpY2tyLmpzIiwibm9kZV9tb2R1bGVzL3Z1ZS1ob3QtcmVsb2FkLWFwaS9pbmRleC5qcyIsInZ1ZVxcdnVlXFxEYXRlUGlja2VyLnZ1ZT8xOTM1YmE4YyIsInZ1ZVxcdnVlXFxVc2VyU2VsZWN0b3IudnVlPzRmZjA1NGNlIiwidnVlXFx2dWVcXHRyb3VibGVzaG9vdGluZy1jZW5zdXMuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUNBQTs7QUNBQTs7QUNBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7O0FDREE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDcEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTs7QUNEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ25CQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzVEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1BBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaENBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2ZBOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDUEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNMQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDWEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzM5REE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDOUhBOzs7Ozs7O0FBR0E7QUFDQTtBQUNBO0FBREE7QUFHQTtBQUNBO0FBQ0E7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUFBO0FBQUE7QUFGQTtBQUlBO0FBQ0E7QUFDQTtBQUZBO0FBWkE7QUFpQkE7QUFDQTtBQUNBO0FBREE7QUFHQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUhBO0FBS0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBTkE7QUFRQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQXJEQTs7Ozs7QUFaQTtBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7O0FDNkJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFIQTtBQUtBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQUE7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUFBOztBQUFBOztBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFoREE7QUE5QkE7Ozs7O0FBN0JBO0FBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ0FFOzs7O0FBQ0Y7Ozs7QUFDQTs7OztBQUNBOzs7O0FBQ0E7Ozs7OztBQUVBLGNBQUksR0FBSjtBQUNBLGNBQUksR0FBSjs7QUFFQSxjQUFJLFNBQUosQ0FBYyxXQUFkO0FBQ0EsY0FBSSxTQUFKLENBQWMsZUFBZDs7QUFFQSxJQUFJLE1BQU0sa0JBQVE7QUFDZCxVQUFNO0FBQ0YsdUJBQWUsSUFEYjtBQUVGLHlCQUFpQixJQUZmO0FBR0Ysa0JBQVUsSUFIUjtBQUlGLGdCQUFRLElBSk47QUFLRixnQ0FBd0I7QUFDcEIsa0JBQU0sT0FEYztBQUVwQixxQkFBUyxPQUZXO0FBR3BCLHFCQUFTLElBQUksSUFBSixHQUFXLE9BQVgsQ0FBbUIsQ0FBQyxFQUFwQjtBQUhXO0FBTHRCLEtBRFE7QUFZZCxhQUFTO0FBQ0wsb0JBREssd0JBQ1EsUUFEUixFQUNrQjtBQUNuQixpQkFBSyxhQUFMLEdBQXFCLFFBQXJCO0FBQ0gsU0FISTtBQUlMLDZCQUpLLGlDQUlpQixRQUpqQixFQUkyQjtBQUM1QixpQkFBSyxlQUFMLEdBQXVCLFFBQXZCO0FBQ0gsU0FOSTtBQU9MLHFCQVBLLHlCQU9TLFNBUFQsRUFPb0IsSUFQcEIsRUFPMEIsRUFQMUIsRUFPOEI7QUFDL0IsaUJBQUssUUFBTCxHQUFnQixJQUFoQjtBQUNBLGlCQUFLLE1BQUwsR0FBYyxFQUFkO0FBQ0gsU0FWSTtBQVdMLG9CQVhLLDBCQVdVO0FBQUE7O0FBQ1gsaUJBQUssVUFBTCxDQUFnQixXQUFoQixHQUE4QixJQUE5QixDQUFtQyxrQkFBVTtBQUN6QyxvQkFBSSxNQUFKLEVBQVk7QUFDUiwwQkFBSyxjQUFMO0FBQ0g7QUFDSixhQUpEO0FBS0gsU0FqQkk7QUFrQkwsc0JBbEJLLDRCQWtCWTtBQUNiLHFCQUFTLGFBQVQsQ0FBdUIsTUFBdkIsRUFBK0IsU0FBL0IsQ0FBeUMsTUFBekMsQ0FBZ0Qsc0JBQWhEO0FBQ0g7QUFwQkksS0FaSztBQWtDZCxhQUFTLG1CQUFXO0FBQ2hCLGlCQUFTLGFBQVQsQ0FBdUIsTUFBdkIsRUFBK0IsU0FBL0IsQ0FBeUMsTUFBekMsQ0FBZ0QsaUJBQWhEO0FBQ0g7QUFwQ2EsQ0FBUixDQUFWOztBQXVDQSxPQUFPLE1BQVAsR0FBZ0IsWUFBWTtBQUN4QixrQkFBSSxJQUFKLENBQVMsT0FBVCxDQUFpQixNQUFqQixDQUF3QixlQUF4QixJQUEyQyxNQUFNLFFBQU4sQ0FBZSxLQUFmLENBQXFCLEtBQWhFOztBQUVBLFFBQUksTUFBSixDQUFXLE1BQVg7QUFDSCxDQUpEIiwiZmlsZSI6ImdlbmVyYXRlZC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzQ29udGVudCI6WyIoZnVuY3Rpb24gZSh0LG4scil7ZnVuY3Rpb24gcyhvLHUpe2lmKCFuW29dKXtpZighdFtvXSl7dmFyIGE9dHlwZW9mIHJlcXVpcmU9PVwiZnVuY3Rpb25cIiYmcmVxdWlyZTtpZighdSYmYSlyZXR1cm4gYShvLCEwKTtpZihpKXJldHVybiBpKG8sITApO3ZhciBmPW5ldyBFcnJvcihcIkNhbm5vdCBmaW5kIG1vZHVsZSAnXCIrbytcIidcIik7dGhyb3cgZi5jb2RlPVwiTU9EVUxFX05PVF9GT1VORFwiLGZ9dmFyIGw9bltvXT17ZXhwb3J0czp7fX07dFtvXVswXS5jYWxsKGwuZXhwb3J0cyxmdW5jdGlvbihlKXt2YXIgbj10W29dWzFdW2VdO3JldHVybiBzKG4/bjplKX0sbCxsLmV4cG9ydHMsZSx0LG4scil9cmV0dXJuIG5bb10uZXhwb3J0c312YXIgaT10eXBlb2YgcmVxdWlyZT09XCJmdW5jdGlvblwiJiZyZXF1aXJlO2Zvcih2YXIgbz0wO288ci5sZW5ndGg7bysrKXMocltvXSk7cmV0dXJuIHN9KSIsIm1vZHVsZS5leHBvcnRzID0geyBcImRlZmF1bHRcIjogcmVxdWlyZShcImNvcmUtanMvbGlicmFyeS9mbi9qc29uL3N0cmluZ2lmeVwiKSwgX19lc01vZHVsZTogdHJ1ZSB9OyIsIm1vZHVsZS5leHBvcnRzID0geyBcImRlZmF1bHRcIjogcmVxdWlyZShcImNvcmUtanMvbGlicmFyeS9mbi9vYmplY3QvYXNzaWduXCIpLCBfX2VzTW9kdWxlOiB0cnVlIH07IiwidmFyIGNvcmUgID0gcmVxdWlyZSgnLi4vLi4vbW9kdWxlcy9fY29yZScpXG4gICwgJEpTT04gPSBjb3JlLkpTT04gfHwgKGNvcmUuSlNPTiA9IHtzdHJpbmdpZnk6IEpTT04uc3RyaW5naWZ5fSk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uIHN0cmluZ2lmeShpdCl7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW51c2VkLXZhcnNcbiAgcmV0dXJuICRKU09OLnN0cmluZ2lmeS5hcHBseSgkSlNPTiwgYXJndW1lbnRzKTtcbn07IiwicmVxdWlyZSgnLi4vLi4vbW9kdWxlcy9lczYub2JqZWN0LmFzc2lnbicpO1xubW9kdWxlLmV4cG9ydHMgPSByZXF1aXJlKCcuLi8uLi9tb2R1bGVzL19jb3JlJykuT2JqZWN0LmFzc2lnbjsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgaWYodHlwZW9mIGl0ICE9ICdmdW5jdGlvbicpdGhyb3cgVHlwZUVycm9yKGl0ICsgJyBpcyBub3QgYSBmdW5jdGlvbiEnKTtcbiAgcmV0dXJuIGl0O1xufTsiLCJ2YXIgaXNPYmplY3QgPSByZXF1aXJlKCcuL19pcy1vYmplY3QnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICBpZighaXNPYmplY3QoaXQpKXRocm93IFR5cGVFcnJvcihpdCArICcgaXMgbm90IGFuIG9iamVjdCEnKTtcbiAgcmV0dXJuIGl0O1xufTsiLCIvLyBmYWxzZSAtPiBBcnJheSNpbmRleE9mXG4vLyB0cnVlICAtPiBBcnJheSNpbmNsdWRlc1xudmFyIHRvSU9iamVjdCA9IHJlcXVpcmUoJy4vX3RvLWlvYmplY3QnKVxuICAsIHRvTGVuZ3RoICA9IHJlcXVpcmUoJy4vX3RvLWxlbmd0aCcpXG4gICwgdG9JbmRleCAgID0gcmVxdWlyZSgnLi9fdG8taW5kZXgnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oSVNfSU5DTFVERVMpe1xuICByZXR1cm4gZnVuY3Rpb24oJHRoaXMsIGVsLCBmcm9tSW5kZXgpe1xuICAgIHZhciBPICAgICAgPSB0b0lPYmplY3QoJHRoaXMpXG4gICAgICAsIGxlbmd0aCA9IHRvTGVuZ3RoKE8ubGVuZ3RoKVxuICAgICAgLCBpbmRleCAgPSB0b0luZGV4KGZyb21JbmRleCwgbGVuZ3RoKVxuICAgICAgLCB2YWx1ZTtcbiAgICAvLyBBcnJheSNpbmNsdWRlcyB1c2VzIFNhbWVWYWx1ZVplcm8gZXF1YWxpdHkgYWxnb3JpdGhtXG4gICAgaWYoSVNfSU5DTFVERVMgJiYgZWwgIT0gZWwpd2hpbGUobGVuZ3RoID4gaW5kZXgpe1xuICAgICAgdmFsdWUgPSBPW2luZGV4KytdO1xuICAgICAgaWYodmFsdWUgIT0gdmFsdWUpcmV0dXJuIHRydWU7XG4gICAgLy8gQXJyYXkjdG9JbmRleCBpZ25vcmVzIGhvbGVzLCBBcnJheSNpbmNsdWRlcyAtIG5vdFxuICAgIH0gZWxzZSBmb3IoO2xlbmd0aCA+IGluZGV4OyBpbmRleCsrKWlmKElTX0lOQ0xVREVTIHx8IGluZGV4IGluIE8pe1xuICAgICAgaWYoT1tpbmRleF0gPT09IGVsKXJldHVybiBJU19JTkNMVURFUyB8fCBpbmRleCB8fCAwO1xuICAgIH0gcmV0dXJuICFJU19JTkNMVURFUyAmJiAtMTtcbiAgfTtcbn07IiwidmFyIHRvU3RyaW5nID0ge30udG9TdHJpbmc7XG5cbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gdG9TdHJpbmcuY2FsbChpdCkuc2xpY2UoOCwgLTEpO1xufTsiLCJ2YXIgY29yZSA9IG1vZHVsZS5leHBvcnRzID0ge3ZlcnNpb246ICcyLjQuMCd9O1xuaWYodHlwZW9mIF9fZSA9PSAnbnVtYmVyJylfX2UgPSBjb3JlOyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVuZGVmIiwiLy8gb3B0aW9uYWwgLyBzaW1wbGUgY29udGV4dCBiaW5kaW5nXG52YXIgYUZ1bmN0aW9uID0gcmVxdWlyZSgnLi9fYS1mdW5jdGlvbicpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihmbiwgdGhhdCwgbGVuZ3RoKXtcbiAgYUZ1bmN0aW9uKGZuKTtcbiAgaWYodGhhdCA9PT0gdW5kZWZpbmVkKXJldHVybiBmbjtcbiAgc3dpdGNoKGxlbmd0aCl7XG4gICAgY2FzZSAxOiByZXR1cm4gZnVuY3Rpb24oYSl7XG4gICAgICByZXR1cm4gZm4uY2FsbCh0aGF0LCBhKTtcbiAgICB9O1xuICAgIGNhc2UgMjogcmV0dXJuIGZ1bmN0aW9uKGEsIGIpe1xuICAgICAgcmV0dXJuIGZuLmNhbGwodGhhdCwgYSwgYik7XG4gICAgfTtcbiAgICBjYXNlIDM6IHJldHVybiBmdW5jdGlvbihhLCBiLCBjKXtcbiAgICAgIHJldHVybiBmbi5jYWxsKHRoYXQsIGEsIGIsIGMpO1xuICAgIH07XG4gIH1cbiAgcmV0dXJuIGZ1bmN0aW9uKC8qIC4uLmFyZ3MgKi8pe1xuICAgIHJldHVybiBmbi5hcHBseSh0aGF0LCBhcmd1bWVudHMpO1xuICB9O1xufTsiLCIvLyA3LjIuMSBSZXF1aXJlT2JqZWN0Q29lcmNpYmxlKGFyZ3VtZW50KVxubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIGlmKGl0ID09IHVuZGVmaW5lZCl0aHJvdyBUeXBlRXJyb3IoXCJDYW4ndCBjYWxsIG1ldGhvZCBvbiAgXCIgKyBpdCk7XG4gIHJldHVybiBpdDtcbn07IiwiLy8gVGhhbmsncyBJRTggZm9yIGhpcyBmdW5ueSBkZWZpbmVQcm9wZXJ0eVxubW9kdWxlLmV4cG9ydHMgPSAhcmVxdWlyZSgnLi9fZmFpbHMnKShmdW5jdGlvbigpe1xuICByZXR1cm4gT2JqZWN0LmRlZmluZVByb3BlcnR5KHt9LCAnYScsIHtnZXQ6IGZ1bmN0aW9uKCl7IHJldHVybiA3OyB9fSkuYSAhPSA3O1xufSk7IiwidmFyIGlzT2JqZWN0ID0gcmVxdWlyZSgnLi9faXMtb2JqZWN0JylcbiAgLCBkb2N1bWVudCA9IHJlcXVpcmUoJy4vX2dsb2JhbCcpLmRvY3VtZW50XG4gIC8vIGluIG9sZCBJRSB0eXBlb2YgZG9jdW1lbnQuY3JlYXRlRWxlbWVudCBpcyAnb2JqZWN0J1xuICAsIGlzID0gaXNPYmplY3QoZG9jdW1lbnQpICYmIGlzT2JqZWN0KGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBpcyA/IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoaXQpIDoge307XG59OyIsIi8vIElFIDgtIGRvbid0IGVudW0gYnVnIGtleXNcbm1vZHVsZS5leHBvcnRzID0gKFxuICAnY29uc3RydWN0b3IsaGFzT3duUHJvcGVydHksaXNQcm90b3R5cGVPZixwcm9wZXJ0eUlzRW51bWVyYWJsZSx0b0xvY2FsZVN0cmluZyx0b1N0cmluZyx2YWx1ZU9mJ1xuKS5zcGxpdCgnLCcpOyIsInZhciBnbG9iYWwgICAgPSByZXF1aXJlKCcuL19nbG9iYWwnKVxuICAsIGNvcmUgICAgICA9IHJlcXVpcmUoJy4vX2NvcmUnKVxuICAsIGN0eCAgICAgICA9IHJlcXVpcmUoJy4vX2N0eCcpXG4gICwgaGlkZSAgICAgID0gcmVxdWlyZSgnLi9faGlkZScpXG4gICwgUFJPVE9UWVBFID0gJ3Byb3RvdHlwZSc7XG5cbnZhciAkZXhwb3J0ID0gZnVuY3Rpb24odHlwZSwgbmFtZSwgc291cmNlKXtcbiAgdmFyIElTX0ZPUkNFRCA9IHR5cGUgJiAkZXhwb3J0LkZcbiAgICAsIElTX0dMT0JBTCA9IHR5cGUgJiAkZXhwb3J0LkdcbiAgICAsIElTX1NUQVRJQyA9IHR5cGUgJiAkZXhwb3J0LlNcbiAgICAsIElTX1BST1RPICA9IHR5cGUgJiAkZXhwb3J0LlBcbiAgICAsIElTX0JJTkQgICA9IHR5cGUgJiAkZXhwb3J0LkJcbiAgICAsIElTX1dSQVAgICA9IHR5cGUgJiAkZXhwb3J0LldcbiAgICAsIGV4cG9ydHMgICA9IElTX0dMT0JBTCA/IGNvcmUgOiBjb3JlW25hbWVdIHx8IChjb3JlW25hbWVdID0ge30pXG4gICAgLCBleHBQcm90byAgPSBleHBvcnRzW1BST1RPVFlQRV1cbiAgICAsIHRhcmdldCAgICA9IElTX0dMT0JBTCA/IGdsb2JhbCA6IElTX1NUQVRJQyA/IGdsb2JhbFtuYW1lXSA6IChnbG9iYWxbbmFtZV0gfHwge30pW1BST1RPVFlQRV1cbiAgICAsIGtleSwgb3duLCBvdXQ7XG4gIGlmKElTX0dMT0JBTClzb3VyY2UgPSBuYW1lO1xuICBmb3Ioa2V5IGluIHNvdXJjZSl7XG4gICAgLy8gY29udGFpbnMgaW4gbmF0aXZlXG4gICAgb3duID0gIUlTX0ZPUkNFRCAmJiB0YXJnZXQgJiYgdGFyZ2V0W2tleV0gIT09IHVuZGVmaW5lZDtcbiAgICBpZihvd24gJiYga2V5IGluIGV4cG9ydHMpY29udGludWU7XG4gICAgLy8gZXhwb3J0IG5hdGl2ZSBvciBwYXNzZWRcbiAgICBvdXQgPSBvd24gPyB0YXJnZXRba2V5XSA6IHNvdXJjZVtrZXldO1xuICAgIC8vIHByZXZlbnQgZ2xvYmFsIHBvbGx1dGlvbiBmb3IgbmFtZXNwYWNlc1xuICAgIGV4cG9ydHNba2V5XSA9IElTX0dMT0JBTCAmJiB0eXBlb2YgdGFyZ2V0W2tleV0gIT0gJ2Z1bmN0aW9uJyA/IHNvdXJjZVtrZXldXG4gICAgLy8gYmluZCB0aW1lcnMgdG8gZ2xvYmFsIGZvciBjYWxsIGZyb20gZXhwb3J0IGNvbnRleHRcbiAgICA6IElTX0JJTkQgJiYgb3duID8gY3R4KG91dCwgZ2xvYmFsKVxuICAgIC8vIHdyYXAgZ2xvYmFsIGNvbnN0cnVjdG9ycyBmb3IgcHJldmVudCBjaGFuZ2UgdGhlbSBpbiBsaWJyYXJ5XG4gICAgOiBJU19XUkFQICYmIHRhcmdldFtrZXldID09IG91dCA/IChmdW5jdGlvbihDKXtcbiAgICAgIHZhciBGID0gZnVuY3Rpb24oYSwgYiwgYyl7XG4gICAgICAgIGlmKHRoaXMgaW5zdGFuY2VvZiBDKXtcbiAgICAgICAgICBzd2l0Y2goYXJndW1lbnRzLmxlbmd0aCl7XG4gICAgICAgICAgICBjYXNlIDA6IHJldHVybiBuZXcgQztcbiAgICAgICAgICAgIGNhc2UgMTogcmV0dXJuIG5ldyBDKGEpO1xuICAgICAgICAgICAgY2FzZSAyOiByZXR1cm4gbmV3IEMoYSwgYik7XG4gICAgICAgICAgfSByZXR1cm4gbmV3IEMoYSwgYiwgYyk7XG4gICAgICAgIH0gcmV0dXJuIEMuYXBwbHkodGhpcywgYXJndW1lbnRzKTtcbiAgICAgIH07XG4gICAgICBGW1BST1RPVFlQRV0gPSBDW1BST1RPVFlQRV07XG4gICAgICByZXR1cm4gRjtcbiAgICAvLyBtYWtlIHN0YXRpYyB2ZXJzaW9ucyBmb3IgcHJvdG90eXBlIG1ldGhvZHNcbiAgICB9KShvdXQpIDogSVNfUFJPVE8gJiYgdHlwZW9mIG91dCA9PSAnZnVuY3Rpb24nID8gY3R4KEZ1bmN0aW9uLmNhbGwsIG91dCkgOiBvdXQ7XG4gICAgLy8gZXhwb3J0IHByb3RvIG1ldGhvZHMgdG8gY29yZS4lQ09OU1RSVUNUT1IlLm1ldGhvZHMuJU5BTUUlXG4gICAgaWYoSVNfUFJPVE8pe1xuICAgICAgKGV4cG9ydHMudmlydHVhbCB8fCAoZXhwb3J0cy52aXJ0dWFsID0ge30pKVtrZXldID0gb3V0O1xuICAgICAgLy8gZXhwb3J0IHByb3RvIG1ldGhvZHMgdG8gY29yZS4lQ09OU1RSVUNUT1IlLnByb3RvdHlwZS4lTkFNRSVcbiAgICAgIGlmKHR5cGUgJiAkZXhwb3J0LlIgJiYgZXhwUHJvdG8gJiYgIWV4cFByb3RvW2tleV0paGlkZShleHBQcm90bywga2V5LCBvdXQpO1xuICAgIH1cbiAgfVxufTtcbi8vIHR5cGUgYml0bWFwXG4kZXhwb3J0LkYgPSAxOyAgIC8vIGZvcmNlZFxuJGV4cG9ydC5HID0gMjsgICAvLyBnbG9iYWxcbiRleHBvcnQuUyA9IDQ7ICAgLy8gc3RhdGljXG4kZXhwb3J0LlAgPSA4OyAgIC8vIHByb3RvXG4kZXhwb3J0LkIgPSAxNjsgIC8vIGJpbmRcbiRleHBvcnQuVyA9IDMyOyAgLy8gd3JhcFxuJGV4cG9ydC5VID0gNjQ7ICAvLyBzYWZlXG4kZXhwb3J0LlIgPSAxMjg7IC8vIHJlYWwgcHJvdG8gbWV0aG9kIGZvciBgbGlicmFyeWAgXG5tb2R1bGUuZXhwb3J0cyA9ICRleHBvcnQ7IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihleGVjKXtcbiAgdHJ5IHtcbiAgICByZXR1cm4gISFleGVjKCk7XG4gIH0gY2F0Y2goZSl7XG4gICAgcmV0dXJuIHRydWU7XG4gIH1cbn07IiwiLy8gaHR0cHM6Ly9naXRodWIuY29tL3psb2lyb2NrL2NvcmUtanMvaXNzdWVzLzg2I2lzc3VlY29tbWVudC0xMTU3NTkwMjhcbnZhciBnbG9iYWwgPSBtb2R1bGUuZXhwb3J0cyA9IHR5cGVvZiB3aW5kb3cgIT0gJ3VuZGVmaW5lZCcgJiYgd2luZG93Lk1hdGggPT0gTWF0aFxuICA/IHdpbmRvdyA6IHR5cGVvZiBzZWxmICE9ICd1bmRlZmluZWQnICYmIHNlbGYuTWF0aCA9PSBNYXRoID8gc2VsZiA6IEZ1bmN0aW9uKCdyZXR1cm4gdGhpcycpKCk7XG5pZih0eXBlb2YgX19nID09ICdudW1iZXInKV9fZyA9IGdsb2JhbDsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bmRlZiIsInZhciBoYXNPd25Qcm9wZXJ0eSA9IHt9Lmhhc093blByb3BlcnR5O1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCwga2V5KXtcbiAgcmV0dXJuIGhhc093blByb3BlcnR5LmNhbGwoaXQsIGtleSk7XG59OyIsInZhciBkUCAgICAgICAgID0gcmVxdWlyZSgnLi9fb2JqZWN0LWRwJylcbiAgLCBjcmVhdGVEZXNjID0gcmVxdWlyZSgnLi9fcHJvcGVydHktZGVzYycpO1xubW9kdWxlLmV4cG9ydHMgPSByZXF1aXJlKCcuL19kZXNjcmlwdG9ycycpID8gZnVuY3Rpb24ob2JqZWN0LCBrZXksIHZhbHVlKXtcbiAgcmV0dXJuIGRQLmYob2JqZWN0LCBrZXksIGNyZWF0ZURlc2MoMSwgdmFsdWUpKTtcbn0gOiBmdW5jdGlvbihvYmplY3QsIGtleSwgdmFsdWUpe1xuICBvYmplY3Rba2V5XSA9IHZhbHVlO1xuICByZXR1cm4gb2JqZWN0O1xufTsiLCJtb2R1bGUuZXhwb3J0cyA9ICFyZXF1aXJlKCcuL19kZXNjcmlwdG9ycycpICYmICFyZXF1aXJlKCcuL19mYWlscycpKGZ1bmN0aW9uKCl7XG4gIHJldHVybiBPYmplY3QuZGVmaW5lUHJvcGVydHkocmVxdWlyZSgnLi9fZG9tLWNyZWF0ZScpKCdkaXYnKSwgJ2EnLCB7Z2V0OiBmdW5jdGlvbigpeyByZXR1cm4gNzsgfX0pLmEgIT0gNztcbn0pOyIsIi8vIGZhbGxiYWNrIGZvciBub24tYXJyYXktbGlrZSBFUzMgYW5kIG5vbi1lbnVtZXJhYmxlIG9sZCBWOCBzdHJpbmdzXG52YXIgY29mID0gcmVxdWlyZSgnLi9fY29mJyk7XG5tb2R1bGUuZXhwb3J0cyA9IE9iamVjdCgneicpLnByb3BlcnR5SXNFbnVtZXJhYmxlKDApID8gT2JqZWN0IDogZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gY29mKGl0KSA9PSAnU3RyaW5nJyA/IGl0LnNwbGl0KCcnKSA6IE9iamVjdChpdCk7XG59OyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gdHlwZW9mIGl0ID09PSAnb2JqZWN0JyA/IGl0ICE9PSBudWxsIDogdHlwZW9mIGl0ID09PSAnZnVuY3Rpb24nO1xufTsiLCIndXNlIHN0cmljdCc7XG4vLyAxOS4xLjIuMSBPYmplY3QuYXNzaWduKHRhcmdldCwgc291cmNlLCAuLi4pXG52YXIgZ2V0S2V5cyAgPSByZXF1aXJlKCcuL19vYmplY3Qta2V5cycpXG4gICwgZ09QUyAgICAgPSByZXF1aXJlKCcuL19vYmplY3QtZ29wcycpXG4gICwgcElFICAgICAgPSByZXF1aXJlKCcuL19vYmplY3QtcGllJylcbiAgLCB0b09iamVjdCA9IHJlcXVpcmUoJy4vX3RvLW9iamVjdCcpXG4gICwgSU9iamVjdCAgPSByZXF1aXJlKCcuL19pb2JqZWN0JylcbiAgLCAkYXNzaWduICA9IE9iamVjdC5hc3NpZ247XG5cbi8vIHNob3VsZCB3b3JrIHdpdGggc3ltYm9scyBhbmQgc2hvdWxkIGhhdmUgZGV0ZXJtaW5pc3RpYyBwcm9wZXJ0eSBvcmRlciAoVjggYnVnKVxubW9kdWxlLmV4cG9ydHMgPSAhJGFzc2lnbiB8fCByZXF1aXJlKCcuL19mYWlscycpKGZ1bmN0aW9uKCl7XG4gIHZhciBBID0ge31cbiAgICAsIEIgPSB7fVxuICAgICwgUyA9IFN5bWJvbCgpXG4gICAgLCBLID0gJ2FiY2RlZmdoaWprbG1ub3BxcnN0JztcbiAgQVtTXSA9IDc7XG4gIEsuc3BsaXQoJycpLmZvckVhY2goZnVuY3Rpb24oayl7IEJba10gPSBrOyB9KTtcbiAgcmV0dXJuICRhc3NpZ24oe30sIEEpW1NdICE9IDcgfHwgT2JqZWN0LmtleXMoJGFzc2lnbih7fSwgQikpLmpvaW4oJycpICE9IEs7XG59KSA/IGZ1bmN0aW9uIGFzc2lnbih0YXJnZXQsIHNvdXJjZSl7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW51c2VkLXZhcnNcbiAgdmFyIFQgICAgID0gdG9PYmplY3QodGFyZ2V0KVxuICAgICwgYUxlbiAgPSBhcmd1bWVudHMubGVuZ3RoXG4gICAgLCBpbmRleCA9IDFcbiAgICAsIGdldFN5bWJvbHMgPSBnT1BTLmZcbiAgICAsIGlzRW51bSAgICAgPSBwSUUuZjtcbiAgd2hpbGUoYUxlbiA+IGluZGV4KXtcbiAgICB2YXIgUyAgICAgID0gSU9iamVjdChhcmd1bWVudHNbaW5kZXgrK10pXG4gICAgICAsIGtleXMgICA9IGdldFN5bWJvbHMgPyBnZXRLZXlzKFMpLmNvbmNhdChnZXRTeW1ib2xzKFMpKSA6IGdldEtleXMoUylcbiAgICAgICwgbGVuZ3RoID0ga2V5cy5sZW5ndGhcbiAgICAgICwgaiAgICAgID0gMFxuICAgICAgLCBrZXk7XG4gICAgd2hpbGUobGVuZ3RoID4gailpZihpc0VudW0uY2FsbChTLCBrZXkgPSBrZXlzW2orK10pKVRba2V5XSA9IFNba2V5XTtcbiAgfSByZXR1cm4gVDtcbn0gOiAkYXNzaWduOyIsInZhciBhbk9iamVjdCAgICAgICA9IHJlcXVpcmUoJy4vX2FuLW9iamVjdCcpXG4gICwgSUU4X0RPTV9ERUZJTkUgPSByZXF1aXJlKCcuL19pZTgtZG9tLWRlZmluZScpXG4gICwgdG9QcmltaXRpdmUgICAgPSByZXF1aXJlKCcuL190by1wcmltaXRpdmUnKVxuICAsIGRQICAgICAgICAgICAgID0gT2JqZWN0LmRlZmluZVByb3BlcnR5O1xuXG5leHBvcnRzLmYgPSByZXF1aXJlKCcuL19kZXNjcmlwdG9ycycpID8gT2JqZWN0LmRlZmluZVByb3BlcnR5IDogZnVuY3Rpb24gZGVmaW5lUHJvcGVydHkoTywgUCwgQXR0cmlidXRlcyl7XG4gIGFuT2JqZWN0KE8pO1xuICBQID0gdG9QcmltaXRpdmUoUCwgdHJ1ZSk7XG4gIGFuT2JqZWN0KEF0dHJpYnV0ZXMpO1xuICBpZihJRThfRE9NX0RFRklORSl0cnkge1xuICAgIHJldHVybiBkUChPLCBQLCBBdHRyaWJ1dGVzKTtcbiAgfSBjYXRjaChlKXsgLyogZW1wdHkgKi8gfVxuICBpZignZ2V0JyBpbiBBdHRyaWJ1dGVzIHx8ICdzZXQnIGluIEF0dHJpYnV0ZXMpdGhyb3cgVHlwZUVycm9yKCdBY2Nlc3NvcnMgbm90IHN1cHBvcnRlZCEnKTtcbiAgaWYoJ3ZhbHVlJyBpbiBBdHRyaWJ1dGVzKU9bUF0gPSBBdHRyaWJ1dGVzLnZhbHVlO1xuICByZXR1cm4gTztcbn07IiwiZXhwb3J0cy5mID0gT2JqZWN0LmdldE93blByb3BlcnR5U3ltYm9sczsiLCJ2YXIgaGFzICAgICAgICAgID0gcmVxdWlyZSgnLi9faGFzJylcbiAgLCB0b0lPYmplY3QgICAgPSByZXF1aXJlKCcuL190by1pb2JqZWN0JylcbiAgLCBhcnJheUluZGV4T2YgPSByZXF1aXJlKCcuL19hcnJheS1pbmNsdWRlcycpKGZhbHNlKVxuICAsIElFX1BST1RPICAgICA9IHJlcXVpcmUoJy4vX3NoYXJlZC1rZXknKSgnSUVfUFJPVE8nKTtcblxubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihvYmplY3QsIG5hbWVzKXtcbiAgdmFyIE8gICAgICA9IHRvSU9iamVjdChvYmplY3QpXG4gICAgLCBpICAgICAgPSAwXG4gICAgLCByZXN1bHQgPSBbXVxuICAgICwga2V5O1xuICBmb3Ioa2V5IGluIE8paWYoa2V5ICE9IElFX1BST1RPKWhhcyhPLCBrZXkpICYmIHJlc3VsdC5wdXNoKGtleSk7XG4gIC8vIERvbid0IGVudW0gYnVnICYgaGlkZGVuIGtleXNcbiAgd2hpbGUobmFtZXMubGVuZ3RoID4gaSlpZihoYXMoTywga2V5ID0gbmFtZXNbaSsrXSkpe1xuICAgIH5hcnJheUluZGV4T2YocmVzdWx0LCBrZXkpIHx8IHJlc3VsdC5wdXNoKGtleSk7XG4gIH1cbiAgcmV0dXJuIHJlc3VsdDtcbn07IiwiLy8gMTkuMS4yLjE0IC8gMTUuMi4zLjE0IE9iamVjdC5rZXlzKE8pXG52YXIgJGtleXMgICAgICAgPSByZXF1aXJlKCcuL19vYmplY3Qta2V5cy1pbnRlcm5hbCcpXG4gICwgZW51bUJ1Z0tleXMgPSByZXF1aXJlKCcuL19lbnVtLWJ1Zy1rZXlzJyk7XG5cbm1vZHVsZS5leHBvcnRzID0gT2JqZWN0LmtleXMgfHwgZnVuY3Rpb24ga2V5cyhPKXtcbiAgcmV0dXJuICRrZXlzKE8sIGVudW1CdWdLZXlzKTtcbn07IiwiZXhwb3J0cy5mID0ge30ucHJvcGVydHlJc0VudW1lcmFibGU7IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihiaXRtYXAsIHZhbHVlKXtcbiAgcmV0dXJuIHtcbiAgICBlbnVtZXJhYmxlICA6ICEoYml0bWFwICYgMSksXG4gICAgY29uZmlndXJhYmxlOiAhKGJpdG1hcCAmIDIpLFxuICAgIHdyaXRhYmxlICAgIDogIShiaXRtYXAgJiA0KSxcbiAgICB2YWx1ZSAgICAgICA6IHZhbHVlXG4gIH07XG59OyIsInZhciBzaGFyZWQgPSByZXF1aXJlKCcuL19zaGFyZWQnKSgna2V5cycpXG4gICwgdWlkICAgID0gcmVxdWlyZSgnLi9fdWlkJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGtleSl7XG4gIHJldHVybiBzaGFyZWRba2V5XSB8fCAoc2hhcmVkW2tleV0gPSB1aWQoa2V5KSk7XG59OyIsInZhciBnbG9iYWwgPSByZXF1aXJlKCcuL19nbG9iYWwnKVxuICAsIFNIQVJFRCA9ICdfX2NvcmUtanNfc2hhcmVkX18nXG4gICwgc3RvcmUgID0gZ2xvYmFsW1NIQVJFRF0gfHwgKGdsb2JhbFtTSEFSRURdID0ge30pO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihrZXkpe1xuICByZXR1cm4gc3RvcmVba2V5XSB8fCAoc3RvcmVba2V5XSA9IHt9KTtcbn07IiwidmFyIHRvSW50ZWdlciA9IHJlcXVpcmUoJy4vX3RvLWludGVnZXInKVxuICAsIG1heCAgICAgICA9IE1hdGgubWF4XG4gICwgbWluICAgICAgID0gTWF0aC5taW47XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGluZGV4LCBsZW5ndGgpe1xuICBpbmRleCA9IHRvSW50ZWdlcihpbmRleCk7XG4gIHJldHVybiBpbmRleCA8IDAgPyBtYXgoaW5kZXggKyBsZW5ndGgsIDApIDogbWluKGluZGV4LCBsZW5ndGgpO1xufTsiLCIvLyA3LjEuNCBUb0ludGVnZXJcbnZhciBjZWlsICA9IE1hdGguY2VpbFxuICAsIGZsb29yID0gTWF0aC5mbG9vcjtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gaXNOYU4oaXQgPSAraXQpID8gMCA6IChpdCA+IDAgPyBmbG9vciA6IGNlaWwpKGl0KTtcbn07IiwiLy8gdG8gaW5kZXhlZCBvYmplY3QsIHRvT2JqZWN0IHdpdGggZmFsbGJhY2sgZm9yIG5vbi1hcnJheS1saWtlIEVTMyBzdHJpbmdzXG52YXIgSU9iamVjdCA9IHJlcXVpcmUoJy4vX2lvYmplY3QnKVxuICAsIGRlZmluZWQgPSByZXF1aXJlKCcuL19kZWZpbmVkJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIElPYmplY3QoZGVmaW5lZChpdCkpO1xufTsiLCIvLyA3LjEuMTUgVG9MZW5ndGhcbnZhciB0b0ludGVnZXIgPSByZXF1aXJlKCcuL190by1pbnRlZ2VyJylcbiAgLCBtaW4gICAgICAgPSBNYXRoLm1pbjtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gaXQgPiAwID8gbWluKHRvSW50ZWdlcihpdCksIDB4MWZmZmZmZmZmZmZmZmYpIDogMDsgLy8gcG93KDIsIDUzKSAtIDEgPT0gOTAwNzE5OTI1NDc0MDk5MVxufTsiLCIvLyA3LjEuMTMgVG9PYmplY3QoYXJndW1lbnQpXG52YXIgZGVmaW5lZCA9IHJlcXVpcmUoJy4vX2RlZmluZWQnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gT2JqZWN0KGRlZmluZWQoaXQpKTtcbn07IiwiLy8gNy4xLjEgVG9QcmltaXRpdmUoaW5wdXQgWywgUHJlZmVycmVkVHlwZV0pXG52YXIgaXNPYmplY3QgPSByZXF1aXJlKCcuL19pcy1vYmplY3QnKTtcbi8vIGluc3RlYWQgb2YgdGhlIEVTNiBzcGVjIHZlcnNpb24sIHdlIGRpZG4ndCBpbXBsZW1lbnQgQEB0b1ByaW1pdGl2ZSBjYXNlXG4vLyBhbmQgdGhlIHNlY29uZCBhcmd1bWVudCAtIGZsYWcgLSBwcmVmZXJyZWQgdHlwZSBpcyBhIHN0cmluZ1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCwgUyl7XG4gIGlmKCFpc09iamVjdChpdCkpcmV0dXJuIGl0O1xuICB2YXIgZm4sIHZhbDtcbiAgaWYoUyAmJiB0eXBlb2YgKGZuID0gaXQudG9TdHJpbmcpID09ICdmdW5jdGlvbicgJiYgIWlzT2JqZWN0KHZhbCA9IGZuLmNhbGwoaXQpKSlyZXR1cm4gdmFsO1xuICBpZih0eXBlb2YgKGZuID0gaXQudmFsdWVPZikgPT0gJ2Z1bmN0aW9uJyAmJiAhaXNPYmplY3QodmFsID0gZm4uY2FsbChpdCkpKXJldHVybiB2YWw7XG4gIGlmKCFTICYmIHR5cGVvZiAoZm4gPSBpdC50b1N0cmluZykgPT0gJ2Z1bmN0aW9uJyAmJiAhaXNPYmplY3QodmFsID0gZm4uY2FsbChpdCkpKXJldHVybiB2YWw7XG4gIHRocm93IFR5cGVFcnJvcihcIkNhbid0IGNvbnZlcnQgb2JqZWN0IHRvIHByaW1pdGl2ZSB2YWx1ZVwiKTtcbn07IiwidmFyIGlkID0gMFxuICAsIHB4ID0gTWF0aC5yYW5kb20oKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oa2V5KXtcbiAgcmV0dXJuICdTeW1ib2woJy5jb25jYXQoa2V5ID09PSB1bmRlZmluZWQgPyAnJyA6IGtleSwgJylfJywgKCsraWQgKyBweCkudG9TdHJpbmcoMzYpKTtcbn07IiwiLy8gMTkuMS4zLjEgT2JqZWN0LmFzc2lnbih0YXJnZXQsIHNvdXJjZSlcbnZhciAkZXhwb3J0ID0gcmVxdWlyZSgnLi9fZXhwb3J0Jyk7XG5cbiRleHBvcnQoJGV4cG9ydC5TICsgJGV4cG9ydC5GLCAnT2JqZWN0Jywge2Fzc2lnbjogcmVxdWlyZSgnLi9fb2JqZWN0LWFzc2lnbicpfSk7IiwidmFyIF9leHRlbmRzID0gT2JqZWN0LmFzc2lnbiB8fCBmdW5jdGlvbiAodGFyZ2V0KSB7IGZvciAodmFyIGkgPSAxOyBpIDwgYXJndW1lbnRzLmxlbmd0aDsgaSsrKSB7IHZhciBzb3VyY2UgPSBhcmd1bWVudHNbaV07IGZvciAodmFyIGtleSBpbiBzb3VyY2UpIHsgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChzb3VyY2UsIGtleSkpIHsgdGFyZ2V0W2tleV0gPSBzb3VyY2Vba2V5XTsgfSB9IH0gcmV0dXJuIHRhcmdldDsgfTtcblxudmFyIF90eXBlb2YgPSB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgdHlwZW9mIFN5bWJvbC5pdGVyYXRvciA9PT0gXCJzeW1ib2xcIiA/IGZ1bmN0aW9uIChvYmopIHsgcmV0dXJuIHR5cGVvZiBvYmo7IH0gOiBmdW5jdGlvbiAob2JqKSB7IHJldHVybiBvYmogJiYgdHlwZW9mIFN5bWJvbCA9PT0gXCJmdW5jdGlvblwiICYmIG9iai5jb25zdHJ1Y3RvciA9PT0gU3ltYm9sICYmIG9iaiAhPT0gU3ltYm9sLnByb3RvdHlwZSA/IFwic3ltYm9sXCIgOiB0eXBlb2Ygb2JqOyB9O1xuXG4vKiEgZmxhdHBpY2tyIHYyLjQuNCwgQGxpY2Vuc2UgTUlUICovXG5mdW5jdGlvbiBGbGF0cGlja3IoZWxlbWVudCwgY29uZmlnKSB7XG5cdHZhciBzZWxmID0gdGhpcztcblxuXHRzZWxmLmNoYW5nZU1vbnRoID0gY2hhbmdlTW9udGg7XG5cdHNlbGYuY2hhbmdlWWVhciA9IGNoYW5nZVllYXI7XG5cdHNlbGYuY2xlYXIgPSBjbGVhcjtcblx0c2VsZi5jbG9zZSA9IGNsb3NlO1xuXHRzZWxmLl9jcmVhdGVFbGVtZW50ID0gY3JlYXRlRWxlbWVudDtcblx0c2VsZi5kZXN0cm95ID0gZGVzdHJveTtcblx0c2VsZi5mb3JtYXREYXRlID0gZm9ybWF0RGF0ZTtcblx0c2VsZi5pc0VuYWJsZWQgPSBpc0VuYWJsZWQ7XG5cdHNlbGYuanVtcFRvRGF0ZSA9IGp1bXBUb0RhdGU7XG5cdHNlbGYub3BlbiA9IG9wZW47XG5cdHNlbGYucmVkcmF3ID0gcmVkcmF3O1xuXHRzZWxmLnNldCA9IHNldDtcblx0c2VsZi5zZXREYXRlID0gc2V0RGF0ZTtcblx0c2VsZi50b2dnbGUgPSB0b2dnbGU7XG5cblx0ZnVuY3Rpb24gaW5pdCgpIHtcblx0XHRpZiAoZWxlbWVudC5fZmxhdHBpY2tyKSBkZXN0cm95KGVsZW1lbnQuX2ZsYXRwaWNrcik7XG5cblx0XHRlbGVtZW50Ll9mbGF0cGlja3IgPSBzZWxmO1xuXG5cdFx0c2VsZi5lbGVtZW50ID0gZWxlbWVudDtcblx0XHRzZWxmLmluc3RhbmNlQ29uZmlnID0gY29uZmlnIHx8IHt9O1xuXHRcdHNlbGYucGFyc2VEYXRlID0gRmxhdHBpY2tyLnByb3RvdHlwZS5wYXJzZURhdGUuYmluZChzZWxmKTtcblxuXHRcdHNldHVwRm9ybWF0cygpO1xuXHRcdHBhcnNlQ29uZmlnKCk7XG5cdFx0c2V0dXBMb2NhbGUoKTtcblx0XHRzZXR1cElucHV0cygpO1xuXHRcdHNldHVwRGF0ZXMoKTtcblx0XHRzZXR1cEhlbHBlckZ1bmN0aW9ucygpO1xuXG5cdFx0c2VsZi5pc09wZW4gPSBzZWxmLmNvbmZpZy5pbmxpbmU7XG5cblx0XHRzZWxmLmlzTW9iaWxlID0gIXNlbGYuY29uZmlnLmRpc2FibGVNb2JpbGUgJiYgIXNlbGYuY29uZmlnLmlubGluZSAmJiBzZWxmLmNvbmZpZy5tb2RlID09PSBcInNpbmdsZVwiICYmICFzZWxmLmNvbmZpZy5kaXNhYmxlLmxlbmd0aCAmJiAhc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCAmJiAhc2VsZi5jb25maWcud2Vla051bWJlcnMgJiYgL0FuZHJvaWR8d2ViT1N8aVBob25lfGlQYWR8aVBvZHxCbGFja0JlcnJ5fElFTW9iaWxlfE9wZXJhIE1pbmkvaS50ZXN0KG5hdmlnYXRvci51c2VyQWdlbnQpO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSBidWlsZCgpO1xuXG5cdFx0YmluZCgpO1xuXG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHtcblx0XHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSBzZXRIb3Vyc0Zyb21EYXRlKCk7XG5cdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycykge1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS53aWR0aCA9IHNlbGYuZGF5cy5jbGllbnRXaWR0aCArIHNlbGYud2Vla1dyYXBwZXIuY2xpZW50V2lkdGggKyBcInB4XCI7XG5cdFx0fVxuXG5cdFx0c2VsZi5zaG93VGltZUlucHV0ID0gc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCB8fCBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyO1xuXG5cdFx0aWYgKCFzZWxmLmlzTW9iaWxlKSBwb3NpdGlvbkNhbGVuZGFyKCk7XG5cdFx0dHJpZ2dlckV2ZW50KFwiUmVhZHlcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBiaW5kVG9JbnN0YW5jZShmbikge1xuXHRcdGlmIChmbiAmJiBmbi5iaW5kKSByZXR1cm4gZm4uYmluZChzZWxmKTtcblx0XHRyZXR1cm4gZm47XG5cdH1cblxuXHRmdW5jdGlvbiB1cGRhdGVUaW1lKGUpIHtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhciAmJiAhc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aClcblx0XHRcdC8vIHBpY2tpbmcgdGltZSBvbmx5XG5cdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZi5ub3ddO1xuXG5cdFx0dGltZVdyYXBwZXIoZSk7XG5cblx0XHRpZiAoIXNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHJldHVybjtcblxuXHRcdGlmICghc2VsZi5taW5EYXRlSGFzVGltZSB8fCBlLnR5cGUgIT09IFwiaW5wdXRcIiB8fCBlLnRhcmdldC52YWx1ZS5sZW5ndGggPj0gMikge1xuXHRcdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdH0gZWxzZSB7XG5cdFx0XHRzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0c2V0SG91cnNGcm9tSW5wdXRzKCk7XG5cdFx0XHRcdHVwZGF0ZVZhbHVlKCk7XG5cdFx0XHR9LCAxMDAwKTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBzZXRIb3Vyc0Zyb21JbnB1dHMoKSB7XG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSByZXR1cm47XG5cblx0XHR2YXIgaG91cnMgPSBwYXJzZUludChzZWxmLmhvdXJFbGVtZW50LnZhbHVlLCAxMCkgfHwgMCxcblx0XHQgICAgbWludXRlcyA9IHBhcnNlSW50KHNlbGYubWludXRlRWxlbWVudC52YWx1ZSwgMTApIHx8IDAsXG5cdFx0ICAgIHNlY29uZHMgPSBzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gcGFyc2VJbnQoc2VsZi5zZWNvbmRFbGVtZW50LnZhbHVlLCAxMCkgfHwgMCA6IDA7XG5cblx0XHRpZiAoc2VsZi5hbVBNKSBob3VycyA9IGhvdXJzICUgMTIgKyAxMiAqIChzZWxmLmFtUE0udGV4dENvbnRlbnQgPT09IFwiUE1cIik7XG5cblx0XHRpZiAoc2VsZi5taW5EYXRlSGFzVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmosIHNlbGYuY29uZmlnLm1pbkRhdGUpID09PSAwKSB7XG5cblx0XHRcdGhvdXJzID0gTWF0aC5tYXgoaG91cnMsIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSk7XG5cdFx0XHRpZiAoaG91cnMgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0SG91cnMoKSkgbWludXRlcyA9IE1hdGgubWF4KG1pbnV0ZXMsIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0TWludXRlcygpKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5tYXhEYXRlSGFzVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmosIHNlbGYuY29uZmlnLm1heERhdGUpID09PSAwKSB7XG5cdFx0XHRob3VycyA9IE1hdGgubWluKGhvdXJzLCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkpO1xuXHRcdFx0aWYgKGhvdXJzID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEhvdXJzKCkpIG1pbnV0ZXMgPSBNYXRoLm1pbihtaW51dGVzLCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1pbnV0ZXMoKSk7XG5cdFx0fVxuXG5cdFx0c2V0SG91cnMoaG91cnMsIG1pbnV0ZXMsIHNlY29uZHMpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0SG91cnNGcm9tRGF0ZShkYXRlT2JqKSB7XG5cdFx0dmFyIGRhdGUgPSBkYXRlT2JqIHx8IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqO1xuXG5cdFx0aWYgKGRhdGUpIHNldEhvdXJzKGRhdGUuZ2V0SG91cnMoKSwgZGF0ZS5nZXRNaW51dGVzKCksIGRhdGUuZ2V0U2Vjb25kcygpKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldEhvdXJzKGhvdXJzLCBtaW51dGVzLCBzZWNvbmRzKSB7XG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGgpIHtcblx0XHRcdHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqLnNldEhvdXJzKGhvdXJzICUgMjQsIG1pbnV0ZXMsIHNlY29uZHMgfHwgMCwgMCk7XG5cdFx0fVxuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5lbmFibGVUaW1lIHx8IHNlbGYuaXNNb2JpbGUpIHJldHVybjtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZCghc2VsZi5jb25maWcudGltZV8yNGhyID8gKDEyICsgaG91cnMpICUgMTIgKyAxMiAqIChob3VycyAlIDEyID09PSAwKSA6IGhvdXJzKTtcblxuXHRcdHNlbGYubWludXRlRWxlbWVudC52YWx1ZSA9IHNlbGYucGFkKG1pbnV0ZXMpO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy50aW1lXzI0aHIgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkgc2VsZi5hbVBNLnRleHRDb250ZW50ID0gc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouZ2V0SG91cnMoKSA+PSAxMiA/IFwiUE1cIiA6IFwiQU1cIjtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzKSBzZWxmLnNlY29uZEVsZW1lbnQudmFsdWUgPSBzZWxmLnBhZChzZWNvbmRzKTtcblx0fVxuXG5cdGZ1bmN0aW9uIG9uWWVhcklucHV0KGV2ZW50KSB7XG5cdFx0dmFyIHllYXIgPSBldmVudC50YXJnZXQudmFsdWU7XG5cdFx0aWYgKGV2ZW50LmRlbHRhKSB5ZWFyID0gKHBhcnNlSW50KHllYXIpICsgZXZlbnQuZGVsdGEpLnRvU3RyaW5nKCk7XG5cblx0XHRpZiAoeWVhci5sZW5ndGggPT09IDQpIHtcblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmJsdXIoKTtcblx0XHRcdGlmICghL1teXFxkXS8udGVzdCh5ZWFyKSkgY2hhbmdlWWVhcih5ZWFyKTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBvbk1vbnRoU2Nyb2xsKGUpIHtcblx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0c2VsZi5jaGFuZ2VNb250aChNYXRoLm1heCgtMSwgTWF0aC5taW4oMSwgZS53aGVlbERlbHRhIHx8IC1lLmRlbHRhWSkpKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGJpbmQoKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLndyYXApIHtcblx0XHRcdFtcIm9wZW5cIiwgXCJjbG9zZVwiLCBcInRvZ2dsZVwiLCBcImNsZWFyXCJdLmZvckVhY2goZnVuY3Rpb24gKGVsKSB7XG5cdFx0XHRcdHZhciB0b2dnbGVzID0gc2VsZi5lbGVtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCJbZGF0YS1cIiArIGVsICsgXCJdXCIpO1xuXHRcdFx0XHRmb3IgKHZhciBpID0gMDsgaSA8IHRvZ2dsZXMubGVuZ3RoOyBpKyspIHtcblx0XHRcdFx0XHR0b2dnbGVzW2ldLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBzZWxmW2VsXSk7XG5cdFx0XHRcdH1cblx0XHRcdH0pO1xuXHRcdH1cblxuXHRcdGlmICh3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRXZlbnQgIT09IHVuZGVmaW5lZCkge1xuXHRcdFx0c2VsZi5jaGFuZ2VFdmVudCA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFdmVudChcIkhUTUxFdmVudHNcIik7XG5cdFx0XHRzZWxmLmNoYW5nZUV2ZW50LmluaXRFdmVudChcImNoYW5nZVwiLCBmYWxzZSwgdHJ1ZSk7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuaXNNb2JpbGUpIHJldHVybiBzZXR1cE1vYmlsZSgpO1xuXG5cdFx0c2VsZi5kZWJvdW5jZWRSZXNpemUgPSBkZWJvdW5jZShvblJlc2l6ZSwgNTApO1xuXHRcdHNlbGYudHJpZ2dlckNoYW5nZSA9IGZ1bmN0aW9uICgpIHtcblx0XHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0XHR9O1xuXHRcdHNlbGYuZGVib3VuY2VkQ2hhbmdlID0gZGVib3VuY2Uoc2VsZi50cmlnZ2VyQ2hhbmdlLCAzMDApO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIiAmJiBzZWxmLmRheXMpIHNlbGYuZGF5cy5hZGRFdmVudExpc3RlbmVyKFwibW91c2VvdmVyXCIsIG9uTW91c2VPdmVyKTtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcImtleWRvd25cIiwgb25LZXlEb3duKTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuc3RhdGljICYmIHNlbGYuY29uZmlnLmFsbG93SW5wdXQpIChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLmFkZEV2ZW50TGlzdGVuZXIoXCJrZXlkb3duXCIsIG9uS2V5RG93bik7XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmlubGluZSAmJiAhc2VsZi5jb25maWcuc3RhdGljKSB3aW5kb3cuYWRkRXZlbnRMaXN0ZW5lcihcInJlc2l6ZVwiLCBzZWxmLmRlYm91bmNlZFJlc2l6ZSk7XG5cblx0XHRpZiAod2luZG93Lm9udG91Y2hzdGFydCkgd2luZG93LmRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJ0b3VjaHN0YXJ0XCIsIGRvY3VtZW50Q2xpY2spO1xuXG5cdFx0d2luZG93LmRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBkb2N1bWVudENsaWNrKTtcblx0XHR3aW5kb3cuZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImJsdXJcIiwgZG9jdW1lbnRDbGljayk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuY2xpY2tPcGVucykgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuYWRkRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIG9wZW4pO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSB7XG5cdFx0XHRzZWxmLnByZXZNb250aE5hdi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRyZXR1cm4gY2hhbmdlTW9udGgoLTEpO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLm5leHRNb250aE5hdi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRyZXR1cm4gY2hhbmdlTW9udGgoMSk7XG5cdFx0XHR9KTtcblxuXHRcdFx0c2VsZi5jdXJyZW50TW9udGhFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJ3aGVlbFwiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0XHRyZXR1cm4gZGVib3VuY2Uob25Nb250aFNjcm9sbChlKSwgNTApO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwid2hlZWxcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdFx0cmV0dXJuIGRlYm91bmNlKHllYXJTY3JvbGwoZSksIDUwKTtcblx0XHRcdH0pO1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuc2VsZWN0KCk7XG5cdFx0XHR9KTtcblxuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImlucHV0XCIsIG9uWWVhcklucHV0KTtcblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJpbmNyZW1lbnRcIiwgb25ZZWFySW5wdXQpO1xuXG5cdFx0XHRzZWxmLmRheXMuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIHNlbGVjdERhdGUpO1xuXHRcdH1cblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSB7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcInRyYW5zaXRpb25lbmRcIiwgcG9zaXRpb25DYWxlbmRhcik7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcIndoZWVsXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRcdHJldHVybiBkZWJvdW5jZSh1cGRhdGVUaW1lKGUpLCA1KTtcblx0XHRcdH0pO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJpbnB1dFwiLCB1cGRhdGVUaW1lKTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwiaW5jcmVtZW50XCIsIHVwZGF0ZVRpbWUpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFkZEV2ZW50TGlzdGVuZXIoXCJpbmNyZW1lbnRcIiwgc2VsZi5kZWJvdW5jZWRDaGFuZ2UpO1xuXG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuYWRkRXZlbnRMaXN0ZW5lcihcIndoZWVsXCIsIHNlbGYuZGVib3VuY2VkQ2hhbmdlKTtcblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hZGRFdmVudExpc3RlbmVyKFwiaW5wdXRcIiwgc2VsZi50cmlnZ2VyQ2hhbmdlKTtcblxuXHRcdFx0c2VsZi5ob3VyRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiZm9jdXNcIiwgZnVuY3Rpb24gKCkge1xuXHRcdFx0XHRzZWxmLmhvdXJFbGVtZW50LnNlbGVjdCgpO1xuXHRcdFx0fSk7XG5cdFx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIGZ1bmN0aW9uICgpIHtcblx0XHRcdFx0c2VsZi5taW51dGVFbGVtZW50LnNlbGVjdCgpO1xuXHRcdFx0fSk7XG5cblx0XHRcdGlmIChzZWxmLnNlY29uZEVsZW1lbnQpIHtcblx0XHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJmb2N1c1wiLCBmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50LnNlbGVjdCgpO1xuXHRcdFx0XHR9KTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuYW1QTSkge1xuXHRcdFx0XHRzZWxmLmFtUE0uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRcdFx0dXBkYXRlVGltZShlKTtcblx0XHRcdFx0XHRzZWxmLnRyaWdnZXJDaGFuZ2UoZSk7XG5cdFx0XHRcdH0pO1xuXHRcdFx0fVxuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGp1bXBUb0RhdGUoanVtcERhdGUpIHtcblx0XHRqdW1wRGF0ZSA9IGp1bXBEYXRlID8gc2VsZi5wYXJzZURhdGUoanVtcERhdGUpIDogc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogfHwgKHNlbGYuY29uZmlnLm1pbkRhdGUgPiBzZWxmLm5vdyA/IHNlbGYuY29uZmlnLm1pbkRhdGUgOiBzZWxmLmNvbmZpZy5tYXhEYXRlICYmIHNlbGYuY29uZmlnLm1heERhdGUgPCBzZWxmLm5vdyA/IHNlbGYuY29uZmlnLm1heERhdGUgOiBzZWxmLm5vdyk7XG5cblx0XHR0cnkge1xuXHRcdFx0c2VsZi5jdXJyZW50WWVhciA9IGp1bXBEYXRlLmdldEZ1bGxZZWFyKCk7XG5cdFx0XHRzZWxmLmN1cnJlbnRNb250aCA9IGp1bXBEYXRlLmdldE1vbnRoKCk7XG5cdFx0fSBjYXRjaCAoZSkge1xuXHRcdFx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0XHRcdGNvbnNvbGUuZXJyb3IoZS5zdGFjayk7XG5cdFx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdFx0Y29uc29sZS53YXJuKFwiSW52YWxpZCBkYXRlIHN1cHBsaWVkOiBcIiArIGp1bXBEYXRlKTtcblx0XHR9XG5cblx0XHRzZWxmLnJlZHJhdygpO1xuXHR9XG5cblx0ZnVuY3Rpb24gaW5jcmVtZW50TnVtSW5wdXQoZSwgZGVsdGEsIGlucHV0RWxlbSkge1xuXHRcdHZhciBpbnB1dCA9IGlucHV0RWxlbSB8fCBlLnRhcmdldC5wYXJlbnROb2RlLmNoaWxkTm9kZXNbMF07XG5cblx0XHRpZiAodHlwZW9mIEV2ZW50ICE9PSBcInVuZGVmaW5lZFwiKSB7XG5cdFx0XHR2YXIgZXYgPSBuZXcgRXZlbnQoXCJpbmNyZW1lbnRcIiwgeyBcImJ1YmJsZXNcIjogdHJ1ZSB9KTtcblx0XHRcdGV2LmRlbHRhID0gZGVsdGE7XG5cdFx0XHRpbnB1dC5kaXNwYXRjaEV2ZW50KGV2KTtcblx0XHR9IGVsc2Uge1xuXHRcdFx0dmFyIF9ldiA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFdmVudChcIkN1c3RvbUV2ZW50XCIpO1xuXHRcdFx0X2V2LmluaXRDdXN0b21FdmVudChcImluY3JlbWVudFwiLCB0cnVlLCB0cnVlLCB7fSk7XG5cdFx0XHRfZXYuZGVsdGEgPSBkZWx0YTtcblx0XHRcdGlucHV0LmRpc3BhdGNoRXZlbnQoX2V2KTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBjcmVhdGVOdW1iZXJJbnB1dChpbnB1dENsYXNzTmFtZSkge1xuXHRcdHZhciB3cmFwcGVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcIm51bUlucHV0V3JhcHBlclwiKSxcblx0XHQgICAgbnVtSW5wdXQgPSBjcmVhdGVFbGVtZW50KFwiaW5wdXRcIiwgXCJudW1JbnB1dCBcIiArIGlucHV0Q2xhc3NOYW1lKSxcblx0XHQgICAgYXJyb3dVcCA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiYXJyb3dVcFwiKSxcblx0XHQgICAgYXJyb3dEb3duID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJhcnJvd0Rvd25cIik7XG5cblx0XHRudW1JbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0bnVtSW5wdXQucGF0dGVybiA9IFwiXFxcXGQqXCI7XG5cdFx0d3JhcHBlci5hcHBlbmRDaGlsZChudW1JbnB1dCk7XG5cdFx0d3JhcHBlci5hcHBlbmRDaGlsZChhcnJvd1VwKTtcblx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKGFycm93RG93bik7XG5cblx0XHRhcnJvd1VwLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBmdW5jdGlvbiAoZSkge1xuXHRcdFx0cmV0dXJuIGluY3JlbWVudE51bUlucHV0KGUsIDEpO1xuXHRcdH0pO1xuXHRcdGFycm93RG93bi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgZnVuY3Rpb24gKGUpIHtcblx0XHRcdHJldHVybiBpbmNyZW1lbnROdW1JbnB1dChlLCAtMSk7XG5cdFx0fSk7XG5cdFx0cmV0dXJuIHdyYXBwZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZCgpIHtcblx0XHR2YXIgZnJhZ21lbnQgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRG9jdW1lbnRGcmFnbWVudCgpO1xuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLWNhbGVuZGFyXCIpO1xuXHRcdHNlbGYubnVtSW5wdXRUeXBlID0gbmF2aWdhdG9yLnVzZXJBZ2VudC5pbmRleE9mKFwiTVNJRSA5LjBcIikgPiAwID8gXCJ0ZXh0XCIgOiBcIm51bWJlclwiO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSB7XG5cdFx0XHRmcmFnbWVudC5hcHBlbmRDaGlsZChidWlsZE1vbnRoTmF2KCkpO1xuXHRcdFx0c2VsZi5pbm5lckNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3ItaW5uZXJDb250YWluZXJcIik7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy53ZWVrTnVtYmVycykgc2VsZi5pbm5lckNvbnRhaW5lci5hcHBlbmRDaGlsZChidWlsZFdlZWtzKCkpO1xuXG5cdFx0XHRzZWxmLnJDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXJDb250YWluZXJcIik7XG5cdFx0XHRzZWxmLnJDb250YWluZXIuYXBwZW5kQ2hpbGQoYnVpbGRXZWVrZGF5cygpKTtcblxuXHRcdFx0aWYgKCFzZWxmLmRheXMpIHtcblx0XHRcdFx0c2VsZi5kYXlzID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci1kYXlzXCIpO1xuXHRcdFx0XHRzZWxmLmRheXMudGFiSW5kZXggPSAtMTtcblx0XHRcdH1cblxuXHRcdFx0YnVpbGREYXlzKCk7XG5cdFx0XHRzZWxmLnJDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VsZi5kYXlzKTtcblxuXHRcdFx0c2VsZi5pbm5lckNvbnRhaW5lci5hcHBlbmRDaGlsZChzZWxmLnJDb250YWluZXIpO1xuXHRcdFx0ZnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5pbm5lckNvbnRhaW5lcik7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZVRpbWUpIGZyYWdtZW50LmFwcGVuZENoaWxkKGJ1aWxkVGltZSgpKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChcInJhbmdlTW9kZVwiKTtcblxuXHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuYXBwZW5kQ2hpbGQoZnJhZ21lbnQpO1xuXG5cdFx0dmFyIGN1c3RvbUFwcGVuZCA9IHNlbGYuY29uZmlnLmFwcGVuZFRvICYmIHNlbGYuY29uZmlnLmFwcGVuZFRvLm5vZGVUeXBlO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmlubGluZSB8fCBzZWxmLmNvbmZpZy5zdGF0aWMpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LmFkZChzZWxmLmNvbmZpZy5pbmxpbmUgPyBcImlubGluZVwiIDogXCJzdGF0aWNcIik7XG5cdFx0XHRwb3NpdGlvbkNhbGVuZGFyKCk7XG5cblx0XHRcdGlmIChzZWxmLmNvbmZpZy5pbmxpbmUgJiYgIWN1c3RvbUFwcGVuZCkge1xuXHRcdFx0XHRyZXR1cm4gc2VsZi5lbGVtZW50LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLm5leHRTaWJsaW5nKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuY29uZmlnLnN0YXRpYykge1xuXHRcdFx0XHR2YXIgd3JhcHBlciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd3JhcHBlclwiKTtcblx0XHRcdFx0c2VsZi5lbGVtZW50LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHdyYXBwZXIsIHNlbGYuZWxlbWVudCk7XG5cdFx0XHRcdHdyYXBwZXIuYXBwZW5kQ2hpbGQoc2VsZi5lbGVtZW50KTtcblxuXHRcdFx0XHRpZiAoc2VsZi5hbHRJbnB1dCkgd3JhcHBlci5hcHBlbmRDaGlsZChzZWxmLmFsdElucHV0KTtcblxuXHRcdFx0XHR3cmFwcGVyLmFwcGVuZENoaWxkKHNlbGYuY2FsZW5kYXJDb250YWluZXIpO1xuXHRcdFx0XHRyZXR1cm47XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0KGN1c3RvbUFwcGVuZCA/IHNlbGYuY29uZmlnLmFwcGVuZFRvIDogd2luZG93LmRvY3VtZW50LmJvZHkpLmFwcGVuZENoaWxkKHNlbGYuY2FsZW5kYXJDb250YWluZXIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY3JlYXRlRGF5KGNsYXNzTmFtZSwgZGF0ZSwgZGF5TnVtYmVyKSB7XG5cdFx0dmFyIGRhdGVJc0VuYWJsZWQgPSBpc0VuYWJsZWQoZGF0ZSwgdHJ1ZSksXG5cdFx0ICAgIGRheUVsZW1lbnQgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1kYXkgXCIgKyBjbGFzc05hbWUsIGRhdGUuZ2V0RGF0ZSgpKTtcblxuXHRcdGRheUVsZW1lbnQuZGF0ZU9iaiA9IGRhdGU7XG5cblx0XHR0b2dnbGVDbGFzcyhkYXlFbGVtZW50LCBcInRvZGF5XCIsIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLm5vdykgPT09IDApO1xuXG5cdFx0aWYgKGRhdGVJc0VuYWJsZWQpIHtcblx0XHRcdGRheUVsZW1lbnQudGFiSW5kZXggPSAwO1xuXG5cdFx0XHRpZiAoaXNEYXRlU2VsZWN0ZWQoZGF0ZSkpIHtcblx0XHRcdFx0ZGF5RWxlbWVudC5jbGFzc0xpc3QuYWRkKFwic2VsZWN0ZWRcIik7XG5cdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlRWxlbSA9IGRheUVsZW1lbnQ7XG5cdFx0XHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdFx0XHR0b2dnbGVDbGFzcyhkYXlFbGVtZW50LCBcInN0YXJ0UmFuZ2VcIiwgY29tcGFyZURhdGVzKGRhdGUsIHNlbGYuc2VsZWN0ZWREYXRlc1swXSkgPT09IDApO1xuXG5cdFx0XHRcdFx0dG9nZ2xlQ2xhc3MoZGF5RWxlbWVudCwgXCJlbmRSYW5nZVwiLCBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5zZWxlY3RlZERhdGVzWzFdKSA9PT0gMCk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblx0XHR9IGVsc2Uge1xuXHRcdFx0ZGF5RWxlbWVudC5jbGFzc0xpc3QuYWRkKFwiZGlzYWJsZWRcIik7XG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzWzBdICYmIGRhdGUgPiBzZWxmLm1pblJhbmdlRGF0ZSAmJiBkYXRlIDwgc2VsZi5zZWxlY3RlZERhdGVzWzBdKSBzZWxmLm1pblJhbmdlRGF0ZSA9IGRhdGU7ZWxzZSBpZiAoc2VsZi5zZWxlY3RlZERhdGVzWzBdICYmIGRhdGUgPCBzZWxmLm1heFJhbmdlRGF0ZSAmJiBkYXRlID4gc2VsZi5zZWxlY3RlZERhdGVzWzBdKSBzZWxmLm1heFJhbmdlRGF0ZSA9IGRhdGU7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIikge1xuXHRcdFx0aWYgKGlzRGF0ZUluUmFuZ2UoZGF0ZSkgJiYgIWlzRGF0ZVNlbGVjdGVkKGRhdGUpKSBkYXlFbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJpblJhbmdlXCIpO1xuXG5cdFx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSAmJiAoZGF0ZSA8IHNlbGYubWluUmFuZ2VEYXRlIHx8IGRhdGUgPiBzZWxmLm1heFJhbmdlRGF0ZSkpIGRheUVsZW1lbnQuY2xhc3NMaXN0LmFkZChcIm5vdEFsbG93ZWRcIik7XG5cdFx0fVxuXG5cdFx0aWYgKHNlbGYuY29uZmlnLndlZWtOdW1iZXJzICYmIGNsYXNzTmFtZSAhPT0gXCJwcmV2TW9udGhEYXlcIiAmJiBkYXlOdW1iZXIgJSA3ID09PSAxKSB7XG5cdFx0XHRzZWxmLndlZWtOdW1iZXJzLmluc2VydEFkamFjZW50SFRNTChcImJlZm9yZWVuZFwiLCBcIjxzcGFuIGNsYXNzPSdkaXNhYmxlZCBmbGF0cGlja3ItZGF5Jz5cIiArIHNlbGYuY29uZmlnLmdldFdlZWsoZGF0ZSkgKyBcIjwvc3Bhbj5cIik7XG5cdFx0fVxuXG5cdFx0dHJpZ2dlckV2ZW50KFwiRGF5Q3JlYXRlXCIsIGRheUVsZW1lbnQpO1xuXG5cdFx0cmV0dXJuIGRheUVsZW1lbnQ7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZERheXMoeWVhciwgbW9udGgpIHtcblx0XHR2YXIgZmlyc3RPZk1vbnRoID0gKG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoLCAxKS5nZXREYXkoKSAtIHNlbGYubDEwbi5maXJzdERheU9mV2VlayArIDcpICUgNyxcblx0XHQgICAgaXNSYW5nZU1vZGUgPSBzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCI7XG5cblx0XHRzZWxmLnByZXZNb250aERheXMgPSBzZWxmLnV0aWxzLmdldERheXNpbk1vbnRoKChzZWxmLmN1cnJlbnRNb250aCAtIDEgKyAxMikgJSAxMik7XG5cblx0XHR2YXIgZGF5c0luTW9udGggPSBzZWxmLnV0aWxzLmdldERheXNpbk1vbnRoKCksXG5cdFx0ICAgIGRheXMgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRG9jdW1lbnRGcmFnbWVudCgpO1xuXG5cdFx0dmFyIGRheU51bWJlciA9IHNlbGYucHJldk1vbnRoRGF5cyArIDEgLSBmaXJzdE9mTW9udGg7XG5cblx0XHRpZiAoc2VsZi5jb25maWcud2Vla051bWJlcnMgJiYgc2VsZi53ZWVrTnVtYmVycy5maXJzdENoaWxkKSBzZWxmLndlZWtOdW1iZXJzLnRleHRDb250ZW50ID0gXCJcIjtcblxuXHRcdGlmIChpc1JhbmdlTW9kZSkge1xuXHRcdFx0Ly8gY29uc3QgZGF0ZUxpbWl0cyA9IHNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggfHwgc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGggfHwgc2VsZi5jb25maWcubWl4RGF0ZSB8fCBzZWxmLmNvbmZpZy5tYXhEYXRlO1xuXHRcdFx0c2VsZi5taW5SYW5nZURhdGUgPSBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCAtIDEsIGRheU51bWJlcik7XG5cdFx0XHRzZWxmLm1heFJhbmdlRGF0ZSA9IG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoICsgMSwgKDQyIC0gZmlyc3RPZk1vbnRoKSAlIGRheXNJbk1vbnRoKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5kYXlzLmZpcnN0Q2hpbGQpIHNlbGYuZGF5cy50ZXh0Q29udGVudCA9IFwiXCI7XG5cblx0XHQvLyBwcmVwZW5kIGRheXMgZnJvbSB0aGUgZW5kaW5nIG9mIHByZXZpb3VzIG1vbnRoXG5cdFx0Zm9yICg7IGRheU51bWJlciA8PSBzZWxmLnByZXZNb250aERheXM7IGRheU51bWJlcisrKSB7XG5cdFx0XHRkYXlzLmFwcGVuZENoaWxkKGNyZWF0ZURheShcInByZXZNb250aERheVwiLCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCAtIDEsIGRheU51bWJlciksIGRheU51bWJlcikpO1xuXHRcdH1cblxuXHRcdC8vIFN0YXJ0IGF0IDEgc2luY2UgdGhlcmUgaXMgbm8gMHRoIGRheVxuXHRcdGZvciAoZGF5TnVtYmVyID0gMTsgZGF5TnVtYmVyIDw9IGRheXNJbk1vbnRoOyBkYXlOdW1iZXIrKykge1xuXHRcdFx0ZGF5cy5hcHBlbmRDaGlsZChjcmVhdGVEYXkoXCJcIiwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGgsIGRheU51bWJlciksIGRheU51bWJlcikpO1xuXHRcdH1cblxuXHRcdC8vIGFwcGVuZCBkYXlzIGZyb20gdGhlIG5leHQgbW9udGhcblx0XHRmb3IgKHZhciBkYXlOdW0gPSBkYXlzSW5Nb250aCArIDE7IGRheU51bSA8PSA0MiAtIGZpcnN0T2ZNb250aDsgZGF5TnVtKyspIHtcblx0XHRcdGRheXMuYXBwZW5kQ2hpbGQoY3JlYXRlRGF5KFwibmV4dE1vbnRoRGF5XCIsIG5ldyBEYXRlKHNlbGYuY3VycmVudFllYXIsIHNlbGYuY3VycmVudE1vbnRoICsgMSwgZGF5TnVtICUgZGF5c0luTW9udGgpLCBkYXlOdW0pKTtcblx0XHR9XG5cblx0XHRpZiAoaXNSYW5nZU1vZGUgJiYgc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA9PT0gMSAmJiBkYXlzLmNoaWxkTm9kZXNbMF0pIHtcblx0XHRcdHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyA9IHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyB8fCBzZWxmLm1pblJhbmdlRGF0ZSA+IGRheXMuY2hpbGROb2Rlc1swXS5kYXRlT2JqO1xuXG5cdFx0XHRzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgPSBzZWxmLl9oaWRlTmV4dE1vbnRoQXJyb3cgfHwgc2VsZi5tYXhSYW5nZURhdGUgPCBuZXcgRGF0ZShzZWxmLmN1cnJlbnRZZWFyLCBzZWxmLmN1cnJlbnRNb250aCArIDEsIDEpO1xuXHRcdH0gZWxzZSB1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCk7XG5cblx0XHRzZWxmLmRheXMuYXBwZW5kQ2hpbGQoZGF5cyk7XG5cdFx0cmV0dXJuIHNlbGYuZGF5cztcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkTW9udGhOYXYoKSB7XG5cdFx0dmFyIG1vbnRoTmF2RnJhZ21lbnQgPSB3aW5kb3cuZG9jdW1lbnQuY3JlYXRlRG9jdW1lbnRGcmFnbWVudCgpO1xuXHRcdHNlbGYubW9udGhOYXYgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLW1vbnRoXCIpO1xuXG5cdFx0c2VsZi5wcmV2TW9udGhOYXYgPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1wcmV2LW1vbnRoXCIpO1xuXHRcdHNlbGYucHJldk1vbnRoTmF2LmlubmVySFRNTCA9IHNlbGYuY29uZmlnLnByZXZBcnJvdztcblxuXHRcdHNlbGYuY3VycmVudE1vbnRoRWxlbWVudCA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiY3VyLW1vbnRoXCIpO1xuXHRcdHNlbGYuY3VycmVudE1vbnRoRWxlbWVudC50aXRsZSA9IHNlbGYubDEwbi5zY3JvbGxUaXRsZTtcblxuXHRcdHZhciB5ZWFySW5wdXQgPSBjcmVhdGVOdW1iZXJJbnB1dChcImN1ci15ZWFyXCIpO1xuXHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50ID0geWVhcklucHV0LmNoaWxkTm9kZXNbMF07XG5cdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQudGl0bGUgPSBzZWxmLmwxMG4uc2Nyb2xsVGl0bGU7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWluRGF0ZSkgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWluID0gc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLm1heERhdGUpIHtcblx0XHRcdHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1heCA9IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKTtcblxuXHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuZGlzYWJsZWQgPSBzZWxmLmNvbmZpZy5taW5EYXRlICYmIHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKSA9PT0gc2VsZi5jb25maWcubWF4RGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdH1cblxuXHRcdHNlbGYubmV4dE1vbnRoTmF2ID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItbmV4dC1tb250aFwiKTtcblx0XHRzZWxmLm5leHRNb250aE5hdi5pbm5lckhUTUwgPSBzZWxmLmNvbmZpZy5uZXh0QXJyb3c7XG5cblx0XHRzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGggPSBjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci1jdXJyZW50LW1vbnRoXCIpO1xuXHRcdHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aC5hcHBlbmRDaGlsZChzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQpO1xuXHRcdHNlbGYubmF2aWdhdGlvbkN1cnJlbnRNb250aC5hcHBlbmRDaGlsZCh5ZWFySW5wdXQpO1xuXG5cdFx0bW9udGhOYXZGcmFnbWVudC5hcHBlbmRDaGlsZChzZWxmLnByZXZNb250aE5hdik7XG5cdFx0bW9udGhOYXZGcmFnbWVudC5hcHBlbmRDaGlsZChzZWxmLm5hdmlnYXRpb25DdXJyZW50TW9udGgpO1xuXHRcdG1vbnRoTmF2RnJhZ21lbnQuYXBwZW5kQ2hpbGQoc2VsZi5uZXh0TW9udGhOYXYpO1xuXHRcdHNlbGYubW9udGhOYXYuYXBwZW5kQ2hpbGQobW9udGhOYXZGcmFnbWVudCk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZiwgXCJfaGlkZVByZXZNb250aEFycm93XCIsIHtcblx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRyZXR1cm4gdGhpcy5fX2hpZGVQcmV2TW9udGhBcnJvdztcblx0XHRcdH0sXG5cdFx0XHRzZXQ6IGZ1bmN0aW9uIHNldChib29sKSB7XG5cdFx0XHRcdGlmICh0aGlzLl9faGlkZVByZXZNb250aEFycm93ICE9PSBib29sKSBzZWxmLnByZXZNb250aE5hdi5zdHlsZS5kaXNwbGF5ID0gYm9vbCA/IFwibm9uZVwiIDogXCJibG9ja1wiO1xuXHRcdFx0XHR0aGlzLl9faGlkZVByZXZNb250aEFycm93ID0gYm9vbDtcblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShzZWxmLCBcIl9oaWRlTmV4dE1vbnRoQXJyb3dcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiB0aGlzLl9faGlkZU5leHRNb250aEFycm93O1xuXHRcdFx0fSxcblx0XHRcdHNldDogZnVuY3Rpb24gc2V0KGJvb2wpIHtcblx0XHRcdFx0aWYgKHRoaXMuX19oaWRlTmV4dE1vbnRoQXJyb3cgIT09IGJvb2wpIHNlbGYubmV4dE1vbnRoTmF2LnN0eWxlLmRpc3BsYXkgPSBib29sID8gXCJub25lXCIgOiBcImJsb2NrXCI7XG5cdFx0XHRcdHRoaXMuX19oaWRlTmV4dE1vbnRoQXJyb3cgPSBib29sO1xuXHRcdFx0fVxuXHRcdH0pO1xuXG5cdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXG5cdFx0cmV0dXJuIHNlbGYubW9udGhOYXY7XG5cdH1cblxuXHRmdW5jdGlvbiBidWlsZFRpbWUoKSB7XG5cdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwiaGFzVGltZVwiKTtcblx0XHRpZiAoc2VsZi5jb25maWcubm9DYWxlbmRhcikgc2VsZi5jYWxlbmRhckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKFwibm9DYWxlbmRhclwiKTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIgPSBjcmVhdGVFbGVtZW50KFwiZGl2XCIsIFwiZmxhdHBpY2tyLXRpbWVcIik7XG5cdFx0c2VsZi50aW1lQ29udGFpbmVyLnRhYkluZGV4ID0gLTE7XG5cdFx0dmFyIHNlcGFyYXRvciA9IGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXRpbWUtc2VwYXJhdG9yXCIsIFwiOlwiKTtcblxuXHRcdHZhciBob3VySW5wdXQgPSBjcmVhdGVOdW1iZXJJbnB1dChcImZsYXRwaWNrci1ob3VyXCIpO1xuXHRcdHNlbGYuaG91ckVsZW1lbnQgPSBob3VySW5wdXQuY2hpbGROb2Rlc1swXTtcblxuXHRcdHZhciBtaW51dGVJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiZmxhdHBpY2tyLW1pbnV0ZVwiKTtcblx0XHRzZWxmLm1pbnV0ZUVsZW1lbnQgPSBtaW51dGVJbnB1dC5jaGlsZE5vZGVzWzBdO1xuXG5cdFx0c2VsZi5ob3VyRWxlbWVudC50YWJJbmRleCA9IHNlbGYubWludXRlRWxlbWVudC50YWJJbmRleCA9IDA7XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPyBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRIb3VycygpIDogc2VsZi5jb25maWcuZGVmYXVsdEhvdXIpO1xuXG5cdFx0c2VsZi5taW51dGVFbGVtZW50LnZhbHVlID0gc2VsZi5wYWQoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPyBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iai5nZXRNaW51dGVzKCkgOiBzZWxmLmNvbmZpZy5kZWZhdWx0TWludXRlKTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQuc3RlcCA9IHNlbGYuY29uZmlnLmhvdXJJbmNyZW1lbnQ7XG5cdFx0c2VsZi5taW51dGVFbGVtZW50LnN0ZXAgPSBzZWxmLmNvbmZpZy5taW51dGVJbmNyZW1lbnQ7XG5cblx0XHRzZWxmLmhvdXJFbGVtZW50Lm1pbiA9IHNlbGYuY29uZmlnLnRpbWVfMjRociA/IDAgOiAxO1xuXHRcdHNlbGYuaG91ckVsZW1lbnQubWF4ID0gc2VsZi5jb25maWcudGltZV8yNGhyID8gMjMgOiAxMjtcblxuXHRcdHNlbGYubWludXRlRWxlbWVudC5taW4gPSAwO1xuXHRcdHNlbGYubWludXRlRWxlbWVudC5tYXggPSA1OTtcblxuXHRcdHNlbGYuaG91ckVsZW1lbnQudGl0bGUgPSBzZWxmLm1pbnV0ZUVsZW1lbnQudGl0bGUgPSBzZWxmLmwxMG4uc2Nyb2xsVGl0bGU7XG5cblx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoaG91cklucHV0KTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQoc2VwYXJhdG9yKTtcblx0XHRzZWxmLnRpbWVDb250YWluZXIuYXBwZW5kQ2hpbGQobWludXRlSW5wdXQpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLnRpbWVfMjRocikgc2VsZi50aW1lQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJ0aW1lMjRoclwiKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzKSB7XG5cdFx0XHRzZWxmLnRpbWVDb250YWluZXIuY2xhc3NMaXN0LmFkZChcImhhc1NlY29uZHNcIik7XG5cblx0XHRcdHZhciBzZWNvbmRJbnB1dCA9IGNyZWF0ZU51bWJlcklucHV0KFwiZmxhdHBpY2tyLXNlY29uZFwiKTtcblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudCA9IHNlY29uZElucHV0LmNoaWxkTm9kZXNbMF07XG5cblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC52YWx1ZSA9IHNlbGYubGF0ZXN0U2VsZWN0ZWREYXRlT2JqID8gc2VsZi5wYWQoc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmouZ2V0U2Vjb25kcygpKSA6IFwiMDBcIjtcblxuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50LnN0ZXAgPSBzZWxmLm1pbnV0ZUVsZW1lbnQuc3RlcDtcblx0XHRcdHNlbGYuc2Vjb25kRWxlbWVudC5taW4gPSBzZWxmLm1pbnV0ZUVsZW1lbnQubWluO1xuXHRcdFx0c2VsZi5zZWNvbmRFbGVtZW50Lm1heCA9IHNlbGYubWludXRlRWxlbWVudC5tYXg7XG5cblx0XHRcdHNlbGYudGltZUNvbnRhaW5lci5hcHBlbmRDaGlsZChjcmVhdGVFbGVtZW50KFwic3BhblwiLCBcImZsYXRwaWNrci10aW1lLXNlcGFyYXRvclwiLCBcIjpcIikpO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlY29uZElucHV0KTtcblx0XHR9XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLnRpbWVfMjRocikge1xuXHRcdFx0Ly8gYWRkIHNlbGYuYW1QTSBpZiBhcHByb3ByaWF0ZVxuXHRcdFx0c2VsZi5hbVBNID0gY3JlYXRlRWxlbWVudChcInNwYW5cIiwgXCJmbGF0cGlja3ItYW0tcG1cIiwgW1wiQU1cIiwgXCJQTVwiXVtzZWxmLmhvdXJFbGVtZW50LnZhbHVlID4gMTEgfCAwXSk7XG5cdFx0XHRzZWxmLmFtUE0udGl0bGUgPSBzZWxmLmwxMG4udG9nZ2xlVGl0bGU7XG5cdFx0XHRzZWxmLmFtUE0udGFiSW5kZXggPSAwO1xuXHRcdFx0c2VsZi50aW1lQ29udGFpbmVyLmFwcGVuZENoaWxkKHNlbGYuYW1QTSk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIHNlbGYudGltZUNvbnRhaW5lcjtcblx0fVxuXG5cdGZ1bmN0aW9uIGJ1aWxkV2Vla2RheXMoKSB7XG5cdFx0aWYgKCFzZWxmLndlZWtkYXlDb250YWluZXIpIHNlbGYud2Vla2RheUNvbnRhaW5lciA9IGNyZWF0ZUVsZW1lbnQoXCJkaXZcIiwgXCJmbGF0cGlja3Itd2Vla2RheXNcIik7XG5cblx0XHR2YXIgZmlyc3REYXlPZldlZWsgPSBzZWxmLmwxMG4uZmlyc3REYXlPZldlZWs7XG5cdFx0dmFyIHdlZWtkYXlzID0gc2VsZi5sMTBuLndlZWtkYXlzLnNob3J0aGFuZC5zbGljZSgpO1xuXG5cdFx0aWYgKGZpcnN0RGF5T2ZXZWVrID4gMCAmJiBmaXJzdERheU9mV2VlayA8IHdlZWtkYXlzLmxlbmd0aCkge1xuXHRcdFx0d2Vla2RheXMgPSBbXS5jb25jYXQod2Vla2RheXMuc3BsaWNlKGZpcnN0RGF5T2ZXZWVrLCB3ZWVrZGF5cy5sZW5ndGgpLCB3ZWVrZGF5cy5zcGxpY2UoMCwgZmlyc3REYXlPZldlZWspKTtcblx0XHR9XG5cblx0XHRzZWxmLndlZWtkYXlDb250YWluZXIuaW5uZXJIVE1MID0gXCJcXG5cXHRcXHQ8c3BhbiBjbGFzcz1mbGF0cGlja3Itd2Vla2RheT5cXG5cXHRcXHRcXHRcIiArIHdlZWtkYXlzLmpvaW4oXCI8L3NwYW4+PHNwYW4gY2xhc3M9ZmxhdHBpY2tyLXdlZWtkYXk+XCIpICsgXCJcXG5cXHRcXHQ8L3NwYW4+XFxuXFx0XFx0XCI7XG5cblx0XHRyZXR1cm4gc2VsZi53ZWVrZGF5Q29udGFpbmVyO1xuXHR9XG5cblx0LyogaXN0YW5idWwgaWdub3JlIG5leHQgKi9cblx0ZnVuY3Rpb24gYnVpbGRXZWVrcygpIHtcblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJoYXNXZWVrc1wiKTtcblx0XHRzZWxmLndlZWtXcmFwcGVyID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci13ZWVrd3JhcHBlclwiKTtcblx0XHRzZWxmLndlZWtXcmFwcGVyLmFwcGVuZENoaWxkKGNyZWF0ZUVsZW1lbnQoXCJzcGFuXCIsIFwiZmxhdHBpY2tyLXdlZWtkYXlcIiwgc2VsZi5sMTBuLndlZWtBYmJyZXZpYXRpb24pKTtcblx0XHRzZWxmLndlZWtOdW1iZXJzID0gY3JlYXRlRWxlbWVudChcImRpdlwiLCBcImZsYXRwaWNrci13ZWVrc1wiKTtcblx0XHRzZWxmLndlZWtXcmFwcGVyLmFwcGVuZENoaWxkKHNlbGYud2Vla051bWJlcnMpO1xuXG5cdFx0cmV0dXJuIHNlbGYud2Vla1dyYXBwZXI7XG5cdH1cblxuXHRmdW5jdGlvbiBjaGFuZ2VNb250aCh2YWx1ZSwgaXNfb2Zmc2V0KSB7XG5cdFx0aXNfb2Zmc2V0ID0gdHlwZW9mIGlzX29mZnNldCA9PT0gXCJ1bmRlZmluZWRcIiB8fCBpc19vZmZzZXQ7XG5cdFx0dmFyIGRlbHRhID0gaXNfb2Zmc2V0ID8gdmFsdWUgOiB2YWx1ZSAtIHNlbGYuY3VycmVudE1vbnRoO1xuXG5cdFx0aWYgKGRlbHRhIDwgMCAmJiBzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgfHwgZGVsdGEgPiAwICYmIHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdykgcmV0dXJuO1xuXG5cdFx0c2VsZi5jdXJyZW50TW9udGggKz0gZGVsdGE7XG5cblx0XHRpZiAoc2VsZi5jdXJyZW50TW9udGggPCAwIHx8IHNlbGYuY3VycmVudE1vbnRoID4gMTEpIHtcblx0XHRcdHNlbGYuY3VycmVudFllYXIgKz0gc2VsZi5jdXJyZW50TW9udGggPiAxMSA/IDEgOiAtMTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0gKHNlbGYuY3VycmVudE1vbnRoICsgMTIpICUgMTI7XG5cblx0XHRcdHRyaWdnZXJFdmVudChcIlllYXJDaGFuZ2VcIik7XG5cdFx0fVxuXG5cdFx0dXBkYXRlTmF2aWdhdGlvbkN1cnJlbnRNb250aCgpO1xuXHRcdGJ1aWxkRGF5cygpO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5ub0NhbGVuZGFyKSBzZWxmLmRheXMuZm9jdXMoKTtcblxuXHRcdHRyaWdnZXJFdmVudChcIk1vbnRoQ2hhbmdlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2xlYXIodHJpZ2dlckNoYW5nZUV2ZW50KSB7XG5cdFx0c2VsZi5pbnB1dC52YWx1ZSA9IFwiXCI7XG5cblx0XHRpZiAoc2VsZi5hbHRJbnB1dCkgc2VsZi5hbHRJbnB1dC52YWx1ZSA9IFwiXCI7XG5cblx0XHRpZiAoc2VsZi5tb2JpbGVJbnB1dCkgc2VsZi5tb2JpbGVJbnB1dC52YWx1ZSA9IFwiXCI7XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbXTtcblx0XHRzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IG51bGw7XG5cdFx0c2VsZi5zaG93VGltZUlucHV0ID0gZmFsc2U7XG5cblx0XHRzZWxmLnJlZHJhdygpO1xuXG5cdFx0aWYgKHRyaWdnZXJDaGFuZ2VFdmVudCAhPT0gZmFsc2UpXG5cdFx0XHQvLyB0cmlnZ2VyQ2hhbmdlRXZlbnQgaXMgdHJ1ZSAoZGVmYXVsdCkgb3IgYW4gRXZlbnRcblx0XHRcdHRyaWdnZXJFdmVudChcIkNoYW5nZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNsb3NlKCkge1xuXHRcdHNlbGYuaXNPcGVuID0gZmFsc2U7XG5cblx0XHRpZiAoIXNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuY2xhc3NMaXN0LnJlbW92ZShcIm9wZW5cIik7XG5cdFx0XHQoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5jbGFzc0xpc3QucmVtb3ZlKFwiYWN0aXZlXCIpO1xuXHRcdH1cblxuXHRcdHRyaWdnZXJFdmVudChcIkNsb3NlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gZGVzdHJveShpbnN0YW5jZSkge1xuXHRcdGluc3RhbmNlID0gaW5zdGFuY2UgfHwgc2VsZjtcblx0XHRpbnN0YW5jZS5jbGVhcihmYWxzZSk7XG5cblx0XHR3aW5kb3cucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInJlc2l6ZVwiLCBpbnN0YW5jZS5kZWJvdW5jZWRSZXNpemUpO1xuXG5cdFx0d2luZG93LmRvY3VtZW50LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBkb2N1bWVudENsaWNrKTtcblx0XHR3aW5kb3cuZG9jdW1lbnQucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInRvdWNoc3RhcnRcIiwgZG9jdW1lbnRDbGljayk7XG5cdFx0d2luZG93LmRvY3VtZW50LnJlbW92ZUV2ZW50TGlzdGVuZXIoXCJibHVyXCIsIGRvY3VtZW50Q2xpY2spO1xuXG5cdFx0aWYgKGluc3RhbmNlLnRpbWVDb250YWluZXIpIGluc3RhbmNlLnRpbWVDb250YWluZXIucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInRyYW5zaXRpb25lbmRcIiwgcG9zaXRpb25DYWxlbmRhcik7XG5cblx0XHRpZiAoaW5zdGFuY2UubW9iaWxlSW5wdXQpIHtcblx0XHRcdGlmIChpbnN0YW5jZS5tb2JpbGVJbnB1dC5wYXJlbnROb2RlKSBpbnN0YW5jZS5tb2JpbGVJbnB1dC5wYXJlbnROb2RlLnJlbW92ZUNoaWxkKGluc3RhbmNlLm1vYmlsZUlucHV0KTtcblx0XHRcdGRlbGV0ZSBpbnN0YW5jZS5tb2JpbGVJbnB1dDtcblx0XHR9IGVsc2UgaWYgKGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyICYmIGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyLnBhcmVudE5vZGUpIGluc3RhbmNlLmNhbGVuZGFyQ29udGFpbmVyLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoaW5zdGFuY2UuY2FsZW5kYXJDb250YWluZXIpO1xuXG5cdFx0aWYgKGluc3RhbmNlLmFsdElucHV0KSB7XG5cdFx0XHRpbnN0YW5jZS5pbnB1dC50eXBlID0gXCJ0ZXh0XCI7XG5cdFx0XHRpZiAoaW5zdGFuY2UuYWx0SW5wdXQucGFyZW50Tm9kZSkgaW5zdGFuY2UuYWx0SW5wdXQucGFyZW50Tm9kZS5yZW1vdmVDaGlsZChpbnN0YW5jZS5hbHRJbnB1dCk7XG5cdFx0XHRkZWxldGUgaW5zdGFuY2UuYWx0SW5wdXQ7XG5cdFx0fVxuXG5cdFx0aW5zdGFuY2UuaW5wdXQudHlwZSA9IGluc3RhbmNlLmlucHV0Ll90eXBlO1xuXHRcdGluc3RhbmNlLmlucHV0LmNsYXNzTGlzdC5yZW1vdmUoXCJmbGF0cGlja3ItaW5wdXRcIik7XG5cdFx0aW5zdGFuY2UuaW5wdXQucmVtb3ZlRXZlbnRMaXN0ZW5lcihcImZvY3VzXCIsIG9wZW4pO1xuXHRcdGluc3RhbmNlLmlucHV0LnJlbW92ZUF0dHJpYnV0ZShcInJlYWRvbmx5XCIpO1xuXG5cdFx0ZGVsZXRlIGluc3RhbmNlLmlucHV0Ll9mbGF0cGlja3I7XG5cdH1cblxuXHRmdW5jdGlvbiBpc0NhbGVuZGFyRWxlbShlbGVtKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLmFwcGVuZFRvICYmIHNlbGYuY29uZmlnLmFwcGVuZFRvLmNvbnRhaW5zKGVsZW0pKSByZXR1cm4gdHJ1ZTtcblxuXHRcdHJldHVybiBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNvbnRhaW5zKGVsZW0pO1xuXHR9XG5cblx0ZnVuY3Rpb24gZG9jdW1lbnRDbGljayhlKSB7XG5cdFx0dmFyIGlzSW5wdXQgPSBzZWxmLmVsZW1lbnQuY29udGFpbnMoZS50YXJnZXQpIHx8IGUudGFyZ2V0ID09PSBzZWxmLmlucHV0IHx8IGUudGFyZ2V0ID09PSBzZWxmLmFsdElucHV0IHx8XG5cdFx0Ly8gd2ViIGNvbXBvbmVudHNcblx0XHRlLnBhdGggJiYgZS5wYXRoLmluZGV4T2YgJiYgKH5lLnBhdGguaW5kZXhPZihzZWxmLmlucHV0KSB8fCB+ZS5wYXRoLmluZGV4T2Yoc2VsZi5hbHRJbnB1dCkpO1xuXG5cdFx0aWYgKHNlbGYuaXNPcGVuICYmICFzZWxmLmNvbmZpZy5pbmxpbmUgJiYgIWlzQ2FsZW5kYXJFbGVtKGUudGFyZ2V0KSAmJiAhaXNJbnB1dCkge1xuXHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0c2VsZi5jbG9zZSgpO1xuXG5cdFx0XHRpZiAoc2VsZi5jb25maWcubW9kZSA9PT0gXCJyYW5nZVwiICYmIHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDEpIHtcblx0XHRcdFx0c2VsZi5jbGVhcigpO1xuXHRcdFx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdFx0fVxuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGZvcm1hdERhdGUoZnJtdCwgZGF0ZU9iaikge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5mb3JtYXREYXRlKSByZXR1cm4gc2VsZi5jb25maWcuZm9ybWF0RGF0ZShmcm10LCBkYXRlT2JqKTtcblxuXHRcdHZhciBjaGFycyA9IGZybXQuc3BsaXQoXCJcIik7XG5cdFx0cmV0dXJuIGNoYXJzLm1hcChmdW5jdGlvbiAoYywgaSkge1xuXHRcdFx0cmV0dXJuIHNlbGYuZm9ybWF0c1tjXSAmJiBjaGFyc1tpIC0gMV0gIT09IFwiXFxcXFwiID8gc2VsZi5mb3JtYXRzW2NdKGRhdGVPYmopIDogYyAhPT0gXCJcXFxcXCIgPyBjIDogXCJcIjtcblx0XHR9KS5qb2luKFwiXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gY2hhbmdlWWVhcihuZXdZZWFyKSB7XG5cdFx0aWYgKCFuZXdZZWFyIHx8IHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1pbiAmJiBuZXdZZWFyIDwgc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWluIHx8IHNlbGYuY3VycmVudFllYXJFbGVtZW50Lm1heCAmJiBuZXdZZWFyID4gc2VsZi5jdXJyZW50WWVhckVsZW1lbnQubWF4KSByZXR1cm47XG5cblx0XHR2YXIgbmV3WWVhck51bSA9IHBhcnNlSW50KG5ld1llYXIsIDEwKSxcblx0XHQgICAgaXNOZXdZZWFyID0gc2VsZi5jdXJyZW50WWVhciAhPT0gbmV3WWVhck51bTtcblxuXHRcdHNlbGYuY3VycmVudFllYXIgPSBuZXdZZWFyTnVtIHx8IHNlbGYuY3VycmVudFllYXI7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWF4RGF0ZSAmJiBzZWxmLmN1cnJlbnRZZWFyID09PSBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCkpIHtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0gTWF0aC5taW4oc2VsZi5jb25maWcubWF4RGF0ZS5nZXRNb250aCgpLCBzZWxmLmN1cnJlbnRNb250aCk7XG5cdFx0fSBlbHNlIGlmIChzZWxmLmNvbmZpZy5taW5EYXRlICYmIHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1pbkRhdGUuZ2V0RnVsbFllYXIoKSkge1xuXHRcdFx0c2VsZi5jdXJyZW50TW9udGggPSBNYXRoLm1heChzZWxmLmNvbmZpZy5taW5EYXRlLmdldE1vbnRoKCksIHNlbGYuY3VycmVudE1vbnRoKTtcblx0XHR9XG5cblx0XHRpZiAoaXNOZXdZZWFyKSB7XG5cdFx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdFx0dHJpZ2dlckV2ZW50KFwiWWVhckNoYW5nZVwiKTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBpc0VuYWJsZWQoZGF0ZSwgdGltZWxlc3MpIHtcblx0XHR2YXIgbHRtaW4gPSBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5jb25maWcubWluRGF0ZSwgdHlwZW9mIHRpbWVsZXNzICE9PSBcInVuZGVmaW5lZFwiID8gdGltZWxlc3MgOiAhc2VsZi5taW5EYXRlSGFzVGltZSkgPCAwO1xuXHRcdHZhciBndG1heCA9IGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLmNvbmZpZy5tYXhEYXRlLCB0eXBlb2YgdGltZWxlc3MgIT09IFwidW5kZWZpbmVkXCIgPyB0aW1lbGVzcyA6ICFzZWxmLm1heERhdGVIYXNUaW1lKSA+IDA7XG5cblx0XHRpZiAobHRtaW4gfHwgZ3RtYXgpIHJldHVybiBmYWxzZTtcblxuXHRcdGlmICghc2VsZi5jb25maWcuZW5hYmxlLmxlbmd0aCAmJiAhc2VsZi5jb25maWcuZGlzYWJsZS5sZW5ndGgpIHJldHVybiB0cnVlO1xuXG5cdFx0dmFyIGRhdGVUb0NoZWNrID0gc2VsZi5wYXJzZURhdGUoZGF0ZSwgdHJ1ZSk7IC8vIHRpbWVsZXNzXG5cblx0XHR2YXIgYm9vbCA9IHNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGggPiAwLFxuXHRcdCAgICBhcnJheSA9IGJvb2wgPyBzZWxmLmNvbmZpZy5lbmFibGUgOiBzZWxmLmNvbmZpZy5kaXNhYmxlO1xuXG5cdFx0Zm9yICh2YXIgaSA9IDAsIGQ7IGkgPCBhcnJheS5sZW5ndGg7IGkrKykge1xuXHRcdFx0ZCA9IGFycmF5W2ldO1xuXG5cdFx0XHRpZiAoZCBpbnN0YW5jZW9mIEZ1bmN0aW9uICYmIGQoZGF0ZVRvQ2hlY2spKSAvLyBkaXNhYmxlZCBieSBmdW5jdGlvblxuXHRcdFx0XHRyZXR1cm4gYm9vbDtlbHNlIGlmIChkIGluc3RhbmNlb2YgRGF0ZSAmJiBkLmdldFRpbWUoKSA9PT0gZGF0ZVRvQ2hlY2suZ2V0VGltZSgpKVxuXHRcdFx0XHQvLyBkaXNhYmxlZCBieSBkYXRlXG5cdFx0XHRcdHJldHVybiBib29sO2Vsc2UgaWYgKHR5cGVvZiBkID09PSBcInN0cmluZ1wiICYmIHNlbGYucGFyc2VEYXRlKGQsIHRydWUpLmdldFRpbWUoKSA9PT0gZGF0ZVRvQ2hlY2suZ2V0VGltZSgpKVxuXHRcdFx0XHQvLyBkaXNhYmxlZCBieSBkYXRlIHN0cmluZ1xuXHRcdFx0XHRyZXR1cm4gYm9vbDtlbHNlIGlmICggLy8gZGlzYWJsZWQgYnkgcmFuZ2Vcblx0XHRcdCh0eXBlb2YgZCA9PT0gXCJ1bmRlZmluZWRcIiA/IFwidW5kZWZpbmVkXCIgOiBfdHlwZW9mKGQpKSA9PT0gXCJvYmplY3RcIiAmJiBkLmZyb20gJiYgZC50byAmJiBkYXRlVG9DaGVjayA+PSBkLmZyb20gJiYgZGF0ZVRvQ2hlY2sgPD0gZC50bykgcmV0dXJuIGJvb2w7XG5cdFx0fVxuXG5cdFx0cmV0dXJuICFib29sO1xuXHR9XG5cblx0ZnVuY3Rpb24gb25LZXlEb3duKGUpIHtcblx0XHRpZiAoZS50YXJnZXQgPT09IChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpICYmIGUud2hpY2ggPT09IDEzKSBzZWxlY3REYXRlKGUpO2Vsc2UgaWYgKHNlbGYuaXNPcGVuIHx8IHNlbGYuY29uZmlnLmlubGluZSkge1xuXHRcdFx0c3dpdGNoIChlLndoaWNoKSB7XG5cdFx0XHRcdGNhc2UgMTM6XG5cdFx0XHRcdFx0aWYgKHNlbGYudGltZUNvbnRhaW5lciAmJiBzZWxmLnRpbWVDb250YWluZXIuY29udGFpbnMoZS50YXJnZXQpKSB1cGRhdGVWYWx1ZSgpO2Vsc2Ugc2VsZWN0RGF0ZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgMjc6XG5cdFx0XHRcdFx0Ly8gZXNjYXBlXG5cdFx0XHRcdFx0c2VsZi5jbG9zZSgpO1xuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgMzc6XG5cdFx0XHRcdFx0aWYgKGUudGFyZ2V0ICE9PSBzZWxmLmlucHV0ICYgZS50YXJnZXQgIT09IHNlbGYuYWx0SW5wdXQpIHtcblx0XHRcdFx0XHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRcdFx0XHRcdGNoYW5nZU1vbnRoKC0xKTtcblx0XHRcdFx0XHRcdHNlbGYuY3VycmVudE1vbnRoRWxlbWVudC5mb2N1cygpO1xuXHRcdFx0XHRcdH1cblx0XHRcdFx0XHRicmVhaztcblxuXHRcdFx0XHRjYXNlIDM4OlxuXHRcdFx0XHRcdGlmICghc2VsZi50aW1lQ29udGFpbmVyIHx8ICFzZWxmLnRpbWVDb250YWluZXIuY29udGFpbnMoZS50YXJnZXQpKSB7XG5cdFx0XHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdFx0XHRzZWxmLmN1cnJlbnRZZWFyKys7XG5cdFx0XHRcdFx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdFx0XHRcdH0gZWxzZSB1cGRhdGVUaW1lKGUpO1xuXG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSAzOTpcblx0XHRcdFx0XHRpZiAoZS50YXJnZXQgIT09IHNlbGYuaW5wdXQgJiBlLnRhcmdldCAhPT0gc2VsZi5hbHRJbnB1dCkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0Y2hhbmdlTW9udGgoMSk7XG5cdFx0XHRcdFx0XHRzZWxmLmN1cnJlbnRNb250aEVsZW1lbnQuZm9jdXMoKTtcblx0XHRcdFx0XHR9XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0Y2FzZSA0MDpcblx0XHRcdFx0XHRpZiAoIXNlbGYudGltZUNvbnRhaW5lciB8fCAhc2VsZi50aW1lQ29udGFpbmVyLmNvbnRhaW5zKGUudGFyZ2V0KSkge1xuXHRcdFx0XHRcdFx0ZS5wcmV2ZW50RGVmYXVsdCgpO1xuXHRcdFx0XHRcdFx0c2VsZi5jdXJyZW50WWVhci0tO1xuXHRcdFx0XHRcdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRcdFx0XHR9IGVsc2UgdXBkYXRlVGltZShlKTtcblxuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGRlZmF1bHQ6XG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdH1cblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBvbk1vdXNlT3ZlcihlKSB7XG5cdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggIT09IDEgfHwgIWUudGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcImZsYXRwaWNrci1kYXlcIikpIHJldHVybjtcblxuXHRcdHZhciBob3ZlckRhdGUgPSBlLnRhcmdldC5kYXRlT2JqLFxuXHRcdCAgICBpbml0aWFsRGF0ZSA9IHNlbGYucGFyc2VEYXRlKHNlbGYuc2VsZWN0ZWREYXRlc1swXSwgdHJ1ZSksXG5cdFx0ICAgIHJhbmdlU3RhcnREYXRlID0gTWF0aC5taW4oaG92ZXJEYXRlLmdldFRpbWUoKSwgc2VsZi5zZWxlY3RlZERhdGVzWzBdLmdldFRpbWUoKSksXG5cdFx0ICAgIHJhbmdlRW5kRGF0ZSA9IE1hdGgubWF4KGhvdmVyRGF0ZS5nZXRUaW1lKCksIHNlbGYuc2VsZWN0ZWREYXRlc1swXS5nZXRUaW1lKCkpLFxuXHRcdCAgICBjb250YWluc0Rpc2FibGVkID0gZmFsc2U7XG5cblx0XHRmb3IgKHZhciB0ID0gcmFuZ2VTdGFydERhdGU7IHQgPCByYW5nZUVuZERhdGU7IHQgKz0gc2VsZi51dGlscy5kdXJhdGlvbi5EQVkpIHtcblx0XHRcdGlmICghaXNFbmFibGVkKG5ldyBEYXRlKHQpKSkge1xuXHRcdFx0XHRjb250YWluc0Rpc2FibGVkID0gdHJ1ZTtcblx0XHRcdFx0YnJlYWs7XG5cdFx0XHR9XG5cdFx0fVxuXG5cdFx0dmFyIF9sb29wID0gZnVuY3Rpb24gX2xvb3AodGltZXN0YW1wLCBpKSB7XG5cdFx0XHR2YXIgb3V0T2ZSYW5nZSA9IHRpbWVzdGFtcCA8IHNlbGYubWluUmFuZ2VEYXRlLmdldFRpbWUoKSB8fCB0aW1lc3RhbXAgPiBzZWxmLm1heFJhbmdlRGF0ZS5nZXRUaW1lKCk7XG5cblx0XHRcdGlmIChvdXRPZlJhbmdlKSB7XG5cdFx0XHRcdHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5hZGQoXCJub3RBbGxvd2VkXCIpO1xuXHRcdFx0XHRbXCJpblJhbmdlXCIsIFwic3RhcnRSYW5nZVwiLCBcImVuZFJhbmdlXCJdLmZvckVhY2goZnVuY3Rpb24gKGMpIHtcblx0XHRcdFx0XHRzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QucmVtb3ZlKGMpO1xuXHRcdFx0XHR9KTtcblx0XHRcdFx0cmV0dXJuIFwiY29udGludWVcIjtcblx0XHRcdH0gZWxzZSBpZiAoY29udGFpbnNEaXNhYmxlZCAmJiAhb3V0T2ZSYW5nZSkgcmV0dXJuIFwiY29udGludWVcIjtcblxuXHRcdFx0W1wic3RhcnRSYW5nZVwiLCBcImluUmFuZ2VcIiwgXCJlbmRSYW5nZVwiLCBcIm5vdEFsbG93ZWRcIl0uZm9yRWFjaChmdW5jdGlvbiAoYykge1xuXHRcdFx0XHRzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QucmVtb3ZlKGMpO1xuXHRcdFx0fSk7XG5cblx0XHRcdHZhciBtaW5SYW5nZURhdGUgPSBNYXRoLm1heChzZWxmLm1pblJhbmdlRGF0ZS5nZXRUaW1lKCksIHJhbmdlU3RhcnREYXRlKSxcblx0XHRcdCAgICBtYXhSYW5nZURhdGUgPSBNYXRoLm1pbihzZWxmLm1heFJhbmdlRGF0ZS5nZXRUaW1lKCksIHJhbmdlRW5kRGF0ZSk7XG5cblx0XHRcdGUudGFyZ2V0LmNsYXNzTGlzdC5hZGQoaG92ZXJEYXRlIDwgc2VsZi5zZWxlY3RlZERhdGVzWzBdID8gXCJzdGFydFJhbmdlXCIgOiBcImVuZFJhbmdlXCIpO1xuXG5cdFx0XHRpZiAoaW5pdGlhbERhdGUgPiBob3ZlckRhdGUgJiYgdGltZXN0YW1wID09PSBpbml0aWFsRGF0ZS5nZXRUaW1lKCkpIHNlbGYuZGF5cy5jaGlsZE5vZGVzW2ldLmNsYXNzTGlzdC5hZGQoXCJlbmRSYW5nZVwiKTtlbHNlIGlmIChpbml0aWFsRGF0ZSA8IGhvdmVyRGF0ZSAmJiB0aW1lc3RhbXAgPT09IGluaXRpYWxEYXRlLmdldFRpbWUoKSkgc2VsZi5kYXlzLmNoaWxkTm9kZXNbaV0uY2xhc3NMaXN0LmFkZChcInN0YXJ0UmFuZ2VcIik7ZWxzZSBpZiAodGltZXN0YW1wID49IG1pblJhbmdlRGF0ZSAmJiB0aW1lc3RhbXAgPD0gbWF4UmFuZ2VEYXRlKSBzZWxmLmRheXMuY2hpbGROb2Rlc1tpXS5jbGFzc0xpc3QuYWRkKFwiaW5SYW5nZVwiKTtcblx0XHR9O1xuXG5cdFx0Zm9yICh2YXIgdGltZXN0YW1wID0gc2VsZi5kYXlzLmNoaWxkTm9kZXNbMF0uZGF0ZU9iai5nZXRUaW1lKCksIGkgPSAwOyBpIDwgNDI7IGkrKywgdGltZXN0YW1wICs9IHNlbGYudXRpbHMuZHVyYXRpb24uREFZKSB7XG5cdFx0XHR2YXIgX3JldCA9IF9sb29wKHRpbWVzdGFtcCwgaSk7XG5cblx0XHRcdGlmIChfcmV0ID09PSBcImNvbnRpbnVlXCIpIGNvbnRpbnVlO1xuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIG9uUmVzaXplKCkge1xuXHRcdGlmIChzZWxmLmlzT3BlbiAmJiAhc2VsZi5jb25maWcuc3RhdGljICYmICFzZWxmLmNvbmZpZy5pbmxpbmUpIHBvc2l0aW9uQ2FsZW5kYXIoKTtcblx0fVxuXG5cdGZ1bmN0aW9uIG9wZW4oZSkge1xuXHRcdGlmIChzZWxmLmlzTW9iaWxlKSB7XG5cdFx0XHRpZiAoZSkge1xuXHRcdFx0XHRlLnByZXZlbnREZWZhdWx0KCk7XG5cdFx0XHRcdGUudGFyZ2V0LmJsdXIoKTtcblx0XHRcdH1cblxuXHRcdFx0c2V0VGltZW91dChmdW5jdGlvbiAoKSB7XG5cdFx0XHRcdHNlbGYubW9iaWxlSW5wdXQuY2xpY2soKTtcblx0XHRcdH0sIDApO1xuXG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJPcGVuXCIpO1xuXHRcdFx0cmV0dXJuO1xuXHRcdH0gZWxzZSBpZiAoc2VsZi5pc09wZW4gfHwgKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkuZGlzYWJsZWQgfHwgc2VsZi5jb25maWcuaW5saW5lKSByZXR1cm47XG5cblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoXCJvcGVuXCIpO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5zdGF0aWMgJiYgIXNlbGYuY29uZmlnLmlubGluZSkgcG9zaXRpb25DYWxlbmRhcigpO1xuXG5cdFx0c2VsZi5pc09wZW4gPSB0cnVlO1xuXG5cdFx0aWYgKCFzZWxmLmNvbmZpZy5hbGxvd0lucHV0KSB7XG5cdFx0XHQoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5ibHVyKCk7XG5cdFx0XHQoc2VsZi5jb25maWcubm9DYWxlbmRhciA/IHNlbGYudGltZUNvbnRhaW5lciA6IHNlbGYuc2VsZWN0ZWREYXRlRWxlbSA/IHNlbGYuc2VsZWN0ZWREYXRlRWxlbSA6IHNlbGYuZGF5cykuZm9jdXMoKTtcblx0XHR9XG5cblx0XHQoc2VsZi5hbHRJbnB1dCB8fCBzZWxmLmlucHV0KS5jbGFzc0xpc3QuYWRkKFwiYWN0aXZlXCIpO1xuXHRcdHRyaWdnZXJFdmVudChcIk9wZW5cIik7XG5cdH1cblxuXHRmdW5jdGlvbiBtaW5NYXhEYXRlU2V0dGVyKHR5cGUpIHtcblx0XHRyZXR1cm4gZnVuY3Rpb24gKGRhdGUpIHtcblx0XHRcdHZhciBkYXRlT2JqID0gc2VsZi5jb25maWdbXCJfXCIgKyB0eXBlICsgXCJEYXRlXCJdID0gc2VsZi5wYXJzZURhdGUoZGF0ZSk7XG5cblx0XHRcdHZhciBpbnZlcnNlRGF0ZU9iaiA9IHNlbGYuY29uZmlnW1wiX1wiICsgKHR5cGUgPT09IFwibWluXCIgPyBcIm1heFwiIDogXCJtaW5cIikgKyBcIkRhdGVcIl07XG5cdFx0XHR2YXIgaXNWYWxpZERhdGUgPSBkYXRlICYmIGRhdGVPYmogaW5zdGFuY2VvZiBEYXRlO1xuXG5cdFx0XHRpZiAoaXNWYWxpZERhdGUpIHtcblx0XHRcdFx0c2VsZlt0eXBlICsgXCJEYXRlSGFzVGltZVwiXSA9IGRhdGVPYmouZ2V0SG91cnMoKSB8fCBkYXRlT2JqLmdldE1pbnV0ZXMoKSB8fCBkYXRlT2JqLmdldFNlY29uZHMoKTtcblx0XHRcdH1cblxuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcykge1xuXHRcdFx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBzZWxmLnNlbGVjdGVkRGF0ZXMuZmlsdGVyKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRcdFx0cmV0dXJuIGlzRW5hYmxlZChkKTtcblx0XHRcdFx0fSk7XG5cdFx0XHRcdGlmICghc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCAmJiB0eXBlID09PSBcIm1pblwiKSBzZXRIb3Vyc0Zyb21EYXRlKGRhdGVPYmopO1xuXHRcdFx0XHR1cGRhdGVWYWx1ZSgpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5kYXlzKSB7XG5cdFx0XHRcdHJlZHJhdygpO1xuXG5cdFx0XHRcdGlmIChpc1ZhbGlkRGF0ZSkgc2VsZi5jdXJyZW50WWVhckVsZW1lbnRbdHlwZV0gPSBkYXRlT2JqLmdldEZ1bGxZZWFyKCk7ZWxzZSBzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC5yZW1vdmVBdHRyaWJ1dGUodHlwZSk7XG5cblx0XHRcdFx0c2VsZi5jdXJyZW50WWVhckVsZW1lbnQuZGlzYWJsZWQgPSBpbnZlcnNlRGF0ZU9iaiAmJiBkYXRlT2JqICYmIGludmVyc2VEYXRlT2JqLmdldEZ1bGxZZWFyKCkgPT09IGRhdGVPYmouZ2V0RnVsbFllYXIoKTtcblx0XHRcdH1cblx0XHR9O1xuXHR9XG5cblx0ZnVuY3Rpb24gcGFyc2VDb25maWcoKSB7XG5cdFx0dmFyIGJvb2xPcHRzID0gW1widXRjXCIsIFwid3JhcFwiLCBcIndlZWtOdW1iZXJzXCIsIFwiYWxsb3dJbnB1dFwiLCBcImNsaWNrT3BlbnNcIiwgXCJ0aW1lXzI0aHJcIiwgXCJlbmFibGVUaW1lXCIsIFwibm9DYWxlbmRhclwiLCBcImFsdElucHV0XCIsIFwic2hvcnRoYW5kQ3VycmVudE1vbnRoXCIsIFwiaW5saW5lXCIsIFwic3RhdGljXCIsIFwiZW5hYmxlU2Vjb25kc1wiLCBcImRpc2FibGVNb2JpbGVcIl07XG5cblx0XHR2YXIgaG9va3MgPSBbXCJvbkNoYW5nZVwiLCBcIm9uQ2xvc2VcIiwgXCJvbkRheUNyZWF0ZVwiLCBcIm9uTW9udGhDaGFuZ2VcIiwgXCJvbk9wZW5cIiwgXCJvblBhcnNlQ29uZmlnXCIsIFwib25SZWFkeVwiLCBcIm9uVmFsdWVVcGRhdGVcIiwgXCJvblllYXJDaGFuZ2VcIl07XG5cblx0XHRzZWxmLmNvbmZpZyA9IE9iamVjdC5jcmVhdGUoRmxhdHBpY2tyLmRlZmF1bHRDb25maWcpO1xuXG5cdFx0dmFyIHVzZXJDb25maWcgPSBfZXh0ZW5kcyh7fSwgc2VsZi5pbnN0YW5jZUNvbmZpZywgSlNPTi5wYXJzZShKU09OLnN0cmluZ2lmeShzZWxmLmVsZW1lbnQuZGF0YXNldCB8fCB7fSkpKTtcblxuXHRcdHNlbGYuY29uZmlnLnBhcnNlRGF0ZSA9IHVzZXJDb25maWcucGFyc2VEYXRlO1xuXHRcdHNlbGYuY29uZmlnLmZvcm1hdERhdGUgPSB1c2VyQ29uZmlnLmZvcm1hdERhdGU7XG5cblx0XHRfZXh0ZW5kcyhzZWxmLmNvbmZpZywgdXNlckNvbmZpZyk7XG5cblx0XHRpZiAoIXVzZXJDb25maWcuZGF0ZUZvcm1hdCAmJiB1c2VyQ29uZmlnLmVuYWJsZVRpbWUpIHtcblx0XHRcdHNlbGYuY29uZmlnLmRhdGVGb3JtYXQgPSBzZWxmLmNvbmZpZy5ub0NhbGVuZGFyID8gXCJIOmlcIiArIChzZWxmLmNvbmZpZy5lbmFibGVTZWNvbmRzID8gXCI6U1wiIDogXCJcIikgOiBGbGF0cGlja3IuZGVmYXVsdENvbmZpZy5kYXRlRm9ybWF0ICsgXCIgSDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpO1xuXHRcdH1cblxuXHRcdGlmICh1c2VyQ29uZmlnLmFsdElucHV0ICYmIHVzZXJDb25maWcuZW5hYmxlVGltZSAmJiAhdXNlckNvbmZpZy5hbHRGb3JtYXQpIHtcblx0XHRcdHNlbGYuY29uZmlnLmFsdEZvcm1hdCA9IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgPyBcImg6aVwiICsgKHNlbGYuY29uZmlnLmVuYWJsZVNlY29uZHMgPyBcIjpTIEtcIiA6IFwiIEtcIikgOiBGbGF0cGlja3IuZGVmYXVsdENvbmZpZy5hbHRGb3JtYXQgKyAoXCIgaDppXCIgKyAoc2VsZi5jb25maWcuZW5hYmxlU2Vjb25kcyA/IFwiOlNcIiA6IFwiXCIpICsgXCIgS1wiKTtcblx0XHR9XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZi5jb25maWcsIFwibWluRGF0ZVwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX21pbkRhdGU7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBtaW5NYXhEYXRlU2V0dGVyKFwibWluXCIpXG5cdFx0fSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZi5jb25maWcsIFwibWF4RGF0ZVwiLCB7XG5cdFx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdFx0cmV0dXJuIHRoaXMuX21heERhdGU7XG5cdFx0XHR9LFxuXHRcdFx0c2V0OiBtaW5NYXhEYXRlU2V0dGVyKFwibWF4XCIpXG5cdFx0fSk7XG5cblx0XHRzZWxmLmNvbmZpZy5taW5EYXRlID0gdXNlckNvbmZpZy5taW5EYXRlO1xuXHRcdHNlbGYuY29uZmlnLm1heERhdGUgPSB1c2VyQ29uZmlnLm1heERhdGU7XG5cblx0XHRmb3IgKHZhciBpID0gMDsgaSA8IGJvb2xPcHRzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPSBzZWxmLmNvbmZpZ1tib29sT3B0c1tpXV0gPT09IHRydWUgfHwgc2VsZi5jb25maWdbYm9vbE9wdHNbaV1dID09PSBcInRydWVcIjtcblx0XHR9Zm9yICh2YXIgX2kgPSAwOyBfaSA8IGhvb2tzLmxlbmd0aDsgX2krKykge1xuXHRcdFx0c2VsZi5jb25maWdbaG9va3NbX2ldXSA9IGFycmF5aWZ5KHNlbGYuY29uZmlnW2hvb2tzW19pXV0gfHwgW10pLm1hcChiaW5kVG9JbnN0YW5jZSk7XG5cdFx0fWZvciAodmFyIF9pMiA9IDA7IF9pMiA8IHNlbGYuY29uZmlnLnBsdWdpbnMubGVuZ3RoOyBfaTIrKykge1xuXHRcdFx0dmFyIHBsdWdpbkNvbmYgPSBzZWxmLmNvbmZpZy5wbHVnaW5zW19pMl0oc2VsZikgfHwge307XG5cdFx0XHRmb3IgKHZhciBrZXkgaW4gcGx1Z2luQ29uZikge1xuXHRcdFx0XHRpZiAoQXJyYXkuaXNBcnJheShzZWxmLmNvbmZpZ1trZXldKSkgc2VsZi5jb25maWdba2V5XSA9IGFycmF5aWZ5KHBsdWdpbkNvbmZba2V5XSkubWFwKGJpbmRUb0luc3RhbmNlKS5jb25jYXQoc2VsZi5jb25maWdba2V5XSk7ZWxzZSBpZiAodHlwZW9mIHVzZXJDb25maWdba2V5XSA9PT0gXCJ1bmRlZmluZWRcIikgc2VsZi5jb25maWdba2V5XSA9IHBsdWdpbkNvbmZba2V5XTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJQYXJzZUNvbmZpZ1wiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldHVwTG9jYWxlKCkge1xuXHRcdGlmIChfdHlwZW9mKHNlbGYuY29uZmlnLmxvY2FsZSkgIT09IFwib2JqZWN0XCIgJiYgdHlwZW9mIEZsYXRwaWNrci5sMTBuc1tzZWxmLmNvbmZpZy5sb2NhbGVdID09PSBcInVuZGVmaW5lZFwiKSBjb25zb2xlLndhcm4oXCJmbGF0cGlja3I6IGludmFsaWQgbG9jYWxlIFwiICsgc2VsZi5jb25maWcubG9jYWxlKTtcblxuXHRcdHNlbGYubDEwbiA9IF9leHRlbmRzKE9iamVjdC5jcmVhdGUoRmxhdHBpY2tyLmwxMG5zLmRlZmF1bHQpLCBfdHlwZW9mKHNlbGYuY29uZmlnLmxvY2FsZSkgPT09IFwib2JqZWN0XCIgPyBzZWxmLmNvbmZpZy5sb2NhbGUgOiBzZWxmLmNvbmZpZy5sb2NhbGUgIT09IFwiZGVmYXVsdFwiID8gRmxhdHBpY2tyLmwxMG5zW3NlbGYuY29uZmlnLmxvY2FsZV0gfHwge30gOiB7fSk7XG5cdH1cblxuXHRmdW5jdGlvbiBwb3NpdGlvbkNhbGVuZGFyKGUpIHtcblx0XHRpZiAoZSAmJiBlLnRhcmdldCAhPT0gc2VsZi50aW1lQ29udGFpbmVyKSByZXR1cm47XG5cblx0XHR2YXIgY2FsZW5kYXJIZWlnaHQgPSBzZWxmLmNhbGVuZGFyQ29udGFpbmVyLm9mZnNldEhlaWdodCxcblx0XHQgICAgY2FsZW5kYXJXaWR0aCA9IHNlbGYuY2FsZW5kYXJDb250YWluZXIub2Zmc2V0V2lkdGgsXG5cdFx0ICAgIGNvbmZpZ1BvcyA9IHNlbGYuY29uZmlnLnBvc2l0aW9uLFxuXHRcdCAgICBpbnB1dCA9IHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCxcblx0XHQgICAgaW5wdXRCb3VuZHMgPSBpbnB1dC5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKSxcblx0XHQgICAgZGlzdGFuY2VGcm9tQm90dG9tID0gd2luZG93LmlubmVySGVpZ2h0IC0gaW5wdXRCb3VuZHMuYm90dG9tICsgaW5wdXQub2Zmc2V0SGVpZ2h0LFxuXHRcdCAgICBzaG93T25Ub3AgPSBjb25maWdQb3MgPT09IFwiYWJvdmVcIiB8fCBjb25maWdQb3MgIT09IFwiYmVsb3dcIiAmJiBkaXN0YW5jZUZyb21Cb3R0b20gPCBjYWxlbmRhckhlaWdodCArIDYwO1xuXG5cdFx0dmFyIHRvcCA9IHdpbmRvdy5wYWdlWU9mZnNldCArIGlucHV0Qm91bmRzLnRvcCArICghc2hvd09uVG9wID8gaW5wdXQub2Zmc2V0SGVpZ2h0ICsgMiA6IC1jYWxlbmRhckhlaWdodCAtIDIpO1xuXG5cdFx0dG9nZ2xlQ2xhc3Moc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgXCJhcnJvd1RvcFwiLCAhc2hvd09uVG9wKTtcblx0XHR0b2dnbGVDbGFzcyhzZWxmLmNhbGVuZGFyQ29udGFpbmVyLCBcImFycm93Qm90dG9tXCIsIHNob3dPblRvcCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuaW5saW5lKSByZXR1cm47XG5cblx0XHR2YXIgbGVmdCA9IHdpbmRvdy5wYWdlWE9mZnNldCArIGlucHV0Qm91bmRzLmxlZnQ7XG5cdFx0dmFyIHJpZ2h0ID0gd2luZG93LmRvY3VtZW50LmJvZHkub2Zmc2V0V2lkdGggLSBpbnB1dEJvdW5kcy5yaWdodDtcblx0XHR2YXIgcmlnaHRNb3N0ID0gbGVmdCArIGNhbGVuZGFyV2lkdGggPiB3aW5kb3cuZG9jdW1lbnQuYm9keS5vZmZzZXRXaWR0aDtcblxuXHRcdHRvZ2dsZUNsYXNzKHNlbGYuY2FsZW5kYXJDb250YWluZXIsIFwicmlnaHRNb3N0XCIsIHJpZ2h0TW9zdCk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuc3RhdGljKSByZXR1cm47XG5cblx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLnRvcCA9IHRvcCArIFwicHhcIjtcblxuXHRcdGlmICghcmlnaHRNb3N0KSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLmxlZnQgPSBsZWZ0ICsgXCJweFwiO1xuXHRcdFx0c2VsZi5jYWxlbmRhckNvbnRhaW5lci5zdHlsZS5yaWdodCA9IFwiYXV0b1wiO1xuXHRcdH0gZWxzZSB7XG5cdFx0XHRzZWxmLmNhbGVuZGFyQ29udGFpbmVyLnN0eWxlLmxlZnQgPSBcImF1dG9cIjtcblx0XHRcdHNlbGYuY2FsZW5kYXJDb250YWluZXIuc3R5bGUucmlnaHQgPSByaWdodCArIFwicHhcIjtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiByZWRyYXcoKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgfHwgc2VsZi5pc01vYmlsZSkgcmV0dXJuO1xuXG5cdFx0YnVpbGRXZWVrZGF5cygpO1xuXHRcdHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblx0XHRidWlsZERheXMoKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNlbGVjdERhdGUoZSkge1xuXHRcdGUucHJldmVudERlZmF1bHQoKTtcblx0XHRlLnN0b3BQcm9wYWdhdGlvbigpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmFsbG93SW5wdXQgJiYgZS53aGljaCA9PT0gMTMgJiYgZS50YXJnZXQgPT09IChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpKSB7XG5cdFx0XHRzZWxmLnNldERhdGUoKHNlbGYuYWx0SW5wdXQgfHwgc2VsZi5pbnB1dCkudmFsdWUsIHRydWUsIGUudGFyZ2V0ID09PSBzZWxmLmFsdElucHV0ID8gc2VsZi5jb25maWcuYWx0Rm9ybWF0IDogc2VsZi5jb25maWcuZGF0ZUZvcm1hdCk7XG5cdFx0XHRyZXR1cm4gZS50YXJnZXQuYmx1cigpO1xuXHRcdH1cblxuXHRcdGlmICghZS50YXJnZXQuY2xhc3NMaXN0LmNvbnRhaW5zKFwiZmxhdHBpY2tyLWRheVwiKSB8fCBlLnRhcmdldC5jbGFzc0xpc3QuY29udGFpbnMoXCJkaXNhYmxlZFwiKSB8fCBlLnRhcmdldC5jbGFzc0xpc3QuY29udGFpbnMoXCJub3RBbGxvd2VkXCIpKSByZXR1cm47XG5cblx0XHR2YXIgc2VsZWN0ZWREYXRlID0gc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBuZXcgRGF0ZShlLnRhcmdldC5kYXRlT2JqLmdldFRpbWUoKSk7XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZUVsZW0gPSBlLnRhcmdldDtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInNpbmdsZVwiKSBzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZWN0ZWREYXRlXTtlbHNlIGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcIm11bHRpcGxlXCIpIHtcblx0XHRcdHZhciBzZWxlY3RlZEluZGV4ID0gaXNEYXRlU2VsZWN0ZWQoc2VsZWN0ZWREYXRlKTtcblx0XHRcdGlmIChzZWxlY3RlZEluZGV4KSBzZWxmLnNlbGVjdGVkRGF0ZXMuc3BsaWNlKHNlbGVjdGVkSW5kZXgsIDEpO2Vsc2Ugc2VsZi5zZWxlY3RlZERhdGVzLnB1c2goc2VsZWN0ZWREYXRlKTtcblx0XHR9IGVsc2UgaWYgKHNlbGYuY29uZmlnLm1vZGUgPT09IFwicmFuZ2VcIikge1xuXHRcdFx0aWYgKHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggPT09IDIpIHNlbGYuY2xlYXIoKTtcblxuXHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzLnB1c2goc2VsZWN0ZWREYXRlKTtcblxuXHRcdFx0Ly8gdW5sZXNzIHNlbGVjdGluZyBzYW1lIGRhdGUgdHdpY2UsIHNvcnQgYXNjZW5kaW5nbHlcblx0XHRcdGlmIChjb21wYXJlRGF0ZXMoc2VsZWN0ZWREYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMF0sIHRydWUpICE9PSAwKSBzZWxmLnNlbGVjdGVkRGF0ZXMuc29ydChmdW5jdGlvbiAoYSwgYikge1xuXHRcdFx0XHRyZXR1cm4gYS5nZXRUaW1lKCkgLSBiLmdldFRpbWUoKTtcblx0XHRcdH0pO1xuXHRcdH1cblxuXHRcdHNldEhvdXJzRnJvbUlucHV0cygpO1xuXG5cdFx0aWYgKHNlbGVjdGVkRGF0ZS5nZXRNb250aCgpICE9PSBzZWxmLmN1cnJlbnRNb250aCAmJiBzZWxmLmNvbmZpZy5tb2RlICE9PSBcInJhbmdlXCIpIHtcblx0XHRcdHZhciBpc05ld1llYXIgPSBzZWxmLmN1cnJlbnRZZWFyICE9PSBzZWxlY3RlZERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudFllYXIgPSBzZWxlY3RlZERhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHRcdHNlbGYuY3VycmVudE1vbnRoID0gc2VsZWN0ZWREYXRlLmdldE1vbnRoKCk7XG5cblx0XHRcdGlmIChpc05ld1llYXIpIHRyaWdnZXJFdmVudChcIlllYXJDaGFuZ2VcIik7XG5cblx0XHRcdHRyaWdnZXJFdmVudChcIk1vbnRoQ2hhbmdlXCIpO1xuXHRcdH1cblxuXHRcdGJ1aWxkRGF5cygpO1xuXG5cdFx0aWYgKHNlbGYubWluRGF0ZUhhc1RpbWUgJiYgc2VsZi5jb25maWcuZW5hYmxlVGltZSAmJiBjb21wYXJlRGF0ZXMoc2VsZWN0ZWREYXRlLCBzZWxmLmNvbmZpZy5taW5EYXRlKSA9PT0gMCkgc2V0SG91cnNGcm9tRGF0ZShzZWxmLmNvbmZpZy5taW5EYXRlKTtcblxuXHRcdHVwZGF0ZVZhbHVlKCk7XG5cblx0XHRzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdHJldHVybiBzZWxmLnNob3dUaW1lSW5wdXQgPSB0cnVlO1xuXHRcdH0sIDUwKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInJhbmdlXCIpIHtcblx0XHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID09PSAxKSB7XG5cdFx0XHRcdG9uTW91c2VPdmVyKGUpO1xuXG5cdFx0XHRcdHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyA9IHNlbGYuX2hpZGVQcmV2TW9udGhBcnJvdyB8fCBzZWxmLm1pblJhbmdlRGF0ZSA+IHNlbGYuZGF5cy5jaGlsZE5vZGVzWzBdLmRhdGVPYmo7XG5cblx0XHRcdFx0c2VsZi5faGlkZU5leHRNb250aEFycm93ID0gc2VsZi5faGlkZU5leHRNb250aEFycm93IHx8IHNlbGYubWF4UmFuZ2VEYXRlIDwgbmV3IERhdGUoc2VsZi5jdXJyZW50WWVhciwgc2VsZi5jdXJyZW50TW9udGggKyAxLCAxKTtcblx0XHRcdH0gZWxzZSB7XG5cdFx0XHRcdHVwZGF0ZU5hdmlnYXRpb25DdXJyZW50TW9udGgoKTtcblx0XHRcdFx0c2VsZi5jbG9zZSgpO1xuXHRcdFx0fVxuXHRcdH1cblxuXHRcdGlmIChlLndoaWNoID09PSAxMyAmJiBzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSBzZXRUaW1lb3V0KGZ1bmN0aW9uICgpIHtcblx0XHRcdHJldHVybiBzZWxmLmhvdXJFbGVtZW50LmZvY3VzKCk7XG5cdFx0fSwgNDUxKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5tb2RlID09PSBcInNpbmdsZVwiICYmICFzZWxmLmNvbmZpZy5lbmFibGVUaW1lKSBzZWxmLmNsb3NlKCk7XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXQob3B0aW9uLCB2YWx1ZSkge1xuXHRcdHNlbGYuY29uZmlnW29wdGlvbl0gPSB2YWx1ZTtcblx0XHRzZWxmLnJlZHJhdygpO1xuXHRcdGp1bXBUb0RhdGUoKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldFNlbGVjdGVkRGF0ZShpbnB1dERhdGUsIGZvcm1hdCkge1xuXHRcdGlmIChBcnJheS5pc0FycmF5KGlucHV0RGF0ZSkpIHNlbGYuc2VsZWN0ZWREYXRlcyA9IGlucHV0RGF0ZS5tYXAoZnVuY3Rpb24gKGQpIHtcblx0XHRcdHJldHVybiBzZWxmLnBhcnNlRGF0ZShkLCBmYWxzZSwgZm9ybWF0KTtcblx0XHR9KTtlbHNlIGlmIChpbnB1dERhdGUgaW5zdGFuY2VvZiBEYXRlIHx8ICFpc05hTihpbnB1dERhdGUpKSBzZWxmLnNlbGVjdGVkRGF0ZXMgPSBbc2VsZi5wYXJzZURhdGUoaW5wdXREYXRlKV07ZWxzZSBpZiAoaW5wdXREYXRlICYmIGlucHV0RGF0ZS5zdWJzdHJpbmcpIHtcblx0XHRcdHN3aXRjaCAoc2VsZi5jb25maWcubW9kZSkge1xuXHRcdFx0XHRjYXNlIFwic2luZ2xlXCI6XG5cdFx0XHRcdFx0c2VsZi5zZWxlY3RlZERhdGVzID0gW3NlbGYucGFyc2VEYXRlKGlucHV0RGF0ZSwgZmFsc2UsIGZvcm1hdCldO1xuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJtdWx0aXBsZVwiOlxuXHRcdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IGlucHV0RGF0ZS5zcGxpdChcIjsgXCIpLm1hcChmdW5jdGlvbiAoZGF0ZSkge1xuXHRcdFx0XHRcdFx0cmV0dXJuIHNlbGYucGFyc2VEYXRlKGRhdGUsIGZhbHNlLCBmb3JtYXQpO1xuXHRcdFx0XHRcdH0pO1xuXHRcdFx0XHRcdGJyZWFrO1xuXG5cdFx0XHRcdGNhc2UgXCJyYW5nZVwiOlxuXHRcdFx0XHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IGlucHV0RGF0ZS5zcGxpdChzZWxmLmwxMG4ucmFuZ2VTZXBhcmF0b3IpLm1hcChmdW5jdGlvbiAoZGF0ZSkge1xuXHRcdFx0XHRcdFx0cmV0dXJuIHNlbGYucGFyc2VEYXRlKGRhdGUsIGZhbHNlLCBmb3JtYXQpO1xuXHRcdFx0XHRcdH0pO1xuXG5cdFx0XHRcdFx0YnJlYWs7XG5cblx0XHRcdFx0ZGVmYXVsdDpcblx0XHRcdFx0XHRicmVhaztcblx0XHRcdH1cblx0XHR9XG5cblx0XHRzZWxmLnNlbGVjdGVkRGF0ZXMgPSBzZWxmLnNlbGVjdGVkRGF0ZXMuZmlsdGVyKGZ1bmN0aW9uIChkKSB7XG5cdFx0XHRyZXR1cm4gZCBpbnN0YW5jZW9mIERhdGUgJiYgZC5nZXRUaW1lKCkgJiYgaXNFbmFibGVkKGQsIGZhbHNlKTtcblx0XHR9KTtcblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlcy5zb3J0KGZ1bmN0aW9uIChhLCBiKSB7XG5cdFx0XHRyZXR1cm4gYS5nZXRUaW1lKCkgLSBiLmdldFRpbWUoKTtcblx0XHR9KTtcblx0fVxuXG5cdGZ1bmN0aW9uIHNldERhdGUoZGF0ZSwgdHJpZ2dlckNoYW5nZSwgZm9ybWF0KSB7XG5cdFx0aWYgKCFkYXRlKSByZXR1cm4gc2VsZi5jbGVhcigpO1xuXG5cdFx0c2V0U2VsZWN0ZWREYXRlKGRhdGUsIGZvcm1hdCk7XG5cblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCA+IDApIHtcblx0XHRcdHNlbGYuc2hvd1RpbWVJbnB1dCA9IHRydWU7XG5cdFx0XHRzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IHNlbGYuc2VsZWN0ZWREYXRlc1swXTtcblx0XHR9IGVsc2Ugc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmogPSBudWxsO1xuXG5cdFx0c2VsZi5yZWRyYXcoKTtcblx0XHRqdW1wVG9EYXRlKCk7XG5cblx0XHRzZXRIb3Vyc0Zyb21EYXRlKCk7XG5cdFx0dXBkYXRlVmFsdWUoKTtcblxuXHRcdGlmICh0cmlnZ2VyQ2hhbmdlICE9PSBmYWxzZSkgdHJpZ2dlckV2ZW50KFwiQ2hhbmdlXCIpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0dXBEYXRlcygpIHtcblx0XHRmdW5jdGlvbiBwYXJzZURhdGVSdWxlcyhhcnIpIHtcblx0XHRcdGZvciAodmFyIGkgPSBhcnIubGVuZ3RoOyBpLS07KSB7XG5cdFx0XHRcdGlmICh0eXBlb2YgYXJyW2ldID09PSBcInN0cmluZ1wiIHx8ICthcnJbaV0pIGFycltpXSA9IHNlbGYucGFyc2VEYXRlKGFycltpXSwgdHJ1ZSk7ZWxzZSBpZiAoYXJyW2ldICYmIGFycltpXS5mcm9tICYmIGFycltpXS50bykge1xuXHRcdFx0XHRcdGFycltpXS5mcm9tID0gc2VsZi5wYXJzZURhdGUoYXJyW2ldLmZyb20pO1xuXHRcdFx0XHRcdGFycltpXS50byA9IHNlbGYucGFyc2VEYXRlKGFycltpXS50byk7XG5cdFx0XHRcdH1cblx0XHRcdH1cblxuXHRcdFx0cmV0dXJuIGFyci5maWx0ZXIoZnVuY3Rpb24gKHgpIHtcblx0XHRcdFx0cmV0dXJuIHg7XG5cdFx0XHR9KTsgLy8gcmVtb3ZlIGZhbHN5IHZhbHVlc1xuXHRcdH1cblxuXHRcdHNlbGYuc2VsZWN0ZWREYXRlcyA9IFtdO1xuXHRcdHNlbGYubm93ID0gbmV3IERhdGUoKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5kaXNhYmxlLmxlbmd0aCkgc2VsZi5jb25maWcuZGlzYWJsZSA9IHBhcnNlRGF0ZVJ1bGVzKHNlbGYuY29uZmlnLmRpc2FibGUpO1xuXG5cdFx0aWYgKHNlbGYuY29uZmlnLmVuYWJsZS5sZW5ndGgpIHNlbGYuY29uZmlnLmVuYWJsZSA9IHBhcnNlRGF0ZVJ1bGVzKHNlbGYuY29uZmlnLmVuYWJsZSk7XG5cblx0XHRzZXRTZWxlY3RlZERhdGUoc2VsZi5jb25maWcuZGVmYXVsdERhdGUgfHwgc2VsZi5pbnB1dC52YWx1ZSk7XG5cblx0XHR2YXIgaW5pdGlhbERhdGUgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID8gc2VsZi5zZWxlY3RlZERhdGVzWzBdIDogc2VsZi5jb25maWcubWluRGF0ZSAmJiBzZWxmLmNvbmZpZy5taW5EYXRlLmdldFRpbWUoKSA+IHNlbGYubm93ID8gc2VsZi5jb25maWcubWluRGF0ZSA6IHNlbGYuY29uZmlnLm1heERhdGUgJiYgc2VsZi5jb25maWcubWF4RGF0ZS5nZXRUaW1lKCkgPCBzZWxmLm5vdyA/IHNlbGYuY29uZmlnLm1heERhdGUgOiBzZWxmLm5vdztcblxuXHRcdHNlbGYuY3VycmVudFllYXIgPSBpbml0aWFsRGF0ZS5nZXRGdWxsWWVhcigpO1xuXHRcdHNlbGYuY3VycmVudE1vbnRoID0gaW5pdGlhbERhdGUuZ2V0TW9udGgoKTtcblxuXHRcdGlmIChzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSBzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IHNlbGYuc2VsZWN0ZWREYXRlc1swXTtcblxuXHRcdHNlbGYubWluRGF0ZUhhc1RpbWUgPSBzZWxmLmNvbmZpZy5taW5EYXRlICYmIChzZWxmLmNvbmZpZy5taW5EYXRlLmdldEhvdXJzKCkgfHwgc2VsZi5jb25maWcubWluRGF0ZS5nZXRNaW51dGVzKCkgfHwgc2VsZi5jb25maWcubWluRGF0ZS5nZXRTZWNvbmRzKCkpO1xuXG5cdFx0c2VsZi5tYXhEYXRlSGFzVGltZSA9IHNlbGYuY29uZmlnLm1heERhdGUgJiYgKHNlbGYuY29uZmlnLm1heERhdGUuZ2V0SG91cnMoKSB8fCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldE1pbnV0ZXMoKSB8fCBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldFNlY29uZHMoKSk7XG5cblx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoc2VsZiwgXCJsYXRlc3RTZWxlY3RlZERhdGVPYmpcIiwge1xuXHRcdFx0Z2V0OiBmdW5jdGlvbiBnZXQoKSB7XG5cdFx0XHRcdHJldHVybiBzZWxmLl9zZWxlY3RlZERhdGVPYmogfHwgc2VsZi5zZWxlY3RlZERhdGVzW3NlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGggLSAxXSB8fCBudWxsO1xuXHRcdFx0fSxcblx0XHRcdHNldDogZnVuY3Rpb24gc2V0KGRhdGUpIHtcblx0XHRcdFx0c2VsZi5fc2VsZWN0ZWREYXRlT2JqID0gZGF0ZTtcblx0XHRcdH1cblx0XHR9KTtcblxuXHRcdGlmICghc2VsZi5pc01vYmlsZSkge1xuXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KHNlbGYsIFwic2hvd1RpbWVJbnB1dFwiLCB7XG5cdFx0XHRcdHNldDogZnVuY3Rpb24gc2V0KGJvb2wpIHtcblx0XHRcdFx0XHRpZiAoc2VsZi5jYWxlbmRhckNvbnRhaW5lcikgdG9nZ2xlQ2xhc3Moc2VsZi5jYWxlbmRhckNvbnRhaW5lciwgXCJzaG93VGltZUlucHV0XCIsIGJvb2wpO1xuXHRcdFx0XHR9XG5cdFx0XHR9KTtcblx0XHR9XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cEhlbHBlckZ1bmN0aW9ucygpIHtcblx0XHRzZWxmLnV0aWxzID0ge1xuXHRcdFx0ZHVyYXRpb246IHtcblx0XHRcdFx0REFZOiA4NjQwMDAwMFxuXHRcdFx0fSxcblx0XHRcdGdldERheXNpbk1vbnRoOiBmdW5jdGlvbiBnZXREYXlzaW5Nb250aChtb250aCwgeXIpIHtcblx0XHRcdFx0bW9udGggPSB0eXBlb2YgbW9udGggPT09IFwidW5kZWZpbmVkXCIgPyBzZWxmLmN1cnJlbnRNb250aCA6IG1vbnRoO1xuXG5cdFx0XHRcdHlyID0gdHlwZW9mIHlyID09PSBcInVuZGVmaW5lZFwiID8gc2VsZi5jdXJyZW50WWVhciA6IHlyO1xuXG5cdFx0XHRcdGlmIChtb250aCA9PT0gMSAmJiAoeXIgJSA0ID09PSAwICYmIHlyICUgMTAwICE9PSAwIHx8IHlyICUgNDAwID09PSAwKSkgcmV0dXJuIDI5O1xuXG5cdFx0XHRcdHJldHVybiBzZWxmLmwxMG4uZGF5c0luTW9udGhbbW9udGhdO1xuXHRcdFx0fSxcblx0XHRcdG1vbnRoVG9TdHI6IGZ1bmN0aW9uIG1vbnRoVG9TdHIobW9udGhOdW1iZXIsIHNob3J0aGFuZCkge1xuXHRcdFx0XHRzaG9ydGhhbmQgPSB0eXBlb2Ygc2hvcnRoYW5kID09PSBcInVuZGVmaW5lZFwiID8gc2VsZi5jb25maWcuc2hvcnRoYW5kQ3VycmVudE1vbnRoIDogc2hvcnRoYW5kO1xuXG5cdFx0XHRcdHJldHVybiBzZWxmLmwxMG4ubW9udGhzWyhzaG9ydGhhbmQgPyBcInNob3J0XCIgOiBcImxvbmdcIikgKyBcImhhbmRcIl1bbW9udGhOdW1iZXJdO1xuXHRcdFx0fVxuXHRcdH07XG5cdH1cblxuXHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRmdW5jdGlvbiBzZXR1cEZvcm1hdHMoKSB7XG5cdFx0W1wiRFwiLCBcIkZcIiwgXCJKXCIsIFwiTVwiLCBcIldcIiwgXCJsXCJdLmZvckVhY2goZnVuY3Rpb24gKGYpIHtcblx0XHRcdHNlbGYuZm9ybWF0c1tmXSA9IEZsYXRwaWNrci5wcm90b3R5cGUuZm9ybWF0c1tmXS5iaW5kKHNlbGYpO1xuXHRcdH0pO1xuXG5cdFx0c2VsZi5yZXZGb3JtYXQuRiA9IEZsYXRwaWNrci5wcm90b3R5cGUucmV2Rm9ybWF0LkYuYmluZChzZWxmKTtcblx0XHRzZWxmLnJldkZvcm1hdC5NID0gRmxhdHBpY2tyLnByb3RvdHlwZS5yZXZGb3JtYXQuTS5iaW5kKHNlbGYpO1xuXHR9XG5cblx0ZnVuY3Rpb24gc2V0dXBJbnB1dHMoKSB7XG5cdFx0c2VsZi5pbnB1dCA9IHNlbGYuY29uZmlnLndyYXAgPyBzZWxmLmVsZW1lbnQucXVlcnlTZWxlY3RvcihcIltkYXRhLWlucHV0XVwiKSA6IHNlbGYuZWxlbWVudDtcblxuXHRcdC8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5cdFx0aWYgKCFzZWxmLmlucHV0KSByZXR1cm4gY29uc29sZS53YXJuKFwiRXJyb3I6IGludmFsaWQgaW5wdXQgZWxlbWVudCBzcGVjaWZpZWRcIiwgc2VsZi5pbnB1dCk7XG5cblx0XHRzZWxmLmlucHV0Ll90eXBlID0gc2VsZi5pbnB1dC50eXBlO1xuXHRcdHNlbGYuaW5wdXQudHlwZSA9IFwidGV4dFwiO1xuXHRcdHNlbGYuaW5wdXQuY2xhc3NMaXN0LmFkZChcImZsYXRwaWNrci1pbnB1dFwiKTtcblxuXHRcdGlmIChzZWxmLmNvbmZpZy5hbHRJbnB1dCkge1xuXHRcdFx0Ly8gcmVwbGljYXRlIHNlbGYuZWxlbWVudFxuXHRcdFx0c2VsZi5hbHRJbnB1dCA9IGNyZWF0ZUVsZW1lbnQoc2VsZi5pbnB1dC5ub2RlTmFtZSwgc2VsZi5pbnB1dC5jbGFzc05hbWUgKyBcIiBcIiArIHNlbGYuY29uZmlnLmFsdElucHV0Q2xhc3MpO1xuXHRcdFx0c2VsZi5hbHRJbnB1dC5wbGFjZWhvbGRlciA9IHNlbGYuaW5wdXQucGxhY2Vob2xkZXI7XG5cdFx0XHRzZWxmLmFsdElucHV0LnR5cGUgPSBcInRleHRcIjtcblx0XHRcdHNlbGYuaW5wdXQudHlwZSA9IFwiaGlkZGVuXCI7XG5cblx0XHRcdGlmICghc2VsZi5jb25maWcuc3RhdGljICYmIHNlbGYuaW5wdXQucGFyZW50Tm9kZSkgc2VsZi5pbnB1dC5wYXJlbnROb2RlLmluc2VydEJlZm9yZShzZWxmLmFsdElucHV0LCBzZWxmLmlucHV0Lm5leHRTaWJsaW5nKTtcblx0XHR9XG5cblx0XHRpZiAoIXNlbGYuY29uZmlnLmFsbG93SW5wdXQpIChzZWxmLmFsdElucHV0IHx8IHNlbGYuaW5wdXQpLnNldEF0dHJpYnV0ZShcInJlYWRvbmx5XCIsIFwicmVhZG9ubHlcIik7XG5cdH1cblxuXHRmdW5jdGlvbiBzZXR1cE1vYmlsZSgpIHtcblx0XHR2YXIgaW5wdXRUeXBlID0gc2VsZi5jb25maWcuZW5hYmxlVGltZSA/IHNlbGYuY29uZmlnLm5vQ2FsZW5kYXIgPyBcInRpbWVcIiA6IFwiZGF0ZXRpbWUtbG9jYWxcIiA6IFwiZGF0ZVwiO1xuXG5cdFx0c2VsZi5tb2JpbGVJbnB1dCA9IGNyZWF0ZUVsZW1lbnQoXCJpbnB1dFwiLCBzZWxmLmlucHV0LmNsYXNzTmFtZSArIFwiIGZsYXRwaWNrci1tb2JpbGVcIik7XG5cdFx0c2VsZi5tb2JpbGVJbnB1dC5zdGVwID0gXCJhbnlcIjtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnRhYkluZGV4ID0gMTtcblx0XHRzZWxmLm1vYmlsZUlucHV0LnR5cGUgPSBpbnB1dFR5cGU7XG5cdFx0c2VsZi5tb2JpbGVJbnB1dC5kaXNhYmxlZCA9IHNlbGYuaW5wdXQuZGlzYWJsZWQ7XG5cdFx0c2VsZi5tb2JpbGVJbnB1dC5wbGFjZWhvbGRlciA9IHNlbGYuaW5wdXQucGxhY2Vob2xkZXI7XG5cblx0XHRzZWxmLm1vYmlsZUZvcm1hdFN0ciA9IGlucHV0VHlwZSA9PT0gXCJkYXRldGltZS1sb2NhbFwiID8gXCJZLW0tZFxcXFxUSDppOlNcIiA6IGlucHV0VHlwZSA9PT0gXCJkYXRlXCIgPyBcIlktbS1kXCIgOiBcIkg6aTpTXCI7XG5cblx0XHRpZiAoc2VsZi5zZWxlY3RlZERhdGVzLmxlbmd0aCkge1xuXHRcdFx0c2VsZi5tb2JpbGVJbnB1dC5kZWZhdWx0VmFsdWUgPSBzZWxmLm1vYmlsZUlucHV0LnZhbHVlID0gZm9ybWF0RGF0ZShzZWxmLm1vYmlsZUZvcm1hdFN0ciwgc2VsZi5zZWxlY3RlZERhdGVzWzBdKTtcblx0XHR9XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWluRGF0ZSkgc2VsZi5tb2JpbGVJbnB1dC5taW4gPSBmb3JtYXREYXRlKFwiWS1tLWRcIiwgc2VsZi5jb25maWcubWluRGF0ZSk7XG5cblx0XHRpZiAoc2VsZi5jb25maWcubWF4RGF0ZSkgc2VsZi5tb2JpbGVJbnB1dC5tYXggPSBmb3JtYXREYXRlKFwiWS1tLWRcIiwgc2VsZi5jb25maWcubWF4RGF0ZSk7XG5cblx0XHRzZWxmLmlucHV0LnR5cGUgPSBcImhpZGRlblwiO1xuXHRcdGlmIChzZWxmLmNvbmZpZy5hbHRJbnB1dCkgc2VsZi5hbHRJbnB1dC50eXBlID0gXCJoaWRkZW5cIjtcblxuXHRcdHRyeSB7XG5cdFx0XHRzZWxmLmlucHV0LnBhcmVudE5vZGUuaW5zZXJ0QmVmb3JlKHNlbGYubW9iaWxlSW5wdXQsIHNlbGYuaW5wdXQubmV4dFNpYmxpbmcpO1xuXHRcdH0gY2F0Y2ggKGUpIHtcblx0XHRcdC8vXG5cdFx0fVxuXG5cdFx0c2VsZi5tb2JpbGVJbnB1dC5hZGRFdmVudExpc3RlbmVyKFwiY2hhbmdlXCIsIGZ1bmN0aW9uIChlKSB7XG5cdFx0XHRzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaiA9IHNlbGYucGFyc2VEYXRlKGUudGFyZ2V0LnZhbHVlKTtcblx0XHRcdHNlbGYuc2V0RGF0ZShzZWxmLmxhdGVzdFNlbGVjdGVkRGF0ZU9iaik7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJDaGFuZ2VcIik7XG5cdFx0XHR0cmlnZ2VyRXZlbnQoXCJDbG9zZVwiKTtcblx0XHR9KTtcblx0fVxuXG5cdGZ1bmN0aW9uIHRvZ2dsZSgpIHtcblx0XHRpZiAoc2VsZi5pc09wZW4pIHNlbGYuY2xvc2UoKTtlbHNlIHNlbGYub3BlbigpO1xuXHR9XG5cblx0ZnVuY3Rpb24gdHJpZ2dlckV2ZW50KGV2ZW50LCBkYXRhKSB7XG5cdFx0dmFyIGhvb2tzID0gc2VsZi5jb25maWdbXCJvblwiICsgZXZlbnRdO1xuXG5cdFx0aWYgKGhvb2tzKSB7XG5cdFx0XHRmb3IgKHZhciBpID0gMDsgaSA8IGhvb2tzLmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRcdGhvb2tzW2ldKHNlbGYuc2VsZWN0ZWREYXRlcywgc2VsZi5pbnB1dCAmJiBzZWxmLmlucHV0LnZhbHVlLCBzZWxmLCBkYXRhKTtcblx0XHRcdH1cblx0XHR9XG5cblx0XHRpZiAoZXZlbnQgPT09IFwiQ2hhbmdlXCIpIHtcblx0XHRcdGlmICh0eXBlb2YgRXZlbnQgPT09IFwiZnVuY3Rpb25cIiAmJiBFdmVudC5jb25zdHJ1Y3Rvcikge1xuXHRcdFx0XHRzZWxmLmlucHV0LmRpc3BhdGNoRXZlbnQobmV3IEV2ZW50KFwiY2hhbmdlXCIsIHsgXCJidWJibGVzXCI6IHRydWUgfSkpO1xuXG5cdFx0XHRcdC8vIG1hbnkgZnJvbnQtZW5kIGZyYW1ld29ya3MgYmluZCB0byB0aGUgaW5wdXQgZXZlbnRcblx0XHRcdFx0c2VsZi5pbnB1dC5kaXNwYXRjaEV2ZW50KG5ldyBFdmVudChcImlucHV0XCIsIHsgXCJidWJibGVzXCI6IHRydWUgfSkpO1xuXHRcdFx0fVxuXG5cdFx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdFx0ZWxzZSB7XG5cdFx0XHRcdFx0aWYgKHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFdmVudCAhPT0gdW5kZWZpbmVkKSByZXR1cm4gc2VsZi5pbnB1dC5kaXNwYXRjaEV2ZW50KHNlbGYuY2hhbmdlRXZlbnQpO1xuXG5cdFx0XHRcdFx0c2VsZi5pbnB1dC5maXJlRXZlbnQoXCJvbmNoYW5nZVwiKTtcblx0XHRcdFx0fVxuXHRcdH1cblx0fVxuXG5cdGZ1bmN0aW9uIGlzRGF0ZVNlbGVjdGVkKGRhdGUpIHtcblx0XHRmb3IgKHZhciBpID0gMDsgaSA8IHNlbGYuc2VsZWN0ZWREYXRlcy5sZW5ndGg7IGkrKykge1xuXHRcdFx0aWYgKGNvbXBhcmVEYXRlcyhzZWxmLnNlbGVjdGVkRGF0ZXNbaV0sIGRhdGUpID09PSAwKSByZXR1cm4gXCJcIiArIGk7XG5cdFx0fVxuXG5cdFx0cmV0dXJuIGZhbHNlO1xuXHR9XG5cblx0ZnVuY3Rpb24gaXNEYXRlSW5SYW5nZShkYXRlKSB7XG5cdFx0aWYgKHNlbGYuY29uZmlnLm1vZGUgIT09IFwicmFuZ2VcIiB8fCBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoIDwgMikgcmV0dXJuIGZhbHNlO1xuXHRcdHJldHVybiBjb21wYXJlRGF0ZXMoZGF0ZSwgc2VsZi5zZWxlY3RlZERhdGVzWzBdKSA+PSAwICYmIGNvbXBhcmVEYXRlcyhkYXRlLCBzZWxmLnNlbGVjdGVkRGF0ZXNbMV0pIDw9IDA7XG5cdH1cblxuXHRmdW5jdGlvbiB1cGRhdGVOYXZpZ2F0aW9uQ3VycmVudE1vbnRoKCkge1xuXHRcdGlmIChzZWxmLmNvbmZpZy5ub0NhbGVuZGFyIHx8IHNlbGYuaXNNb2JpbGUgfHwgIXNlbGYubW9udGhOYXYpIHJldHVybjtcblxuXHRcdHNlbGYuY3VycmVudE1vbnRoRWxlbWVudC50ZXh0Q29udGVudCA9IHNlbGYudXRpbHMubW9udGhUb1N0cihzZWxmLmN1cnJlbnRNb250aCkgKyBcIiBcIjtcblx0XHRzZWxmLmN1cnJlbnRZZWFyRWxlbWVudC52YWx1ZSA9IHNlbGYuY3VycmVudFllYXI7XG5cblx0XHRzZWxmLl9oaWRlUHJldk1vbnRoQXJyb3cgPSBzZWxmLmNvbmZpZy5taW5EYXRlICYmIChzZWxmLmN1cnJlbnRZZWFyID09PSBzZWxmLmNvbmZpZy5taW5EYXRlLmdldEZ1bGxZZWFyKCkgPyBzZWxmLmN1cnJlbnRNb250aCA8PSBzZWxmLmNvbmZpZy5taW5EYXRlLmdldE1vbnRoKCkgOiBzZWxmLmN1cnJlbnRZZWFyIDwgc2VsZi5jb25maWcubWluRGF0ZS5nZXRGdWxsWWVhcigpKTtcblxuXHRcdHNlbGYuX2hpZGVOZXh0TW9udGhBcnJvdyA9IHNlbGYuY29uZmlnLm1heERhdGUgJiYgKHNlbGYuY3VycmVudFllYXIgPT09IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0RnVsbFllYXIoKSA/IHNlbGYuY3VycmVudE1vbnRoICsgMSA+IHNlbGYuY29uZmlnLm1heERhdGUuZ2V0TW9udGgoKSA6IHNlbGYuY3VycmVudFllYXIgPiBzZWxmLmNvbmZpZy5tYXhEYXRlLmdldEZ1bGxZZWFyKCkpO1xuXHR9XG5cblx0ZnVuY3Rpb24gdXBkYXRlVmFsdWUoKSB7XG5cdFx0aWYgKCFzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoKSByZXR1cm4gc2VsZi5jbGVhcigpO1xuXG5cdFx0aWYgKHNlbGYuaXNNb2JpbGUpIHtcblx0XHRcdHNlbGYubW9iaWxlSW5wdXQudmFsdWUgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubGVuZ3RoID8gZm9ybWF0RGF0ZShzZWxmLm1vYmlsZUZvcm1hdFN0ciwgc2VsZi5sYXRlc3RTZWxlY3RlZERhdGVPYmopIDogXCJcIjtcblx0XHR9XG5cblx0XHR2YXIgam9pbkNoYXIgPSBzZWxmLmNvbmZpZy5tb2RlICE9PSBcInJhbmdlXCIgPyBcIjsgXCIgOiBzZWxmLmwxMG4ucmFuZ2VTZXBhcmF0b3I7XG5cblx0XHRzZWxmLmlucHV0LnZhbHVlID0gc2VsZi5zZWxlY3RlZERhdGVzLm1hcChmdW5jdGlvbiAoZE9iaikge1xuXHRcdFx0cmV0dXJuIGZvcm1hdERhdGUoc2VsZi5jb25maWcuZGF0ZUZvcm1hdCwgZE9iaik7XG5cdFx0fSkuam9pbihqb2luQ2hhcik7XG5cblx0XHRpZiAoc2VsZi5jb25maWcuYWx0SW5wdXQpIHtcblx0XHRcdHNlbGYuYWx0SW5wdXQudmFsdWUgPSBzZWxmLnNlbGVjdGVkRGF0ZXMubWFwKGZ1bmN0aW9uIChkT2JqKSB7XG5cdFx0XHRcdHJldHVybiBmb3JtYXREYXRlKHNlbGYuY29uZmlnLmFsdEZvcm1hdCwgZE9iaik7XG5cdFx0XHR9KS5qb2luKGpvaW5DaGFyKTtcblx0XHR9XG5cblx0XHR0cmlnZ2VyRXZlbnQoXCJWYWx1ZVVwZGF0ZVwiKTtcblx0fVxuXG5cdGZ1bmN0aW9uIHllYXJTY3JvbGwoZSkge1xuXHRcdGUucHJldmVudERlZmF1bHQoKTtcblxuXHRcdHZhciBkZWx0YSA9IE1hdGgubWF4KC0xLCBNYXRoLm1pbigxLCBlLndoZWVsRGVsdGEgfHwgLWUuZGVsdGFZKSksXG5cdFx0ICAgIG5ld1llYXIgPSBwYXJzZUludChlLnRhcmdldC52YWx1ZSwgMTApICsgZGVsdGE7XG5cblx0XHRjaGFuZ2VZZWFyKG5ld1llYXIpO1xuXHRcdGUudGFyZ2V0LnZhbHVlID0gc2VsZi5jdXJyZW50WWVhcjtcblx0fVxuXG5cdGZ1bmN0aW9uIGNyZWF0ZUVsZW1lbnQodGFnLCBjbGFzc05hbWUsIGNvbnRlbnQpIHtcblx0XHR2YXIgZSA9IHdpbmRvdy5kb2N1bWVudC5jcmVhdGVFbGVtZW50KHRhZyk7XG5cdFx0Y2xhc3NOYW1lID0gY2xhc3NOYW1lIHx8IFwiXCI7XG5cdFx0Y29udGVudCA9IGNvbnRlbnQgfHwgXCJcIjtcblxuXHRcdGUuY2xhc3NOYW1lID0gY2xhc3NOYW1lO1xuXG5cdFx0aWYgKGNvbnRlbnQpIGUudGV4dENvbnRlbnQgPSBjb250ZW50O1xuXG5cdFx0cmV0dXJuIGU7XG5cdH1cblxuXHRmdW5jdGlvbiBhcnJheWlmeShvYmopIHtcblx0XHRpZiAoQXJyYXkuaXNBcnJheShvYmopKSByZXR1cm4gb2JqO1xuXHRcdHJldHVybiBbb2JqXTtcblx0fVxuXG5cdGZ1bmN0aW9uIHRvZ2dsZUNsYXNzKGVsZW0sIGNsYXNzTmFtZSwgYm9vbCkge1xuXHRcdGlmIChib29sKSByZXR1cm4gZWxlbS5jbGFzc0xpc3QuYWRkKGNsYXNzTmFtZSk7XG5cdFx0ZWxlbS5jbGFzc0xpc3QucmVtb3ZlKGNsYXNzTmFtZSk7XG5cdH1cblxuXHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRmdW5jdGlvbiBkZWJvdW5jZShmdW5jLCB3YWl0LCBpbW1lZGlhdGUpIHtcblx0XHR2YXIgdGltZW91dCA9IHZvaWQgMDtcblx0XHRyZXR1cm4gZnVuY3Rpb24gKCkge1xuXHRcdFx0Zm9yICh2YXIgX2xlbiA9IGFyZ3VtZW50cy5sZW5ndGgsIGFyZ3MgPSBBcnJheShfbGVuKSwgX2tleSA9IDA7IF9rZXkgPCBfbGVuOyBfa2V5KyspIHtcblx0XHRcdFx0YXJnc1tfa2V5XSA9IGFyZ3VtZW50c1tfa2V5XTtcblx0XHRcdH1cblxuXHRcdFx0dmFyIGNvbnRleHQgPSB0aGlzO1xuXHRcdFx0dmFyIGxhdGVyID0gZnVuY3Rpb24gbGF0ZXIoKSB7XG5cdFx0XHRcdHRpbWVvdXQgPSBudWxsO1xuXHRcdFx0XHRpZiAoIWltbWVkaWF0ZSkgZnVuYy5hcHBseShjb250ZXh0LCBhcmdzKTtcblx0XHRcdH07XG5cblx0XHRcdGNsZWFyVGltZW91dCh0aW1lb3V0KTtcblx0XHRcdHRpbWVvdXQgPSBzZXRUaW1lb3V0KGxhdGVyLCB3YWl0KTtcblx0XHRcdGlmIChpbW1lZGlhdGUgJiYgIXRpbWVvdXQpIGZ1bmMuYXBwbHkoY29udGV4dCwgYXJncyk7XG5cdFx0fTtcblx0fVxuXG5cdGZ1bmN0aW9uIGNvbXBhcmVEYXRlcyhkYXRlMSwgZGF0ZTIsIHRpbWVsZXNzKSB7XG5cdFx0aWYgKCEoZGF0ZTEgaW5zdGFuY2VvZiBEYXRlKSB8fCAhKGRhdGUyIGluc3RhbmNlb2YgRGF0ZSkpIHJldHVybiBmYWxzZTtcblxuXHRcdGlmICh0aW1lbGVzcyAhPT0gZmFsc2UpIHtcblx0XHRcdHJldHVybiBuZXcgRGF0ZShkYXRlMS5nZXRUaW1lKCkpLnNldEhvdXJzKDAsIDAsIDAsIDApIC0gbmV3IERhdGUoZGF0ZTIuZ2V0VGltZSgpKS5zZXRIb3VycygwLCAwLCAwLCAwKTtcblx0XHR9XG5cblx0XHRyZXR1cm4gZGF0ZTEuZ2V0VGltZSgpIC0gZGF0ZTIuZ2V0VGltZSgpO1xuXHR9XG5cblx0ZnVuY3Rpb24gdGltZVdyYXBwZXIoZSkge1xuXHRcdGUucHJldmVudERlZmF1bHQoKTtcblxuXHRcdHZhciBpc0tleURvd24gPSBlLnR5cGUgPT09IFwia2V5ZG93blwiLFxuXHRcdCAgICBpc1doZWVsID0gZS50eXBlID09PSBcIndoZWVsXCIsXG5cdFx0ICAgIGlzSW5jcmVtZW50ID0gZS50eXBlID09PSBcImluY3JlbWVudFwiLFxuXHRcdCAgICBpbnB1dCA9IGUudGFyZ2V0O1xuXG5cdFx0aWYgKGUudHlwZSAhPT0gXCJpbnB1dFwiICYmICFpc0tleURvd24gJiYgKGUudGFyZ2V0LnZhbHVlIHx8IGUudGFyZ2V0LnRleHRDb250ZW50KS5sZW5ndGggPj0gMiAvLyB0eXBlZCB0d28gZGlnaXRzXG5cdFx0KSB7XG5cdFx0XHRcdGUudGFyZ2V0LmZvY3VzKCk7XG5cdFx0XHRcdGUudGFyZ2V0LmJsdXIoKTtcblx0XHRcdH1cblxuXHRcdGlmIChzZWxmLmFtUE0gJiYgZS50YXJnZXQgPT09IHNlbGYuYW1QTSkgcmV0dXJuIGUudGFyZ2V0LnRleHRDb250ZW50ID0gW1wiQU1cIiwgXCJQTVwiXVtlLnRhcmdldC50ZXh0Q29udGVudCA9PT0gXCJBTVwiIHwgMF07XG5cblx0XHR2YXIgbWluID0gTnVtYmVyKGlucHV0Lm1pbiksXG5cdFx0ICAgIG1heCA9IE51bWJlcihpbnB1dC5tYXgpLFxuXHRcdCAgICBzdGVwID0gTnVtYmVyKGlucHV0LnN0ZXApLFxuXHRcdCAgICBjdXJWYWx1ZSA9IHBhcnNlSW50KGlucHV0LnZhbHVlLCAxMCksXG5cdFx0ICAgIGRlbHRhID0gZS5kZWx0YSB8fCAoIWlzS2V5RG93biA/IE1hdGgubWF4KC0xLCBNYXRoLm1pbigxLCBlLndoZWVsRGVsdGEgfHwgLWUuZGVsdGFZKSkgfHwgMCA6IGUud2hpY2ggPT09IDM4ID8gMSA6IC0xKTtcblxuXHRcdHZhciBuZXdWYWx1ZSA9IGN1clZhbHVlICsgc3RlcCAqIGRlbHRhO1xuXG5cdFx0aWYgKGlucHV0LnZhbHVlLmxlbmd0aCA9PT0gMikge1xuXHRcdFx0dmFyIGlzSG91ckVsZW0gPSBpbnB1dCA9PT0gc2VsZi5ob3VyRWxlbWVudCxcblx0XHRcdCAgICBpc01pbnV0ZUVsZW0gPSBpbnB1dCA9PT0gc2VsZi5taW51dGVFbGVtZW50O1xuXG5cdFx0XHRpZiAobmV3VmFsdWUgPCBtaW4pIHtcblx0XHRcdFx0bmV3VmFsdWUgPSBtYXggKyBuZXdWYWx1ZSArICFpc0hvdXJFbGVtICsgKGlzSG91ckVsZW0gJiYgIXNlbGYuYW1QTSk7XG5cblx0XHRcdFx0aWYgKGlzTWludXRlRWxlbSkgaW5jcmVtZW50TnVtSW5wdXQobnVsbCwgLTEsIHNlbGYuaG91ckVsZW1lbnQpO1xuXHRcdFx0fSBlbHNlIGlmIChuZXdWYWx1ZSA+IG1heCkge1xuXHRcdFx0XHRuZXdWYWx1ZSA9IGlucHV0ID09PSBzZWxmLmhvdXJFbGVtZW50ID8gbmV3VmFsdWUgLSBtYXggLSAhc2VsZi5hbVBNIDogbWluO1xuXG5cdFx0XHRcdGlmIChpc01pbnV0ZUVsZW0pIGluY3JlbWVudE51bUlucHV0KG51bGwsIDEsIHNlbGYuaG91ckVsZW1lbnQpO1xuXHRcdFx0fVxuXG5cdFx0XHRpZiAoc2VsZi5hbVBNICYmIGlzSG91ckVsZW0gJiYgKHN0ZXAgPT09IDEgPyBuZXdWYWx1ZSArIGN1clZhbHVlID09PSAyMyA6IE1hdGguYWJzKG5ld1ZhbHVlIC0gY3VyVmFsdWUpID4gc3RlcCkpIHNlbGYuYW1QTS50ZXh0Q29udGVudCA9IHNlbGYuYW1QTS50ZXh0Q29udGVudCA9PT0gXCJQTVwiID8gXCJBTVwiIDogXCJQTVwiO1xuXG5cdFx0XHRpbnB1dC52YWx1ZSA9IHNlbGYucGFkKG5ld1ZhbHVlKTtcblx0XHR9XG5cdH1cblxuXHRpbml0KCk7XG5cdHJldHVybiBzZWxmO1xufVxuXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuRmxhdHBpY2tyLmRlZmF1bHRDb25maWcgPSB7XG5cblx0bW9kZTogXCJzaW5nbGVcIixcblxuXHRwb3NpdGlvbjogXCJ0b3BcIixcblxuXHQvKiBpZiB0cnVlLCBkYXRlcyB3aWxsIGJlIHBhcnNlZCwgZm9ybWF0dGVkLCBhbmQgZGlzcGxheWVkIGluIFVUQy5cbiBwcmVsb2FkaW5nIGRhdGUgc3RyaW5ncyB3LyB0aW1lem9uZXMgaXMgcmVjb21tZW5kZWQgYnV0IG5vdCBuZWNlc3NhcnkgKi9cblx0dXRjOiBmYWxzZSxcblxuXHQvLyB3cmFwOiBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNzdHJhcFxuXHR3cmFwOiBmYWxzZSxcblxuXHQvLyBlbmFibGVzIHdlZWsgbnVtYmVyc1xuXHR3ZWVrTnVtYmVyczogZmFsc2UsXG5cblx0Ly8gYWxsb3cgbWFudWFsIGRhdGV0aW1lIGlucHV0XG5cdGFsbG93SW5wdXQ6IGZhbHNlLFxuXG5cdC8qXG4gXHRjbGlja2luZyBvbiBpbnB1dCBvcGVucyB0aGUgZGF0ZSh0aW1lKXBpY2tlci5cbiBcdGRpc2FibGUgaWYgeW91IHdpc2ggdG8gb3BlbiB0aGUgY2FsZW5kYXIgbWFudWFsbHkgd2l0aCAub3BlbigpXG4gKi9cblx0Y2xpY2tPcGVuczogdHJ1ZSxcblxuXHQvLyBkaXNwbGF5IHRpbWUgcGlja2VyIGluIDI0IGhvdXIgbW9kZVxuXHR0aW1lXzI0aHI6IGZhbHNlLFxuXG5cdC8vIGVuYWJsZXMgdGhlIHRpbWUgcGlja2VyIGZ1bmN0aW9uYWxpdHlcblx0ZW5hYmxlVGltZTogZmFsc2UsXG5cblx0Ly8gbm9DYWxlbmRhcjogdHJ1ZSB3aWxsIGhpZGUgdGhlIGNhbGVuZGFyLiB1c2UgZm9yIGEgdGltZSBwaWNrZXIgYWxvbmcgdy8gZW5hYmxlVGltZVxuXHRub0NhbGVuZGFyOiBmYWxzZSxcblxuXHQvLyBtb3JlIGRhdGUgZm9ybWF0IGNoYXJzIGF0IGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jZGF0ZWZvcm1hdFxuXHRkYXRlRm9ybWF0OiBcIlktbS1kXCIsXG5cblx0Ly8gYWx0SW5wdXQgLSBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNhbHRpbnB1dFxuXHRhbHRJbnB1dDogZmFsc2UsXG5cblx0Ly8gdGhlIGNyZWF0ZWQgYWx0SW5wdXQgZWxlbWVudCB3aWxsIGhhdmUgdGhpcyBjbGFzcy5cblx0YWx0SW5wdXRDbGFzczogXCJmbGF0cGlja3ItaW5wdXQgZm9ybS1jb250cm9sIGlucHV0XCIsXG5cblx0Ly8gc2FtZSBhcyBkYXRlRm9ybWF0LCBidXQgZm9yIGFsdElucHV0XG5cdGFsdEZvcm1hdDogXCJGIGosIFlcIiwgLy8gZGVmYXVsdHMgdG8gZS5nLiBKdW5lIDEwLCAyMDE2XG5cblx0Ly8gZGVmYXVsdERhdGUgLSBlaXRoZXIgYSBkYXRlc3RyaW5nIG9yIGEgZGF0ZSBvYmplY3QuIHVzZWQgZm9yIGRhdGV0aW1lcGlja2VyXCJzIGluaXRpYWwgdmFsdWVcblx0ZGVmYXVsdERhdGU6IG51bGwsXG5cblx0Ly8gdGhlIG1pbmltdW0gZGF0ZSB0aGF0IHVzZXIgY2FuIHBpY2sgKGluY2x1c2l2ZSlcblx0bWluRGF0ZTogbnVsbCxcblxuXHQvLyB0aGUgbWF4aW11bSBkYXRlIHRoYXQgdXNlciBjYW4gcGljayAoaW5jbHVzaXZlKVxuXHRtYXhEYXRlOiBudWxsLFxuXG5cdC8vIGRhdGVwYXJzZXIgdGhhdCB0cmFuc2Zvcm1zIGEgZ2l2ZW4gc3RyaW5nIHRvIGEgZGF0ZSBvYmplY3Rcblx0cGFyc2VEYXRlOiBudWxsLFxuXG5cdC8vIGRhdGVmb3JtYXR0ZXIgdGhhdCB0cmFuc2Zvcm1zIGEgZ2l2ZW4gZGF0ZSBvYmplY3QgdG8gYSBzdHJpbmcsIGFjY29yZGluZyB0byBwYXNzZWQgZm9ybWF0XG5cdGZvcm1hdERhdGU6IG51bGwsXG5cblx0Z2V0V2VlazogZnVuY3Rpb24gZ2V0V2VlayhnaXZlbkRhdGUpIHtcblx0XHR2YXIgZGF0ZSA9IG5ldyBEYXRlKGdpdmVuRGF0ZS5nZXRUaW1lKCkpO1xuXHRcdGRhdGUuc2V0SG91cnMoMCwgMCwgMCwgMCk7XG5cblx0XHQvLyBUaHVyc2RheSBpbiBjdXJyZW50IHdlZWsgZGVjaWRlcyB0aGUgeWVhci5cblx0XHRkYXRlLnNldERhdGUoZGF0ZS5nZXREYXRlKCkgKyAzIC0gKGRhdGUuZ2V0RGF5KCkgKyA2KSAlIDcpO1xuXHRcdC8vIEphbnVhcnkgNCBpcyBhbHdheXMgaW4gd2VlayAxLlxuXHRcdHZhciB3ZWVrMSA9IG5ldyBEYXRlKGRhdGUuZ2V0RnVsbFllYXIoKSwgMCwgNCk7XG5cdFx0Ly8gQWRqdXN0IHRvIFRodXJzZGF5IGluIHdlZWsgMSBhbmQgY291bnQgbnVtYmVyIG9mIHdlZWtzIGZyb20gZGF0ZSB0byB3ZWVrMS5cblx0XHRyZXR1cm4gMSArIE1hdGgucm91bmQoKChkYXRlLmdldFRpbWUoKSAtIHdlZWsxLmdldFRpbWUoKSkgLyA4NjQwMDAwMCAtIDMgKyAod2VlazEuZ2V0RGF5KCkgKyA2KSAlIDcpIC8gNyk7XG5cdH0sXG5cblx0Ly8gc2VlIGh0dHBzOi8vY2htbG4uZ2l0aHViLmlvL2ZsYXRwaWNrci8jZGlzYWJsZVxuXHRlbmFibGU6IFtdLFxuXG5cdC8vIHNlZSBodHRwczovL2NobWxuLmdpdGh1Yi5pby9mbGF0cGlja3IvI2Rpc2FibGVcblx0ZGlzYWJsZTogW10sXG5cblx0Ly8gZGlzcGxheSB0aGUgc2hvcnQgdmVyc2lvbiBvZiBtb250aCBuYW1lcyAtIGUuZy4gU2VwIGluc3RlYWQgb2YgU2VwdGVtYmVyXG5cdHNob3J0aGFuZEN1cnJlbnRNb250aDogZmFsc2UsXG5cblx0Ly8gZGlzcGxheXMgY2FsZW5kYXIgaW5saW5lLiBzZWUgaHR0cHM6Ly9jaG1sbi5naXRodWIuaW8vZmxhdHBpY2tyLyNpbmxpbmUtY2FsZW5kYXJcblx0aW5saW5lOiBmYWxzZSxcblxuXHQvLyBwb3NpdGlvbiBjYWxlbmRhciBpbnNpZGUgd3JhcHBlciBhbmQgbmV4dCB0byB0aGUgaW5wdXQgZWxlbWVudFxuXHQvLyBsZWF2ZSBhdCBmYWxzZSB1bmxlc3MgeW91IGtub3cgd2hhdCB5b3VcInJlIGRvaW5nXG5cdHN0YXRpYzogZmFsc2UsXG5cblx0Ly8gRE9NIG5vZGUgdG8gYXBwZW5kIHRoZSBjYWxlbmRhciB0byBpbiAqc3RhdGljKiBtb2RlXG5cdGFwcGVuZFRvOiBudWxsLFxuXG5cdC8vIGNvZGUgZm9yIHByZXZpb3VzL25leHQgaWNvbnMuIHRoaXMgaXMgd2hlcmUgeW91IHB1dCB5b3VyIGN1c3RvbSBpY29uIGNvZGUgZS5nLiBmb250YXdlc29tZVxuXHRwcmV2QXJyb3c6IFwiPHN2ZyB2ZXJzaW9uPScxLjEnIHhtbG5zPSdodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZycgeG1sbnM6eGxpbms9J2h0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsnIHZpZXdCb3g9JzAgMCAxNyAxNyc+PGc+PC9nPjxwYXRoIGQ9J001LjIwNyA4LjQ3MWw3LjE0NiA3LjE0Ny0wLjcwNyAwLjcwNy03Ljg1My03Ljg1NCA3Ljg1NC03Ljg1MyAwLjcwNyAwLjcwNy03LjE0NyA3LjE0NnonIC8+PC9zdmc+XCIsXG5cdG5leHRBcnJvdzogXCI8c3ZnIHZlcnNpb249JzEuMScgeG1sbnM9J2h0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnJyB4bWxuczp4bGluaz0naHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluaycgdmlld0JveD0nMCAwIDE3IDE3Jz48Zz48L2c+PHBhdGggZD0nTTEzLjIwNyA4LjQ3MmwtNy44NTQgNy44NTQtMC43MDctMC43MDcgNy4xNDYtNy4xNDYtNy4xNDYtNy4xNDggMC43MDctMC43MDcgNy44NTQgNy44NTR6JyAvPjwvc3ZnPlwiLFxuXG5cdC8vIGVuYWJsZXMgc2Vjb25kcyBpbiB0aGUgdGltZSBwaWNrZXJcblx0ZW5hYmxlU2Vjb25kczogZmFsc2UsXG5cblx0Ly8gc3RlcCBzaXplIHVzZWQgd2hlbiBzY3JvbGxpbmcvaW5jcmVtZW50aW5nIHRoZSBob3VyIGVsZW1lbnRcblx0aG91ckluY3JlbWVudDogMSxcblxuXHQvLyBzdGVwIHNpemUgdXNlZCB3aGVuIHNjcm9sbGluZy9pbmNyZW1lbnRpbmcgdGhlIG1pbnV0ZSBlbGVtZW50XG5cdG1pbnV0ZUluY3JlbWVudDogNSxcblxuXHQvLyBpbml0aWFsIHZhbHVlIGluIHRoZSBob3VyIGVsZW1lbnRcblx0ZGVmYXVsdEhvdXI6IDEyLFxuXG5cdC8vIGluaXRpYWwgdmFsdWUgaW4gdGhlIG1pbnV0ZSBlbGVtZW50XG5cdGRlZmF1bHRNaW51dGU6IDAsXG5cblx0Ly8gZGlzYWJsZSBuYXRpdmUgbW9iaWxlIGRhdGV0aW1lIGlucHV0IHN1cHBvcnRcblx0ZGlzYWJsZU1vYmlsZTogZmFsc2UsXG5cblx0Ly8gZGVmYXVsdCBsb2NhbGVcblx0bG9jYWxlOiBcImRlZmF1bHRcIixcblxuXHRwbHVnaW5zOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSBjYWxlbmRhciBpcyBjbG9zZWRcblx0b25DbG9zZTogW10sIC8vIGZ1bmN0aW9uIChkYXRlT2JqLCBkYXRlU3RyKSB7fVxuXG5cdC8vIG9uQ2hhbmdlIGNhbGxiYWNrIHdoZW4gdXNlciBzZWxlY3RzIGEgZGF0ZSBvciB0aW1lXG5cdG9uQ2hhbmdlOiBbXSwgLy8gZnVuY3Rpb24gKGRhdGVPYmosIGRhdGVTdHIpIHt9XG5cblx0Ly8gY2FsbGVkIGZvciBldmVyeSBkYXkgZWxlbWVudFxuXHRvbkRheUNyZWF0ZTogW10sXG5cblx0Ly8gY2FsbGVkIGV2ZXJ5IHRpbWUgdGhlIG1vbnRoIGlzIGNoYW5nZWRcblx0b25Nb250aENoYW5nZTogW10sXG5cblx0Ly8gY2FsbGVkIGV2ZXJ5IHRpbWUgY2FsZW5kYXIgaXMgb3BlbmVkXG5cdG9uT3BlbjogW10sIC8vIGZ1bmN0aW9uIChkYXRlT2JqLCBkYXRlU3RyKSB7fVxuXG5cdC8vIGNhbGxlZCBhZnRlciB0aGUgY29uZmlndXJhdGlvbiBoYXMgYmVlbiBwYXJzZWRcblx0b25QYXJzZUNvbmZpZzogW10sXG5cblx0Ly8gY2FsbGVkIGFmdGVyIGNhbGVuZGFyIGlzIHJlYWR5XG5cdG9uUmVhZHk6IFtdLCAvLyBmdW5jdGlvbiAoZGF0ZU9iaiwgZGF0ZVN0cikge31cblxuXHQvLyBjYWxsZWQgYWZ0ZXIgaW5wdXQgdmFsdWUgdXBkYXRlZFxuXHRvblZhbHVlVXBkYXRlOiBbXSxcblxuXHQvLyBjYWxsZWQgZXZlcnkgdGltZSB0aGUgeWVhciBpcyBjaGFuZ2VkXG5cdG9uWWVhckNoYW5nZTogW11cbn07XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5GbGF0cGlja3IubDEwbnMgPSB7XG5cdGVuOiB7XG5cdFx0d2Vla2RheXM6IHtcblx0XHRcdHNob3J0aGFuZDogW1wiU3VuXCIsIFwiTW9uXCIsIFwiVHVlXCIsIFwiV2VkXCIsIFwiVGh1XCIsIFwiRnJpXCIsIFwiU2F0XCJdLFxuXHRcdFx0bG9uZ2hhbmQ6IFtcIlN1bmRheVwiLCBcIk1vbmRheVwiLCBcIlR1ZXNkYXlcIiwgXCJXZWRuZXNkYXlcIiwgXCJUaHVyc2RheVwiLCBcIkZyaWRheVwiLCBcIlNhdHVyZGF5XCJdXG5cdFx0fSxcblx0XHRtb250aHM6IHtcblx0XHRcdHNob3J0aGFuZDogW1wiSmFuXCIsIFwiRmViXCIsIFwiTWFyXCIsIFwiQXByXCIsIFwiTWF5XCIsIFwiSnVuXCIsIFwiSnVsXCIsIFwiQXVnXCIsIFwiU2VwXCIsIFwiT2N0XCIsIFwiTm92XCIsIFwiRGVjXCJdLFxuXHRcdFx0bG9uZ2hhbmQ6IFtcIkphbnVhcnlcIiwgXCJGZWJydWFyeVwiLCBcIk1hcmNoXCIsIFwiQXByaWxcIiwgXCJNYXlcIiwgXCJKdW5lXCIsIFwiSnVseVwiLCBcIkF1Z3VzdFwiLCBcIlNlcHRlbWJlclwiLCBcIk9jdG9iZXJcIiwgXCJOb3ZlbWJlclwiLCBcIkRlY2VtYmVyXCJdXG5cdFx0fSxcblx0XHRkYXlzSW5Nb250aDogWzMxLCAyOCwgMzEsIDMwLCAzMSwgMzAsIDMxLCAzMSwgMzAsIDMxLCAzMCwgMzFdLFxuXHRcdGZpcnN0RGF5T2ZXZWVrOiAwLFxuXHRcdG9yZGluYWw6IGZ1bmN0aW9uIG9yZGluYWwobnRoKSB7XG5cdFx0XHR2YXIgcyA9IG50aCAlIDEwMDtcblx0XHRcdGlmIChzID4gMyAmJiBzIDwgMjEpIHJldHVybiBcInRoXCI7XG5cdFx0XHRzd2l0Y2ggKHMgJSAxMCkge1xuXHRcdFx0XHRjYXNlIDE6XG5cdFx0XHRcdFx0cmV0dXJuIFwic3RcIjtcblx0XHRcdFx0Y2FzZSAyOlxuXHRcdFx0XHRcdHJldHVybiBcIm5kXCI7XG5cdFx0XHRcdGNhc2UgMzpcblx0XHRcdFx0XHRyZXR1cm4gXCJyZFwiO1xuXHRcdFx0XHRkZWZhdWx0OlxuXHRcdFx0XHRcdHJldHVybiBcInRoXCI7XG5cdFx0XHR9XG5cdFx0fSxcblx0XHRyYW5nZVNlcGFyYXRvcjogXCIgdG8gXCIsXG5cdFx0d2Vla0FiYnJldmlhdGlvbjogXCJXa1wiLFxuXHRcdHNjcm9sbFRpdGxlOiBcIlNjcm9sbCB0byBpbmNyZW1lbnRcIixcblx0XHR0b2dnbGVUaXRsZTogXCJDbGljayB0byB0b2dnbGVcIlxuXHR9XG59O1xuXG5GbGF0cGlja3IubDEwbnMuZGVmYXVsdCA9IE9iamVjdC5jcmVhdGUoRmxhdHBpY2tyLmwxMG5zLmVuKTtcbkZsYXRwaWNrci5sb2NhbGl6ZSA9IGZ1bmN0aW9uIChsMTBuKSB7XG5cdHJldHVybiBfZXh0ZW5kcyhGbGF0cGlja3IubDEwbnMuZGVmYXVsdCwgbDEwbiB8fCB7fSk7XG59O1xuRmxhdHBpY2tyLnNldERlZmF1bHRzID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRyZXR1cm4gX2V4dGVuZHMoRmxhdHBpY2tyLmRlZmF1bHRDb25maWcsIGNvbmZpZyB8fCB7fSk7XG59O1xuXG5GbGF0cGlja3IucHJvdG90eXBlID0ge1xuXHRmb3JtYXRzOiB7XG5cdFx0Ly8gZ2V0IHRoZSBkYXRlIGluIFVUQ1xuXHRcdFo6IGZ1bmN0aW9uIFooZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUudG9JU09TdHJpbmcoKTtcblx0XHR9LFxuXG5cdFx0Ly8gd2Vla2RheSBuYW1lLCBzaG9ydCwgZS5nLiBUaHVcblx0XHREOiBmdW5jdGlvbiBEKGRhdGUpIHtcblx0XHRcdHJldHVybiB0aGlzLmwxMG4ud2Vla2RheXMuc2hvcnRoYW5kW3RoaXMuZm9ybWF0cy53KGRhdGUpXTtcblx0XHR9LFxuXG5cdFx0Ly8gZnVsbCBtb250aCBuYW1lIGUuZy4gSmFudWFyeVxuXHRcdEY6IGZ1bmN0aW9uIEYoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMudXRpbHMubW9udGhUb1N0cih0aGlzLmZvcm1hdHMubihkYXRlKSAtIDEsIGZhbHNlKTtcblx0XHR9LFxuXG5cdFx0Ly8gaG91cnMgd2l0aCBsZWFkaW5nIHplcm8gZS5nLiAwM1xuXHRcdEg6IGZ1bmN0aW9uIEgoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0SG91cnMoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGRheSAoMS0zMCkgd2l0aCBvcmRpbmFsIHN1ZmZpeCBlLmcuIDFzdCwgMm5kXG5cdFx0SjogZnVuY3Rpb24gSihkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXREYXRlKCkgKyB0aGlzLmwxMG4ub3JkaW5hbChkYXRlLmdldERhdGUoKSk7XG5cdFx0fSxcblxuXHRcdC8vIEFNL1BNXG5cdFx0SzogZnVuY3Rpb24gSyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZS5nZXRIb3VycygpID4gMTEgPyBcIlBNXCIgOiBcIkFNXCI7XG5cdFx0fSxcblxuXHRcdC8vIHNob3J0aGFuZCBtb250aCBlLmcuIEphbiwgU2VwLCBPY3QsIGV0Y1xuXHRcdE06IGZ1bmN0aW9uIE0oZGF0ZSkge1xuXHRcdFx0cmV0dXJuIHRoaXMudXRpbHMubW9udGhUb1N0cihkYXRlLmdldE1vbnRoKCksIHRydWUpO1xuXHRcdH0sXG5cblx0XHQvLyBzZWNvbmRzIDAwLTU5XG5cdFx0UzogZnVuY3Rpb24gUyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gRmxhdHBpY2tyLnByb3RvdHlwZS5wYWQoZGF0ZS5nZXRTZWNvbmRzKCkpO1xuXHRcdH0sXG5cblx0XHQvLyB1bml4IHRpbWVzdGFtcFxuXHRcdFU6IGZ1bmN0aW9uIFUoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0VGltZSgpIC8gMTAwMDtcblx0XHR9LFxuXG5cdFx0VzogZnVuY3Rpb24gVyhkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5jb25maWcuZ2V0V2VlayhkYXRlKTtcblx0XHR9LFxuXG5cdFx0Ly8gZnVsbCB5ZWFyIGUuZy4gMjAxNlxuXHRcdFk6IGZ1bmN0aW9uIFkoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0RnVsbFllYXIoKTtcblx0XHR9LFxuXG5cdFx0Ly8gZGF5IGluIG1vbnRoLCBwYWRkZWQgKDAxLTMwKVxuXHRcdGQ6IGZ1bmN0aW9uIGQoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIEZsYXRwaWNrci5wcm90b3R5cGUucGFkKGRhdGUuZ2V0RGF0ZSgpKTtcblx0XHR9LFxuXG5cdFx0Ly8gaG91ciBmcm9tIDEtMTIgKGFtL3BtKVxuXHRcdGg6IGZ1bmN0aW9uIGgoZGF0ZSkge1xuXHRcdFx0cmV0dXJuIGRhdGUuZ2V0SG91cnMoKSAlIDEyID8gZGF0ZS5nZXRIb3VycygpICUgMTIgOiAxMjtcblx0XHR9LFxuXG5cdFx0Ly8gbWludXRlcywgcGFkZGVkIHdpdGggbGVhZGluZyB6ZXJvIGUuZy4gMDlcblx0XHRpOiBmdW5jdGlvbiBpKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldE1pbnV0ZXMoKSk7XG5cdFx0fSxcblxuXHRcdC8vIGRheSBpbiBtb250aCAoMS0zMClcblx0XHRqOiBmdW5jdGlvbiBqKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldERhdGUoKTtcblx0XHR9LFxuXG5cdFx0Ly8gd2Vla2RheSBuYW1lLCBmdWxsLCBlLmcuIFRodXJzZGF5XG5cdFx0bDogZnVuY3Rpb24gbChkYXRlKSB7XG5cdFx0XHRyZXR1cm4gdGhpcy5sMTBuLndlZWtkYXlzLmxvbmdoYW5kW2RhdGUuZ2V0RGF5KCldO1xuXHRcdH0sXG5cblx0XHQvLyBwYWRkZWQgbW9udGggbnVtYmVyICgwMS0xMilcblx0XHRtOiBmdW5jdGlvbiBtKGRhdGUpIHtcblx0XHRcdHJldHVybiBGbGF0cGlja3IucHJvdG90eXBlLnBhZChkYXRlLmdldE1vbnRoKCkgKyAxKTtcblx0XHR9LFxuXG5cdFx0Ly8gdGhlIG1vbnRoIG51bWJlciAoMS0xMilcblx0XHRuOiBmdW5jdGlvbiBuKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldE1vbnRoKCkgKyAxO1xuXHRcdH0sXG5cblx0XHQvLyBzZWNvbmRzIDAtNTlcblx0XHRzOiBmdW5jdGlvbiBzKGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldFNlY29uZHMoKTtcblx0XHR9LFxuXG5cdFx0Ly8gbnVtYmVyIG9mIHRoZSBkYXkgb2YgdGhlIHdlZWtcblx0XHR3OiBmdW5jdGlvbiB3KGRhdGUpIHtcblx0XHRcdHJldHVybiBkYXRlLmdldERheSgpO1xuXHRcdH0sXG5cblx0XHQvLyBsYXN0IHR3byBkaWdpdHMgb2YgeWVhciBlLmcuIDE2IGZvciAyMDE2XG5cdFx0eTogZnVuY3Rpb24geShkYXRlKSB7XG5cdFx0XHRyZXR1cm4gU3RyaW5nKGRhdGUuZ2V0RnVsbFllYXIoKSkuc3Vic3RyaW5nKDIpO1xuXHRcdH1cblx0fSxcblxuXHRyZXZGb3JtYXQ6IHtcblx0XHREOiBmdW5jdGlvbiBEKCkge30sXG5cdFx0RjogZnVuY3Rpb24gRihkYXRlT2JqLCBtb250aE5hbWUpIHtcblx0XHRcdGRhdGVPYmouc2V0TW9udGgodGhpcy5sMTBuLm1vbnRocy5sb25naGFuZC5pbmRleE9mKG1vbnRoTmFtZSkpO1xuXHRcdH0sXG5cdFx0SDogZnVuY3Rpb24gSChkYXRlT2JqLCBob3VyKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRIb3VycyhwYXJzZUZsb2F0KGhvdXIpKTtcblx0XHR9LFxuXHRcdEo6IGZ1bmN0aW9uIEooZGF0ZU9iaiwgZGF5KSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXREYXRlKHBhcnNlRmxvYXQoZGF5KSk7XG5cdFx0fSxcblx0XHRLOiBmdW5jdGlvbiBLKGRhdGVPYmosIGFtUE0pIHtcblx0XHRcdHZhciBob3VycyA9IGRhdGVPYmouZ2V0SG91cnMoKTtcblxuXHRcdFx0aWYgKGhvdXJzICE9PSAxMikgZGF0ZU9iai5zZXRIb3Vycyhob3VycyAlIDEyICsgMTIgKiAvcG0vaS50ZXN0KGFtUE0pKTtcblx0XHR9LFxuXHRcdE06IGZ1bmN0aW9uIE0oZGF0ZU9iaiwgc2hvcnRNb250aCkge1xuXHRcdFx0ZGF0ZU9iai5zZXRNb250aCh0aGlzLmwxMG4ubW9udGhzLnNob3J0aGFuZC5pbmRleE9mKHNob3J0TW9udGgpKTtcblx0XHR9LFxuXHRcdFM6IGZ1bmN0aW9uIFMoZGF0ZU9iaiwgc2Vjb25kcykge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0U2Vjb25kcyhzZWNvbmRzKTtcblx0XHR9LFxuXHRcdFc6IGZ1bmN0aW9uIFcoKSB7fSxcblx0XHRZOiBmdW5jdGlvbiBZKGRhdGVPYmosIHllYXIpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldEZ1bGxZZWFyKHllYXIpO1xuXHRcdH0sXG5cdFx0WjogZnVuY3Rpb24gWihkYXRlT2JqLCBJU09EYXRlKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iaiA9IG5ldyBEYXRlKElTT0RhdGUpO1xuXHRcdH0sXG5cblx0XHRkOiBmdW5jdGlvbiBkKGRhdGVPYmosIGRheSkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0RGF0ZShwYXJzZUZsb2F0KGRheSkpO1xuXHRcdH0sXG5cdFx0aDogZnVuY3Rpb24gaChkYXRlT2JqLCBob3VyKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRIb3VycyhwYXJzZUZsb2F0KGhvdXIpKTtcblx0XHR9LFxuXHRcdGk6IGZ1bmN0aW9uIGkoZGF0ZU9iaiwgbWludXRlcykge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0TWludXRlcyhwYXJzZUZsb2F0KG1pbnV0ZXMpKTtcblx0XHR9LFxuXHRcdGo6IGZ1bmN0aW9uIGooZGF0ZU9iaiwgZGF5KSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXREYXRlKHBhcnNlRmxvYXQoZGF5KSk7XG5cdFx0fSxcblx0XHRsOiBmdW5jdGlvbiBsKCkge30sXG5cdFx0bTogZnVuY3Rpb24gbShkYXRlT2JqLCBtb250aCkge1xuXHRcdFx0cmV0dXJuIGRhdGVPYmouc2V0TW9udGgocGFyc2VGbG9hdChtb250aCkgLSAxKTtcblx0XHR9LFxuXHRcdG46IGZ1bmN0aW9uIG4oZGF0ZU9iaiwgbW9udGgpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldE1vbnRoKHBhcnNlRmxvYXQobW9udGgpIC0gMSk7XG5cdFx0fSxcblx0XHRzOiBmdW5jdGlvbiBzKGRhdGVPYmosIHNlY29uZHMpIHtcblx0XHRcdHJldHVybiBkYXRlT2JqLnNldFNlY29uZHMocGFyc2VGbG9hdChzZWNvbmRzKSk7XG5cdFx0fSxcblx0XHR3OiBmdW5jdGlvbiB3KCkge30sXG5cdFx0eTogZnVuY3Rpb24geShkYXRlT2JqLCB5ZWFyKSB7XG5cdFx0XHRyZXR1cm4gZGF0ZU9iai5zZXRGdWxsWWVhcigyMDAwICsgcGFyc2VGbG9hdCh5ZWFyKSk7XG5cdFx0fVxuXHR9LFxuXG5cdHRva2VuUmVnZXg6IHtcblx0XHREOiBcIihcXFxcdyspXCIsXG5cdFx0RjogXCIoXFxcXHcrKVwiLFxuXHRcdEg6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0SjogXCIoXFxcXGRcXFxcZHxcXFxcZClcXFxcdytcIixcblx0XHRLOiBcIihcXFxcdyspXCIsXG5cdFx0TTogXCIoXFxcXHcrKVwiLFxuXHRcdFM6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0WTogXCIoXFxcXGR7NH0pXCIsXG5cdFx0WjogXCIoLispXCIsXG5cdFx0ZDogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRoOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdGk6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0ajogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRsOiBcIihcXFxcdyspXCIsXG5cdFx0bTogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHRuOiBcIihcXFxcZFxcXFxkfFxcXFxkKVwiLFxuXHRcdHM6IFwiKFxcXFxkXFxcXGR8XFxcXGQpXCIsXG5cdFx0dzogXCIoXFxcXGRcXFxcZHxcXFxcZClcIixcblx0XHR5OiBcIihcXFxcZHsyfSlcIlxuXHR9LFxuXG5cdHBhZDogZnVuY3Rpb24gcGFkKG51bWJlcikge1xuXHRcdHJldHVybiAoXCIwXCIgKyBudW1iZXIpLnNsaWNlKC0yKTtcblx0fSxcblxuXHRwYXJzZURhdGU6IGZ1bmN0aW9uIHBhcnNlRGF0ZShkYXRlLCB0aW1lbGVzcywgZ2l2ZW5Gb3JtYXQpIHtcblx0XHRpZiAoIWRhdGUpIHJldHVybiBudWxsO1xuXG5cdFx0dmFyIGRhdGVfb3JpZyA9IGRhdGU7XG5cblx0XHRpZiAoZGF0ZS50b0ZpeGVkKSAvLyB0aW1lc3RhbXBcblx0XHRcdGRhdGUgPSBuZXcgRGF0ZShkYXRlKTtlbHNlIGlmICh0eXBlb2YgZGF0ZSA9PT0gXCJzdHJpbmdcIikge1xuXHRcdFx0dmFyIGZvcm1hdCA9IHR5cGVvZiBnaXZlbkZvcm1hdCA9PT0gXCJzdHJpbmdcIiA/IGdpdmVuRm9ybWF0IDogdGhpcy5jb25maWcuZGF0ZUZvcm1hdDtcblx0XHRcdGRhdGUgPSBkYXRlLnRyaW0oKTtcblxuXHRcdFx0aWYgKGRhdGUgPT09IFwidG9kYXlcIikge1xuXHRcdFx0XHRkYXRlID0gbmV3IERhdGUoKTtcblx0XHRcdFx0dGltZWxlc3MgPSB0cnVlO1xuXHRcdFx0fSBlbHNlIGlmICh0aGlzLmNvbmZpZyAmJiB0aGlzLmNvbmZpZy5wYXJzZURhdGUpIGRhdGUgPSB0aGlzLmNvbmZpZy5wYXJzZURhdGUoZGF0ZSk7ZWxzZSBpZiAoL1okLy50ZXN0KGRhdGUpIHx8IC9HTVQkLy50ZXN0KGRhdGUpKSAvLyBkYXRlc3RyaW5ncyB3LyB0aW1lem9uZVxuXHRcdFx0XHRkYXRlID0gbmV3IERhdGUoZGF0ZSk7ZWxzZSB7XG5cdFx0XHRcdHZhciBwYXJzZWREYXRlID0gdGhpcy5jb25maWcubm9DYWxlbmRhciA/IG5ldyBEYXRlKG5ldyBEYXRlKCkuc2V0SG91cnMoMCwgMCwgMCwgMCkpIDogbmV3IERhdGUobmV3IERhdGUoKS5nZXRGdWxsWWVhcigpLCAwLCAxLCAwLCAwLCAwLCAwKTtcblxuXHRcdFx0XHR2YXIgbWF0Y2hlZCA9IGZhbHNlO1xuXG5cdFx0XHRcdGZvciAodmFyIGkgPSAwLCBtYXRjaEluZGV4ID0gMCwgcmVnZXhTdHIgPSBcIlwiOyBpIDwgZm9ybWF0Lmxlbmd0aDsgaSsrKSB7XG5cdFx0XHRcdFx0dmFyIHRva2VuID0gZm9ybWF0W2ldO1xuXHRcdFx0XHRcdHZhciBpc0JhY2tTbGFzaCA9IHRva2VuID09PSBcIlxcXFxcIjtcblx0XHRcdFx0XHR2YXIgZXNjYXBlZCA9IGZvcm1hdFtpIC0gMV0gPT09IFwiXFxcXFwiIHx8IGlzQmFja1NsYXNoO1xuXHRcdFx0XHRcdGlmICh0aGlzLnRva2VuUmVnZXhbdG9rZW5dICYmICFlc2NhcGVkKSB7XG5cdFx0XHRcdFx0XHRyZWdleFN0ciArPSB0aGlzLnRva2VuUmVnZXhbdG9rZW5dO1xuXHRcdFx0XHRcdFx0dmFyIG1hdGNoID0gbmV3IFJlZ0V4cChyZWdleFN0cikuZXhlYyhkYXRlKTtcblx0XHRcdFx0XHRcdGlmIChtYXRjaCAmJiAobWF0Y2hlZCA9IHRydWUpKSB0aGlzLnJldkZvcm1hdFt0b2tlbl0ocGFyc2VkRGF0ZSwgbWF0Y2hbKyttYXRjaEluZGV4XSk7XG5cdFx0XHRcdFx0fSBlbHNlIGlmICghaXNCYWNrU2xhc2gpIHJlZ2V4U3RyICs9IFwiLlwiOyAvLyBkb24ndCByZWFsbHkgY2FyZVxuXHRcdFx0XHR9XG5cblx0XHRcdFx0ZGF0ZSA9IG1hdGNoZWQgPyBwYXJzZWREYXRlIDogbnVsbDtcblx0XHRcdH1cblx0XHR9IGVsc2UgaWYgKGRhdGUgaW5zdGFuY2VvZiBEYXRlKSBkYXRlID0gbmV3IERhdGUoZGF0ZS5nZXRUaW1lKCkpOyAvLyBjcmVhdGUgYSBjb3B5XG5cblx0XHQvKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuXHRcdGlmICghKGRhdGUgaW5zdGFuY2VvZiBEYXRlKSkge1xuXHRcdFx0Y29uc29sZS53YXJuKFwiZmxhdHBpY2tyOiBpbnZhbGlkIGRhdGUgXCIgKyBkYXRlX29yaWcpO1xuXHRcdFx0Y29uc29sZS5pbmZvKHRoaXMuZWxlbWVudCk7XG5cdFx0XHRyZXR1cm4gbnVsbDtcblx0XHR9XG5cblx0XHRpZiAodGhpcy5jb25maWcgJiYgdGhpcy5jb25maWcudXRjICYmICFkYXRlLmZwX2lzVVRDKSBkYXRlID0gZGF0ZS5mcF90b1VUQygpO1xuXG5cdFx0aWYgKHRpbWVsZXNzID09PSB0cnVlKSBkYXRlLnNldEhvdXJzKDAsIDAsIDAsIDApO1xuXG5cdFx0cmV0dXJuIGRhdGU7XG5cdH1cbn07XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5mdW5jdGlvbiBfZmxhdHBpY2tyKG5vZGVMaXN0LCBjb25maWcpIHtcblx0dmFyIG5vZGVzID0gQXJyYXkucHJvdG90eXBlLnNsaWNlLmNhbGwobm9kZUxpc3QpOyAvLyBzdGF0aWMgbGlzdFxuXHR2YXIgaW5zdGFuY2VzID0gW107XG5cdGZvciAodmFyIGkgPSAwOyBpIDwgbm9kZXMubGVuZ3RoOyBpKyspIHtcblx0XHR0cnkge1xuXHRcdFx0bm9kZXNbaV0uX2ZsYXRwaWNrciA9IG5ldyBGbGF0cGlja3Iobm9kZXNbaV0sIGNvbmZpZyB8fCB7fSk7XG5cdFx0XHRpbnN0YW5jZXMucHVzaChub2Rlc1tpXS5fZmxhdHBpY2tyKTtcblx0XHR9IGNhdGNoIChlKSB7XG5cdFx0XHRjb25zb2xlLndhcm4oZSwgZS5zdGFjayk7XG5cdFx0fVxuXHR9XG5cblx0cmV0dXJuIGluc3RhbmNlcy5sZW5ndGggPT09IDEgPyBpbnN0YW5jZXNbMF0gOiBpbnN0YW5jZXM7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5pZiAodHlwZW9mIEhUTUxFbGVtZW50ICE9PSBcInVuZGVmaW5lZFwiKSB7XG5cdC8vIGJyb3dzZXIgZW52XG5cdEhUTUxDb2xsZWN0aW9uLnByb3RvdHlwZS5mbGF0cGlja3IgPSBOb2RlTGlzdC5wcm90b3R5cGUuZmxhdHBpY2tyID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRcdHJldHVybiBfZmxhdHBpY2tyKHRoaXMsIGNvbmZpZyk7XG5cdH07XG5cblx0SFRNTEVsZW1lbnQucHJvdG90eXBlLmZsYXRwaWNrciA9IGZ1bmN0aW9uIChjb25maWcpIHtcblx0XHRyZXR1cm4gX2ZsYXRwaWNrcihbdGhpc10sIGNvbmZpZyk7XG5cdH07XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5mdW5jdGlvbiBmbGF0cGlja3Ioc2VsZWN0b3IsIGNvbmZpZykge1xuXHRyZXR1cm4gX2ZsYXRwaWNrcih3aW5kb3cuZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChzZWxlY3RvciksIGNvbmZpZyk7XG59XG5cbi8qIGlzdGFuYnVsIGlnbm9yZSBuZXh0ICovXG5pZiAodHlwZW9mIGpRdWVyeSAhPT0gXCJ1bmRlZmluZWRcIikge1xuXHRqUXVlcnkuZm4uZmxhdHBpY2tyID0gZnVuY3Rpb24gKGNvbmZpZykge1xuXHRcdHJldHVybiBfZmxhdHBpY2tyKHRoaXMsIGNvbmZpZyk7XG5cdH07XG59XG5cbkRhdGUucHJvdG90eXBlLmZwX2luY3IgPSBmdW5jdGlvbiAoZGF5cykge1xuXHRyZXR1cm4gbmV3IERhdGUodGhpcy5nZXRGdWxsWWVhcigpLCB0aGlzLmdldE1vbnRoKCksIHRoaXMuZ2V0RGF0ZSgpICsgcGFyc2VJbnQoZGF5cywgMTApKTtcbn07XG5cbkRhdGUucHJvdG90eXBlLmZwX2lzVVRDID0gZmFsc2U7XG5EYXRlLnByb3RvdHlwZS5mcF90b1VUQyA9IGZ1bmN0aW9uICgpIHtcblx0dmFyIG5ld0RhdGUgPSBuZXcgRGF0ZSh0aGlzLmdldFVUQ0Z1bGxZZWFyKCksIHRoaXMuZ2V0VVRDTW9udGgoKSwgdGhpcy5nZXRVVENEYXRlKCksIHRoaXMuZ2V0VVRDSG91cnMoKSwgdGhpcy5nZXRVVENNaW51dGVzKCksIHRoaXMuZ2V0VVRDU2Vjb25kcygpKTtcblxuXHRuZXdEYXRlLmZwX2lzVVRDID0gdHJ1ZTtcblx0cmV0dXJuIG5ld0RhdGU7XG59O1xuXG4vLyBJRTkgY2xhc3NMaXN0IHBvbHlmaWxsXG4vKiBpc3RhbmJ1bCBpZ25vcmUgbmV4dCAqL1xuaWYgKCF3aW5kb3cuZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LmNsYXNzTGlzdCAmJiBPYmplY3QuZGVmaW5lUHJvcGVydHkgJiYgdHlwZW9mIEhUTUxFbGVtZW50ICE9PSBcInVuZGVmaW5lZFwiKSB7XG5cdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShIVE1MRWxlbWVudC5wcm90b3R5cGUsIFwiY2xhc3NMaXN0XCIsIHtcblx0XHRnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcblx0XHRcdHZhciBzZWxmID0gdGhpcztcblx0XHRcdGZ1bmN0aW9uIHVwZGF0ZShmbikge1xuXHRcdFx0XHRyZXR1cm4gZnVuY3Rpb24gKHZhbHVlKSB7XG5cdFx0XHRcdFx0dmFyIGNsYXNzZXMgPSBzZWxmLmNsYXNzTmFtZS5zcGxpdCgvXFxzKy8pLFxuXHRcdFx0XHRcdCAgICBpbmRleCA9IGNsYXNzZXMuaW5kZXhPZih2YWx1ZSk7XG5cblx0XHRcdFx0XHRmbihjbGFzc2VzLCBpbmRleCwgdmFsdWUpO1xuXHRcdFx0XHRcdHNlbGYuY2xhc3NOYW1lID0gY2xhc3Nlcy5qb2luKFwiIFwiKTtcblx0XHRcdFx0fTtcblx0XHRcdH1cblxuXHRcdFx0dmFyIHJldCA9IHtcblx0XHRcdFx0YWRkOiB1cGRhdGUoZnVuY3Rpb24gKGNsYXNzZXMsIGluZGV4LCB2YWx1ZSkge1xuXHRcdFx0XHRcdGlmICghfmluZGV4KSBjbGFzc2VzLnB1c2godmFsdWUpO1xuXHRcdFx0XHR9KSxcblxuXHRcdFx0XHRyZW1vdmU6IHVwZGF0ZShmdW5jdGlvbiAoY2xhc3NlcywgaW5kZXgpIHtcblx0XHRcdFx0XHRpZiAofmluZGV4KSBjbGFzc2VzLnNwbGljZShpbmRleCwgMSk7XG5cdFx0XHRcdH0pLFxuXG5cdFx0XHRcdHRvZ2dsZTogdXBkYXRlKGZ1bmN0aW9uIChjbGFzc2VzLCBpbmRleCwgdmFsdWUpIHtcblx0XHRcdFx0XHRpZiAofmluZGV4KSBjbGFzc2VzLnNwbGljZShpbmRleCwgMSk7ZWxzZSBjbGFzc2VzLnB1c2godmFsdWUpO1xuXHRcdFx0XHR9KSxcblxuXHRcdFx0XHRjb250YWluczogZnVuY3Rpb24gY29udGFpbnModmFsdWUpIHtcblx0XHRcdFx0XHRyZXR1cm4gISF+c2VsZi5jbGFzc05hbWUuc3BsaXQoL1xccysvKS5pbmRleE9mKHZhbHVlKTtcblx0XHRcdFx0fSxcblxuXHRcdFx0XHRpdGVtOiBmdW5jdGlvbiBpdGVtKGkpIHtcblx0XHRcdFx0XHRyZXR1cm4gc2VsZi5jbGFzc05hbWUuc3BsaXQoL1xccysvKVtpXSB8fCBudWxsO1xuXHRcdFx0XHR9XG5cdFx0XHR9O1xuXG5cdFx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkocmV0LCBcImxlbmd0aFwiLCB7XG5cdFx0XHRcdGdldDogZnVuY3Rpb24gZ2V0KCkge1xuXHRcdFx0XHRcdHJldHVybiBzZWxmLmNsYXNzTmFtZS5zcGxpdCgvXFxzKy8pLmxlbmd0aDtcblx0XHRcdFx0fVxuXHRcdFx0fSk7XG5cblx0XHRcdHJldHVybiByZXQ7XG5cdFx0fVxuXHR9KTtcbn1cblxuaWYgKHR5cGVvZiBtb2R1bGUgIT09IFwidW5kZWZpbmVkXCIpIG1vZHVsZS5leHBvcnRzID0gRmxhdHBpY2tyO1xuIiwidmFyIFZ1ZSAvLyBsYXRlIGJpbmRcbnZhciB2ZXJzaW9uXG52YXIgbWFwID0gd2luZG93Ll9fVlVFX0hPVF9NQVBfXyA9IE9iamVjdC5jcmVhdGUobnVsbClcbnZhciBpbnN0YWxsZWQgPSBmYWxzZVxudmFyIGlzQnJvd3NlcmlmeSA9IGZhbHNlXG52YXIgaW5pdEhvb2tOYW1lID0gJ2JlZm9yZUNyZWF0ZSdcblxuZXhwb3J0cy5pbnN0YWxsID0gZnVuY3Rpb24gKHZ1ZSwgYnJvd3NlcmlmeSkge1xuICBpZiAoaW5zdGFsbGVkKSByZXR1cm5cbiAgaW5zdGFsbGVkID0gdHJ1ZVxuXG4gIFZ1ZSA9IHZ1ZS5fX2VzTW9kdWxlID8gdnVlLmRlZmF1bHQgOiB2dWVcbiAgdmVyc2lvbiA9IFZ1ZS52ZXJzaW9uLnNwbGl0KCcuJykubWFwKE51bWJlcilcbiAgaXNCcm93c2VyaWZ5ID0gYnJvd3NlcmlmeVxuXG4gIC8vIGNvbXBhdCB3aXRoIDwgMi4wLjAtYWxwaGEuN1xuICBpZiAoVnVlLmNvbmZpZy5fbGlmZWN5Y2xlSG9va3MuaW5kZXhPZignaW5pdCcpID4gLTEpIHtcbiAgICBpbml0SG9va05hbWUgPSAnaW5pdCdcbiAgfVxuXG4gIGV4cG9ydHMuY29tcGF0aWJsZSA9IHZlcnNpb25bMF0gPj0gMlxuICBpZiAoIWV4cG9ydHMuY29tcGF0aWJsZSkge1xuICAgIGNvbnNvbGUud2FybihcbiAgICAgICdbSE1SXSBZb3UgYXJlIHVzaW5nIGEgdmVyc2lvbiBvZiB2dWUtaG90LXJlbG9hZC1hcGkgdGhhdCBpcyAnICtcbiAgICAgICdvbmx5IGNvbXBhdGlibGUgd2l0aCBWdWUuanMgY29yZSBeMi4wLjAuJ1xuICAgIClcbiAgICByZXR1cm5cbiAgfVxufVxuXG4vKipcbiAqIENyZWF0ZSBhIHJlY29yZCBmb3IgYSBob3QgbW9kdWxlLCB3aGljaCBrZWVwcyB0cmFjayBvZiBpdHMgY29uc3RydWN0b3JcbiAqIGFuZCBpbnN0YW5jZXNcbiAqXG4gKiBAcGFyYW0ge1N0cmluZ30gaWRcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKi9cblxuZXhwb3J0cy5jcmVhdGVSZWNvcmQgPSBmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgdmFyIEN0b3IgPSBudWxsXG4gIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIEN0b3IgPSBvcHRpb25zXG4gICAgb3B0aW9ucyA9IEN0b3Iub3B0aW9uc1xuICB9XG4gIG1ha2VPcHRpb25zSG90KGlkLCBvcHRpb25zKVxuICBtYXBbaWRdID0ge1xuICAgIEN0b3I6IFZ1ZS5leHRlbmQob3B0aW9ucyksXG4gICAgaW5zdGFuY2VzOiBbXVxuICB9XG59XG5cbi8qKlxuICogTWFrZSBhIENvbXBvbmVudCBvcHRpb25zIG9iamVjdCBob3QuXG4gKlxuICogQHBhcmFtIHtTdHJpbmd9IGlkXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICovXG5cbmZ1bmN0aW9uIG1ha2VPcHRpb25zSG90IChpZCwgb3B0aW9ucykge1xuICBpbmplY3RIb29rKG9wdGlvbnMsIGluaXRIb29rTmFtZSwgZnVuY3Rpb24gKCkge1xuICAgIG1hcFtpZF0uaW5zdGFuY2VzLnB1c2godGhpcylcbiAgfSlcbiAgaW5qZWN0SG9vayhvcHRpb25zLCAnYmVmb3JlRGVzdHJveScsIGZ1bmN0aW9uICgpIHtcbiAgICB2YXIgaW5zdGFuY2VzID0gbWFwW2lkXS5pbnN0YW5jZXNcbiAgICBpbnN0YW5jZXMuc3BsaWNlKGluc3RhbmNlcy5pbmRleE9mKHRoaXMpLCAxKVxuICB9KVxufVxuXG4vKipcbiAqIEluamVjdCBhIGhvb2sgdG8gYSBob3QgcmVsb2FkYWJsZSBjb21wb25lbnQgc28gdGhhdFxuICogd2UgY2FuIGtlZXAgdHJhY2sgb2YgaXQuXG4gKlxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqIEBwYXJhbSB7U3RyaW5nfSBuYW1lXG4gKiBAcGFyYW0ge0Z1bmN0aW9ufSBob29rXG4gKi9cblxuZnVuY3Rpb24gaW5qZWN0SG9vayAob3B0aW9ucywgbmFtZSwgaG9vaykge1xuICB2YXIgZXhpc3RpbmcgPSBvcHRpb25zW25hbWVdXG4gIG9wdGlvbnNbbmFtZV0gPSBleGlzdGluZ1xuICAgID8gQXJyYXkuaXNBcnJheShleGlzdGluZylcbiAgICAgID8gZXhpc3RpbmcuY29uY2F0KGhvb2spXG4gICAgICA6IFtleGlzdGluZywgaG9va11cbiAgICA6IFtob29rXVxufVxuXG5mdW5jdGlvbiB0cnlXcmFwIChmbikge1xuICByZXR1cm4gZnVuY3Rpb24gKGlkLCBhcmcpIHtcbiAgICB0cnkgeyBmbihpZCwgYXJnKSB9IGNhdGNoIChlKSB7XG4gICAgICBjb25zb2xlLmVycm9yKGUpXG4gICAgICBjb25zb2xlLndhcm4oJ1NvbWV0aGluZyB3ZW50IHdyb25nIGR1cmluZyBWdWUgY29tcG9uZW50IGhvdC1yZWxvYWQuIEZ1bGwgcmVsb2FkIHJlcXVpcmVkLicpXG4gICAgfVxuICB9XG59XG5cbmV4cG9ydHMucmVyZW5kZXIgPSB0cnlXcmFwKGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICB2YXIgcmVjb3JkID0gbWFwW2lkXVxuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBvcHRpb25zID0gb3B0aW9ucy5vcHRpb25zXG4gIH1cbiAgcmVjb3JkLkN0b3Iub3B0aW9ucy5yZW5kZXIgPSBvcHRpb25zLnJlbmRlclxuICByZWNvcmQuQ3Rvci5vcHRpb25zLnN0YXRpY1JlbmRlckZucyA9IG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zXG4gIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgIGluc3RhbmNlLiRvcHRpb25zLnJlbmRlciA9IG9wdGlvbnMucmVuZGVyXG4gICAgaW5zdGFuY2UuJG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zID0gb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnNcbiAgICBpbnN0YW5jZS5fc3RhdGljVHJlZXMgPSBbXSAvLyByZXNldCBzdGF0aWMgdHJlZXNcbiAgICBpbnN0YW5jZS4kZm9yY2VVcGRhdGUoKVxuICB9KVxufSlcblxuZXhwb3J0cy5yZWxvYWQgPSB0cnlXcmFwKGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBvcHRpb25zID0gb3B0aW9ucy5vcHRpb25zXG4gIH1cbiAgbWFrZU9wdGlvbnNIb3QoaWQsIG9wdGlvbnMpXG4gIHZhciByZWNvcmQgPSBtYXBbaWRdXG4gIGlmICh2ZXJzaW9uWzFdIDwgMikge1xuICAgIC8vIHByZXNlcnZlIHByZSAyLjIgYmVoYXZpb3IgZm9yIGdsb2JhbCBtaXhpbiBoYW5kbGluZ1xuICAgIHJlY29yZC5DdG9yLmV4dGVuZE9wdGlvbnMgPSBvcHRpb25zXG4gIH1cbiAgdmFyIG5ld0N0b3IgPSByZWNvcmQuQ3Rvci5zdXBlci5leHRlbmQob3B0aW9ucylcbiAgcmVjb3JkLkN0b3Iub3B0aW9ucyA9IG5ld0N0b3Iub3B0aW9uc1xuICByZWNvcmQuQ3Rvci5jaWQgPSBuZXdDdG9yLmNpZFxuICByZWNvcmQuQ3Rvci5wcm90b3R5cGUgPSBuZXdDdG9yLnByb3RvdHlwZVxuICBpZiAobmV3Q3Rvci5yZWxlYXNlKSB7XG4gICAgLy8gdGVtcG9yYXJ5IGdsb2JhbCBtaXhpbiBzdHJhdGVneSB1c2VkIGluIDwgMi4wLjAtYWxwaGEuNlxuICAgIG5ld0N0b3IucmVsZWFzZSgpXG4gIH1cbiAgcmVjb3JkLmluc3RhbmNlcy5zbGljZSgpLmZvckVhY2goZnVuY3Rpb24gKGluc3RhbmNlKSB7XG4gICAgaWYgKGluc3RhbmNlLiR2bm9kZSAmJiBpbnN0YW5jZS4kdm5vZGUuY29udGV4dCkge1xuICAgICAgaW5zdGFuY2UuJHZub2RlLmNvbnRleHQuJGZvcmNlVXBkYXRlKClcbiAgICB9IGVsc2Uge1xuICAgICAgY29uc29sZS53YXJuKCdSb290IG9yIG1hbnVhbGx5IG1vdW50ZWQgaW5zdGFuY2UgbW9kaWZpZWQuIEZ1bGwgcmVsb2FkIHJlcXVpcmVkLicpXG4gICAgfVxuICB9KVxufSlcbiIsIu+7vzx0ZW1wbGF0ZT5cclxuICAgIDxkaXYgY2xhc3M9XCJmb3JtLWRhdGUgaW5wdXQtZ3JvdXBcIj5cclxuICAgICAgICA8aW5wdXQgdHlwZT1cInRleHRcIiA6Y2xhc3M9XCJpbnB1dENsYXNzXCIgOnBsYWNlaG9sZGVyPVwicGxhY2Vob2xkZXJcIiA6dmFsdWU9XCJ2YWx1ZVwiIEBpbnB1dD1cIm9uSW5wdXRcIiAvPlxyXG4gICAgICAgIDxzcGFuIGNsYXNzPVwiaW5wdXQtZ3JvdXAtYWRkb25cIj5cclxuICAgICAgICAgICAgPHNwYW4gY2xhc3M9XCJjYWxlbmRhclwiPjwvc3Bhbj5cclxuICAgICAgICA8L3NwYW4+XHJcbiAgICA8L2Rpdj5cclxuPC90ZW1wbGF0ZT5cclxuXHJcbjxzY3JpcHQ+XHJcbmltcG9ydCBGbGF0cGlja3IgZnJvbSAnZmxhdHBpY2tyJ1xyXG5cclxuZXhwb3J0IGRlZmF1bHQge1xyXG4gICAgcHJvcHM6IHtcclxuICAgICAgICBpbnB1dENsYXNzOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IFN0cmluZ1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcGxhY2Vob2xkZXI6IHtcclxuICAgICAgICAgICAgdHlwZTogU3RyaW5nLFxyXG4gICAgICAgICAgICBkZWZhdWx0OiAnJ1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgb3B0aW9uczoge1xyXG4gICAgICAgICAgICB0eXBlOiBPYmplY3QsXHJcbiAgICAgICAgICAgIGRlZmF1bHQ6ICgpID0+IHsgcmV0dXJuIHt9IH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIHZhbHVlOiB7XHJcbiAgICAgICAgICAgIHR5cGU6IFN0cmluZyxcclxuICAgICAgICAgICAgZGVmYXVsdDogJydcclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gIGRhdGEgKCkge1xyXG4gICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgZnA6IG51bGxcclxuICAgICAgfVxyXG4gIH0sXHJcbiAgICBjb21wdXRlZDoge1xyXG4gICAgICAgIGZwT3B0aW9ucyAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBKU09OLnN0cmluZ2lmeSh0aGlzLm9wdGlvbnMpXHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICAgIHdhdGNoOiB7XHJcbiAgICAgICAgZnBPcHRpb25zIChuZXdPcHQpIHtcclxuICAgICAgICAgICAgY29uc3Qgb3B0aW9uID0gSlNPTi5wYXJzZShuZXdPcHQpXHJcbiAgICAgICAgICAgIGZvciAobGV0IG8gaW4gb3B0aW9uKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmZwLnNldChvLCBvcHRpb25bb10pXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9LFxyXG4gIG1vdW50ZWQgKCkge1xyXG4gICAgICBjb25zdCBzZWxmID0gdGhpc1xyXG4gICAgICBjb25zdCBvcmlnT25WYWxVcGRhdGUgPSB0aGlzLm9wdGlvbnMub25WYWx1ZVVwZGF0ZVxyXG4gICAgICB0aGlzLmZwID0gbmV3IEZsYXRwaWNrcih0aGlzLiRlbC5xdWVyeVNlbGVjdG9yKCdpbnB1dCcpLCBPYmplY3QuYXNzaWduKHRoaXMub3B0aW9ucywge1xyXG4gICAgICAgICAgb25WYWx1ZVVwZGF0ZSAoKSB7XHJcbiAgICAgICAgICAgICAgc2VsZi5vbklucHV0KHNlbGYuJGVsLnF1ZXJ5U2VsZWN0b3IoJ2lucHV0JykudmFsdWUpXHJcbiAgICAgICAgICAgICAgaWYgKHR5cGVvZiBvcmlnT25WYWxVcGRhdGUgPT09ICdmdW5jdGlvbicpIHtcclxuICAgICAgICAgICAgICAgICAgb3JpZ09uVmFsVXBkYXRlKClcclxuICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICB9XHJcbiAgICAgIH0pKVxyXG4gICAgICB0aGlzLiRlbWl0KCdGbGF0cGlja3JSZWYnLCB0aGlzLmZwKVxyXG4gIH0sXHJcbiAgZGVzdHJveWVkICgpIHtcclxuICAgICAgdGhpcy5mcC5kZXN0cm95KClcclxuICAgICAgdGhpcy5mcCA9IG51bGxcclxuICB9LFxyXG4gICAgbWV0aG9kczoge1xyXG4gICAgICAgIG9uSW5wdXQgKGUpIHtcclxuICAgICAgICAgICAgY29uc3Qgc2VsZWN0ZWREYXRlcyA9IHRoaXMuZnAuc2VsZWN0ZWREYXRlcyB8fCBbXTtcclxuICAgICAgICAgICAgY29uc3QgbGVmdCA9IHNlbGVjdGVkRGF0ZXMubGVuZ3RoID4gMCA/IHNlbGVjdGVkRGF0ZXNbMF0gOiBudWxsO1xyXG4gICAgICAgICAgICBjb25zdCByaWdodCA9IHNlbGVjdGVkRGF0ZXMubGVuZ3RoID4gMSA/IHNlbGVjdGVkRGF0ZXNbMV0gOiBudWxsO1xyXG4gICAgICAgICAgICB0aGlzLiRlbWl0KCdpbnB1dCcsICh0eXBlb2YgZSA9PT0gJ3N0cmluZycgPyBlIDogZS50YXJnZXQudmFsdWUpLCBsZWZ0LCByaWdodClcclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuPC9zY3JpcHQ+Iiwi77u/PHRlbXBsYXRlPlxyXG4gICAgPGRpdiBjbGFzcz1cImNvbWJvLWJveFwiPlxyXG4gICAgICAgIDxkaXYgY2xhc3M9XCJidG4tZ3JvdXAgYnRuLWlucHV0IGNsZWFyZml4XCI+XHJcbiAgICAgICAgICAgIDxidXR0b24gdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuIGRyb3Bkb3duLXRvZ2dsZVwiIGRhdGEtdG9nZ2xlPVwiZHJvcGRvd25cIj5cclxuICAgICAgICAgICAgICAgIDxzcGFuIGRhdGEtYmluZD1cImxhYmVsXCIgdi1pZj1cInZhbHVlID09PSBudWxsXCIgY2xhc3M9XCJncmF5LXRleHRcIj57e3BsYWNlaG9sZGVyVGV4dH19PC9zcGFuPlxyXG4gICAgICAgICAgICAgICAgPHNwYW4gZGF0YS1iaW5kPVwibGFiZWxcIiB2LWVsc2U+e3t2YWx1ZS52YWx1ZX19PC9zcGFuPlxyXG4gICAgICAgICAgICA8L2J1dHRvbj5cclxuICAgICAgICAgICAgPHVsIHJlZj1cImRyb3Bkb3duTWVudVwiIGNsYXNzPVwiZHJvcGRvd24tbWVudVwiIHJvbGU9XCJtZW51XCI+XHJcbiAgICAgICAgICAgICAgICA8bGk+XHJcbiAgICAgICAgICAgICAgICAgICAgPGlucHV0IHR5cGU9XCJ0ZXh0XCIgcmVmPVwic2VhcmNoQm94XCIgOmlkPVwiaW5wdXRJZFwiIHBsYWNlaG9sZGVyPVwiU2VhcmNoXCIgQGlucHV0PVwidXBkYXRlT3B0aW9uc0xpc3RcIiB2LW9uOmtleXVwLmRvd249XCJvblNlYXJjaEJveERvd25LZXlcIiB2LW1vZGVsPVwic2VhcmNoVGVybVwiIC8+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtZm9yPVwib3B0aW9uIGluIG9wdGlvbnNcIiA6a2V5PVwib3B0aW9uLmtleVwiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhIGhyZWY9XCJqYXZhc2NyaXB0OnZvaWQoMCk7XCIgdi1vbjpjbGljaz1cInNlbGVjdE9wdGlvbihvcHRpb24pXCIgdi1odG1sPVwiaGlnaGxpZ2h0KG9wdGlvbi52YWx1ZSwgc2VhcmNoVGVybSlcIiB2LW9uOmtleWRvd24udXA9XCJvbk9wdGlvblVwS2V5XCI+PC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWlmPVwiaXNMb2FkaW5nXCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGE+TG9hZGluZy4uLjwvYT5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1pZj1cIiFpc0xvYWRpbmcgJiYgb3B0aW9ucy5sZW5ndGggPT09IDBcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YT5ObyByZXN1bHRzIGZvdW5kPC9hPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgPC91bD5cclxuICAgICAgICA8L2Rpdj5cclxuICAgICAgICA8YnV0dG9uIHYtaWY9XCJ2YWx1ZSAhPT0gbnVsbFwiIGNsYXNzPVwiYnRuIGJ0bi1saW5rIGJ0bi1jbGVhclwiIEBjbGljaz1cImNsZWFyXCI+XHJcbiAgICAgICAgICAgIDxzcGFuPjwvc3Bhbj5cclxuICAgICAgICA8L2J1dHRvbj5cclxuICAgIDwvZGl2PlxyXG48L3RlbXBsYXRlPlxyXG5cclxuPHNjcmlwdD5cclxuICAgIG1vZHVsZS5leHBvcnRzID0ge1xyXG4gICAgICAgIG5hbWU6ICd1c2VyLXNlbGVjdG9yJyxcclxuICAgICAgICBwcm9wczogWydmZXRjaFVybCcsICdjb250cm9sSWQnLCAndmFsdWUnLCAncGxhY2Vob2xkZXInXSxcclxuICAgICAgICBkYXRhOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zOiBbXSxcclxuICAgICAgICAgICAgICAgIGlzTG9hZGluZzogZmFsc2UsXHJcbiAgICAgICAgICAgICAgICBzZWFyY2hUZXJtOiAnJ1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgY29tcHV0ZWQ6IHtcclxuICAgICAgICAgICAgaW5wdXRJZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGBzYl8ke3RoaXMuY29udHJvbElkfWA7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHBsYWNlaG9sZGVyVGV4dDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMucGxhY2Vob2xkZXIgfHwgXCJTZWxlY3RcIjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgbW91bnRlZDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGNvbnN0IGpxRWwgPSAkKHRoaXMuJGVsKVxyXG4gICAgICAgICAgICBjb25zdCBmb2N1c1RvID0ganFFbC5maW5kKGAjJHt0aGlzLmlucHV0SWR9YClcclxuICAgICAgICAgICAganFFbC5vbignc2hvd24uYnMuZHJvcGRvd24nLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICBmb2N1c1RvLmZvY3VzKClcclxuICAgICAgICAgICAgICAgIHRoaXMuZmV0Y2hPcHRpb25zKHRoaXMuc2VhcmNoVGVybSlcclxuICAgICAgICAgICAgfSlcclxuXHJcbiAgICAgICAgICAgIGpxRWwub24oJ2hpZGRlbi5icy5kcm9wZG93bicsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VhcmNoVGVybSA9IFwiXCJcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICB9LFxyXG4gICAgICAgIG1ldGhvZHM6IHtcclxuICAgICAgICAgICAgb25TZWFyY2hCb3hEb3duS2V5KGV2ZW50KSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgJGZpcnN0T3B0aW9uQW5jaG9yID0gJCh0aGlzLiRyZWZzLmRyb3Bkb3duTWVudSkuZmluZCgnYScpLmZpcnN0KCk7XHJcbiAgICAgICAgICAgICAgICAkZmlyc3RPcHRpb25BbmNob3IuZm9jdXMoKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgb25PcHRpb25VcEtleShldmVudCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGlzRmlyc3RPcHRpb24gPSAkKGV2ZW50LnRhcmdldCkucGFyZW50KCkuaW5kZXgoKSA9PT0gMTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoaXNGaXJzdE9wdGlvbikge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuJHJlZnMuc2VhcmNoQm94LmZvY3VzKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnQuc3RvcFByb3BhZ2F0aW9uKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGZldGNoT3B0aW9uczogZnVuY3Rpb24gKGZpbHRlciA9IFwiXCIpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUubG9nKGBmaWx0ZXI6IHtmaWx0ZXJ9YCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRodHRwLmdldCh0aGlzLmZldGNoVXJsLCB7cGFyYW1zOiB7IHF1ZXJ5OiBmaWx0ZXIgfX0pXHJcbiAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLm9wdGlvbnMgPSByZXNwb25zZS5ib2R5Lm9wdGlvbnMgfHwgW107XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfSwgcmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGNsZWFyOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRlbWl0KCdzZWxlY3RlZCcsIG51bGwsIHRoaXMuY29udHJvbElkKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VhcmNoVGVybSA9IFwiXCI7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHNlbGVjdE9wdGlvbjogZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRlbWl0KCdzZWxlY3RlZCcsIHZhbHVlLCB0aGlzLmNvbnRyb2xJZCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHVwZGF0ZU9wdGlvbnNMaXN0KGUpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZmV0Y2hPcHRpb25zKGUudGFyZ2V0LnZhbHVlKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgaGlnaGxpZ2h0OiBmdW5jdGlvbiAodGl0bGUsIHNlYXJjaFRlcm0pIHtcclxuICAgICAgICAgICAgICAgIHZhciBlbmNvZGVkVGl0bGUgPSBfLmVzY2FwZSh0aXRsZSk7XHJcbiAgICAgICAgICAgICAgICBpZiAoc2VhcmNoVGVybSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBzYWZlU2VhcmNoVGVybSA9IF8uZXNjYXBlKF8uZXNjYXBlUmVnRXhwKHNlYXJjaFRlcm0pKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGlRdWVyeSA9IG5ldyBSZWdFeHAoc2FmZVNlYXJjaFRlcm0sIFwiaWdcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGVuY29kZWRUaXRsZS5yZXBsYWNlKGlRdWVyeSwgKG1hdGNoZWRUeHQsIGEsIGIpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGA8c3Ryb25nPiR7bWF0Y2hlZFR4dH08L3N0cm9uZz5gO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiBlbmNvZGVkVGl0bGU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9O1xyXG48L3NjcmlwdD4iLCLvu7/vu79pbXBvcnQgVnVlIGZyb20gJ3Z1ZSdcclxuaW1wb3J0IFZ1ZVJlc291cmNlIGZyb20gJ3Z1ZS1yZXNvdXJjZSdcclxuaW1wb3J0IFVzZXJTZWxlY3RvciBmcm9tICcuL1VzZXJTZWxlY3Rvci52dWUnXHJcbmltcG9ydCBEYXRlUGlja2VyIGZyb20gJy4vRGF0ZVBpY2tlci52dWUnXHJcbmltcG9ydCBWZWVWYWxpZGF0ZSBmcm9tICd2ZWUtdmFsaWRhdGUnO1xyXG5cclxuVnVlLnVzZShWZWVWYWxpZGF0ZSk7XHJcblZ1ZS51c2UoVnVlUmVzb3VyY2UpO1xyXG5cclxuVnVlLmNvbXBvbmVudCgnRmxhdHBpY2tyJywgRGF0ZVBpY2tlcik7XHJcblZ1ZS5jb21wb25lbnQoXCJ1c2VyLXNlbGVjdG9yXCIsIFVzZXJTZWxlY3Rvcik7XHJcblxyXG52YXIgYXBwID0gbmV3IFZ1ZSh7XHJcbiAgICBkYXRhOiB7XHJcbiAgICAgICAgaW50ZXJ2aWV3ZXJJZDogbnVsbCxcclxuICAgICAgICBxdWVzdGlvbm5haXJlSWQ6IG51bGwsXHJcbiAgICAgICAgZGF0ZUZyb206IG51bGwsXHJcbiAgICAgICAgZGF0ZVRvOiBudWxsLFxyXG4gICAgICAgIGRhdGVSYW5nZVBpY2tlck9wdGlvbnM6IHtcclxuICAgICAgICAgICAgbW9kZTogXCJyYW5nZVwiLFxyXG4gICAgICAgICAgICBtYXhEYXRlOiBcInRvZGF5XCIsXHJcbiAgICAgICAgICAgIG1pbkRhdGU6IG5ldyBEYXRlKCkuZnBfaW5jcigtMzApXHJcbiAgICAgICAgfVxyXG4gICAgfSxcclxuICAgIG1ldGhvZHM6IHtcclxuICAgICAgICB1c2VyU2VsZWN0ZWQobmV3VmFsdWUpIHtcclxuICAgICAgICAgICAgdGhpcy5pbnRlcnZpZXdlcklkID0gbmV3VmFsdWU7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBxdWVzdGlvbm5haXJlU2VsZWN0ZWQobmV3VmFsdWUpIHtcclxuICAgICAgICAgICAgdGhpcy5xdWVzdGlvbm5haXJlSWQgPSBuZXdWYWx1ZTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIHJhbmdlU2VsZWN0ZWQodGV4dFZhbHVlLCBmcm9tLCB0bykge1xyXG4gICAgICAgICAgICB0aGlzLmRhdGVGcm9tID0gZnJvbTtcclxuICAgICAgICAgICAgdGhpcy5kYXRlVG8gPSB0bztcclxuICAgICAgICB9LFxyXG4gICAgICAgIHZhbGlkYXRlRm9ybSgpIHtcclxuICAgICAgICAgICAgdGhpcy4kdmFsaWRhdG9yLnZhbGlkYXRlQWxsKCkudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgaWYgKHJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuZmluZEludGVydmlld3MoKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBmaW5kSW50ZXJ2aWV3cygpIHtcclxuICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIm1haW5cIikuY2xhc3NMaXN0LnJlbW92ZShcInNlYXJjaC13YXNudC1zdGFydGVkXCIpO1xyXG4gICAgICAgIH1cclxuICAgIH0sXHJcbiAgICBtb3VudGVkOiBmdW5jdGlvbigpIHtcclxuICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwibWFpblwiKS5jbGFzc0xpc3QucmVtb3ZlKFwiaG9sZC10cmFuc2l0aW9uXCIpO1xyXG4gICAgfVxyXG59KTtcclxuXHJcbndpbmRvdy5vbmxvYWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICBWdWUuaHR0cC5oZWFkZXJzLmNvbW1vblsnQXV0aG9yaXphdGlvbiddID0gaW5wdXQuc2V0dGluZ3MuYWNzcmYudG9rZW47XHJcblxyXG4gICAgYXBwLiRtb3VudCgnI2FwcCcpO1xyXG59Il19
