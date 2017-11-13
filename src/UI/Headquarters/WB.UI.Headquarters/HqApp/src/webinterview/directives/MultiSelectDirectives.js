import Vue from "vue"

Vue.directive("disabledWhenUnchecked", {
    bind: (el, binding) => {
        if(el.disabled) return;

        if(binding.value.answerNotAllowed){
            el.disabled = true;
            return;
        }
        el.disabled = binding.value.maxAnswerReached && !el.checked
    },
    update: (el, binding) => {

        if(binding.value.answerNotAllowed){
            el.disabled = true;
            return;
        }
        el.disabled = binding.value.maxAnswerReached && !el.checked
    }
})
