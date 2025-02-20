import { Modal } from 'bootstrap';

export default {
    init(i18n, locale) {
        this.locale = locale;
        this.translations = {
            OK: i18n.t('Common.Ok'),
            CANCEL: i18n.t('Common.Cancel'),
            CONFIRM: i18n.t('Common.Confirm'),
        };
    },

    alert(message) {
        return new Promise((resolve) => {
            this.showModal({
                title: '',
                message,
                buttons: {
                    success: { label: this.translations.OK, className: 'btn-primary', callback: resolve }
                }
            });
        });
    },

    confirm(message) {
        return new Promise((resolve) => {
            this.showModal({
                title: '',
                message,
                buttons: {
                    success: { label: this.translations.CONFIRM, className: 'btn-primary', callback: () => resolve(true) },
                    cancel: { label: this.translations.CANCEL, className: 'btn-secondary', callback: () => resolve(false) }
                }
            });
        });
    },

    prompt(message) {
        return new Promise((resolve) => {
            this.showModal({
                title: '',
                message: `<input type="text" class="form-control" id="promptInput">`,
                buttons: {
                    success: {
                        label: this.translations.OK,
                        className: 'btn-primary',
                        callback: () => {
                            const inputValue = document.getElementById('promptInput').value;
                            resolve(inputValue);
                        }
                    },
                    cancel: { label: this.translations.CANCEL, className: 'btn-secondary', callback: () => resolve(null) }
                }
            });
        });
    },

    dialog(options) {
        return new Promise((resolve) => {
            this.showModal(options);
        });
    },

    showModal({ title = '', message = '', buttons = {}, closeButton = true, size = 'default', onShow = null }) {
        if (typeof buttons !== 'object' || buttons === null) {
            console.error('Error: buttons must be an object with success and/or cancel properties');
            buttons = {};
        }

        let sizeClass = '';
        if (size === 'small') sizeClass = 'modal-sm';
        else if (size === 'large') sizeClass = 'modal-lg';
        else if (size === 'extra-large') sizeClass = 'modal-xl';

        let modalHTML = `
      <div class="modal fade" id="customModal" tabindex="-1">
        <div class="modal-dialog ${sizeClass}">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">${title}</h5>
              ${closeButton ? '<button type="button" class="bootbox-close-button close btn-close" data-bs-dismiss="modal" aria-label="Close"></button>' : ''}
            </div>
            <div class="modal-body">${message}</div>
            <div class="modal-footer">
              ${buttons.cancel ? `<button type="button" class="btn ${buttons.cancel.className ?? 'btn-secondary'}" id="modal-btn-cancel">${buttons.cancel.label}</button>` : ''}
              ${buttons.success ? `<button type="button" class="btn ${buttons.success.className ?? 'btn-primary'}" id="modal-btn-success">${buttons.success.label}</button>` : ''}
            </div>
          </div>
        </div>
      </div>`;

        const existingModal = document.getElementById('customModal');
        if (existingModal) {
            existingModal.remove();
        }

        document.body.insertAdjacentHTML('beforeend', modalHTML);
        const modalElement = document.getElementById('customModal');
        const modalInstance = new Modal(modalElement);

        if (buttons.success) {
            document.getElementById(`modal-btn-success`).addEventListener('click', () => {
                if (typeof buttons.success.callback === 'function') {
                    buttons.success.callback();
                }
                modalInstance.hide();
            });
        }

        if (buttons.cancel) {
            document.getElementById(`modal-btn-cancel`).addEventListener('click', () => {
                if (typeof buttons.cancel.callback === 'function') {
                    buttons.cancel.callback();
                }
                modalInstance.hide();
            });
        }

        modalElement.addEventListener('shown.bs.modal', () => {
            if (typeof onShow === 'function') {
                onShow();
            }
        });

        modalInstance.show();
    }
};