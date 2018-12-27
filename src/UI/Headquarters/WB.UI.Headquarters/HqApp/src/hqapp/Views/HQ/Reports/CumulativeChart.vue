<script>
import { Line } from "vue-chartjs";
import Vue from "vue";

export default {
    extends: Line,
    props: {
        options: { required: true },
        chartData: { required: true }
    },

    mounted() {
        this.renderChart(this.chartData, this.options);
    },

    watch: {
        chartData() {
            this.renderChart(this.chartData, this.options);
        }
    }
};
</script>
