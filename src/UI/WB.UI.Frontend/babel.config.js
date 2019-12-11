module.exports = {
  presets: [
    ['@vue/app', {
      polyfills: [
        'es6.object.assign',
        'es6.promise'
      ]
    }]
  ]
};
