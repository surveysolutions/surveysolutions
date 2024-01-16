import focus from './focus';
import i18next from './i18next';
import number from './number';
import pattern from './pattern';
import contextmenu from './contextmenu';

const directives = app => {
    focus(app);
    i18next(app);
    number(app);
    pattern(app);
    contextmenu(app);
};

export default directives;
