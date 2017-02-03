import * as $ from "jquery"

// export function multiSelectDirectivesMixin() {
//     return {
//         directives: {
//             disabledWhenUnchecked: {
//                 bind: (el, binding) => {
//                     $(el).prop("disabled", binding.value && !el.checked)
//                 },
//                 update: (el, binding) => {
//                     $(el).prop("disabled", binding.value && !el.checked)
//                 }
//             }
//         }
//     }
// }

// export const multiSelectDirectives = multiSelectDirectivesMixin()

export const multiSelectDirectives = {
    directives: {
            disabledWhenUnchecked: {
                bind: (el, binding) => {
                    $(el).prop("disabled", binding.value && !el.checked)
                },
                update: (el, binding) => {
                    $(el).prop("disabled", binding.value && !el.checked)
                }
            }
        }
}
