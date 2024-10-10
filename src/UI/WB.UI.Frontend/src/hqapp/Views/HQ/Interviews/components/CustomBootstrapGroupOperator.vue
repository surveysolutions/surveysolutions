<template>
    <!-- eslint-disable vue/no-v-html -->
    <div class="vqb-group">
        <div class="vqb-group-heading card-header">
            <div class="match-type-container form-inline">
                <label class="mr-2" for="vqb-match-type">
                    {{ labels.matchType }}
                </label>

                <select :value="groupCtrl.currentOperator" class="form-control"
                    @input="groupCtrl.updateCurrentOperator($event.target.value)">
                    <option v-for="operator in groupCtrl.operators" :key="operator.identifier"
                        :value="operator.identifier" v-text="operator.name" />
                </select>
            </div>
        </div>
    </div>
</template>

<script>

export default {
    name: 'QueryBuilderGroupOperator',

    props: {
        groupCtrl: {
            required: true,
        },
        labels: {
            required: true,
        }
    },
    data() {
        return {
            selectedRule: "",
        };
    },
    computed: {

    }

}
</script>
