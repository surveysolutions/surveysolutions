
import { registerBlurOnEnterKey } from './BlurOnEnterKey'
import { registerDateTimeFormatting } from './DateTimeFormatting'
import { registerMultiSelectDirectives } from './MultiSelectDirectives'
import { registerNumericFormatting } from './NumericFormatting'
import { registerMaskedText } from './MaskedText'
import { registerLinkToRoute } from './LinkToRoute'
import autosize from './autosize'

export function registerDerictives(vue) {

    registerBlurOnEnterKey(vue)
    registerDateTimeFormatting(vue)
    registerMultiSelectDirectives(vue)
    registerNumericFormatting(vue)
    registerMaskedText(vue)
    registerLinkToRoute(vue)
    autosize(vue)
}
