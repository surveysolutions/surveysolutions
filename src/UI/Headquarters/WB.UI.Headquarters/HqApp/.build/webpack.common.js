const webpack = require('webpack')
const HtmlWebpackPlugin = require('html-webpack-plugin');
const WebpackNotifierPlugin = require('webpack-notifier');
const cleanWebpackPlugin = require('clean-webpack-plugin');
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const RuntimePublicPathPlugin = require("./RuntimePublicPathPlugin")
const WriteFilePlugin = require('write-file-webpack-plugin')
const FriendlyErrorsWebpackPlugin = require('friendly-errors-webpack-plugin');
const _ = require("lodash")
const localization = require("./localization")

const UglifyJsPlugin = require('uglifyjs-webpack-plugin')

const devMode = process.env.NODE_ENV != 'production';
const isHot = process.env.NODE_ENV === "hot";
const path = require('path')

const baseDir = path.resolve(__dirname, "../");
const join = path.join.bind(path, baseDir);

const hqViewsFolder = join("..", "Views")
const assetsPath = isHot ? "http://localhost:8080/" : "~/HqApp/dist/";

module.exports = function (appConfig) {
    // cacheDirectory improve development build greatly, but doesn't need on production build
    const babelLoader = devMode ? "babel-loader?cacheDirectory=true" : "babel-loader"

    // linking shared_vendor.dll file to output. Manifest path passed to html template
    const manifest = require("../dist/shared_vendor.manifest.json")
    const vendor_dll_hash = manifest.name.replace("shared_vendor_", "");

    // combine, group and merge all localization files into consumable form specific for each entry
    const localizationInfo = localization.buildLocalizationFiles(appConfig);

    const entryNames = Object.keys(appConfig) // [ 'hq', 'webinterview' ]

    const cssUseList = ['css-loader'];

    // using `clean-css` to minize css output for production builds
    if (!devMode) {
        cssUseList.push({
            loader: 'clean-css-loader',
            options: {
                compatibility: 'ie9',
                level: 2,
                inline: ['remote']
            }
        });
    }

    // extracting entry names and path to entry file from appconfig to webpack
    // output will look similiar to:
    // > { hq: './src/hqapp/main.js', webinterview: './src/webinterview/main.js' }
    const entry = entryNames.reduce((result, key) => { result[key] = appConfig[key].entry; return result; }, {})

    const webpackConfig = {
        entry,

        output: {
            path: join("dist"), // setting up webpack output to `dist` folder

            // for development we do not add any chunk info to make breakpoints work
            // for production each output file will have own hash in name for cache busting
            filename: devMode ? "[name].bundle.js" : "[name].bundle.[chunkhash].js",
            chunkFilename: devMode ? "[name].chunk.js" : "[name].chunk.[chunkhash].js",
            publicPath: ""
        },

        resolve: {
            unsafeCache: true, // performance related optimization
            extensions: ['.js', '.vue', '.json'],
            symlinks: false, // performance related optimization
            alias: {
                "~": join('src'),
                'vue$': 'vue/dist/vue.esm.js',
                moment$: 'moment/moment.js'
            }
        },

        stats: devMode ? "errors-only" : {
            chunks: false
        },

        devtool: 'source-map', // '#cheap-module-eval-source-map',

        module: {
            rules: [
                {
                    // parsing vue files, including only src folder for perf
                    test: /\.vue$/,
                    include: join("src"),
                    use: [{
                        loader: 'vue-loader',
                        options: {
                            loaders: { js: babelLoader }
                        }
                    }]
                }, {
                    // parsing js files, including only src folder for perf
                    test: /\.js$/,
                    include: join("src"),
                    use: [babelLoader]
                },

                { test: /\.css$/, use: ExtractTextPlugin.extract({ use: cssUseList }) },

                // disabled for now
                // , {
                //     test: /\.(js|vue)$/,
                //     loader: 'eslint-loader',
                //     enforce: 'pre',
                //     options: {
                //         formatter: require('eslint-friendly-formatter')
                //     }
                // }
            ].filter((x) => x)
        },

        plugins: _.flatten([
            new FriendlyErrorsWebpackPlugin(),
            // provide global accessible variables
            new webpack.ProvidePlugin({
                _: 'lodash',
                '$': "jquery",
                "jQuery": 'jquery',
                'moment': 'moment'
            }),

            // to make async chunks loading happen in any domain/supath combination,
            // we need to make sure, that webpack knows where our assets located
            new RuntimePublicPathPlugin({
                runtimePublicPath: "window.CONFIG.assetsPath"
            }),

            isHot ? new webpack.NamedModulesPlugin() : null,
            isHot ? new webpack.HotModuleReplacementPlugin() : null,

            // attaching shared_vendor.dll js file to our build
            new webpack.DllReferencePlugin({
                manifest
            }),

            new ExtractTextPlugin(`styles${devMode ? "" : ".[hash]"}.css`),

            // extracting all common stuff beetween hq/webinterview into separate chunk
            devMode ? null : new webpack.optimize.CommonsChunkPlugin({
                name: 'common'
            }),

            // build optimization stuff
            devMode ? null : new webpack.optimize.CommonsChunkPlugin({
                name: "manifest",
                minChunks: Infinity
            }),

            // build optimization stuff
            devMode ? null : new webpack.HashedModuleIdsPlugin(),

            // build optimization stuff
            devMode ? null : new webpack.optimize.ModuleConcatenationPlugin(),

            // minizing js files
            devMode ? null : new UglifyJsPlugin({
                sourceMap: true,
                cache: true,
				parallel:true,
                uglifyOptions: {                  
				  compress:true
				}
            }),

            // make sure that build will be optimized for production. Required by vuejs
            devMode ? null : new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify('production')
            }),

            // notify on build
            devMode ? new WebpackNotifierPlugin({ alwaysNotify: true }) : null,


            _.map(entryNames, (entryName) => {
                const viewsFolder = appConfig[entryName].appRootPath ? (appConfig[entryName].appRootPath + "/Views") : hqViewsFolder;

                // for each entry we produce separate partial cshtml file
                // this file will contain all required chunks
                return new HtmlWebpackPlugin({
                    inject: false,
                    filename: path.resolve(viewsFolder, "shared", `partial.${entryName}.cshtml`),

                    // we dont need webinterview chunk in hq output and vice versa
                    excludeChunks: entryNames.filter((name) => name !== entryName),
                    template: '!!pug-loader!.build/partial.template.pug',

                    // provide list of available locales to template and app
                    locales: localizationInfo[entryName],
                    entry: entryName,
                    cache: false,
                    // provide path to shared_vendor.dll
                    manifest: "shared_vendor.dll." + vendor_dll_hash + ".js",

                    assetsPath: appConfig[entryName].assetsPath || assetsPath,

                    isHot
                })
            }),

            isHot ? new WriteFilePlugin({ test: /\.cshtml$/, useHashIndex: true, force: true, log: true }) : null,

            // build stats
            devMode ? null : new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: 'stats.html',
                defaultSizes: 'gzip',
                openAnalyzer: false,
                statsOptions: { chunkModules: true, assets: true },
            })
        ]).filter(x => x != null),

        devServer: {
            contentBase: join("dist"),
            publicPath: "/headquarters/hqapp/dist/",
            hot: true,
            compress: false,

            headers: { "Access-Control-Allow-Origin": "*" },
            host: "localhost",
            port: 8080,
            quiet: true
        }
    };

    return webpackConfig;
}
