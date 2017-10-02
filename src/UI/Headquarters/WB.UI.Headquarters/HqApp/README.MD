Structure
=========

HQ.vendor.libs.js
-----------------

Collection of essential js libraries that used by all client side projects:

* `vue`, `vuex`, `vue-router` - ui part
* `jquery` - we want to get rid of it, but we can't 
* `axios` - all ajax should go through this lib, migrate from jquery
* ?`lodash` - full blown lodash( i guess it's better to not do)
* `i18next` - all client side libs will use localization via this lib
* `bootstrap` - bootstrap related js libs


Shared components
-----------------

Separate project that contains all shared vue components. this will. Other project should include it as dependency in `package.json`

```js
{
    "dependencies": {
        "wb-shared": "../shared"
    }
}
```




Notes
=====

We do not use `css-loader` - so any css include should be done through @alex






