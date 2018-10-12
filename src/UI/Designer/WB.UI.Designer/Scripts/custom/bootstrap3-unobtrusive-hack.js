$(document).ready(function () {
    $('form').each(function () {
        var formData = $.data(this),
            settings = formData.validator.settings,
            // Store existing event handlers in local variables
            oldErrorPlacement = settings.errorPlacement,
            oldSuccess = settings.success;

        settings.errorPlacement = function (label, element) {

            // Call old handler so it can update the HTML
            oldErrorPlacement(label, element);

            // Add Bootstrap classes to newly added elements
            label.parents('.form-group').addClass('has-error');
            label.parents('.form-group').find('input').addClass('has-error');
            label.addClass('text-danger');
        };

        settings.success = function (label) {
            // Remove error class from <div class="form-group">, but don't worry about
            // validation error messages as the plugin is going to remove it anyway
            label.parents('.form-group').removeClass('has-error');
            label.parents('.form-group').find('input').removeClass('has-error');
            // Call old handler to do rest of the work
            oldSuccess(label);
        };
    });
});
