module.exports = class RuntimePublicPathPlugin {
    constructor(options) {
        this.options = options;
    }

    apply(compiler){
        var runtimePublicPathStr = this.options.runtimePublicPath;
        if (!runtimePublicPathStr) {
            console.error('RuntimePublicPath: no `runtimePublicPath` is specified. This plugin will do nothing.');
            return;
        }

        compiler.plugin('this-compilation', function(compilation) {
            compilation.mainTemplate.plugin('require-extensions', function(source, chunk, hash) {
                const publicPathOverride = `// Dynamic assets path override\n${this.requireFn}.p = (${runtimePublicPathStr}) || ${this.requireFn}.p;`;
                return `${source}\n\n${publicPathOverride}`;
            });
        });
    }
}
 