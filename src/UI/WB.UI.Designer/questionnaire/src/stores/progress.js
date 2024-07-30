import { defineStore } from 'pinia';

export const useProgressStore = defineStore('progress', {
    state: () => ({
        runningTasks: 0
    }),
    getters: {
        getIsRunning: state => state.runningTasks > 0
    },
    actions: {
        start() {
            this.runningTasks ++;
        },

        stop() {
            this.runningTasks --;
        },

        reset(){
            this.runningTasks = 0;
        }
    }
});
