angular-pines-notify
====================

This is a simple wrapper for [pines-notify](http://sciactive.com/pnotify/) as an
[AngularJS](http://angularjs.org/) service. This service provides several helper methods
to display notifications on web applications.

### Requirements

- [JQuery](http://jquery.com/)
- [Pines Notify](http://sciactive.com/pnotify/)
- [AngularJS](http://angularjs.org/)

Optionally it can use [Twitter Bootstrap](http://getbootstrap.com) or [jqueryui](http://jqueryui.com)
for theming the notifications.

### Demo

`$ git clone https://github.com/mykabam/angular-pines-notify.git`
`$ cd angular-pines-notify`
`$ bower install angular-pines-notify`

browser open `app/demo.html`

### Setup

We use [bower](https://github.com/bower/bower) for dependency management. Install angular-pines-notify
into your project by running the command

`$ bower install angular-pines-notify`

If you use a `bower.json` file in your project, you can have Bower save angular-pines-notify as a dependency
by passing the `--save` or `--save-dev` flag with the above command.

This will copy the angular-pines-notify files into your `bower_components` folder, along with its dependencies.
Load the script files in your application:

```html
<link rel="stylesheet" type="text/css" href="bower_components/pines-notify/pnotify.core.css" />
<link rel="stylesheet" type="text/css" href="bower_components/pines-notify/pnotify.buttons.css" />
<link rel="stylesheet" type="text/css" href="bower_components/pines-notify/pnotify.picon.css" />

<script type="text/javascript" src="bower_components/jquery/jquery.js"></script>
<script src="bower_components/pines-notify/pnotify.core.js"></script>
<script src="bower_components/pines-notify/pnotify.buttons.js"></script>
<script src="bower_components/pines-notify/pnotify.callbacks.js"></script>
<script src="bower_components/pines-notify/pnotify.confirm.js"></script>
<script src="bower_components/pines-notify/pnotify.desktop.js"></script>
<script src="bower_components/pines-notify/pnotify.history.js"></script>
<script src="bower_components/pines-notify/pnotify.nonblock.js"></script>
<script type="text/javascript" src="bower_components/angular/angular.js"></script>
<script type="text/javascript" src="bower_components/angular-pines-notify/src/pnotify.js"></script>
```

(Note that `jquery` must be loaded before `angular` so that it doesn't use `jqLite` internally)

### Usage

Add the angular-pines-notify module as a dependency to your application module:

```javascript
var myAppModule = angular.module('MyApp', ['ui.notify']);
```

In order to use the API you need to inject the `notificationService` service into
your controllers. From there you can use one of the many different notifications
like:

 * info
 * notice
 * error
 * success

You can use these methods with the following line of code

 * `notificationService.info(content);`
 * `notificationService.notice(content);`
 * `notificationService.error(content);`
 * `notificationService.success(content);`

Or you can also use a generic notify method with more customization
by passing the pines notify's options object:

`notificationService.notify(options);`

For example:

```javascript
myAppModule.controller(
  'MyCtrl',
  [
    '$scope', notificationService,
    function($scope, notificationService) {
      $scope.action = function() {
        // This is a sample using the success helper method
        notificationService.success('This is a success notification');
      };

      $scope.anotherAction = function() {
        // This is a sample using the generic pines notification object
        notificationService.notify({
          title: 'Notice Title',
          text: 'Notice Text',
          hide: false
        });
      };
    }
  ]
);
```

You can use the provider to set defaults for all your notifications:

```javascript
myAppModule.config(['notificationServiceProvider', function(notificationServiceProvider) {

  notificationServiceProvider.setDefaults({
    history: false,
    delay: 4000,
    styling: 'bootstrap',
 	  closer: false,
	  closer_hover: false
  });

}]);
```

### Options

All the pines-notify options can be passed through the notify functions.
You can read more about the supported list of options and what they do on the
[Pines Notify Github Page](https://github.com/sciactive/pnotify)

### Coming Soon

 * Tests
 * Configuration helpers
