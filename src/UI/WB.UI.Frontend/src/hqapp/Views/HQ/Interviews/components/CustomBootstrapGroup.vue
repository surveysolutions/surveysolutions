<template>
    <div class="vqb-group">
        <div class="vqb-group-body card-body">
            <div class="rule-actions form-inline">
                <div class="form-group">

                    <select v-model="selectedRule"
                        class="form-control mr-2 mb-5 query-builder-group-slot__rule-selection">
                        <option v-for="rule in groupCtrl.rules" :key="rule.identifier" :value="rule.identifier"
                            v-text="rule.name" />
                    </select>

                    <button type="button" class="btn btn-secondary mr-2 mb-5" @click="groupCtrl.addRule(selectedRule)">
                        {{ labels.addRule }}
                    </button>

                    <button v-if="!groupCtrl.maxDepthExeeded" type="button" class="btn btn-secondary mb-5"
                        @click="groupCtrl.newGroup">
                        {{ labels.addGroup }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>

export default {
    name: 'QueryBuilderGroup',

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
            selectedRule: null,
        };
    },
    mounted() {
        this.setDefaultOptionIfNeed()
    },
    watch: {
        "groupCtrl": {
            handler(newVal, oldVal) {
                this.setDefaultOptionIfNeed()
            },
            deep: false
        },
    },
    methods: {
        setDefaultOptionIfNeed() {
            if (!this.selectedRule && this.groupCtrl.rules && this.groupCtrl.rules.length > 0) {
                this.selectedRule = this.groupCtrl.rules[0].identifier
            }
        }
    }

}
</script>

<style>
.query-builder-group__group-children .query-builder-group .rule-actions {
    margin-bottom: 20px;
}

.query-builder-group__group-children .vqb-rule {
    margin-top: 15px;
    margin-bottom: 15px;
    background-color: #f5f5f5;
    border-color: #ddd;
    padding: 15px;
}

.query-builder-group__group-children .query-builder-child .query-builder-child__delete-child {
    opacity: 1;
    color: rgb(150, 150, 150);
}

@media (min-width: 768px) {
    .query-builder-group__group-children .vqb-rule.form-inline .form-group {
        display: block;
    }
}
</style>