import LocalizationBuilder from './localization.cjs';

export default (userOptions = {}) => {
    return {
        name: 'vite-plugin-localization',
        enforce: 'pre',
        userOptions: userOptions,
        buildStart(options) {
            if (!userOptions.resxProcessed) {
                this.localization = new LocalizationBuilder(userOptions);
                this.localization.prepareLocalizationFiles();
                userOptions.resxProcessed = true;
            }
        }
    };
};
