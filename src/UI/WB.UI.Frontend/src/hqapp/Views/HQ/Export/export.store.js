import Vue from 'vue'
import { find, findIndex } from 'lodash'
import moment from 'moment'
import { DateFormats } from '~/shared/helpers'

function formatDate(data) {
    return moment.utc(data).local().format(DateFormats.dateTimeInList)
}

export default {
    state: {
        jobs: [],
        exportServiceIsUnavailable: true,
        _exportStatusUpdateInProgres: false,
    },

    actions: {
        async getExportStatus({ state, dispatch, commit }) {

            if (state._exportStatusUpdateInProgres) return
            commit('SET_UPDATE_IN_PROGRESS', true)

            const api = Vue.$config.model.api

            dispatch('showProgress')

            const jobsToUpdate = []

            try {
                const response = await Vue.$http.get(api.statusUrl)

                if (response.data == null) {
                    commit('SET_SERVICE_STATE', false)
                    return
                }

                response.data.forEach(status => {
                    var existing = find(state.jobs, { id: status.id })

                    if (existing == null) {
                        commit('ADD_JOB', { id: status.id })
                        jobsToUpdate.push(status.id)
                    }
                })

                state.jobs.forEach(job => {
                    if (!find(response.data, { id: job.id })) {
                        commit('REMOVE_JOB', { id: job.id })
                    }

                    if (job.isRunning == true) {
                        jobsToUpdate.push(job.id)
                    }
                })

                commit('SET_SERVICE_STATE', true)

                if (jobsToUpdate.length > 0)
                    dispatch('getJobsUpdate', jobsToUpdate)

            } catch (error) {
                Vue.config.errorHandler(error)
            } finally {
                dispatch('hideProgress')

                await new Promise(r => setTimeout(r, jobsToUpdate.length > 0 ? 1000 : 2000))
                commit('SET_UPDATE_IN_PROGRESS', false)
                dispatch('getExportStatus')
            }
        },

        async getJobsUpdate({ commit }, ids) {
            const api = Vue.$config.model.api

            const response = await Vue.$http.post(api.exportStatusUrl, ids)

            if (response.data == null) {
                return
            }

            response.data.forEach(job => {
                commit('UPDATE_JOB', job)
            })
        },
    },

    mutations: {
        SET_SERVICE_STATE(state, value, sdfad) {
            state.exportServiceIsUnavailable = !value
        },

        ADD_JOB(state, { id }) {
            const job = { id, isInitializing: true }

            // making sure all jobs ordered by ID descending
            const idx = findIndex(state.jobs, j => j.id < id)

            if (idx < 0)
                state.jobs.push(job)
            else
                state.jobs.splice(idx, 0, job)
        },

        UPDATE_JOB(state, job) {
            const index = findIndex(state.jobs, { id: job.id })

            job.initialized = true
            job.isInitializing = false
            job.beginDate = formatDate(job.beginDate)
            job.lastUpdateDate = formatDate(job.lastUpdateDate)
            job.interviewStatus = job.interviewStatus || 'AllStatuses'
            job.dataFileLastUpdateDate = formatDate(job.dataFileLastUpdateDate)
            job.fileDestination = job.dataDestination
            job.error = (job.error || {}).message
            job.timeEstimation = moment.duration(job.timeEstimation).humanize(true)
            Vue.set(state.jobs, index, job)
        },

        REMOVE_JOB(state, { id }) {
            const index = findIndex(state.jobs, { id })
            Vue.delete(state.jobs, index)
        },

        SET_UPDATE_IN_PROGRESS(state, value) {
            state._exportStatusUpdateInProgres = value
        },
    },
}
