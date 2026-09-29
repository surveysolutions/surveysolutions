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

const localeImports = {
    ar: () => import('@vee-validate/i18n/dist/locale/ar.json'),
    cs: () => import('@vee-validate/i18n/dist/locale/cs.json'),
    en: () => import('@vee-validate/i18n/dist/locale/en.json'),
    es: () => import('@vee-validate/i18n/dist/locale/es.json'),
    fr: () => import('@vee-validate/i18n/dist/locale/fr.json'),
    id: () => import('@vee-validate/i18n/dist/locale/id.json'),
    ka: () => import('@vee-validate/i18n/dist/locale/ka.json'),
    km: () => import('@vee-validate/i18n/dist/locale/km.json'),
    ro: () => import('@vee-validate/i18n/dist/locale/ro.json'),
    ru: () => import('@vee-validate/i18n/dist/locale/ru.json'),
    sq: () => import('@vee-validate/i18n/dist/locale/sq.json'),
    th: () => import('@vee-validate/i18n/dist/locale/th.json'),
    uk: () => import('@vee-validate/i18n/dist/locale/uk.json'),
    vi: () => import('@vee-validate/i18n/dist/locale/vi.json'),
}

    ; (localeImports[lang] || localeImports['en'])().then((messages) => {
        configure({
            generateMessage: localize({ [lang]: messages.default }),
        })
        setLocale(lang)
    })
