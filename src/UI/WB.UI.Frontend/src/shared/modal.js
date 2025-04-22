import './jquery';
import { Modal } from 'bootstrap';
import sanitizeHtml from 'sanitize-html';
import { newGuid } from './guid';

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

    confirm(message, resultCallback) {
        var callbackWasCalled = false;
        this.showModal({
            title: '',
            message,
            buttons: {
                confirm: {
                    label: this.translations.CONFIRM, className: 'btn-primary', callback: () => {
                        callbackWasCalled = true;
                        resultCallback(true)
                    }
                },
                cancel: {
                    label: this.translations.CANCEL, className: 'btn-secondary', callback: () => {
                        callbackWasCalled = true;
                        resultCallback(false)
                    }
                }
            },
            closeButton: true,
            onClose: () => {
                if (!callbackWasCalled)
                    resultCallback(false)
            }
        });
    },

    dialog(options) {
        return new Promise((resolve) => {
            this.showModal(options);
        });
    },

    showModal({ title = '', message = '', buttons = {}, closeButton = true, size = 'default', onEscape = true, onShow = null, onClose = null }) {
        if (typeof buttons !== 'object' || buttons === null) {
            console.error('Error: buttons must be an object');
            buttons = {};
        }

        const modalId = `customModal-${newGuid()}`;

        let sizeClass = '';
        if (size === 'small') sizeClass = 'modal-sm';
        else if (size === 'large') sizeClass = 'modal-lg';
        else if (size === 'extra-large') sizeClass = 'modal-xl';

        let buttonsHTML = Object.entries(buttons).map(([key, btn]) =>
            `<button type="button" class="btn btn-default ${btn.className ?? (key == 'ok' || key == 'success' ? 'btn-primary' : 'btn-secondary')}" id="${modalId}-btn-${key}">${this.processText(btn.label)}</button>`
        ).join('');

        let titleHTML = title ? `<h5 class="modal-title">${this.processText(title)}</h5>` : ''; // Render title only if provided

        let headerHTML = (title || closeButton) ? `
        <div class="modal-header">
          ${titleHTML}
          ${closeButton ? '<button type="button" class="bootbox-close-button close btn-close" data-bs-dismiss="modal" aria-label="Close"></button>' : ''}
        </div>` : ''; // Render header only if title or closeButton is provided

        let modalHTML = `
      <div class="modal fade" id="${modalId}" tabindex="-1" ${onEscape ? '' : 'data-bs-keyboard="false"'} aria-hidden="true">
        <div class="modal-dialog ${sizeClass}">
          <div class="modal-content">
            ${headerHTML}
            <div class="modal-body">${this.processText(message)}</div>
            <div class="modal-footer">${buttonsHTML}</div>
          </div>
        </div>
      </div>`;

        document.body.insertAdjacentHTML('beforeend', modalHTML);
        const modalElement = document.getElementById(modalId);
        const modalInstance = new Modal(modalElement, {
            keyboard: onEscape,
            backdrop: onEscape ? true : 'static'
        });

        const buttonListeners = [];
        Object.entries(buttons).forEach(([key, btn]) => {
            const buttonElement = document.getElementById(`${modalId}-btn-${key}`);
            const listener = () => {
                if (typeof btn.callback === 'function') {
                    btn.callback();
                }
                modalInstance.hide();
            };
            buttonElement.addEventListener('click', listener);
            buttonListeners.push({ element: buttonElement, listener });
        });

        const onShownListener = () => {
            if (typeof onShow === 'function') {
                onShow(modalElement);
            }
        };

        const onHiddenListener = () => {
            if (typeof onClose === 'function') {
                onClose();
            }

            // Remove event listeners and modal element
            buttonListeners.forEach(({ element, listener }) => {
                element.removeEventListener('click', listener);
            });
            modalElement.removeEventListener('shown.bs.modal', onShownListener);
            modalElement.removeEventListener('hidden.bs.modal', onHiddenListener);
            modalElement.remove();
        };

        modalElement.addEventListener('shown.bs.modal', onShownListener);
        modalElement.addEventListener('hidden.bs.modal', onHiddenListener);

        modalInstance.show();
    },

    processText(message) {
        return sanitizeHtml(message, { allowedTags: ['b', 'i', 'strong', 'em', 'p', 'ul', 'li', 'br'] });
    }
};