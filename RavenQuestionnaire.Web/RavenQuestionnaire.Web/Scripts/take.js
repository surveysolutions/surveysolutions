$(document).ready(function () {
    $(".alert .close").live("click", function () {
        var id = $(this).parent().attr('close-marker');
        $("[close-marker=" + id + "]").alert('close');
    });
});

function ShowOverlay() {
    $('#loadingModal').addClass('overlay');
    $('.modal-backdrop').remove();
}
function MultySelectChangeStyles(input) {
    if (input.checked)
        $(input).addClass('btn-primary');
    else
        $(input).removeClass('btn-primary');

}

function HideOverlay() {
    $('#loadingModal').removeClass('overlay');
}

$.validator.addMethod(
    "sum",
    function (value, element, params) {
        var sumOfVals = 0;
        var parent = $(element).parents(".question");
        var selector = $(element).attr('group');
        $(parent).find("input[group=" + selector + "]").each(function () {
            sumOfVals = sumOfVals + parseInt($(this).val(), 10);
        });
        if (sumOfVals == params) return true;
        return false;
    },
    jQuery.format("Sum must be {0}")
);