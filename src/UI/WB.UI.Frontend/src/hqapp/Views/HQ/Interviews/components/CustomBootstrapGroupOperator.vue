<template>
    <!-- eslint-disable vue/no-v-html -->
    <div class="vqb-group card" :class="'depth-' + groupCtrl.depth">
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
//import QueryBuilderGroup from 'vue-query-builder/dist/group/QueryBuilderGroup.umd.js'
import QueryBuilderRule from './CustomBootstrapRule.vue'

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
    components: {
        // eslint-disable-next-line vue/no-unused-components
        'QueryBuilderRule': QueryBuilderRule,
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

<style scoped>
.query-builder-group .vqb-group .rule-actions {
    margin-bottom: 20px;
}

.query-builder-group .vqb-rule {
    margin-top: 15px;
    margin-bottom: 15px;
    background-color: #f5f5f5;
    border-color: #ddd;
    padding: 15px;
}

.query-builder-group .vqb-group.depth-1 .vqb-rule,
.query-builder-group .vqb-group.depth-2 {
    border-left: 2px solid #8bc34a;
}

.query-builder-group .vqb-group.depth-2 .vqb-rule,
.query-builder-group .vqb-group.depth-3 {
    border-left: 2px solid #00bcd4;
}

.query-builder-group .vqb-group.depth-3 .vqb-rule,
.query-builder-group .vqb-group.depth-4 {
    border-left: 2px solid #ff5722;
}

.query-builder-group .close {
    opacity: 1;
    color: rgb(150, 150, 150);
}

@media (min-width: 768px) {
    .query-builder-group .vqb-rule.form-inline .form-group {
        display: block;
    }
}
</style>