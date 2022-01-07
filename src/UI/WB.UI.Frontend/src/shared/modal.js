import Vue from 'vue'
import 'bootstrap/js/modal'
import Swal from 'sweetalert2'

export default {
    dialog(options) {

        Swal.fire({
            title: options.title,
            html: options.message,

            showConfirmButton: options.showConfirmButton ?? true,
            confirmButtonText: options.confirmButtonText ?? Vue.$t('Common.Ok'),

            buttonsStyling: false,
            allowEscapeKey: options.onEscape ?? false,
            allowOutsideClick : options.onEscape ?? false,
            customClass:{
                confirmButton: 'btn btn-primary ' + (options.confirmButtonClassName ?? 'btn-success'),
                cancelButton:'btn btn-link',
                footer: 'modal-footer',
                popup: 'modal-content',
                header: 'modal-header',
                actions: 'modal-actions',
                htmlContainer: 'modal-body',
            },
            width: 600,
            showCancelButton: options.showCancelButton ?? true,
            cancelButtonText: options.cancelButtonText ?? Vue.$t('Common.Cancel'),

            showCloseButton: options.closeButton ?? true,

        }).then((result) => {
            if (result.isConfirmed) {
                if (options.confirmCallback) {
                    options.confirmCallback.call(this)
                }
            }
            else{
                if (options.cancelCallback) {
                    options.cancelCallback.call(this)
                }
            }
        })
    },

    confirm(message, callback) {
        Swal.fire({
            title: message,
            confirmButtonText: Vue.$t('Common.Ok'),
            buttonsStyling: false,
            customClass:{
                confirmButton: 'btn btn-primary btn-success',
                cancelButton:'btn btn-link',
                footer: 'modal-footer',
                popup: 'modal-content',
                header: 'modal-header',
                actions: 'modal-actions',
                htmlContainer: 'modal-body',
            },
            cancelButtonText: Vue.$t('Common.Cancel'),
            width: 600,
        }).then((result) => {
            if (callback) {
                callback.call(result.isConfirmed)
            }})
    },

    async prompt(options){

        const { value: email } = await Swal.fire({
            title: options.title,
            input: 'email',
            inputLabel: options.inputLabel,
            inputPlaceholder: options.inputPlaceholder,
            customClass:{
                confirmButton: 'btn btn-primary ' + options.confirmButtonClassName ?? 'btn-success',
                cancelButton:'btn btn-link',
                footer: 'modal-footer',
                popup: 'modal-content',
                header: 'modal-header',
                actions: 'modal-actions',
                htmlContainer: 'modal-body',
            },
            width: 600,
            confirmButtonText: options.confirmButtonText ?? Vue.$t('Common.Ok'),
            showCloseButton: options.closeButton ?? true,
            showCancelButton: options.showCancelButton ?? true,
            cancelButtonText: options.cancelButtonText ?? Vue.$t('Common.Cancel'),
        })

        if (email) {
            if (options.confirmCallback ) {
                options.confirmCallback.call(email)
            }
        }
    },
}
