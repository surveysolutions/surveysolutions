var GulpSelenium = require('gulp-selenium');
var gulpSelenium = GulpSelenium();

exports.config = {
  seleniumServerJar: gulpSelenium.path,
  chromeDriver: gulpSelenium.chromeDriverPath,
  //seleniumAddress: 'http://localhost:4444/wd/hub', // Using JAR instead of address
  capabilities: {
    //'browserName': 'phantomjs', // Can't use phantomjs until this is fixed
    // https://github.com/detro/ghostdriver/issues/125
    //'browserName': 'firefox',
    'browserName': 'chrome'
  },
  specs: ['test/ui/**/*.spec.js']
};

console.log(gulpSelenium.chromeDriverPath);
