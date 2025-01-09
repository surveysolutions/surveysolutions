//register validations globaly
//add more rules if required 
//https://vee-validate.logaretm.com/v4/guide/global-validators
import { configure, defineRule } from 'vee-validate'
import { required, email, integer, max_value, min, min_value, max, numeric, not_one_of, regex } from '@vee-validate/rules'

import { browserLanguage } from '~/shared/helpers'
import { localize, setLocale } from '@vee-validate/i18n';
import es from '@vee-validate/i18n/dist/locale/es.json';
import vi from '@vee-validate/i18n/dist/locale/vi.json';
import uk from '@vee-validate/i18n/dist/locale/uk.json';
import th from '@vee-validate/i18n/dist/locale/th.json';
import sq from '@vee-validate/i18n/dist/locale/sq.json';
import ru from '@vee-validate/i18n/dist/locale/ru.json';
import ro from '@vee-validate/i18n/dist/locale/ro.json';
import km from '@vee-validate/i18n/dist/locale/km.json';
import ka from '@vee-validate/i18n/dist/locale/ka.json';
import id from '@vee-validate/i18n/dist/locale/id.json';
import fr from '@vee-validate/i18n/dist/locale/fr.json';
import en from '@vee-validate/i18n/dist/locale/en.json';
import cs from '@vee-validate/i18n/dist/locale/cs.json';
import ar from '@vee-validate/i18n/dist/locale/ar.json';

setLocale(browserLanguage)

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
defineRule("required_if", (value, [target, targetValue], ctx) => {
    if (targetValue === ctx.form[target]) {
        return required(value);
    }
    return true;
});


defineRule('callLocalMethod', (value, { method }) => {
    if (typeof method !== 'function') {
        return 'Method to call is missing';
    }

    const result = method(value);
    if (typeof result === 'string') {
        return result;
    }
    return result ? true : 'Validation error';
});

configure({
    generateMessage: localize({
        en,
        es,
        vi,
        uk,
        th,
        sq,
        ru,
        ro,
        km,
        ka,
        id,
        fr,
        cs,
        ar,
    }),
});
