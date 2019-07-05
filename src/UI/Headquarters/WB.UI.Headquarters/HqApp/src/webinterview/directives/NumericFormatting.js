import Vue from "vue";
import AutoNumeric from "autonumeric/src/main";

const defaults = {
  digitGroupSeparator: "",
  decimalPlaces: 0,
  selectOnFocus: false,
  unformatOnHover: false,
  unformatOnSubmit: false,
  watchExternalChanges: true,
};

Vue.directive("numericFormatting", {
  bind: (el, binding, vnode) => {
    const settings = _.assign(defaults, binding.value);
    vnode.context.autoNumericElement = new AutoNumeric(el, settings);
  }
});
