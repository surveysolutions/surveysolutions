import LocalizationBuilder from './localization.js';

export default (userOptions = {}) => {
    return {
        name: "vite-plagin-localization",
        enforce: "pre",
        buildStart(options){
            this.localization = new LocalizationBuilder(options);
            this.localization.prepareLocalizationFiles();
        }
    }
}
