import * as diffInMinutes from "date-fns/difference_in_minutes"
import modal from "shared/modal"
import store from "./store"
import Vue from "vue"

class IdleTimeouter {
    constructor() {
        this.shown = false
    }
     show() {
        if (!this.shown) {
            this.shown = true
            store.dispatch("stop")
            modal.alert({
                title: Vue.$t("SessionTimeoutTitle"),
                message: `<p>${Vue.$t('SessionTimeoutMessageTitle')}</p><p>${Vue.$t('SessionTimeoutMessage')}</p>`,
                callback: () => {
                    location.reload()
                },
                onEscape: false,
                closeButton: false,
                buttons: {
                    ok: {
                        label: Vue.$t("Reload"),
                        className: "btn-success"
                    }
                }
            })
        }
    }

     start() {
        setInterval(() => {
            if (!this.shown) {
                const minutesAfterLastAction = diffInMinutes(new Date(), (store.state ).lastActivityTimestamp)

                if (Math.abs(minutesAfterLastAction) > 15) {
                    this.show()
                }
            }
        }, 15 * 1000)
    }
}

export default new IdleTimeouter()
