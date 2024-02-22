import { defineStore } from 'pinia';
import moment from 'moment';
import _ from 'lodash';
import { newGuid } from '../helpers/guid';
import { get, post, patch, del } from '../services/apiService';

export const useMacrosStore = defineStore('macros', {
    state: () => ({
        macros: [],        
    }),
    getters: {
        getMacros: state => state.macros,
    },
    actions: {
        async fetchMacros(questionnaireId) {
            // const data = await get(
            //     'questionnaire/' +
            //         questionnaireId 
            // );

            // this.macros = data;
        },

        clear() {
            this.macros = [];            
        },        

        

        async postMacro(macro) {
            
        },

        async deleteMacro(macroId) {
         },        
    }
});
