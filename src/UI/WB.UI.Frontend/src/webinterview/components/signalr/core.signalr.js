import * as signalR from '@microsoft/signalr'

let connection = null

export const hubApi = {
    changeSection(to, from) {
        return connection.send('changeSection', to, from)
    },
    stop() {
        return connection.stop()
    },
}

export default {
    name: 'wb-communicator',

    props: {
        interviewId: {
            type: String,
            required: true,
        },
        mode: {
            type: String,
            required: true,
        },
    },

    beforeMount() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(this.$config.basePath + `interview?interviewId=${this.interviewId}&mode=${this.mode}`)
            .withAutomaticReconnect()
            .build()



        //this.$api.hub = hubApi;

        /*const app = this.$root
        if (!Object.prototype.hasOwnProperty.call(app.config.globalProperties, '$api')) {
            app.config.globalProperties.$api = {}
        }

        Object.defineProperty(app.config.globalProperties.$api, 'hub', {
            get() { return api },
        })*/

        connection.on('refreshEntities', (questions) => {
            this.$store.dispatch('refreshEntities', questions)
        })

        connection.on('refreshSection', () => {
            this.$store.dispatch('fetchSectionEntities')          // fetching entities in section
            this.$store.dispatch('refreshSectionState')           // fetching breadcrumbs/sidebar/buttons
        })

        connection.on('refreshSectionState', () => {
            this.$store.dispatch('refreshSectionState')           // fetching breadcrumbs/sidebar/buttons
        })

        connection.on('markAnswerAsNotSaved', (id, message) => {
            this.$store.dispatch('fetch', { id, done: true })
            this.$store.dispatch('setAnswerAsNotValid', { id, message })
        })

        connection.on('reloadInterview', () => {
            this.$store.dispatch('reloadInterview')
        })

        connection.on('closeInterview', () => {
            if (this.$store.getters.isReviewMode === true)
                return
            this.$store.dispatch('closeInterview')
            this.$store.dispatch('stop')
        })

        connection.on('shutDown', () => {
            this.$store.dispatch('shutDownInterview')
        })

        connection.on('finishInterview', () => {
            this.$store.dispatch('finishInterview')
        })

        connection.start()
            .then(() => this.$emit('connected'))
            .catch(err => document.write(err))
    },

    render() { return null },
}
