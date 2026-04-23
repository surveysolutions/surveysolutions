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

// Mirror the locale detection used in plugins/localization.js so dayjs
// format tokens like MMM/MMMM render in the user's language.
const userLang = navigator.language || navigator.userLanguage || 'en';
const userLocale = userLang.split('-')[0];
dayjs.locale(userLocale);

export default dayjs;
