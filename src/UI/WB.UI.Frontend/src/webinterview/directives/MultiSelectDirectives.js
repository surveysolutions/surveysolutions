import Vue from "vue"

Vue.directive("disabledWhenUnchecked", {
    bind: (el, binding) => {
        if(el.disabled) return;

        if(binding.value.forceDisabled){
            if(el.disabled) return;

            el.disabled = true;
            return;
        }
        
        if(binding.value.answerNotAllowed){
            el.disabled = true;
            return;
        }
        el.disabled = binding.value.maxAnswerReached && !el.checked
    },
    update: (el, binding) => {

        if(binding.value.forceDisabled){
            if(el.disabled) return;
            
            el.disabled = true;
            return;
        }

        if(binding.value.answerNotAllowed){
            el.disabled = true;
            return;
        }
        el.disabled = binding.value.maxAnswerReached && !el.checked
    }
})
