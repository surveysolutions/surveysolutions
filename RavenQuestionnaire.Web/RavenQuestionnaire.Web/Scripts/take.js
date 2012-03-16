$(document).ready(function () {
    $(".alert .close").live("click",function () {
        var id = $(this).parent().attr('close-marker');
        $("[close-marker=" + id + "]").alert('close');
    });
});

function ShowOverlay() {
    $('#loadingModal').addClass('overlay');
    $('.modal-backdrop').remove();
}
function HideOverlay() {
    $('#loadingModal').removeClass('overlay');
}