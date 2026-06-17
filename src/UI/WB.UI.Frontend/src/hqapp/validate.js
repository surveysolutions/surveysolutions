//register validations globaly
//add more rules if required
//https://vee-validate.logaretm.com/v4/guide/global-validators
import { configure, defineRule } from 'vee-validate'
import { required, email, integer, max_value, min, min_value, max, numeric, not_one_of, regex } from '@vee-validate/rules'

import { browserLanguage } from '~/shared/helpers'
import { localize, setLocale } from '@vee-validate/i18n'

const supportedLocales = ['ar', 'cs', 'en', 'es', 'fr', 'id', 'ka', 'km', 'ro', 'ru', 'sq', 'th', 'uk', 'vi']
const lang = supportedLocales.includes(browserLanguage) ? browserLanguage : 'en'

defineRule('required', required)
defineRule('email', email)
defineRule('integer', integer)
defineRule('max_value', max_value)
defineRule('min', min)
defineRule('min_value', min_value)
defineRule('max', max)
defineRule('numeric', numeric)
defineRule('not_one_of', not_one_of)
defineRule('regex', regex)

//import once it's impenemted
defineRule('required_if', (value, [target, targetValue], ctx) => {
    if (targetValue === ctx.form[target]) {
        return required(value)
    }
    return true
})


defineRule('callLocalMethod', (value, { method }) => {
    if (typeof method !== 'function') {
        return 'Method to call is missing'
    }

    const result = method(value)
    if (typeof result === 'string') {
        return result
    }
    return result ? true : 'Validation error'
})

import(`@vee-validate/i18n/dist/locale/${lang}.json`).then((messages) => {
    configure({
        generateMessage: localize({ [lang]: messages.default }),
    })
    setLocale(lang)
})
