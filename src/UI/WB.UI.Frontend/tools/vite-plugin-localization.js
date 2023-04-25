import LocalizationBuilder from './localization.js';

export default (userOptions = {}) => {
    return {
        name: "vite-plugin-localization",
        enforce: "pre",
        userOptions: userOptions,
        buildStart(options){
            this.localization = new LocalizationBuilder(userOptions);
            this.localization.prepareLocalizationFiles();
        }
    }
}
