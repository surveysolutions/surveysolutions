import LocalizationBuilder from './localization.js';

export default (userOptions = {}) => {
    return {
        name: "vite-plugin-localization",
        enforce: "pre",
        userOptions: userOptions,
        options(options) {
            this.localization = new LocalizationBuilder(userOptions);
            this.localization.prepareLocalizationFiles();
        }
    }
}
