<template>
    <!-- eslint-disable vue/no-v-html -->
    <div class="vqb-group" :class="'depth-' + groupCtrl.depth">
        <div class="vqb-group-heading card-header">
            <div class="match-type-container form-inline">
                <label class="mr-2" for="vqb-match-type">
                    {{ labels.matchType }}
                </label>

                <!--select id="vqb-match-type" v-model="query.logicalOperator" class="form-control">
                    <option v-for="label in labels.matchTypes" :key="label.id" :value="label.id">
                        {{ label.label }}
                    </option>
                </select-->

                <select :value="groupCtrl.currentOperator" class="form-control"
                    @input="groupCtrl.updateCurrentOperator($event.target.value)">
                    <option v-for="operator in groupCtrl.operators" :key="operator.identifier"
                        :value="operator.identifier" v-text="operator.name" />
                </select>

                <button v-if="groupCtrl.depth > 1" type="button" class="close ml-auto" @click="remove"
                    v-html="labels.removeGroup">
                </button>
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
