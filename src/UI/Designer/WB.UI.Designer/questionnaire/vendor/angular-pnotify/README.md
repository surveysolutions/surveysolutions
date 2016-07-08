angular-pnotify
====================

This is a simple wrapper for [PNotify](http://sciactive.com/pnotify/) as a [AngularJS](http://angularjs.org/) service. 
This service provides several helper methods to display notifications on web applications.

A PNotify 2.0 port heavily based on [angular-pines-notify](https://github.com/mykabam/angular-pines-notify) project. 

### Dependencies

- [JQuery](http://jquery.com/)
- [PNotify](http://sciactive.com/pnotify/)
- [AngularJS](http://angularjs.org/)

#### Optional

Need to use at least Bootstrap 3 or jQuery UI to make pretty notifications.

- [Bootstrap 3](http://getbootstrap.com)
- [jQuery UI](http://http://jqueryui.com)
- [FontAwesome](http://http://fontawesome.io)

### Demo

[Check out the demo here!](http://pnotify.jlareau.com/demo/)

### Install

`bower install angular-pnotify`

### Usage

Include at least Bootstrap 3 or jQuery UI CSS.

Include PNotify related assets. You need to include at least pnotify.core.css and pnotify.core.js.
Don't forget pnotify.confirm.js if you need confirmation dialogs.

Then add angular-pnotify.js.

Here is an example using Bootstrap 3.

```html
<link rel="stylesheet" href="bower_components/bootstrap/dist/css/bootstrap.min.css">
<link rel="stylesheet" href="bower_components/pnotify/pnotify.core.css">
<link rel="stylesheet" href="bower_components/pnotify/pnotify.buttons.css">

<script src="bower_components/jquery/dist/jquery.min.js"></script>
<script src="bower_components/angular/angular.js"></script>
<script src="bower_components/pnotify/pnotify.core.js"></script>
<script src="bower_components/pnotify/pnotify.confirm.js"></script>
<script src="bower_components/pnotify/pnotify.buttons.js"></script>
<script src="bower_components/angular-pnotify/src/angular-pnotify.js"></script>
```

Add the angular-pnotify module as a dependency to your application module:

```javascript
angular.module('MyApp', ['jlareau.pnotify']);
```

In order to use the API you need to inject the `notificationService` service into your controllers. 

```javascript
angular.module('MyApp')
	.controller('MyCtrl', ['$scope', 'notificationService', function($scope, notificationService) {
		$scope.action = function() {
			notificationService.success('Success!!!');
		};
	}])
;
```

### Methods

#### info

Display an info notice.

`notificationService.info(text, [stack name]);`

#### notice

Display notification as a notice.

`notificationService.notice(text, [stack name]);`

#### error

Display an error notice.

`notificationService.error(text, [stack name]);`

#### success

Display a success notice.

`notificationService.success(text, [stack name]);`

#### notify

Display a generic PNotify notification with the options you pass to it. It ignores defaults.

`notificationService.notify(options);`

#### notifyWithDefaults

Same as `notify` but will merge the options with defaults. Useful when you only want to override or add one options, without 
clearing all defaults.

`notificationService.notifyWithDefaults(options);`

### Provider

Sometimes you want to set defaults for the whole application. You can do so in your module's config.

```javascript
angular.module('MyApp')
	.config(['notificationServiceProvider', function(notificationServiceProvider) {

		notificationServiceProvider.setDefaults({
			history: false,
			delay: 4000,
			closer: false,
			closer_hover: false
		});

	}])
;
```

You can also chain configuration calls:

```javascript
angular.module('MyApp')
	.config(['notificationServiceProvider', function(notificationServiceProvider) {

		notificationServiceProvider
		
			.setDefaults({
				history: false,
				delay: 4000,
				closer: false,
				closer_hover: false
			})
		
			// Configure a stack named 'bottom_right' that append a call 'stack-bottomright'
			.setStack('bottom_right', 'stack-bottomright', {
				dir1: 'up',
				dir2: 'left',
				firstpos1: 25,
				firstpos2: 25
			})

			// Configure a stack named 'top_left' that append a call 'stack-topleft'
			.setStack('top_left', 'stack-topleft', {
				dir1: 'down',
				dir2: 'right',
				push: 'top'
			})
			
		;

	}])
;
```


### PNotify Stacks

You can set the position and direction of notifications by using PNotify stacks. You can add stack information to the following methods: 

* info
* notice
* error
* success

You need to define the stacks in the config section before:

```javascript
angular.module('MyApp')
	.config(['notificationServiceProvider', function(notificationServiceProvider) {

		// Configure a stack named 'top_left' that append a call 'stack-topleft'
		notificationServiceProvider.setStack('top_left', 'stack-topleft', {
			dir1: 'down',
			dir2: 'right',
			push: 'top'
		}); 

	}])
;
```

Later, in a controller:

```javascript
notificationService.info('Hello World : Top left', 'top_left');
```

You can also set a defined stack as the default:

```javascript
notificationServiceProvider.setDefaultStack('top_left');
```

### Examples

```javascript
angular.module('MyApp')
	.controller('MyCtrl', ['$scope', 'notificationService', function($scope, notificationService) {

		$scope.action = function() {
			// This is a sample using the success helper method
			notificationService.success('This is a success notification');
		};

		$scope.anotherAction = function() {
			// This is a sample using the generic PNotify notification object
			notificationService.notify({
				title: 'Notice Title',
				text: 'Notice Text',
				hide: false
			});
		};

	}])
;
```

### Options

All the PNotify options can be passed through the notify functions.
You can read more about the supported list of options and what they do on the
[PNotify Github Page](https://github.com/sciactive/pnotify)

### Thanks

To the [angular-pines-notify](https://github.com/mykabam/angular-pines-notify) contributors:

- [mykabam](https://github.com/mykabam)
- [valmy](https://github.com/valmy)
- [mibamur](https://github.com/mibamur)
- [MaximilianoRicoTabo](https://github.com/MaximilianoRicoTabo)

Thanks to [mehdi-ghezal](https://github.com/mehdi-ghezal) for stack implementation.