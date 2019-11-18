import * as signalR from "@microsoft/signalr";
import config from "~/shared/config"
import axios from 'axios'
import Vue from 'vue'

export default {
    name: 'wb-communicator',

    beforeMount() {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/interview?interviewId=" + this.$route.params.interviewId)
            .build();

        const api = {
            get: this.apiGet,

            changeSection(to) {
                return connection.send("changeSection", to)
            }
        };

        Object.defineProperty(Vue, "$api", {
            get: function () { return api; }
        })

        connection.on("refreshEntities", (questions) => {
            this.$store.dispatch("refreshEntities", questions)
        })

        connection.on("refreshSection", () => {
            this.$store.dispatch("fetchSectionEntities")          // fetching entities in section
            this.$store.dispatch("refreshSectionState")           // fetching breadcrumbs/sidebar/buttons
        })

        connection.on("refreshSectionState", () => {
            this.$store.dispatch("refreshSectionState")           // fetching breadcrumbs/sidebar/buttons
        })

        connection.on("markAnswerAsNotSaved", (id, message) => {
            this.$store.dispatch("fetchProgress", -1)
            this.$store.dispatch("fetch", { id, done: true })
            this.$store.dispatch("setAnswerAsNotSaved", { id, message })
        })

        connection.on("reloadInterview", () => {
            this.$store.dispatch("reloadInterview")
        })

        connection.on("closeInterview", () => {
            if (this.$store.getters.isReviewMode === true)
                return
            this.$store.dispatch("closeInterview")
            this.$store.dispatch("stop")
        })

        connection.on("shutDown", () => {
            this.$store.dispatch("shutDownInterview")
        })

        connection.on("finishInterview", () => {
            this.$store.dispatch("finishInterview")
        })

        connection.start()
            .then(() => this.$emit("connected"))
            .catch(err => document.write(err));
    },

    render() { return null }
}
