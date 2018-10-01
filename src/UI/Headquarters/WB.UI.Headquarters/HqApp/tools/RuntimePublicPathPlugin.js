const PLUGINNAME = "RuntimePublicPathPlugin";

module.exports = class RuntimePublicPathPlugin {
    constructor(options) {
        this.options = options;
    }

    apply(compiler) {
        var runtimePublicPathStr = this.options.runtimePublicPath;
        if (!runtimePublicPathStr) {
            throw PLUGINNAME +
                ": no `runtimePublicPath` is specified. This plugin will do nothing.";
        }
        // compiler.hooks.thisCompilation
        // compiler.hooks.compilation
        compiler.hooks.thisCompilation.tap(PLUGINNAME, compilation => {
            compilation.mainTemplate.hooks.requireExtensions.tap(
                PLUGINNAME,
                source => {
                    const publicPathOverride = `// Dynamic assets path override\n__webpack_require__.p = (${runtimePublicPathStr}) || __webpack_require__.p;`;
                    return `${source}\n\n${publicPathOverride}`;
                }
            );
        });
    }
};
