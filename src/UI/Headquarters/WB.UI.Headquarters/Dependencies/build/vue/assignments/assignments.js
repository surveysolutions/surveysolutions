(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
module.exports = { "default": require("core-js/library/fn/object/assign"), __esModule: true };
},{"core-js/library/fn/object/assign":2}],2:[function(require,module,exports){
require('../../modules/es6.object.assign');
module.exports = require('../../modules/_core').Object.assign;
},{"../../modules/_core":7,"../../modules/es6.object.assign":37}],3:[function(require,module,exports){
module.exports = function(it){
  if(typeof it != 'function')throw TypeError(it + ' is not a function!');
  return it;
};
},{}],4:[function(require,module,exports){
var isObject = require('./_is-object');
module.exports = function(it){
  if(!isObject(it))throw TypeError(it + ' is not an object!');
  return it;
};
},{"./_is-object":20}],5:[function(require,module,exports){
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
},{"./_to-index":30,"./_to-iobject":32,"./_to-length":33}],6:[function(require,module,exports){
var toString = {}.toString;

module.exports = function(it){
  return toString.call(it).slice(8, -1);
};
},{}],7:[function(require,module,exports){
var core = module.exports = {version: '2.4.0'};
if(typeof __e == 'number')__e = core; // eslint-disable-line no-undef
},{}],8:[function(require,module,exports){
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
},{"./_a-function":3}],9:[function(require,module,exports){
// 7.2.1 RequireObjectCoercible(argument)
module.exports = function(it){
  if(it == undefined)throw TypeError("Can't call method on  " + it);
  return it;
};
},{}],10:[function(require,module,exports){
// Thank's IE8 for his funny defineProperty
module.exports = !require('./_fails')(function(){
  return Object.defineProperty({}, 'a', {get: function(){ return 7; }}).a != 7;
});
},{"./_fails":14}],11:[function(require,module,exports){
var isObject = require('./_is-object')
  , document = require('./_global').document
  // in old IE typeof document.createElement is 'object'
  , is = isObject(document) && isObject(document.createElement);
module.exports = function(it){
  return is ? document.createElement(it) : {};
};
},{"./_global":15,"./_is-object":20}],12:[function(require,module,exports){
// IE 8- don't enum bug keys
module.exports = (
  'constructor,hasOwnProperty,isPrototypeOf,propertyIsEnumerable,toLocaleString,toString,valueOf'
).split(',');
},{}],13:[function(require,module,exports){
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
},{"./_core":7,"./_ctx":8,"./_global":15,"./_hide":17}],14:[function(require,module,exports){
module.exports = function(exec){
  try {
    return !!exec();
  } catch(e){
    return true;
  }
};
},{}],15:[function(require,module,exports){
// https://github.com/zloirock/core-js/issues/86#issuecomment-115759028
var global = module.exports = typeof window != 'undefined' && window.Math == Math
  ? window : typeof self != 'undefined' && self.Math == Math ? self : Function('return this')();
if(typeof __g == 'number')__g = global; // eslint-disable-line no-undef
},{}],16:[function(require,module,exports){
var hasOwnProperty = {}.hasOwnProperty;
module.exports = function(it, key){
  return hasOwnProperty.call(it, key);
};
},{}],17:[function(require,module,exports){
var dP         = require('./_object-dp')
  , createDesc = require('./_property-desc');
module.exports = require('./_descriptors') ? function(object, key, value){
  return dP.f(object, key, createDesc(1, value));
} : function(object, key, value){
  object[key] = value;
  return object;
};
},{"./_descriptors":10,"./_object-dp":22,"./_property-desc":27}],18:[function(require,module,exports){
module.exports = !require('./_descriptors') && !require('./_fails')(function(){
  return Object.defineProperty(require('./_dom-create')('div'), 'a', {get: function(){ return 7; }}).a != 7;
});
},{"./_descriptors":10,"./_dom-create":11,"./_fails":14}],19:[function(require,module,exports){
// fallback for non-array-like ES3 and non-enumerable old V8 strings
var cof = require('./_cof');
module.exports = Object('z').propertyIsEnumerable(0) ? Object : function(it){
  return cof(it) == 'String' ? it.split('') : Object(it);
};
},{"./_cof":6}],20:[function(require,module,exports){
module.exports = function(it){
  return typeof it === 'object' ? it !== null : typeof it === 'function';
};
},{}],21:[function(require,module,exports){
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
},{"./_fails":14,"./_iobject":19,"./_object-gops":23,"./_object-keys":25,"./_object-pie":26,"./_to-object":34}],22:[function(require,module,exports){
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
},{"./_an-object":4,"./_descriptors":10,"./_ie8-dom-define":18,"./_to-primitive":35}],23:[function(require,module,exports){
exports.f = Object.getOwnPropertySymbols;
},{}],24:[function(require,module,exports){
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
},{"./_array-includes":5,"./_has":16,"./_shared-key":28,"./_to-iobject":32}],25:[function(require,module,exports){
// 19.1.2.14 / 15.2.3.14 Object.keys(O)
var $keys       = require('./_object-keys-internal')
  , enumBugKeys = require('./_enum-bug-keys');

module.exports = Object.keys || function keys(O){
  return $keys(O, enumBugKeys);
};
},{"./_enum-bug-keys":12,"./_object-keys-internal":24}],26:[function(require,module,exports){
exports.f = {}.propertyIsEnumerable;
},{}],27:[function(require,module,exports){
module.exports = function(bitmap, value){
  return {
    enumerable  : !(bitmap & 1),
    configurable: !(bitmap & 2),
    writable    : !(bitmap & 4),
    value       : value
  };
};
},{}],28:[function(require,module,exports){
var shared = require('./_shared')('keys')
  , uid    = require('./_uid');
module.exports = function(key){
  return shared[key] || (shared[key] = uid(key));
};
},{"./_shared":29,"./_uid":36}],29:[function(require,module,exports){
var global = require('./_global')
  , SHARED = '__core-js_shared__'
  , store  = global[SHARED] || (global[SHARED] = {});
module.exports = function(key){
  return store[key] || (store[key] = {});
};
},{"./_global":15}],30:[function(require,module,exports){
var toInteger = require('./_to-integer')
  , max       = Math.max
  , min       = Math.min;
module.exports = function(index, length){
  index = toInteger(index);
  return index < 0 ? max(index + length, 0) : min(index, length);
};
},{"./_to-integer":31}],31:[function(require,module,exports){
// 7.1.4 ToInteger
var ceil  = Math.ceil
  , floor = Math.floor;
module.exports = function(it){
  return isNaN(it = +it) ? 0 : (it > 0 ? floor : ceil)(it);
};
},{}],32:[function(require,module,exports){
// to indexed object, toObject with fallback for non-array-like ES3 strings
var IObject = require('./_iobject')
  , defined = require('./_defined');
module.exports = function(it){
  return IObject(defined(it));
};
},{"./_defined":9,"./_iobject":19}],33:[function(require,module,exports){
// 7.1.15 ToLength
var toInteger = require('./_to-integer')
  , min       = Math.min;
module.exports = function(it){
  return it > 0 ? min(toInteger(it), 0x1fffffffffffff) : 0; // pow(2, 53) - 1 == 9007199254740991
};
},{"./_to-integer":31}],34:[function(require,module,exports){
// 7.1.13 ToObject(argument)
var defined = require('./_defined');
module.exports = function(it){
  return Object(defined(it));
};
},{"./_defined":9}],35:[function(require,module,exports){
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
},{"./_is-object":20}],36:[function(require,module,exports){
var id = 0
  , px = Math.random();
module.exports = function(key){
  return 'Symbol('.concat(key === undefined ? '' : key, ')_', (++id + px).toString(36));
};
},{}],37:[function(require,module,exports){
// 19.1.3.1 Object.assign(target, source)
var $export = require('./_export');

$export($export.S + $export.F, 'Object', {assign: require('./_object-assign')});
},{"./_export":13,"./_object-assign":21}],38:[function(require,module,exports){
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

},{}],39:[function(require,module,exports){
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
    hotAPI.rerender("data-v-25b84282", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/object/assign":1,"vue-hot-reload-api":38}],40:[function(require,module,exports){
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
      this.table.rows().deselect();
      this.table.ajax.reload();
    },
    onTableInitComplete: function onTableInitComplete() {
      var self = this;
      $(this.$el).parent('.dataTables_wrapper').find('.dataTables_filter label').on('click', function (e) {
        if (e.target !== this) return;
        if ($(this).hasClass("active")) {
          $(this).removeClass("active");
        } else {
          $(this).addClass("active");
          $(this).children("input[type='search']").delay(200).queue(function () {
            $(this).focus();$(this).dequeue();
          });
        }
      });
      var firstHeader = this.table.column(0).header();
      $(firstHeader).html('<input class="double-checkbox" id="check-all" type="checkbox">' + '<label for="check-all">' + '<span class="tick"></span>' + '</label>');
      $('#check-all').change(function () {
        if (!this.checked) {
          self.table.rows().deselect();
          $(self.table.rows).find(".checkbox-filter").prop('checked', false);
        } else {
          self.table.rows().select();
          $(self.table.rows).find(".checkbox-filter").prop('checked', true);
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
      pageLength: 20,
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
    this.table.on('select', function (e, dt, type, indexes) {
      self.$emit('select', e, dt, type, indexes);
    });
    this.table.on('deselect', function (e, dt, type, indexes) {
      self.$emit('deselect', e, dt, type, indexes);
    });
    this.table.on('click', 'tbody td', function () {
      var cell = self.table.cell(this);

      if (cell.index().column > 0) {
        var rowId = self.table.row(this).id();
        var columns = self.table.settings().init().columns;

        self.$emit('cell-clicked', columns[this.cellIndex].name, rowId, cell.data());
      }
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
__vue__options__.staticRenderFns = [function render () {var _vm=this;var _h=_vm.$createElement;var _c=_vm._self._c||_h;return _c('table',{staticClass:"table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews responsive"},[_c('thead'),_vm._v(" "),_c('tbody')])}]
if (module.hot) {(function () {  var hotAPI = require("vue-hot-reload-api")
  hotAPI.install((typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null), true)
  if (!hotAPI.compatible) return
  module.hot.accept()
  if (!module.hot.data) {
    hotAPI.createRecord("data-v-07594013", __vue__options__)
  } else {
    hotAPI.reload("data-v-07594013", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"babel-runtime/core-js/object/assign":1,"vue-hot-reload-api":38}],41:[function(require,module,exports){
(function (global){
'use strict';

var _vue = (typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null);

var _vue2 = _interopRequireDefault(_vue);

var _vueResource = (typeof window !== "undefined" ? window['VueResource'] : typeof global !== "undefined" ? global['VueResource'] : null);

var _vueResource2 = _interopRequireDefault(_vueResource);

var _Typeahead = require('./../Typeahead.vue');

var _Typeahead2 = _interopRequireDefault(_Typeahead);

var _AssignmentsTable = require('./AssignmentsTable.vue');

var _AssignmentsTable2 = _interopRequireDefault(_AssignmentsTable);

var _veeValidate = (typeof window !== "undefined" ? window['VeeValidate'] : typeof global !== "undefined" ? global['VeeValidate'] : null);

var _veeValidate2 = _interopRequireDefault(_veeValidate);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

_vue2.default.use(_vueResource2.default);

_vue2.default.component("typeahead", _Typeahead2.default);
_vue2.default.use(_veeValidate2.default);
_vue2.default.component("assignments-table", _AssignmentsTable2.default);

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"./../Typeahead.vue":39,"./AssignmentsTable.vue":40}]},{},[41])
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvYmFiZWwtcnVudGltZS9jb3JlLWpzL29iamVjdC9hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L2ZuL29iamVjdC9hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2EtZnVuY3Rpb24uanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2FuLW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fYXJyYXktaW5jbHVkZXMuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2NvZi5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY29yZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fY3R4LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kZWZpbmVkLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19kZXNjcmlwdG9ycy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fZG9tLWNyZWF0ZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fZW51bS1idWcta2V5cy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fZXhwb3J0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19mYWlscy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fZ2xvYmFsLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19oYXMuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2hpZGUuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX2llOC1kb20tZGVmaW5lLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19pb2JqZWN0LmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19pcy1vYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1hc3NpZ24uanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1kcC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LWdvcHMuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX29iamVjdC1rZXlzLWludGVybmFsLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL19vYmplY3Qta2V5cy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fb2JqZWN0LXBpZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fcHJvcGVydHktZGVzYy5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fc2hhcmVkLWtleS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fc2hhcmVkLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1pbmRleC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8taW50ZWdlci5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8taW9iamVjdC5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdG8tbGVuZ3RoLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL190by1vYmplY3QuanMiLCJub2RlX21vZHVsZXMvY29yZS1qcy9saWJyYXJ5L21vZHVsZXMvX3RvLXByaW1pdGl2ZS5qcyIsIm5vZGVfbW9kdWxlcy9jb3JlLWpzL2xpYnJhcnkvbW9kdWxlcy9fdWlkLmpzIiwibm9kZV9tb2R1bGVzL2NvcmUtanMvbGlicmFyeS9tb2R1bGVzL2VzNi5vYmplY3QuYXNzaWduLmpzIiwibm9kZV9tb2R1bGVzL3Z1ZS1ob3QtcmVsb2FkLWFwaS9pbmRleC5qcyIsInZ1ZVxcdnVlXFxUeXBlYWhlYWQudnVlP2Y5Y2NmOGIwIiwidnVlXFxhc3NpZ25tZW50c1xcdnVlXFxhc3NpZ25tZW50c1xcQXNzaWdubWVudHNUYWJsZS52dWU/ZWFiNWNhNTYiLCJ2dWVcXGFzc2lnbm1lbnRzXFx2dWVcXGFzc2lnbm1lbnRzXFxhc3NpZ25tZW50cy5qcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtBQ0FBOztBQ0FBO0FBQ0E7O0FDREE7QUFDQTtBQUNBO0FBQ0E7O0FDSEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDcEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDSkE7QUFDQTs7QUNEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ25CQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BO0FBQ0E7QUFDQTtBQUNBOztBQ0hBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQzVEQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNOQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ1BBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0pBO0FBQ0E7QUFDQTs7QUNGQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaENBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ2ZBOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDaEJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ05BOztBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDUEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQ0xBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNMQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDTEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FDWEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUNKQTtBQUNBO0FBQ0E7QUFDQTs7QUNIQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7Ozs7Ozs7Ozs7O0FDbkhBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFIQTtBQUtBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFOQTtBQVFBO0FBQUE7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUFBOztBQUFBOztBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFoREE7QUE5QkE7Ozs7O0FBN0JBO0FBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ1VBO0FBQ0E7QUFDQTtBQUNBO0FBQUE7QUFBQTtBQUZBO0FBSUE7QUFDQTtBQUNBO0FBQUE7QUFBQTtBQUZBO0FBSUE7QUFDQTtBQUNBO0FBQUE7QUFBQTtBQUZBO0FBVEE7QUFjQTtBQUNBO0FBQ0E7QUFEQTtBQUdBOztBQUNBO0FBRUE7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFDQTtBQUVBO0FBQ0E7QUFBQTtBQUFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFJQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQWxDQTtBQW9DQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFFQTtBQURBO0FBR0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBWkE7O0FBZUE7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBekdBOzs7OztBQVRBO0FBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ0FFOzs7O0FBQ0Y7Ozs7QUFDQTs7OztBQUNBOzs7O0FBQ0M7Ozs7OztBQUVELGNBQUksR0FBSjs7QUFFQSxjQUFJLFNBQUosQ0FBYyxXQUFkO0FBQ0MsY0FBSSxHQUFKO0FBQ0QsY0FBSSxTQUFKLENBQWMsbUJBQWQiLCJmaWxlIjoiZ2VuZXJhdGVkLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXNDb250ZW50IjpbIihmdW5jdGlvbiBlKHQsbixyKXtmdW5jdGlvbiBzKG8sdSl7aWYoIW5bb10pe2lmKCF0W29dKXt2YXIgYT10eXBlb2YgcmVxdWlyZT09XCJmdW5jdGlvblwiJiZyZXF1aXJlO2lmKCF1JiZhKXJldHVybiBhKG8sITApO2lmKGkpcmV0dXJuIGkobywhMCk7dmFyIGY9bmV3IEVycm9yKFwiQ2Fubm90IGZpbmQgbW9kdWxlICdcIitvK1wiJ1wiKTt0aHJvdyBmLmNvZGU9XCJNT0RVTEVfTk9UX0ZPVU5EXCIsZn12YXIgbD1uW29dPXtleHBvcnRzOnt9fTt0W29dWzBdLmNhbGwobC5leHBvcnRzLGZ1bmN0aW9uKGUpe3ZhciBuPXRbb11bMV1bZV07cmV0dXJuIHMobj9uOmUpfSxsLGwuZXhwb3J0cyxlLHQsbixyKX1yZXR1cm4gbltvXS5leHBvcnRzfXZhciBpPXR5cGVvZiByZXF1aXJlPT1cImZ1bmN0aW9uXCImJnJlcXVpcmU7Zm9yKHZhciBvPTA7bzxyLmxlbmd0aDtvKyspcyhyW29dKTtyZXR1cm4gc30pIiwibW9kdWxlLmV4cG9ydHMgPSB7IFwiZGVmYXVsdFwiOiByZXF1aXJlKFwiY29yZS1qcy9saWJyYXJ5L2ZuL29iamVjdC9hc3NpZ25cIiksIF9fZXNNb2R1bGU6IHRydWUgfTsiLCJyZXF1aXJlKCcuLi8uLi9tb2R1bGVzL2VzNi5vYmplY3QuYXNzaWduJyk7XG5tb2R1bGUuZXhwb3J0cyA9IHJlcXVpcmUoJy4uLy4uL21vZHVsZXMvX2NvcmUnKS5PYmplY3QuYXNzaWduOyIsIm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICBpZih0eXBlb2YgaXQgIT0gJ2Z1bmN0aW9uJyl0aHJvdyBUeXBlRXJyb3IoaXQgKyAnIGlzIG5vdCBhIGZ1bmN0aW9uIScpO1xuICByZXR1cm4gaXQ7XG59OyIsInZhciBpc09iamVjdCA9IHJlcXVpcmUoJy4vX2lzLW9iamVjdCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIGlmKCFpc09iamVjdChpdCkpdGhyb3cgVHlwZUVycm9yKGl0ICsgJyBpcyBub3QgYW4gb2JqZWN0IScpO1xuICByZXR1cm4gaXQ7XG59OyIsIi8vIGZhbHNlIC0+IEFycmF5I2luZGV4T2Zcbi8vIHRydWUgIC0+IEFycmF5I2luY2x1ZGVzXG52YXIgdG9JT2JqZWN0ID0gcmVxdWlyZSgnLi9fdG8taW9iamVjdCcpXG4gICwgdG9MZW5ndGggID0gcmVxdWlyZSgnLi9fdG8tbGVuZ3RoJylcbiAgLCB0b0luZGV4ICAgPSByZXF1aXJlKCcuL190by1pbmRleCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihJU19JTkNMVURFUyl7XG4gIHJldHVybiBmdW5jdGlvbigkdGhpcywgZWwsIGZyb21JbmRleCl7XG4gICAgdmFyIE8gICAgICA9IHRvSU9iamVjdCgkdGhpcylcbiAgICAgICwgbGVuZ3RoID0gdG9MZW5ndGgoTy5sZW5ndGgpXG4gICAgICAsIGluZGV4ICA9IHRvSW5kZXgoZnJvbUluZGV4LCBsZW5ndGgpXG4gICAgICAsIHZhbHVlO1xuICAgIC8vIEFycmF5I2luY2x1ZGVzIHVzZXMgU2FtZVZhbHVlWmVybyBlcXVhbGl0eSBhbGdvcml0aG1cbiAgICBpZihJU19JTkNMVURFUyAmJiBlbCAhPSBlbCl3aGlsZShsZW5ndGggPiBpbmRleCl7XG4gICAgICB2YWx1ZSA9IE9baW5kZXgrK107XG4gICAgICBpZih2YWx1ZSAhPSB2YWx1ZSlyZXR1cm4gdHJ1ZTtcbiAgICAvLyBBcnJheSN0b0luZGV4IGlnbm9yZXMgaG9sZXMsIEFycmF5I2luY2x1ZGVzIC0gbm90XG4gICAgfSBlbHNlIGZvcig7bGVuZ3RoID4gaW5kZXg7IGluZGV4KyspaWYoSVNfSU5DTFVERVMgfHwgaW5kZXggaW4gTyl7XG4gICAgICBpZihPW2luZGV4XSA9PT0gZWwpcmV0dXJuIElTX0lOQ0xVREVTIHx8IGluZGV4IHx8IDA7XG4gICAgfSByZXR1cm4gIUlTX0lOQ0xVREVTICYmIC0xO1xuICB9O1xufTsiLCJ2YXIgdG9TdHJpbmcgPSB7fS50b1N0cmluZztcblxubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiB0b1N0cmluZy5jYWxsKGl0KS5zbGljZSg4LCAtMSk7XG59OyIsInZhciBjb3JlID0gbW9kdWxlLmV4cG9ydHMgPSB7dmVyc2lvbjogJzIuNC4wJ307XG5pZih0eXBlb2YgX19lID09ICdudW1iZXInKV9fZSA9IGNvcmU7IC8vIGVzbGludC1kaXNhYmxlLWxpbmUgbm8tdW5kZWYiLCIvLyBvcHRpb25hbCAvIHNpbXBsZSBjb250ZXh0IGJpbmRpbmdcbnZhciBhRnVuY3Rpb24gPSByZXF1aXJlKCcuL19hLWZ1bmN0aW9uJyk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGZuLCB0aGF0LCBsZW5ndGgpe1xuICBhRnVuY3Rpb24oZm4pO1xuICBpZih0aGF0ID09PSB1bmRlZmluZWQpcmV0dXJuIGZuO1xuICBzd2l0Y2gobGVuZ3RoKXtcbiAgICBjYXNlIDE6IHJldHVybiBmdW5jdGlvbihhKXtcbiAgICAgIHJldHVybiBmbi5jYWxsKHRoYXQsIGEpO1xuICAgIH07XG4gICAgY2FzZSAyOiByZXR1cm4gZnVuY3Rpb24oYSwgYil7XG4gICAgICByZXR1cm4gZm4uY2FsbCh0aGF0LCBhLCBiKTtcbiAgICB9O1xuICAgIGNhc2UgMzogcmV0dXJuIGZ1bmN0aW9uKGEsIGIsIGMpe1xuICAgICAgcmV0dXJuIGZuLmNhbGwodGhhdCwgYSwgYiwgYyk7XG4gICAgfTtcbiAgfVxuICByZXR1cm4gZnVuY3Rpb24oLyogLi4uYXJncyAqLyl7XG4gICAgcmV0dXJuIGZuLmFwcGx5KHRoYXQsIGFyZ3VtZW50cyk7XG4gIH07XG59OyIsIi8vIDcuMi4xIFJlcXVpcmVPYmplY3RDb2VyY2libGUoYXJndW1lbnQpXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgaWYoaXQgPT0gdW5kZWZpbmVkKXRocm93IFR5cGVFcnJvcihcIkNhbid0IGNhbGwgbWV0aG9kIG9uICBcIiArIGl0KTtcbiAgcmV0dXJuIGl0O1xufTsiLCIvLyBUaGFuaydzIElFOCBmb3IgaGlzIGZ1bm55IGRlZmluZVByb3BlcnR5XG5tb2R1bGUuZXhwb3J0cyA9ICFyZXF1aXJlKCcuL19mYWlscycpKGZ1bmN0aW9uKCl7XG4gIHJldHVybiBPYmplY3QuZGVmaW5lUHJvcGVydHkoe30sICdhJywge2dldDogZnVuY3Rpb24oKXsgcmV0dXJuIDc7IH19KS5hICE9IDc7XG59KTsiLCJ2YXIgaXNPYmplY3QgPSByZXF1aXJlKCcuL19pcy1vYmplY3QnKVxuICAsIGRvY3VtZW50ID0gcmVxdWlyZSgnLi9fZ2xvYmFsJykuZG9jdW1lbnRcbiAgLy8gaW4gb2xkIElFIHR5cGVvZiBkb2N1bWVudC5jcmVhdGVFbGVtZW50IGlzICdvYmplY3QnXG4gICwgaXMgPSBpc09iamVjdChkb2N1bWVudCkgJiYgaXNPYmplY3QoZG9jdW1lbnQuY3JlYXRlRWxlbWVudCk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0KXtcbiAgcmV0dXJuIGlzID8gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChpdCkgOiB7fTtcbn07IiwiLy8gSUUgOC0gZG9uJ3QgZW51bSBidWcga2V5c1xubW9kdWxlLmV4cG9ydHMgPSAoXG4gICdjb25zdHJ1Y3RvcixoYXNPd25Qcm9wZXJ0eSxpc1Byb3RvdHlwZU9mLHByb3BlcnR5SXNFbnVtZXJhYmxlLHRvTG9jYWxlU3RyaW5nLHRvU3RyaW5nLHZhbHVlT2YnXG4pLnNwbGl0KCcsJyk7IiwidmFyIGdsb2JhbCAgICA9IHJlcXVpcmUoJy4vX2dsb2JhbCcpXG4gICwgY29yZSAgICAgID0gcmVxdWlyZSgnLi9fY29yZScpXG4gICwgY3R4ICAgICAgID0gcmVxdWlyZSgnLi9fY3R4JylcbiAgLCBoaWRlICAgICAgPSByZXF1aXJlKCcuL19oaWRlJylcbiAgLCBQUk9UT1RZUEUgPSAncHJvdG90eXBlJztcblxudmFyICRleHBvcnQgPSBmdW5jdGlvbih0eXBlLCBuYW1lLCBzb3VyY2Upe1xuICB2YXIgSVNfRk9SQ0VEID0gdHlwZSAmICRleHBvcnQuRlxuICAgICwgSVNfR0xPQkFMID0gdHlwZSAmICRleHBvcnQuR1xuICAgICwgSVNfU1RBVElDID0gdHlwZSAmICRleHBvcnQuU1xuICAgICwgSVNfUFJPVE8gID0gdHlwZSAmICRleHBvcnQuUFxuICAgICwgSVNfQklORCAgID0gdHlwZSAmICRleHBvcnQuQlxuICAgICwgSVNfV1JBUCAgID0gdHlwZSAmICRleHBvcnQuV1xuICAgICwgZXhwb3J0cyAgID0gSVNfR0xPQkFMID8gY29yZSA6IGNvcmVbbmFtZV0gfHwgKGNvcmVbbmFtZV0gPSB7fSlcbiAgICAsIGV4cFByb3RvICA9IGV4cG9ydHNbUFJPVE9UWVBFXVxuICAgICwgdGFyZ2V0ICAgID0gSVNfR0xPQkFMID8gZ2xvYmFsIDogSVNfU1RBVElDID8gZ2xvYmFsW25hbWVdIDogKGdsb2JhbFtuYW1lXSB8fCB7fSlbUFJPVE9UWVBFXVxuICAgICwga2V5LCBvd24sIG91dDtcbiAgaWYoSVNfR0xPQkFMKXNvdXJjZSA9IG5hbWU7XG4gIGZvcihrZXkgaW4gc291cmNlKXtcbiAgICAvLyBjb250YWlucyBpbiBuYXRpdmVcbiAgICBvd24gPSAhSVNfRk9SQ0VEICYmIHRhcmdldCAmJiB0YXJnZXRba2V5XSAhPT0gdW5kZWZpbmVkO1xuICAgIGlmKG93biAmJiBrZXkgaW4gZXhwb3J0cyljb250aW51ZTtcbiAgICAvLyBleHBvcnQgbmF0aXZlIG9yIHBhc3NlZFxuICAgIG91dCA9IG93biA/IHRhcmdldFtrZXldIDogc291cmNlW2tleV07XG4gICAgLy8gcHJldmVudCBnbG9iYWwgcG9sbHV0aW9uIGZvciBuYW1lc3BhY2VzXG4gICAgZXhwb3J0c1trZXldID0gSVNfR0xPQkFMICYmIHR5cGVvZiB0YXJnZXRba2V5XSAhPSAnZnVuY3Rpb24nID8gc291cmNlW2tleV1cbiAgICAvLyBiaW5kIHRpbWVycyB0byBnbG9iYWwgZm9yIGNhbGwgZnJvbSBleHBvcnQgY29udGV4dFxuICAgIDogSVNfQklORCAmJiBvd24gPyBjdHgob3V0LCBnbG9iYWwpXG4gICAgLy8gd3JhcCBnbG9iYWwgY29uc3RydWN0b3JzIGZvciBwcmV2ZW50IGNoYW5nZSB0aGVtIGluIGxpYnJhcnlcbiAgICA6IElTX1dSQVAgJiYgdGFyZ2V0W2tleV0gPT0gb3V0ID8gKGZ1bmN0aW9uKEMpe1xuICAgICAgdmFyIEYgPSBmdW5jdGlvbihhLCBiLCBjKXtcbiAgICAgICAgaWYodGhpcyBpbnN0YW5jZW9mIEMpe1xuICAgICAgICAgIHN3aXRjaChhcmd1bWVudHMubGVuZ3RoKXtcbiAgICAgICAgICAgIGNhc2UgMDogcmV0dXJuIG5ldyBDO1xuICAgICAgICAgICAgY2FzZSAxOiByZXR1cm4gbmV3IEMoYSk7XG4gICAgICAgICAgICBjYXNlIDI6IHJldHVybiBuZXcgQyhhLCBiKTtcbiAgICAgICAgICB9IHJldHVybiBuZXcgQyhhLCBiLCBjKTtcbiAgICAgICAgfSByZXR1cm4gQy5hcHBseSh0aGlzLCBhcmd1bWVudHMpO1xuICAgICAgfTtcbiAgICAgIEZbUFJPVE9UWVBFXSA9IENbUFJPVE9UWVBFXTtcbiAgICAgIHJldHVybiBGO1xuICAgIC8vIG1ha2Ugc3RhdGljIHZlcnNpb25zIGZvciBwcm90b3R5cGUgbWV0aG9kc1xuICAgIH0pKG91dCkgOiBJU19QUk9UTyAmJiB0eXBlb2Ygb3V0ID09ICdmdW5jdGlvbicgPyBjdHgoRnVuY3Rpb24uY2FsbCwgb3V0KSA6IG91dDtcbiAgICAvLyBleHBvcnQgcHJvdG8gbWV0aG9kcyB0byBjb3JlLiVDT05TVFJVQ1RPUiUubWV0aG9kcy4lTkFNRSVcbiAgICBpZihJU19QUk9UTyl7XG4gICAgICAoZXhwb3J0cy52aXJ0dWFsIHx8IChleHBvcnRzLnZpcnR1YWwgPSB7fSkpW2tleV0gPSBvdXQ7XG4gICAgICAvLyBleHBvcnQgcHJvdG8gbWV0aG9kcyB0byBjb3JlLiVDT05TVFJVQ1RPUiUucHJvdG90eXBlLiVOQU1FJVxuICAgICAgaWYodHlwZSAmICRleHBvcnQuUiAmJiBleHBQcm90byAmJiAhZXhwUHJvdG9ba2V5XSloaWRlKGV4cFByb3RvLCBrZXksIG91dCk7XG4gICAgfVxuICB9XG59O1xuLy8gdHlwZSBiaXRtYXBcbiRleHBvcnQuRiA9IDE7ICAgLy8gZm9yY2VkXG4kZXhwb3J0LkcgPSAyOyAgIC8vIGdsb2JhbFxuJGV4cG9ydC5TID0gNDsgICAvLyBzdGF0aWNcbiRleHBvcnQuUCA9IDg7ICAgLy8gcHJvdG9cbiRleHBvcnQuQiA9IDE2OyAgLy8gYmluZFxuJGV4cG9ydC5XID0gMzI7ICAvLyB3cmFwXG4kZXhwb3J0LlUgPSA2NDsgIC8vIHNhZmVcbiRleHBvcnQuUiA9IDEyODsgLy8gcmVhbCBwcm90byBtZXRob2QgZm9yIGBsaWJyYXJ5YCBcbm1vZHVsZS5leHBvcnRzID0gJGV4cG9ydDsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGV4ZWMpe1xuICB0cnkge1xuICAgIHJldHVybiAhIWV4ZWMoKTtcbiAgfSBjYXRjaChlKXtcbiAgICByZXR1cm4gdHJ1ZTtcbiAgfVxufTsiLCIvLyBodHRwczovL2dpdGh1Yi5jb20vemxvaXJvY2svY29yZS1qcy9pc3N1ZXMvODYjaXNzdWVjb21tZW50LTExNTc1OTAyOFxudmFyIGdsb2JhbCA9IG1vZHVsZS5leHBvcnRzID0gdHlwZW9mIHdpbmRvdyAhPSAndW5kZWZpbmVkJyAmJiB3aW5kb3cuTWF0aCA9PSBNYXRoXG4gID8gd2luZG93IDogdHlwZW9mIHNlbGYgIT0gJ3VuZGVmaW5lZCcgJiYgc2VsZi5NYXRoID09IE1hdGggPyBzZWxmIDogRnVuY3Rpb24oJ3JldHVybiB0aGlzJykoKTtcbmlmKHR5cGVvZiBfX2cgPT0gJ251bWJlcicpX19nID0gZ2xvYmFsOyAvLyBlc2xpbnQtZGlzYWJsZS1saW5lIG5vLXVuZGVmIiwidmFyIGhhc093blByb3BlcnR5ID0ge30uaGFzT3duUHJvcGVydHk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0LCBrZXkpe1xuICByZXR1cm4gaGFzT3duUHJvcGVydHkuY2FsbChpdCwga2V5KTtcbn07IiwidmFyIGRQICAgICAgICAgPSByZXF1aXJlKCcuL19vYmplY3QtZHAnKVxuICAsIGNyZWF0ZURlc2MgPSByZXF1aXJlKCcuL19wcm9wZXJ0eS1kZXNjJyk7XG5tb2R1bGUuZXhwb3J0cyA9IHJlcXVpcmUoJy4vX2Rlc2NyaXB0b3JzJykgPyBmdW5jdGlvbihvYmplY3QsIGtleSwgdmFsdWUpe1xuICByZXR1cm4gZFAuZihvYmplY3QsIGtleSwgY3JlYXRlRGVzYygxLCB2YWx1ZSkpO1xufSA6IGZ1bmN0aW9uKG9iamVjdCwga2V5LCB2YWx1ZSl7XG4gIG9iamVjdFtrZXldID0gdmFsdWU7XG4gIHJldHVybiBvYmplY3Q7XG59OyIsIm1vZHVsZS5leHBvcnRzID0gIXJlcXVpcmUoJy4vX2Rlc2NyaXB0b3JzJykgJiYgIXJlcXVpcmUoJy4vX2ZhaWxzJykoZnVuY3Rpb24oKXtcbiAgcmV0dXJuIE9iamVjdC5kZWZpbmVQcm9wZXJ0eShyZXF1aXJlKCcuL19kb20tY3JlYXRlJykoJ2RpdicpLCAnYScsIHtnZXQ6IGZ1bmN0aW9uKCl7IHJldHVybiA3OyB9fSkuYSAhPSA3O1xufSk7IiwiLy8gZmFsbGJhY2sgZm9yIG5vbi1hcnJheS1saWtlIEVTMyBhbmQgbm9uLWVudW1lcmFibGUgb2xkIFY4IHN0cmluZ3NcbnZhciBjb2YgPSByZXF1aXJlKCcuL19jb2YnKTtcbm1vZHVsZS5leHBvcnRzID0gT2JqZWN0KCd6JykucHJvcGVydHlJc0VudW1lcmFibGUoMCkgPyBPYmplY3QgOiBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBjb2YoaXQpID09ICdTdHJpbmcnID8gaXQuc3BsaXQoJycpIDogT2JqZWN0KGl0KTtcbn07IiwibW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiB0eXBlb2YgaXQgPT09ICdvYmplY3QnID8gaXQgIT09IG51bGwgOiB0eXBlb2YgaXQgPT09ICdmdW5jdGlvbic7XG59OyIsIid1c2Ugc3RyaWN0Jztcbi8vIDE5LjEuMi4xIE9iamVjdC5hc3NpZ24odGFyZ2V0LCBzb3VyY2UsIC4uLilcbnZhciBnZXRLZXlzICA9IHJlcXVpcmUoJy4vX29iamVjdC1rZXlzJylcbiAgLCBnT1BTICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1nb3BzJylcbiAgLCBwSUUgICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1waWUnKVxuICAsIHRvT2JqZWN0ID0gcmVxdWlyZSgnLi9fdG8tb2JqZWN0JylcbiAgLCBJT2JqZWN0ICA9IHJlcXVpcmUoJy4vX2lvYmplY3QnKVxuICAsICRhc3NpZ24gID0gT2JqZWN0LmFzc2lnbjtcblxuLy8gc2hvdWxkIHdvcmsgd2l0aCBzeW1ib2xzIGFuZCBzaG91bGQgaGF2ZSBkZXRlcm1pbmlzdGljIHByb3BlcnR5IG9yZGVyIChWOCBidWcpXG5tb2R1bGUuZXhwb3J0cyA9ICEkYXNzaWduIHx8IHJlcXVpcmUoJy4vX2ZhaWxzJykoZnVuY3Rpb24oKXtcbiAgdmFyIEEgPSB7fVxuICAgICwgQiA9IHt9XG4gICAgLCBTID0gU3ltYm9sKClcbiAgICAsIEsgPSAnYWJjZGVmZ2hpamtsbW5vcHFyc3QnO1xuICBBW1NdID0gNztcbiAgSy5zcGxpdCgnJykuZm9yRWFjaChmdW5jdGlvbihrKXsgQltrXSA9IGs7IH0pO1xuICByZXR1cm4gJGFzc2lnbih7fSwgQSlbU10gIT0gNyB8fCBPYmplY3Qua2V5cygkYXNzaWduKHt9LCBCKSkuam9pbignJykgIT0gSztcbn0pID8gZnVuY3Rpb24gYXNzaWduKHRhcmdldCwgc291cmNlKXsgLy8gZXNsaW50LWRpc2FibGUtbGluZSBuby11bnVzZWQtdmFyc1xuICB2YXIgVCAgICAgPSB0b09iamVjdCh0YXJnZXQpXG4gICAgLCBhTGVuICA9IGFyZ3VtZW50cy5sZW5ndGhcbiAgICAsIGluZGV4ID0gMVxuICAgICwgZ2V0U3ltYm9scyA9IGdPUFMuZlxuICAgICwgaXNFbnVtICAgICA9IHBJRS5mO1xuICB3aGlsZShhTGVuID4gaW5kZXgpe1xuICAgIHZhciBTICAgICAgPSBJT2JqZWN0KGFyZ3VtZW50c1tpbmRleCsrXSlcbiAgICAgICwga2V5cyAgID0gZ2V0U3ltYm9scyA/IGdldEtleXMoUykuY29uY2F0KGdldFN5bWJvbHMoUykpIDogZ2V0S2V5cyhTKVxuICAgICAgLCBsZW5ndGggPSBrZXlzLmxlbmd0aFxuICAgICAgLCBqICAgICAgPSAwXG4gICAgICAsIGtleTtcbiAgICB3aGlsZShsZW5ndGggPiBqKWlmKGlzRW51bS5jYWxsKFMsIGtleSA9IGtleXNbaisrXSkpVFtrZXldID0gU1trZXldO1xuICB9IHJldHVybiBUO1xufSA6ICRhc3NpZ247IiwidmFyIGFuT2JqZWN0ICAgICAgID0gcmVxdWlyZSgnLi9fYW4tb2JqZWN0JylcbiAgLCBJRThfRE9NX0RFRklORSA9IHJlcXVpcmUoJy4vX2llOC1kb20tZGVmaW5lJylcbiAgLCB0b1ByaW1pdGl2ZSAgICA9IHJlcXVpcmUoJy4vX3RvLXByaW1pdGl2ZScpXG4gICwgZFAgICAgICAgICAgICAgPSBPYmplY3QuZGVmaW5lUHJvcGVydHk7XG5cbmV4cG9ydHMuZiA9IHJlcXVpcmUoJy4vX2Rlc2NyaXB0b3JzJykgPyBPYmplY3QuZGVmaW5lUHJvcGVydHkgOiBmdW5jdGlvbiBkZWZpbmVQcm9wZXJ0eShPLCBQLCBBdHRyaWJ1dGVzKXtcbiAgYW5PYmplY3QoTyk7XG4gIFAgPSB0b1ByaW1pdGl2ZShQLCB0cnVlKTtcbiAgYW5PYmplY3QoQXR0cmlidXRlcyk7XG4gIGlmKElFOF9ET01fREVGSU5FKXRyeSB7XG4gICAgcmV0dXJuIGRQKE8sIFAsIEF0dHJpYnV0ZXMpO1xuICB9IGNhdGNoKGUpeyAvKiBlbXB0eSAqLyB9XG4gIGlmKCdnZXQnIGluIEF0dHJpYnV0ZXMgfHwgJ3NldCcgaW4gQXR0cmlidXRlcyl0aHJvdyBUeXBlRXJyb3IoJ0FjY2Vzc29ycyBub3Qgc3VwcG9ydGVkIScpO1xuICBpZigndmFsdWUnIGluIEF0dHJpYnV0ZXMpT1tQXSA9IEF0dHJpYnV0ZXMudmFsdWU7XG4gIHJldHVybiBPO1xufTsiLCJleHBvcnRzLmYgPSBPYmplY3QuZ2V0T3duUHJvcGVydHlTeW1ib2xzOyIsInZhciBoYXMgICAgICAgICAgPSByZXF1aXJlKCcuL19oYXMnKVxuICAsIHRvSU9iamVjdCAgICA9IHJlcXVpcmUoJy4vX3RvLWlvYmplY3QnKVxuICAsIGFycmF5SW5kZXhPZiA9IHJlcXVpcmUoJy4vX2FycmF5LWluY2x1ZGVzJykoZmFsc2UpXG4gICwgSUVfUFJPVE8gICAgID0gcmVxdWlyZSgnLi9fc2hhcmVkLWtleScpKCdJRV9QUk9UTycpO1xuXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKG9iamVjdCwgbmFtZXMpe1xuICB2YXIgTyAgICAgID0gdG9JT2JqZWN0KG9iamVjdClcbiAgICAsIGkgICAgICA9IDBcbiAgICAsIHJlc3VsdCA9IFtdXG4gICAgLCBrZXk7XG4gIGZvcihrZXkgaW4gTylpZihrZXkgIT0gSUVfUFJPVE8paGFzKE8sIGtleSkgJiYgcmVzdWx0LnB1c2goa2V5KTtcbiAgLy8gRG9uJ3QgZW51bSBidWcgJiBoaWRkZW4ga2V5c1xuICB3aGlsZShuYW1lcy5sZW5ndGggPiBpKWlmKGhhcyhPLCBrZXkgPSBuYW1lc1tpKytdKSl7XG4gICAgfmFycmF5SW5kZXhPZihyZXN1bHQsIGtleSkgfHwgcmVzdWx0LnB1c2goa2V5KTtcbiAgfVxuICByZXR1cm4gcmVzdWx0O1xufTsiLCIvLyAxOS4xLjIuMTQgLyAxNS4yLjMuMTQgT2JqZWN0LmtleXMoTylcbnZhciAka2V5cyAgICAgICA9IHJlcXVpcmUoJy4vX29iamVjdC1rZXlzLWludGVybmFsJylcbiAgLCBlbnVtQnVnS2V5cyA9IHJlcXVpcmUoJy4vX2VudW0tYnVnLWtleXMnKTtcblxubW9kdWxlLmV4cG9ydHMgPSBPYmplY3Qua2V5cyB8fCBmdW5jdGlvbiBrZXlzKE8pe1xuICByZXR1cm4gJGtleXMoTywgZW51bUJ1Z0tleXMpO1xufTsiLCJleHBvcnRzLmYgPSB7fS5wcm9wZXJ0eUlzRW51bWVyYWJsZTsiLCJtb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGJpdG1hcCwgdmFsdWUpe1xuICByZXR1cm4ge1xuICAgIGVudW1lcmFibGUgIDogIShiaXRtYXAgJiAxKSxcbiAgICBjb25maWd1cmFibGU6ICEoYml0bWFwICYgMiksXG4gICAgd3JpdGFibGUgICAgOiAhKGJpdG1hcCAmIDQpLFxuICAgIHZhbHVlICAgICAgIDogdmFsdWVcbiAgfTtcbn07IiwidmFyIHNoYXJlZCA9IHJlcXVpcmUoJy4vX3NoYXJlZCcpKCdrZXlzJylcbiAgLCB1aWQgICAgPSByZXF1aXJlKCcuL191aWQnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oa2V5KXtcbiAgcmV0dXJuIHNoYXJlZFtrZXldIHx8IChzaGFyZWRba2V5XSA9IHVpZChrZXkpKTtcbn07IiwidmFyIGdsb2JhbCA9IHJlcXVpcmUoJy4vX2dsb2JhbCcpXG4gICwgU0hBUkVEID0gJ19fY29yZS1qc19zaGFyZWRfXydcbiAgLCBzdG9yZSAgPSBnbG9iYWxbU0hBUkVEXSB8fCAoZ2xvYmFsW1NIQVJFRF0gPSB7fSk7XG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGtleSl7XG4gIHJldHVybiBzdG9yZVtrZXldIHx8IChzdG9yZVtrZXldID0ge30pO1xufTsiLCJ2YXIgdG9JbnRlZ2VyID0gcmVxdWlyZSgnLi9fdG8taW50ZWdlcicpXG4gICwgbWF4ICAgICAgID0gTWF0aC5tYXhcbiAgLCBtaW4gICAgICAgPSBNYXRoLm1pbjtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaW5kZXgsIGxlbmd0aCl7XG4gIGluZGV4ID0gdG9JbnRlZ2VyKGluZGV4KTtcbiAgcmV0dXJuIGluZGV4IDwgMCA/IG1heChpbmRleCArIGxlbmd0aCwgMCkgOiBtaW4oaW5kZXgsIGxlbmd0aCk7XG59OyIsIi8vIDcuMS40IFRvSW50ZWdlclxudmFyIGNlaWwgID0gTWF0aC5jZWlsXG4gICwgZmxvb3IgPSBNYXRoLmZsb29yO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBpc05hTihpdCA9ICtpdCkgPyAwIDogKGl0ID4gMCA/IGZsb29yIDogY2VpbCkoaXQpO1xufTsiLCIvLyB0byBpbmRleGVkIG9iamVjdCwgdG9PYmplY3Qgd2l0aCBmYWxsYmFjayBmb3Igbm9uLWFycmF5LWxpa2UgRVMzIHN0cmluZ3NcbnZhciBJT2JqZWN0ID0gcmVxdWlyZSgnLi9faW9iamVjdCcpXG4gICwgZGVmaW5lZCA9IHJlcXVpcmUoJy4vX2RlZmluZWQnKTtcbm1vZHVsZS5leHBvcnRzID0gZnVuY3Rpb24oaXQpe1xuICByZXR1cm4gSU9iamVjdChkZWZpbmVkKGl0KSk7XG59OyIsIi8vIDcuMS4xNSBUb0xlbmd0aFxudmFyIHRvSW50ZWdlciA9IHJlcXVpcmUoJy4vX3RvLWludGVnZXInKVxuICAsIG1pbiAgICAgICA9IE1hdGgubWluO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBpdCA+IDAgPyBtaW4odG9JbnRlZ2VyKGl0KSwgMHgxZmZmZmZmZmZmZmZmZikgOiAwOyAvLyBwb3coMiwgNTMpIC0gMSA9PSA5MDA3MTk5MjU0NzQwOTkxXG59OyIsIi8vIDcuMS4xMyBUb09iamVjdChhcmd1bWVudClcbnZhciBkZWZpbmVkID0gcmVxdWlyZSgnLi9fZGVmaW5lZCcpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihpdCl7XG4gIHJldHVybiBPYmplY3QoZGVmaW5lZChpdCkpO1xufTsiLCIvLyA3LjEuMSBUb1ByaW1pdGl2ZShpbnB1dCBbLCBQcmVmZXJyZWRUeXBlXSlcbnZhciBpc09iamVjdCA9IHJlcXVpcmUoJy4vX2lzLW9iamVjdCcpO1xuLy8gaW5zdGVhZCBvZiB0aGUgRVM2IHNwZWMgdmVyc2lvbiwgd2UgZGlkbid0IGltcGxlbWVudCBAQHRvUHJpbWl0aXZlIGNhc2Vcbi8vIGFuZCB0aGUgc2Vjb25kIGFyZ3VtZW50IC0gZmxhZyAtIHByZWZlcnJlZCB0eXBlIGlzIGEgc3RyaW5nXG5tb2R1bGUuZXhwb3J0cyA9IGZ1bmN0aW9uKGl0LCBTKXtcbiAgaWYoIWlzT2JqZWN0KGl0KSlyZXR1cm4gaXQ7XG4gIHZhciBmbiwgdmFsO1xuICBpZihTICYmIHR5cGVvZiAoZm4gPSBpdC50b1N0cmluZykgPT0gJ2Z1bmN0aW9uJyAmJiAhaXNPYmplY3QodmFsID0gZm4uY2FsbChpdCkpKXJldHVybiB2YWw7XG4gIGlmKHR5cGVvZiAoZm4gPSBpdC52YWx1ZU9mKSA9PSAnZnVuY3Rpb24nICYmICFpc09iamVjdCh2YWwgPSBmbi5jYWxsKGl0KSkpcmV0dXJuIHZhbDtcbiAgaWYoIVMgJiYgdHlwZW9mIChmbiA9IGl0LnRvU3RyaW5nKSA9PSAnZnVuY3Rpb24nICYmICFpc09iamVjdCh2YWwgPSBmbi5jYWxsKGl0KSkpcmV0dXJuIHZhbDtcbiAgdGhyb3cgVHlwZUVycm9yKFwiQ2FuJ3QgY29udmVydCBvYmplY3QgdG8gcHJpbWl0aXZlIHZhbHVlXCIpO1xufTsiLCJ2YXIgaWQgPSAwXG4gICwgcHggPSBNYXRoLnJhbmRvbSgpO1xubW9kdWxlLmV4cG9ydHMgPSBmdW5jdGlvbihrZXkpe1xuICByZXR1cm4gJ1N5bWJvbCgnLmNvbmNhdChrZXkgPT09IHVuZGVmaW5lZCA/ICcnIDoga2V5LCAnKV8nLCAoKytpZCArIHB4KS50b1N0cmluZygzNikpO1xufTsiLCIvLyAxOS4xLjMuMSBPYmplY3QuYXNzaWduKHRhcmdldCwgc291cmNlKVxudmFyICRleHBvcnQgPSByZXF1aXJlKCcuL19leHBvcnQnKTtcblxuJGV4cG9ydCgkZXhwb3J0LlMgKyAkZXhwb3J0LkYsICdPYmplY3QnLCB7YXNzaWduOiByZXF1aXJlKCcuL19vYmplY3QtYXNzaWduJyl9KTsiLCJ2YXIgVnVlIC8vIGxhdGUgYmluZFxudmFyIHZlcnNpb25cbnZhciBtYXAgPSB3aW5kb3cuX19WVUVfSE9UX01BUF9fID0gT2JqZWN0LmNyZWF0ZShudWxsKVxudmFyIGluc3RhbGxlZCA9IGZhbHNlXG52YXIgaXNCcm93c2VyaWZ5ID0gZmFsc2VcbnZhciBpbml0SG9va05hbWUgPSAnYmVmb3JlQ3JlYXRlJ1xuXG5leHBvcnRzLmluc3RhbGwgPSBmdW5jdGlvbiAodnVlLCBicm93c2VyaWZ5KSB7XG4gIGlmIChpbnN0YWxsZWQpIHJldHVyblxuICBpbnN0YWxsZWQgPSB0cnVlXG5cbiAgVnVlID0gdnVlLl9fZXNNb2R1bGUgPyB2dWUuZGVmYXVsdCA6IHZ1ZVxuICB2ZXJzaW9uID0gVnVlLnZlcnNpb24uc3BsaXQoJy4nKS5tYXAoTnVtYmVyKVxuICBpc0Jyb3dzZXJpZnkgPSBicm93c2VyaWZ5XG5cbiAgLy8gY29tcGF0IHdpdGggPCAyLjAuMC1hbHBoYS43XG4gIGlmIChWdWUuY29uZmlnLl9saWZlY3ljbGVIb29rcy5pbmRleE9mKCdpbml0JykgPiAtMSkge1xuICAgIGluaXRIb29rTmFtZSA9ICdpbml0J1xuICB9XG5cbiAgZXhwb3J0cy5jb21wYXRpYmxlID0gdmVyc2lvblswXSA+PSAyXG4gIGlmICghZXhwb3J0cy5jb21wYXRpYmxlKSB7XG4gICAgY29uc29sZS53YXJuKFxuICAgICAgJ1tITVJdIFlvdSBhcmUgdXNpbmcgYSB2ZXJzaW9uIG9mIHZ1ZS1ob3QtcmVsb2FkLWFwaSB0aGF0IGlzICcgK1xuICAgICAgJ29ubHkgY29tcGF0aWJsZSB3aXRoIFZ1ZS5qcyBjb3JlIF4yLjAuMC4nXG4gICAgKVxuICAgIHJldHVyblxuICB9XG59XG5cbi8qKlxuICogQ3JlYXRlIGEgcmVjb3JkIGZvciBhIGhvdCBtb2R1bGUsIHdoaWNoIGtlZXBzIHRyYWNrIG9mIGl0cyBjb25zdHJ1Y3RvclxuICogYW5kIGluc3RhbmNlc1xuICpcbiAqIEBwYXJhbSB7U3RyaW5nfSBpZFxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqL1xuXG5leHBvcnRzLmNyZWF0ZVJlY29yZCA9IGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICB2YXIgQ3RvciA9IG51bGxcbiAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgQ3RvciA9IG9wdGlvbnNcbiAgICBvcHRpb25zID0gQ3Rvci5vcHRpb25zXG4gIH1cbiAgbWFrZU9wdGlvbnNIb3QoaWQsIG9wdGlvbnMpXG4gIG1hcFtpZF0gPSB7XG4gICAgQ3RvcjogVnVlLmV4dGVuZChvcHRpb25zKSxcbiAgICBpbnN0YW5jZXM6IFtdXG4gIH1cbn1cblxuLyoqXG4gKiBNYWtlIGEgQ29tcG9uZW50IG9wdGlvbnMgb2JqZWN0IGhvdC5cbiAqXG4gKiBAcGFyYW0ge1N0cmluZ30gaWRcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKi9cblxuZnVuY3Rpb24gbWFrZU9wdGlvbnNIb3QgKGlkLCBvcHRpb25zKSB7XG4gIGluamVjdEhvb2sob3B0aW9ucywgaW5pdEhvb2tOYW1lLCBmdW5jdGlvbiAoKSB7XG4gICAgbWFwW2lkXS5pbnN0YW5jZXMucHVzaCh0aGlzKVxuICB9KVxuICBpbmplY3RIb29rKG9wdGlvbnMsICdiZWZvcmVEZXN0cm95JywgZnVuY3Rpb24gKCkge1xuICAgIHZhciBpbnN0YW5jZXMgPSBtYXBbaWRdLmluc3RhbmNlc1xuICAgIGluc3RhbmNlcy5zcGxpY2UoaW5zdGFuY2VzLmluZGV4T2YodGhpcyksIDEpXG4gIH0pXG59XG5cbi8qKlxuICogSW5qZWN0IGEgaG9vayB0byBhIGhvdCByZWxvYWRhYmxlIGNvbXBvbmVudCBzbyB0aGF0XG4gKiB3ZSBjYW4ga2VlcCB0cmFjayBvZiBpdC5cbiAqXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICogQHBhcmFtIHtTdHJpbmd9IG5hbWVcbiAqIEBwYXJhbSB7RnVuY3Rpb259IGhvb2tcbiAqL1xuXG5mdW5jdGlvbiBpbmplY3RIb29rIChvcHRpb25zLCBuYW1lLCBob29rKSB7XG4gIHZhciBleGlzdGluZyA9IG9wdGlvbnNbbmFtZV1cbiAgb3B0aW9uc1tuYW1lXSA9IGV4aXN0aW5nXG4gICAgPyBBcnJheS5pc0FycmF5KGV4aXN0aW5nKVxuICAgICAgPyBleGlzdGluZy5jb25jYXQoaG9vaylcbiAgICAgIDogW2V4aXN0aW5nLCBob29rXVxuICAgIDogW2hvb2tdXG59XG5cbmZ1bmN0aW9uIHRyeVdyYXAgKGZuKSB7XG4gIHJldHVybiBmdW5jdGlvbiAoaWQsIGFyZykge1xuICAgIHRyeSB7IGZuKGlkLCBhcmcpIH0gY2F0Y2ggKGUpIHtcbiAgICAgIGNvbnNvbGUuZXJyb3IoZSlcbiAgICAgIGNvbnNvbGUud2FybignU29tZXRoaW5nIHdlbnQgd3JvbmcgZHVyaW5nIFZ1ZSBjb21wb25lbnQgaG90LXJlbG9hZC4gRnVsbCByZWxvYWQgcmVxdWlyZWQuJylcbiAgICB9XG4gIH1cbn1cblxuZXhwb3J0cy5yZXJlbmRlciA9IHRyeVdyYXAoZnVuY3Rpb24gKGlkLCBvcHRpb25zKSB7XG4gIHZhciByZWNvcmQgPSBtYXBbaWRdXG4gIGlmICghb3B0aW9ucykge1xuICAgIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgICAgaW5zdGFuY2UuJGZvcmNlVXBkYXRlKClcbiAgICB9KVxuICAgIHJldHVyblxuICB9XG4gIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIG9wdGlvbnMgPSBvcHRpb25zLm9wdGlvbnNcbiAgfVxuICByZWNvcmQuQ3Rvci5vcHRpb25zLnJlbmRlciA9IG9wdGlvbnMucmVuZGVyXG4gIHJlY29yZC5DdG9yLm9wdGlvbnMuc3RhdGljUmVuZGVyRm5zID0gb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnNcbiAgcmVjb3JkLmluc3RhbmNlcy5zbGljZSgpLmZvckVhY2goZnVuY3Rpb24gKGluc3RhbmNlKSB7XG4gICAgaW5zdGFuY2UuJG9wdGlvbnMucmVuZGVyID0gb3B0aW9ucy5yZW5kZXJcbiAgICBpbnN0YW5jZS4kb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnMgPSBvcHRpb25zLnN0YXRpY1JlbmRlckZuc1xuICAgIGluc3RhbmNlLl9zdGF0aWNUcmVlcyA9IFtdIC8vIHJlc2V0IHN0YXRpYyB0cmVlc1xuICAgIGluc3RhbmNlLiRmb3JjZVVwZGF0ZSgpXG4gIH0pXG59KVxuXG5leHBvcnRzLnJlbG9hZCA9IHRyeVdyYXAoZnVuY3Rpb24gKGlkLCBvcHRpb25zKSB7XG4gIHZhciByZWNvcmQgPSBtYXBbaWRdXG4gIGlmIChvcHRpb25zKSB7XG4gICAgaWYgKHR5cGVvZiBvcHRpb25zID09PSAnZnVuY3Rpb24nKSB7XG4gICAgICBvcHRpb25zID0gb3B0aW9ucy5vcHRpb25zXG4gICAgfVxuICAgIG1ha2VPcHRpb25zSG90KGlkLCBvcHRpb25zKVxuICAgIGlmICh2ZXJzaW9uWzFdIDwgMikge1xuICAgICAgLy8gcHJlc2VydmUgcHJlIDIuMiBiZWhhdmlvciBmb3IgZ2xvYmFsIG1peGluIGhhbmRsaW5nXG4gICAgICByZWNvcmQuQ3Rvci5leHRlbmRPcHRpb25zID0gb3B0aW9uc1xuICAgIH1cbiAgICB2YXIgbmV3Q3RvciA9IHJlY29yZC5DdG9yLnN1cGVyLmV4dGVuZChvcHRpb25zKVxuICAgIHJlY29yZC5DdG9yLm9wdGlvbnMgPSBuZXdDdG9yLm9wdGlvbnNcbiAgICByZWNvcmQuQ3Rvci5jaWQgPSBuZXdDdG9yLmNpZFxuICAgIHJlY29yZC5DdG9yLnByb3RvdHlwZSA9IG5ld0N0b3IucHJvdG90eXBlXG4gICAgaWYgKG5ld0N0b3IucmVsZWFzZSkge1xuICAgICAgLy8gdGVtcG9yYXJ5IGdsb2JhbCBtaXhpbiBzdHJhdGVneSB1c2VkIGluIDwgMi4wLjAtYWxwaGEuNlxuICAgICAgbmV3Q3Rvci5yZWxlYXNlKClcbiAgICB9XG4gIH1cbiAgcmVjb3JkLmluc3RhbmNlcy5zbGljZSgpLmZvckVhY2goZnVuY3Rpb24gKGluc3RhbmNlKSB7XG4gICAgaWYgKGluc3RhbmNlLiR2bm9kZSAmJiBpbnN0YW5jZS4kdm5vZGUuY29udGV4dCkge1xuICAgICAgaW5zdGFuY2UuJHZub2RlLmNvbnRleHQuJGZvcmNlVXBkYXRlKClcbiAgICB9IGVsc2Uge1xuICAgICAgY29uc29sZS53YXJuKCdSb290IG9yIG1hbnVhbGx5IG1vdW50ZWQgaW5zdGFuY2UgbW9kaWZpZWQuIEZ1bGwgcmVsb2FkIHJlcXVpcmVkLicpXG4gICAgfVxuICB9KVxufSlcbiIsIu+7vzx0ZW1wbGF0ZT5cclxuICAgIDxkaXYgY2xhc3M9XCJjb21iby1ib3hcIj5cclxuICAgICAgICA8ZGl2IGNsYXNzPVwiYnRuLWdyb3VwIGJ0bi1pbnB1dCBjbGVhcmZpeFwiPlxyXG4gICAgICAgICAgICA8YnV0dG9uIHR5cGU9XCJidXR0b25cIiBjbGFzcz1cImJ0biBkcm9wZG93bi10b2dnbGVcIiBkYXRhLXRvZ2dsZT1cImRyb3Bkb3duXCI+XHJcbiAgICAgICAgICAgICAgICA8c3BhbiBkYXRhLWJpbmQ9XCJsYWJlbFwiIHYtaWY9XCJ2YWx1ZSA9PT0gbnVsbFwiIGNsYXNzPVwiZ3JheS10ZXh0XCI+e3twbGFjZWhvbGRlclRleHR9fTwvc3Bhbj5cclxuICAgICAgICAgICAgICAgIDxzcGFuIGRhdGEtYmluZD1cImxhYmVsXCIgdi1lbHNlPnt7dmFsdWUudmFsdWV9fTwvc3Bhbj5cclxuICAgICAgICAgICAgPC9idXR0b24+XHJcbiAgICAgICAgICAgIDx1bCByZWY9XCJkcm9wZG93bk1lbnVcIiBjbGFzcz1cImRyb3Bkb3duLW1lbnVcIiByb2xlPVwibWVudVwiPlxyXG4gICAgICAgICAgICAgICAgPGxpPlxyXG4gICAgICAgICAgICAgICAgICAgIDxpbnB1dCB0eXBlPVwidGV4dFwiIHJlZj1cInNlYXJjaEJveFwiIDppZD1cImlucHV0SWRcIiBwbGFjZWhvbGRlcj1cIlNlYXJjaFwiIEBpbnB1dD1cInVwZGF0ZU9wdGlvbnNMaXN0XCIgdi1vbjprZXl1cC5kb3duPVwib25TZWFyY2hCb3hEb3duS2V5XCIgdi1tb2RlbD1cInNlYXJjaFRlcm1cIiAvPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWZvcj1cIm9wdGlvbiBpbiBvcHRpb25zXCIgOmtleT1cIm9wdGlvbi5rZXlcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YSBocmVmPVwiamF2YXNjcmlwdDp2b2lkKDApO1wiIHYtb246Y2xpY2s9XCJzZWxlY3RPcHRpb24ob3B0aW9uKVwiIHYtaHRtbD1cImhpZ2hsaWdodChvcHRpb24udmFsdWUsIHNlYXJjaFRlcm0pXCIgdi1vbjprZXlkb3duLnVwPVwib25PcHRpb25VcEtleVwiPjwvYT5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1pZj1cImlzTG9hZGluZ1wiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhPkxvYWRpbmcuLi48L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtaWY9XCIhaXNMb2FkaW5nICYmIG9wdGlvbnMubGVuZ3RoID09PSAwXCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGE+Tm8gcmVzdWx0cyBmb3VuZDwvYT5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgIDwvdWw+XHJcbiAgICAgICAgPC9kaXY+XHJcbiAgICAgICAgPGJ1dHRvbiB2LWlmPVwidmFsdWUgIT09IG51bGxcIiBjbGFzcz1cImJ0biBidG4tbGluayBidG4tY2xlYXJcIiBAY2xpY2s9XCJjbGVhclwiPlxyXG4gICAgICAgICAgICA8c3Bhbj48L3NwYW4+XHJcbiAgICAgICAgPC9idXR0b24+XHJcbiAgICA8L2Rpdj5cclxuPC90ZW1wbGF0ZT5cclxuXHJcbjxzY3JpcHQ+XHJcbiAgICBtb2R1bGUuZXhwb3J0cyA9IHtcclxuICAgICAgICBuYW1lOiAndXNlci1zZWxlY3RvcicsXHJcbiAgICAgICAgcHJvcHM6IFsnZmV0Y2hVcmwnLCAnY29udHJvbElkJywgJ3ZhbHVlJywgJ3BsYWNlaG9sZGVyJywgJ2FqYXhQYXJhbXMnXSxcclxuICAgICAgICBkYXRhOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBvcHRpb25zOiBbXSxcclxuICAgICAgICAgICAgICAgIGlzTG9hZGluZzogZmFsc2UsXHJcbiAgICAgICAgICAgICAgICBzZWFyY2hUZXJtOiAnJ1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgY29tcHV0ZWQ6IHtcclxuICAgICAgICAgICAgaW5wdXRJZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIGBzYl8ke3RoaXMuY29udHJvbElkfWA7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHBsYWNlaG9sZGVyVGV4dDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRoaXMucGxhY2Vob2xkZXIgfHwgXCJTZWxlY3RcIjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgbW91bnRlZDogZnVuY3Rpb24oKSB7XHJcbiAgICAgICAgICAgIGNvbnN0IGpxRWwgPSAkKHRoaXMuJGVsKVxyXG4gICAgICAgICAgICBjb25zdCBmb2N1c1RvID0ganFFbC5maW5kKGAjJHt0aGlzLmlucHV0SWR9YClcclxuICAgICAgICAgICAganFFbC5vbignc2hvd24uYnMuZHJvcGRvd24nLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICBmb2N1c1RvLmZvY3VzKClcclxuICAgICAgICAgICAgICAgIHRoaXMuZmV0Y2hPcHRpb25zKHRoaXMuc2VhcmNoVGVybSlcclxuICAgICAgICAgICAgfSlcclxuXHJcbiAgICAgICAgICAgIGpxRWwub24oJ2hpZGRlbi5icy5kcm9wZG93bicsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VhcmNoVGVybSA9IFwiXCJcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICB9LFxyXG4gICAgICAgIG1ldGhvZHM6IHtcclxuICAgICAgICAgICAgb25TZWFyY2hCb3hEb3duS2V5KGV2ZW50KSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgJGZpcnN0T3B0aW9uQW5jaG9yID0gJCh0aGlzLiRyZWZzLmRyb3Bkb3duTWVudSkuZmluZCgnYScpLmZpcnN0KCk7XHJcbiAgICAgICAgICAgICAgICAkZmlyc3RPcHRpb25BbmNob3IuZm9jdXMoKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgb25PcHRpb25VcEtleShldmVudCkge1xyXG4gICAgICAgICAgICAgICAgdmFyIGlzRmlyc3RPcHRpb24gPSAkKGV2ZW50LnRhcmdldCkucGFyZW50KCkuaW5kZXgoKSA9PT0gMTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoaXNGaXJzdE9wdGlvbikge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuJHJlZnMuc2VhcmNoQm94LmZvY3VzKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnQuc3RvcFByb3BhZ2F0aW9uKCk7XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGZldGNoT3B0aW9uczogZnVuY3Rpb24gKGZpbHRlciA9IFwiXCIpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIHZhciByZXF1ZXN0UGFyYW1zID0gT2JqZWN0LmFzc2lnbih7IHF1ZXJ5OiBmaWx0ZXIsIGNhY2hlOiBmYWxzZSB9LCB0aGlzLmFqYXhQYXJhbXMpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kaHR0cC5nZXQodGhpcy5mZXRjaFVybCwge3BhcmFtczogcmVxdWVzdFBhcmFtc30pXHJcbiAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLm9wdGlvbnMgPSByZXNwb25zZS5ib2R5Lm9wdGlvbnMgfHwgW107XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfSwgcmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRpbmcgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGNsZWFyOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRlbWl0KCdzZWxlY3RlZCcsIG51bGwsIHRoaXMuY29udHJvbElkKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VhcmNoVGVybSA9IFwiXCI7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHNlbGVjdE9wdGlvbjogZnVuY3Rpb24gKHZhbHVlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLiRlbWl0KCdzZWxlY3RlZCcsIHZhbHVlLCB0aGlzLmNvbnRyb2xJZCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHVwZGF0ZU9wdGlvbnNMaXN0KGUpIHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZmV0Y2hPcHRpb25zKGUudGFyZ2V0LnZhbHVlKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgaGlnaGxpZ2h0OiBmdW5jdGlvbiAodGl0bGUsIHNlYXJjaFRlcm0pIHtcclxuICAgICAgICAgICAgICAgIHZhciBlbmNvZGVkVGl0bGUgPSBfLmVzY2FwZSh0aXRsZSk7XHJcbiAgICAgICAgICAgICAgICBpZiAoc2VhcmNoVGVybSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBzYWZlU2VhcmNoVGVybSA9IF8uZXNjYXBlKF8uZXNjYXBlUmVnRXhwKHNlYXJjaFRlcm0pKTtcclxuXHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGlRdWVyeSA9IG5ldyBSZWdFeHAoc2FmZVNlYXJjaFRlcm0sIFwiaWdcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGVuY29kZWRUaXRsZS5yZXBsYWNlKGlRdWVyeSwgKG1hdGNoZWRUeHQsIGEsIGIpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGA8c3Ryb25nPiR7bWF0Y2hlZFR4dH08L3N0cm9uZz5gO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIHJldHVybiBlbmNvZGVkVGl0bGU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9O1xyXG48L3NjcmlwdD4iLCLvu788dGVtcGxhdGU+XHJcbiAgPHRhYmxlIGNsYXNzPVwidGFibGUgdGFibGUtc3RyaXBlZCB0YWJsZS1vcmRlcmVkIHRhYmxlLWJvcmRlcmVkIHRhYmxlLWhvdmVyIHRhYmxlLXdpdGgtY2hlY2tib3hlcyB0YWJsZS13aXRoLXByZWZpbGxlZC1jb2x1bW4gdGFibGUtaW50ZXJ2aWV3cyByZXNwb25zaXZlXCI+XHJcbiAgICA8dGhlYWQ+XHJcbiAgICA8L3RoZWFkPlxyXG4gICAgPHRib2R5PjwvdGJvZHk+XHJcbiAgPC90YWJsZT5cclxuPC90ZW1wbGF0ZT5cclxuXHJcbjxzY3JpcHQ+XHJcbmV4cG9ydCBkZWZhdWx0IHtcclxuICBwcm9wczoge1xyXG4gICAgYWRkUGFyYW1zVG9SZXF1ZXN0OiB7XHJcbiAgICAgIHR5cGU6IEZ1bmN0aW9uLFxyXG4gICAgICBkZWZhdWx0OiAoZCkgPT4geyByZXR1cm4gZCB9XHJcbiAgICB9LFxyXG4gICAgcmVzcG9uc2VQcm9jZXNzb3I6IHtcclxuICAgICAgdHlwZTogRnVuY3Rpb24sXHJcbiAgICAgIGRlZmF1bHQ6IChyKSA9PiB7IHJldHVybiByIH1cclxuICAgIH0sXHJcbiAgICB0YWJsZU9wdGlvbnM6IHtcclxuICAgICAgdHlwZTogT2JqZWN0LFxyXG4gICAgICBkZWZhdWx0OiAoKSA9PiB7IHJldHVybiB7fSB9XHJcbiAgICB9XHJcbiAgfSxcclxuICBkYXRhKCkge1xyXG4gICAgcmV0dXJuIHtcclxuICAgICAgdGFibGU6IG51bGxcclxuICAgIH1cclxuICB9LFxyXG4gIGNvbXB1dGVkOiB7XHJcbiAgfSxcclxuICB3YXRjaDoge1xyXG4gIH0sXHJcbiAgbWV0aG9kczoge1xyXG4gICAgcmVsb2FkOiBmdW5jdGlvbiAoZGF0YSkge1xyXG4gICAgICB0aGlzLnRhYmxlLmFqYXguZGF0YSA9IGRhdGFcclxuICAgICAgdGhpcy50YWJsZS5yb3dzKCkuZGVzZWxlY3QoKTtcclxuICAgICAgdGhpcy50YWJsZS5hamF4LnJlbG9hZCgpXHJcbiAgICB9LFxyXG4gICAgb25UYWJsZUluaXRDb21wbGV0ZTogZnVuY3Rpb24gKCkge1xyXG4gICAgICB2YXIgc2VsZiA9IHRoaXM7XHJcbiAgICAgICQodGhpcy4kZWwpLnBhcmVudCgnLmRhdGFUYWJsZXNfd3JhcHBlcicpLmZpbmQoJy5kYXRhVGFibGVzX2ZpbHRlciBsYWJlbCcpLm9uKCdjbGljaycsIGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgaWYgKGUudGFyZ2V0ICE9PSB0aGlzKVxyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIGlmICgkKHRoaXMpLmhhc0NsYXNzKFwiYWN0aXZlXCIpKSB7XHJcbiAgICAgICAgICAkKHRoaXMpLnJlbW92ZUNsYXNzKFwiYWN0aXZlXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICQodGhpcykuYWRkQ2xhc3MoXCJhY3RpdmVcIik7XHJcbiAgICAgICAgICAkKHRoaXMpLmNoaWxkcmVuKFwiaW5wdXRbdHlwZT0nc2VhcmNoJ11cIikuZGVsYXkoMjAwKS5xdWV1ZShmdW5jdGlvbiAoKSB7ICQodGhpcykuZm9jdXMoKTsgJCh0aGlzKS5kZXF1ZXVlKCk7IH0pO1xyXG4gICAgICAgIH1cclxuICAgICAgfSk7XHJcbiAgICAgIHZhciBmaXJzdEhlYWRlciA9IHRoaXMudGFibGUuY29sdW1uKDApLmhlYWRlcigpO1xyXG4gICAgICAkKGZpcnN0SGVhZGVyKS5odG1sKCc8aW5wdXQgY2xhc3M9XCJkb3VibGUtY2hlY2tib3hcIiBpZD1cImNoZWNrLWFsbFwiIHR5cGU9XCJjaGVja2JveFwiPicgK1xyXG4gICAgICAgICc8bGFiZWwgZm9yPVwiY2hlY2stYWxsXCI+JyArXHJcbiAgICAgICAgJzxzcGFuIGNsYXNzPVwidGlja1wiPjwvc3Bhbj4nICtcclxuICAgICAgICAnPC9sYWJlbD4nKVxyXG4gICAgICAkKCcjY2hlY2stYWxsJykuY2hhbmdlKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICBpZiAoIXRoaXMuY2hlY2tlZCkge1xyXG4gICAgICAgICAgc2VsZi50YWJsZS5yb3dzKCkuZGVzZWxlY3QoKVxyXG4gICAgICAgICAgJChzZWxmLnRhYmxlLnJvd3MpLmZpbmQoXCIuY2hlY2tib3gtZmlsdGVyXCIpLnByb3AoJ2NoZWNrZWQnLCBmYWxzZSlcclxuICAgICAgICB9XHJcbiAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICBzZWxmLnRhYmxlLnJvd3MoKS5zZWxlY3QoKVxyXG4gICAgICAgICAgJChzZWxmLnRhYmxlLnJvd3MpLmZpbmQoXCIuY2hlY2tib3gtZmlsdGVyXCIpLnByb3AoJ2NoZWNrZWQnLCB0cnVlKVxyXG4gICAgICAgIH1cclxuICAgICAgfSk7XHJcbiAgICB9XHJcbiAgfSxcclxuICBtb3VudGVkKCkge1xyXG4gICAgY29uc3Qgc2VsZiA9IHRoaXNcclxuICAgIHZhciBvcHRpb25zID0gT2JqZWN0LmFzc2lnbih7XHJcbiAgICAgIHByb2Nlc3Npbmc6IHRydWUsXHJcbiAgICAgIHNlcnZlclNpZGU6IHRydWUsXHJcbiAgICAgIGxhbmd1YWdlOlxyXG4gICAgICB7XHJcbiAgICAgICAgXCJ1cmxcIjogd2luZG93LmlucHV0LnNldHRpbmdzLmNvbmZpZy5kYXRhVGFibGVUcmFuc2xhdGlvbnNVcmwsXHJcbiAgICAgIH0sXHJcbiAgICAgIHNlYXJjaEhpZ2hsaWdodDogdHJ1ZSxcclxuICAgICAgcGFnaW5nVHlwZTogXCJmdWxsX251bWJlcnNcIixcclxuICAgICAgbGVuZ3RoQ2hhbmdlOiBmYWxzZSwgLy8gZG8gbm90IHNob3cgcGFnZSBzaXplIHNlbGVjdG9yXHJcbiAgICAgIHBhZ2VMZW5ndGg6IDIwLCAvLyBwYWdlIHNpemVcclxuICAgICAgZG9tOiBcImZydHBcIixcclxuICAgICAgY29uZGl0aW9uYWxQYWdpbmc6IHRydWVcclxuICAgIH0sIHRoaXMudGFibGVPcHRpb25zKVxyXG5cclxuICAgIG9wdGlvbnMuYWpheC5kYXRhID0gZnVuY3Rpb24gKGQpIHtcclxuICAgICAgc2VsZi5hZGRQYXJhbXNUb1JlcXVlc3QoZCk7XHJcbiAgICB9O1xyXG5cclxuICAgIG9wdGlvbnMuYWpheC5jb21wbGV0ZSA9IGZ1bmN0aW9uIChyZXNwb25zZSkge1xyXG4gICAgICBzZWxmLnJlc3BvbnNlUHJvY2Vzc29yKHJlc3BvbnNlLnJlc3BvbnNlSlNPTik7XHJcbiAgICB9O1xyXG5cclxuICAgIHRoaXMudGFibGUgPSAkKHRoaXMuJGVsKS5EYXRhVGFibGUob3B0aW9ucylcclxuICAgIHRoaXMudGFibGUub24oJ2luaXQuZHQnLCB0aGlzLm9uVGFibGVJbml0Q29tcGxldGUpO1xyXG4gICAgdGhpcy50YWJsZS5vbignc2VsZWN0JywgZnVuY3Rpb24gKGUsIGR0LCB0eXBlLCBpbmRleGVzKSB7XHJcbiAgICAgIHNlbGYuJGVtaXQoJ3NlbGVjdCcsIGUsIGR0LCB0eXBlLCBpbmRleGVzKVxyXG4gICAgfSk7XHJcbiAgICB0aGlzLnRhYmxlLm9uKCdkZXNlbGVjdCcsIGZ1bmN0aW9uIChlLCBkdCwgdHlwZSwgaW5kZXhlcykge1xyXG4gICAgICBzZWxmLiRlbWl0KCdkZXNlbGVjdCcsIGUsIGR0LCB0eXBlLCBpbmRleGVzKVxyXG4gICAgfSk7XHJcbiAgICB0aGlzLnRhYmxlLm9uKCdjbGljaycsICd0Ym9keSB0ZCcsIGZ1bmN0aW9uKCkge1xyXG4gICAgICAgIHZhciBjZWxsID0gc2VsZi50YWJsZS5jZWxsKHRoaXMpXHJcblxyXG4gICAgICAgIGlmIChjZWxsLmluZGV4KCkuY29sdW1uID4gMCkge1xyXG4gICAgICAgICAgICB2YXIgcm93SWQgPSBzZWxmLnRhYmxlLnJvdyh0aGlzKS5pZCgpXHJcbiAgICAgICAgICAgIHZhciBjb2x1bW5zID0gc2VsZi50YWJsZS5zZXR0aW5ncygpLmluaXQoKS5jb2x1bW5zO1xyXG5cclxuICAgICAgICAgICAgc2VsZi4kZW1pdCgnY2VsbC1jbGlja2VkJywgY29sdW1uc1t0aGlzLmNlbGxJbmRleF0ubmFtZSwgcm93SWQsIGNlbGwuZGF0YSgpKVxyXG4gICAgICAgIH1cclxuICAgIH0pO1xyXG4gICAgdGhpcy4kZW1pdCgnRGF0YVRhYmxlUmVmJywgdGhpcy50YWJsZSk7XHJcbiAgfSxcclxuICBkZXN0cm95ZWQoKSB7XHJcbiAgfVxyXG59XHJcbjwvc2NyaXB0PiIsIu+7v++7v2ltcG9ydCBWdWUgZnJvbSAndnVlJ1xyXG5pbXBvcnQgVnVlUmVzb3VyY2UgZnJvbSAndnVlLXJlc291cmNlJ1xyXG5pbXBvcnQgVHlwZWFoZWFkIGZyb20gJy4vLi4vVHlwZWFoZWFkLnZ1ZSdcclxuaW1wb3J0IEFzc2lnbm1lbnRzVGFibGUgZnJvbSAnLi9Bc3NpZ25tZW50c1RhYmxlLnZ1ZSdcclxu77u/aW1wb3J0IFZlZVZhbGlkYXRlIGZyb20gJ3ZlZS12YWxpZGF0ZSc7XHJcblxyXG5WdWUudXNlKFZ1ZVJlc291cmNlKTtcclxuXHJcblZ1ZS5jb21wb25lbnQoXCJ0eXBlYWhlYWRcIiwgVHlwZWFoZWFkKTtcclxu77u/VnVlLnVzZShWZWVWYWxpZGF0ZSk7XHJcblZ1ZS5jb21wb25lbnQoXCJhc3NpZ25tZW50cy10YWJsZVwiLCBBc3NpZ25tZW50c1RhYmxlKTsiXX0=
