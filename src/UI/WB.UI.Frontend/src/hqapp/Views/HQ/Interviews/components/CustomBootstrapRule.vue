<template>
    <div class="rule-slot vqb-rule card">
        <div class="form-inline">
            <label class="mr-5">
                {{ rule.name }}
            </label>

            <!-- List of operands (optional) -->
            <select v-if="typeof rule.operands !== 'undefined'" v-model="query.operand" class="form-control mr-2 mb-5">
                <option v-for="operand in rule.operands" :key="operand">
                    {{ operand }}
                </option>
            </select>

            <!-- List of operators (e.g. =, !=, >, <) -->
            <select v-if="typeof rule.operators !== 'undefined' && rule.operators.length > 1" v-model="query.operator"
                class="form-control mr-2 mb-5">
                <option v-for="operator in rule.operators" :key="operator" :value="operator">
                    {{ operator }}
                </option>
            </select>

            <!-- Basic text input -->
            <input v-if="rule.type === 'text' && !unaryOperatorSelected" v-model="query.value" class="form-control mb-5"
                type="text" :placeholder="labels.textInputPlaceholder">

            <!-- Basic number input -->
            <input v-if="rule.type === 'numeric' && !unaryOperatorSelected" v-model="query.value"
                class="form-control mb-5" type="number">

            <!-- Datepicker -->
            <input v-if="rule.type === 'date' && !unaryOperatorSelected" v-model="query.value" class="form-control mb-5"
                type="date">

            <!-- Custom component input -->
            <div v-if="isCustomComponent" class="vqb-custom-component-wrap">
                <component :is="rule.component" :value="query.value" @input="updateQuery" />
            </div>

            <!-- Checkbox input -->
            <template v-if="rule.type === 'checkbox' && !unaryOperatorSelected">
                <div v-for="choice in rule.choices" :key="choice.value" class="form-check form-check-inline">
                    <input :id="'depth' + depth + '-' + rule.id + '-' + index + '-' + choice.value"
                        v-model="query.value" type="checkbox" :value="choice.value" class="form-check-input">
                    <label class="form-check-label"
                        :for="'depth' + depth + '-' + rule.id + '-' + index + '-' + choice.value">
                        {{ choice.label }}
                    </label>
                </div>
            </template>

            <!-- Radio input -->
            <template v-if="rule.type === 'radio' && !unaryOperatorSelected">
                <div v-for="choice in rule.choices" :key="choice.value" class="form-check form-check-inline">
                    <input :id="'depth' + depth + '-' + rule.id + '-' + index + '-' + choice.value"
                        v-model="query.value" :name="'depth' + depth + '-' + rule.id + '-' + index" type="radio"
                        :value="choice.value" class="form-check-input">
                    <label class="form-check-label"
                        :for="'depth' + depth + '-' + rule.id + '-' + index + '-' + choice.value">
                        {{ choice.label }}
                    </label>
                </div>
            </template>

            <!-- Select without groups -->
            <select v-if="rule.type === 'select' && !hasOptionGroups && !unaryOperatorSelected" v-model="query.value"
                class="form-control mb-5" :multiple="rule.type === 'multi-select'">
                <option v-for="option in rule.choices" :key="option.value" :value="option.value">
                    {{ option.label }}
                </option>
            </select>

            <!-- Select with groups -->
            <select v-if="rule.type === 'select' && hasOptionGroups && !unaryOperatorSelected" v-model="query.value"
                class="form-control mb-5" :multiple="rule.type === 'multi-select'">
                <optgroup v-for="(option, option_key) in rule.selectOptions" :key="option_key" :label="option_key">
                    <option v-for="sub_option in option" :key="sub_option.value" :value="sub_option.value">
                        {{ sub_option.label }}
                    </option>
                </optgroup>
            </select>
        </div>
    </div>
</template>

<script>

export default {

    props: ["ruleCtrl", "rule", "labels"],

    data() {
        return {
            query: {
                value: null,
                operand: null,
                operator: null,
            },
            hasOptionGroups: false,
        }
    },
    watch: {
        query: {
            handler(newVal, oldVal) {
                this.ruleCtrl.updateRuleData(newVal);
            },
            deep: true
        },
        /*"rule": {
            handler(newVal, oldVal) {
                if (!this.query.operator && this.rule.operators && this.rule.operators.length > 0) {
                    this.selectedRule = this.rule.operators[0]
                }
            },
            deep: false
        },*/
    },
    mounted() {
        const ruleData = this.ruleCtrl.ruleData
        if (ruleData) {
            this.query = ruleData
        }
        else if (!this.query.operator && this.rule.operators && this.rule.operators.length > 0) {
            this.query.operator = this.rule.operators[0]
        }
    },
    computed: {
        unaryOperatorSelected() {
            return this.rule.unaryOperators ? this.rule.unaryOperators.indexOf(this.query.operator) > -1 : false
        },
    },
}
</script>
