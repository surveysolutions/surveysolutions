import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

const dataSetInfo = {
    100: { label: Vue.$t('Strings.InterviewStatus_Completed'), backgroundColor: "#86B828" },
    65: { label: Vue.$t('Strings.InterviewStatus_RejectedBySupervisor'), backgroundColor: "#F08531" },
    120: { label: Vue.$t('Strings.InterviewStatus_ApprovedBySupervisor'), backgroundColor: "#13A388" },
    125: { label: Vue.$t('Strings.InterviewStatus_RejectedByHeadquarters'), backgroundColor: "#E06B5C" },
    130: { label: Vue.$t('Strings.InterviewStatus_ApprovedByHeadquarters'), backgroundColor: "#00647F" }
};

export default {
    state: {
        chartData: null,
        hasData: false
    },

    actions: {
        async queryChartData({ commit, dispatch }, queryString) {
            dispatch('showProgress')
            const response = await Vue.$hq.Report.Chart(queryString);
            dispatch('hideProgress')

            const dataSets = response.data.DataSets;

            const datasets = [];

            _.forEach(dataSets, set => {
                const info = dataSetInfo[set.Status];

                datasets.push(
                    Object.assign(info, {
                        data: set.Data,
                        cubicInterpolationMode: 'monotone'
                    })
                );
            });

            commit("SET_CHART_DATA", { datasets });
        }
    },

    mutations: {
        SET_CHART_DATA(state, { datasets }) {
            state.chartData = { datasets };
            state.hasData = datasets.length > 0;
        }
    },

    getters: {}
};
