<template>
    <v-dialog v-model="shownModel" persistent max-width="600px">
        <v-card>
            <v-card-title>
                <span class="headline">{{ title }}</span>
            </v-card-title>
            <v-card-text>
                <v-container>
                    <v-form ref="form" v-model="valid" validate-on="input" fast-fail>
                        <v-row>
                            <v-col cols="12">
                                <v-text-field
                                    v-model="itemTitle"
                                    :label="
                                        $t('QuestionnaireEditor.GroupTitle') +
                                            '*'
                                    "
                                    :rules="[required]"                                    
                                ></v-text-field>
                            </v-col>
                            <v-col cols="12" :sm="showParentValue ? 6 : 12">
                                <v-text-field
                                    v-model="itemValue"
                                    :label="
                                        $t(
                                            'QuestionnaireEditor.OptionsUploadValue'
                                        ) + '*'
                                    "
                                    :rules="[required, maxValue]"
                                    type="number"                                    
                                ></v-text-field>
                            </v-col>
                            <v-col v-if="showParentValue" cols="12" sm="6">
                                <v-autocomplete
                                    v-if="parentCategories && parentCategories.length > 0"
                                    v-model="itemParentValue"
                                    autofocus
                                    eager
                                    single-line
                                    :items="parentCategories"
                                    :item-title="getTitle"
                                    item-value="value"                                    
                                />
                                <v-text-field
                                    v-else
                                    v-model="itemParentValue"
                                    :label="
                                        $t(
                                            'QuestionnaireEditor.OptionsUploadParent'
                                        ) + '*'
                                    "
                                    :rules="[required, maxValue]"
                                    type="number"                                    
                                ></v-text-field>
                            </v-col>
                            <v-col cols="12">
                                <v-text-field
                                    v-model="itemAttachmentName"
                                    :label="
                                        $t('QuestionnaireEditor.AttachmentName')
                                    "
                                ></v-text-field>
                            </v-col>
                        </v-row>
                    </v-form>
                </v-container>
            </v-card-text>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn 
                    class="ma-2"
                    color="success" 
                    :disabled="!valid" 
                    @click="save">{{
                        $t('QuestionnaireEditor.Save')
                }}</v-btn>
                <v-btn color="primary darken-1" text @click="cancel">{{
                    $t('QuestionnaireEditor.Cancel')
                }}</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<style lang="scss">
body {
    overflow: scroll;
}
</style>

<script>
export default {
    props: {
        title: { type: String, required: true },
        item: {
            /* { title: '', value: '', parentValue: '' } */
            type: Object,
            required: true
        },
        showParentValue: { type: Boolean, required: true },
        shown: { type: Boolean, required: true },
        parentCategories: { type: Array, required: false, default: () => [] }
    },

    data() {
        return {
            itemTitle: this.item.title,
            itemValue: this.item.value,
            itemParentValue: this.item.parentValue,
            itemAttachmentName: this.item.attachmentName,            

            required: value => {
                if(value) return true 
                return this.$t('QuestionnaireEditor.RequiredField')
            },
            maxValue: v =>
                (/^[-+]?\d+$/.test(v) &&
                    Math.abs(parseInt(v)) <= 2147483647 &&
                    Math.abs(parseInt(v)) >= -2147483648) ||
                this.$t('QuestionnaireEditor.ValidationIntValue'),            
            isValid: true
        };
    },
    computed: {
        shownModel: {
            get() {
                return this.shown;
            },
            set(value) {
                this.$emit('update:shown', value);
            }
        },
        valid: {
            get(){return this.isValid},
            set(value) {this.isValid = value }
        }
    },
    methods: {
        async save() {
            await this.$refs.form.validate();
            if (this.valid) {
                this.$emit('saveCategory', {
                    title: this.itemTitle,
                    value: this.itemValue,
                    parentValue: this.itemParentValue,
                    attachmentName: this.itemAttachmentName
                });
            }
        },
        cancel() {
            this.$emit('cancel');
        },
        getTitle(v){

            if(v.value !== undefined)
                return v.value + ' - ' + v.title;
            
            if(this.parentCategories && this.parentCategories.length > 0)
            {
                let item = this.parentCategories.find(x => x.value == v);
                if(item !== undefined)
                    return item.value + ' - ' + item.title
            }    

            return v;
        }
    },
    async mounted() {
        if(this.itemTitle || this.itemValue)
            await this.$refs.form.validate()
    }    
};
</script>
