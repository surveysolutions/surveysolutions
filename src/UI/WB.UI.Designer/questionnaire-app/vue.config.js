const path = require('path');
const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const LocalizationPlugin = require('./tools/LocalizationPlugin');
const extraWatch = require('extra-watch-webpack-plugin');

module.exports = {
    transpileDependencies: ['vuetify'],

    pluginOptions: {
        i18n: {
            locale: 'en',
            fallbackLocale: 'en',
            localeDir: 'locale',
            enableInSFC: false
        }
    },

    chainWebpack: config => {
        const resxFiles = [
            join('../Resources/QuestionnaireEditor.resx'),
            join('../Resources/QuestionnaireEditor.*.resx')
        ];

        config.plugin('extraWatch').use(extraWatch, [{ files: resxFiles }]);

        config.plugin('localization').use(LocalizationPlugin, [
            {
                noHash: true,
                inline: true,
                patterns: resxFiles,
                destination: './src/locale',
                locales: {
                    '.': ['QuestionnaireEditor']
                }
            }
        ]);
    }
};
