import * as signalR from "@microsoft/signalr";
import config from "~/shared/config"
import axios from 'axios'
import Vue from 'vue'

export default {
    name: 'wb-communicator',

    beforeMount() {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/interview?interviewId=" +  this.$route.params.interviewId)
            .build();

        const api = {
            get: this.apiGet,

            changeSection(from, to) {
                connection.send("changeSection", from, to)
            }
        };

        Object.defineProperty(Vue, "$api", {
            get: function () { return api; }
        })

        connection.start()
            .then(() => this.$emit("connected"))
            .catch(err => document.write(err));
    },

    render() { return null }
}
