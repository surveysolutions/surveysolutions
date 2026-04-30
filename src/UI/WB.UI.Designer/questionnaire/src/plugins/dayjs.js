import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import duration from 'dayjs/plugin/duration';
import relativeTime from 'dayjs/plugin/relativeTime';
import localizedFormat from 'dayjs/plugin/localizedFormat';

dayjs.extend(utc);
dayjs.extend(duration);
dayjs.extend(relativeTime);
dayjs.extend(localizedFormat);

// Import locale packs for all languages supported by the Designer SPA.
// dayjs defaults to English so we only import non-English packs explicitly;
// the English pack is built in.
import 'dayjs/locale/ar';
import 'dayjs/locale/cs';
import 'dayjs/locale/es';
import 'dayjs/locale/fr';
import 'dayjs/locale/pt';
import 'dayjs/locale/ro';
import 'dayjs/locale/ru';
import 'dayjs/locale/sq';
import 'dayjs/locale/uk';
import 'dayjs/locale/zh';

// Prefer the UI language from <html lang> so dayjs stays aligned with the
// rest of the application, and only fall back to the browser language when
// no explicit UI language is available.
const uiLang = document.documentElement.lang
    || navigator.language
    || navigator.userLanguage
    || 'en';
const userLocale = uiLang.split('-')[0];
dayjs.locale(userLocale);

export default dayjs;
