import * as $ from "jquery";
import Vue from "vue";
import AutoNumeric from "autonumeric";

const defaults = {
  digitGroupSeparator: "",
  decimalPlaces: 0,
  selectOnFocus: false
};

Vue.directive("numericFormatting", {
  bind: (el, binding, vnode) => {
    const settings = $.extend({}, defaults, binding.value);
    vnode.context.autoNumericElement = new AutoNumeric(el, settings);
  }
});
