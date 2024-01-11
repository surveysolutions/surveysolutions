import focus from './focus';
import i18next from './i18next';
import number from './number';
import pattern from './pattern';

const directives = app => {
    focus(app);
    i18next(app);
    number(app);
    pattern(app);
};

export default directives;
