import * as signalR from '@microsoft/signalr'
import Vue from 'vue'

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
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(Vue.$config.basePath + `interview?interviewId=${this.interviewId}&mode=${this.mode}`)
            .withAutomaticReconnect()
            .build()

        const api = {
            changeSection(to, from) {
                return connection.send('changeSection', to, from)
            },
            stop() {
                return connection.stop()
            },
        }

        if (!Object.prototype.hasOwnProperty.call(Vue, '$api')) {
            Vue.$api = {}
        }

        Object.defineProperty(Vue.$api, 'hub', {
            get() { return api },
        })

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
            this.$store.dispatch('setAnswerAsNotSaved', { id, message })
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
