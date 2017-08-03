import * as diffInMinutes from "date-fns/difference_in_minutes"
import modal from "./modal"
import store from "./store"

class IdleTimeouter {
    constructor() {
        this.shown = false
    }
     show() {
        if (!this.shown) {
            this.shown = true
            store.dispatch("stop")
            modal.alert({
                title: "Your session has timed out",
                message: "<p>Your session has timed out because you didn't do any action for 15 minutes.</p>"
                + "<p>Please reload the page to continue this interview.</p>",
                callback: () => {
                    location.reload()
                },
                onEscape: false,
                closeButton: false,
                buttons: {
                    ok: {
                        label: "Reload",
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
