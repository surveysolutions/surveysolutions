import config from "~/shared/config"
import * as $script from "scriptjs"

import Vue from 'vue'
import "signalr"

// wraps jQuery promises into awaitable ES 2016 Promise
const wrap = (jqueryPromise) => {
    return new Promise((res, rej) =>
        jqueryPromise
            .done(data => res(data))
            .fail(error => rej(error))
    )
}

function Connector(store, interviewId, connected) {
    let hub = null;

    async function connect() {
        async function changeSection(sectionId) {
            try {
                const state = jQuery.signalR[config.hubName].state
                const oldSectionId = state.sectionId
                state.sectionId = sectionId

                await wrap(hub.server.changeSection(sectionId, oldSectionId))
            } catch (err) {
                store.dispatch("UNHANDLED_ERROR", err)
            }
        }

        if (!Vue.hasOwnProperty("$api")) {
            Vue.$api = {}
        }

        Object.defineProperty(Vue.$api, "hub", {
            get() { return { changeSection } }
        })

        $script(config.signalrPath, signalrScriptLoaded);
    }

    async function signalrScriptLoaded() {
        const interviewProxy = $.connection[config.hubName]

        interviewProxy.client.reloadInterview = () => {
            store.dispatch("reloadInterview")
        }

        interviewProxy.client.closeInterview = () => {
            if (store.getters.isReviewMode === true)
                return
            store.dispatch("closeInterview")
            store.dispatch("stop")
        }

        interviewProxy.client.shutDown = () => {
            store.dispatch("shutDownInterview")
        }

        interviewProxy.client.finishInterview = () => {
            store.dispatch("finishInterview")
        }

        interviewProxy.client.refreshEntities = (questions) => {
            store.dispatch("refreshEntities", questions)
        }

        interviewProxy.client.refreshSection = () => {
            store.dispatch("fetchSectionEntities")          // fetching entities in section
            store.dispatch("refreshSectionState")           // fetching breadcrumbs/sidebar/buttons
        }

        interviewProxy.client.refreshSectionState = () => {
            store.dispatch("refreshSectionState")           // fetching breadcrumbs/sidebar/buttons
        }

        interviewProxy.client.markAnswerAsNotSaved = (id, message) => {
            store.dispatch("fetch", { id, done: true })
            store.dispatch("setAnswerAsNotSaved", { id, message })
        }

        await hubStarter();
    }

    async function hubStarter(options) {
        const queryString = {
            interviewId,
            appVersion: config.appVersion
        }

        if (queryString.interviewId == null && options != null) {
            queryString.interviewId = options.interviewId;
        }

        if(store.getters.isReviewMode) {
            queryString.review = true
        }

        $.connection.hub.qs = _.assign(options, queryString);

        // { transport: supportedTransports }
        $.signalR.hub.start({ transport: config.supportedTransports }).then(() => {
            // transport: "longPolling"
            $.connection.hub.connectionSlow(() => {
                store.dispatch("connectionSlow")
            })

            $.connection.hub.reconnecting(() => {
                store.dispatch("tryingToReconnect", true)
            })

            $.connection.hub.reconnected(() => {
                store.dispatch("tryingToReconnect", false)
            })

            $.connection.hub.disconnected(() => {
                store.dispatch("disconnected")
            })

            hub = jQuery.signalR[config.hubName]
            connected();
        })
    }

    return {
        hubStarter, connect
    }
}

export default {
    name: "old-signal-connection",

    data() {
        return {
            hubInstance: null
        }
    },

    mounted() {
        var connector = Connector(this.$store, this.interviewId, () => this.$emit("connected"));
        connector.connect()
    },

    props: {
        interviewId: {
            type: String,
            required: true
        }
    },

    render() { return null }
}
