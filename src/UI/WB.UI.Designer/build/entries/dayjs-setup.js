import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import localizedFormat from 'dayjs/plugin/localizedFormat';
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

dayjs.extend(utc);
dayjs.extend(localizedFormat);

// Use the culture set by the server on the <html lang="..."> attribute, falling back to
// the browser's preferred language and then to English.
const serverLocale = document.documentElement.lang;
const userLocale = (serverLocale || navigator.language || navigator.userLanguage || 'en').split('-')[0];
dayjs.locale(userLocale);

export default dayjs;
