import Vue from "vue";

const dataSetInfo = [
    {status: 100, label: Vue.$t('Strings.InterviewStatus_Completed'), backgroundColor: "#86B828" },
    {status: 65,  label: Vue.$t('Strings.InterviewStatus_RejectedBySupervisor'), backgroundColor: "#F08531" },
    {status: 120, label: Vue.$t('Strings.InterviewStatus_ApprovedBySupervisor'), backgroundColor: "#13A388" },
    {status: 125, label: Vue.$t('Strings.InterviewStatus_RejectedByHeadquarters'), backgroundColor: "#E06B5C" },
    {status: 130, label: Vue.$t('Strings.InterviewStatus_ApprovedByHeadquarters'), backgroundColor: "#00647F" }
]

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

            const datasets = [];

            _.forEach(response.data.DataSets, set => {
                const infoIndex = _.findIndex(dataSetInfo, {status: set.Status})
                const info = dataSetInfo[infoIndex]

                datasets.push(
                    Object.assign(info, {
                        data: set.Data,
                        index: infoIndex,
                        cubicInterpolationMode: 'monotone'
                    })
                );
            });

            commit("SET_CHART_DATA", { datasets: _.sortBy(datasets, 'index') });
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
