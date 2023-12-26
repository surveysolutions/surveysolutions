import focus from './focus';
import i18next from './i18next';

const directives = app => {
    focus(app);
    i18next(app);
};

export default directives;
