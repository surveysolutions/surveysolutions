import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import duration from 'dayjs/plugin/duration';
import relativeTime from 'dayjs/plugin/relativeTime';
import localizedFormat from 'dayjs/plugin/localizedFormat';
import { getUiLocale } from './uiLocale';

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

dayjs.locale(getUiLocale());

export default dayjs;
