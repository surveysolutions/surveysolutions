const LocalizationBuilder = require('./localization');
const PluginName = 'LocalizationPlugin';

class LocalizationPlugin {
    constructor(options) {
        this.localization = new LocalizationBuilder(options);
        this.firstRun = true;
    }

    apply(compiler) {
        compiler.hooks.watchRun.tapAsync(PluginName, (comp, cb) => {
            this.localization.prepareLocalizationFiles(comp);
            cb();
        });

        compiler.hooks.run.tapAsync(PluginName, (comp, cb) => {
            this.localization.prepareLocalizationFiles();
            cb();
        });
    }
}

module.exports = LocalizationPlugin;
