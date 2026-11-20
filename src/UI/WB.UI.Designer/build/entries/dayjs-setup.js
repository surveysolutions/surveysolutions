import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import localizedFormat from 'dayjs/plugin/localizedFormat';
import { getUiLocale } from '../../questionnaire/src/plugins/uiLocale';
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

dayjs.locale(getUiLocale());

export default dayjs;
