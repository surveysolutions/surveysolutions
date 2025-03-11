import './jquery'
import { Modal } from 'bootstrap';
import sanitizeHtml from 'sanitize-html';

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
                    ok: { label: this.translations.OK, className: 'btn-primary', callback: resolve }
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
                    confirm: { label: this.translations.CONFIRM, className: 'btn-primary', callback: () => resolve(true) },
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
                    ok: {
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

    showModal({ title = '', message = '', buttons = {}, closeButton = true, size = 'default', onEscape = true, onShow = null }) {
        if (typeof buttons !== 'object' || buttons === null) {
            console.error('Error: buttons must be an object');
            buttons = {};
        }

        let sizeClass = '';
        if (size === 'small') sizeClass = 'modal-sm';
        else if (size === 'large') sizeClass = 'modal-lg';
        else if (size === 'extra-large') sizeClass = 'modal-xl';

        let buttonsHTML = Object.entries(buttons).map(([key, btn]) => 
            `<button type="button" class="btn btn-default ${btn.className ?? (key == 'ok' || key == 'success' ? 'btn-primary' : 'btn-secondary')}" id="modal-btn-${key}">${sanitizeHtml(btn.label)}</button>`
        ).join('');

        let modalHTML = `
      <div class="modal fade" id="customModal" tabindex="-1" ${onEscape ? '' : 'data-bs-keyboard="false"'}>
        <div class="modal-dialog ${sizeClass}">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">${sanitizeHtml(title)}</h5>
              ${closeButton ? '<button type="button" class="bootbox-close-button close btn-close" data-bs-dismiss="modal" aria-label="Close"></button>' : ''}
            </div>
            <div class="modal-body">${sanitizeHtml(message, { allowedTags: ['b', 'i', 'strong', 'em', 'p', 'ul', 'li', 'br'] })}</div>
            <div class="modal-footer">${buttonsHTML}</div>
          </div>
        </div>
      </div>`;

        const existingModal = document.getElementById('customModal');
        if (existingModal) {
            existingModal.remove();
        }

        document.body.insertAdjacentHTML('beforeend', modalHTML);
        const modalElement = document.getElementById('customModal');
        const modalInstance = new Modal(modalElement, { 
            keyboard: onEscape,
            backdrop: onEscape ? true : 'static'
         });

        Object.entries(buttons).forEach(([key, btn]) => {
            document.getElementById(`modal-btn-${key}`).addEventListener('click', () => {
                if (typeof btn.callback === 'function') {
                    btn.callback();
                }
                modalInstance.hide();
            });
        });

        modalElement.addEventListener('shown.bs.modal', () => {
            if (typeof onShow === 'function') {
                onShow();
            }
        });

        modalInstance.show();
    }
};