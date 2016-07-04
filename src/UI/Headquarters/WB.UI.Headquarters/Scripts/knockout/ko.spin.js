(function(root, factory) {
	if (typeof define === 'function' && define.amd) {
		// AMD. Register as an anonymous module.
		define(["jquery", "knockout", "spin"], factory);
	} else {
		// Browser globals
		factory(jQuery, ko, Spinner);
	}
}(this, function ($, ko, Spinner) {
	ko.bindingHandlers.spinner = {
		init: function (element, valueAccessor, allBindings) {
			// initialize the object which will return the eventual spinner
			var deferred = $.Deferred();
			element.spinner = deferred.promise();

			// force this to the bottom of the event queue in the rendering thread,
			// so we can get the computed color of the containing element after other bindings
			// (e.g. class, style) have evalutated.
			// add some more delay as the class bindings of the parent fire asynchronously.
			setTimeout(function () {
				var options = {};
				options.color = $(element).css("color");
				$.extend(options, ko.bindingHandlers.spinner.defaultOptions, ko.unwrap(allBindings.get("spinnerOptions")));

				deferred.resolve(new Spinner(options));
			}, 30);
		},
		update: function (element, valueAccessor, allBindingsAccessor) {
			// when the spinner exists, pick up the existing one and call appropriate methods on it

			var result = ko.unwrap(valueAccessor());

			element.spinner.done(function (spinner) {
				var isSpinning = result;
				if (isSpinning) {
					$(element).show();
					spinner.spin(element);
				} else {
					if (spinner.el) { //don't stop first time
						spinner.stop();
					}

					$(element).hide();
				}
			});
		},
		defaultOptions: {
			lines: 19,
			length: 0,
			width: 2,
			corners: 0,
			radius: 5,
			speed: 2.5,
			trail: 20
		}
	};
}));
