(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
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

},{}],2:[function(require,module,exports){
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
    hotAPI.rerender("data-v-4ea71100", __vue__options__)
  }
})()}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"vue-hot-reload-api":1}],3:[function(require,module,exports){
(function (global){
'use strict';

var _vue = (typeof window !== "undefined" ? window['Vue'] : typeof global !== "undefined" ? global['Vue'] : null);

var _vue2 = _interopRequireDefault(_vue);

var _vueResource = (typeof window !== "undefined" ? window['VueResource'] : typeof global !== "undefined" ? global['VueResource'] : null);

var _vueResource2 = _interopRequireDefault(_vueResource);

var _UserSelector = require('./UserSelector.vue');

var _UserSelector2 = _interopRequireDefault(_UserSelector);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

_vue2.default.use(_vueResource2.default);

_vue2.default.component("user-selector", _UserSelector2.default);

var app = new _vue2.default({
    data: {
        interviewerId: null,
        questionnaireId: null
    },
    methods: {
        userSelected: function userSelected(newValue, id) {
            this.interviewerId = newValue;
        },
        questionnaireSelected: function questionnaireSelected(newValue, id) {
            this.questionnaireId = newValue;
        }
    }
});

window.onload = function () {
    _vue2.default.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
};

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})

},{"./UserSelector.vue":2}]},{},[3])
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm5vZGVfbW9kdWxlcy9icm93c2VyLXBhY2svX3ByZWx1ZGUuanMiLCJub2RlX21vZHVsZXMvdnVlLWhvdC1yZWxvYWQtYXBpL2luZGV4LmpzIiwidnVlXFx2dWVcXFVzZXJTZWxlY3Rvci52dWU/NGZmMDU0Y2UiLCJ2dWVcXHZ1ZVxcdHJvdWJsZXNob290aW5nLWNlbnN1cy5qcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtBQ0FBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7Ozs7OztBQzNHQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBSEE7QUFLQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBTkE7QUFRQTtBQUFBOztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBQ0E7QUFBQTs7QUFBQTs7QUFDQTtBQUNBO0FBQ0E7QUFFQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBaERBO0FBOUJBOzs7OztBQTdCQTtBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7QUNBQzs7OztBQUNEOzs7O0FBQ0E7Ozs7OztBQUVBLGNBQUksR0FBSjs7QUFFQSxjQUFJLFNBQUosQ0FBYyxlQUFkOztBQUVBLElBQUksTUFBTSxrQkFBUTtBQUNkLFVBQU07QUFDRix1QkFBZSxJQURiO0FBRUYseUJBQWlCO0FBRmYsS0FEUTtBQUtkLGFBQVM7QUFDTCxvQkFESyx3QkFDUSxRQURSLEVBQ2tCLEVBRGxCLEVBQ3NCO0FBQ3ZCLGlCQUFLLGFBQUwsR0FBcUIsUUFBckI7QUFDSCxTQUhJO0FBSUwsNkJBSkssaUNBSWlCLFFBSmpCLEVBSTJCLEVBSjNCLEVBSStCO0FBQ2hDLGlCQUFLLGVBQUwsR0FBdUIsUUFBdkI7QUFDSDtBQU5JO0FBTEssQ0FBUixDQUFWOztBQWVBLE9BQU8sTUFBUCxHQUFnQixZQUFZO0FBQ3hCLGtCQUFJLElBQUosQ0FBUyxPQUFULENBQWlCLE1BQWpCLENBQXdCLGVBQXhCLElBQTJDLE1BQU0sUUFBTixDQUFlLEtBQWYsQ0FBcUIsS0FBaEU7O0FBRUEsUUFBSSxNQUFKLENBQVcsTUFBWDtBQUNILENBSkQiLCJmaWxlIjoiZ2VuZXJhdGVkLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXNDb250ZW50IjpbIihmdW5jdGlvbiBlKHQsbixyKXtmdW5jdGlvbiBzKG8sdSl7aWYoIW5bb10pe2lmKCF0W29dKXt2YXIgYT10eXBlb2YgcmVxdWlyZT09XCJmdW5jdGlvblwiJiZyZXF1aXJlO2lmKCF1JiZhKXJldHVybiBhKG8sITApO2lmKGkpcmV0dXJuIGkobywhMCk7dmFyIGY9bmV3IEVycm9yKFwiQ2Fubm90IGZpbmQgbW9kdWxlICdcIitvK1wiJ1wiKTt0aHJvdyBmLmNvZGU9XCJNT0RVTEVfTk9UX0ZPVU5EXCIsZn12YXIgbD1uW29dPXtleHBvcnRzOnt9fTt0W29dWzBdLmNhbGwobC5leHBvcnRzLGZ1bmN0aW9uKGUpe3ZhciBuPXRbb11bMV1bZV07cmV0dXJuIHMobj9uOmUpfSxsLGwuZXhwb3J0cyxlLHQsbixyKX1yZXR1cm4gbltvXS5leHBvcnRzfXZhciBpPXR5cGVvZiByZXF1aXJlPT1cImZ1bmN0aW9uXCImJnJlcXVpcmU7Zm9yKHZhciBvPTA7bzxyLmxlbmd0aDtvKyspcyhyW29dKTtyZXR1cm4gc30pIiwidmFyIFZ1ZSAvLyBsYXRlIGJpbmRcbnZhciB2ZXJzaW9uXG52YXIgbWFwID0gd2luZG93Ll9fVlVFX0hPVF9NQVBfXyA9IE9iamVjdC5jcmVhdGUobnVsbClcbnZhciBpbnN0YWxsZWQgPSBmYWxzZVxudmFyIGlzQnJvd3NlcmlmeSA9IGZhbHNlXG52YXIgaW5pdEhvb2tOYW1lID0gJ2JlZm9yZUNyZWF0ZSdcblxuZXhwb3J0cy5pbnN0YWxsID0gZnVuY3Rpb24gKHZ1ZSwgYnJvd3NlcmlmeSkge1xuICBpZiAoaW5zdGFsbGVkKSByZXR1cm5cbiAgaW5zdGFsbGVkID0gdHJ1ZVxuXG4gIFZ1ZSA9IHZ1ZS5fX2VzTW9kdWxlID8gdnVlLmRlZmF1bHQgOiB2dWVcbiAgdmVyc2lvbiA9IFZ1ZS52ZXJzaW9uLnNwbGl0KCcuJykubWFwKE51bWJlcilcbiAgaXNCcm93c2VyaWZ5ID0gYnJvd3NlcmlmeVxuXG4gIC8vIGNvbXBhdCB3aXRoIDwgMi4wLjAtYWxwaGEuN1xuICBpZiAoVnVlLmNvbmZpZy5fbGlmZWN5Y2xlSG9va3MuaW5kZXhPZignaW5pdCcpID4gLTEpIHtcbiAgICBpbml0SG9va05hbWUgPSAnaW5pdCdcbiAgfVxuXG4gIGV4cG9ydHMuY29tcGF0aWJsZSA9IHZlcnNpb25bMF0gPj0gMlxuICBpZiAoIWV4cG9ydHMuY29tcGF0aWJsZSkge1xuICAgIGNvbnNvbGUud2FybihcbiAgICAgICdbSE1SXSBZb3UgYXJlIHVzaW5nIGEgdmVyc2lvbiBvZiB2dWUtaG90LXJlbG9hZC1hcGkgdGhhdCBpcyAnICtcbiAgICAgICdvbmx5IGNvbXBhdGlibGUgd2l0aCBWdWUuanMgY29yZSBeMi4wLjAuJ1xuICAgIClcbiAgICByZXR1cm5cbiAgfVxufVxuXG4vKipcbiAqIENyZWF0ZSBhIHJlY29yZCBmb3IgYSBob3QgbW9kdWxlLCB3aGljaCBrZWVwcyB0cmFjayBvZiBpdHMgY29uc3RydWN0b3JcbiAqIGFuZCBpbnN0YW5jZXNcbiAqXG4gKiBAcGFyYW0ge1N0cmluZ30gaWRcbiAqIEBwYXJhbSB7T2JqZWN0fSBvcHRpb25zXG4gKi9cblxuZXhwb3J0cy5jcmVhdGVSZWNvcmQgPSBmdW5jdGlvbiAoaWQsIG9wdGlvbnMpIHtcbiAgdmFyIEN0b3IgPSBudWxsXG4gIGlmICh0eXBlb2Ygb3B0aW9ucyA9PT0gJ2Z1bmN0aW9uJykge1xuICAgIEN0b3IgPSBvcHRpb25zXG4gICAgb3B0aW9ucyA9IEN0b3Iub3B0aW9uc1xuICB9XG4gIG1ha2VPcHRpb25zSG90KGlkLCBvcHRpb25zKVxuICBtYXBbaWRdID0ge1xuICAgIEN0b3I6IFZ1ZS5leHRlbmQob3B0aW9ucyksXG4gICAgaW5zdGFuY2VzOiBbXVxuICB9XG59XG5cbi8qKlxuICogTWFrZSBhIENvbXBvbmVudCBvcHRpb25zIG9iamVjdCBob3QuXG4gKlxuICogQHBhcmFtIHtTdHJpbmd9IGlkXG4gKiBAcGFyYW0ge09iamVjdH0gb3B0aW9uc1xuICovXG5cbmZ1bmN0aW9uIG1ha2VPcHRpb25zSG90IChpZCwgb3B0aW9ucykge1xuICBpbmplY3RIb29rKG9wdGlvbnMsIGluaXRIb29rTmFtZSwgZnVuY3Rpb24gKCkge1xuICAgIG1hcFtpZF0uaW5zdGFuY2VzLnB1c2godGhpcylcbiAgfSlcbiAgaW5qZWN0SG9vayhvcHRpb25zLCAnYmVmb3JlRGVzdHJveScsIGZ1bmN0aW9uICgpIHtcbiAgICB2YXIgaW5zdGFuY2VzID0gbWFwW2lkXS5pbnN0YW5jZXNcbiAgICBpbnN0YW5jZXMuc3BsaWNlKGluc3RhbmNlcy5pbmRleE9mKHRoaXMpLCAxKVxuICB9KVxufVxuXG4vKipcbiAqIEluamVjdCBhIGhvb2sgdG8gYSBob3QgcmVsb2FkYWJsZSBjb21wb25lbnQgc28gdGhhdFxuICogd2UgY2FuIGtlZXAgdHJhY2sgb2YgaXQuXG4gKlxuICogQHBhcmFtIHtPYmplY3R9IG9wdGlvbnNcbiAqIEBwYXJhbSB7U3RyaW5nfSBuYW1lXG4gKiBAcGFyYW0ge0Z1bmN0aW9ufSBob29rXG4gKi9cblxuZnVuY3Rpb24gaW5qZWN0SG9vayAob3B0aW9ucywgbmFtZSwgaG9vaykge1xuICB2YXIgZXhpc3RpbmcgPSBvcHRpb25zW25hbWVdXG4gIG9wdGlvbnNbbmFtZV0gPSBleGlzdGluZ1xuICAgID8gQXJyYXkuaXNBcnJheShleGlzdGluZylcbiAgICAgID8gZXhpc3RpbmcuY29uY2F0KGhvb2spXG4gICAgICA6IFtleGlzdGluZywgaG9va11cbiAgICA6IFtob29rXVxufVxuXG5mdW5jdGlvbiB0cnlXcmFwIChmbikge1xuICByZXR1cm4gZnVuY3Rpb24gKGlkLCBhcmcpIHtcbiAgICB0cnkgeyBmbihpZCwgYXJnKSB9IGNhdGNoIChlKSB7XG4gICAgICBjb25zb2xlLmVycm9yKGUpXG4gICAgICBjb25zb2xlLndhcm4oJ1NvbWV0aGluZyB3ZW50IHdyb25nIGR1cmluZyBWdWUgY29tcG9uZW50IGhvdC1yZWxvYWQuIEZ1bGwgcmVsb2FkIHJlcXVpcmVkLicpXG4gICAgfVxuICB9XG59XG5cbmV4cG9ydHMucmVyZW5kZXIgPSB0cnlXcmFwKGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICB2YXIgcmVjb3JkID0gbWFwW2lkXVxuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBvcHRpb25zID0gb3B0aW9ucy5vcHRpb25zXG4gIH1cbiAgcmVjb3JkLkN0b3Iub3B0aW9ucy5yZW5kZXIgPSBvcHRpb25zLnJlbmRlclxuICByZWNvcmQuQ3Rvci5vcHRpb25zLnN0YXRpY1JlbmRlckZucyA9IG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zXG4gIHJlY29yZC5pbnN0YW5jZXMuc2xpY2UoKS5mb3JFYWNoKGZ1bmN0aW9uIChpbnN0YW5jZSkge1xuICAgIGluc3RhbmNlLiRvcHRpb25zLnJlbmRlciA9IG9wdGlvbnMucmVuZGVyXG4gICAgaW5zdGFuY2UuJG9wdGlvbnMuc3RhdGljUmVuZGVyRm5zID0gb3B0aW9ucy5zdGF0aWNSZW5kZXJGbnNcbiAgICBpbnN0YW5jZS5fc3RhdGljVHJlZXMgPSBbXSAvLyByZXNldCBzdGF0aWMgdHJlZXNcbiAgICBpbnN0YW5jZS4kZm9yY2VVcGRhdGUoKVxuICB9KVxufSlcblxuZXhwb3J0cy5yZWxvYWQgPSB0cnlXcmFwKGZ1bmN0aW9uIChpZCwgb3B0aW9ucykge1xuICBpZiAodHlwZW9mIG9wdGlvbnMgPT09ICdmdW5jdGlvbicpIHtcbiAgICBvcHRpb25zID0gb3B0aW9ucy5vcHRpb25zXG4gIH1cbiAgbWFrZU9wdGlvbnNIb3QoaWQsIG9wdGlvbnMpXG4gIHZhciByZWNvcmQgPSBtYXBbaWRdXG4gIGlmICh2ZXJzaW9uWzFdIDwgMikge1xuICAgIC8vIHByZXNlcnZlIHByZSAyLjIgYmVoYXZpb3IgZm9yIGdsb2JhbCBtaXhpbiBoYW5kbGluZ1xuICAgIHJlY29yZC5DdG9yLmV4dGVuZE9wdGlvbnMgPSBvcHRpb25zXG4gIH1cbiAgdmFyIG5ld0N0b3IgPSByZWNvcmQuQ3Rvci5zdXBlci5leHRlbmQob3B0aW9ucylcbiAgcmVjb3JkLkN0b3Iub3B0aW9ucyA9IG5ld0N0b3Iub3B0aW9uc1xuICByZWNvcmQuQ3Rvci5jaWQgPSBuZXdDdG9yLmNpZFxuICByZWNvcmQuQ3Rvci5wcm90b3R5cGUgPSBuZXdDdG9yLnByb3RvdHlwZVxuICBpZiAobmV3Q3Rvci5yZWxlYXNlKSB7XG4gICAgLy8gdGVtcG9yYXJ5IGdsb2JhbCBtaXhpbiBzdHJhdGVneSB1c2VkIGluIDwgMi4wLjAtYWxwaGEuNlxuICAgIG5ld0N0b3IucmVsZWFzZSgpXG4gIH1cbiAgcmVjb3JkLmluc3RhbmNlcy5zbGljZSgpLmZvckVhY2goZnVuY3Rpb24gKGluc3RhbmNlKSB7XG4gICAgaWYgKGluc3RhbmNlLiR2bm9kZSAmJiBpbnN0YW5jZS4kdm5vZGUuY29udGV4dCkge1xuICAgICAgaW5zdGFuY2UuJHZub2RlLmNvbnRleHQuJGZvcmNlVXBkYXRlKClcbiAgICB9IGVsc2Uge1xuICAgICAgY29uc29sZS53YXJuKCdSb290IG9yIG1hbnVhbGx5IG1vdW50ZWQgaW5zdGFuY2UgbW9kaWZpZWQuIEZ1bGwgcmVsb2FkIHJlcXVpcmVkLicpXG4gICAgfVxuICB9KVxufSlcbiIsIu+7vzx0ZW1wbGF0ZT5cclxuICAgIDxkaXYgY2xhc3M9XCJjb21iby1ib3hcIj5cclxuICAgICAgICA8ZGl2IGNsYXNzPVwiYnRuLWdyb3VwIGJ0bi1pbnB1dCBjbGVhcmZpeFwiPlxyXG4gICAgICAgICAgICA8YnV0dG9uIHR5cGU9XCJidXR0b25cIiBjbGFzcz1cImJ0biBkcm9wZG93bi10b2dnbGVcIiBkYXRhLXRvZ2dsZT1cImRyb3Bkb3duXCI+XHJcbiAgICAgICAgICAgICAgICA8c3BhbiBkYXRhLWJpbmQ9XCJsYWJlbFwiIHYtaWY9XCJ2YWx1ZSA9PT0gbnVsbFwiIGNsYXNzPVwiZ3JheS10ZXh0XCI+e3twbGFjZWhvbGRlclRleHR9fTwvc3Bhbj5cclxuICAgICAgICAgICAgICAgIDxzcGFuIGRhdGEtYmluZD1cImxhYmVsXCIgdi1lbHNlPnt7dmFsdWUudmFsdWV9fTwvc3Bhbj5cclxuICAgICAgICAgICAgPC9idXR0b24+XHJcbiAgICAgICAgICAgIDx1bCByZWY9XCJkcm9wZG93bk1lbnVcIiBjbGFzcz1cImRyb3Bkb3duLW1lbnVcIiByb2xlPVwibWVudVwiPlxyXG4gICAgICAgICAgICAgICAgPGxpPlxyXG4gICAgICAgICAgICAgICAgICAgIDxpbnB1dCB0eXBlPVwidGV4dFwiIHJlZj1cInNlYXJjaEJveFwiIDppZD1cImlucHV0SWRcIiBwbGFjZWhvbGRlcj1cIlNlYXJjaFwiIEBpbnB1dD1cInVwZGF0ZU9wdGlvbnNMaXN0XCIgdi1vbjprZXl1cC5kb3duPVwib25TZWFyY2hCb3hEb3duS2V5XCIgdi1tb2RlbD1cInNlYXJjaFRlcm1cIiAvPlxyXG4gICAgICAgICAgICAgICAgPC9saT5cclxuICAgICAgICAgICAgICAgIDxsaSB2LWZvcj1cIm9wdGlvbiBpbiBvcHRpb25zXCIgOmtleT1cIm9wdGlvbi5rZXlcIj5cclxuICAgICAgICAgICAgICAgICAgICA8YSBocmVmPVwiamF2YXNjcmlwdDp2b2lkKDApO1wiIHYtb246Y2xpY2s9XCJzZWxlY3RPcHRpb24ob3B0aW9uKVwiIHYtaHRtbD1cImhpZ2hsaWdodChvcHRpb24udmFsdWUsIHNlYXJjaFRlcm0pXCIgdi1vbjprZXlkb3duLnVwPVwib25PcHRpb25VcEtleVwiPjwvYT5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgICAgICA8bGkgdi1pZj1cImlzTG9hZGluZ1wiPlxyXG4gICAgICAgICAgICAgICAgICAgIDxhPkxvYWRpbmcuLi48L2E+XHJcbiAgICAgICAgICAgICAgICA8L2xpPlxyXG4gICAgICAgICAgICAgICAgPGxpIHYtaWY9XCIhaXNMb2FkaW5nICYmIG9wdGlvbnMubGVuZ3RoID09PSAwXCI+XHJcbiAgICAgICAgICAgICAgICAgICAgPGE+Tm8gcmVzdWx0cyBmb3VuZDwvYT5cclxuICAgICAgICAgICAgICAgIDwvbGk+XHJcbiAgICAgICAgICAgIDwvdWw+XHJcbiAgICAgICAgPC9kaXY+XHJcbiAgICAgICAgPGJ1dHRvbiB2LWlmPVwidmFsdWUgIT09IG51bGxcIiBjbGFzcz1cImJ0biBidG4tbGluayBidG4tY2xlYXJcIiBAY2xpY2s9XCJjbGVhclwiPlxyXG4gICAgICAgICAgICA8c3Bhbj48L3NwYW4+XHJcbiAgICAgICAgPC9idXR0b24+XHJcbiAgICA8L2Rpdj5cclxuPC90ZW1wbGF0ZT5cclxuXHJcbjxzY3JpcHQ+XHJcbiAgICBtb2R1bGUuZXhwb3J0cyA9IHtcclxuICAgICAgICBuYW1lOiAndXNlci1zZWxlY3RvcicsXHJcbiAgICAgICAgcHJvcHM6IFsnZmV0Y2hVcmwnLCAnY29udHJvbElkJywgJ3ZhbHVlJywgJ3BsYWNlaG9sZGVyJ10sXHJcbiAgICAgICAgZGF0YTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICByZXR1cm4ge1xyXG4gICAgICAgICAgICAgICAgb3B0aW9uczogW10sXHJcbiAgICAgICAgICAgICAgICBpc0xvYWRpbmc6IGZhbHNlLFxyXG4gICAgICAgICAgICAgICAgc2VhcmNoVGVybTogJydcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIGNvbXB1dGVkOiB7XHJcbiAgICAgICAgICAgIGlucHV0SWQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiBgc2JfJHt0aGlzLmNvbnRyb2xJZH1gO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBwbGFjZWhvbGRlclRleHQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLnBsYWNlaG9sZGVyIHx8IFwiU2VsZWN0XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG4gICAgICAgIG1vdW50ZWQ6IGZ1bmN0aW9uKCkge1xyXG4gICAgICAgICAgICBjb25zdCBqcUVsID0gJCh0aGlzLiRlbClcclxuICAgICAgICAgICAgY29uc3QgZm9jdXNUbyA9IGpxRWwuZmluZChgIyR7dGhpcy5pbnB1dElkfWApXHJcbiAgICAgICAgICAgIGpxRWwub24oJ3Nob3duLmJzLmRyb3Bkb3duJywgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgZm9jdXNUby5mb2N1cygpXHJcbiAgICAgICAgICAgICAgICB0aGlzLmZldGNoT3B0aW9ucyh0aGlzLnNlYXJjaFRlcm0pXHJcbiAgICAgICAgICAgIH0pXHJcblxyXG4gICAgICAgICAgICBqcUVsLm9uKCdoaWRkZW4uYnMuZHJvcGRvd24nLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnNlYXJjaFRlcm0gPSBcIlwiXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgfSxcclxuICAgICAgICBtZXRob2RzOiB7XHJcbiAgICAgICAgICAgIG9uU2VhcmNoQm94RG93bktleShldmVudCkge1xyXG4gICAgICAgICAgICAgICAgdmFyICRmaXJzdE9wdGlvbkFuY2hvciA9ICQodGhpcy4kcmVmcy5kcm9wZG93bk1lbnUpLmZpbmQoJ2EnKS5maXJzdCgpO1xyXG4gICAgICAgICAgICAgICAgJGZpcnN0T3B0aW9uQW5jaG9yLmZvY3VzKCk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIG9uT3B0aW9uVXBLZXkoZXZlbnQpIHtcclxuICAgICAgICAgICAgICAgIHZhciBpc0ZpcnN0T3B0aW9uID0gJChldmVudC50YXJnZXQpLnBhcmVudCgpLmluZGV4KCkgPT09IDE7XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKGlzRmlyc3RPcHRpb24pIHtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLiRyZWZzLnNlYXJjaEJveC5mb2N1cygpO1xyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50LnN0b3BQcm9wYWdhdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBmZXRjaE9wdGlvbnM6IGZ1bmN0aW9uIChmaWx0ZXIgPSBcIlwiKSB7XHJcbiAgICAgICAgICAgICAgICBjb25zb2xlLmxvZyhgZmlsdGVyOiB7ZmlsdGVyfWApO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRpbmcgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kaHR0cC5nZXQodGhpcy5mZXRjaFVybCwge3BhcmFtczogeyBxdWVyeTogZmlsdGVyIH19KVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5vcHRpb25zID0gcmVzcG9uc2UuYm9keS5vcHRpb25zIHx8IFtdO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGluZyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIH0sIHJlc3BvbnNlID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBjbGVhcjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kZW1pdCgnc2VsZWN0ZWQnLCBudWxsLCB0aGlzLmNvbnRyb2xJZCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnNlYXJjaFRlcm0gPSBcIlwiO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICBzZWxlY3RPcHRpb246IGZ1bmN0aW9uICh2YWx1ZSkge1xyXG4gICAgICAgICAgICAgICAgdGhpcy4kZW1pdCgnc2VsZWN0ZWQnLCB2YWx1ZSwgdGhpcy5jb250cm9sSWQpO1xyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICB1cGRhdGVPcHRpb25zTGlzdChlKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmZldGNoT3B0aW9ucyhlLnRhcmdldC52YWx1ZSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIGhpZ2hsaWdodDogZnVuY3Rpb24gKHRpdGxlLCBzZWFyY2hUZXJtKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgZW5jb2RlZFRpdGxlID0gXy5lc2NhcGUodGl0bGUpO1xyXG4gICAgICAgICAgICAgICAgaWYgKHNlYXJjaFRlcm0pIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgc2FmZVNlYXJjaFRlcm0gPSBfLmVzY2FwZShfLmVzY2FwZVJlZ0V4cChzZWFyY2hUZXJtKSk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgIHZhciBpUXVlcnkgPSBuZXcgUmVnRXhwKHNhZmVTZWFyY2hUZXJtLCBcImlnXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBlbmNvZGVkVGl0bGUucmVwbGFjZShpUXVlcnksIChtYXRjaGVkVHh0LCBhLCBiKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBgPHN0cm9uZz4ke21hdGNoZWRUeHR9PC9zdHJvbmc+YDtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgICAgICByZXR1cm4gZW5jb2RlZFRpdGxlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuPC9zY3JpcHQ+Iiwi77u/aW1wb3J0IFZ1ZSBmcm9tICd2dWUnXHJcbmltcG9ydCBWdWVSZXNvdXJjZSBmcm9tICd2dWUtcmVzb3VyY2UnXHJcbmltcG9ydCBVc2VyU2VsZWN0b3IgZnJvbSAnLi9Vc2VyU2VsZWN0b3IudnVlJ1xyXG5cclxuVnVlLnVzZShWdWVSZXNvdXJjZSk7XHJcblxyXG5WdWUuY29tcG9uZW50KFwidXNlci1zZWxlY3RvclwiLCBVc2VyU2VsZWN0b3IpO1xyXG5cclxudmFyIGFwcCA9IG5ldyBWdWUoe1xyXG4gICAgZGF0YToge1xyXG4gICAgICAgIGludGVydmlld2VySWQ6IG51bGwsXHJcbiAgICAgICAgcXVlc3Rpb25uYWlyZUlkOiBudWxsXHJcbiAgICB9LFxyXG4gICAgbWV0aG9kczoge1xyXG4gICAgICAgIHVzZXJTZWxlY3RlZChuZXdWYWx1ZSwgaWQpIHtcclxuICAgICAgICAgICAgdGhpcy5pbnRlcnZpZXdlcklkID0gbmV3VmFsdWU7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBxdWVzdGlvbm5haXJlU2VsZWN0ZWQobmV3VmFsdWUsIGlkKSB7XHJcbiAgICAgICAgICAgIHRoaXMucXVlc3Rpb25uYWlyZUlkID0gbmV3VmFsdWU7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG59KTtcclxuXHJcbndpbmRvdy5vbmxvYWQgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICBWdWUuaHR0cC5oZWFkZXJzLmNvbW1vblsnQXV0aG9yaXphdGlvbiddID0gaW5wdXQuc2V0dGluZ3MuYWNzcmYudG9rZW47XHJcblxyXG4gICAgYXBwLiRtb3VudCgnI2FwcCcpO1xyXG59Il19
