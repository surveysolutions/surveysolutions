import focus from './focus';
import i18next from './i18next';
import number from './number';
import patern from './patern';

const directives = app => {
    focus(app);
    i18next(app);
    number(app);
    patern(app);
};

export default directives;
