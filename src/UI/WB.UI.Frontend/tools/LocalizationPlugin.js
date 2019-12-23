const LocalizationBuilder = require("./localization");
const PluginName = "LocalizationPlugin";

class LocalizationPlugin {
  constructor(options) {
    this.localization = new LocalizationBuilder(options);
  }

  apply(compiler) {
    compiler.hooks.beforeRun.tapAsync(PluginName, (params, callback) => {
      this.localization.prepareLocalizationFiles();
      // params['localization'] = this.localization;
      callback()
    });
  }
}

module.exports = LocalizationPlugin;
