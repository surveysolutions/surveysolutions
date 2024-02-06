import focus from './focus';
import i18next from './i18next';
import number from './number';
import pattern from './pattern';
import contextmenu from './contextmenu';
import autosize from './autosize';
import sanitizeText from './sanitizeText';
import sanitizeHtml from './sanitizeHtml';

const directives = app => {
    focus(app);
    i18next(app);
    number(app);
    pattern(app);
    contextmenu(app);
    autosize(app);
    sanitizeText(app);
    sanitizeHtml(app);
};

export default directives;
